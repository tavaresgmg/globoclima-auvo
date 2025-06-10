using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Configuration;

namespace GloboClima.Infrastructure.Repositories;

public class DynamoDbContext
{
    private readonly AmazonDynamoDBClient _client;
    private readonly DynamoDBContext _context;

    public DynamoDbContext(IConfiguration configuration)
    {
        var region = configuration["AWS:Region"] ?? "us-east-1";
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        
        _client = new AmazonDynamoDBClient(regionEndpoint);
        
        var contextConfig = new DynamoDBContextConfig
        {
            ConsistentRead = false // Cannot use consistent read with GSI
        };
        
        _context = new DynamoDBContext(_client, contextConfig);
    }

    public DynamoDBContext Context => _context;
    public AmazonDynamoDBClient Client => _client;
}