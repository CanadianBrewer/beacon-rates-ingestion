using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class ProductRate
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty("productId")]
    public string ProductId { get; set; } = string.Empty;

    [FirestoreProperty("ddwGroupId")]
    public string DdwGroupId { get; set; } = string.Empty;

    [FirestoreProperty("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [FirestoreProperty("strategyId")]
    public string? StrategyId { get; set; }

    [FirestoreProperty("strategyName")]
    public string? StrategyName { get; set; }
    
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
    public bool IsActive { get; set; } = true;

    [FirestoreProperty("isClosed")]
    public bool IsClosed { get; set; }

    [FirestoreProperty("isVisible")]
    public bool IsVisible { get; set; } = true;

    [FirestoreProperty("isProprietary")]
    public bool IsProprietary { get; set; }

    [FirestoreProperty("isRop")]
    public bool IsRop { get; set; }

    [FirestoreProperty("isScorecardEligible")]
    public bool IsScorecardEligible { get; set; }

    [FirestoreProperty("premium")]
    public PremiumRange Premium { get; set; } = new();

    [FirestoreProperty("states")]
    public List<string>? States { get; set; } = [];
    
    // used exclusively for grouping rates by state
    internal string? StatesJoined => string.Join(",", States ?? []);

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
    public Terms Terms { get; set; } = new();

    [FirestoreProperty("source")]
    public string? Source { get; set; }

    [FirestoreProperty("sourceId")]
    public string? SourceId { get; set; }

    [FirestoreProperty("symbol")]
    public string? Symbol { get; set; }

    public int? VarId { get; set; }
}

    [FirestoreData]
    public class PremiumRange
    {
        [FirestoreProperty("minimum")]
        public long? Minimum { get; set; }

        [FirestoreProperty("maximum")]
        public long? Maximum { get; set; }
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
        public double? Value { get; set; }

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
        
        [FirestoreProperty("bailout")]
        public double? Bailout { get; set; }
    }

    [FirestoreData]
    public class BonusRate
    {
        [FirestoreProperty("value")]
        public double? Value { get; set; }

        [FirestoreProperty("term")]
        public double? Term { get; set; }

        [FirestoreProperty("type")]
        public string? Type { get; set; }
    }

    [FirestoreData]
    public class Terms
    {   
        // common
        [FirestoreProperty("rop")]
        public bool? Rop { get; set;}
        
        // fixed
        [FirestoreProperty("productType")]
        public string? ProductType { get; set;}
        
        [FirestoreProperty("interestType")]
        public string? InterestType { get; set;}
        
        [FirestoreProperty("mva")]
        public bool? Mva { get; set;}
        
        [FirestoreProperty("qualifier")]
        public string? Qualifier { get; set;}
        
        [FirestoreProperty("bailoutRate")]
        public double? BailoutRate { get; set;}
        
        [FirestoreProperty("surrenderExpirationDate")]
        public DateTime? SurrenderExpirationDate { get; set;}
        
        [FirestoreProperty("surrenderId")]
        public int? SurrenderId { get; set;}
        
        [FirestoreProperty("surrenderIncreaseDate")]
        public DateTime? SurrenderIncreaseDate { get; set;}
        
        [FirestoreProperty("surrenderYear")]
        public double? SurrenderYear { get; set;}

        // indexed
        [FirestoreProperty("tickerSymbol")]
        public string? TickerSymbol { get; set;}

        [FirestoreProperty("rebalanceFixedAllocation")]
        public double? RebalanceFixedAllocation { get; set;}
        
        [FirestoreProperty("rebalanceFixedRate")]
        public double? RebalanceFixedRate { get; set;}

        [FirestoreProperty("rebalanceIndexAllocation")]
        public double? RebalanceIndexAllocation { get; set;}

        [FirestoreProperty("minimumGuaranteedRate2")]
        public double? MinimumGuaranteedRate2 { get; set;}

        [FirestoreProperty("minimumInitialGuaranteedPercent1")]
        public double? MinimumInitialGuaranteedPercent1 { get; set;}

        [FirestoreProperty("minimumInitialGuaranteedPercent2")]
        public double? MinimumInitialGuaranteedPercent2 { get; set;}
        
        [FirestoreProperty("gmirCode1")]
        public string? GmirCode1 { get; set;}
        
        [FirestoreProperty("gmirCode2")]
        public string? GmirCode2 { get; set;}
        
        [FirestoreProperty("gmirStateAvailability1")]
        public string? GmirStateAvailability1 { get; set;}

        [FirestoreProperty("gmirStateAvailability2")]
        public string? GmirStateAvailability2 { get; set;}
    }
