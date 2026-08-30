using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class AnnuityBaseRate
{
    public virtual string DocId { get; set; } = string.Empty;
    
    [FirestoreProperty("typeId")]
    public string TypeId => "annuity";

    [FirestoreProperty("createdBy")]
    public string CreatedBy => "BeaconRateIngestion";

    [FirestoreProperty("createdOn")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("modifiedBy")]
    public string ModifiedBy => "BeaconRateIngestion";

    [FirestoreProperty("modifiedOn")]
    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("isActive")]
    public bool IsActive => true;

    [FirestoreProperty("isVisible")]
    public bool IsVisible => false;

    [FirestoreProperty("ddwGroupId")]
    public string? DdwGroupId { get; set; }
}