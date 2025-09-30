using app.Models;
using app.Service;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.Repository;

public class PostDynamoRepository : IPostRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly string _tableName;

    public PostDynamoRepository(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDBClient, IConfiguration configuration)
    {
        _dynamoDbContext = dynamoDbContext;
        _dynamoDBClient = dynamoDBClient;
        _tableName = configuration["DynamoDB:TableName"] ?? "swarmdemo-Posts";
        EnsureTableExistsAsync().Wait();
    }

    private async Task EnsureTableExistsAsync()
    {
        try
        {
            await _dynamoDBClient.DescribeTableAsync(_tableName);
            Console.WriteLine($"✅ DynamoDB table '{_tableName}' exists");
        }
        catch (ResourceNotFoundException)
        {
            Console.WriteLine($"🔧 Creating DynamoDB table '{_tableName}'");
            var request = new CreateTableRequest
            {
                TableName = _tableName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = ScalarAttributeType.S
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            await _dynamoDBClient.CreateTableAsync(request);
            Console.WriteLine($"✅ DynamoDB table '{_tableName}' created successfully");
        }
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        var scanConditions = new List<ScanCondition>();
        var operationConfig = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName
        };

        var posts = await _dynamoDbContext.ScanAsync<Post>(scanConditions, operationConfig).GetRemainingAsync();
        return posts;
    }

    public async Task<Post?> GetByIdAsync(string id)
    {
        
        var operationConfig = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName
        };
        var post = await _dynamoDbContext.LoadAsync<Post>(id, operationConfig);
        return post;
    }

    public async Task<Post> CreateAsync(Post post)
    {

        if (string.IsNullOrWhiteSpace(post.Id))
            post.Id = Guid.NewGuid().ToString("N"); // make a new string id

        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        var operationConfig = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName
        };

        await _dynamoDbContext.SaveAsync(post, operationConfig);
        return post;
    }

    public async Task<Post?> UpdateAsync(Post post)
    {
        var existingPost = await GetByIdAsync(post.Id);
        if (existingPost != null)
        {
            // Update fields
            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.ImagePath = post.ImagePath;
            existingPost.UpdatedAt = DateTime.UtcNow;
            var operationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            };

            await _dynamoDbContext.SaveAsync(existingPost, operationConfig);
        }
        return existingPost;
    }

    public async Task<Post?> DeleteAsync(string id)
    {
        var post = await GetByIdAsync(id);
        if (post != null)
        {
            var operationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            };
            await _dynamoDbContext.DeleteAsync<Post>(id, operationConfig);
        }
        return post;
    }
    // Call this right after CreateTable (or at app start) to ensure table is ready
    public static async Task WaitForTableActiveAsync(IAmazonDynamoDB client, string tableName, int maxSeconds = 60)
    {
        var deadline = DateTime.UtcNow.AddSeconds(maxSeconds);
        while (DateTime.UtcNow < deadline)
        {
            var resp = await client.DescribeTableAsync(new DescribeTableRequest { TableName = tableName });
            if (resp.Table.TableStatus == TableStatus.ACTIVE)
                return;

            await Task.Delay(1000);
        }
        throw new TimeoutException($"Table {tableName} not ACTIVE after {maxSeconds}s");
    }
}