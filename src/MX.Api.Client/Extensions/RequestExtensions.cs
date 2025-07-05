using MX.Api.Abstractions;
using RestSharp;

namespace MX.Api.Client.Extensions;

/// <summary>
/// Extension methods for RestRequest to add common query parameters.
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Adds filter options as query parameters to the request.
    /// </summary>
    /// <param name="request">The REST request.</param>
    /// <param name="filterOptions">The filter options.</param>
    /// <returns>The updated request.</returns>
    public static RestRequest AddFilterOptions(this RestRequest request, FilterOptions filterOptions)
    {
        if (filterOptions == null)
        {
            return request;
        }

        // Add filter expression
        if (!string.IsNullOrWhiteSpace(filterOptions.FilterExpression))
        {
            request.AddQueryParameter("$filter", filterOptions.FilterExpression);
        }

        // Add select fields
        if (filterOptions.Select != null && filterOptions.Select.Length > 0)
        {
            request.AddQueryParameter("$select", string.Join(",", filterOptions.Select));
        }

        // Add expand fields
        if (filterOptions.Expand != null && filterOptions.Expand.Length > 0)
        {
            request.AddQueryParameter("$expand", string.Join(",", filterOptions.Expand));
        }

        // Add order by
        if (!string.IsNullOrWhiteSpace(filterOptions.OrderBy))
        {
            request.AddQueryParameter("$orderby", filterOptions.OrderBy);
        }

        // Add pagination
        if (filterOptions.Skip > 0)
        {
            request.AddQueryParameter("$skip", filterOptions.Skip.ToString());
        }

        if (filterOptions.Top > 0)
        {
            request.AddQueryParameter("$top", filterOptions.Top.ToString());
        }

        // Add count flag
        if (filterOptions.Count)
        {
            request.AddQueryParameter("$count", "true");
        }

        return request;
    }
}
