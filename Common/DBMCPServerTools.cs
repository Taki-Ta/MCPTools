using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using MCPTool.Common;

[McpServerToolType]
public class DBMCPServerTools
{
    // 创建DBMCP实例
    private static readonly IDBMCP _dbmcp = new PostgresDBMCP();

    [McpServerTool, Description("注册一个新的数据库连接")]
    public static async Task<string> Register(
        [Description("数据库连接字符串")] string connStr)
    {
        try
        {
            return await _dbmcp.Register(connStr);
        }
        catch (Exception ex)
        {
            return $"错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("注销一个数据库连接")]
    public static async Task<string> Unregister(
        [Description("连接ID")] string connId)
    {
        try
        {
            bool result = await _dbmcp.Unregister(connId);
            return result ? "连接已注销" : "注销失败：连接ID不存在";
        }
        catch (Exception ex)
        {
            return $"错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("执行SELECT查询")]
    public static async Task<string> Query(
        [Description("连接ID")] string connId,
        [Description("SELECT查询SQL")] string querySql)
    {
        try
        {
            return await _dbmcp.Query(connId, querySql);
        }
        catch (Exception ex)
        {
            return $"查询错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("执行INSERT语句")]
    public static async Task<string> Insert(
        [Description("连接ID")] string connId,
        [Description("INSERT SQL语句")] string insertSql)
    {
        try
        {
            return await _dbmcp.Insert(connId, insertSql);
        }
        catch (Exception ex)
        {
            return $"插入错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("执行UPDATE语句")]
    public static async Task<string> Update(
        [Description("连接ID")] string connId,
        [Description("UPDATE SQL语句")] string updateSql)
    {
        try
        {
            return await _dbmcp.Update(connId, updateSql);
        }
        catch (Exception ex)
        {
            return $"更新错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("执行DELETE语句")]
    public static async Task<string> Delete(
        [Description("连接ID")] string connId,
        [Description("DELETE SQL语句")] string deleteSql)
    {
        try
        {
            return await _dbmcp.Delete(connId, deleteSql);
        }
        catch (Exception ex)
        {
            return $"删除错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("描述表结构")]
    public static async Task<string> Describe(
        [Description("连接ID")] string connId,
        [Description("表名")] string tableName)
    {
        try
        {
            return await _dbmcp.Describe(connId, tableName);
        }
        catch (Exception ex)
        {
            return $"描述表错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("创建新表")]
    public static async Task<string> CreateTable(
        [Description("连接ID")] string connId,
        [Description("CREATE TABLE SQL语句")] string createSql)
    {
        try
        {
            return await _dbmcp.CreateTable(connId, createSql);
        }
        catch (Exception ex)
        {
            return $"创建表错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("删除表")]
    public static async Task<string> DropTable(
        [Description("连接ID")] string connId,
        [Description("表名")] string tableName)
    {
        try
        {
            return await _dbmcp.DropTable(connId, tableName);
        }
        catch (Exception ex)
        {
            return $"删除表错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("创建索引")]
    public static async Task<string> CreateIndex(
        [Description("连接ID")] string connId,
        [Description("CREATE INDEX SQL语句")] string createIndexSql)
    {
        try
        {
            return await _dbmcp.CreateIndex(connId, createIndexSql);
        }
        catch (Exception ex)
        {
            return $"创建索引错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("删除索引")]
    public static async Task<string> DropIndex(
        [Description("连接ID")] string connId,
        [Description("索引名")] string indexName)
    {
        try
        {
            return await _dbmcp.DropIndex(connId, indexName);
        }
        catch (Exception ex)
        {
            return $"删除索引错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("列出所有表")]
    public static async Task<string> ListTables(
        [Description("连接ID")] string connId,
        [Description("schema名，默认为public")] string schema = "public")
    {
        try
        {
            return await _dbmcp.ListTables(connId, schema);
        }
        catch (Exception ex)
        {
            return $"列出表错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("创建类型")]
    public static async Task<string> CreateType(
        [Description("连接ID")] string connId,
        [Description("CREATE TYPE SQL语句")] string createTypeSql)
    {
        try
        {
            return await _dbmcp.CreateType(connId, createTypeSql);
        }
        catch (Exception ex)
        {
            return $"创建类型错误: {ex.Message}";
        }
    }

    [McpServerTool, Description("创建Schema")]
    public static async Task<string> CreateSchema(
        [Description("连接ID")] string connId,
        [Description("Schema名")] string schemaName)
    {
        try
        {
            return await _dbmcp.CreateSchema(connId, schemaName);
        }
        catch (Exception ex)
        {
            return $"创建Schema错误: {ex.Message}";
        }
    }
} 