using AuditTrail.Model;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AuditTrail
{
    public class AuditTrailProvider<T> : IAuditTrailProvider<T> where T : class
    {
        private const string _alias = "auditlog";
        private string _indexName = $"{_alias}-{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
        private static Field TimestampField = new Field("timestamp");
        private readonly IOptions<AuditTrailOptions> _options;

        private ElasticClient _elasticClient { get; }

        public AuditTrailProvider(
           IOptions<AuditTrailOptions> auditTrailOptions)
        {
            _options = auditTrailOptions ?? throw new ArgumentNullException(nameof(auditTrailOptions));

            if(_options.Value.IndexPerMonth)
            {
                _indexName = $"{_alias}-{DateTime.UtcNow.ToString("yyyy-MM")}";
            }

            var pool = new StaticConnectionPool(new List<Uri> { new Uri("http://localhost:9200") });

            var connectionSettings = new ConnectionSettings(pool)
                    .DefaultMappingFor<T>(m => m
                    .IndexName(_indexName));

           
                //new HttpConnection(),
                //new SerializerFactory((jsonSettings, nestSettings) => jsonSettings.Converters.Add(new StringEnumConverter())))
              //.DisableDirectStreaming();

            _elasticClient = new ElasticClient(connectionSettings);
        }

        public void AddLog(T auditTrailLog)
        {
            var indexRequest = new IndexRequest<T>(auditTrailLog);

            var response = _elasticClient.Index(indexRequest);
            if (!response.IsValid)
            {
                throw new ElasticsearchClientException("Add auditlog disaster!");
            }
        }

        public long Count(string filter = "*")
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

            var searchResponse = _elasticClient.Search<AuditTrailLog>(searchRequest);

            return searchResponse.Total;
        }

        public IEnumerable<T> QueryAuditLogs(string filter = "*", AuditTrailPaging auditTrailPaging = null)
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

            var searchResponse = _elasticClient.Search<T>(searchRequest);

            return searchResponse.Documents;
        }

        private void CreateAliasForAllIndices()
        {
            var response = _elasticClient.Indices.AliasExists(new AliasExistsRequest(new Names(new List<string> { _alias })));
            //if (!response.IsValid)
            //{
            //    throw response.OriginalException;
            //}

            if (response.Exists)
            {
                _elasticClient.Indices.DeleteAlias(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
            }

            var responseCreateIndex = _elasticClient.Indices.PutAlias(new PutAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
            if (!responseCreateIndex.IsValid)
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
            var responseCatIndices = _elasticClient.Cat.Indices(new CatIndicesRequest(Indices.Parse($"{_alias}-*")));
            var records = responseCatIndices.Records.ToList();
            List<string> indicesToAddToAlias = new List<string>();
            for(int i = amount;i>0;i--)
            {
                if (_options.Value.IndexPerMonth)
                {
                    var indexName = $"{_alias}-{DateTime.UtcNow.AddMonths(-i + 1).ToString("yyyy-MM")}";
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

            var response = _elasticClient.Indices.AliasExists(new AliasExistsRequest(new Names(new List<string> { _alias })));
            //if (!response.IsValid)
            //{
            //    throw response.OriginalException;
            //}

            if (response.Exists)
            {
                _elasticClient.Indices.DeleteAlias(new DeleteAliasRequest(Indices.Parse($"{_alias}-*"), _alias));
            }

            Indices multipleIndicesFromStringArray = indicesToAddToAlias.ToArray();
            var responseCreateIndex = _elasticClient.Indices.PutAlias(new PutAliasRequest(multipleIndicesFromStringArray, _alias));
            if (!responseCreateIndex.IsValid)
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
}
