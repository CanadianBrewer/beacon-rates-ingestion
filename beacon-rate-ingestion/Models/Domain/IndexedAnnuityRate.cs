using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class IndexedAnnuityRate : AnnuityBaseRate
{
    private DateTime? _beginDate;
    private DateTime? _endDate;
    private List<string> _gmirStates1 = [];
    private List<string> _gmirStates2 = [];
    private List<string> _overallStates = [];
    private string _productId = string.Empty;

    [FirestoreDocumentId]
    public override string DocId
    {
        get => Urn;
        set => Urn = value;
    }
    
    [JsonPropertyName("beginDate")]
    [FirestoreProperty("beginDate")]
    public DateTime? BeginDate
    {
        get => _beginDate?.ToUniversalTime() ?? _beginDate;
        set => _beginDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("capBail")]
    [FirestoreProperty("capBail")]
    public double? CapBail { get; set; }

    [JsonPropertyName("capMinimum")]
    [FirestoreProperty("capMinimum")]
    public double? CapMinimum { get; set; }

    [JsonPropertyName("capRate")]
    [FirestoreProperty("capRate")]
    public double? CapRate { get; set; }

    [JsonPropertyName("companyName")]
    [FirestoreProperty("companyName")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("creditingFrequency")]
    [FirestoreProperty("creditingFrequency")]
    public string? CreditingFrequency { get; set; }

    [JsonPropertyName("creditStrategy")]
    [FirestoreProperty("creditStrategy")]
    public string? CreditStrategy { get; set; }

    [JsonPropertyName("description")]
    [FirestoreProperty("description")]
    public string? Description { get; set; }

    [JsonPropertyName("endDate")]
    [FirestoreProperty("endDate")]
    public DateTime? EndDate
    {
        get => _endDate?.ToUniversalTime() ?? _endDate;
        set => _endDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("fixedRate")]
    [FirestoreProperty("fixedRate")]
    public double? FixedRate { get; set; }

    [JsonPropertyName("gmirCode1")]
    [FirestoreProperty("gmirCode1")]
    public string? GmirCode1 { get; set; }

    [JsonPropertyName("gmirCode2")]
    [FirestoreProperty("gmirCode2")]
    public string? GmirCode2 { get; set; }

    [FirestoreProperty("gmirStateAvailability1")]
    public List<string> GmirStateAvailability1
    {
        get
        {
            _gmirStates1 = RawGmirStateAvailability1 is null ? [] : [.. RawGmirStateAvailability1.Split(',')];
            return _gmirStates1;
        }
    }

    [FirestoreProperty("gmirStateAvailability2")]
    public List<string> GmirStateAvailability2
    {
        get
        {
            _gmirStates2 = RawGmirStateAvailability2 is null ? [] : [.. RawGmirStateAvailability2.Split(',')];
            return _gmirStates2;
        }
    }

    [JsonPropertyName("index")]
    [FirestoreProperty("index")]
    public string? Index { get; set; }

    [FirestoreProperty("maximum")]
    public long? MaximumContribution { get; set; }

    [JsonPropertyName("minFixedRate")]
    [FirestoreProperty("minFixedRate")]
    public double? MinFixedRate { get; set; }

    [JsonPropertyName("minimum")]
    [FirestoreProperty("minimum")]
    public long? MinimumContribution { get; set; }

    [JsonPropertyName("minimumGuaranteedRate1")]
    [FirestoreProperty("minimumGuaranteedRate1")]
    public double? MinimumGuaranteedRate1 { get; set; }

    [JsonPropertyName("minimumGuaranteedRate2")]
    [FirestoreProperty("minimumGuaranteedRate2")]
    public double? MinimumGuaranteedRate2 { get; set; }

    [JsonPropertyName("minimumInitialGuaranteePercent1")]
    [FirestoreProperty("minimumInitialGuaranteePercent1")]
    public double? MinimumInitialGuaranteePercent1 { get; set; }

    [JsonPropertyName("minimumInitialGuaranteePercent2")]
    [FirestoreProperty("minimumInitialGuaranteePercent2")]
    public double? MinimumInitialGuaranteePercent2 { get; set; }

    [JsonPropertyName("mktgname")]
    [FirestoreProperty("marketingName")]
    public string? MarketingName { get; set; }

    [FirestoreProperty("overallStateAvailability")]
    public List<string> OverallStateAvailability
    {
        get
        {
            _overallStates = RawOverallStateAvailability is null ? [] : [.. RawOverallStateAvailability.Split(',')];
            return _overallStates;
        }
    }

    [JsonPropertyName("participationRate")]
    [FirestoreProperty("participationRate")]
    public double? ParticipationRate { get; set; }

    [JsonPropertyName("particaptionRateBailout")]
    [FirestoreProperty("participationRateBailout")]
    public double? ParticipationRateBailout { get; set; }

    [JsonPropertyName("particaptionRateMinimum")]
    [FirestoreProperty("participationRateMinimum")]
    public double? ParticipationRateMinimum { get; set; }

    [JsonPropertyName("perChangeTerm")]
    [FirestoreProperty("perChangeTerm")]
    public string? PerChangeTerm { get; set; }

    [JsonPropertyName("performanceTrigger")]
    [FirestoreProperty("performanceTrigger")]
    public double? PerformanceTrigger { get; set; }

    [JsonPropertyName("performanceTriggerMinimumCredit")]
    [FirestoreProperty("performanceTriggerMinimumCredit")]
    public double? PerformanceTriggerMinimumCredit { get; set; }

    [JsonPropertyName("performanceTriggerRate")]
    [FirestoreProperty("performanceTriggerRate")]
    public double? PerformanceTriggerRate { get; set; }

    [FirestoreProperty("productId")]
    public string ProductId
    {
        get
        {
            if (RawProductId == 0)
            {
                return _productId;
            }

            _ = long.TryParse(RawProductId.ToString(), out long productId);
            return $"ia_{productId}";
        }
        set => _productId = value;
    }

    [JsonPropertyName("productName")]
    [FirestoreProperty("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("rebalanceFixedAllocation")]
    [FirestoreProperty("rebalanceFixedAllocation")]
    public double? RebalanceFixedAllocation { get; set; }

    [JsonPropertyName("rebalanceFixedRate")]
    [FirestoreProperty("rebalanceFixedRate")]
    public double? RebalanceFixedRate { get; set; }

    [JsonPropertyName("rebalanceIndexAllocation")]
    [FirestoreProperty("rebalanceIndexAllocation")]
    public double? RebalanceIndexAllocation { get; set; }

    [JsonPropertyName("returnofPremium")]
    [FirestoreProperty("rop")]
    public bool? Rop { get; set; }

    [JsonPropertyName("spreadMaximum")]
    [FirestoreProperty("spreadMaximum")]
    public double? SpreadMaximum { get; set; }

    [JsonPropertyName("spreadRate")]
    [FirestoreProperty("spreadRate")]
    public double? SpreadRate { get; set; }

    [JsonPropertyName("strategyName")]
    [FirestoreProperty("strategyName")]
    public string? StrategyName { get; set; }

    [JsonPropertyName("tickerSymbol")]
    [FirestoreProperty("tickerSymbol")]
    public string? TickerSymbol { get; set; }

    [JsonPropertyName("urn")]
    [FirestoreProperty("urn")]
    public string Urn { get; set; } = Guid.NewGuid().ToString();

    // fields below are for our use, not sourced from beacon
    [FirestoreProperty("categoryId")]
    public string CategoryId => "indexed";

    // fields below are for normalizing odd beacon data formats to what we want 
    [JsonPropertyName("productId")] // 1080.0,
    public long RawProductId { get; set; }

    [JsonPropertyName("gmirstateAvailability1")]
    public string? RawGmirStateAvailability1 { get; set; }

    [JsonPropertyName("gmirstateAvailability2")]
    public string? RawGmirStateAvailability2 { get; set; }

    [JsonPropertyName("overallStateAvailability")]
    public string? RawOverallStateAvailability { get; set; }
}