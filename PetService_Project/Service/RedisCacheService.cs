using StackExchange.Redis;

namespace PetService_Project_Api.Service
{
    public class RedisCacheService:ICodeService
    {
        private readonly IDatabase _redisDb;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        public async Task SetCodeAsync(string email, string code, TimeSpan expiration)
        {
            await _redisDb.StringSetAsync(email, code, expiration);
        }

        public async Task<string> GetCodeAsync(string email)
        {
            return await _redisDb.StringGetAsync(email);
        }
    }
}
