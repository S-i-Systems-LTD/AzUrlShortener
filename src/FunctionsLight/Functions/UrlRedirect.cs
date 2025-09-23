using Cloud5mins.ShortenerTools.Core.Services;
using Cloud5mins.ShortenerTools.Core.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cloud5mins.ShortenerTools.Core.Service;
using Azure.Data.Tables;

namespace Cloud5mins.ShortenerTools.Functions
{
    public class UrlRedirect
    {
        private readonly ILogger _logger;
        private TableServiceClient _tblClient;

        public UrlRedirect(ILoggerFactory loggerFactory, TableServiceClient tblClient)
        {
            _logger = loggerFactory.CreateLogger<UrlRedirect>();
            // _logger.LogDebug("UrlRedirect in constructor");
            _tblClient = tblClient;
        }

        [Function("UrlRedirect")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shorturl/{shortUrl?}")]
    HttpRequestData req,
    string? shortUrl,
    ExecutionContext context)
{
    _logger.LogDebug("Function reached");

    try
    {
        _logger.LogDebug($"Received shortUrl: {shortUrl}");

        // Initialize services
        UrlServices UrlServices = new UrlServices(_logger, new AzStrorageTablesService(_tblClient));
        _logger.LogDebug("Services created");

        // Attempt to get redirect URL
        string redirectUrl = await UrlServices.Redirect(shortUrl);
        _logger.LogDebug($"Redirect URL obtained: {redirectUrl}");

        // Create redirect response
        var res = req.CreateResponse(HttpStatusCode.Redirect);
        res.Headers.Add("Location", redirectUrl);
        _logger.LogDebug("Redirect response created");

        return res;
    }
    catch (Exception ex)
    {
        _logger.LogError($"UrlRedirect failed: {ex}");

        // Return 500 with exception message for debugging
        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
        await errorResponse.WriteStringAsync($"Error in UrlRedirect: {ex.Message}");
        return errorResponse;
    }
}

    }
}
