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
        private string indexName = $"auditlog-{DateTime.UtcNow.ToString("yyyy-MM-dd")}";
        private const string alias = "auditlog";

        public ElasticClient _elasticClient { get; }

        public AuditTrailProvider()
        {
            var pool = new StaticConnectionPool(new List<Uri> { new Uri("http://localhost:9200") });
            var connectionSettings = new ConnectionSettings(
                pool,
                new HttpConnection(),
                new SerializerFactory((jsonSettings, nestSettings) => jsonSettings.Converters.Add(new StringEnumConverter())));

            _elasticClient = new ElasticClient(connectionSettings);
        }

        public void AddLog(AuditTrailLog auditTrailLog)
        {
            var index = new IndexName()
            {
                Name = indexName
            };

            var indexRequest = new IndexRequest<AuditTrailLog>(auditTrailLog, index);

            var response = _elasticClient.Index(indexRequest);
            if (!response.IsValid)
            {
                throw new ElasticsearchClientException("Add auditlog disaster!");
            }
        }

        public long Count(string filter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditTrailLog> SelectItems(string filter, AuditTrailPaging auditTrailPaging = null)
        {
            throw new NotImplementedException();
        }

    }
}
