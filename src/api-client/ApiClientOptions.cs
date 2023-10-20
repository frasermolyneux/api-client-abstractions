namespace MxIO.ApiClient
{
    public class ApiClientOptions
    {
        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }

        public string ApiAudience { get; set; }

        public string? ApiPathPrefix { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ApiClientOptions()
        {

        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
