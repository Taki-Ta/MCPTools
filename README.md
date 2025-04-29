# MCPTool

MCPTool 是一个 C#编写的操作数据库的 MCP(Model Context Protocol)实现。

## 项目结构

-   `IDBMCP.cs` - DBMCP 接口定义
-   `PostgresDBMCP.cs` - PostgreSQL 的 DBMCP 实现
-   `DBMCPServerTools.cs` - 使用 MCP 工具包装 DBMCP 功能
-   `Program.cs` - 主程序，启动 MCP 服务器
-   `DBMCPTests.cs` - 测试类，测试主要功能

## 功能

实现了以下数据库操作功能：

1. 注册/注销数据库连接
2. 查询(SELECT)
3. 插入(INSERT)
4. 更新(UPDATE)
5. 删除(DELETE)
6. 描述表结构
7. 创建/删除表
8. 创建/删除索引
9. 列出所有表
10. 创建类型
11. 创建 Schema

## 依赖项

-   .NET 7.0
-   ModelContextProtocol v0.1.0-preview.11
-   Npgsql v9.0.3
-   SQLParser v3.0.22
-   xUnit (用于测试)

## 编译和运行

### 服务器

```bash
# 编译
dotnet build

# 运行
dotnet run
```

### 测试

```bash
dotnet test
```

## 使用示例

### 配置 MCP

```json
{
    "servers": {
        "dbmcp-stdio": {
            "type": "stdio",
            "command": "dotnet",
            "args": ["run", "--project", "path/to/dbmcp", "--no-build"]
        },
        "dbmcp-sse": {
            "type": "sse",
            "url": "http://127.0.0.1:5000/sse"
        }
    }
}
```

## 注意事项

-   连接字符串中需要正确配置 PostgreSQL 的连接信息
-   客户端测试默认连接到本地 PostgreSQL(localhost:5432)
-   需要确保测试数据库存在并且有访问权限
