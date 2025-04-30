using System.Text.Json;
using System.IO;

namespace Common.config
{
    /// <summary>
    /// 数据库配置管理类
    /// </summary>
    public class DatabaseConfig
    {
        private static DatabaseConfig? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 单例实例
        /// </summary>
        public static DatabaseConfig Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DatabaseConfig();
                    }
                }
                return _instance;
            } 
        }

        /// <summary>
        /// PostgreSQL连接字符串
        /// </summary>
        public string PostgreSQLConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// MSSQL连接字符串
        /// </summary>
        public string MSSQLConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// MongoDB连接字符串
        /// </summary>
        public string MongoDBConnectionString { get; set; } = string.Empty;

        // 私有构造函数，防止外部实例化
        private DatabaseConfig()
        {
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                string configPath = GetConfigPath();
                if (File.Exists(configPath))
                {
                    string jsonContent = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<DatabaseConfig>(jsonContent);
                    if (config != null)
                    {
                        PostgreSQLConnectionString = config.PostgreSQLConnectionString;
                        MSSQLConnectionString = config.MSSQLConnectionString;
                        MongoDBConnectionString = config.MongoDBConnectionString;
                    }
                }
                else
                {
                    // 创建默认配置文件
                    PostgreSQLConnectionString = "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres;";
                    MSSQLConnectionString = "Server=localhost;Database=testdb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;";
                    MongoDBConnectionString = "mongodb://localhost:27017/?retryWrites=true&w=majority";
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                string configPath = GetConfigPath();
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonContent = JsonSerializer.Serialize(this, options);
                File.WriteAllText(configPath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        private string GetConfigPath()
        {
            // 优先使用指定环境变量中的配置路径
            string envPath = Environment.GetEnvironmentVariable("MCPTOOL_CONFIG_PATH") ?? string.Empty;
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
            {
                return Path.Combine(envPath, "dbconfig.json");
            }

            // 其次使用应用程序根目录
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, "config", "dbconfig.json");
        }

        /// <summary>
        /// 获取指定数据库类型的连接字符串
        /// </summary>
        /// <param name="databaseType">数据库类型</param>
        /// <returns>连接字符串</returns>
        public string GetConnectionString(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.PostgreSQL => PostgreSQLConnectionString,
                DatabaseType.MSSQL => MSSQLConnectionString,
                DatabaseType.MongoDB => MongoDBConnectionString,
                _ => throw new ArgumentException($"不支持的数据库类型: {databaseType}")
            };
        }
    }

    /// <summary>
    /// 数据库类型枚举
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// PostgreSQL数据库
        /// </summary>
        PostgreSQL,

        /// <summary>
        /// MSSQL数据库
        /// </summary>
        MSSQL,

        /// <summary>
        /// MongoDB数据库
        /// </summary>
        MongoDB
    }
} 