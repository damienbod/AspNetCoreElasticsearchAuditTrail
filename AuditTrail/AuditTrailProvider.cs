using AuditTrail.Model;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Transport;
using System.Threading.Tasks;

namespace AuditTrail;

public class AuditTrailProvider<T> : IAuditTrailProvider<T> where T : class
{
    private const string _alias = "auditlog";
    private string _indexName = $"{_alias}-{DateTime.UtcNow:yyyy-MM-dd}";
    private static Field TimestampField = new("timestamp");
    private readonly IOptions<AuditTrailOptions> _options;

    private ElasticsearchClient _elasticsearchClient { get; }

    public AuditTrailProvider(IOptions<AuditTrailOptions> auditTrailOptions)
    {
        _options = auditTrailOptions ?? throw new ArgumentNullException(nameof(auditTrailOptions));

        if(_options.Value.IndexPerMonth)
        {
            _indexName = $"{_alias}-{DateTime.UtcNow:yyyy-MM}";
        }

        var settings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .Authentication(new BasicAuthentication("elastic", "Password1!"))
            .DefaultMappingFor<T>(m => m.IndexName(_indexName));

        var _elasticsearchClient = new ElasticsearchClient(settings);
    }

    public async Task AddLog(T auditTrailLog)
    {
        var indexRequest = new IndexRequest<T>(auditTrailLog);

        var response = await _elasticsearchClient.IndexAsync(indexRequest);
        if (!response.IsValidResponse)
        {
            throw new ArgumentException("Add auditlog disaster!");
        }
    }

    public async Task<long> Count(string filter = "*")
    {
        EnsureAlias();
        var searchRequest = new SearchRequest<T>(Indices.Parse(_alias))
        {
            Size = 0,
            Query = new QueryContainer(
                new SimpleQueryStringQuery
                {
                    Query = filter
                }
            ),
            Sort = new List<ISort>
                {
                    new FieldSort { Field = TimestampField, Order = SortOrder.Descending }
                }
        };

        var searchResponse = await _elasticsearchClient.SearchAsync<AuditTrailLog>(searchRequest);

        return searchResponse.Total;
    }

    public async Task<IEnumerable<T>> QueryAuditLogs(string filter = "*", AuditTrailPaging auditTrailPaging = null)
    {
        var from = 0;
        var size = 10;
        EnsureAlias();
        if(auditTrailPaging != null)
        {
            from = auditTrailPaging.Skip;
            size = auditTrailPaging.Size;
            if(size > 1000)
            {
                // max limit 1000 items
                size = 1000;
            }
        }
        var searchRequest = new SearchRequest<T>(Indices.Parse(_alias))
        {
            Size = size,
            From = from,
            Query = new QueryContainer(
                new SimpleQueryStringQuery
                {
                    Query = filter
                }
            ),
            Sort = new List<ISort>
                {
                    new FieldSort { Field = TimestampField, Order = SortOrder.Descending }
                }
        };

        var searchResponse = await _elasticsearchClient.SearchAsync<T>(searchRequest);

        return searchResponse.Documents;
    }

    private void CreateAliasForAllIndices()
    {
        var response = _elasticsearchClient.Indices.AliasExists(new AliasExistsRequest(new Names(new List<string> { _alias })));

        if (response.Exists)
        {
            _elasticsearchClient.Indices.DeleteAlias(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
        }

        var responseCreateIndex = _elasticsearchClient.Indices.PutAlias(new PutAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
        if (!responseCreateIndex.IsValidResponse)
        {
            throw response.OriginalException;
        }
    }

    private void CreateAlias()
    {
        if (_options.Value.AmountOfPreviousIndicesUsedInAlias > 0)
        {
            CreateAliasForLastNIndices(_options.Value.AmountOfPreviousIndicesUsedInAlias);
        }
        else
        {
            CreateAliasForAllIndices();
        }
    }

    private void CreateAliasForLastNIndices(int amount)
    {
        var responseCatIndices = _elasticsearchClient.Cat.Indices(new CatIndicesRequest(Indices.Parse($"{_alias}-*")));
        var records = responseCatIndices.Records.ToList();
        List<string> indicesToAddToAlias = new List<string>();
        for(int i = amount;i>0;i--)
        {
            if (_options.Value.IndexPerMonth)
            {
                var indexName = $"{_alias}-{DateTime.UtcNow.AddMonths(-i + 1):yyyy-MM}";
                if(records.Exists(t => t.Index == indexName))
                {
                    indicesToAddToAlias.Add(indexName);
                }
            }
            else
            {
                var indexName = $"{_alias}-{DateTime.UtcNow.AddDays(-i + 1).ToString("yyyy-MM-dd")}";                   
                if (records.Exists(t => t.Index == indexName))
                {
                    indicesToAddToAlias.Add(indexName);
                }
            }
        }

        var response = _elasticsearchClient.Indices.AliasExists(new AliasExistsRequest(new Names(new List<string> { _alias })));

        if (response.Exists)
        {
            _elasticsearchClient.Indices.DeleteAlias(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
        }

        Indices multipleIndicesFromStringArray = indicesToAddToAlias.ToArray();
        var responseCreateIndex = _elasticsearchClient.Indices.PutAlias(new PutAliasRequest(multipleIndicesFromStringArray, _alias));
        if (!responseCreateIndex.IsValidResponse)
        {
            throw responseCreateIndex.OriginalException;
        }
    }

    private static DateTime aliasUpdated = DateTime.UtcNow.AddYears(-50);

    private void EnsureAlias()
    {
        if (_options.Value.IndexPerMonth)
        {
            if (aliasUpdated.Date < DateTime.UtcNow.AddMonths(-1).Date)
            {
                aliasUpdated = DateTime.UtcNow;
                CreateAlias();
            }
        }
        else
        {
            if (aliasUpdated.Date < DateTime.UtcNow.AddDays(-1).Date)
            {
                aliasUpdated = DateTime.UtcNow;
                CreateAlias();
            }
        }           
    }
}
