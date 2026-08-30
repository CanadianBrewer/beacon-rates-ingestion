using System.Globalization;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class RilaRate : AnnuityBaseRate
{
    private double? _capRate;
    private double? _fixedRate;
    private double? _minFixedRate;
    private double? _participationRate;
    private double? _performanceTriggerRate;
    private string _productId = string.Empty;
    private double? _spreadRate;
    private List<string> _states = [];

    [FirestoreDocumentId]
    public override string DocId
    {
        get => Urn;
        set => Urn = value;
    }
    
    [JsonPropertyName("buffer")]
    [FirestoreProperty("buffer")]
    public double? Buffer { get; set; }

    [JsonPropertyName("capBail")]
    [FirestoreProperty("capBail")]
    public double? CapBail { get; set; }

    [JsonPropertyName("capMinimum")]
    [FirestoreProperty("capMinimum")]
    public double? CapMinimum { get; set; }

    [JsonPropertyName("capRate")]
    [FirestoreProperty("capRate")]
    public double? CapRate
    {
        get => _capRate / 100.0;
        set => _capRate = value;
    }

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

    [FirestoreProperty("beginDate")]
    public DateTime? EffectiveDate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RawEffectiveDate))
            {
                return null;
            }

            _ = DateTime.TryParseExact(RawEffectiveDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime retVal);
            return retVal == DateTime.MinValue ? null : retVal.ToUniversalTime();
        }
    }

    [JsonPropertyName("fixedRate")]
    [FirestoreProperty("fixedRate")]
    public double? FixedRate
    {
        get => _fixedRate / 100.0;
        set => _fixedRate = value;
    }

    [JsonPropertyName("floor")]
    [FirestoreProperty("floor")]
    public double? Floor { get; set; }

    [JsonPropertyName("index")]
    [FirestoreProperty("index")]
    public string? Index { get; set; }

    [JsonPropertyName("marketingName")]
    [FirestoreProperty("marketingName")]
    public string? MarketingName { get; set; }

    [FirestoreProperty("maximum")]
    public long? MaximumContribution { get; set; }


    [JsonPropertyName("minFixedRate")]
    [FirestoreProperty("minFixedRate")]
    public double? MinFixedRate
    {
        get => _minFixedRate / 100.0;
        set => _minFixedRate = value;
    }

    [JsonPropertyName("minimum")]
    [FirestoreProperty("minimum")]
    public long? MinimumContribution { get; set; }

    [JsonPropertyName("particaptionRateBailout")]
    [FirestoreProperty("participationRateBailout")]
    public double? ParticipationRateBailout { get; set; }

    [JsonPropertyName("particaptionRateMinimum")]
    [FirestoreProperty("participationRateMinimum")]
    public double? ParticipationRateMinimum { get; set; }

    [JsonPropertyName("participationRate")]
    [FirestoreProperty("participationRate")]
    public double? ParticipationRate
    {
        get => _participationRate / 100.0;
        set => _participationRate = value;
    }

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
    public double? PerformanceTriggerRate
    {
        get => _performanceTriggerRate / 100.0;
        set => _performanceTriggerRate = value;
    }

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
            return $"iva_{productId}";
        }
        set => _productId = value;
    }

    [JsonPropertyName("productName")]
    [FirestoreProperty("productName")]
    public string? ProductName { get; set; }

    [FirestoreProperty("rop")]
    public bool Rop
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RawRop) || string.Equals(RawRop, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }

    [JsonPropertyName("spreadMaximum")]
    [FirestoreProperty("spreadMaximum")]
    public double? SpreadMaximum { get; set; }

    [JsonPropertyName("spreadRate")]
    [FirestoreProperty("spreadRate")]
    public double? SpreadRate
    {
        get => _spreadRate / 100.0;
        set => _spreadRate = value;
    }

    [FirestoreProperty("overallStateAvailability")]
    public List<string> States
    {
        get
        {
            _states = RawStates is null ? [] : [.. RawStates.Split(',')];
            return _states;
        }
    }

    [JsonPropertyName("strategyName")]
    [FirestoreProperty("strategyName")]
    public string? StrategyName { get; set; }

    [JsonPropertyName("tickerSymbol")]
    [FirestoreProperty("tickerSymbol")]
    public string? TickerSymbol { get; set; }

    [JsonPropertyName("urn")]
    [FirestoreProperty("urn")]
    public string Urn { get; private set; } = Guid.NewGuid().ToString();

    // fields below are for our use, not sourced from beacon
    [FirestoreProperty("categoryId")]
    public string CategoryId => "rila";

    // fields below are for normalizing odd beacon data formats to what we want
    [JsonPropertyName("productId")] // 1080.0,
    public double RawProductId { get; set; }

    [JsonPropertyName("states")]
    public string? RawStates { get; set; }

    [JsonPropertyName("returnOfPremium")] // NO,
    public string? RawRop { get; set; }

    [JsonPropertyName("effectiveDate")]
    public string? RawEffectiveDate { get; set; }
}