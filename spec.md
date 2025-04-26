# DBMCP

DBMCP 是一个 C#编写的 操作数据库的 MCP(Model Context Protocol).

# interface IDBMCP

此项目定义了一个通用接口 IDBMCP，用于规范 DB 行为。具体方法定义见 APIs

# pg_db

pg_db 是一个 Postgres 操作类，实现了 IDBMCP

# APIs

### db_mcp register <conn_str> ✅

Register a new Postgres/MSSQL connection pool. AI agents can use this id to query the database.

```shell
db_mcp register "postgres://postgres:postgres@localhost:5432/postgres"
123e4567-e89b-12d3-a456-426614174000
```

### db_mcp unregister <conn_id> ✅

Unregister a Postgres/MSSQL connection. The connection pool will be closed and the connection id can't be used again.

```shell
db_mcp unregister 123e4567-e89b-12d3-a456-426614174000
```

### db_mcp query <conn_id> <query_sql> ✅

Query the database with a SQL statement. It must be a valid "SELECT" statement. We will use sqlparser to parse the statement, validate it is a valid "SELECT" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp query 123e4567-e89b-12d3-a456-426614174000 "SELECT * FROM users"
```

### db_mcp insert <conn_id> <query_sql> ✅

Insert a new row into the database. It must be a valid "INSERT" statement. We will use sqlparser to parse the statement, validate it is a valid "INSERT" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp insert 123e4567-e89b-12d3-a456-426614174000 "INSERT INTO users (name, email) VALUES ('John Doe', 'john.doe@example.com')"
```

### db_mcp update <conn_id> <query_sql> ✅

Update a row in the database. It must be a valid "UPDATE" statement. We will use sqlparser to parse the statement, validate it is a valid "UPDATE" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

### db_mcp delete <conn_id> <query_sql> ✅

Delete a row in the database. It must be a valid "DELETE" statement. We will use sqlparser to parse the statement, validate it is a valid "DELETE" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp delete 123e4567-e89b-12d3-a456-426614174000 "DELETE FROM users WHERE id = 1"
```

### db_mcp describe <conn_id> <table_name> ✅

Describe a table in the database. We will generate the SQL statement and execute it against the database.

```shell
db_mcp describe 123e4567-e89b-12d3-a456-426614174000 "users"
```

### db_mcp create_table <conn_id> <create_sql> ✅

Create a new table in the database. It must be a valid "CREATE TABLE" statement. We will use sqlparser to parse the statement, validate it is a valid "CREATE TABLE" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp create_table 123e4567-e89b-12d3-a456-426614174000 "CREATE TABLE users (id SERIAL PRIMARY KEY, name VARCHAR(255), email VARCHAR(255))"
```

### db_mcp drop_table <conn_id> <table_name> ✅

Drop a table in the database. We will generate the SQL statement and execute it against the database.

```shell
db_mcp drop_table 123e4567-e89b-12d3-a456-426614174000 "users"
```

### db_mcp create_index <conn_id> <create_index_sql> ✅

Create an index on a table. It must be a valid "CREATE INDEX" statement. We will use sqlparser to parse the statement, validate it is a valid "CREATE INDEX" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp create_index 123e4567-e89b-12d3-a456-426614174000 "CREATE INDEX idx_users_name ON users (name)"
```

### db_mcp drop_index <conn_id> <index_name> ✅

Drop an index on a table. We will generate the SQL statement and execute it against the database.

### db_mcp list_tables <conn_id> <schema> ✅

List all tables in a given schema. If schema is not provided, it will use the current schema.

```shell
db_mcp list_tables 123e4567-e89b-12d3-a456-426614174000 "public"
```

### db_mcp create_type <conn_id> <create_type_sql> ✅

Create a new type in the database. It must be a valid "CREATE TYPE" statement. We will use sqlparser to parse the statement, validate it is a valid "CREATE TYPE" statement, and then generate the SQL statement again. The newly generated SQL statement will be executed against the database.

```shell
db_mcp create_type 123e4567-e89b-12d3-a456-426614174000 "CREATE TYPE users AS ENUM ('admin', 'user')"
```

### db_mcp create_schema <conn_id> <schema_name> ✅

Create a new schema in the database. We will generate the SQL statement and execute it against the database.

```shell
db_mcp create_schema 123e4567-e89b-12d3-a456-426614174000 "hr"
```

# Dependencies

## .net6.0

## ModelContextProtocol v0.1.0-preview.11

MCP C# SDK
NuGet preview version

The official C# SDK for the Model Context Protocol, enabling .NET applications, services, and libraries to implement and interact with MCP clients and servers. Please visit our API documentation for more details on available functionality.

[!NOTE] This project is in preview; breaking changes can be introduced without prior notice.

About MCP
The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information about MCP:

Official Documentation
Protocol Specification
GitHub Organization
Installation
To get started, install the package from NuGet

```shell
dotnet add package ModelContextProtocol --prerelease
```

Getting Started (Client)
To get started writing a client, the McpClientFactory.CreateAsync method is used to instantiate and connect an IMcpClient to a server. Once you have an IMcpClient, you can interact with it, such as to enumerate all available tools and invoke tools.

```c#
var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
Name = "Everything",
Command = "npx",
Arguments = ["-y", "@modelcontextprotocol/server-everything"],
});

var client = await McpClientFactory.CreateAsync(clientTransport);

// Print the list of tools available from the server.
foreach (var tool in await client.ListToolsAsync())
{
Console.WriteLine($"{tool.Name} ({tool.Description})");
}

// Execute a tool (this would normally be driven by LLM tool invocations).
var result = await client.CallToolAsync(
"echo",
new Dictionary<string, object?>() { ["message"] = "Hello MCP!" },
cancellationToken:CancellationToken.None);

// echo always returns one and only one text content object
Console.WriteLine(result.Content.First(c => c.Type == "text").Text);
```

You can find samples demonstrating how to use ModelContextProtocol with an LLM SDK in the samples directory, and also refer to the tests project for more examples. Additional examples and documentation will be added as in the near future.

Clients can connect to any MCP server, not just ones created using this library. The protocol is designed to be server-agnostic, so you can use this library to connect to any compliant server.

Tools can be easily exposed for immediate use by IChatClients, because McpClientTool inherits from AIFunction.

```c#
// Get available functions.
IList<McpClientTool> tools = await client.ListToolsAsync();

// Call the chat client using the tools.
IChatClient chatClient = ...;
var response = await chatClient.GetResponseAsync(
"your prompt here",
new() { Tools = [.. tools] },

```

Getting Started (Server)
Here is an example of how to create an MCP server and register all tools from the current application. It includes a simple echo tool as an example (this is included in the same file here for easy of copy and paste, but it needn't be in the same file... the employed overload of WithTools examines the current assembly for classes with the McpServerToolType attribute, and registers all methods with the McpTool attribute as tools.)

```shell
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.Extensions.Hosting

```

```c#
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
// Configure all logs to go to stderr
consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
.AddMcpServer()
.WithStdioServerTransport()
.WithToolsFromAssembly();
await builder.Build().RunAsync();

[McpServerToolType]
public static class EchoTool
{
[McpServerTool, Description("Echoes the message back to the client.")]
public static string Echo(string message) => $"hello {message}";
}
```

Tools can have the IMcpServer representing the server injected via a parameter to the method, and can use that for interaction with the connected client. Similarly, arguments may be injected via dependency injection. For example, this tool will use the supplied IMcpServer to make sampling requests back to the client in order to summarize content it downloads from the specified url via an HttpClient injected via dependency injection.

```c#
[McpServerTool(Name = "SummarizeContentFromUrl"), Description("Summarizes content downloaded from a specific URI")]
public static async Task<string> SummarizeDownloadedContent(
IMcpServer thisServer,
HttpClient httpClient,
[Description("The url from which to download the content to summarize")] string url,
CancellationToken cancellationToken)
{
string content = await httpClient.GetStringAsync(url);

    ChatMessage[] messages =
    [
        new(ChatRole.User, "Briefly summarize the following downloaded content:"),
        new(ChatRole.User, content),
    ];

    ChatOptions options = new()
    {
        MaxOutputTokens = 256,
        Temperature = 0.3f,
    };

    return $"Summary: {await thisServer.AsSamplingChatClient().GetResponseAsync(messages, options, cancellationToken)}";

}
```

Prompts can be exposed in a similar manner, using [McpServerPrompt], e.g.

```c#
[McpServerPromptType]
public static class MyPrompts
{
[McpServerPrompt, Description("Creates a prompt to summarize the provided message.")]
public static ChatMessage Summarize([Description("The content to summarize")] string content) =>
new(ChatRole.User, $"Please summarize this content into a single sentence: {content}");
}
```

More control is also available, with fine-grained control over configuring the server and how it should handle client requests. For example:

```c#
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using System.Text.Json;

McpServerOptions options = new()
{
ServerInfo = new Implementation() { Name = "MyServer", Version = "1.0.0" },
Capabilities = new ServerCapabilities()
{
Tools = new ToolsCapability()
{
ListToolsHandler = (request, cancellationToken) =>
Task.FromResult(new ListToolsResult()
{
Tools =
[
new Tool()
{
Name = "echo",
Description = "Echoes the input back to the client.",
InputSchema = JsonSerializer.Deserialize<JsonElement>("""
{
"type": "object",
"properties": {
"message": {
"type": "string",
"description": "The input to echo back"
}
},
"required": ["message"]
}
"""),
}
]
}),

            CallToolHandler = (request, cancellationToken) =>
            {
                if (request.Params?.Name == "echo")
                {
                    if (request.Params.Arguments?.TryGetValue("message", out var message) is not true)
                    {
                        throw new McpException("Missing required argument 'message'");
                    }

                    return Task.FromResult(new CallToolResponse()
                    {
                        Content = [new Content() { Text = $"Echo: {message}", Type = "text" }]
                    });
                }

                throw new McpException($"Unknown tool: '{request.Params?.Name}'");
            },
        }
    },

};

await using IMcpServer server = McpServerFactory.Create(new StdioServerTransport("MyServer"), options);
await server.RunAsync();
```

## SQLParser version v3.0.22

SQLParser

This C# library provides a SQL parser and lexer implementation using ANTLR. It allows you to parse SQL queries into an abstract syntax tree (AST) and perform various operations on the parsed queries.

Features
Lexical analysis: Tokenizing SQL queries into individual tokens.
Syntactic analysis: Parsing SQL queries into an abstract syntax tree.
Query manipulation: Modifying and transforming the parsed SQL queries.
Query analysis: Extracting metadata and information from SQL queries.
Installation
You can install the library via NuGet:

```shell
dotnet add package SQLParser

```

Usage
To use this library in your C# project, follow these steps:

1. Add a reference to the SQLParser package in your project.

2. Import the SQLParser namespace in your code:

```c#
using SQLParser.Parsers.TSql;
using SQLParser;


```

3. Create a parser listener class:

```c#
/// <summary>
/// This is an example of a printer that can be used to parse a statement.
/// </summary>
/// <seealso cref="TSqlParserBaseListener" />
internal class Printer : TSqlParserBaseListener
{
    /// <summary>
    /// Gets or sets a value indicating whether [statement found].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [statement found]; otherwise, <c>false</c>.
    /// </value>
    public bool StatementFound { get; set; }

    /// <summary>
    /// Enter a parse tree produced by <see cref="M:SQLParser.Parsers.TSql.TSqlParser.dml_clause" />.
    /// <para>The default implementation does nothing.</para>
    /// </summary>
    /// <param name="context">The parse tree.</param>
    public override void EnterDml_clause([NotNull] TSqlParser.Dml_clauseContext context)
    {
        // This is a select statement if the select_statement_standalone is not null
        StatementFound |= context.select_statement_standalone() != null;
        base.EnterDml_clause(context);
    }
}
```

4. Parse the query:

```c#

Parser.Parse("SELECT \* FROM Somewhere", ExamplePrinter, Enums.SQLType.TSql);
```

## Npgsql v9.0.3
