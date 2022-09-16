using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;
using UNAHUR.MessageFun.Business;

namespace UNAHUR.MessageFun.Api
{
    public class Program
    {
        static bool? _isRunningInContainer;
        public static bool IsRunningInContainer =>
            _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inDocker) && inDocker;

        public static void Main(string[] args)
        {
            try
            {
                Serilog.Debugging.SelfLog.Enable(Console.Error);
                Log.Information("Building host..");
                var host = CreateHostBuilder(args) .Build();
                


                Log.Information("Building host OK");
                Log.Information("Running host host...");
                host.Run();
            }
            catch (Exception ex)
            {
                Console.Error.Write($"Faltal error {ex.Message}");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }


        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .UseSerilog((ctx, logConfig) =>
                {
                    logConfig.ReadFrom.Configuration(ctx.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}