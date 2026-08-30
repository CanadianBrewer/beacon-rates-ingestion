using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class FixedAnnuityRate : AnnuityBaseRate
{
    private DateTime? _beginDate;
    private DateTime? _bonusBeginDate;
    private DateTime? _bonusEndDate;
    private DateTime? _endDate;
    private DateTime? _expirationDate;
    private DateTime? _mgirBeginDate;
    private List<string> _mgirStates = [];
    private long? _minimum;
    private DateTime? _productCloseDate;
    private string _productId = string.Empty;
    private long? _scheduleId;
    private DateTime? _surrenderExpirationDate;
    private DateTime? _surrenderInceptionDate;
    private long? _surrenderYear;
    private DateTime? _termBeginDate;
    private DateTime? _termEndDate;

    [FirestoreDocumentId]
    public override string DocId
    {
        get => Urn;
        set => Urn = value;
    }

    [JsonPropertyName("bailoutrate")] // null,
    [FirestoreProperty("bailOutRate")]
    public double? BailOutRate { get; set; }

    [JsonPropertyName("bandDesc")] // "CA only; non-MVA",
    [FirestoreProperty("bandDesc")]
    public string? BandDescription { get; set; }

    [JsonPropertyName("bandid")] // 5460,
    [FirestoreProperty("bandId")]
    public int? BandId { get; set; }

    [JsonPropertyName("rateBegindate")] // "2019-10-10T00 //00 //00",
    [FirestoreProperty("beginDate")]
    public DateTime? BeginDate
    {
        get => _beginDate?.ToUniversalTime() ?? _beginDate;
        set => _beginDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("blockid")] // 233408,
    [FirestoreProperty("blockId")]
    public int? BlockId { get; set; }

    [JsonPropertyName("bonusBegindate")] // null,
    [FirestoreProperty("bonusBeginDate")]
    public DateTime? BonusBeginDate
    {
        get => _bonusBeginDate?.ToUniversalTime() ?? _bonusBeginDate;
        set => _bonusBeginDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("bonusEnddate")] // null,
    [FirestoreProperty("bonusEndDate")]
    public DateTime? BonusEndDate
    {
        get => _bonusEndDate?.ToUniversalTime() ?? _bonusEndDate;
        set => _bonusEndDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("bonusLen")] // null,
    [FirestoreProperty("bonusLen")]
    public int? BonusLength { get; set; }

    [JsonPropertyName("bonusPct")] // null,
    [FirestoreProperty("bonusPct")]
    public double? BonusPercent { get; set; }

    [JsonPropertyName("bonusType")] // null,
    [FirestoreProperty("bonusType")]
    public string? BonusType { get; set; }

    [JsonPropertyName("companyName")] // "American Equity Investment Life Insurance Company",
    [FirestoreProperty("companyName")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("rateEnddate")] // null,
    [FirestoreProperty("endDate")]
    public DateTime? EndDate
    {
        get => _endDate?.ToUniversalTime() ?? _endDate;
        set => _endDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("initRate")] // 1.8,
    [FirestoreProperty("initRate")]
    public double? InitialRate { get; set; }

    [JsonPropertyName("intrateter")] // 5,
    [FirestoreProperty("intrateter")]
    public int? Intrateter { get; set; }

    [JsonPropertyName("intType")] // "MYGA",
    [FirestoreProperty("intType")]
    public string? IntType { get; set; }

    [FirestoreProperty("maximum")]
    public long? MaximumContribution { get; set; }

    [JsonPropertyName("mgirBegindate")] // "2020-01-01T00 //00 //00",
    [FirestoreProperty("mgirBeginDate")]
    public DateTime? MgirBeginDate
    {
        get => _expirationDate?.ToUniversalTime() ?? _expirationDate;
        set => _expirationDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("mgirEnddate")] // null,
    [FirestoreProperty("mgirEndDate")]
    public DateTime? MgirEndDate
    {
        get => _mgirBeginDate?.ToUniversalTime() ?? _mgirBeginDate;
        set => _mgirBeginDate = value?.ToUniversalTime();
    }

    [FirestoreProperty("mgirStateAvailability1")]
    public List<string> MgirStates
    {
        get
        {
            _mgirStates = RawMgirStates is null ? [] : [.. RawMgirStates.Split(',')];
            return _mgirStates;
        }
    }

    [JsonPropertyName("mineffrate")] // 1.8,
    [FirestoreProperty("minEffRate")]
    public double? MinimumEffectiveRate { get; set; }

    [JsonPropertyName("minGrate")] // 1.0,
    [FirestoreProperty("minGrate")]
    public double? MinimumGrate { get; set; }

    [FirestoreProperty("minimum")]
    public long? MinimumContribution
    {
        get
        {
            if (RawMinimum is null)
            {
                _minimum = null;
            }
            else
            {
                _ = long.TryParse(RawMinimum.Value.ToString(), out long parsed);
                _minimum = parsed;
            }

            return _minimum;
        }
    }

    [JsonPropertyName("mva")] // false,
    [FirestoreProperty("mva")]
    public bool? Mva { get; set; }

    [JsonPropertyName("prodType")] // "MYGA",
    [FirestoreProperty("prodType")]
    public string? ProductType { get; set; }

    [JsonPropertyName("productCloseDate")] // null,
    [FirestoreProperty("productCloseDate")]
    public DateTime? ProductCloseDate
    {
        get => _productCloseDate?.ToUniversalTime() ?? _productCloseDate;
        set => _productCloseDate = value?.ToUniversalTime();
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
            return $"fa_{productId}";
        }
        set => _productId = value;
    }

    [JsonPropertyName("productName")] // "Guarantee Series",
    [FirestoreProperty("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("qualifier")] // "5 Year",
    [FirestoreProperty("qualifier")]
    public string? Qualifier { get; set; }

    [JsonPropertyName("rop")] // null,
    [FirestoreProperty("rop")]
    public bool? Rop { get; set; }

    [JsonPropertyName("rowid")] // 8613,
    [FirestoreProperty("rowId")]
    public int? RowId { get; set; }

    [FirestoreProperty("schedId")]
    public long? ScheduleId
    {
        get
        {
            if (RawScheduleId is null)
            {
                _scheduleId = null;
            }
            else
            {
                _ = long.TryParse(RawScheduleId.Value.ToString(), out long parsed);
                _scheduleId = parsed;
            }

            return _scheduleId;
        }
    }

    [JsonPropertyName("surrExpdate")] // null,
    [FirestoreProperty("surrExpDate")]
    public DateTime? SurrenderExpirationDate
    {
        get => _surrenderExpirationDate?.ToUniversalTime() ?? _surrenderExpirationDate;
        set => _surrenderExpirationDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("surrId")] // 2050,
    [FirestoreProperty("surrId")]
    public int? SurrenderId { get; set; }

    [JsonPropertyName("surrIncDate")] // "2019-04-16T00 //00 //00",
    [FirestoreProperty("surrIncDate")]
    public DateTime? SurrenderInceptionDate
    {
        get => _surrenderInceptionDate?.ToUniversalTime() ?? _surrenderInceptionDate;
        set => _surrenderInceptionDate = value?.ToUniversalTime();
    }

    [FirestoreProperty("surrYr")]
    public long? SurrenderYear
    {
        get
        {
            if (RawSurrenderYear is null)
            {
                _surrenderYear = null;
            }
            else
            {
                _ = long.TryParse(RawSurrenderYear.Value.ToString(), out long parsed);
                _surrenderYear = parsed;
            }

            return _surrenderYear;
        }
    }

    [JsonPropertyName("termBeginDate")] // "2014-04-10T00 //00 //00",
    [FirestoreProperty("termBeginDate")]
    public DateTime? TermBeginDate
    {
        get => _termBeginDate?.ToUniversalTime() ?? _termBeginDate;
        set => _termBeginDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("termEndDate")] // null,
    [FirestoreProperty("termEndDate")]
    public DateTime? TermEndDate
    {
        get => _termEndDate?.ToUniversalTime() ?? _termEndDate;
        set => _termEndDate = value?.ToUniversalTime();
    }

    [JsonPropertyName("urnCode")] // "240-F0881-002",
    [FirestoreProperty("urn")]
    public string Urn { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("varid")] // 3047
    [FirestoreProperty("varId")]
    public int? VarId { get; set; }

    // fields below are for our use, not sourced from beacon
    [FirestoreProperty("categoryId")]
    public string CategoryId => "fixed";

    // fields below are for normalizing odd beacon data formats to what we want 
    [JsonPropertyName("minimumCon")] // 1000.0,
    public double? RawMinimum { get; set; }

    [JsonPropertyName("productid")] // 1080.0,
    public double RawProductId { get; set; }

    [JsonPropertyName("schedid")] // 1860.0,
    public double? RawScheduleId { get; set; }

    [JsonPropertyName("surryr")] // 1860.0,
    public double? RawSurrenderYear { get; set; }

    [JsonPropertyName("mgirStates")]
    public string? RawMgirStates { get; set; }
}