using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Builder.Extensions;
using ModelContextProtocol.AspNetCore;
using MCPTool.Common;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
	.WithHttpTransport()
	.WithToolsFromAssembly().WithTools<DBMCPServerTools>();

builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.MapMcp();

app.Run();