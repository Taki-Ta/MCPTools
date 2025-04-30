using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using ModelContextProtocol.Server;
using ModelContextProtocol.AspNetCore;
using MCPTool.Common;
using Common.@interface;
using Common.provider;

// 创建应用程序配置，禁用静态文件功能
var options = new WebApplicationOptions
{
    WebRootPath = null,  // 禁用静态文件
    Args = args
};

var builder = WebApplication.CreateBuilder(options);

// 添加MCP服务
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithTools<DBServerTools>();

// 注册PostgreSQL和MongoDB实现
builder.Services.AddSingleton<IPostgreSQL, PostgreSQLProvider>();
builder.Services.AddSingleton<IMongoDB, MongoDBProvider>();

// 设置监听所有IP地址
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// 配置MCP路由
app.MapMcp();

app.Run();
