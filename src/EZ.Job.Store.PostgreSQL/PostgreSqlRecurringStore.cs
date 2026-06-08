using System.Text.Json;
using EZ.Job.Core;
using Npgsql;

namespace EZJob.Store.PostgreSQL;

public sealed class PostgreSqlRecurringStore : IRecurringStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _connectionString;

    public PostgreSqlRecurringStore(string connectionString)
    {
        _connectionString = connectionString;
        EnsureTableAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureTableAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync().ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("""
            CREATE TABLE IF NOT EXISTS ez_recurring_definitions (
                id                  TEXT PRIMARY KEY,
                type_name           TEXT NOT NULL,
                method_name         TEXT NOT NULL,
                argument_types      TEXT NOT NULL,
                arguments           TEXT NOT NULL,
                cron_expression     TEXT NOT NULL,
                is_active           BOOLEAN NOT NULL DEFAULT TRUE,
                created_at_utc      TIMESTAMPTZ NOT NULL,
                last_execution_utc  TIMESTAMPTZ
            )
            """, conn);

        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async ValueTask AddOrUpdateAsync(RecurringDefinition definition, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("""
            INSERT INTO ez_recurring_definitions (id, type_name, method_name, argument_types, arguments, cron_expression, is_active, created_at_utc, last_execution_utc)
            VALUES (@id, @type_name, @method_name, @argument_types, @arguments, @cron_expression, @is_active, @created_at_utc, @last_execution_utc)
            ON CONFLICT (id) DO UPDATE SET
                type_name = EXCLUDED.type_name,
                method_name = EXCLUDED.method_name,
                argument_types = EXCLUDED.argument_types,
                arguments = EXCLUDED.arguments,
                cron_expression = EXCLUDED.cron_expression,
                is_active = EXCLUDED.is_active,
                last_execution_utc = EXCLUDED.last_execution_utc
            """, conn);

        AddParameters(cmd, definition);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("DELETE FROM ez_recurring_definitions WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id.ToString());

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<RecurringDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("SELECT * FROM ez_recurring_definitions WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id.ToString());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return ReadDefinition(reader);
        }

        return null;
    }

    public async ValueTask<IEnumerable<RecurringDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var definitions = new List<RecurringDefinition>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("SELECT * FROM ez_recurring_definitions ORDER BY created_at_utc", conn);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            definitions.Add(ReadDefinition(reader));
        }

        return definitions;
    }

    public async ValueTask SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = new NpgsqlCommand("UPDATE ez_recurring_definitions SET is_active = @is_active WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@is_active", isActive);
        cmd.Parameters.AddWithValue("@id", id.ToString());

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddParameters(NpgsqlCommand cmd, RecurringDefinition def)
    {
        cmd.Parameters.AddWithValue("@id", def.Id.ToString());
        cmd.Parameters.AddWithValue("@type_name", def.TypeName);
        cmd.Parameters.AddWithValue("@method_name", def.MethodName);
        cmd.Parameters.AddWithValue("@argument_types", JsonSerializer.Serialize(def.ArgumentTypes, JsonOptions));
        cmd.Parameters.AddWithValue("@arguments", JsonSerializer.Serialize(def.Arguments, JsonOptions));
        cmd.Parameters.AddWithValue("@cron_expression", def.CronExpression);
        cmd.Parameters.AddWithValue("@is_active", def.IsActive);
        cmd.Parameters.AddWithValue("@created_at_utc", def.CreatedAtUtc);
        cmd.Parameters.AddWithValue("@last_execution_utc", (object?)def.LastExecutionUtc ?? DBNull.Value);
    }

    private static RecurringDefinition ReadDefinition(NpgsqlDataReader reader)
    {
        return new RecurringDefinition(
            Id: Guid.Parse(reader.GetString(0)),
            TypeName: reader.GetString(1),
            MethodName: reader.GetString(2),
            ArgumentTypes: JsonSerializer.Deserialize<string[]>(reader.GetString(3), JsonOptions) ?? [],
            Arguments: JsonSerializer.Deserialize<object?[]>(reader.GetString(4), JsonOptions) ?? [],
            CronExpression: reader.GetString(5),
            IsActive: reader.GetBoolean(6),
            CreatedAtUtc: reader.GetDateTime(7),
            LastExecutionUtc: reader.IsDBNull(8) ? null : reader.GetDateTime(8));
    }
}
