using FindFilesViaWindowsSearch.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindFilesViaWindowsSearch.Configuration
{
    public static class FileProcessingConfigExtensions
    {
        public static IServiceCollection AddFileProcessingConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("FileProcessingConfig");
            services.Configure<FileProcessingConfigModel>(configSection);
            
            // Register FileProcessingConfig as a singleton that can be injected directly
            services.AddSingleton(provider =>
            {
                var config = configSection.Get<FileProcessingConfigModel>();
                if (config == null)
                {
                    throw new InvalidOperationException("FileProcessingConfig section is missing in appsettings.json");
                }
                return config;
            });

            return services;
        }
    }
}
