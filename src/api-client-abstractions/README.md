# MxIO.ApiClient.Abstractions

A library containing common abstractions and DTOs for API clients in .NET applications. This package provides the fundamental data structures and interfaces for building API clients.

## Features

- Common API response DTOs 
- Collection DTOs for paginated results
- Base interfaces for implementing custom API clients

## Installation

```
dotnet add package MxIO.ApiClient.Abstractions
```

## Basic Usage

```csharp
// Example of using the ApiResponseDto
ApiResponseDto<MyData> response = await apiClient.GetDataAsync();

if (response.IsSuccess)
{
    MyData data = response.Data;
    // Process data
}
else
{
    // Handle error
    string errorMessage = response.Message;
}

// Example of working with collections
CollectionDto<MyItem> collection = await apiClient.GetCollectionAsync();
foreach (var item in collection.Items)
{
    // Process each item
}
```

## License

GPL-3.0-only