using System.Data;
using System.Text.Json;
using DolapTakipSistemi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DolapTakipSistemi.Api.Controllers;

[ApiController]
[Route("api/admin/database")]
public class AdminDatabaseController : ControllerBase
{
    private const string AdminHeaderName = "X-Admin-Password";
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AdminDatabaseController(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpGet("tables")]
    public async Task<ActionResult<IReadOnlyList<DatabaseTableResponse>>> Tables(CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var tables = new List<DatabaseTableResponse>();
        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.name AS SchemaName, t.name AS TableName
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE t.is_ms_shipped = 0
            ORDER BY s.name, t.name
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add(new DatabaseTableResponse(reader.GetString(0), reader.GetString(1)));
        }

        return Ok(tables);
    }

    [HttpGet("tables/{schema}/{table}/columns")]
    public async Task<ActionResult<IReadOnlyList<DatabaseColumnResponse>>> Columns(
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!await TableExistsAsync(schema, table, cancellationToken))
        {
            return NotFound();
        }

        return Ok(await GetColumnsAsync(schema, table, cancellationToken));
    }

    [HttpGet("tables/{schema}/{table}/rows")]
    public async Task<ActionResult<DatabaseRowsResponse>> Rows(
        string schema,
        string table,
        CancellationToken cancellationToken,
        int take = 100)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!await TableExistsAsync(schema, table, cancellationToken))
        {
            return NotFound();
        }

        take = Math.Clamp(take, 1, 500);
        var columns = await GetColumnsAsync(schema, table, cancellationToken);
        var rows = new List<Dictionary<string, object?>>();
        var columnNames = columns.Select(column => Quote(column.Name)).ToList();
        var primaryKey = columns.FirstOrDefault(column => column.IsPrimaryKey);
        var orderBy = primaryKey is null ? string.Empty : $" ORDER BY {Quote(primaryKey.Name)}";

        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT TOP (@take) {string.Join(", ", columnNames)} FROM {Quote(schema)}.{Quote(table)}{orderBy}";
        AddParameter(command, "@take", take);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new Dictionary<string, object?>();

            foreach (var column in columns)
            {
                var value = reader[column.Name];
                row[column.Name] = value == DBNull.Value ? null : value;
            }

            rows.Add(row);
        }

        return Ok(new DatabaseRowsResponse(columns, rows));
    }

    [HttpPost("tables/{schema}/{table}/rows")]
    public async Task<ActionResult> InsertRow(
        string schema,
        string table,
        Dictionary<string, JsonElement> values,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!await TableExistsAsync(schema, table, cancellationToken))
        {
            return NotFound();
        }

        var columns = await GetColumnsAsync(schema, table, cancellationToken);
        var writableColumns = columns.Where(column => !column.IsIdentity).ToDictionary(column => column.Name);
        var selectedColumns = values.Keys.Where(writableColumns.ContainsKey).ToList();

        if (selectedColumns.Count == 0)
        {
            return BadRequest("Kaydedilecek kolon bulunamadi.");
        }

        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        var parameterNames = new List<string>();

        for (var index = 0; index < selectedColumns.Count; index++)
        {
            var parameterName = $"@p{index}";
            parameterNames.Add(parameterName);
            AddParameter(command, parameterName, JsonToValue(values[selectedColumns[index]]));
        }

        command.CommandText =
            $"INSERT INTO {Quote(schema)}.{Quote(table)} ({string.Join(", ", selectedColumns.Select(Quote))}) VALUES ({string.Join(", ", parameterNames)})";

        await command.ExecuteNonQueryAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("tables/{schema}/{table}/rows/{key}")]
    public async Task<ActionResult> UpdateRow(
        string schema,
        string table,
        string key,
        Dictionary<string, JsonElement> values,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!await TableExistsAsync(schema, table, cancellationToken))
        {
            return NotFound();
        }

        var columns = await GetColumnsAsync(schema, table, cancellationToken);
        var primaryKey = columns.FirstOrDefault(column => column.IsPrimaryKey);

        if (primaryKey is null)
        {
            return BadRequest("Bu tabloda tekil primary key bulunamadi.");
        }

        var writableColumns = columns
            .Where(column => !column.IsIdentity && !column.IsPrimaryKey)
            .ToDictionary(column => column.Name);
        var selectedColumns = values.Keys.Where(writableColumns.ContainsKey).ToList();

        if (selectedColumns.Count == 0)
        {
            return BadRequest("Guncellenecek kolon bulunamadi.");
        }

        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        var assignments = new List<string>();

        for (var index = 0; index < selectedColumns.Count; index++)
        {
            var parameterName = $"@p{index}";
            assignments.Add($"{Quote(selectedColumns[index])} = {parameterName}");
            AddParameter(command, parameterName, JsonToValue(values[selectedColumns[index]]));
        }

        AddParameter(command, "@key", ConvertKey(key, primaryKey));
        command.CommandText =
            $"UPDATE {Quote(schema)}.{Quote(table)} SET {string.Join(", ", assignments)} WHERE {Quote(primaryKey.Name)} = @key";

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("tables/{schema}/{table}/rows/{key}")]
    public async Task<ActionResult> DeleteRow(
        string schema,
        string table,
        string key,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        if (!await TableExistsAsync(schema, table, cancellationToken))
        {
            return NotFound();
        }

        var columns = await GetColumnsAsync(schema, table, cancellationToken);
        var primaryKey = columns.FirstOrDefault(column => column.IsPrimaryKey);

        if (primaryKey is null)
        {
            return BadRequest("Bu tabloda tekil primary key bulunamadi.");
        }

        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {Quote(schema)}.{Quote(table)} WHERE {Quote(primaryKey.Name)} = @key";
        AddParameter(command, "@key", ConvertKey(key, primaryKey));

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected == 0 ? NotFound() : NoContent();
    }

    private bool IsAuthorized()
    {
        var expectedPassword = _configuration["AdminSettings:Password"];

        if (string.IsNullOrWhiteSpace(expectedPassword))
        {
            return false;
        }

        return Request.Headers.TryGetValue(AdminHeaderName, out var actualPassword)
            && actualPassword == expectedPassword;
    }

    private async Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(1)
            FROM sys.tables t
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = @schema AND t.name = @table AND t.is_ms_shipped = 0
            """;
        AddParameter(command, "@schema", schema);
        AddParameter(command, "@table", table);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result) > 0;
    }

    private async Task<IReadOnlyList<DatabaseColumnResponse>> GetColumnsAsync(
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        var columns = new List<DatabaseColumnResponse>();
        var connection = _dbContext.Database.GetDbConnection();
        await OpenIfNeededAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                c.name AS ColumnName,
                ty.name AS DataType,
                c.max_length AS MaxLength,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                CAST(CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS bit) AS IsPrimaryKey
            FROM sys.columns c
            INNER JOIN sys.tables t ON t.object_id = c.object_id
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                WHERE i.is_primary_key = 1
            ) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
            WHERE s.name = @schema AND t.name = @table
            ORDER BY c.column_id
            """;
        AddParameter(command, "@schema", schema);
        AddParameter(command, "@table", table);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(new DatabaseColumnResponse(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt16(2),
                reader.GetBoolean(3),
                reader.GetBoolean(4),
                reader.GetBoolean(5)));
        }

        return columns;
    }

    private static async Task OpenIfNeededAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open)
        {
            if (connection is System.Data.Common.DbConnection dbConnection)
            {
                await dbConnection.OpenAsync(cancellationToken);
                return;
            }

            connection.Open();
        }
    }

    private static void AddParameter(IDbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string Quote(string identifier)
    {
        return $"[{identifier.Replace("]", "]]")}]";
    }

    private static object? JsonToValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.String => element.GetString(),
            _ => element.ToString()
        };
    }

    private static object ConvertKey(string key, DatabaseColumnResponse primaryKey)
    {
        return primaryKey.DataType switch
        {
            "int" => int.Parse(key),
            "bigint" => long.Parse(key),
            "smallint" => short.Parse(key),
            "uniqueidentifier" => Guid.Parse(key),
            _ => key
        };
    }
}

public record DatabaseTableResponse(string Schema, string Name);

public record DatabaseColumnResponse(
    string Name,
    string DataType,
    short MaxLength,
    bool IsNullable,
    bool IsIdentity,
    bool IsPrimaryKey);

public record DatabaseRowsResponse(
    IReadOnlyList<DatabaseColumnResponse> Columns,
    IReadOnlyList<Dictionary<string, object?>> Rows);
