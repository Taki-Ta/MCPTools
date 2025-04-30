using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq;
using Common.@interface;

namespace Common.provider
{
    /// <summary>
    /// MongoDB的DBMCP实现
    /// </summary>
    public class MongoDBProvider : IMongoDB
    {
        private readonly ConcurrentDictionary<string, MongoClient> _connections = new();

        /// <summary>
        /// 注册新的MongoDB连接
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <returns>连接ID</returns>
        public async Task<string> Register(string connStr)
        {
            try
            {
                var connId = Guid.NewGuid().ToString();

                // 创建MongoDB设置，增加超时时间
                var settings = MongoClientSettings.FromConnectionString(connStr);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(60);
                settings.ConnectTimeout = TimeSpan.FromSeconds(60);
                settings.SocketTimeout = TimeSpan.FromSeconds(60);

                var client = new MongoClient(settings);

                // 测试连接是否有效
                await client.ListDatabaseNames().ToListAsync();

                _connections[connId] = client;
                return connId;
            }
            catch (Exception ex)
            {
                throw new Exception($"注册MongoDB连接失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注销MongoDB连接
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <returns>操作是否成功</returns>
        public Task<bool> Unregister(string connId)
        {
            var result = _connections.TryRemove(connId, out _);
            return Task.FromResult(result);
        }

        /// <summary>
        /// 查询文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">查询条件（JSON格式）</param>
        /// <returns>查询结果</returns>
        public async Task<string> Find(string connId, string database, string collection, string filter)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                BsonDocument filterDoc = BsonDocument.Parse(filter);
                var cursor = await coll.FindAsync(filterDoc);
                var documents = await cursor.ToListAsync();

                return FormatDocuments(documents);
            }
            catch (Exception ex)
            {
                throw new Exception($"查询文档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 插入单个文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="document">文档内容（JSON格式）</param>
        /// <returns>插入结果</returns>
        public async Task<string> InsertOne(string connId, string database, string collection, string document)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                BsonDocument doc = BsonDocument.Parse(document);
                await coll.InsertOneAsync(doc);

                return $"已成功插入1个文档。新文档ID: {doc["_id"]}";
            }
            catch (Exception ex)
            {
                throw new Exception($"插入文档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 插入多个文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="documents">文档内容数组（JSON格式）</param>
        /// <returns>插入结果</returns>
        public async Task<string> InsertMany(string connId, string database, string collection, string documents)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                // 使用Json.NET反序列化JSON数组
                var docsArray = BsonSerializer.Deserialize<BsonDocument[]>(documents);
                var docs = new List<BsonDocument>(docsArray);

                await coll.InsertManyAsync(docs);

                return $"已成功插入{docs.Count}个文档";
            }
            catch (Exception ex)
            {
                throw new Exception($"批量插入文档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">筛选条件（JSON格式）</param>
        /// <param name="update">更新内容（JSON格式）</param>
        /// <returns>更新结果</returns>
        public async Task<string> UpdateMany(string connId, string database, string collection, string filter, string update)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                BsonDocument filterDoc = BsonDocument.Parse(filter);
                BsonDocument updateDoc = BsonDocument.Parse(update);

                var result = await coll.UpdateManyAsync(filterDoc, updateDoc);

                return $"已更新 {result.ModifiedCount} 个文档，匹配 {result.MatchedCount} 个文档";
            }
            catch (Exception ex)
            {
                throw new Exception($"更新文档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">筛选条件（JSON格式）</param>
        /// <returns>删除结果</returns>
        public async Task<string> DeleteMany(string connId, string database, string collection, string filter)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                BsonDocument filterDoc = BsonDocument.Parse(filter);
                var result = await coll.DeleteManyAsync(filterDoc);

                return $"已删除 {result.DeletedCount} 个文档";
            }
            catch (Exception ex)
            {
                throw new Exception($"删除文档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateCollection(string connId, string database, string collection)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                await db.CreateCollectionAsync(collection);
                return "集合创建成功";
            }
            catch (MongoCommandException ex) when (ex.CodeName == "NamespaceExists")
            {
                return "集合创建成功（集合已存在）";
            }
            catch (Exception ex)
            {
                throw new Exception($"创建集合失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropCollection(string connId, string database, string collection)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                await db.DropCollectionAsync(collection);
                return "集合删除成功";
            }
            catch (Exception ex)
            {
                throw new Exception($"删除集合失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="indexDefinition">索引定义（JSON格式）</param>
        /// <param name="indexName">索引名称</param>
        /// <returns>创建结果</returns>
        public async Task<string> CreateIndex(string connId, string database, string collection, string indexDefinition, string indexName)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                BsonDocument keys = BsonDocument.Parse(indexDefinition);
                var indexOptions = new CreateIndexOptions { Name = indexName };
                var indexModel = new CreateIndexModel<BsonDocument>(keys, indexOptions);

                var indexName2 = await coll.Indexes.CreateOneAsync(indexModel);
                return $"索引创建成功，索引名：{indexName2}";
            }
            catch (Exception ex)
            {
                throw new Exception($"创建索引失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="indexName">索引名称</param>
        /// <returns>删除结果</returns>
        public async Task<string> DropIndex(string connId, string database, string collection, string indexName)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var coll = db.GetCollection<BsonDocument>(collection);

                await coll.Indexes.DropOneAsync(indexName);
                return "索引删除成功";
            }
            catch (Exception ex)
            {
                throw new Exception($"删除索引失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 列出数据库所有集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <returns>集合列表</returns>
        public async Task<string> ListCollections(string connId, string database)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);
                var collections = await db.ListCollectionNames().ToListAsync();

                var result = new StringBuilder();
                result.AppendLine($"数据库 '{database}' 中的集合列表：");
                foreach (var collName in collections)
                {
                    result.AppendLine(collName);
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取集合列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取集合详情
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>集合详情</returns>
        public async Task<string> GetCollectionInfo(string connId, string database, string collection)
        {
            if (!_connections.TryGetValue(connId, out var client))
            {
                throw new Exception($"连接ID {connId} 不存在");
            }

            try
            {
                var db = client.GetDatabase(database);

                // 获取集合统计信息
                var stats = await db.RunCommandAsync<BsonDocument>(new BsonDocument("collStats", collection));

                // 获取集合索引信息
                var coll = db.GetCollection<BsonDocument>(collection);
                var indexes = await coll.Indexes.List().ToListAsync();

                var result = new StringBuilder();
                result.AppendLine($"集合 '{collection}' 详情：");
                result.AppendLine($"文档数量: {stats["count"]}");
                result.AppendLine($"占用空间: {stats["size"]} 字节");
                result.AppendLine($"存储大小: {stats["storageSize"]} 字节");

                result.AppendLine("索引列表:");
                foreach (var index in indexes)
                {
                    result.AppendLine($"  - {index["name"]}: {index["key"].ToJson()}");
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取集合详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 格式化文档集合为可读字符串
        /// </summary>
        /// <param name="documents">文档集合</param>
        /// <returns>格式化后的字符串</returns>
        private string FormatDocuments(List<BsonDocument> documents)
        {
            if (documents.Count == 0)
            {
                return "查询返回 0 个文档";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"查询返回 {documents.Count} 个文档:");
            sb.AppendLine();

            foreach (var doc in documents)
            {
                sb.AppendLine(doc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}