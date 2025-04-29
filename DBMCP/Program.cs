using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using MCPTool.Common;

// 创建应用程序构建器
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// 添加控制台日志
builder.Logging.AddConsole(options =>
{
    // 配置所有日志输出到标准错误
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// 配置MCP服务器
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport() // 使用标准IO通信
    .WithToolsFromAssembly(); // 从程序集中注册所有工具

// 注册DBMCP实现
builder.Services.AddSingleton<IDBMCP, PostgresDBMCP>();

// 构建并运行应用程序
var app = builder.Build();
await app.RunAsync();