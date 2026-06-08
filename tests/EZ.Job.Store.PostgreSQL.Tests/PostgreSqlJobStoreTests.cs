using Xunit;
using EZJob.Store.PostgreSQL;
using Xunit;

namespace EZ.Job.Store.PostgreSQL.Tests;

public sealed class PostgreSqlJobStoreTests
{
    private const string ConnectionString = "Host=localhost;Database=ez_jobs_test;Username=postgres;Password=postgres";

    [Fact(Skip = "Requires PostgreSQL container")]
    public async Task AddAsync_should_store_job()
    {
        var store = new PostgreSqlJobStore(ConnectionString);
        var job = new EZ.Job.Core.Job("test-id", "T", "M", [], [], EZ.Job.Core.JobStatus.Enqueued, System.DateTime.UtcNow, null, null, null, null);

        await store.AddAsync(job);
        var result = await store.GetAsync("test-id");

        Assert.NotNull(result);
        Assert.Equal("test-id", result!.Id);
    }
}
