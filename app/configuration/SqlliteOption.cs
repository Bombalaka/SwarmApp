

namespace app.Configuration
{
    public class MySqlOptions
    {
        public const string SectionName = "MySql";

        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string PostsTableName { get; set; } = string.Empty;
    }
}