using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MCPTool.Common
{
    /// <summary>
    /// DBMCP接口定义，用于规范DB行为
    /// </summary>
    public interface IDBMCP
    {
        /// <summary>
        /// 注册新的数据库连接
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <returns>连接ID</returns>
        Task<string> Register(string connStr);

        /// <summary>
        /// 注销数据库连接
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <returns>操作是否成功</returns>
        Task<bool> Unregister(string connId);

        /// <summary>
        /// 执行查询SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="querySql">查询SQL语句</param>
        /// <returns>查询结果</returns>
        Task<string> Query(string connId, string querySql);

        /// <summary>
        /// 执行插入SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="insertSql">插入SQL语句</param>
        /// <returns>插入结果</returns>
        Task<string> Insert(string connId, string insertSql);

        /// <summary>
        /// 执行更新SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="updateSql">更新SQL语句</param>
        /// <returns>更新结果</returns>
        Task<string> Update(string connId, string updateSql);

        /// <summary>
        /// 执行删除SQL
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="deleteSql">删除SQL语句</param>
        /// <returns>删除结果</returns>
        Task<string> Delete(string connId, string deleteSql);

        /// <summary>
        /// 描述表结构
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>表结构描述</returns>
        Task<string> Describe(string connId, string tableName);

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createSql">创建表SQL</param>
        /// <returns>创建结果</returns>
        Task<string> CreateTable(string connId, string createSql);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="tableName">表名</param>
        /// <returns>删除结果</returns>
        Task<string> DropTable(string connId, string tableName);

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createIndexSql">创建索引SQL</param>
        /// <returns>创建结果</returns>
        Task<string> CreateIndex(string connId, string createIndexSql);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="indexName">索引名</param>
        /// <returns>删除结果</returns>
        Task<string> DropIndex(string connId, string indexName);

        /// <summary>
        /// 列出所有表
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="schema">schema名</param>
        /// <returns>表列表</returns>
        Task<string> ListTables(string connId, string schema);

        /// <summary>
        /// 创建类型
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="createTypeSql">创建类型SQL</param>
        /// <returns>创建结果</returns>
        Task<string> CreateType(string connId, string createTypeSql);

        /// <summary>
        /// 创建Schema
        /// </summary>
        /// <param name="connId">连接ID</param>
        /// <param name="schemaName">Schema名</param>
        /// <returns>创建结果</returns>
        Task<string> CreateSchema(string connId, string schemaName);
    }
} 