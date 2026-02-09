using Prometheus;

namespace SaaS.Platform.API.Monitoring
{
    public static class CanaryMetrics
    {
        public static readonly Counter Requests =
            Metrics.CreateCounter(
                "canary_requests_total",
                "Total canary requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "type" }
                });

        public static readonly Counter Errors =
            Metrics.CreateCounter(
                "canary_errors_total",
                "Total canary errors",
                new CounterConfiguration
                {
                    LabelNames = new[] { "type" }
                });

        public static readonly Histogram Latency =
            Metrics.CreateHistogram(
                "canary_request_duration_seconds",
                "Canary request latency",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "type" }
                });
    }
}
