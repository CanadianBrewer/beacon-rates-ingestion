using System.Text.Json.Serialization;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

public class BeaconFixedRate
{
    [JsonPropertyName("bailoutrate")]
    public double? BailOutRate { get; set; }

    [JsonPropertyName("rateBegindate")]
    public DateTime BeginDate { get; set; }

    [JsonPropertyName("bonusLen")]
    public double? BonusLen { get; set; }

    [JsonPropertyName("bonusPct")]
    public double? BonusPct { get; set; }

    [JsonPropertyName("bonusType")]
    public string? BonusType { get; set; }

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("rateEnddate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("initRate")]
    public double? InitRate { get; set; }

    [JsonPropertyName("intType")]
    public string? IntType { get; set; }

    [JsonPropertyName("intrateter")]
    public double? Intrateter { get; set; }

    [JsonPropertyName("maximum")]
    public double? Maximum { get; set; }

    [JsonPropertyName("mineffrate")]
    public double? MinEffRate { get; set; }

    [JsonPropertyName("minGrate")]
    public double? MinGrate { get; set; }

    [JsonPropertyName("minimumCon")]
    public double? Minimum { get; set; }

    [JsonPropertyName("mva")]
    public bool? Mva { get; set; }

    [JsonPropertyName("overallStateAvailability")]
    public List<string>? OverallStateAvailability { get; set; }

    [JsonPropertyName("prodType")]
    public string? ProdType { get; set; }

    [JsonPropertyName("productid")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("qualifier")]
    public string? Qualifier { get; set; }

    [JsonPropertyName("rop")]
    public bool? Rop { get; set; }

    [JsonPropertyName("surrExpDate")]
    public DateTime? SurrExpDate { get; set; }

    [JsonPropertyName("surrId")]
    public int? SurrId { get; set; }

    [JsonPropertyName("surrIncDate")]
    public DateTime? SurrIncDate { get; set; }

    [JsonPropertyName("surrYr")]
    public double? SurrYr { get; set; }

    [JsonPropertyName("termBeginDate")]
    public DateTime? TermBeginDate { get; set; }

    [JsonPropertyName("termEndDate")]
    public DateTime? TermEndDate { get; set; }

    [JsonPropertyName("urnCode")]
    public string Urn { get; set; } = string.Empty;
    
    [JsonPropertyName("varId")]
    public int? VarId { get; set; }  
}
