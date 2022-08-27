using System.Reflection;
using Crowdin.Api;
using Kook.Bot.Crowdin.Attributes;
using Kook.Bot.Crowdin.Configurations;
using Kook.Bot.Crowdin.Data;
using Kook.Bot.Crowdin.Data.Repositories;
using Kook.Bot.Crowdin.Data.Services;
using Kook.Bot.Crowdin.Extensions;
using Kook.Bot.Crowdin.Interfaces;
using Kook.Bot.Crowdin.ScheduledServices;
using Kook.Commands;
using Kook.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Kook.Bot.Crowdin;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        PreTask();
        
        Log.Information("Starting KOOK Crowdin Bot");
        
        using IHost host = CreateHostBuilder(args).Build();
        IServiceProvider service = host.Services;
        service.GetRequiredService<KookBotExtension>();
        CrowdinBotDbContext dbContext = service.GetRequiredService<CrowdinBotDbContext>();
        await dbContext.Database.MigrateAsync();

        await host.RunAsync();
    }

    private static void PreTask()
    {
        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "db")))
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "db"));
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(ConfigureServices)
            .UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
#if DEBUG
                    .MinimumLevel.Debug()
#endif
                    .WriteTo.Console();
            });
        return hostBuilder;
    }

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="hostContext"></param>
    /// <param name="services"></param>
    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        // 根配置
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
            .Build();

        // // Get all instances which implement IConfigurations
        // IEnumerable<Type> configurations = Assembly.GetEntryAssembly()!
        //     .GetTypes()
        //     .Where(x => x.GetCustomAttributes<ConfigurationsAttribute>().Any());
        // foreach (Type configurationType in configurations)
        // {
        //     object instance = Activator.CreateInstance(configurationType);
        //     configurationRoot.GetSection(configurationType.Name.Replace("Configurations", string.Empty)).Bind(instance);
        //     services.TryAddSingleton(ServiceDescriptor.Singleton(configurationType, instance!));
        // }
        
        services.AddSingleton(configurationRoot.GetSection("Kook").Get<KookConfigurations>());
        services.AddSingleton(configurationRoot.GetSection("Crowdin").Get<CrowdinConfigurations>());

        // 服务配置
        services
            .AddSingleton(_ => new KookSocketClient(new KookSocketConfig()
            {
                AlwaysDownloadUsers = true,
#if DEBUG
                LogLevel = LogSeverity.Debug
#endif
            }))
            .AddSingleton<KookBotExtension>()
            .AddHostedService(p => p.GetRequiredService<KookBotExtension>())
            .AddSingleton<CommandService>()
            .AddSingleton<KookCommandHandlingExtension>()

            .AddSingleton(p => new CrowdinApiClient(new CrowdinCredentials
            {
                AccessToken = p.GetRequiredService<CrowdinConfigurations>().Token
            }))

            .AddSingleton<CrowdinExtension>()

            // 数据库对象
            .AddDbContext<CrowdinBotDbContext>(optionsBuilder => optionsBuilder
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                .UseSqlite(configurationRoot.GetConnectionString("CrowdinBotDbConnection"),
                    builder => builder.MigrationsAssembly("Kook.Bot.Crowdin.Migrations")))
            
            .AddScoped<ITermService, TermRepository>()

            .AddHttpClient();
        
        // 定时服务
        // IEnumerable<Type> scheduledServices = Assembly.GetEntryAssembly()!
        //     .GetTypes()
        //     .Where(x => x.BaseType == typeof(ScheduledServiceBase));
        // foreach (Type type in scheduledServices)
        // {
        //     services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), type));
        // }
        
        services.AddSingleton<CrowdinTermsAutoRefreshService>()
            .AddHostedService(p => p.GetRequiredService<CrowdinTermsAutoRefreshService>());
    }
}