using System.Text.Json.Serialization;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

public class BeaconFixedRate
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("beginDate")]
    public DateTime BeginDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("minimum")]
    public double? Minimum { get; set; }

    [JsonPropertyName("maximum")]
    public double? Maximum { get; set; }

    [JsonPropertyName("overallStateAvailability")]
    public List<string>? OverallStateAvailability { get; set; }

    [JsonPropertyName("intrateter")]
    public double? Intrateter { get; set; }

    [JsonPropertyName("termBeginDate")]
    public DateTime? TermBeginDate { get; set; }

    [JsonPropertyName("termEndDate")]
    public DateTime? TermEndDate { get; set; }

    [JsonPropertyName("initRate")]
    public double? InitRate { get; set; }

    [JsonPropertyName("minEffRate")]
    public double? MinEffRate { get; set; }

    [JsonPropertyName("minGrate")]
    public double? MinGrate { get; set; }

    [JsonPropertyName("bonusPct")]
    public double? BonusPct { get; set; }

    [JsonPropertyName("bonusLen")]
    public double? BonusLen { get; set; }

    [JsonPropertyName("bonusType")]
    public string? BonusType { get; set; }

    [JsonPropertyName("prodType")]
    public string? ProdType { get; set; }

    [JsonPropertyName("intType")]
    public string? IntType { get; set; }

    [JsonPropertyName("mva")]
    public bool? Mva { get; set; }

    [JsonPropertyName("rop")]
    public bool? Rop { get; set; }

    [JsonPropertyName("qualifier")]
    public string? Qualifier { get; set; }

    [JsonPropertyName("bailOutRate")]
    public double? BailOutRate { get; set; }

    [JsonPropertyName("surrExpDate")]
    public DateTime? SurrExpDate { get; set; }

    [JsonPropertyName("surrId")]
    public int? SurrId { get; set; }

    [JsonPropertyName("surrIncDate")]
    public DateTime? SurrIncDate { get; set; }

    [JsonPropertyName("surrYr")]
    public double? SurrYr { get; set; }
    
    [JsonPropertyName("urn")]
    public string Urn { get; set; }
}
