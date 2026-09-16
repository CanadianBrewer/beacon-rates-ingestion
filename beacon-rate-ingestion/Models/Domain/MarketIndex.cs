using System.Text.Json.Serialization;
using Google.Cloud.Firestore;

namespace BeaconDataIngestion.Models.DataModels.DDW.Rates;

[FirestoreData]
public class MarketIndex
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    [FirestoreProperty("keywords")]
    public List<string> Keywords { get; set; } = [];
    
    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [FirestoreProperty("needsReview")]
    public bool NeedsReview { get; set; }
    
    [FirestoreProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [FirestoreProperty("weight")]
    public int Weight { get; set; }

    [FirestoreProperty("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;
    
    [FirestoreProperty("createdOn")]
    public DateTime CreatedOn { get; set; }

    [FirestoreProperty("modifiedBy")]
    public string ModifiedBy { get; set; } = string.Empty;   

    [FirestoreProperty("modifiedOn")]
    public DateTime ModifiedOn { get; set; }
}