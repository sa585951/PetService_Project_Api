using Microsoft.Extensions.Caching.Memory;

namespace PetService_Project_Api.Service
{
    public class MemoryCacheService:ICodeService
    {
        private readonly IMemoryCache _cache;
        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public Task SetCodeAsync(string email, string code, TimeSpan expiration)
        {
            _cache.Set(email, code, expiration);
            return Task.CompletedTask;
        }

        public Task<string> GetCodeAsync(string email)
        {
            _cache.TryGetValue(email, out string code);
            return Task.FromResult(code);
        }
    }
}
