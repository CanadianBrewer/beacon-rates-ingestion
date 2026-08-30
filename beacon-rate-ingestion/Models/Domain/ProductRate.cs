using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class ProductRate
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty("productId")]
    public string ProductId { get; set; } = string.Empty;

    [FirestoreProperty("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [FirestoreProperty("strategyId")]
    public string? StrategyId { get; set; }

    [FirestoreProperty("creditingMethodId")]
    public string? CreditingMethodId { get; set; }

    [FirestoreProperty("marketIndexId")]
    public string? MarketIndexId { get; set; }

    [FirestoreProperty("startDate")]
    public DateTime StartDate { get; set; }

    [FirestoreProperty("endDate")]
    public DateTime? EndDate { get; set; }

    [FirestoreProperty("name")]
    public string? Name { get; set; }

    [FirestoreProperty("description")]
    public string? Description { get; set; }

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    [FirestoreProperty("isClosed")]
    public bool IsClosed { get; set; }
    
    [FirestoreProperty("isVisible")]
    public bool IsVisible { get; set; }
    
    [FirestoreProperty("isProprietary")]
    public bool IsProprietary { get; set; }

    [FirestoreProperty("isRop")]
    public bool IsRop { get; set; }
    
    [FirestoreProperty("isScorecardEligible")]
    public bool IsScorecardEligible { get; set; }

    [FirestoreProperty("premium")]
    public PremiumRange? Premium { get; set; }

    [FirestoreProperty("stateAvailability")]
    public List<string>? StateAvailability { get; set; }

    [FirestoreProperty("term")]
    public RateTerm? Term { get; set; }

    [FirestoreProperty("creditingFrequency")]
    public string? CreditingFrequency { get; set; }

    [FirestoreProperty("rate")]
    public RateValue? Rate { get; set; }

    [FirestoreProperty("cap")]
    public RateRange? Cap { get; set; }

    [FirestoreProperty("floor")]
    public double? Floor { get; set; }

    [FirestoreProperty("buffer")]
    public double? Buffer { get; set; }

    [FirestoreProperty("spread")]
    public RateRange? Spread { get; set; }

    [FirestoreProperty("participation")]
    public RateRange? Participation { get; set; }

    [FirestoreProperty("trigger")]
    public TriggerRate? Trigger { get; set; }

    [FirestoreProperty("minimumCredit")]
    public double? MinimumCredit { get; set; }

    [FirestoreProperty("bonus")]
    public BonusRate? Bonus { get; set; }

    [FirestoreProperty("terms")]
    public Dictionary<string, object?>? Terms { get; set; }

    [FirestoreProperty("source")]
    public string? Source { get; set; }

    [FirestoreProperty("sourceId")]
    public string? SourceId { get; set; }

    [FirestoreProperty("symbol")]
    public string? Symbol { get; set; }
    

    
    [FirestoreData]
    public class PremiumRange
    {
        [FirestoreProperty("minimum")]
        public double? Minimum { get; set; }

        [FirestoreProperty("maximum")]
        public double? Maximum { get; set; }
    }

    [FirestoreData]
    public class RateTerm
    {
        [FirestoreProperty("value")]
        public double? Value { get; set; }

        [FirestoreProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [FirestoreProperty("endDate")]
        public DateTime? EndDate { get; set; }
    }

    [FirestoreData]
    public class RateValue
    {
        [FirestoreProperty("value")]
        public double? Value { get; set; }

        [FirestoreProperty("minimum")]
        public double? Minimum { get; set; }

        [FirestoreProperty("guaranteed")]
        public double? Guaranteed { get; set; }
    }

    [FirestoreData]
    public class RateRange
    {
        [FirestoreProperty("value")]
        public double Value { get; set; }

        [FirestoreProperty("minimum")]
        public double? Minimum { get; set; }

        [FirestoreProperty("maximum")]
        public double? Maximum { get; set; }

        [FirestoreProperty("bailout")]
        public double? Bailout { get; set; }
    }

    [FirestoreData]
    public class TriggerRate
    {
        [FirestoreProperty("value")]
        public double? Value { get; set; }

        [FirestoreProperty("rate")]
        public double? Rate { get; set; }
    }

    [FirestoreData]
    public class BonusRate
    {
        [FirestoreProperty("value")]
        public double Value { get; set; }

        [FirestoreProperty("term")]
        public double? Term { get; set; }

        [FirestoreProperty("type")]
        public string? Type { get; set; }
    }
}
