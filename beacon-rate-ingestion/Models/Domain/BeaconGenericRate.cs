using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

public class BeaconGenericRate
{
    // fixed
    [JsonPropertyName("bailoutrate")]
    public double? BailoutRate { get; set; }

    // fixed, indexed
    [JsonPropertyName("beginDate")] // indexed
    [JsonAlias("rateBegindate")] // fixed
    public string? BeginDate { get; set; } // "2016-04-20T00:00:00"

    // fixed
    [JsonPropertyName("bonusLen")]
    public int? BonusLen { get; set; }

    // fixed
    [JsonPropertyName("bonusPct")]
    public double? BonusPercent { get; set; }

    // fixed
    [JsonPropertyName("bonusType")]
    public string? BonusType { get; set; }

    // rila
    [JsonPropertyName("buffer")]
    public double? Buffer { get; set; }

    // indexed, rila
    [JsonPropertyName("capBail")]
    public double? CapBail { get; set; }

    // indexed, rila
    [JsonPropertyName("capMinimum")]
    public double? CapMinimum { get; set; }

    // indexed, rila
    [JsonPropertyName("capRate")]
    public double? CapRate { get; set; }

    // indexed, rila
    [JsonPropertyName("creditStrategy")]
    public string? CreditStrategy { get; set; }

    // indexed
    [JsonPropertyName("strategyName")]
    public string? StrategyName  { get; set; }
    
    // indexed, rila
    [JsonPropertyName("creditingFrequency")]
    public string? CreditingFrequency { get; set; }

    // indexed, rila
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // rila
    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; } // "04/01/2026"

    // fixed, indexed
    [JsonPropertyName("endDate")] // indexed
    [JsonAlias("rateEnddate")] // fixed
    public string? EndDate { get; set; } // "2026-09-04T00:00:00"

    // indexed, rila
    [JsonPropertyName("fixedRate")]
    public double? FixedRate { get; set; }

    // rila
    [JsonPropertyName("floor")]
    public double? Floor { get; set; }

    // indexed
    [JsonPropertyName("gmirCode1")]
    public string? GmirCode1 { get; set; }

    // indexed
    [JsonPropertyName("gmirCode2")]
    public string? GmirCode2 { get; set; }

    // indexed
    [JsonPropertyName("gmirstateAvailability1")]
    public string? GmirStateAvailability1 { get; set; }

    // indexed
    [JsonPropertyName("gmirstateAvailability2")]
    public string? GmirStateAvailability2 { get; set; }

    // indexed, rila
    [JsonPropertyName("index")]
    public string? Index { get; set; }

    // fixed
    [JsonPropertyName("initRate")]
    public double? InitialRate { get; set; }

    // fixed
    [JsonPropertyName("intType")]
    public string? InterestType { get; set; }

    // fixed
    [JsonPropertyName("intrateter")]
    public int Intrateter { get; set; }

    // indexed, rila
    [JsonPropertyName("minFixedRate")]
    public double? MinimumFixedRate { get; set; }

    // fixed
    [JsonPropertyName("minGrate")]
    public double? MinimumGuaranteedRate { get; set; }

    // fixed
    [JsonPropertyName("mineffrate")]
    public double? MinimumEffectiveRate { get; set; }

    // indexed, rila
    /// <summary>
    /// This is similar to MinimumCon below but it comes as an integer from Beacon
    /// </summary>
    [JsonPropertyName("minimum")]
    public int? Minimum { get; set; }

    // fixed
    /// <summary>
    /// This is similar to Minimum above but it comes as a double from Beacon
    /// </summary>
    [JsonPropertyName("minimumCon")]
    public double? MinimumContribution { get; set; }

    // indexed
    [JsonPropertyName("minimumGuaranteedRate1")]
    public double? MinimumGuaranteedRate1 { get; set; }

    // indexed
    [JsonPropertyName("minimumGuaranteedRate2")]
    public double? MinimumGuaranteedRate2 { get; set; }

    // indexed
    [JsonPropertyName("minimumInitialGuaranteePercent1")]
    public double? MinimumInitialGuaranteePercent1 { get; set; }

    // indexed
    [JsonPropertyName("minimumInitialGuaranteePercent2")]
    public double? MinimumInitialGuaranteePercent2 { get; set; }

    // indexed, rila
    [JsonPropertyName("mktgname")] // indexed
    [JsonAlias("marketingName")] // rila
    public string? MarketingName { get; set; }

    // fixed
    [JsonPropertyName("mva")]
    public bool? Mva { get; set; }

    // fixed, indexed, rila 
    [JsonPropertyName("overallStateAvailability")] // indexed, rila
    [JsonAlias("mgirStates")] // fixed
    public string? OverallStateAvailability { get; set; }

    // indexed, rila
    [JsonPropertyName("particaptionRateBailout")]
    public double? ParticipationRateBailout { get; set; }

    // indexed, rila
    [JsonPropertyName("particaptionRateMinimum")]
    public double? ParticipationRateMinimum { get; set; }

    // indexed, rila
    [JsonPropertyName("participationRate")]
    public double? ParticipationRate { get; set; }

    // indexed, rila
    [JsonPropertyName("performanceTrigger")]
    public double? PerformanceTrigger { get; set; }

    // indexed, rila
    [JsonPropertyName("performanceTriggerMinimumCredit")]
    public double? PerformanceTriggerMinimumCredit { get; set; }

    // indexed, rila
    [JsonPropertyName("performanceTriggerRate")]
    public double? PerformanceTriggerRate { get; set; }

    // fixed
    [JsonPropertyName("prodType")]
    public string? ProductType { get; set; }

    // fixed, indexed, rila
    [JsonPropertyName("productId")] // indexed, rila
    [JsonAlias("productid")] // fixed
    public double ProductId { get; set; }

    // fixed
    [JsonPropertyName("qualifier")]
    public string? Qualifier { get; set; }

    // indexed
    [JsonPropertyName("rebalanceFixedAllocation")]
    public double? RebalanceFixedAllocation { get; set; }

    // indexed
    [JsonPropertyName("rebalanceFixedRate")]
    public double? RebalanceFixedRate { get; set; }

    // indexed
    [JsonPropertyName("rebalanceIndexAllocation")]
    public double? RebalanceIndexAllocation { get; set; }

    // fixed, indexed
    [JsonPropertyName("rop")]
    public bool? Rop { get; set; }

    // indexed, rila
    [JsonPropertyName("spreadMaximum")]
    public double? SpreadMaximum { get; set; }

    // indexed, rila
    [JsonPropertyName("spreadRate")]
    public double? SpreadRate { get; set; }

    // rila
    [JsonPropertyName("states")]
    public string? States { get; set; }

    // fixed
    [JsonPropertyName("surrExpdate")]
    public string? SurrenderExpirationDate { get; set; } // "2020-06-08T00:00:00"

    // fixed
    [JsonPropertyName("surrId")]
    public int? SurrenderId { get; set; }

    // fixed
    [JsonPropertyName("surrIncDate")]
    public string? SurrenderIncreaseDate { get; set; } // "2020-06-08T00:00:00"

    // fixed
    [JsonPropertyName("surryr")]
    public double? SurrenderYear { get; set; }

    // fixed
    [JsonPropertyName("termBeginDate")]
    public string? TermBeginDate { get; set; } // "2020-06-08T00:00:00"

    // fixed
    [JsonPropertyName("termEndDate")]
    public string? TermEndDate { get; set; } // likely "2020-06-08T00:00:00"

    // indexed, rila
    [JsonPropertyName("tickerSymbol")]
    public string? TickerSymbol { get; set; }

    // fixed, indexed, rila
    [JsonPropertyName("urn")] // indexed, rila
    [JsonAlias("urnCode")] // fixed
    public string Urn { get; set; } = "unknown-urn";
    
    // fixed; needed for grouping to set DdwGroupId
    [JsonPropertyName("varid")] 
    public int VarId { get; set; }
}


[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class JsonAliasAttribute : Attribute
{
    public string Name { get; }
    public JsonAliasAttribute(string name) => Name = name;
}
