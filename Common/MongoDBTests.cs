using System;
using System.Threading.Tasks;
using Xunit;
using MCPTool.Common;

namespace DBMCPTests
{
    public class MongoDBTests
    {
        private readonly string _testConnectionString = "mongodb://gw:123.zxc@10.10.1.105:27017";
        private readonly IMongoDB _dbProvider;
        private string? _connId;
        private readonly string _testDatabase = "test";
        private readonly string _testCollection = "test_collection";

        public MongoDBTests()
        {
            _dbProvider = new MongoDBProvider();
        }

        private async Task CleanupTestEnvironment()
        {
            if (!string.IsNullOrEmpty(_connId))
            {
                try
                {
                    // 删除测试集合
                    await _dbProvider.DropCollection(_connId, _testDatabase, _testCollection);
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
            var connId = await _dbProvider.Register(_testConnectionString);
            Assert.NotNull(connId);
            Assert.NotEmpty(connId);

            // 保存连接ID以供其他测试使用
            _connId = connId;
        }

        [Fact]
        public async Task CreateCollectionTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
            }

            // 创建测试集合
            var result = await _dbProvider.CreateCollection(_connId, _testDatabase, _testCollection);
            Assert.Contains("集合创建成功", result);
        }

        [Fact]
        public async Task InsertOneTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
            }

            // 插入测试文档
            string document = @"{
                ""name"": ""测试用户"",
                ""email"": ""test@example.com"",
                ""age"": 30,
                ""created_at"": """ + DateTime.UtcNow.ToString("o") + @"""
            }";

            var result = await _dbProvider.InsertOne(_connId, _testDatabase, _testCollection, document);
            Assert.Contains("已成功插入1个文档", result);
        }

        [Fact]
        public async Task FindTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
                await InsertOneTest();
            }

            // 查询测试文档
            string filter = @"{ ""name"": ""测试用户"" }";
            var result = await _dbProvider.Find(_connId, _testDatabase, _testCollection, filter);
            
            // 验证结果包含测试数据
            Assert.Contains("测试用户", result);
            Assert.Contains("test@example.com", result);
        }

        [Fact]
        public async Task InsertManyTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
            }

            // 插入多个测试文档
            string documents = @"[
                {
                    ""name"": ""用户1"",
                    ""email"": ""user1@example.com"",
                    ""age"": 31,
                    ""created_at"": """ + DateTime.UtcNow.ToString("o") + @"""
                },
                {
                    ""name"": ""用户2"",
                    ""email"": ""user2@example.com"",
                    ""age"": 32,
                    ""created_at"": """ + DateTime.UtcNow.ToString("o") + @"""
                }
            ]";

            var result = await _dbProvider.InsertMany(_connId, _testDatabase, _testCollection, documents);
            Assert.Contains("已成功插入2个文档", result);
            
            // 验证插入结果
            string filter = @"{ ""$or"": [ { ""name"": ""用户1"" }, { ""name"": ""用户2"" } ] }";
            var findResult = await _dbProvider.Find(_connId, _testDatabase, _testCollection, filter);
            Assert.Contains("user1@example.com", findResult);
            Assert.Contains("user2@example.com", findResult);
        }

        [Fact]
        public async Task UpdateManyTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
                await InsertManyTest();
            }

            // 更新测试文档
            string filter = @"{ ""email"": ""user1@example.com"" }";
            string update = @"{ ""$set"": { ""name"": ""更新用户1"", ""age"": 40 } }";
            var result = await _dbProvider.UpdateMany(_connId, _testDatabase, _testCollection, filter, update);
            Assert.Contains("已更新", result);

            // 验证更新结果
            var findResult = await _dbProvider.Find(_connId, _testDatabase, _testCollection, filter);
            Assert.Contains("更新用户1", findResult);
            Assert.Contains("40", findResult);
        }

        [Fact]
        public async Task CreateIndexTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
            }

            // 创建索引
            string indexDefinition = @"{ ""email"": 1 }";
            string indexName = "idx_email";
            var result = await _dbProvider.CreateIndex(_connId, _testDatabase, _testCollection, indexDefinition, indexName);
            Assert.Contains("索引创建成功", result);
            
            // 验证索引创建
            var collectionInfo = await _dbProvider.GetCollectionInfo(_connId, _testDatabase, _testCollection);
            Assert.Contains("idx_email", collectionInfo);
        }

        [Fact]
        public async Task ListCollectionsTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
            }

            // 列出测试数据库中的集合
            var result = await _dbProvider.ListCollections(_connId, _testDatabase);
            
            // 验证结果包含测试集合
            Assert.Contains(_testCollection, result);
        }

        [Fact]
        public async Task GetCollectionInfoTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
                await InsertOneTest();
                await CreateIndexTest();
            }

            // 获取集合详情
            var result = await _dbProvider.GetCollectionInfo(_connId, _testDatabase, _testCollection);
            
            // 验证结果包含关键信息
            Assert.Contains("文档数量", result);
            Assert.Contains("索引列表", result);
        }

        [Fact]
        public async Task DeleteManyTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
                await InsertOneTest();
            }

            // 删除测试文档
            string filter = @"{ ""email"": ""test@example.com"" }";
            var result = await _dbProvider.DeleteMany(_connId, _testDatabase, _testCollection, filter);
            Assert.Contains("已删除", result);

            // 验证删除结果
            var findResult = await _dbProvider.Find(_connId, _testDatabase, _testCollection, filter);
            Assert.Contains("查询返回 0 个文档", findResult);
        }

        [Fact]
        public async Task DropIndexTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
                await CreateIndexTest();
            }

            // 删除索引
            var result = await _dbProvider.DropIndex(_connId, _testDatabase, _testCollection, "idx_email");
            Assert.Contains("索引删除成功", result);
        }

        [Fact]
        public async Task DropCollectionTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
                await CreateCollectionTest();
            }

            // 删除集合
            var result = await _dbProvider.DropCollection(_connId, _testDatabase, _testCollection);
            Assert.Contains("集合删除成功", result);
        }

        [Fact]
        public async Task UnregisterTest()
        {
            if (string.IsNullOrEmpty(_connId))
            {
                await RegisterTest();
            }

            // 测试注销连接
            bool result = await _dbProvider.Unregister(_connId);
            Assert.True(result);
            
            // 清空连接ID
            _connId = null;
        }
    }
} 