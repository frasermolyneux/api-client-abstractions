namespace MxIO.ApiClient;

public interface IApiTokenProvider
{
    Task<string> GetAccessToken();
}