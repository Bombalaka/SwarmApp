using app.Repository;
using app.Models;
using app.Configuration;
using app.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.DataProtection;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using app.Storage;


// Load environment variables from .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



// Check feature flags to determine which database to use
var useMySQL = builder.Configuration.GetValue<bool>("FeatureFlags:MySql");
var useDynamoDB = builder.Configuration.GetValue<bool>("FeatureFlags:DynamoDB");


if (useMySQL)
{
    Console.WriteLine("✅ Using MySQL database (SQL mode).");
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    builder.Services.AddScoped<IPostRepository, PostSqlRepository>();
}

else if (useDynamoDB)
{
    Console.WriteLine("✅ Using DynamoDB database.");
    
    // Get DynamoDB configuration from appsettings.json
    var dynamoDBRegion = builder.Configuration.GetConnectionString("DynamoDBRegion");
    
    // Configure DynamoDB client for AWS DynamoDB
    Console.WriteLine("☁️ Connecting to AWS DynamoDB");
    var dynamoDBConfig = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(dynamoDBRegion)
    };
    
    // Create DynamoDB client
    var dynamoDBClient = new AmazonDynamoDBClient(dynamoDBConfig);
    
    // Register DynamoDB services
    builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDBClient);
    builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
    builder.Services.AddScoped<IPostRepository, PostDynamoRepository>();
    
    Console.WriteLine($"🔧 DynamoDB table 'Posts' will be created automatically on first use");
}
else
{
    Console.WriteLine("✅ Using InMemory database.");
    builder.Services.AddSingleton<IPostRepository, InMemoryRepository>();
}

builder.Services.AddDataProtection()
    .PersistKeysToAWSSystemsManager("/swarmapp/dataprotection")
    .SetApplicationName("SwarmApp");

//s3 for storage image uploads
var awsOptions = builder.Configuration.GetAWSOptions(); // reads "AWS:Region" if present
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();
var useS3 = builder.Configuration.GetValue<bool>("FeatureFlags:UseS3");
if (useS3)
{
    builder.Services.AddSingleton<IImageStorage, S3ImageStorage>();
    Console.WriteLine("✅ Using S3 for storage image uploads.");
}
else
{
    builder.Services.AddSingleton<IImageStorage, LocalImageStorage>();
    Console.WriteLine("✅ Using Local for storage image uploads.");
}



var app = builder.Build();

// Only create database if using MySQL
if (useMySQL)
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("✅ MySQL database and tables created successfully!");
    }
}
// Wait for DynamoDB table to be created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
    await PostDynamoRepository.WaitForTableActiveAsync(context, "Posts");
    Console.WriteLine("✅ DynamoDB database and tables created successfully!");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();


//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
