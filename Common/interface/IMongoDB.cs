using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.@interface
{
    /// <summary>
    /// MongoDB的DBMCP接口定义，用于规范MongoDB操作行为
    /// </summary>
    public interface IMongoDB
    {
        /// <summary>
        /// 注册新的MongoDB连接，使用配置文件中的连接字符串
        /// </summary>
        /// <returns>连接ID</returns>
        Task<string> Register();

        /// <summary>
        /// 注册新的MongoDB连接
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <returns>连接ID</returns>
        Task<string> Register(string connStr);

        /// <summary>
        /// 注销MongoDB连接
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> Unregister(string connId);

        /// <summary>
        /// 查询文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">查询条件（JSON格式）</param>
        /// <returns>查询结果</returns>
        Task<string> Find(string connId, string database, string collection, string filter);

        /// <summary>
        /// 插入单个文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="document">文档内容（JSON格式）</param>
        /// <returns>插入结果</returns>
        Task<string> InsertOne(string connId, string database, string collection, string document);

        /// <summary>
        /// 插入多个文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="documents">文档内容数组（JSON格式）</param>
        /// <returns>插入结果</returns>
        Task<string> InsertMany(string connId, string database, string collection, string documents);

        /// <summary>
        /// 更新文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">筛选条件（JSON格式）</param>
        /// <param name="update">更新内容（JSON格式）</param>
        /// <returns>更新结果</returns>
        Task<string> UpdateMany(string connId, string database, string collection, string filter, string update);

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="filter">筛选条件（JSON格式）</param>
        /// <returns>删除结果</returns>
        Task<string> DeleteMany(string connId, string database, string collection, string filter);

        /// <summary>
        /// 创建集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>创建结果</returns>
        Task<string> CreateCollection(string connId, string database, string collection);

        /// <summary>
        /// 删除集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>删除结果</returns>
        Task<string> DropCollection(string connId, string database, string collection);

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="indexDefinition">索引定义（JSON格式）</param>
        /// <param name="indexName">索引名称</param>
        /// <returns>创建结果</returns>
        Task<string> CreateIndex(string connId, string database, string collection, string indexDefinition, string indexName);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <param name="indexName">索引名称</param>
        /// <returns>删除结果</returns>
        Task<string> DropIndex(string connId, string database, string collection, string indexName);

        /// <summary>
        /// 列出数据库所有集合
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <returns>集合列表</returns>
        Task<string> ListCollections(string connId, string database);

        /// <summary>
        /// 获取集合详情
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="database">数据库名</param>
        /// <param name="collection">集合名</param>
        /// <returns>集合详情</returns>
        Task<string> GetCollectionInfo(string connId, string database, string collection);
    }
}