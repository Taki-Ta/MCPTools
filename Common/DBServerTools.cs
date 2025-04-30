using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Common.@interface;
using Common.provider;
using ModelContextProtocol.Server;

namespace MCPTool.Common
{
    [McpServerToolType]
    public class DBServerTools
    {
        // 创建PostgreSQL实例
        private static readonly IRelationalDB _postgresProvider = new PostgreSQLProvider();
        
        // 创建MongoDB实例
        private static readonly IMongoDB _mongoProvider = new MongoDBProvider();

        // 创建MSSQL实例
        private static readonly IRelationalDB _mssqlProvider = new MSSQLProvider();

		#region PostgreSQL工具

		[McpServerTool, Description("PostgreSQL: 注册一个新的数据库连接")]
        public static async Task<string> PgRegister(
            [Description("PostgreSQL连接字符串")] string connStr)
        {
            try
            {
                return await _postgresProvider.Register(connStr);
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 注销一个数据库连接")]
        public static async Task<string> PgUnregister(
            [Description("连接ID")] string connId)
        {
            try
            {
                bool result = await _postgresProvider.Unregister(connId);
                return result ? "连接已注销" : "注销失败：连接ID不存在";
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 执行SELECT查询")]
        public static async Task<string> PgQuery(
            [Description("连接ID")] string connId,
            [Description("SELECT查询SQL")] string querySql)
        {
            try
            {
                return await _postgresProvider.Query(connId, querySql);
            }
            catch (Exception ex)
            {
                return $"查询错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 执行INSERT语句")]
        public static async Task<string> PgInsert(
            [Description("连接ID")] string connId,
            [Description("INSERT SQL语句")] string insertSql)
        {
            try
            {
                return await _postgresProvider.Insert(connId, insertSql);
            }
            catch (Exception ex)
            {
                return $"插入错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 执行UPDATE语句")]
        public static async Task<string> PgUpdate(
            [Description("连接ID")] string connId,
            [Description("UPDATE SQL语句")] string updateSql)
        {
            try
            {
                return await _postgresProvider.Update(connId, updateSql);
            }
            catch (Exception ex)
            {
                return $"更新错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 执行DELETE语句")]
        public static async Task<string> PgDelete(
            [Description("连接ID")] string connId,
            [Description("DELETE SQL语句")] string deleteSql)
        {
            try
            {
                return await _postgresProvider.Delete(connId, deleteSql);
            }
            catch (Exception ex)
            {
                return $"删除错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 描述表结构")]
        public static async Task<string> PgDescribe(
            [Description("连接ID")] string connId,
            [Description("表名")] string tableName)
        {
            try
            {
                return await _postgresProvider.Describe(connId, tableName);
            }
            catch (Exception ex)
            {
                return $"描述表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 创建新表")]
        public static async Task<string> PgCreateTable(
            [Description("连接ID")] string connId,
            [Description("CREATE TABLE SQL语句")] string createSql)
        {
            try
            {
                return await _postgresProvider.CreateTable(connId, createSql);
            }
            catch (Exception ex)
            {
                return $"创建表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 删除表")]
        public static async Task<string> PgDropTable(
            [Description("连接ID")] string connId,
            [Description("表名")] string tableName)
        {
            try
            {
                return await _postgresProvider.DropTable(connId, tableName);
            }
            catch (Exception ex)
            {
                return $"删除表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 创建索引")]
        public static async Task<string> PgCreateIndex(
            [Description("连接ID")] string connId,
            [Description("CREATE INDEX SQL语句")] string createIndexSql)
        {
            try
            {
                return await _postgresProvider.CreateIndex(connId, createIndexSql);
            }
            catch (Exception ex)
            {
                return $"创建索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 删除索引")]
        public static async Task<string> PgDropIndex(
            [Description("连接ID")] string connId,
            [Description("索引名")] string indexName)
        {
            try
            {
                return await _postgresProvider.DropIndex(connId, indexName);
            }
            catch (Exception ex)
            {
                return $"删除索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 列出所有表")]
        public static async Task<string> PgListTables(
            [Description("连接ID")] string connId,
            [Description("schema名，默认为public")] string schema = "public")
        {
            try
            {
                return await _postgresProvider.ListTables(connId, schema);
            }
            catch (Exception ex)
            {
                return $"列出表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 创建类型")]
        public static async Task<string> PgCreateType(
            [Description("连接ID")] string connId,
            [Description("CREATE TYPE SQL语句")] string createTypeSql)
        {
            try
            {
                return await _postgresProvider.CreateType(connId, createTypeSql);
            }
            catch (Exception ex)
            {
                return $"创建类型错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("PostgreSQL: 创建Schema")]
        public static async Task<string> PgCreateSchema(
            [Description("连接ID")] string connId,
            [Description("Schema名")] string schemaName)
        {
            try
            {
                return await _postgresProvider.CreateSchema(connId, schemaName);
            }
            catch (Exception ex)
            {
                return $"创建Schema错误: {ex.Message}";
            }
        }

		#endregion

		#region MSSQL工具

		[McpServerTool, Description("MSSQL: 注册一个新的数据库连接")]
        public static async Task<string> MssqlRegister(
            [Description("MSSQL连接字符串")] string connStr)
        {
            try
            {
                return await _mssqlProvider.Register(connStr);
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 注销一个数据库连接")]
        public static async Task<string> MssqlUnregister(
            [Description("连接ID")] string connId)
        {
            try
            {
                bool result = await _mssqlProvider.Unregister(connId);
                return result ? "连接已注销" : "注销失败：连接ID不存在";
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 执行SELECT查询")]
        public static async Task<string> MssqlQuery(
            [Description("连接ID")] string connId,
            [Description("SELECT查询SQL")] string querySql)
        {
            try
            {
                return await _mssqlProvider.Query(connId, querySql);
            }
            catch (Exception ex)
            {
                return $"查询错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 执行INSERT语句")]
        public static async Task<string> MssqlInsert(
            [Description("连接ID")] string connId,
            [Description("INSERT SQL语句")] string insertSql)
        {
            try
            {
                return await _mssqlProvider.Insert(connId, insertSql);
            }
            catch (Exception ex)
            {
                return $"插入错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 执行UPDATE语句")]
        public static async Task<string> MssqlUpdate(
            [Description("连接ID")] string connId,
            [Description("UPDATE SQL语句")] string updateSql)
        {
            try
            {
                return await _mssqlProvider.Update(connId, updateSql);
            }
            catch (Exception ex)
            {
                return $"更新错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 执行DELETE语句")]
        public static async Task<string> MssqlDelete(
            [Description("连接ID")] string connId,
            [Description("DELETE SQL语句")] string deleteSql)
        {
            try
            {
                return await _mssqlProvider.Delete(connId, deleteSql);
            }
            catch (Exception ex)
            {
                return $"删除错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 描述表结构")]
        public static async Task<string> MssqlDescribe(
            [Description("连接ID")] string connId,
            [Description("表名")] string tableName)
        {
            try
            {
                return await _mssqlProvider.Describe(connId, tableName);
            }
            catch (Exception ex)
            {
                return $"描述表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 创建新表")]
        public static async Task<string> MssqlCreateTable(
            [Description("连接ID")] string connId,
            [Description("CREATE TABLE SQL语句")] string createSql)
        {
            try
            {
                return await _mssqlProvider.CreateTable(connId, createSql);
            }
            catch (Exception ex)
            {
                return $"创建表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 删除表")]
        public static async Task<string> MssqlDropTable(
            [Description("连接ID")] string connId,
            [Description("表名")] string tableName)
        {
            try
            {
                return await _mssqlProvider.DropTable(connId, tableName);
            }
            catch (Exception ex)
            {
                return $"删除表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 创建索引")]
        public static async Task<string> MssqlCreateIndex(
            [Description("连接ID")] string connId,
            [Description("CREATE INDEX SQL语句")] string createIndexSql)
        {
            try
            {
                return await _mssqlProvider.CreateIndex(connId, createIndexSql);
            }
            catch (Exception ex)
            {
                return $"创建索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 删除索引")]
        public static async Task<string> MssqlDropIndex(
            [Description("连接ID")] string connId,
            [Description("索引名")] string indexName)
        {
            try
            {
                return await _mssqlProvider.DropIndex(connId, indexName);
            }
            catch (Exception ex)
            {
                return $"删除索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 列出所有表")]
        public static async Task<string> MssqlListTables(
            [Description("连接ID")] string connId,
            [Description("schema名，默认为dbo")] string schema = "dbo")
        {
            try
            {
                return await _mssqlProvider.ListTables(connId, schema);
            }
            catch (Exception ex)
            {
                return $"列出表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 创建类型")]
        public static async Task<string> MssqlCreateType(
            [Description("连接ID")] string connId,
            [Description("CREATE TYPE SQL语句")] string createTypeSql)
        {
            try
            {
                return await _mssqlProvider.CreateType(connId, createTypeSql);
            }
            catch (Exception ex)
            {
                return $"创建类型错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MSSQL: 创建Schema")]
        public static async Task<string> MssqlCreateSchema(
            [Description("连接ID")] string connId,
            [Description("Schema名")] string schemaName)
        {
            try
            {
                return await _mssqlProvider.CreateSchema(connId, schemaName);
            }
            catch (Exception ex)
            {
                return $"创建Schema错误: {ex.Message}";
            }
        }

		#endregion

		#region MongoDB工具

		[McpServerTool, Description("MongoDB: 注册一个新的MongoDB连接")]
        public static async Task<string> MongoRegister(
            [Description("MongoDB连接字符串")] string connStr)
        {
            try
            {
                return await _mongoProvider.Register(connStr);
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 注销MongoDB连接")]
        public static async Task<string> MongoUnregister(
            [Description("连接ID")] string connId)
        {
            try
            {
                bool result = await _mongoProvider.Unregister(connId);
                return result ? "连接已注销" : "注销失败：连接ID不存在";
            }
            catch (Exception ex)
            {
                return $"错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 查询文档")]
        public static async Task<string> MongoFind(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("查询条件（JSON格式）")] string filter)
        {
            try
            {
                return await _mongoProvider.Find(connId, database, collection, filter);
            }
            catch (Exception ex)
            {
                return $"查询文档错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 插入单个文档")]
        public static async Task<string> MongoInsertOne(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("文档内容（JSON格式）")] string document)
        {
            try
            {
                return await _mongoProvider.InsertOne(connId, database, collection, document);
            }
            catch (Exception ex)
            {
                return $"插入文档错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 插入多个文档")]
        public static async Task<string> MongoInsertMany(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("文档内容数组（JSON格式）")] string documents)
        {
            try
            {
                return await _mongoProvider.InsertMany(connId, database, collection, documents);
            }
            catch (Exception ex)
            {
                return $"批量插入文档错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 更新文档")]
        public static async Task<string> MongoUpdateMany(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("筛选条件（JSON格式）")] string filter,
            [Description("更新内容（JSON格式）")] string update)
        {
            try
            {
                return await _mongoProvider.UpdateMany(connId, database, collection, filter, update);
            }
            catch (Exception ex)
            {
                return $"更新文档错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 删除文档")]
        public static async Task<string> MongoDeleteMany(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("筛选条件（JSON格式）")] string filter)
        {
            try
            {
                return await _mongoProvider.DeleteMany(connId, database, collection, filter);
            }
            catch (Exception ex)
            {
                return $"删除文档错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 创建集合")]
        public static async Task<string> MongoCreateCollection(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection)
        {
            try
            {
                return await _mongoProvider.CreateCollection(connId, database, collection);
            }
            catch (Exception ex)
            {
                return $"创建集合错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 删除集合")]
        public static async Task<string> MongoDropCollection(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection)
        {
            try
            {
                return await _mongoProvider.DropCollection(connId, database, collection);
            }
            catch (Exception ex)
            {
                return $"删除集合错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 创建索引")]
        public static async Task<string> MongoCreateIndex(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("索引定义（JSON格式）")] string indexDefinition,
            [Description("索引名称")] string indexName)
        {
            try
            {
                return await _mongoProvider.CreateIndex(connId, database, collection, indexDefinition, indexName);
            }
            catch (Exception ex)
            {
                return $"创建索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 删除索引")]
        public static async Task<string> MongoDropIndex(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection,
            [Description("索引名称")] string indexName)
        {
            try
            {
                return await _mongoProvider.DropIndex(connId, database, collection, indexName);
            }
            catch (Exception ex)
            {
                return $"删除索引错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 列出数据库所有集合")]
        public static async Task<string> MongoListCollections(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database)
        {
            try
            {
                return await _mongoProvider.ListCollections(connId, database);
            }
            catch (Exception ex)
            {
                return $"获取集合列表错误: {ex.Message}";
            }
        }

        [McpServerTool, Description("MongoDB: 获取集合详情")]
        public static async Task<string> MongoGetCollectionInfo(
            [Description("连接ID")] string connId,
            [Description("数据库名")] string database,
            [Description("集合名")] string collection)
        {
            try
            {
                return await _mongoProvider.GetCollectionInfo(connId, database, collection);
            }
            catch (Exception ex)
            {
                return $"获取集合详情错误: {ex.Message}";
            }
        }

        #endregion
    }
} 