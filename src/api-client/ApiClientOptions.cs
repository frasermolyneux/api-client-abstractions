namespace MxIO.ApiClient
{
    public class ApiClientOptions
    {
        private string _primaryApiKey = null!;

        public string BaseUrl { get; set; }

        [Obsolete("This is now obsolete, please use PrimaryApiKey instead")]
        public string ApiKey
        {
            get
            {
                return _primaryApiKey;
            }
            set
            {
                _primaryApiKey = value;
            }
        }

        public string PrimaryApiKey
        {
            get
            {
                return _primaryApiKey;
            }
            set
            {
                _primaryApiKey = value;
            }
        }
        public string SecondaryApiKey { get; set; }

        public string ApiAudience { get; set; }

        public string? ApiPathPrefix { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ApiClientOptions()
        {

        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
