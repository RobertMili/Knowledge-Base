using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MembershipAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .WithUrls(args)
                .UseStartup<Startup>();

    }

    static class Ext
    {
        public static IWebHostBuilder WithUrls(this IWebHostBuilder builder, string[] args)
        {
            if(args == null || args.Length == 0)
            {
                return builder;
            }
            
            var argSuffix = "--useurls=";
            var urls = args.Where(x => x.ToLower().StartsWith(argSuffix))
                           .Select(x => x.Replace(argSuffix, "").Split(' '))
                           .SelectMany(x => x);
            if (urls.Any())
            {
                return builder.UseUrls(urls.ToArray());
            }
            
            return builder;
        }
    }
}
