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
        private const string AppNameFallback = "Serilog POC - Application Test";
        public static Dictionary<string, string> EnvironmentVariablesToLog { get; set; } =
            new Dictionary<string, string>
            {
                    {AspNetCoreEnvVarName, "Type"},
                    {AppNameEnvVarName, "ApplicationName"}
            };

        public static void Main(string[] args)
        {

            //L� as Info de defini��es do arquivo configura��es.
            var configurations = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            EnvironmentVariablesEnricher.RegisterFallback(
                new EnvironmentVariablesEnricher.EnvironmentVariableValueFallback(AppNameEnvVarName)
                {
                    ValueValidator = string.IsNullOrWhiteSpace,
                    FallbackValueGenerator = () => AppNameFallback
                });


            // Usamos uma instancia de LoggerConfiguration e a partir das informa��es,
            // obtidas no arquivo appsettings criamos um logger usando os SINKS(coletores) e ENRICHERS(Enrriquecedores)
            // e as demais defini��es.
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

                //Defini��o de uso como provedor de Log da Aplica��o    
                .UseSerilog()

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
