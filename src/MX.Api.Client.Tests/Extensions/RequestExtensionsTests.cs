using MX.Api.Abstractions;
using MX.Api.Client.Extensions;
using RestSharp;
using Xunit;

namespace MX.Api.Client.Tests;

public class RequestExtensionsTests
{
    [Fact]
    public void AddFilterOptions_WithNullFilterOptions_ReturnsUnmodifiedRequest()
    {
        // Arrange
        var request = new RestRequest();
        FilterOptions? filterOptions = null;

        // Act
        var result = request.AddFilterOptions(filterOptions!);

        // Assert
        Assert.Same(request, result);
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithFilterExpression_AddsFilterParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            FilterExpression = "name eq 'John'"
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$filter", parameter.Name);
        Assert.Equal("name eq 'John'", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithNullOrEmptyFilterExpression_DoesNotAddFilterParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            FilterExpression = string.Empty
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithSelectFields_AddsSelectParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Select = new[] { "name", "email", "age" }
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$select", parameter.Name);
        Assert.Equal("name,email,age", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithEmptySelectArray_DoesNotAddSelectParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Select = Array.Empty<string>()
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithExpandFields_AddsExpandParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Expand = new[] { "addresses", "orders" }
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$expand", parameter.Name);
        Assert.Equal("addresses,orders", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithOrderBy_AddsOrderByParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            OrderBy = "name asc"
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$orderby", parameter.Name);
        Assert.Equal("name asc", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithSkip_AddsSkipParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Skip = 20
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$skip", parameter.Name);
        Assert.Equal("20", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithZeroSkip_DoesNotAddSkipParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Skip = 0
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithTop_AddsTopParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Top = 50
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$top", parameter.Name);
        Assert.Equal("50", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithZeroTop_DoesNotAddTopParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Top = 0
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithCountTrue_AddsCountParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Count = true
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        var parameter = Assert.Single(request.Parameters);
        Assert.Equal("$count", parameter.Name);
        Assert.Equal("true", parameter.Value);
    }

    [Fact]
    public void AddFilterOptions_WithCountFalse_DoesNotAddCountParameter()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            Count = false
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Empty(request.Parameters);
    }

    [Fact]
    public void AddFilterOptions_WithAllOptions_AddsAllParameters()
    {
        // Arrange
        var request = new RestRequest();
        var filterOptions = new FilterOptions
        {
            FilterExpression = "name eq 'John'",
            Select = new[] { "name", "email" },
            Expand = new[] { "addresses" },
            OrderBy = "name asc",
            Skip = 20,
            Top = 50,
            Count = true
        };

        // Act
        request.AddFilterOptions(filterOptions);

        // Assert
        Assert.Equal(7, request.Parameters.Count);
        Assert.Contains(request.Parameters, p => p.Name == "$filter" && p.Value?.ToString() == "name eq 'John'");
        Assert.Contains(request.Parameters, p => p.Name == "$select" && p.Value?.ToString() == "name,email");
        Assert.Contains(request.Parameters, p => p.Name == "$expand" && p.Value?.ToString() == "addresses");
        Assert.Contains(request.Parameters, p => p.Name == "$orderby" && p.Value?.ToString() == "name asc");
        Assert.Contains(request.Parameters, p => p.Name == "$skip" && p.Value?.ToString() == "20");
        Assert.Contains(request.Parameters, p => p.Name == "$top" && p.Value?.ToString() == "50");
        Assert.Contains(request.Parameters, p => p.Name == "$count" && p.Value?.ToString() == "true");
    }
}
