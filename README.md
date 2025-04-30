# MCPTool

MCPTool 是一个 C#编写的操作数据库的 MCP(Model Context Protocol)实现。
现支持Postgres和MongoDB数据库。

## 项目结构

-   `Common/interface/IPostgreSQL.cs` - PostgreSQL接口定义
-   `Common/interface/IMongoDB.cs` - MongoDB接口定义
-   `Common/provider/PostgreSQLProvider.cs` - PostgreSQL的实现
-   `Common/provider/MongoDBProvider.cs` - MongoDB的实现
-   `Common/DBServerTools.cs` - 使用MCP工具包装数据库操作功能
-   `DBMCP/Program.cs` - Standard IO通信模式的MCP服务器
-   `DBMCP_SSE/Program.cs` - SSE通信模式的MCP服务器

## 功能

实现了以下数据库操作功能：

### PostgreSQL 功能

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

### MongoDB 功能

1. 注册/注销数据库连接
2. 查询文档(Find)
3. 插入单个/多个文档(InsertOne/InsertMany)
4. 更新文档(UpdateMany)
5. 删除文档(DeleteMany)
6. 创建/删除集合
7. 创建/删除索引
8. 列出集合
9. 获取集合详情

## 依赖项

-   .NET 7.0
-   ModelContextProtocol v0.1.0-preview.11
-   Npgsql v9.0.3
-   MongoDB.Driver v2.24.0
-   SQLParser v3.0.22
-   xUnit (用于测试)

## 编译和运行

### 服务器

#### Standard IO模式

```bash
# 编译
dotnet build

# 运行
cd DBMCP
dotnet run
```

#### SSE模式

```bash
# 编译
dotnet build

# 运行
cd DBMCP_SSE
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
            "args": ["run", "--project", "path/to/DBMCP", "--no-build"]
        },
        "dbmcp-sse": {
            "type": "sse",
            "url": "http://127.0.0.1:5000/sse"
        }
    }
}
```

## 注意事项

-   连接字符串中需要正确配置 PostgreSQL 或 MongoDB 的连接信息
-   客户端测试默认连接到本地数据库服务器(localhost)
-   需要确保测试数据库存在并且有访问权限
-   MongoDB 文档操作使用标准 JSON 格式
