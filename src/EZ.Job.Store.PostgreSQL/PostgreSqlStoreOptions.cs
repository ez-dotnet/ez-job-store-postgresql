namespace EZJob.Store.PostgreSQL;

public class PostgreSqlStoreOptions
{
    public string ConnectionString { get; set; } = "Host=localhost;Database=ez_jobs;Username=postgres;Password=postgres";
}
