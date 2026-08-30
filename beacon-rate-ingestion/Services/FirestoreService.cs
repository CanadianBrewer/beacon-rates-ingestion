using System.Diagnostics;
using BeaconDataIngestion.Models.DataModels.DDW.Rates;
using DueDiligenceWorks.Beacon.RateIngestion.Models.Application;
using Google.Api.Gax;
using Google.Cloud.Firestore;
using Grpc.Core;

namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public class FirestoreService : IFirestoreService
{
    private const int _maximumBatchSize = 500;
    private readonly FirestoreDb _db;
    private readonly ILogger<FirestoreService> _logger;

    public FirestoreService(ILogger<FirestoreService> logger, FirestoreConfig config)
    {
        _logger = logger;
        var builder = new FirestoreDbBuilder { ProjectId = config.ProjectName };
        if (config.UseEmulator)
        {
            builder.EmulatorDetection = EmulatorDetection.EmulatorOrProduction;
            builder.ChannelCredentials = ChannelCredentials.Insecure;
        }

        _db = builder.Build();
    }

    /// <inheritdoc />
    public async Task SetLastActivityDateAsync(string activityDate, string activity, CancellationToken cancellationToken = default)
    {
        DocumentReference? docReference = _db.Collection("task-activity").Document("rateUpdate");
        Dictionary<string, string> lastRateUpdateActivity = new() { { "activity", activity }, { "activityDate", activityDate } };
        await docReference.SetAsync(lastRateUpdateActivity, SetOptions.MergeAll, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionRecursivelyAsync(string collectionPath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        long documentsDeleted = 0;

        await DeleteCollectionRecursivelyAsync(
            _db.Collection(collectionPath),
            _maximumBatchSize,
            () => Interlocked.Increment(ref documentsDeleted),
            cancellationToken);

        _logger.LogInformation(
            "Deleted {DocumentCount} documents recursively from {CollectionPath} in {ElapsedMilliseconds} ms",
            documentsDeleted,
            collectionPath,
            stopwatch.ElapsedMilliseconds);
    }

    /// <inheritdoc />
    public async Task<List<string>> GetInactiveProductIdsAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        List<string> retVal = [];
        QuerySnapshot? snapshot = await _db.Collection("annuities").WhereEqualTo("categoryId", categoryId).WhereEqualTo("isActive", false).GetSnapshotAsync(cancellationToken);
        foreach (DocumentSnapshot? doc in snapshot.Documents)
        {
            retVal.Add(doc.Id);
        }

        return retVal;
    }

    /// <inheritdoc />
    public Task DeleteRatesForProductAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        return DeleteRatesForProductsAsync([productId], cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteRatesForProductsAsync(
        List<string> productIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(productIds);

        const int maximumInQueryValues = 30;
        string[] distinctProductIds =
        [
            .. productIds
                .Where(productId => !string.IsNullOrWhiteSpace(productId))
                .Distinct(StringComparer.Ordinal)
        ];

        if (distinctProductIds.Length == 0)
        {
            return;
        }

        long documentsDeleted = 0;
        CollectionReference rates = _db.Collection("product-rate");
        using var commitLimiter = new SemaphoreSlim(8);

        await Parallel.ForEachAsync(
            distinctProductIds.Chunk(maximumInQueryValues),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = cancellationToken
            },
            async (productIdChunk, ct) =>
            {
                QuerySnapshot snapshot = await rates
                    .WhereIn("productId", productIdChunk)
                    .GetSnapshotAsync(ct);

                await Parallel.ForEachAsync(
                    snapshot.Documents.Chunk(_maximumBatchSize),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 8,
                        CancellationToken = ct
                    },
                    async (documentBatch, batchCancellationToken) =>
                    {
                        DocumentSnapshot[] documents = documentBatch.ToArray();
                        WriteBatch batch = _db.StartBatch();

                        foreach (DocumentSnapshot document in documents)
                        {
                            batch.Delete(document.Reference);
                        }

                        await commitLimiter.WaitAsync(batchCancellationToken);
                        try
                        {
                            await CommitBatchWithRetryAsync(batch, batchCancellationToken);
                            Interlocked.Add(ref documentsDeleted, documents.Length);
                        }
                        finally
                        {
                            commitLimiter.Release();
                        }
                    });
            });

        _logger.LogDebug(
            "Deleted {DocumentCount} product-rate documents matching {ProductCount} product IDs",
            documentsDeleted,
            distinctProductIds.Length);
    }

    /// <inheritdoc />
    public async Task PersistRatesAsync<T>(
        List<T> rates,
        CancellationToken cancellationToken = default)
        where T : AnnuityBaseRate
    {
        ArgumentNullException.ThrowIfNull(rates);

        if (rates.Count == 0)
        {
            return;
        }

        if (rates.Any(rate => rate is null))
        {
            throw new ArgumentException("The rate list cannot contain null values.", nameof(rates));
        }

        long documentsWritten = 0;
        CollectionReference collection = _db.Collection("product-rate");

        await Parallel.ForEachAsync(
            rates.Chunk(_maximumBatchSize),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = cancellationToken
            },
            async (rateBatch, ct) =>
            {
                WriteBatch batch = _db.StartBatch();

                foreach (T rate in rateBatch)
                {
                    batch.Set(collection.Document(rate.DocId), rate);
                }

                await CommitBatchWithRetryAsync(batch, ct);
                Interlocked.Add(ref documentsWritten, rateBatch.Length);
            });

        _logger.LogDebug(
            "Wrote {DocumentCount} documents to the product-rate collection",
            documentsWritten);
    }

    /// <inheritdoc />
    public async Task SetAnnuityRatesLastUpdatedOnAsync(string productId)
    {
        DocumentReference? docReference = _db.Collection("annuities").Document(productId);
        DocumentSnapshot? doc = await docReference.GetSnapshotAsync();

        if (doc.Exists)
        {
            Dictionary<string, object> updatedData = new() { { "ratesLastUpdatedOn", DateTime.UtcNow } };
            await docReference.UpdateAsync(updatedData);
        }
    }

    /// <inheritdoc />
    public async Task<List<T>> GetAllAnnuitiesRatesAsync<T>(string categoryId)
    {
        CollectionReference? snapshot = _db.Collection("product-rate");
        Query? query = snapshot.WhereEqualTo("categoryId", categoryId);
        QuerySnapshot? querySnapshot = await query.GetSnapshotAsync();

        List<T> annuitiesRates = [];
        foreach (DocumentSnapshot? doc in querySnapshot.Documents)
        {
            var annuityRate = doc.ConvertTo<T>();
            annuitiesRates.Add(annuityRate);
        }

        return annuitiesRates;
    }

    /// <inheritdoc />
    public async Task<bool> CheckFirestoreConnectivityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            DocumentReference? docReference = _db.Collection("task-activity").Document("rateUpdate");
            DocumentSnapshot docReferenceSnapshot = await docReference.GetSnapshotAsync(cancellationToken);
            return docReferenceSnapshot.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Firestore connectivity");
        }

        return false;
    }    
    
    public async Task UpdateRatesLastUpdatedOnAsync()
    {
        DocumentReference? docReference = _db.Collection("product-type").Document("annuity");
        DocumentSnapshot? doc = await docReference.GetSnapshotAsync();

        Dictionary<string, object> updatedData = new() { { "productRateLastUpdatedOn", DateTime.UtcNow } };
        await docReference.UpdateAsync(updatedData);
    }


    private async Task DeleteCollectionRecursivelyAsync(
        CollectionReference collection,
        int batchSize,
        Action documentDeleted,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            QuerySnapshot snapshot = await collection
                .Limit(batchSize)
                .GetSnapshotAsync(cancellationToken);

            if (snapshot.Documents.Count == 0)
            {
                return;
            }

            // Firestore leaves subcollections behind when a parent document is
            // deleted, so recursively remove every descendant first.
            await Parallel.ForEachAsync(
                snapshot.Documents,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 16,
                    CancellationToken = cancellationToken
                },
                async (document, ct) =>
                {
                    await foreach (CollectionReference subcollection in
                                   document.Reference.ListCollectionsAsync().WithCancellation(ct))
                    {
                        await DeleteCollectionRecursivelyAsync(
                            subcollection,
                            batchSize,
                            documentDeleted,
                            ct);
                    }
                });

            WriteBatch batch = _db.StartBatch();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                batch.Delete(document.Reference);
            }

            await CommitBatchWithRetryAsync(batch, cancellationToken);
            foreach (DocumentSnapshot _ in snapshot.Documents)
            {
                documentDeleted();
            }
        }
    }

    private static async Task CommitBatchWithRetryAsync(
        WriteBatch batch,
        CancellationToken cancellationToken,
        int maxRetries = 5,
        int initialDelayMilliseconds = 200)
    {
        var attempt = 0;
        int delay = initialDelayMilliseconds;

        while (true)
        {
            try
            {
                await batch.CommitAsync(cancellationToken);
                return;
            }
            catch (Exception) when (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                attempt++;
                int jitter = Random.Shared.Next(100, 200);
                await Task.Delay(delay + jitter, cancellationToken);
                delay *= 2;
            }
        }
    }
}