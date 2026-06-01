using EZ.Job.Core;
using EZJob.Store.PostgreSQL;

namespace Microsoft.Extensions.DependencyInjection;

public static class EZJobPostgreSqlExtensions
{
    public static EZJobBuilder AddPostgreSqlStore(this EZJobBuilder builder, string connectionString)
    {
        return AddPostgreSqlStore(builder, o => o.ConnectionString = connectionString);
    }

    public static EZJobBuilder AddPostgreSqlStore(this EZJobBuilder builder, Action<PostgreSqlStoreOptions> configure)
    {
        var options = new PostgreSqlStoreOptions();
        configure(options);

        builder.Services.AddSingleton<IJobStore>(_ => new PostgreSqlJobStore(options.ConnectionString));

        return builder;
    }
}
