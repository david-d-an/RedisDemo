using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false);

            IConfiguration config = builder.Build();
            var testObj1 = config.GetSection("TestObj").Get<TestClass>();
            
            var serviceProvider = new ServiceCollection()
                .AddStackExchangeRedisCache(opt => {
                    opt.Configuration = config.GetSection("ConnectionString").Value;
                    // opt.Configuration = config.GetValue<string>("ConnectionString");
                })
                .AddLogging(builder => {
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information);
                })
                // .Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>())
                .BuildServiceProvider();

            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(120)); 

            var logger = serviceProvider
                        .GetService<ILoggerFactory>()
                        .CreateLogger<Program>();
            logger.LogInformation("Starting application");

            //do the actual work here
            var cache = serviceProvider
                        .GetService<IDistributedCache>();

            string testVal = "Value1";
            logger.LogInformation($"Adding value into Redis: {testVal}");
            await cache.SetStringAsync("K1", testVal, cacheEntryOptions);

            var cachedVal = await cache.GetStringAsync("K1");
            logger.LogInformation($"Value retrieved from Redis: {cachedVal}");
        }
    }
}
