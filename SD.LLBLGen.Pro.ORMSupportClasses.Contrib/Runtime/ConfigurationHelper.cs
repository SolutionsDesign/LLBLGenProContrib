using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Configuration;

using SD.LLBLGen.Pro.DQE.SqlServer;

namespace SD.LLBLGen.Pro.ORMSupportClasses.Contrib {
	public static class ConfigurationHelper {
		
		public static void ConfigureFromAppSettings() {
			var environmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");

			if (string.IsNullOrEmpty(environmentName)) {
				environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			}

			if (string.IsNullOrEmpty(environmentName)) {
				environmentName = "Production";
			}

			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{environmentName}.json", true, true)
				.AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
				.Build();

			configureFromConfiguration(config);
		}

		public static void ConfigureFromJson(string configFileName) {
			var configDirectory = Directory.GetCurrentDirectory();
			ConfigureFromJson(configFileName, configDirectory);
		}

		public static void ConfigureFromJson(string configFileName, string configDirectory) {
			var configuration = new ConfigurationBuilder()
				.SetBasePath(configDirectory)
				.AddJsonFile(configFileName, true, true)
				.Build();

			configureFromConfiguration(configuration);
		}

		private static void configureFromConfiguration(IConfiguration configuration) {

			var llblGenSettings = configuration.GetSection("LLBLGen");

			var traceSettings =
				llblGenSettings.GetSection("Tracing");

			List<IConfigurationSection> traceSwitches =
				traceSettings.GetSection("Switches").GetChildren().ToList();

			var traceListeners =
				traceSettings.GetSection("Listeners");

			List<IConfigurationSection> connectionStrings =
				configuration.GetSection("ConnectionStrings").GetChildren().ToList();

			if (!connectionStrings.Any()) {
				connectionStrings =
					llblGenSettings.GetSection("ConnectionStrings").GetChildren().ToList();
			}

			List<IConfigurationSection> catalogNameOverwrites =
				llblGenSettings.GetSection("SqlServerCatalogNameOverwrites").GetChildren().ToList();

			var isTraceEnabled = false;

			foreach (var traceSwitch in traceSwitches.Where(s => s.Key != "SqlServerDQE")) {
				if (!int.TryParse(traceSwitch.Value, out var value)) {
					continue;
				}

				if (value > 0) {
					isTraceEnabled = true;
				}

				RuntimeConfiguration.Tracing
					.SetTraceLevel(traceSwitch.Key, (TraceLevel)value);
			}

			foreach (var connection in connectionStrings) {
				RuntimeConfiguration.AddConnectionString($"{connection.Key}.ConnectionString", connection.Value);
			}

			RuntimeConfiguration.ConfigureDQE<SQLServerDQEConfiguration>(config => {
				config
					.AddDbProviderFactory(typeof(SqlClientFactory))
					.SetDefaultCompatibilityLevel(SqlServerCompatibilityLevel.SqlServer2012);

				var dqeTraceSetting =
					traceSwitches.FirstOrDefault(s => s.Key == "SqlServerDQE");

				if (dqeTraceSetting != null) {
					if (int.TryParse(dqeTraceSetting.Value, out var value)) {
						if (value > 0) {
							isTraceEnabled = true;
						}
						config.SetTraceLevel((TraceLevel)value);
					}
				}

				foreach (var catalogNameOverwrite in catalogNameOverwrites) {
					var catalogName =
						catalogNameOverwrite.GetChildren()?.FirstOrDefault(s => s.Key == "CatalogName")?.Value;

					if (catalogName == null) {
						continue;
					}

					var overwrite =
						catalogNameOverwrite.GetChildren()?.FirstOrDefault(s => s.Key == "Overwrite")?.Value;

					config.AddCatalogNameOverwrite(catalogName, overwrite ?? string.Empty);
				}

			});

			if (!isTraceEnabled || !traceListeners.GetChildren().Any()) {
				return;
			}

			var logToConsole = traceListeners.GetValue<bool>("Console");
			var logToDebug = traceListeners.GetValue<bool>("Debug");
			var logFileName = traceListeners.GetValue<string>("File");

			Trace.Listeners.Clear();
	
			if (logToConsole) {
				Trace.Listeners.Add(new TextWriterTraceListener(Console.Out, "Console"));
			}

			if (logToDebug) {
				Trace.Listeners.Add(new DefaultTraceListener {
					Name = "Debug",
					LogFileName = logFileName
				});
				return;
			}

			if (string.IsNullOrEmpty(logFileName)) {
				Trace.Listeners.Add(new TextWriterTraceListener(logFileName, "File"));				
			}
		}
	}
}
