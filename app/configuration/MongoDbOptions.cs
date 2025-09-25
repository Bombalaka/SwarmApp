

namespace app.Configuration;
using app.Models;


public class MongoDbOptions {
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string PostsTableName { get; set; } = string.Empty;
}