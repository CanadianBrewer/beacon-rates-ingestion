using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class Product
{
    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    [FirestoreProperty("isClosed")]
    public bool IsClosed { get; set; }

    [FirestoreProperty("isInScorecard")]
    public bool IsInScorecard { get; set; }

    [FirestoreProperty("isProprietary")]
    public bool IsProprietary { get; set; }
    
    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("_organizationTokens")]
    public List<string> OrganizationTokens { get; set; } = [];
}

[FirestoreData]
public class Company
{
    [FirestoreDocumentId]
    private string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("companyId")]
    public string CompanyId => Id;

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    [FirestoreProperty("isInScorecard")]
    public bool IsInScorecard { get; set; }

    [FirestoreProperty("isProprietary")]
    public bool IsProprietary { get; set; }
    
    [FirestoreProperty("isVisible")]
    public bool IsVisible { get; set; }

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("ratings")]
    public Ratings Ratings { get; set; } = new();
}

[FirestoreData]
public class Ratings
{
    [FirestoreProperty("amBest")]
    public string? AmGrade { get; set; }

    [FirestoreProperty("fitch")]
    public string? Fitch { get; set; }

    [FirestoreProperty("moodys")]
    public string? Moodys { get; set; }

    [FirestoreProperty("sp")]
    public string? SpGrade { get; set; }
}