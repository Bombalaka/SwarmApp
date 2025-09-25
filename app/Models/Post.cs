using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.AspNetCore.Http;
using Amazon.DynamoDBv2.DataModel;


namespace app.Models;

[DynamoDBTable("Posts")]
public class Post
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? Content { get; set; } //message
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // Saved file path (relative to wwwroot)
    public string? ImagePath { get; set; }
    // File upload only
    public IFormFile? ImageFile { get; set; }
}