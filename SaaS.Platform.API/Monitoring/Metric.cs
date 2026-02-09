using Prometheus;

namespace SaaS.Platform.API.Monitoring
{
    public static class MetricsRegistry
    {
        // Toplam istek sayısı
        public static readonly Counter HttpRequestsTotal =
            Prometheus.Metrics.CreateCounter(
                "saas_http_requests_total",
                "Total number of HTTP requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "tenant", "method", "endpoint", "status" }
                });

        // İstek süresi
        public static readonly Histogram HttpRequestDuration =
            Prometheus.Metrics.CreateHistogram(
                "saas_http_request_duration_seconds",
                "HTTP request duration",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "tenant", "endpoint" },
                    Buckets = Histogram.ExponentialBuckets(
                        start: 0.05,   // 50ms
                        factor: 2,
                        count: 8
                    )
                });
    }
}
