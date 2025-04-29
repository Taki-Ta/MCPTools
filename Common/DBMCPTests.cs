using System;
using System.Threading.Tasks;
using Xunit;
using MCPTool.Common;

namespace DBMCPTests
{
    public class PostgresDBMCPTests
    {
        private readonly string _testConnectionString = "Host=localhost;Port=5432;Database=taki;Username=postgres;Password=postgres";
        private IDBMCP _dbmcp;
        private string _connId;

        public PostgresDBMCPTests()
        {
            _dbmcp = new PostgresDBMCP();
        }

        private async Task CleanupTestEnvironment()
        {
            if (!string.IsNullOrEmpty(_connId))
            {
                try
                {
                    // 尝试删除测试表（如果存在）
                    await _dbmcp.DropTable(_connId, "test_schema.test_table");
                    
                    // 删除测试schema
                    var dropSchemaSql = "DROP SCHEMA IF EXISTS test_schema CASCADE;";
                    await _dbmcp.Query(_connId, dropSchemaSql);
                }
                catch
                {
                    // 忽略清理过程中的错误
                }
            }
        }

        [Fact]
        public async Task RegisterTest()
        {
            // 清理之前的测试环境
            await CleanupTestEnvironment();
            
            // 测试注册连接
            var connId = await _dbmcp.Register(_testConnectionString);
            Assert.NotNull(connId);
            Assert.NotEmpty(connId);

            // 保存连接ID以供其他测试使用
            _connId = connId;
        }

        [Fact]
        public async Task CreateSchemaTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
            }

            // 创建测试schema
            var result = await _dbmcp.CreateSchema(_connId, "test_schema");
            Assert.Contains("Schema创建成功", result);
        }

        [Fact]
        public async Task CreateTableTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
            }

            // 首先尝试删除表（如果存在）
            try
            {
                await _dbmcp.DropTable(_connId, "test_schema.test_table");
            }
            catch
            {
                // 忽略删除不存在表的错误
            }

            // 创建测试表
            var createTableSql = @"
                CREATE TABLE test_schema.test_table (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    email VARCHAR(100) UNIQUE,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";

            var result = await _dbmcp.CreateTable(_connId, createTableSql);
            Assert.Contains("表创建成功", result);
        }

        [Fact]
        public async Task InsertTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
            }

            // 插入测试数据
            var insertSql = "INSERT INTO test_schema.test_table (name, email) VALUES ('测试用户', 'test@example.com')";
            var result = await _dbmcp.Insert(_connId, insertSql);
            Assert.Contains("已插入 1 行", result);
        }

        [Fact]
        public async Task QueryTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
                await InsertTest();
            }

            // 查询测试数据
            var querySql = "SELECT * FROM test_schema.test_table";
            var result = await _dbmcp.Query(_connId, querySql);
            
            // 验证结果包含测试数据
            Assert.Contains("测试用户", result);
            Assert.Contains("test@example.com", result);
        }

        [Fact]
        public async Task UpdateTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
                await InsertTest();
            }

            // 更新测试数据
            var updateSql = "UPDATE test_schema.test_table SET name = '更新用户' WHERE email = 'test@example.com'";
            var result = await _dbmcp.Update(_connId, updateSql);
            Assert.Contains("已更新", result);

            // 验证更新结果
            var querySql = "SELECT * FROM test_schema.test_table WHERE email = 'test@example.com'";
            var queryResult = await _dbmcp.Query(_connId, querySql);
            Assert.Contains("更新用户", queryResult);
        }

        [Fact]
        public async Task CreateIndexTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
            }

            // 尝试先删除索引（如果存在）
            try
            {
                await _dbmcp.DropIndex(_connId, "test_schema.idx_test_name");
            }
            catch
            {
                // 忽略删除不存在索引的错误
            }

            // 创建索引
            var createIndexSql = "CREATE INDEX idx_test_name ON test_schema.test_table (name)";
            var result = await _dbmcp.CreateIndex(_connId, createIndexSql);
            Assert.Contains("索引创建成功", result);
        }

        [Fact]
        public async Task DescribeTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
            }

            // 描述表结构
            var result = await _dbmcp.Describe(_connId, "test_schema.test_table");
            
            // 验证结果包含表字段
            Assert.Contains("id", result);
            Assert.Contains("name", result);
            Assert.Contains("email", result);
            Assert.Contains("created_at", result);
        }

        [Fact]
        public async Task ListTablesTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
            }

            // 列出测试schema中的表
            var result = await _dbmcp.ListTables(_connId, "test_schema");
            
            // 验证结果包含测试表
            Assert.Contains("test_table", result);
        }

        [Fact]
        public async Task DeleteTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
                await InsertTest();
            }

            // 删除测试数据
            var deleteSql = "DELETE FROM test_schema.test_table WHERE email = 'test@example.com'";
            var result = await _dbmcp.Delete(_connId, deleteSql);
            Assert.Contains("已删除", result);

            // 验证删除结果
            var querySql = "SELECT * FROM test_schema.test_table WHERE email = 'test@example.com'";
            var queryResult = await _dbmcp.Query(_connId, querySql);
            Assert.Contains("查询返回 0 行", queryResult);
        }

        [Fact]
        public async Task DropIndexTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
                await CreateIndexTest();
            }

            // 删除索引
            var result = await _dbmcp.DropIndex(_connId, "test_schema.idx_test_name");
            Assert.Contains("索引删除成功", result);
        }

        [Fact]
        public async Task DropTableTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateSchemaTest();
                await CreateTableTest();
            }

            // 删除测试表
            var result = await _dbmcp.DropTable(_connId, "test_schema.test_table");
            Assert.Contains("表删除成功", result);
        }

        [Fact]
        public async Task UnregisterTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
            }

            // 清理测试环境
            await CleanupTestEnvironment();

            // 注销连接
            var result = await _dbmcp.Unregister(_connId);
            Assert.True(result);
        }
    }
} 