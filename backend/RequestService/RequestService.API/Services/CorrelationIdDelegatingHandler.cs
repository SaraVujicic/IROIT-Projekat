using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RequestService.API.Services;

public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var correlationId = httpContext.Request.Headers[CorrelationIdHeader].ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                if (request.Headers.Contains(CorrelationIdHeader))
                {
                    request.Headers.Remove(CorrelationIdHeader);
                }
                request.Headers.Add(CorrelationIdHeader, correlationId);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
