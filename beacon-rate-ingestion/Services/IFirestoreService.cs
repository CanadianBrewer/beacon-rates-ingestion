using BeaconDataIngestion.Models.DataModels.DDW.Rates;

namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public interface IFirestoreService
{
    /// <summary>
    ///     Updates a document that tracks process activity.
    /// </summary>
    /// <param name="activityDate">The current date and time</param>
    /// <param name="activity">The name of the activity being tracked</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of
    ///     cancellation.
    /// </param>
    Task SetLastActivityDateAsync(string activityDate, string activity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every document in a collection and all documents in nested
    ///     subcollections. Descendants are deleted before their parent documents.
    /// </summary>
    /// <param name="collectionPath">The root collection to delete</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of
    ///     cancellation.
    /// </param>
    Task DeleteCollectionRecursivelyAsync(string collectionPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all the inactive products for a given category
    /// </summary>
    /// <param name="categoryId">fixed, indexed, or rila</param>
    /// <param name="cancellationToken"></param>
    /// <returns>All the inactive product ids</returns>
    Task<List<string>> GetInactiveProductIdsAsync(string categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all product-rate documents whose product ID matches any of the
    ///     supplied product IDs.
    /// </summary>
    /// <param name="productIds">Product IDs whose rate documents should be deleted.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DeleteRatesForProductsAsync(List<string> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all product-rate documents whose product ID matches the supplied ID.
    /// </summary>
    /// <param name="productId">The product ID whose rate documents should be deleted.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DeleteRatesForProductAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Writes product-rate documents in parallel using Firestore batches.
    /// </summary>
    /// <param name="rates">The rate objects to write.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task PersistRatesAsync<T>(List<T> rates, CancellationToken cancellationToken = default) where T : AnnuityBaseRate;

    /// <summary>
    ///     Update the annuity with the last updated on date.
    /// </summary>
    /// <param name="productId">The annuity product id, prefixed with one of: fa, ia, iva, or va</param>
    Task SetAnnuityRatesLastUpdatedOnAsync(string productId);

    /// <summary>
    ///     Get all the rates for a given annuity category
    /// </summary>
    /// <param name="categoryId">One of fixed, indexed, rila</param>
    /// <returns>A list of annuity rates</returns>
    Task<List<T>> GetAllAnnuitiesRatesAsync<T>(string categoryId);

    /// <summary>
    ///     Update the product-type table 'annuity' document with the current date
    /// </summary>
    Task UpdateRatesLastUpdatedOnAsync();
    
    /// <summary>
    ///     Test if the Firestore db is accessible
    /// </summary>
    Task<bool> CheckFirestoreConnectivityAsync(CancellationToken cancellationToken = default);
}