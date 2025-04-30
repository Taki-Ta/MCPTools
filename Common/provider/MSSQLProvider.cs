using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.@interface;
using Common.config;

namespace Common.provider
{
    /// <summary>
    /// MSSQL的关系型数据库实现
    /// </summary>
    public class MSSQLProvider : IRelationalDB
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();

        /// <summary>
        /// 注册新的数据库连接，使用配置文件中的连接字符串
        /// </summary>
        /// <returns>连接ID</returns>
        public async Task<string> Register()
        {
            string connStr = DatabaseConfig.Instance.GetConnectionString(DatabaseType.MSSQL);
            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("MSSQL连接字符串未配置");
            }
            return await Register(connStr);
        }

        /// <summary>
        /// 注册新的数据库连接
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <returns>连接ID</returns>
        public async Task<string> Register(string connStr)
        {
            try
            {
                var connId = Guid.NewGuid().ToString();
                
                // 测试连接是否有效
                using (var connection = new SqlConnection(connStr))
                {
                    await connection.OpenAsync();
                    if (connection.State != ConnectionState.Open)
                    {
                        throw new Exception("无法建立连接");
                    }
                }

                _connections[connId] = connStr;
                return connId;
            }
            catch (Exception ex)
            {
                throw new Exception($"注册连接失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注销数据库连接
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <returns>操作是否成功</returns>
        public Task<bool> Unregister(string connId)
        {
            var result = _connections.TryRemove(connId, out _);
            return Task.FromResult(result);
        }

        /// <summary>
        /// 执行查询SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="querySql">查询SQL语句</param>
        /// <returns>查询结果</returns>
        public async Task<string> Query(string connId, string querySql)
        {
            ValidateSqlType(querySql, "SELECT");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(querySql, connection);
            using var reader = await command.ExecuteReaderAsync();

            return await FormatQueryResult(reader);
        }

        /// <summary>
        /// 执行插入SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="insertSql">插入SQL语句</param>
        /// <returns>插入结果</returns>
        public async Task<string> Insert(string connId, string insertSql)
        {
            ValidateSqlType(insertSql, "INSERT");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(insertSql, connection);
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return $"已插入 {rowsAffected} 行";
        }

        /// <summary>
        /// 执行更新SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="updateSql">更新SQL语句</param>
        /// <returns>更新结果</returns>
        public async Task<string> Update(string connId, string updateSql)
        {
            ValidateSqlType(updateSql, "UPDATE");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(updateSql, connection);
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return $"已更新 {rowsAffected} 行";
        }

        /// <summary>
        /// 执行删除SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="deleteSql">删除SQL语句</param>
        /// <returns>删除结果</returns>
        public async Task<string> Delete(string connId, string deleteSql)
        {
            ValidateSqlType(deleteSql, "DELETE");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(deleteSql, connection);
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return $"已删除 {rowsAffected} 行";
        }

        /// <summary>
        /// 描述表结构
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>表结构描述</returns>
        public async Task<string> Describe(string connId, string tableName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            // 解析表名，处理schema.tableName格式
            string schemaName = "dbo";
            string tableNameOnly = tableName;
            if (tableName.Contains('.'))
            {
                var parts = tableName.Split('.');
                schemaName = parts[0];
                tableNameOnly = parts[1];
            }

            string sql = @"
SELECT 
    c.name AS column_name, 
    t.name AS data_type,
    c.max_length AS character_maximum_length,
    OBJECT_DEFINITION(c.default_object_id) AS column_default,
    CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS is_nullable
FROM 
    sys.columns c
INNER JOIN 
    sys.types t ON c.user_type_id = t.user_type_id
INNER JOIN 
    sys.tables tbl ON c.object_id = tbl.object_id
INNER JOIN 
    sys.schemas s ON tbl.schema_id = s.schema_id
WHERE 
    s.name = @schema AND 
    tbl.name = @table
ORDER BY 
    c.column_id;";

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@schema", schemaName);
            command.Parameters.AddWithValue("@table", tableNameOnly);

            using var reader = await command.ExecuteReaderAsync();
            return await FormatQueryResult(reader);
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createSql">创建表SQL</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateTable(string connId, string createSql)
        {
            ValidateSqlType(createSql, "CREATE TABLE");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(createSql, connection);
            await command.ExecuteNonQueryAsync();

            return "表创建成功";
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropTable(string connId, string tableName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = $"DROP TABLE {tableName};";

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            return "表删除成功";
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createIndexSql">创建索引SQL</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateIndex(string connId, string createIndexSql)
        {
            ValidateSqlType(createIndexSql, "CREATE INDEX");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(createIndexSql, connection);
            await command.ExecuteNonQueryAsync();

            return "索引创建成功";
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="indexName">索引名</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropIndex(string connId, string indexName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            // 索引名应该包含表名，例如：表名.索引名
            if (!indexName.Contains('.'))
            {
                throw new Exception("索引名格式不正确，应为：表名.索引名");
            }

            string sql = $"DROP INDEX {indexName};";

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            return "索引删除成功";
        }

        /// <summary>
        /// 列出所有表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="schema">schema名</param>
        /// <returns>表列表</returns>
        public async Task<string> ListTables(string connId, string schema)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = @"
SELECT 
    t.name AS table_name,
    s.name AS schema_name
FROM 
    sys.tables t
INNER JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    s.name = @schema
ORDER BY 
    t.name;";

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@schema", schema);

            using var reader = await command.ExecuteReaderAsync();
            return await FormatQueryResult(reader);
        }

        /// <summary>
        /// 创建类型
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createTypeSql">创建类型SQL</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateType(string connId, string createTypeSql)
        {
            ValidateSqlType(createTypeSql, "CREATE TYPE");

            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(createTypeSql, connection);
            await command.ExecuteNonQueryAsync();

            return "类型创建成功";
        }

        /// <summary>
        /// 创建Schema
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="schemaName">Schema名</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateSchema(string connId, string schemaName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = $"CREATE SCHEMA {schemaName};";

            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            return $"Schema {schemaName} 创建成功";
        }

        /// <summary>
        /// 删除Schema
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="schemaName">Schema名称或DROP SCHEMA语句</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropSchema(string connId, string schemaName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            // 如果输入是完整的DROP SCHEMA语句，则直接执行
            string sql = schemaName.Trim().ToUpper().StartsWith("DROP SCHEMA") 
                ? schemaName 
                : $"DROP SCHEMA {schemaName};";

            try 
            {
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();
                using var command = new SqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();

                return $"Schema {schemaName} 删除成功";
            }
            catch (SqlException ex) when (ex.Number == 3701) // 对象不存在
            {
                return $"Schema {schemaName} 已删除或不存在";
            }
        }

        /// <summary>
        /// 删除类型
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="typeName">类型名称或DROP TYPE语句</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropType(string connId, string typeName)
        {
            if (!_connections.TryGetValue(connId, out var connStr))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            // 如果输入是完整的DROP TYPE语句，则直接执行
            string sql = typeName.Trim().ToUpper().StartsWith("DROP TYPE") 
                ? typeName 
                : $"DROP TYPE {typeName};";

            try 
            {
                using var connection = new SqlConnection(connStr);
                await connection.OpenAsync();
                using var command = new SqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();

                return $"类型 {typeName} 删除成功";
            }
            catch (SqlException ex) when (ex.Number == 3701) // 对象不存在
            {
                return $"类型 {typeName} 已删除或不存在";
            }
        }

        /// <summary>
        /// 验证SQL类型
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="expectedType">期望的类型</param>
        private void ValidateSqlType(string sql, string expectedType)
        {
            try
            {
                // 简单验证SQL语句类型
                string trimmedSql = sql.Trim().ToUpper();
                
                if (expectedType == "SELECT" && !IsSelectStatement(trimmedSql))
                {
                    throw new Exception("不是有效的SELECT语句");
                }
                else if (expectedType == "INSERT" && !IsInsertStatement(trimmedSql))
                {
                    throw new Exception("不是有效的INSERT语句");
                }
                else if (expectedType == "UPDATE" && !IsUpdateStatement(trimmedSql))
                {
                    throw new Exception("不是有效的UPDATE语句");
                }
                else if (expectedType == "DELETE" && !IsDeleteStatement(trimmedSql))
                {
                    throw new Exception("不是有效的DELETE语句");
                }
                else if (expectedType == "CREATE TABLE" && !IsCreateTableStatement(trimmedSql))
                {
                    throw new Exception("不是有效的CREATE TABLE语句");
                }
                else if (expectedType == "CREATE INDEX" && !IsCreateIndexStatement(trimmedSql))
                {
                    throw new Exception("不是有效的CREATE INDEX语句");
                }
                else if (expectedType == "CREATE TYPE" && !IsCreateTypeStatement(trimmedSql))
                {
                    throw new Exception("不是有效的CREATE TYPE语句");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SQL验证失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 判断SQL是否为SELECT语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为SELECT语句</returns>
        private bool IsSelectStatement(string sql)
        {
            return sql.StartsWith("SELECT");
        }

        /// <summary>
        /// 判断SQL是否为INSERT语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为INSERT语句</returns>
        private bool IsInsertStatement(string sql)
        {
            return sql.StartsWith("INSERT");
        }

        /// <summary>
        /// 判断SQL是否为UPDATE语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为UPDATE语句</returns>
        private bool IsUpdateStatement(string sql)
        {
            return sql.StartsWith("UPDATE");
        }

        /// <summary>
        /// 判断SQL是否为DELETE语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为DELETE语句</returns>
        private bool IsDeleteStatement(string sql)
        {
            return sql.StartsWith("DELETE");
        }

        /// <summary>
        /// 判断SQL是否为CREATE TABLE语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为CREATE TABLE语句</returns>
        private bool IsCreateTableStatement(string sql)
        {
            return sql.StartsWith("CREATE TABLE");
        }

        /// <summary>
        /// 判断SQL是否为CREATE INDEX语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为CREATE INDEX语句</returns>
        private bool IsCreateIndexStatement(string sql)
        {
            return sql.StartsWith("CREATE INDEX") || 
                   sql.StartsWith("CREATE UNIQUE INDEX");
        }

        /// <summary>
        /// 判断SQL是否为CREATE TYPE语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否为CREATE TYPE语句</returns>
        private bool IsCreateTypeStatement(string sql)
        {
            return sql.StartsWith("CREATE TYPE");
        }

        /// <summary>
        /// 格式化查询结果
        /// </summary>
        /// <param name="reader">数据读取器</param>
        /// <returns>格式化后的查询结果</returns>
        private async Task<string> FormatQueryResult(SqlDataReader reader)
        {
            if (!reader.HasRows)
            {
                return "查询没有结果";
            }

            var columns = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }

            // 使用StringBuilder构建结果
            var sb = new StringBuilder();
            var rows = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object value = reader.GetValue(i);
                    
                    // 处理NULL值
                    if (value == DBNull.Value)
                    {
                        value = null;
                    }
                    
                    row[columnName] = value;
                }
                rows.Add(row);
            }

            // 将结果转换为JSON并返回
            return JsonSerializer.Serialize(rows, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
} 