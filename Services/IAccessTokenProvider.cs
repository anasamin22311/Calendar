using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
    Task<string> GetRefreshTokenAsync();
}

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
    }

    public async Task<string> GetRefreshTokenAsync() 
    {
        return await _httpContextAccessor.HttpContext.GetTokenAsync("refresh_token");
    }
}