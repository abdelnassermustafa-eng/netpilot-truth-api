using TruthApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TruthApi.Services
{
    public class HealthService
    {
        private readonly ServiceConfig _config;
        private readonly ILogger<HealthService> _logger;

        public HealthService(
            IOptions<ServiceConfig> config,
            ILogger<HealthService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public object GetHealthStatus()
        {
            _logger.LogInformation("Health check requested for service {ServiceName}", _config.Name);

            return new
            {
                status = "healthy",
                service = _config.Name
            };
        }
    }
}
