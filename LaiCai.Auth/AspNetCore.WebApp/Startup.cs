using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CacheManager.Core;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

namespace AspNetCore.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                // adding cache.json which contains cachemanager configuration(s)
                .AddJsonFile("cache.json", optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSwaggerGen(null);

            // using the new overload which adds a singleton of the configuration to services and the configure method to add logging
            // TODO: still not 100% happy with the logging part
            services.AddCacheManagerConfiguration(Configuration, cfg => cfg.WithMicrosoftLogging(services));

            //// uses a refined configurastion (this will not log, as we added the MS Logger only to the configuration above
            //services.AddCacheManager<int>(Configuration, configure: builder => builder.WithJsonSerializer());

            //// creates a completely new configuration for this instance (also not logging)
            //services.AddCacheManager<DateTime>(inline => inline.WithDictionaryHandle());

            // any other type will be this. Configurastion used will be the one defined by AddCacheManagerConfiguration earlier.
            services.AddCacheManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
           
            app.UseMvc();
        }
    }
}
