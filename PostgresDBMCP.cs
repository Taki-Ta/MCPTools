using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using SQLParser;
using SQLParser.Parsers.TSql;

namespace DBMCP
{
    /// <summary>
    /// PostgreSQL的DBMCP实现
    /// </summary>
    public class PostgresDBMCP : IDBMCP
    {
        private readonly ConcurrentDictionary<string, NpgsqlDataSource> _connections = new();

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
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connStr);
                var dataSource = dataSourceBuilder.Build();

                // 测试连接是否有效
                using (var conn = await dataSource.OpenConnectionAsync())
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        throw new Exception("无法建立连接");
                    }
                }

                _connections[connId] = dataSource;
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
        public async Task<bool> Unregister(string connId)
        {
            if (_connections.TryRemove(connId, out var dataSource))
            {
                await dataSource.DisposeAsync();
                return true;
            }
            return false;
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(querySql, connection);
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(insertSql, connection);
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(updateSql, connection);
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(deleteSql, connection);
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
            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            // 解析表名，处理schema.tableName格式
            string schemaName = "public";
            string tableNameOnly = tableName;
            if (tableName.Contains('.'))
            {
                var parts = tableName.Split('.');
                schemaName = parts[0];
                tableNameOnly = parts[1];
            }

            string sql = @"
SELECT 
    column_name, 
    data_type, 
    character_maximum_length, 
    column_default, 
    is_nullable
FROM 
    information_schema.columns
WHERE 
    table_schema = @schema AND 
    table_name = @table
ORDER BY 
    ordinal_position;";

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("schema", schemaName);
            command.Parameters.AddWithValue("table", tableNameOnly);
            
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = new NpgsqlCommand(createSql, connection);
                await command.ExecuteNonQueryAsync();
                
                return "表创建成功";
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // 42P07 是"表已存在"的错误代码
            {
                // 表已存在，视为成功
                return "表创建成功（表已存在）";
            }
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropTable(string connId, string tableName)
        {
            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = $"DROP TABLE IF EXISTS {tableName};";
            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sql, connection);
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try 
            {
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = new NpgsqlCommand(createIndexSql, connection);
                await command.ExecuteNonQueryAsync();
                
                return "索引创建成功";
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // 索引已存在
            {
                // 索引已存在，视为成功
                return "索引创建成功（索引已存在）";
            }
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="indexName">索引名</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropIndex(string connId, string indexName)
        {
            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = $"DROP INDEX IF EXISTS {indexName};";
            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sql, connection);
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
            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = @"
SELECT 
    table_name 
FROM 
    information_schema.tables 
WHERE 
    table_schema = @schema 
    AND table_type = 'BASE TABLE'
ORDER BY 
    table_name;";

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("schema", schema);
            
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

            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(createTypeSql, connection);
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
            if (!_connections.TryGetValue(connId, out var dataSource))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            string sql = $"CREATE SCHEMA IF NOT EXISTS {schemaName};";
            using var connection = await dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
            
            return "Schema创建成功";
        }

        /// <summary>
        /// 验证SQL类型
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="expectedType">期望的SQL类型</param>
        private void ValidateSqlType(string sql, string expectedType)
        {
            try
            {
                bool isValid = false;
                
                // 使用SQLParser库验证SQL类型
                if (expectedType == "SELECT")
                {
                    isValid = IsSelectStatement(sql);
                }
                else if (expectedType == "INSERT")
                {
                    isValid = IsInsertStatement(sql);
                }
                else if (expectedType == "UPDATE")
                {
                    isValid = IsUpdateStatement(sql);
                }
                else if (expectedType == "DELETE")
                {
                    isValid = IsDeleteStatement(sql);
                }
                else if (expectedType == "CREATE TABLE")
                {
                    isValid = IsCreateTableStatement(sql);
                }
                else if (expectedType == "CREATE INDEX")
                {
                    isValid = IsCreateIndexStatement(sql);
                }
                else if (expectedType == "CREATE TYPE")
                {
                    isValid = IsCreateTypeStatement(sql);
                }
                
                if (!isValid)
                {
                    throw new Exception($"无效的{expectedType}语句");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SQL验证失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否是SELECT语句
        /// </summary>
        private bool IsSelectStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsSelectStatement;
        }

        /// <summary>
        /// 检查是否是INSERT语句
        /// </summary>
        private bool IsInsertStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsInsertStatement;
        }

        /// <summary>
        /// 检查是否是UPDATE语句
        /// </summary>
        private bool IsUpdateStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsUpdateStatement;
        }

        /// <summary>
        /// 检查是否是DELETE语句
        /// </summary>
        private bool IsDeleteStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsDeleteStatement;
        }

        /// <summary>
        /// 检查是否是CREATE TABLE语句
        /// </summary>
        private bool IsCreateTableStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsCreateTableStatement;
        }

        /// <summary>
        /// 检查是否是CREATE INDEX语句
        /// </summary>
        private bool IsCreateIndexStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsCreateIndexStatement;
        }

        /// <summary>
        /// 检查是否是CREATE TYPE语句
        /// </summary>
        private bool IsCreateTypeStatement(string sql)
        {
            var listener = new SqlStatementTypeValidator();
            Parser.Parse(sql, listener, SQLParser.Enums.SQLType.TSql);
            return listener.IsCreateTypeStatement;
        }

        /// <summary>
        /// 格式化查询结果
        /// </summary>
        private async Task<string> FormatQueryResult(NpgsqlDataReader reader)
        {
            var result = new StringBuilder();
            
            // 获取列名
            var columns = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }
            
            // 添加表头
            result.AppendLine(string.Join("\t", columns));
            
            // 添加分隔线
            result.AppendLine(new string('-', columns.Sum(c => c.Length) + columns.Count * 3));
            
            // 添加数据行
            var rows = new List<List<string>>();
            while (await reader.ReadAsync())
            {
                var row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "NULL");
                }
                rows.Add(row);
            }
            
            foreach (var row in rows)
            {
                result.AppendLine(string.Join("\t", row));
            }
            
            result.AppendLine($"查询返回 {rows.Count} 行");
            
            return result.ToString();
        }
    }

    /// <summary>
    /// SQL语句类型验证器
    /// </summary>
    internal class SqlStatementTypeValidator : TSqlParserBaseListener
    {
        public bool IsSelectStatement { get; private set; }
        public bool IsInsertStatement { get; private set; }
        public bool IsUpdateStatement { get; private set; }
        public bool IsDeleteStatement { get; private set; }
        public bool IsCreateTableStatement { get; private set; }
        public bool IsCreateIndexStatement { get; private set; }
        public bool IsCreateTypeStatement { get; private set; }

        public override void EnterSelect_statement(TSqlParser.Select_statementContext context)
        {
            IsSelectStatement = true;
            base.EnterSelect_statement(context);
        }

        public override void EnterInsert_statement(TSqlParser.Insert_statementContext context)
        {
            IsInsertStatement = true;
            base.EnterInsert_statement(context);
        }

        public override void EnterUpdate_statement(TSqlParser.Update_statementContext context)
        {
            IsUpdateStatement = true;
            base.EnterUpdate_statement(context);
        }

        public override void EnterDelete_statement(TSqlParser.Delete_statementContext context)
        {
            IsDeleteStatement = true;
            base.EnterDelete_statement(context);
        }

        public override void EnterCreate_table(TSqlParser.Create_tableContext context)
        {
            IsCreateTableStatement = true;
            base.EnterCreate_table(context);
        }

        public override void EnterCreate_index(TSqlParser.Create_indexContext context)
        {
            IsCreateIndexStatement = true;
            base.EnterCreate_index(context);
        }

        public override void EnterCreate_type(TSqlParser.Create_typeContext context)
        {
            IsCreateTypeStatement = true;
            base.EnterCreate_type(context);
        }
    }
} 