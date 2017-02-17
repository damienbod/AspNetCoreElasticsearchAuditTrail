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
            CreateIndex(_elasticClient, indexName);
        }

        public void AddLog(AuditTrailLog auditTrailLog)
        {
            throw new NotImplementedException();
        }

        public long Count(string filter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuditTrailLog> SelectItems(string filter, AuditTrailPaging auditTrailPaging = null)
        {
            throw new NotImplementedException();
        }

        private void CreateIndex(ElasticClient elasticClient, string indexName)
        {
            throw new NotImplementedException();
        }

    }
}
