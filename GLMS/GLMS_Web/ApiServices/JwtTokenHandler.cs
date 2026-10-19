namespace GLMS_Web.ApiServices;
//Code Attribution for automatically attaching JWT Bearer tokens to outgoing API requests
//Link: https://stackoverflow.com/questions/62322402/how-to-add-jwt-bearer-token-to-all-requests-to-an-api

// This DelegatingHandler automatically attaches the JWT token stored in Session to every outgoing API request.
// This avoids manually adding the Authorization header inside every ApiService method.
public class JwtTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTokenHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieves JWT token from Session
        var token =  _httpContextAccessor.HttpContext?
            .Session
            .GetString("JwtToken");

        // Adds Authorization header if the token exists
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Continues the HTTP request pipeline
        return await base.SendAsync(request, cancellationToken);
    }
}