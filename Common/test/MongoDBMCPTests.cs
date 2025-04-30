using System;
using System.Threading.Tasks;
using Xunit;
using Common.provider;
using System.Text.Json;
using Common.config;

namespace Common.test
{
    [Trait("Category", "DatabaseIntegration")]
    [Collection("DatabaseIntegrationTests")]
    public class MongoDBMCPTests
    {
        private readonly MongoDBProvider _provider = new MongoDBProvider();
        private string _connId = string.Empty;

        // 从配置文件读取连接字符串
        private readonly string _connectionString;
        
        // 测试数据库和集合名
        private readonly string _testDbName = "test_db_" + Guid.NewGuid().ToString("N");
        private readonly string _testCollectionName = "test_collection_" + Guid.NewGuid().ToString("N");

        public MongoDBMCPTests()
        {
            // 从配置文件加载连接字符串
            _connectionString = DatabaseConfig.Instance.GetConnectionString(DatabaseType.MongoDB);
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
        [Trait("Description", "Test basic CRUD and collection operations")]
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

            try
            {
                // 创建集合
                string result = await _provider.CreateCollection(_connId, _testDbName, _testCollectionName);
                Assert.Contains("成功", result);

                // 插入单个文档
                string singleDoc = @"{""name"": ""测试文档1"", ""value"": 10.5, ""tags"": [""测试"", ""文档""]}";
                result = await _provider.InsertOne(_connId, _testDbName, _testCollectionName, singleDoc);
                Assert.Contains("成功", result);
                Assert.Contains("新文档ID", result);

                // 插入多个文档
                string multiDocs = @"[
                    {""name"": ""测试文档2"", ""value"": 20.75, ""active"": true},
                    {""name"": ""测试文档3"", ""value"": 30, ""active"": false}
                ]";
                result = await _provider.InsertMany(_connId, _testDbName, _testCollectionName, multiDocs);
                Assert.Contains("成功插入", result);
                Assert.Contains("2", result); // 插入了2个文档

                // 查询文档
                result = await _provider.Find(_connId, _testDbName, _testCollectionName, "{}");
                Assert.Contains("测试文档1", result);
                Assert.Contains("测试文档2", result);
                Assert.Contains("测试文档3", result);

                // 条件查询
                result = await _provider.Find(_connId, _testDbName, _testCollectionName, "{\"value\": {\"$gt\": 15}}");
                Assert.Contains("测试文档2", result);
                Assert.Contains("测试文档3", result);
                Assert.DoesNotContain("测试文档1", result);

                // 更新文档
                string filter = "{\"name\": \"测试文档2\"}";
                string update = "{\"$set\": {\"value\": 25.5, \"updated\": true}}";
                result = await _provider.UpdateMany(_connId, _testDbName, _testCollectionName, filter, update);
                Assert.Contains("已更新", result);

                // 验证更新
                result = await _provider.Find(_connId, _testDbName, _testCollectionName, filter);
                Assert.Contains("25.5", result);
                Assert.Contains("updated", result);
                Assert.Contains("true", result);

                // 删除文档
                filter = "{\"name\": \"测试文档3\"}";
                result = await _provider.DeleteMany(_connId, _testDbName, _testCollectionName, filter);
                Assert.Contains("已删除", result);

                // 验证删除
                result = await _provider.Find(_connId, _testDbName, _testCollectionName, "{}");
                Assert.DoesNotContain("测试文档3", result);
                Assert.Contains("测试文档1", result);
                Assert.Contains("测试文档2", result);

                // 创建索引
                string indexDefinition = "{\"name\": 1}";
                string indexName = "idx_name";
                result = await _provider.CreateIndex(_connId, _testDbName, _testCollectionName, indexDefinition, indexName);
                Assert.Contains("成功", result);

                // 获取集合信息
                result = await _provider.GetCollectionInfo(_connId, _testDbName, _testCollectionName);
                Assert.Contains(_testCollectionName, result);
                Assert.Contains("索引", result);

                // 列出所有集合
                result = await _provider.ListCollections(_connId, _testDbName);
                Assert.Contains(_testCollectionName, result);

                // 删除索引
                result = await _provider.DropIndex(_connId, _testDbName, _testCollectionName, indexName);
                Assert.Contains("成功", result);

                // 删除集合
                result = await _provider.DropCollection(_connId, _testDbName, _testCollectionName);
                Assert.Contains("成功", result);
            }
            catch (Exception ex)
            {
                Assert.Fail($"测试失败: {ex.Message}");
            }
            finally
            {
                // 清理资源，即使前面的测试失败
                try { await _provider.DropCollection(_connId, _testDbName, _testCollectionName); } catch { /* Ignore */ }

                // 清理连接
                if (!string.IsNullOrEmpty(_connId))
                {
                    await _provider.Unregister(_connId);
                }
            }
        }
    }
} 