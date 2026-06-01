using EZ.Job.Core;
using EZJob.Store.PostgreSQL;

namespace EZ.Job.Store.PostgreSQL.Tests;

public sealed class PostgreSqlJobStoreTests
{
    private const string ConnectionString = "Host=localhost;Database=ez_jobs_test;Username=postgres;Password=postgres";

    [Fact(Skip = "Requires PostgreSQL container")]
    public async Task AddAsync_should_store_job()
    {
        var store = new PostgreSqlJobStore(ConnectionString);
        var job = new Job("test-id", "T", "M", [], [], JobStatus.Enqueued, DateTime.UtcNow, null);

        await store.AddAsync(job);
        var result = await store.GetAsync("test-id");

        Assert.NotNull(result);
        Assert.Equal("test-id", result!.Id);
    }
}
