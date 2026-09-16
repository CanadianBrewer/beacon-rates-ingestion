using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BeaconDataIngestion.Models.DataModels.DDW.Rates;
using DueDiligenceWorks.Beacon.RateIngestion.Models.Application;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public class BeaconRatesApiService : IBeaconRatesApiService
{
    private readonly BeaconRatesApiConfig _apiConfig;
    private readonly ParallelOptions _parallelOptions = new() { MaxDegreeOfParallelism = 25 };
    private readonly ILogger<BeaconRatesApiService> _logger;
    private readonly IFirestoreService _firestoreService;
    private readonly HttpClient _httpClient;
    private List<MarketIndex> _marketIndices = [];
    private List<CreditingMethod> _creditingMethods = [];
    
    public BeaconRatesApiService(IOptions<BeaconRatesApiConfig> apiConfig,
        ILogger<BeaconRatesApiService> logger,
        IFirestoreService firestoreService,
        HttpClient httpClient)
    {
        _logger = logger;
        _firestoreService = firestoreService;
        _httpClient = httpClient;
        _apiConfig = apiConfig.Value;
    }
    private static JsonSerializerOptions _jsonSerializerOptions = null!;

    private enum RateType
    {
        Fixed = 0,
        Indexed = 1, 
        Rila = 2
    }

    
    public async Task GetAllRates()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddAliasesModifier }
            }
        };
        
        _logger.LogInformation("Rate processing started");
        await GetFixedRates();
        await GetIndexedRates();
        await GetRilaRates();
        // await _firestoreService.UpdateRatesLastUpdatedOnAsync();
        _logger.LogInformation("Rate processing completed");
    }

    public async Task GetFixedRates()
    {
        await _firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-fixed-rates");
        List<BeaconGenericRate> beaconAnnuityRates = await GetRatesFromBeaconAsyncV2(ToBeaconRateCode(RateType.Fixed));
        List<ProductRate> fixedAnnuityRates = PopulateFixedRates(beaconAnnuityRates);

        fixedAnnuityRates = RemoveFutureDatedRates(fixedAnnuityRates, RateType.Fixed);
        List<string> productIds = [.. fixedAnnuityRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await _firestoreService.GetInactiveProductIdsAsync(ToCategoryName(RateType.Fixed));

        _logger.LogInformation("Processing {ItemCount} fixed rates", productIds.Count);
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                return;
            }

            _logger.LogDebug("Processing {Index}/{ItemCount} fixed rates", Interlocked.Increment(ref counter), productIds.Count);
            CalculateMaximumContributionsForFixedRates(fixedAnnuityRates.Where(z => z.ProductId == productId));
            await _firestoreService.DeleteRatesForProductAsync(productId, ct);
            await _firestoreService.PersistRatesAsync([.. fixedAnnuityRates.Where(z => z.ProductId == productId)], ct);
            await _firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the fixed rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated we are no longer receiving rates for a product id
        // we want to remove old product-rate data
        List<string> allFixedRateIds = await _firestoreService.GetAllAnnuityRateIdsAsync("fixed");
        await Parallel.ForEachAsync(allFixedRateIds, _parallelOptions, async (id, ct) =>
        {
            if (productIds.Contains(id))
            {
                return;
            }

            _logger.LogDebug("Deleting fixed rate {Urn} as it is no longer provided by Beacon", id);
            await _firestoreService.DeleteRatesForProductAsync(id, ct);
        });

        _logger.LogInformation("Finished processing {ItemCount} fixed rates", productIds.Count);
    }

    public async Task GetIndexedRates()
    {
        await _firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-indexed-rates");

        List<BeaconGenericRate> beaconAnnuityRates = await GetRatesFromBeaconAsyncV2(ToBeaconRateCode(RateType.Indexed));
        List<ProductRate> indexedAnnuityRates = await PopulateIndexedRates(beaconAnnuityRates);   
        
        indexedAnnuityRates = RemoveFutureDatedRates(indexedAnnuityRates, RateType.Indexed);
        List<string> productIds = [.. indexedAnnuityRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await _firestoreService.GetInactiveProductIdsAsync(ToCategoryName(RateType.Indexed));

        _logger.LogInformation("Processing {ItemCount} indexed rates", productIds.Count);
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                return;
            }

            _logger.LogDebug("Processing {Index}/{ItemCount} indexed rates", Interlocked.Increment(ref counter), productIds.Count);
            CalculateMaximumContributionsForIndexedRates(indexedAnnuityRates.Where(z => z.ProductId == productId));
            await _firestoreService.DeleteRatesForProductAsync(productId, ct);
            await _firestoreService.PersistRatesAsync([.. indexedAnnuityRates.Where(z => z.ProductId == productId)], ct);
            await _firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the indexed rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated and the product id has been removed from the collection
        List<string> allIndexedRateIds = await _firestoreService.GetAllAnnuityRateIdsAsync("indexed");
        await Parallel.ForEachAsync(allIndexedRateIds, _parallelOptions, async (id, ct) =>
        {
            if (productIds.Contains(id))
            {
                return;
            }

            _logger.LogDebug("Deleting indexed rate {Urn} for product as it is no longer provided by Beacon", id);
            await _firestoreService.DeleteRatesForProductAsync(id, ct);
        });

        _logger.LogInformation("Finished processing {ItemCount} indexed rates", productIds.Count);
    }
    
    public async Task GetRilaRates()
    {
        await _firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-rila-rates");

        List<BeaconGenericRate> beaconAnnuityRates = await GetRatesFromBeaconAsyncV2(ToBeaconRateCode(RateType.Rila));
        List<ProductRate> rilaAnnuityRates = await PopulateRilaRates(beaconAnnuityRates);
        
        rilaAnnuityRates = RemoveFutureDatedRates(rilaAnnuityRates, RateType.Rila);
        List<string> productIds = [.. rilaAnnuityRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await _firestoreService.GetInactiveProductIdsAsync(ToCategoryName(RateType.Rila));

        _logger.LogInformation("Processing {ItemCount} rila rates", productIds.Count());
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                _logger.LogInformation("Skipping rila rate for product {ProductId} as it is inactive", productId);
                return;
            }

            _logger.LogDebug("Processing {Index}/{ItemCount} rila rates", Interlocked.Increment(ref counter), productIds.Count());
            CalculateMaximumContributionsForRilaRates(rilaAnnuityRates.Where(z => z.ProductId == productId));
            await _firestoreService.DeleteRatesForProductAsync(productId, ct);
            await _firestoreService.PersistRatesAsync([.. rilaAnnuityRates.Where(z => z.ProductId == productId)], ct);
            await _firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the rila rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated and the product id has been removed from the collection
        List<string> allRilaRates = await _firestoreService.GetAllAnnuityRateIdsAsync("rila");
        await Parallel.ForEachAsync(allRilaRates, _parallelOptions, async (id, ct) =>
        {
            if (productIds.Contains(id))
            {
                return;
            }

            _logger.LogInformation("Deleting rila rate {Urn} for product as it is no longer provided by Beacon", id);
            await _firestoreService.DeleteRatesForProductAsync(id, ct);
        });
    }

    private List<ProductRate> PopulateFixedRates(List<BeaconGenericRate> genericRates)
    {
        List<ProductRate> retVal = [];
        foreach (BeaconGenericRate genericRate in genericRates)
        {
            // guards
            if (string.IsNullOrEmpty(genericRate.BeginDate))
            {
                _logger.LogWarning("Skipping fixed rate for product {ProductId} with urn {Urn} as it has no rate begin date", genericRate.ProductId, genericRate.Urn);
                continue;
            }

            retVal.Add(new ProductRate()
            {
                Id = genericRate.Urn,
                ProductId = $"fa_{genericRate.ProductId}",
                CategoryId = "fixed",
                StartDate = DateTime.ParseExact(
                    genericRate.BeginDate,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                EndDate = string.IsNullOrWhiteSpace(genericRate.EndDate)
                    ? null
                    : DateTime.ParseExact(
                        genericRate.EndDate,
                        "yyyy-MM-dd'T'HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                Premium = new PremiumRange { Minimum = genericRate.MinimumContribution is null ? null : (int?)genericRate.MinimumContribution },
                States = string.IsNullOrWhiteSpace(genericRate.OverallStateAvailability) ? [] : [.. genericRate.OverallStateAvailability.Split(',')],
                Term = new RateTerm
                {
                    Value = genericRate.Intrateter,
                    StartDate = string.IsNullOrWhiteSpace(genericRate.TermBeginDate)
                        ? null
                        : DateTime.ParseExact(
                            genericRate.TermBeginDate,
                            "yyyy-MM-dd'T'HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    EndDate = string.IsNullOrWhiteSpace(genericRate.TermEndDate)
                        ? null
                        : DateTime.ParseExact(
                            genericRate.TermEndDate,
                            "yyyy-MM-dd'T'HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                },
                Rate = new RateValue()
                {
                    Value = genericRate.InitialRate,
                    Minimum = genericRate.MinimumEffectiveRate,
                    Guaranteed = genericRate.MinimumGuaranteedRate
                },
                Bonus = new BonusRate()
                {
                    Value = genericRate.BonusPercent,
                    Term = genericRate.BonusLen,
                    Type = genericRate.BonusType
                },
                Terms = new Terms()
                {
                    ProductType = genericRate.ProductType,
                    InterestType = genericRate.InterestType,
                    Mva = genericRate.Mva,
                    Rop = genericRate.Rop,
                    Qualifier = genericRate.Qualifier,
                    BailoutRate = genericRate.BailoutRate,
                    SurrenderExpirationDate = string.IsNullOrWhiteSpace(genericRate.SurrenderExpirationDate)
                        ? null
                        : DateTime.ParseExact(
                            genericRate.SurrenderExpirationDate,
                            "yyyy-MM-dd'T'HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    SurrenderId = genericRate.SurrenderId,
                    SurrenderIncreaseDate = string.IsNullOrWhiteSpace(genericRate.SurrenderIncreaseDate)
                        ? null
                        : DateTime.ParseExact(
                            genericRate.SurrenderIncreaseDate,
                            "yyyy-MM-dd'T'HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    SurrenderYear = genericRate.SurrenderYear
                },
                VarId = genericRate.VarId
            });
        }

        return retVal;
    }
    
    private async Task<List<ProductRate>> PopulateIndexedRates(List<BeaconGenericRate> genericRates)
    {
        List<ProductRate> retVal = [];
        
        foreach (BeaconGenericRate genericRate in genericRates)
        {
            // guards1
            if (string.IsNullOrEmpty(genericRate.BeginDate))
            {
                _logger.LogWarning("Skipping fixed rate for product {ProductId} with urn {Urn} as it has no rate begin date", genericRate.ProductId, genericRate.Urn);
                continue;
            }

            retVal.Add(new ProductRate()
            {
                Id = genericRate.Urn,
                ProductId = $"ia_{genericRate.ProductId}",
                CategoryId = "indexed",
                StartDate = DateTime.ParseExact(
                    genericRate.BeginDate,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                EndDate = string.IsNullOrWhiteSpace(genericRate.EndDate)
                    ? null
                    : DateTime.ParseExact(
                        genericRate.EndDate,
                        "yyyy-MM-dd'T'HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                Name = genericRate.MarketingName,
                Description = genericRate.Description,
                Premium = new PremiumRange { Minimum = genericRate.Minimum },
                States = string.IsNullOrWhiteSpace(genericRate.OverallStateAvailability) ? [] : [.. genericRate.OverallStateAvailability.Split(',')],
                CreditingFrequency = genericRate.CreditingFrequency,
                StrategyName = genericRate.StrategyName,
                StrategyId = string.Empty, // don't know where to get this from Beacon or how to map it from StrategyName
                MarketIndexId = await GetOrCreateMarketIndexId(genericRate.Index, genericRate.TickerSymbol),  
                CreditingMethodId = await GetOrCreateCreditingMethodId(genericRate.StrategyName),
                Rate = new RateValue()
                {
                    Value = genericRate.FixedRate,
                    Minimum = genericRate.MinimumFixedRate,
                    Guaranteed = genericRate.MinimumGuaranteedRate1
                },
                Cap = new RateRange
                {
                    Value = genericRate.CapRate,
                    Minimum = genericRate.CapMinimum,
                    Bailout = genericRate.CapBail
                },
                Spread = new RateRange
                {
                    Value = genericRate.SpreadRate,
                    Maximum = genericRate.SpreadMaximum
                },
                Participation = new RateRange
                {
                    Value = genericRate.ParticipationRate,
                    Minimum = genericRate.ParticipationRateMinimum,
                    Bailout = genericRate.ParticipationRateBailout
                },
                Trigger = new TriggerRate
                {
                    Value = genericRate.PerformanceTrigger,
                    Rate = genericRate.PerformanceTriggerRate,
                    Bailout = genericRate.PerformanceTriggerMinimumCredit
                },
                Terms = new Terms()
                {
                    TickerSymbol = genericRate.TickerSymbol,
                    Rop = genericRate.Rop,
                    RebalanceFixedAllocation = genericRate.RebalanceFixedAllocation,
                    RebalanceFixedRate = genericRate.RebalanceFixedRate,
                    RebalanceIndexAllocation = genericRate.RebalanceIndexAllocation,
                    MinimumGuaranteedRate2 = genericRate.MinimumGuaranteedRate2,
                    MinimumInitialGuaranteedPercent1 = genericRate.MinimumInitialGuaranteePercent1,
                    MinimumInitialGuaranteedPercent2 = genericRate.MinimumInitialGuaranteePercent2,
                    GmirCode1 = genericRate.GmirCode1,
                    GmirCode2 = genericRate.GmirCode2,
                    GmirStateAvailability1 = genericRate.GmirStateAvailability1,
                    GmirStateAvailability2 = genericRate.GmirStateAvailability2
                },
                VarId = genericRate.VarId
            });
        }

        return retVal;
    }
    
    private async Task<List<ProductRate>> PopulateRilaRates(List<BeaconGenericRate> genericRates)
    {
        List<ProductRate> retVal = [];
        foreach (BeaconGenericRate genericRate in genericRates)
        {
            // guards
            if (string.IsNullOrEmpty(genericRate.EffectiveDate))
            {
                _logger.LogWarning("Skipping fixed rate for product {ProductId} with urn {Urn} as it has no rate begin date", genericRate.ProductId, genericRate.Urn);
                continue;
            }

            retVal.Add(new ProductRate()
            {
                Id = genericRate.Urn,
                ProductId = $"ia_{genericRate.ProductId}",
                CategoryId = "rila",
                StartDate = DateTime.ParseExact(
                    genericRate.EffectiveDate,
                    "MM/dd/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                Name = genericRate.MarketingName,
                Description = genericRate.Description,
                Premium = new PremiumRange { Minimum = genericRate.Minimum },
                States = string.IsNullOrWhiteSpace(genericRate.States) ? [] : [.. genericRate.States.Split(',')],
                CreditingFrequency = genericRate.CreditingFrequency,
                StrategyName = genericRate.StrategyName,
                StrategyId = string.Empty, // don't know where to get this from Beacon or how to map it from StrategyName
                MarketIndexId = await GetOrCreateMarketIndexId(genericRate.Index, genericRate.TickerSymbol),  
                CreditingMethodId = await GetOrCreateCreditingMethodId(genericRate.StrategyName),
                Rate = new RateValue()
                {
                    Value = genericRate.FixedRate,
                    Minimum = genericRate.MinimumFixedRate
                },
                Cap = new RateRange
                {
                    Value = genericRate.CapRate,
                    Minimum = genericRate.CapMinimum,
                    Bailout = genericRate.CapBail
                },
                Buffer = genericRate.Buffer,
                Floor = genericRate.Floor,
                Spread = new RateRange
                {
                    Value = genericRate.SpreadRate,
                    Maximum = genericRate.SpreadMaximum
                },
                Participation = new RateRange
                {
                    Value = genericRate.ParticipationRate,
                    Minimum = genericRate.ParticipationRateMinimum,
                    Bailout = genericRate.ParticipationRateBailout
                },
                Trigger = new TriggerRate
                {
                    Value = genericRate.PerformanceTrigger,
                    Rate = genericRate.PerformanceTriggerRate,
                    Bailout = genericRate.PerformanceTriggerMinimumCredit
                },
                Terms = new Terms()
                {
                    TickerSymbol = genericRate.TickerSymbol,
                    Rop = genericRate.Rop
                },
                VarId = genericRate.VarId
            });
        }

        return retVal;
    }

    // private async Task<List<T>> GetRatesFromBeaconAsync<T>(string rateType) where T : AnnuityBaseRate
    // {
    //     var url = $@"{_apiConfig.Url}/api/DDW_{rateType}/DDW_{rateType}_Rates";
    //     var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url)
    //     {
    //         Headers =
    //         {
    //             { HeaderNames.Accept, "application/json" }
    //         }
    //     };
    //
    //     httpRequestMessage.Headers.Add("ApiKey", _apiConfig.ApiKey);
    //
    //     var stopwatch = Stopwatch.StartNew();
    //     stopwatch.Start();
    //     HttpResponseMessage httpResponseMessage;
    //     if (_apiConfig.SslHackMode)
    //     {
    //         // Beacon cert management is not done well so we just override cert validation
    //         var handler = new HttpClientHandler();
    //         handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    //         HttpClient customHttpClient = new(handler);
    //         httpResponseMessage = await customHttpClient.SendAsync(httpRequestMessage);
    //     }
    //     else
    //     {
    //         httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
    //     }
    //
    //     stopwatch.Stop();
    //     logger.LogInformation("API call to {Url} took {ElapsedMilliseconds}ms", httpRequestMessage.RequestUri, stopwatch.ElapsedMilliseconds);
    //
    //     if (httpResponseMessage.IsSuccessStatusCode)
    //     {
    //         string json = await httpResponseMessage.Content.ReadAsStringAsync();
    //         try
    //         {
    //             var rates = JsonSerializer.Deserialize<List<T>>(json);
    //             return rates!;
    //         }
    //         catch (Exception ex)
    //         {
    //             logger.LogError(ex, "Deserialization failure: {RateType} | {ResponseMessage}",
    //                 rateType,
    //                 JsonSerializer.Serialize(httpResponseMessage));
    //             throw;
    //         }
    //     }
    //
    //     // log error
    //     logger.LogError("The call to the Beacon Api for rates processing returned HTTP {StatusCode} and body {ResponseMessage)}",
    //         httpResponseMessage.StatusCode,
    //         JsonSerializer.Serialize(httpResponseMessage));
    //     throw new BeaconException($"Beacon API call returned {httpResponseMessage.StatusCode}");
    // }

    private async Task<List<BeaconGenericRate>> GetRatesFromBeaconAsyncV2(string rateType)
    {
        var url = $@"{_apiConfig.Url}/api/DDW_{rateType}/DDW_{rateType}_Rates";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers =
            {
                { HeaderNames.Accept, "application/json" }
            }
        };

        httpRequestMessage.Headers.Add("ApiKey", _apiConfig.ApiKey);

        var stopwatch = Stopwatch.StartNew();
        stopwatch.Start();
        HttpResponseMessage httpResponseMessage;
        if (_apiConfig.SslHackMode)
        {
            // Beacon cert management is not done well so we just override cert validation
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            HttpClient customHttpClient = new(handler);
            httpResponseMessage = await customHttpClient.SendAsync(httpRequestMessage);
        }
        else
        {
            httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage);
        }

        stopwatch.Stop();
        _logger.LogInformation("API call to {Url} took {ElapsedMilliseconds}ms", httpRequestMessage.RequestUri, stopwatch.ElapsedMilliseconds);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                List<BeaconGenericRate> beaconRates = JsonSerializer.Deserialize<List<BeaconGenericRate>>(json, _jsonSerializerOptions) ?? [];
                return [.. beaconRates];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deserialization failure: {RateType} | {ResponseMessage}",
                    rateType,
                    JsonSerializer.Serialize(httpResponseMessage));
                throw;
            }
        }

        // log error
        _logger.LogError("The call to the Beacon Api for rates processing returned HTTP {StatusCode} and body {ResponseMessage)}",
            httpResponseMessage.StatusCode,
            JsonSerializer.Serialize(httpResponseMessage));
        throw new BeaconException($"Beacon API call returned {httpResponseMessage.StatusCode}");
    }


    private void CalculateMaximumContributionsForFixedRates(IEnumerable<ProductRate> productRates)
    {
        // if there is only 1 minimum contribution for the entire group of rates then maximum contribution - use 999,999,999
        // otherwise, group the rates by VarId, then SurrId
        // for each group, order by minimumContribution ascending
        // iterate over this group, setting (generally) item[x].maximumContribution = item[x+1].minumumContribtion - 1
        // need to handle scenarios where there are n items with the same minimum contribution
        // for final item, maximumContribution = 999,999,999
        // test products: 619 (21 rates, 3 minimum cons), 1246 (3 rates, 3 minimum cons), 1077 (4 rates, 1 minimum con)

        List<ProductRate> fixedRates = [.. productRates];

        var groupCounter = 1;
        string productId = fixedRates.First().ProductId;
        int minimumContributionCount = fixedRates.Select(z => z.Premium.Minimum).Distinct().Count();
        if (minimumContributionCount == 1)
        {
            foreach (ProductRate productRate in fixedRates)
            {
                productRate.DdwGroupId = $"{productId}_{groupCounter++}";
                productRate.Premium.Maximum = 999999999;
            }

            return;
        }

        Dictionary<long, long> minMaxContributionPairs = new();
        List<int> varIds =
        [
            .. fixedRates
                .Where(z => z.VarId.HasValue)
                .Select(z => z.VarId!.Value)
                .Distinct()
        ];

        foreach (int varId in varIds)
        {
            List<ProductRate> ratesGroupedByVarId = [.. fixedRates.Where(z => z.VarId == varId)];
            List<int> surrenderIds =
            [
                .. ratesGroupedByVarId
                    .Where(z => z.Terms.SurrenderId.HasValue)
                    .Select(z => z.Terms.SurrenderId!.Value)
                    .Distinct()
            ];

            foreach (int surrenderId in surrenderIds)
            {
                List<ProductRate> ratesGroupedBySurrenderId =
                [
                    .. ratesGroupedByVarId
                        .Where(z => z.Terms.SurrenderId == surrenderId)
                        .OrderBy(z => z.Premium.Minimum)
                ];

                for (var i = 0; i < ratesGroupedBySurrenderId.Count - 1; i++)
                {
                    if (ratesGroupedBySurrenderId[i].Premium.Minimum is null)
                    {
                        _logger.LogCritical("{MethodName} :: Fixed rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForFixedRates), productId);
                        continue;
                    }

                    long premiumMinimum = 0;
                    if (ratesGroupedBySurrenderId[i].Premium.Minimum.HasValue)
                    {
                        premiumMinimum = ratesGroupedBySurrenderId[i].Premium.Minimum!.Value;
                    }

                    if (minMaxContributionPairs.TryGetValue(premiumMinimum, out long pairMaximum))
                    {
                        ratesGroupedBySurrenderId[i].Premium.Minimum = premiumMinimum;
                        ratesGroupedBySurrenderId[i].Premium.Maximum = pairMaximum;
                    }
                    else
                    {
                        // we need to get the next higher contribution limit (or 999,999,999 if there isn't one)
                        ProductRate? nextHighestMinimum = ratesGroupedByVarId.OrderBy(z => z.Premium.Minimum).FirstOrDefault(z => z.Premium.Minimum > premiumMinimum);
                        long maxContribution = 999999999;
                        if (nextHighestMinimum?.Premium.Minimum is not null)
                        {
                            maxContribution = nextHighestMinimum.Premium.Minimum.Value - 1;
                        }

                        minMaxContributionPairs.Add(premiumMinimum, maxContribution);
                        ratesGroupedBySurrenderId[i].Premium.Maximum = maxContribution;
                    }
                }

                ratesGroupedBySurrenderId[^1].Premium.Maximum = 999999999;
                ratesGroupedBySurrenderId.ForEach(z => z.DdwGroupId = $"{productId}_{groupCounter}");
                groupCounter++;
            }
        }
    }

    private void CalculateMaximumContributionsForIndexedRates(IEnumerable<ProductRate> productRates)
    {
        // if there is only 1 minimum contribution for the entire group of rates then maximum contribution - use 999,999,999
        // otherwise, group the rates by MarketingName, then Index, then by OverallStateAbility
        // for each group, order by minimumContribution ascending
        // iterate over this group, setting (generally) item[x].maximumContribution = item[x+1].minumumContribtion - 1
        // need to handle scenarios where there are n items with the same minimum contribution
        // for final item, maximumContribution = 999,999,999
        // test products:
        //  987 (10 rates, 2 minimum cons, 3 names, 3 indices, 1 states)
        // 1262 (36 rates, 2 minimum cons, 6 names, 5 indices, 2 states)
        // 2880 (12 rates, 2 minimum cons, 3 names, 3 indices, 2 states)

        List<ProductRate> indexedRates = [.. productRates];

        var groupCounter = 1;
        string productId = indexedRates.First().ProductId;
        int minimumContributionCount = indexedRates.Select(z => z.Premium.Minimum).Distinct().Count();
        if (minimumContributionCount == 1)
        {
            foreach (ProductRate productRate in indexedRates)
            {
                productRate.DdwGroupId = $"{productId}_{groupCounter++}";
                productRate.Premium.Maximum = 999999999;
            }

            return;
        }

        Dictionary<long, long> minMaxContributionPairs = new();
        IEnumerable<string?> marketingNames = [.. indexedRates.Select(z => z.Name).Distinct()];
        foreach (string? marketingName in marketingNames)
        {
            List<ProductRate> ratesGroupedByMarketingName = [.. indexedRates.Where(z => z.Name == marketingName)];
            IEnumerable<string?> indexNames = [.. ratesGroupedByMarketingName.Select(z => z.MarketIndexId).Distinct()];
            foreach (string? indexName in indexNames)
            {
                List<ProductRate> ratesGroupedByState = [.. ratesGroupedByMarketingName.Where(z => z.MarketIndexId == indexName).OrderBy(z => z.StatesJoined)];
                IEnumerable<string?> stateGroups = [.. ratesGroupedByState.Select(z => z.StatesJoined).Distinct()];
                foreach (string? stateGroup in stateGroups)
                {
                    List<ProductRate> stateRateGroup = [.. ratesGroupedByState.Where(z => z.StatesJoined == stateGroup).OrderBy(z => z.Premium.Minimum)];
                    for (var i = 0; i < stateRateGroup.Count - 1; i++)
                    {
                        if (stateRateGroup[i].Premium.Minimum is null)
                        {
                            _logger.LogCritical("{MethodName} :: Indexed rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForIndexedRates), productId);
                            continue;
                        }

                        long minContribution = stateRateGroup[i].Premium.Minimum!.Value;
                        if (minMaxContributionPairs.TryGetValue(minContribution, out long pair))
                        {
                            stateRateGroup[i].Premium.Maximum = pair;
                        }
                        else
                        {
                            // we need to get the next higher contribution limit (or 999,999,999 if there isn't one)
                            ProductRate? nextHighestMinimum = stateRateGroup.OrderBy(z => z.Premium.Minimum).FirstOrDefault(z => z.Premium.Minimum > minContribution);
                            long maxContribution = 999999999;
                            if (nextHighestMinimum?.Premium.Minimum is not null)
                            {
                                maxContribution = nextHighestMinimum.Premium.Minimum!.Value - 1;
                            }

                            minMaxContributionPairs.Add(minContribution, maxContribution);
                            stateRateGroup[i].Premium.Maximum = maxContribution;
                        }
                    }

                    stateRateGroup[^1].Premium.Maximum = 999999999;
                    stateRateGroup.ForEach(z => z.DdwGroupId = $"{productId}_{groupCounter}");
                    groupCounter++;
                }
            }
        }
    }

    private void CalculateMaximumContributionsForRilaRates(IEnumerable<ProductRate> productRatess)
    {
        // if there is only 1 minimum contribution for the entire group of rates then maximum contribution - use 999,999,999
        // otherwise, group the rates by Buffer, then Index, then by MarketingName
        // for each group, order by minimumContribution ascending
        // iterate over this group, setting (generally) item[x].maximumContribution = item[x+1].minumumContribtion - 1
        // need to handle scenarios where there are n items with the same minimum contribution
        // for final item, maximumContribution = 999,999,999
        // test products:
        //  productId 39512 has 2 buffers :: 10, 15
        //  buffer 10 has 3 indices :: MCSI, Russell 2k, S&P 500
        //      index MCSI has 2 names
        //      index Russell2k has 2 names
        //      index S&P 500 has 3 names
        //      buffer 15 has 1 index :: S&P 500
        //      index S&P 500 has 1 names

        // productId 40400 has 3 buffers :: 10, 20, 30
        // buffer 10 has 9 indices :: DJUS, FinSPDR, Gold, MCSI, MCSIEmerging,NASDAQ-100, Oil, Russell 2k, S&P 500
        //      index DJUS has 1 name
        //      index FinSPDR has 1 name
        //      index Gold has 1 name
        //      index MCSI has 1 name
        //      index MCSIEmerging has 1 name
        //      index NASDAQ has 1 name
        //      index Oil has 1 name
        //      index Russell2k has 4 names
        //      index S&P 500 has 4 names
        // buffer 20 has 2 indices :: Russell 2k, S&P 500
        //     index Russell2k has 2 names
        //     index S&P 500 has 2 names
        // buffer 30 has 2 indices :: Russell 2k, S&P 500
        //     index Russell2k has 1 name
        //     index S&P 500 has 1 name

        List<ProductRate> rilaRates = [.. productRatess];

        var groupCounter = 1;
        string productId = rilaRates.First().ProductId;
        int minimumContributionCount = rilaRates.Select(z => z.Premium.Minimum).Distinct().Count();
        if (minimumContributionCount == 1)
        {
            foreach (ProductRate productRate in rilaRates)
            {
                productRate.DdwGroupId = $"{productId}_{groupCounter++}";
                productRate.Premium.Maximum = 999999999;
            }

            return;
        }

        Dictionary<long, long> minMaxContributionPairs = new();
        IEnumerable<double?> buffers = [.. rilaRates.Select(z => z.Buffer).Distinct()]; // all the buffers
        foreach (double? buffer in buffers)
        {
            // eg 90
            List<ProductRate> ratesGroupedByBuffer = [.. rilaRates.Where(z => AreFloatingPointValuesEqual(z.Buffer ?? 0d, buffer ?? 0d, 1e-3))]; // everything with 90
            IEnumerable<string?> indexNames = [.. ratesGroupedByBuffer.Select(z => z.MarketIndexId).Distinct()]; // all the indices for the 90s 
            foreach (string? indexName in indexNames)
            {
                // eg BlackRock Select Factor Index
                List<ProductRate> ratesGroupedByMarketingName = [.. ratesGroupedByBuffer.Where(z => z.MarketIndexId == indexName).OrderBy(z => z.Name)]; // everything 90, BlackRock Select Factor Index 
                IEnumerable<string?> marketingNames = [.. ratesGroupedByMarketingName.Select(z => z.Name).Distinct()];
                foreach (string? marketingName in marketingNames)
                {
                    List<ProductRate> marketingNameGroup = [.. ratesGroupedByMarketingName.Where(z => z.Name == marketingName).OrderBy(z => z.Premium.Minimum)];
                    for (var i = 0; i < marketingNameGroup.Count - 1; i++)
                    {
                        if (marketingNameGroup[i].Premium.Minimum is null)
                        {
                            _logger.LogCritical("{MethodName} :: Rila rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForRilaRates), productId);
                            continue;
                        }

                        long minContribution = marketingNameGroup[i].Premium.Minimum!.Value;
                        if (minMaxContributionPairs.TryGetValue(minContribution, out long pair))
                        {
                            marketingNameGroup[i].Premium.Maximum = pair;
                        }
                        else
                        {
                            // we need to get the next higher contribution limit (or 999,999,999 if there isn't one)
                            ProductRate? nextHighestMinimum = marketingNameGroup.OrderBy(z => z.Premium.Minimum).FirstOrDefault(z => z.Premium.Minimum > minContribution);
                            long maxContribution = 999999999;
                            if (nextHighestMinimum?.Premium.Minimum is not null)
                            {
                                maxContribution = nextHighestMinimum.Premium.Minimum!.Value - 1;
                            }

                            minMaxContributionPairs.Add(minContribution, maxContribution);
                            marketingNameGroup[i].Premium.Maximum = maxContribution;
                        }
                    }

                    marketingNameGroup[^1].Premium.Maximum = 999999999;
                    marketingNameGroup.ForEach(z => z.DdwGroupId = $"{productId}_{groupCounter}");
                    groupCounter++;
                }
            }
        }
    }

    private List<ProductRate> RemoveFutureDatedRates(List<ProductRate> rates, RateType rateType)
    {
        // if the rate being persisted has a "start" date in the future, do not ingest it
        // future is defined as the date portion being ahead of DateTime.UtcNow converted to NYT date
        List<ProductRate> retVal = [];
        DateTimeOffset now = ConvertDateTimeToNewYorkTime();
        foreach (ProductRate rate in rates)
        {
            switch (rateType)
            {
                case RateType.Fixed:
                    // FA rate JSON has a rateBeginDate field :: "2011-09-07T00:00:00"
                    // on our ProductRate object this is the Term.StartDate property
                    if (rate.Term?.StartDate?.Date <= now.Date)
                    {
                        retVal.Add(rate);
                    }

                    break;
                case RateType.Indexed:
                case RateType.Rila:
                    // IA rate JSON has a beginDate field :: "2016-04-02T00:00:00"
                    // RILA rate JSON has an effectiveDate field :: "07/07/2026"
                    // on our ProductRate object this is the StartDate property
                    if (rate.StartDate <= now.Date)
                    {
                        retVal.Add(rate);
                    }

                    break;

                default:
                    throw new ArgumentException($"Unknown rate type: {nameof(rateType)}");
            }
        }

        return retVal;
    }
    
    private DateTimeOffset ConvertDateTimeToNewYorkTime()
    {
        // Windows (aka developer) machines understand "Eastern Standard Time" while GCP servers understand "America/New_York" 
        string timeZoneId = Debugger.IsAttached ? "Eastern Standard Time" : @"America/New_York";
        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, easternZone.Id);
    }

    private static bool AreFloatingPointValuesEqual(
        double left,
        double right,
        double absoluteTolerance = 1e-9,
        double relativeTolerance = 1e-9)
    {
        if (!double.IsFinite(absoluteTolerance) || absoluteTolerance < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteTolerance),
                "The absolute tolerance must be finite and non-negative.");
        }

        if (!double.IsFinite(relativeTolerance) || relativeTolerance < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(relativeTolerance),
                "The relative tolerance must be finite and non-negative.");
        }

        if (double.IsNaN(left) || double.IsNaN(right))
        {
            return false;
        }

        // handles exactly equal values and matching infinities.
        if (left == right)
        {
            return true;
        }

        if (!double.IsFinite(left) || !double.IsFinite(right))
        {
            return false;
        }

        double difference = Math.Abs(left - right);
        if (difference <= absoluteTolerance)
        {
            return true;
        }

        double largestMagnitude = Math.Max(Math.Abs(left), Math.Abs(right));
        return difference <= largestMagnitude * relativeTolerance;
    }
    
    private void AddAliasesModifier(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

        // Create a temporary list to prevent modifying the collection while iterating
        var propertiesToCreate = new List<(string Alias, JsonPropertyInfo Original)>();

        foreach (var property in typeInfo.Properties)
        {
            // Extract custom alias attributes belonging to the property's underlying member info
            var attributes = property.AttributeProvider?
                .GetCustomAttributes(typeof(JsonAliasAttribute), inherit: true);

            if (attributes == null) continue;

            foreach (JsonAliasAttribute attr in attributes)
            {
                propertiesToCreate.Add((attr.Name, property));
            }
        }

        foreach (var (alias, originalProperty) in propertiesToCreate)
        {
            // Create an alternate property configuration pointing to the same underlying property logic
            var aliasProperty = typeInfo.CreateJsonPropertyInfo(originalProperty.PropertyType, alias);
            aliasProperty.Get = originalProperty.Get;
            aliasProperty.Set = originalProperty.Set;
        
            typeInfo.Properties.Add(aliasProperty);
        }
    }
    
    private static string ToBeaconRateCode(RateType rateType) => rateType switch
    {
        RateType.Fixed => "fa",
        RateType.Indexed => "ia",
        RateType.Rila => "iva",
        _ => throw new ArgumentOutOfRangeException(nameof(rateType), rateType, null)
    };
    
    private static string ToCategoryName(RateType rateType) => rateType switch
    {
        RateType.Fixed => "fixed",
        RateType.Indexed => "indexed",
        RateType.Rila => "rila",
        _ => throw new ArgumentOutOfRangeException(nameof(rateType), rateType, null)
    };

    private async Task<string> GetOrCreateMarketIndexId(string? indexName, string? tickerSymbol)
    {
        if (_marketIndices.Count == 0)
        {
            _marketIndices = await _firestoreService.GetMarketIndicesAsync();
        }
        
        // trim inputs
        indexName = indexName?.Trim() ?? string.Empty;
        tickerSymbol = tickerSymbol?.Trim() ?? string.Empty;
        
        // symbol match
        // lookup market-index where symbol = tickerSymbol || tickerSymbol.ToUpper(); return id if found
        if (!string.IsNullOrWhiteSpace(tickerSymbol))
        {
            MarketIndex? matchedIndex = _marketIndices.FirstOrDefault(z => z.Symbol.Equals(tickerSymbol, StringComparison.OrdinalIgnoreCase));
            if (matchedIndex is not null)
            {
                return matchedIndex.Id;
            }
        }

        // if indexName is null or whitespace, return string.Empty
        if (string.IsNullOrWhiteSpace(indexName))
        {
            return string.Empty;
        }

        // keyword match
        // lookup market-index where keywords contains indexName
        // find any results where the description is not null, return the 1st result id
        List<MarketIndex> matchedIndices = [.. _marketIndices
            .Where(z => z.Keywords.Contains(indexName, StringComparer.OrdinalIgnoreCase))
            .Where(z => !string.IsNullOrWhiteSpace(z.Description))];

        if (matchedIndices.Any())
        {
            return matchedIndices.First().Id;
        }
        
        // exact-name fallback
        // lookup market-index where name == indexName
        // return 1st result id
        matchedIndices = [.. _marketIndices
            .Where(z => string.Equals(z.Name, indexName, StringComparison.OrdinalIgnoreCase))];

        if (matchedIndices.Any())
        {
            return matchedIndices.First().Id;
        }

        // auto-create, flagged for review
        MarketIndex newIndex = new()
        {
            Name = indexName,
            Keywords = [indexName],
            Description = string.Empty,
            IsActive = true,
            NeedsReview = true,
            Symbol = tickerSymbol.ToUpper(),
            CreatedBy = "beacon-rate-ingestion",
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = "beacon-rate-ingestion",
            ModifiedOn = DateTime.UtcNow
        };

        newIndex = await _firestoreService.CreateMarketIndexAsync(newIndex);
        _marketIndices.Add(newIndex);
        return newIndex.Id;
    }
    
    private async Task<string> GetOrCreateCreditingMethodId(string? strategyName)
    {
        if (_creditingMethods.Count == 0)
        {
            _creditingMethods = await _firestoreService.GetCreditingMethodsAsync();
        }
        
        // trim inputs
        strategyName = strategyName?.Trim() ?? string.Empty;
       
        // if indexName is null or whitespace, return string.Empty
        if (string.IsNullOrWhiteSpace(strategyName))
        {
            return string.Empty;
        }

        // keyword match
        // lookup market-index where keywords contains indexName
        // find any results where the description is not null, return the 1st result id
        List<CreditingMethod> matchedMethods = [.. _creditingMethods
            .Where(z => z.Keywords.Contains(strategyName, StringComparer.OrdinalIgnoreCase))
            .Where(z => !string.IsNullOrWhiteSpace(z.Description))];

        if (matchedMethods.Any())
        {
            return matchedMethods.First().Id;
        }
        
        // exact-name fallback
        // lookup market-index where name == indexName
        // return 1st result id
        matchedMethods = [.. _creditingMethods
            .Where(z => string.Equals(z.Name, strategyName, StringComparison.OrdinalIgnoreCase))];

        if (matchedMethods.Any())
        {
            return matchedMethods.First().Id;
        }

        // auto-create, flagged for review
        CreditingMethod newMethod = new()
        {
            Name = strategyName,
            Keywords = [strategyName],
            Description = string.Empty,
            IsActive = true,
            NeedsReview = true,
            CreatedBy = "beacon-rate-ingestion",
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = "beacon-rate-ingestion",
            ModifiedOn = DateTime.UtcNow
        };

        newMethod = await _firestoreService.CreateCreditingMethodAsync(newMethod);
        _creditingMethods.Add(newMethod);
        return newMethod.Id;
    }
}
