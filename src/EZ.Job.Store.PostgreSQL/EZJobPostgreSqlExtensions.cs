using EZ.Job.Core;
using EZJob.Store.PostgreSQL;

namespace Microsoft.Extensions.DependencyInjection;

public static class EZJobPostgreSqlExtensions
{
    public static IEZJobBuilder AddPostgreSqlStore(this IEZJobBuilder builder, string connectionString)
    {
        return AddPostgreSqlStore(builder, o => o.ConnectionString = connectionString);
    }

    public static IEZJobBuilder AddPostgreSqlStore(this IEZJobBuilder builder, Action<PostgreSqlStoreOptions> configure)
    {
        var options = new PostgreSqlStoreOptions();
        configure(options);

        builder.Services.AddSingleton<IJobStore>(_ => new PostgreSqlJobStore(options.ConnectionString));
        builder.Services.AddSingleton<IRecurringStore>(_ => new PostgreSqlRecurringStore(options.ConnectionString));

        return builder;
    }
}
