using AuditTrail.Model;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditTrail;

public class AuditTrailProvider<T> : IAuditTrailProvider<T> where T : class
{
    private const string _alias = "auditlog";
    private readonly string _indexName = $"{_alias}-{DateTime.UtcNow:yyyy-MM-dd}";
    private readonly static Field TimestampField = new("timestamp");
    private readonly IOptions<AuditTrailOptions> _options;
    private static ElasticsearchClient _elasticsearchClient;

    public AuditTrailProvider(IOptions<AuditTrailOptions> auditTrailOptions)
    {
        _options = auditTrailOptions ?? throw new ArgumentNullException(nameof(auditTrailOptions));

        if (_options.Value.IndexPerMonth)
        {
            _indexName = $"{_alias}-{DateTime.UtcNow:yyyy-MM}";
        }

        EnsureElasticClient(_indexName);
    }

    private static void EnsureElasticClient(string indexName)
    {
        // TODO fix, make this a scoped service
        if (_elasticsearchClient != null)
        {
            return;
        }

        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
                    .Authentication(new BasicAuthentication("elastic", "Password1!"))
                    .DefaultMappingFor<T>(m => m.IndexName(indexName));

        _elasticsearchClient = new ElasticsearchClient(settings);
    }

    public async Task AddLog(T auditTrailLog)
    {
        EnsureElasticClient(_indexName);

        var indexRequest = new IndexRequest<T>(auditTrailLog);

        var response = await _elasticsearchClient.IndexAsync(indexRequest);
        if (!response.IsValidResponse)
        {
            throw new ArgumentException("Add auditlog disaster!");
        }
    }

    private static List<SortOptions> BuildSort()
    {
        var sorts = new List<SortOptions>();

        var sort = SortOptions.Field(TimestampField, new FieldSort
        {
            Order = SortOrder.Desc
        });

        sorts.Add(sort);

        return sorts;
    }

    public async Task<long> Count(string filter = "*")
    {
        EnsureElasticClient(_indexName);
        await EnsureAlias();

        var searchRequest = new SearchRequest<T>(Indices.Parse(_alias))
        {
            Size = 0,
            Query = new SimpleQueryStringQuery
            {
                Query = filter
            },
            Sort = BuildSort()
        };

        var searchResponse = await _elasticsearchClient.SearchAsync<AuditTrailLog>(searchRequest);

        return searchResponse.Total;
    }

    public async Task<IEnumerable<T>> QueryAuditLogs(string filter = "*", AuditTrailPaging auditTrailPaging = null)
    {
        var from = 0;
        var size = 10;
        EnsureElasticClient(_indexName);
        await EnsureAlias();

        if (auditTrailPaging != null)
        {
            from = auditTrailPaging.Skip;
            size = auditTrailPaging.Size;
            if (size > 1000)
            {
                // max limit 1000 items
                size = 1000;
            }
        }
        var searchRequest = new SearchRequest<T>(Indices.Parse(_alias))
        {
            Size = size,
            From = from,
            Query = new SimpleQueryStringQuery
            {
                Query = filter
            },
            Sort = BuildSort()
        };

        var searchResponse = await _elasticsearchClient.SearchAsync<T>(searchRequest);

        return searchResponse.Documents;
    }

    private async Task CreateAliasForAllIndicesAsync()
    {
        EnsureElasticClient(_indexName);
        var response = await _elasticsearchClient.Indices
            .ExistsAliasAsync(new ExistsAliasRequest(new Names(new List<string> { _alias })));

        if (response.Exists)
        {
            await _elasticsearchClient.Indices
                .DeleteAliasAsync(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
        }

        var responseCreateIndex = await _elasticsearchClient.Indices
            .PutAliasAsync(new PutAliasRequest(Indices.Parse($"{_alias}-*"), _alias));

        if (!responseCreateIndex.IsValidResponse)
        {
            responseCreateIndex.TryGetOriginalException(out var ex);
            throw ex;
        }
    }

    private async Task CreateAlias()
    {
        EnsureElasticClient(_indexName);
        if (_options.Value.AmountOfPreviousIndicesUsedInAlias > 0)
        {
            await CreateAliasForLastNIndicesAsync(_options.Value.AmountOfPreviousIndicesUsedInAlias);
        }
        else
        {
            await CreateAliasForAllIndicesAsync();
        }
    }

    private async Task CreateAliasForLastNIndicesAsync(int amount)
    {
        EnsureElasticClient(_indexName);
        var responseCatIndices = await _elasticsearchClient
            .Indices.GetAsync(new GetIndexRequest(Indices.Parse($"{_alias}-*")));
        var records = responseCatIndices.Indices.ToList();

        var indicesToAddToAlias = new List<string>();

        for (int i = amount; i > 0; i--)
        {
            if (_options.Value.IndexPerMonth)
            {
                var indexName = $"{_alias}-{DateTime.UtcNow.AddMonths(-i + 1):yyyy-MM}";
                if (records.Exists(t => t.Key == indexName))
                {
                    indicesToAddToAlias.Add(indexName);
                }
            }
            else
            {
                var indexName = $"{_alias}-{DateTime.UtcNow.AddDays(-i + 1):yyyy-MM-dd}";
                if (records.Exists(t => t.Key == indexName))
                {
                    indicesToAddToAlias.Add(indexName);
                }
            }
        }

        var response = await _elasticsearchClient.Indices
            .ExistsAliasAsync(new ExistsAliasRequest(new Names(new List<string> { _alias })));

        if (response.Exists)
        {
            await _elasticsearchClient.Indices
                .DeleteAliasAsync(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
        }

        Indices multipleIndicesFromStringArray = indicesToAddToAlias.ToArray();

        var responseCreateIndex = await _elasticsearchClient.Indices
            .PutAliasAsync(new PutAliasRequest(multipleIndicesFromStringArray, _alias));

        if (!responseCreateIndex.IsValidResponse)
        {
            var res = responseCreateIndex.TryGetOriginalException(out var ex);
            throw ex;
        }
    }

    private static DateTime aliasUpdated = DateTime.UtcNow.AddYears(-50);

    private async Task EnsureAlias()
    {
        if (_options.Value.IndexPerMonth)
        {
            if (aliasUpdated.Date < DateTime.UtcNow.AddMonths(-1).Date)
            {
                aliasUpdated = DateTime.UtcNow;
                await CreateAlias();
            }
        }
        else if (aliasUpdated.Date < DateTime.UtcNow.AddDays(-1).Date)
        {
            aliasUpdated = DateTime.UtcNow;
            await CreateAlias();
        }

    }
}
