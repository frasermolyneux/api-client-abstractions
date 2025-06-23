# API Client Abstractions

This library provides abstractions for API responses and common API related models.

## Usage

The abstractions in this library help with standardizing API responses and models across different API clients and API implementations.

### ApiResponse<T>

The `ApiResponse<T>` class is a wrapper for API responses that includes:

- HTTP Status Code
- Data payload
- Error information
- Pagination data
- Metadata

### ApiError

The `ApiError` class provides a standardized format for API errors, including:

- Error code
- Message
- Target field (optional)
- Details (optional nested errors)

### ApiPagination

The `ApiPagination` class provides standardized pagination information including:

- Total count of records
- Current page
- Page size
- Next/previous page URLs

### CollectionModel<T>

The `CollectionModel<T>` class provides a standardized container for collections of resources, including:

- Collection of items
- Total count of items (before pagination)
- Filtered count of items (after filters applied)
- Metadata

### FilterOptions

The `FilterOptions` class provides standardized options for filtering API responses, including:

- Skip (number of records to skip)
- Top (number of records to take)
- Filter expressions
- Sort expressions
