using System;
using System.Threading.Tasks;
using Xunit;
using Common.provider;
using Common.config;

namespace Common.test
{
    [Trait("Category", "DatabaseIntegration")]
    [Collection("DatabaseIntegrationTests")]
    public class PostgreSQLTests
    {
        private readonly PostgreSQLProvider _provider = new PostgreSQLProvider();
        private string _connId = string.Empty;

        // 从配置文件读取连接字符串
        private readonly string _connectionString;

        public PostgreSQLTests()
        {
            // 从配置文件加载连接字符串
            _connectionString = DatabaseConfig.Instance.GetConnectionString(DatabaseType.PostgreSQL);
        }

        [Fact]
        [Trait("Description", "Test Register and Unregister functionality")]
        public async Task RegisterAndUnregister_ShouldWork()
        {
            // 如果配置文件中没有配置连接字符串，则跳过测试
            if (string.IsNullOrEmpty(_connectionString))
            {
                return;
            }

            _connId = await _provider.Register(_connectionString);
            Assert.NotNull(_connId);
            Assert.NotEmpty(_connId);

            bool result = await _provider.Unregister(_connId);
            Assert.True(result);
        }

        [Fact]
        [Trait("Description", "Test basic CRUD and schema operations")]
        public async Task BasicOperations_ShouldWork()
        {
            // 如果配置文件中没有配置连接字符串，则跳过测试
            if (string.IsNullOrEmpty(_connectionString))
            {
                return;
            }

            _connId = await _provider.Register(_connectionString);
            Assert.NotNull(_connId);
            Assert.NotEmpty(_connId);

            string testTableName = "test_table_" + Guid.NewGuid().ToString("N").ToLower(); // PostgreSQL通常使用小写
            string testSchemaName = "test_schema_" + Guid.NewGuid().ToString("N").ToLower();
            string testTypeName = "test_type_" + Guid.NewGuid().ToString("N").ToLower();
            string testIndexName = "idx_name_" + Guid.NewGuid().ToString("N").ToLower();

            try
            {
                // 创建Schema
                await _provider.CreateSchema(_connId, testSchemaName);

                // 创建自定义类型 (PostgreSQL语法)
                string createTypeSql = $"CREATE TYPE {testSchemaName}.{testTypeName} AS (id INTEGER, name VARCHAR(50))";
                await _provider.CreateType(_connId, createTypeSql);

                // 创建测试表
                string createTableSql = $"""
                CREATE TABLE {testSchemaName}.{testTableName} (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(50) NOT NULL,
                    value NUMERIC(10,2) NULL
                )
                """;
                await _provider.CreateTable(_connId, createTableSql);

                // 插入数据
                string insertSql = $"""
                INSERT INTO {testSchemaName}.{testTableName} (id, name, value)
                VALUES (1, '测试1', 10.5),
                       (2, '测试2', 20.75),
                       (3, '测试3', NULL)
                """;
                await _provider.Insert(_connId, insertSql);

                // 查询数据
                string querySql = $"SELECT * FROM {testSchemaName}.{testTableName}";
                string result = await _provider.Query(_connId, querySql);
                Assert.Contains("测试1", result);
                Assert.Contains("10.5", result);

                // 更新数据
                string updateSql = $"UPDATE {testSchemaName}.{testTableName} SET value = 15.75 WHERE id = 1";
                await _provider.Update(_connId, updateSql);

                // 验证更新
                result = await _provider.Query(_connId, querySql);
                Assert.Contains("15.75", result);

                // 删除数据
                string deleteSql = $"DELETE FROM {testSchemaName}.{testTableName} WHERE id = 3";
                await _provider.Delete(_connId, deleteSql);

                // 描述表结构
                result = await _provider.Describe(_connId, $"{testSchemaName}.{testTableName}");
                Assert.Contains("id", result);
                Assert.Contains("name", result);
                Assert.Contains("value", result);

                // 创建索引
                string createIndexSql = $"CREATE INDEX {testIndexName} ON {testSchemaName}.{testTableName}(name)";
                await _provider.CreateIndex(_connId, createIndexSql);

                // 列出表格
                result = await _provider.ListTables(_connId, testSchemaName);
                Assert.Contains(testTableName, result);

                // 删除索引 (PostgreSQL语法)
                string dropIndexSql = $"DROP INDEX {testSchemaName}.{testIndexName}";
                await _provider.DropIndex(_connId, dropIndexSql);

                // 删除表
                await _provider.DropTable(_connId, $"{testSchemaName}.{testTableName}");

                // 删除自定义类型
                string dropTypeSql = $"DROP TYPE {testSchemaName}.{testTypeName}";
                await _provider.DropType(_connId, dropTypeSql);

                // 删除Schema
                await _provider.DropSchema(_connId, $"DROP SCHEMA {testSchemaName} CASCADE");
            }
            catch (Exception ex)
            {
                Assert.Fail($"测试失败: {ex.Message}");
            }
            finally
            {
                // 清理资源，即使前面的测试失败
                try { await _provider.DropTable(_connId, $"{testSchemaName}.{testTableName}"); } catch { /* Ignore */ }
                try { await _provider.DropType(_connId, $"DROP TYPE {testSchemaName}.{testTypeName}"); } catch { /* Ignore */ }
                try { await _provider.DropSchema(_connId, $"DROP SCHEMA {testSchemaName} CASCADE"); } catch { /* Ignore */ }

                // 清理连接
                if (!string.IsNullOrEmpty(_connId))
                {
                    await _provider.Unregister(_connId);
                }
            }
        }
    }
} 