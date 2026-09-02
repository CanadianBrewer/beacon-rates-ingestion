using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using BeaconDataIngestion.Models.DataModels.DDW.Rates;
using DueDiligenceWorks.Beacon.RateIngestion.Models.Application;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public class BeaconRatesApiService(
    IOptions<BeaconRatesApiConfig> apiConfig,
    ILogger<BeaconRatesApiService> logger,
    IFirestoreService firestoreService,
    HttpClient httpClient) : IBeaconRatesApiService
{
    private readonly BeaconRatesApiConfig _apiConfig = apiConfig.Value;
    private readonly ParallelOptions _parallelOptions = new() { MaxDegreeOfParallelism = 25 };

    public async Task GetAllRates()
    {
        logger.LogInformation("Rate processing started");
        // await GetFixedRates();
        await GetFixedRatesV2();
        await GetIndexedRates();
        await GetRilaRates();
        await firestoreService.UpdateRatesLastUpdatedOnAsync();
        logger.LogInformation("Rate processing completed");
    }

    // public async Task GetFixedRates()
    // {
    //     await firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-fixed-rates");
    //
    //     List<FixedAnnuityRate> fixedAnnuityRates = await GetRatesFromBeaconAsync<FixedAnnuityRate>("fa");
    //     fixedAnnuityRates = RemoveFutureDatedRates(fixedAnnuityRates);
    //     List<string> productIds = [.. fixedAnnuityRates.Select(z => z.ProductId).Distinct()];
    //     List<string> inactiveProductIds = await firestoreService.GetInactiveProductIdsAsync("fixed");
    //
    //     logger.LogInformation("Processing {ItemCount} fixed rates", productIds.Count);
    //     var counter = 1;
    //     await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
    //     {
    //         if (inactiveProductIds.Contains(productId))
    //         {
    //             return;
    //         }
    //     
    //         logger.LogDebug("Processing {Index}/{ItemCount} fixed rates", Interlocked.Increment(ref counter), productIds.Count);
    //         CalculateMaximumContributionsForFixedRates(fixedAnnuityRates.Where(z => z.ProductId == productId));
    //         await firestoreService.DeleteRatesForProductAsync(productId, ct);
    //         await firestoreService.PersistRatesAsync([.. fixedAnnuityRates.Where(z => z.ProductId == productId)], ct);
    //         await firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
    //     });
    //     
    //     // now grab all the fixed rates in the collection and delete any where the product id is not in the list of product ids we just processed
    //     // this is to handle the case where the Beacon API has been updated we are no longer receiving rates for a product id
    //     // we want to remove old product-rate data
    //     List<FixedAnnuityRate> allFixedRates = await firestoreService.GetAllAnnuitiesRatesAsync<FixedAnnuityRate>("fixed");
    //     await Parallel.ForEachAsync(allFixedRates, _parallelOptions, async (fixedRate, ct) =>
    //     {
    //         if (productIds.Contains(fixedRate.ProductId))
    //         {
    //             return;
    //         }
    //
    //         logger.LogDebug("Deleting fixed rate {Urn} for product {ProductId} as it is no longer provided by Beacon", fixedRate.Urn, fixedRate.ProductId);
    //         await firestoreService.DeleteRatesForProductAsync(fixedRate.ProductId, ct);
    //     });
    //     
    //     logger.LogInformation("Finished processing {ItemCount} fixed rates", productIds.Count);
    // }

    public async Task GetFixedRatesV2()
    {
        List<ProductRate> fixedAnnuityRates = await GetRatesFromBeaconAsyncV2("fa");
        fixedAnnuityRates = [ .. fixedAnnuityRates.Where(z => z.ProductId == "fa_619")];
        
        fixedAnnuityRates = RemoveFutureDatedRates(fixedAnnuityRates);
        List<string> productIds = [.. fixedAnnuityRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await firestoreService.GetInactiveProductIdsAsync("fixed");

        logger.LogInformation("Processing {ItemCount} fixed rates", productIds.Count);
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                return;
            }

            logger.LogDebug("Processing {Index}/{ItemCount} fixed rates", Interlocked.Increment(ref counter), productIds.Count);
            CalculateMaximumContributionsForFixedRates(fixedAnnuityRates.Where(z => z.ProductId == productId));
            await firestoreService.DeleteRatesForProductAsync(productId, ct);
            await firestoreService.PersistRatesAsync([.. fixedAnnuityRates.Where(z => z.ProductId == productId)], ct);
            await firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the fixed rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated we are no longer receiving rates for a product id
        // we want to remove old product-rate data
        List<FixedAnnuityRate> allFixedRates = await firestoreService.GetAllAnnuitiesRatesAsync<FixedAnnuityRate>("fixed");
        await Parallel.ForEachAsync(allFixedRates, _parallelOptions, async (fixedRate, ct) =>
        {
            if (productIds.Contains(fixedRate.ProductId))
            {
                return;
            }

            logger.LogDebug("Deleting fixed rate {Urn} for product {ProductId} as it is no longer provided by Beacon", fixedRate.Urn, fixedRate.ProductId);
            await firestoreService.DeleteRatesForProductAsync(fixedRate.ProductId, ct);
        });

        logger.LogInformation("Finished processing {ItemCount} fixed rates", productIds.Count);
    }

    public async Task GetIndexedRates()
    {
        await firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-indexed-rates");

        List<IndexedAnnuityRate> indexedAnnuityRates = await GetRatesFromBeaconAsync<IndexedAnnuityRate>("ia");
        indexedAnnuityRates = RemoveFutureDatedRates(indexedAnnuityRates);
        List<string> productIds = [.. indexedAnnuityRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await firestoreService.GetInactiveProductIdsAsync("indexed");

        logger.LogInformation("Processing {ItemCount} indexed rates", productIds.Count);
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                return;
            }

            logger.LogDebug("Processing {Index}/{ItemCount} indexed rates", Interlocked.Increment(ref counter), productIds.Count);
            CalculateMaximumContributionsForIndexedRates(indexedAnnuityRates.Where(z => z.ProductId == productId));
            await firestoreService.DeleteRatesForProductAsync(productId, ct);
            await firestoreService.PersistRatesAsync([.. indexedAnnuityRates.Where(z => z.ProductId == productId)], ct);
            await firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the indexed rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated and the product id has been removed from the collection
        List<IndexedAnnuityRate> allIndexedRates = await firestoreService.GetAllAnnuitiesRatesAsync<IndexedAnnuityRate>("indexed");
        await Parallel.ForEachAsync(allIndexedRates, _parallelOptions, async (indexedRate, ct) =>
        {
            if (productIds.Contains(indexedRate.ProductId))
            {
                return;
            }

            logger.LogDebug("Deleting indexed rate {Urn} for product {ProductId} as it is no longer provided by Beacon", indexedRate.Urn, indexedRate.ProductId);
            await firestoreService.DeleteRatesForProductAsync(indexedRate.ProductId, ct);
        });

        logger.LogInformation("Finished processing {ItemCount} indexed rates", productIds.Count);
    }

    public async Task GetRilaRates()
    {
        await firestoreService.SetLastActivityDateAsync(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture), "get-rila-rates");

        List<RilaRate> rilaRates = await GetRatesFromBeaconAsync<RilaRate>("iva");
        rilaRates = RemoveFutureDatedRates(rilaRates);
        List<string> productIds = [.. rilaRates.Select(z => z.ProductId).Distinct()];
        List<string> inactiveProductIds = await firestoreService.GetInactiveProductIdsAsync("rila");

        logger.LogInformation("Processing {ItemCount} rila rates", productIds.Count());
        var counter = 1;
        await Parallel.ForEachAsync(productIds, _parallelOptions, async (productId, ct) =>
        {
            if (inactiveProductIds.Contains(productId))
            {
                logger.LogInformation("Skipping rila rate for product {ProductId} as it is inactive", productId);
                return;
            }

            logger.LogDebug("Processing {Index}/{ItemCount} rila rates", Interlocked.Increment(ref counter), productIds.Count());
            CalculateMaximumContributionsForRilaRates(rilaRates.Where(z => z.ProductId == productId));
            await firestoreService.DeleteRatesForProductAsync(productId, ct);
            await firestoreService.PersistRatesAsync([.. rilaRates.Where(z => z.ProductId == productId)], ct);
            await firestoreService.SetAnnuityRatesLastUpdatedOnAsync(productId);
        });

        // now grab all the rila rates in the collection and delete any where the product id is not in the list of product ids we just processed
        // this is to handle the case where the Beacon API has been updated and the product id has been removed from the collection
        List<RilaRate> allRilaRates = await firestoreService.GetAllAnnuitiesRatesAsync<RilaRate>("rila");
        await Parallel.ForEachAsync(allRilaRates, _parallelOptions, async (rilaRate, ct) =>
        {
            if (productIds.Contains(rilaRate.ProductId))
            {
                return;
            }

            logger.LogInformation("Deleting rila rate {Urn} for product {ProductId} as it is no longer provided by Beacon", rilaRate.Urn, rilaRate.ProductId);
            await firestoreService.DeleteRatesForProductAsync(rilaRate.ProductId, ct);
        });
    }

    private async Task<List<T>> GetRatesFromBeaconAsync<T>(string rateType) where T : AnnuityBaseRate
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
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            HttpClient customHttpClient = new(handler);
            httpResponseMessage = await customHttpClient.SendAsync(httpRequestMessage);
        }
        else
        {
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        }

        stopwatch.Stop();
        logger.LogInformation("API call to {Url} took {ElapsedMilliseconds}ms", httpRequestMessage.RequestUri, stopwatch.ElapsedMilliseconds);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var rates = JsonSerializer.Deserialize<List<T>>(json);
                return rates!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deserialization failure: {RateType} | {ResponseMessage}",
                    rateType,
                    JsonSerializer.Serialize(httpResponseMessage));
                throw;
            }
        }

        // log error
        logger.LogError("The call to the Beacon Api for rates processing returned HTTP {StatusCode} and body {ResponseMessage)}",
            httpResponseMessage.StatusCode,
            JsonSerializer.Serialize(httpResponseMessage));
        throw new BeaconException($"Beacon API call returned {httpResponseMessage.StatusCode}");
    }

    private async Task<List<ProductRate>> GetRatesFromBeaconAsyncV2(string rateType)
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
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            HttpClient customHttpClient = new(handler);
            httpResponseMessage = await customHttpClient.SendAsync(httpRequestMessage);
        }
        else
        {
            httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        }

        stopwatch.Stop();
        logger.LogInformation("API call to {Url} took {ElapsedMilliseconds}ms", httpRequestMessage.RequestUri, stopwatch.ElapsedMilliseconds);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            try
            {
                var beaconRates = JsonSerializer.Deserialize<List<BeaconFixedRate>>(json);
                List<ProductRate> rates = [];
                foreach (BeaconFixedRate beaconRate in beaconRates)
                {
                    rates.Add(BeaconFixedRateMapper.ToProductRate(beaconRate));
                }

                return rates!;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deserialization failure: {RateType} | {ResponseMessage}",
                    rateType,
                    JsonSerializer.Serialize(httpResponseMessage));
                throw;
            }
        }

        // log error
        logger.LogError("The call to the Beacon Api for rates processing returned HTTP {StatusCode} and body {ResponseMessage)}",
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
                List<ProductRate> ratesGroupedBySurrenderId = [
                    .. ratesGroupedByVarId
                        .Where(z => z.Terms.SurrenderId == surrenderId)
                        .OrderBy(z => z.Premium.Minimum)
                ];
                
                for (var i = 0; i < ratesGroupedBySurrenderId.Count - 1; i++)
                {
                    if (ratesGroupedBySurrenderId[i].Premium.Minimum is null)
                    {
                        logger.LogCritical("{MethodName} :: Fixed rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForFixedRates), productId);
                        continue;
                    }

                    long premiumMinimum = 0;
                    if (ratesGroupedBySurrenderId[i].Premium.Minimum.HasValue)
                    {
                        premiumMinimum = (long) ratesGroupedBySurrenderId[i].Premium.Minimum!.Value;
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
                            maxContribution = (long)nextHighestMinimum.Premium.Minimum.Value - 1;
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

    private void CalculateMaximumContributionsForIndexedRates(IEnumerable<IndexedAnnuityRate> productRates)
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

        List<IndexedAnnuityRate> indexedRates = [.. productRates];

        var groupCounter = 1;
        string productId = indexedRates.First().ProductId;
        int minimumContributionCount = indexedRates.Select(z => z.MinimumContribution).Distinct().Count();
        if (minimumContributionCount == 1)
        {
            foreach (IndexedAnnuityRate productRate in indexedRates)
            {
                productRate.DdwGroupId = $"{productId}_{groupCounter++}";
                productRate.MaximumContribution = 999999999;
            }

            return;
        }

        Dictionary<long, long> minMaxContributionPairs = new();
        IEnumerable<string?> marketingNames = [.. indexedRates.Select(z => z.MarketingName).Distinct()];
        foreach (string? marketingName in marketingNames)
        {
            List<IndexedAnnuityRate> ratesGroupedByMarketingName = [.. indexedRates.Where(z => z.MarketingName == marketingName)];
            IEnumerable<string?> indexNames = ratesGroupedByMarketingName.Select(z => z.Index).Distinct().ToList();
            foreach (string? indexName in indexNames)
            {
                List<IndexedAnnuityRate> ratesGroupedByState = ratesGroupedByMarketingName.Where(z => z.Index == indexName).OrderBy(z => z.RawOverallStateAvailability).ToList();
                IEnumerable<string?> stateGroups = [.. ratesGroupedByState.Select(z => z.RawOverallStateAvailability).Distinct()];
                foreach (string? stateGroup in stateGroups)
                {
                    List<IndexedAnnuityRate> stateRateGroup = ratesGroupedByState.Where(z => z.RawOverallStateAvailability == stateGroup).OrderBy(z => z.MinimumContribution).ToList();
                    for (var i = 0; i < stateRateGroup.Count - 1; i++)
                    {
                        if (stateRateGroup[i].MinimumContribution is null)
                        {
                            logger.LogCritical("{MethodName} :: Indexed rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForIndexedRates), productId);
                            continue;
                        }

                        long minContribution = stateRateGroup[i].MinimumContribution!.Value;
                        if (minMaxContributionPairs.TryGetValue(minContribution, out long pair))
                        {
                            stateRateGroup[i].MaximumContribution = pair;
                        }
                        else
                        {
                            // we need to get the next higher contribution limit (or 999,999,999 if there isn't one)
                            IndexedAnnuityRate? nextHighestMinimum = stateRateGroup.OrderBy(z => z.MinimumContribution).FirstOrDefault(z => z.MinimumContribution > minContribution);
                            long maxContribution = 999999999;
                            if (nextHighestMinimum?.MinimumContribution is not null)
                            {
                                maxContribution = nextHighestMinimum.MinimumContribution!.Value - 1;
                            }

                            minMaxContributionPairs.Add(minContribution, maxContribution);
                            stateRateGroup[i].MaximumContribution = maxContribution;
                        }
                    }

                    stateRateGroup[^1].MaximumContribution = 999999999;
                    stateRateGroup.ForEach(z => z.DdwGroupId = $"{productId}_{groupCounter}");
                    groupCounter++;
                }
            }
        }
    }

    private void CalculateMaximumContributionsForRilaRates(IEnumerable<RilaRate> productRatess)
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

        List<RilaRate> rilaRates = [.. productRatess];

        var groupCounter = 1;
        string productId = rilaRates.First().ProductId;
        int minimumContributionCount = rilaRates.Select(z => z.MinimumContribution).Distinct().Count();
        if (minimumContributionCount == 1)
        {
            foreach (RilaRate productRate in rilaRates)
            {
                productRate.DdwGroupId = $"{productId}_{groupCounter++}";
                productRate.MaximumContribution = 999999999;
            }

            return;
        }

        Dictionary<long, long> minMaxContributionPairs = new();
        IEnumerable<double?> buffers = [.. rilaRates.Select(z => z.Buffer).Distinct()]; // all the buffers
        foreach (double? buffer in buffers)
        {
            // eg 90
            List<RilaRate> ratesGroupedByBuffer = [.. rilaRates.Where(z => AreFloatingPointValuesEqual(z.Buffer ?? 0d, buffer ?? 0d, 1e-3))]; // everything with 90
            IEnumerable<string?> indexNames = [.. ratesGroupedByBuffer.Select(z => z.Index).Distinct()]; // all the indices for the 90s 
            foreach (string? indexName in indexNames)
            {
                // eg BlackRock Select Factor Index
                List<RilaRate> ratesGroupedByMarketingName = [.. ratesGroupedByBuffer.Where(z => z.Index == indexName).OrderBy(z => z.MarketingName)]; // everything 90, BlackRock Select Factor Index 
                IEnumerable<string?> marketingNames = [.. ratesGroupedByMarketingName.Select(z => z.MarketingName).Distinct()];
                foreach (string? marketingName in marketingNames)
                {
                    List<RilaRate> marketingNameGroup = [.. ratesGroupedByMarketingName.Where(z => z.MarketingName == marketingName).OrderBy(z => z.MinimumContribution)];
                    for (var i = 0; i < marketingNameGroup.Count - 1; i++)
                    {
                        if (marketingNameGroup[i].MinimumContribution is null)
                        {
                            logger.LogCritical("{MethodName} :: Rila rate for {ProductId} had a NULL minimum contribution", nameof(CalculateMaximumContributionsForRilaRates), productId);
                            continue;
                        }

                        long minContribution = marketingNameGroup[i].MinimumContribution!.Value;
                        if (minMaxContributionPairs.TryGetValue(minContribution, out long pair))
                        {
                            marketingNameGroup[i].MaximumContribution = pair;
                        }
                        else
                        {
                            // we need to get the next higher contribution limit (or 999,999,999 if there isn't one)
                            RilaRate? nextHighestMinimum = marketingNameGroup.OrderBy(z => z.MinimumContribution).FirstOrDefault(z => z.MinimumContribution > minContribution);
                            long maxContribution = 999999999;
                            if (nextHighestMinimum?.MinimumContribution is not null)
                            {
                                maxContribution = nextHighestMinimum.MinimumContribution!.Value - 1;
                            }

                            minMaxContributionPairs.Add(minContribution, maxContribution);
                            marketingNameGroup[i].MaximumContribution = maxContribution;
                        }
                    }

                    marketingNameGroup[^1].MaximumContribution = 999999999;
                    marketingNameGroup.ForEach(z => z.DdwGroupId = $"{productId}_{groupCounter}");
                    groupCounter++;
                }
            }
        }
    }

    private List<ProductRate> RemoveFutureDatedRates(List<ProductRate> rates)
    {
        // 23 Feb 2026
        // FA rates have rateBeginDate field    :: "2011-09-07T00:00:00"
        // if the rate being persisted has a "start" date in the future, do not ingest it
        // future is defined as the date portion being ahead of DateTime.UtcNow converted to NYT date
        List<ProductRate> retVal = [];
        DateTimeOffset now = ConvertDateTimeToNewYorkTime();
        foreach (ProductRate rate in rates)
        {
            // on our FixedAnnuityRate object this is the BeginDate property
            if (rate.Term?.StartDate?.Date <= now.Date)
            {
                retVal.Add(rate);
            }
            else
            {
                logger.LogDebug("FixedAnnuityRate {ProductId} Urn {Urn} had a future BeginDate of {BeginDate}", rate.ProductId, rate.Id, rate.StartDate);
            }
        }

        return retVal;
    }

    private List<IndexedAnnuityRate> RemoveFutureDatedRates(List<IndexedAnnuityRate> rates)
    {
        // 23 Feb 2026
        // IA rates have a beginDate field      :: "2016-04-20T00:00:00"
        // if the rate being persisted has a "start" date in the future, do not ingest it
        // future is defined as the date portion being ahead of DateTime.UtcNow converted to NYT date
        List<IndexedAnnuityRate> retVal = [];
        DateTimeOffset now = ConvertDateTimeToNewYorkTime();
        foreach (IndexedAnnuityRate rate in rates)
        {
            // on our IndexedAnnuityRate object this is the BeginDate property
            if (rate.BeginDate?.Date <= now.Date)
            {
                retVal.Add(rate);
            }
            else
            {
                logger.LogWarning("IndexedAnnuityRate {ProductId} Urn {Urn} had a future BeginDate of {BeginDate}", rate.ProductId, rate.Urn, rate.BeginDate);
            }
        }

        return retVal;
    }

    private List<RilaRate> RemoveFutureDatedRates(List<RilaRate> rates)
    {
        // 23 Feb 2026
        // IVA rates have a effectiveDate field :: "01/07/2026"
        // if the rate being persisted has a "start" date in the future, do not ingest it
        // future is defined as the date portion being ahead of DateTime.UtcNow converted to NYT date
        List<RilaRate> retVal = [];
        DateTimeOffset now = ConvertDateTimeToNewYorkTime();
        foreach (RilaRate rate in rates)
        {
            if (string.IsNullOrWhiteSpace(rate.Urn))
            {
                continue;
            }

            // on our RilaRate object this is the EffectiveDate property
            if (rate.EffectiveDate?.Date <= now.Date)
            {
                retVal.Add(rate);
            }
            else
            {
                logger.LogWarning("RilaRate {ProductId} Urn {Urn} had a future EffectiveDate of {BeginDate}", rate.ProductId, rate.Urn, rate.EffectiveDate);
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

        // Handles exactly equal values and matching infinities.
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
}