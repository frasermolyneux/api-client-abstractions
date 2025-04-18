using Microsoft.Extensions.Caching.Memory;
using System;
using Xunit;

namespace MxIO.ApiClient
{
    public class TestMemoryCacheTests : IDisposable
    {
        private readonly TestMemoryCache memoryCache;

        public TestMemoryCacheTests()
        {
            memoryCache = new TestMemoryCache();
        }

        public void Dispose()
        {
            memoryCache.Dispose();
        }

        [Fact]
        public void TryGetValue_WithNonExistentKey_ReturnsFalse()
        {
            // Act
            bool result = memoryCache.TryGetValue("non-existent-key", out object? value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void Set_ThenTryGetValue_ShouldReturnTrueAndValue()
        {
            // Arrange
            string key = "test-key";
            string testValue = "test-value";

            // Act
            memoryCache.Set(key, testValue);
            bool result = memoryCache.TryGetValue(key, out object? retrievedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(testValue, retrievedValue);
        }

        [Fact]
        public void Set_WithValueAndOptions_StoresValueRegardlessOfOptions()
        {
            // Arrange
            string key = "expiring-key";
            string testValue = "expiring-value";
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMilliseconds(50));

            // Act
            memoryCache.Set(key, testValue, options);
            bool initialResult = memoryCache.TryGetValue(key, out object? initialValue);

            // Wait for what would be expiration in a real cache
            System.Threading.Thread.Sleep(100);
            // In TestMemoryCache, expiration is not implemented, so value should still be there
            bool afterWaitResult = memoryCache.TryGetValue(key, out object? afterWaitValue);

            // Assert
            Assert.True(initialResult);
            Assert.Equal(testValue, initialValue);
            // TestMemoryCache doesn't implement expiration, so the item is still in the cache
            Assert.True(afterWaitResult);
            Assert.Equal(testValue, afterWaitValue);
        }

        [Fact]
        public void CreateEntry_ShouldCreateCacheEntry()
        {
            // Arrange
            string key = "entry-key";
            string testValue = "entry-value";

            // Act
            ICacheEntry entry = memoryCache.CreateEntry(key);
            entry.Value = testValue;
            entry.Dispose(); // Commits the entry

            bool result = memoryCache.TryGetValue(key, out object? retrievedValue);

            // Assert
            Assert.True(result);
            Assert.Equal(testValue, retrievedValue);
        }

        [Fact]
        public void Remove_ShouldRemoveEntry()
        {
            // Arrange
            string key = "remove-key";
            string testValue = "remove-value";
            memoryCache.Set(key, testValue);

            // Initial verification
            bool initialResult = memoryCache.TryGetValue(key, out object? _);
            Assert.True(initialResult);

            // Act
            memoryCache.Remove(key);

            // Assert
            bool afterRemovalResult = memoryCache.TryGetValue(key, out object? afterRemovalValue);
            Assert.False(afterRemovalResult);
            Assert.Null(afterRemovalValue);
        }
    }
}