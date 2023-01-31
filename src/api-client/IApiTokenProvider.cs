namespace MX.ApiClient;

public interface IApiTokenProvider
{
    Task<string> GetAccessToken();
}