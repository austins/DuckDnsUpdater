using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DuckDnsUpdater
{
    public class Worker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<Settings> _settings;

        public Worker(IOptions<Settings> settings, IHttpClientFactory httpClientFactory, ILogger<Worker> logger)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (_settings.Value.Subdomains == null || !_settings.Value.Subdomains.Any() ||
                string.IsNullOrWhiteSpace(_settings.Value.Token))
            {
                _logger.LogCritical(
                    "Required settings are not set. Please configure your Duck DNS subdomains and token, then restart this app.");

                return base.StopAsync(new CancellationToken(true));
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Duck DNS Updater running at: {time}", DateTimeOffset.Now);

                var httpClient = _httpClientFactory.CreateClient();

                // Build the domains and token parameters for the request.
                var requestUriBuilder = new UriBuilder("https://www.duckdns.org/update");
                var query = HttpUtility.ParseQueryString(requestUriBuilder.Query);
                query["domains"] = string.Join(",", _settings.Value.Subdomains);
                query["token"] = _settings.Value.Token;
                requestUriBuilder.Query = query.ToString();

                // Issue the GET request that will update the IP addresses on the subdomain(s).
                var response = await httpClient.GetAsync(requestUriBuilder.Uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync();
                    if (content.Result == "OK")
                        _logger.LogInformation("Successfully updated Duck DNS subdomains.");
                    else // "KO" is returned indicating an update failure.
                        _logger.LogError("Failed to update Duck DNS subdomains.");
                }
                else
                {
                    _logger.LogError("Duck DNS is offline. Status code: {0}", response.StatusCode);
                }

                // Update every 5 minutes.
                _logger.LogInformation("Update to Duck DNS will be made every 5 minutes.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}