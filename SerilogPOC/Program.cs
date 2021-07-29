using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogPOC.LogEnrichers;
using System;
using System.Collections.Generic;

namespace SerilogPOC
{
    public class Program
    {
        private const string AspNetCoreEnvVarName = "ASPNETCORE_ENVIRONMENT";
        private const string AppNameEnvVarName = "APPLICATION_NAME";
        private const string AppNameFallback = "Serilog POC - Application Test...";
        public static Dictionary<string, string> EnvironmentVariablesToLog { get; set; } =
            new Dictionary<string, string>
            {
                    {AspNetCoreEnvVarName, "Type"},
                    {AppNameEnvVarName, "ApplicationName"}
            };

        public static void Main(string[] args)
        {

            //Lê as Info de definições do arquivo configurações.
            var configurations = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            EnvironmentVariablesEnricher.RegisterFallback(
                new EnvironmentVariablesEnricher.EnvironmentVariableValueFallback(AppNameEnvVarName)
                {
                    ValueValidator = string.IsNullOrWhiteSpace,
                    FallbackValueGenerator = () => AppNameFallback
                });


            // Usamos uma instancia de LoggerConfiguration e a partir das informações,
            // obtidas no arquivo appsettings criamos um logger usando os SINKS(coletores) e ENRICHERS(Enrriquecedores)
            // e as demais definições.
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configurations)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentVariables(EnvironmentVariablesToLog)
                .WriteTo.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();



            try
            {
                Log.Information("Starting API Application....");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error CRITICAL...");
            }
            finally
            {
                Log.CloseAndFlush();
            }



        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                //Definição de uso como provedor de Log da Aplicação    
                .UseSerilog()

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
