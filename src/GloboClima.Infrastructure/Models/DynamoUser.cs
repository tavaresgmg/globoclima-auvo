using Amazon.DynamoDBv2.DataModel;

namespace GloboClima.Infrastructure.Models;

[DynamoDBTable("auvo-users")]
public class DynamoUser
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey("EmailIndex")]
    [DynamoDBProperty("Email")]
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}