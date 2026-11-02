using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class Annuity
{
    [FirestoreDocumentId]
    public required string Id { get; set; }

    [FirestoreProperty("companyId")]
    public string CompanyId { get; set; } = string.Empty;
    
    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    [FirestoreProperty("isClosed")]
    public bool? IsClosed { get; set; }

    [FirestoreProperty("isInScorecard")]
    public bool? IsInScorecard { get; set; }

    [FirestoreProperty("isInScoring")]
    public bool? IsInScoring { get; set; }
    
    [FirestoreProperty("isProprietary")]
    public bool IsProprietary { get; set; }

    [FirestoreProperty("name")]
    public string Name { get; set; } = "unknown-name";
    
    [FirestoreProperty("_organizationTokens")]
    public List<string> OrganizationTokens { get; set; } = [];

    [FirestoreProperty("ages")]
    public List<FixedAge> Ages { get; set; } = [];
    
    [FirestoreProperty("surrenders")]
    public List<Surrender> Surrenders { get; set; } = [];
}