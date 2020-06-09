using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Akrual.DDD.Utils.Internal.Extensions;
using Microsoft.Extensions.Configuration;

namespace Akrual.DDD.Utils.WebApi.Configuration
{
    public static class AppConfigurations
    {
        private static readonly ConcurrentDictionary<string, IConfigurationRoot> _configurationCache;

        static AppConfigurations()
        {
            _configurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();
        }

        public static IConfigurationRoot Get(string environmentName = null, bool addUserSecrets = false)
        {
            var cacheKey = environmentName + "#" + addUserSecrets;
            return _configurationCache.GetOrAdd(
                cacheKey,
                _ => BuildConfiguration(environmentName, addUserSecrets)
            );
        }

        private static IConfigurationRoot BuildConfiguration(string environmentName = null, bool addUserSecrets = false)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (!environmentName.IsNullOrEmptyOrWhiteSpace())
            {
                builder = builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            }

            builder = builder.AddEnvironmentVariables();

            if (addUserSecrets)
            {
                builder.AddUserSecrets(typeof(AppConfigurations).GetTypeInfo().Assembly);
            }

            return builder.Build();
        }
    }
}