using AuditTrail.Model;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace AuditTrail
{
    public class AuditTrailProvider : IAuditTrailProvider
    {
        private string _indexName = $"auditlog-{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
        private const string _alias = "auditlog";
        private static Field TimestampField = new Field("timestamp");
        private ElasticClient _elasticClient { get; }

        public AuditTrailProvider()
        {
            var pool = new StaticConnectionPool(new List<Uri> { new Uri("http://localhost:9200") });
            var connectionSettings = new ConnectionSettings(
                pool,
                new HttpConnection(),
                new SerializerFactory((jsonSettings, nestSettings) => jsonSettings.Converters.Add(new StringEnumConverter())))
              .DisableDirectStreaming();

            _elasticClient = new ElasticClient(connectionSettings);
        }

        public void AddLog(AuditTrailLog auditTrailLog)
        {
            var index = new IndexName()
            {
                Name = _indexName
            };

            var indexRequest = new IndexRequest<AuditTrailLog>(auditTrailLog, index);

            var response = _elasticClient.Index(indexRequest);
            if (!response.IsValid)
            {
                throw new ElasticsearchClientException("Add auditlog disaster!");
            }
        }

        public long Count(string filter = "*")
        {
            EnsureAlias();
            var searchRequest = new SearchRequest<AuditTrailLog>(Indices.Parse(_alias))
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
                        new SortField { Field = TimestampField, Order = SortOrder.Descending }
                    }
            };

            var searchResponse = _elasticClient.Search<AuditTrailLog>(searchRequest);

            return searchResponse.Total;
        }

        public IEnumerable<AuditTrailLog> SelectItems(string filter, AuditTrailPaging auditTrailPaging = null)
        {
            EnsureAlias();
            throw new NotImplementedException();
        }

        private void CreateAliasForAllIndices()
        {
            var response = _elasticClient.AliasExists(new AliasExistsRequest(new Names(new List<string> { _alias })));
            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            if (response.Exists)
            {
                _elasticClient.DeleteAlias(new DeleteAliasRequest(Indices.Parse("auditlog-*"), _alias));
            }

            var responseCreateIndex = _elasticClient.PutAlias(new PutAliasRequest(Indices.Parse("auditlog-*"), _alias));

            if (!responseCreateIndex.IsValid)
            {
                throw response.OriginalException;
            }
        }

        private static DateTime aliasUpdated = DateTime.UtcNow.AddYears(-50);
        private void EnsureAlias()
        {
            if (aliasUpdated.Date < DateTime.UtcNow.AddDays(-1).Date)
            {
                aliasUpdated = DateTime.UtcNow;
                CreateAliasForAllIndices();
            }
        }
    }
}
