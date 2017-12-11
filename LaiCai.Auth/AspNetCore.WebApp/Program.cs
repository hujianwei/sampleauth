using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;


namespace AspNetCore.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 //.AddJsonFile("hosting.json", optional: true)
                 .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:60000", "http://*:60001")
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build(); 
            host.Run();
        }
    }
}
