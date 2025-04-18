﻿namespace MxIO.ApiClient;

public interface IApiTokenProvider
{
    Task<string> GetAccessToken(string audience, CancellationToken cancellationToken = default);
}