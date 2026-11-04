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
    
    [FirestoreProperty("eligibility")]
    public Eligibility? Eligibility { get; set; }
}

[FirestoreData]
public class Eligibility
{
    [FirestoreProperty("ages")]
    public AgeBase? Ages { get; set; }
}

[FirestoreData]
public class FixedAge : AgeBase
{
    [FirestoreProperty("maximumAZQ")]
    public double? MaximumAZQ { get; set; }

    [FirestoreProperty("maximumAZNQ")]
    public double? MaximumAZNQ { get; set; }

    [FirestoreProperty("descriptionMINAQ")]
    public string? DescriptionMINAQ { get; set; }

    [FirestoreProperty("descriptionMXAQ")]
    public string? DescriptionMXAQ { get; set; }

    [FirestoreProperty("descriptionMINANQ")]
    public string? DescriptionMINANQ { get; set; }

    [FirestoreProperty("descriptionMXANQ")]
    public string? DescriptionMXANQ { get; set; }

    [FirestoreProperty("descriptionMXAZQ")]
    public string? DescriptionMXAZQ { get; set; }

    [FirestoreProperty("descriptionMXAZNQ")]
    public string? DescriptionMXAZNQ { get; set; }

    [FirestoreProperty("descriptionMINOQ")]
    public string? DescriptionMINOQ { get; set; }

    [FirestoreProperty("descriptionMXOQ")]
    public string? DescriptionMXOQ { get; set; }

    [FirestoreProperty("descriptionMINONQ")]
    public string? DescriptionMINONQ { get; set; }

    [FirestoreProperty("descriptionMXONQ")]
    public string? DescriptionMXONQ { get; set; }
}

[FirestoreData]
public class AgeBase
{
    [FirestoreProperty("minimumAQ")]
    public double? MinimumAQ { get; set; }

    [FirestoreProperty("maximumAQ")]
    public double? MaximumAQ { get; set; }

    [FirestoreProperty("minimumANQ")]
    public double? MinimumANQ { get; set; }

    [FirestoreProperty("maximumANQ")]
    public double? MaximumANQ { get; set; }

    [FirestoreProperty("minimumOQ")]
    public double? MinimumOQ { get; set; }

    [FirestoreProperty("maximumOQ")]
    public double? MaximumOQ { get; set; }

    [FirestoreProperty("minimumONQ")]
    public double? MinimumONQ { get; set; }

    [FirestoreProperty("maximumONQ")]
    public double? MaximumONQ { get; set; }
}

[FirestoreData]
public class Surrender
{
    [FirestoreProperty("scheduleId")]
    public long ScheduleId { get; set; }
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