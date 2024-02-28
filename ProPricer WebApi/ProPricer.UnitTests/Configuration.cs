/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace ProPricer.UnitTests
{
	using System;
	using ACV.Shared;
	using APTSPropricerApi.Connection;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Caching.Memory;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Configuration helper class
	/// </summary>
	[TestClass]
	public class Configuration
	{
		/// <summary>
		/// Service Provider used to instantiate Services
		/// </summary>
		public static ServiceProvider ServiceProvider { get; private set; }

		/// <summary>
		/// Builds and retuns a Configuration
		/// </summary>
		/// <param name="testContext">The test context to use to find the appsettings files</param>
		/// <returns>Configuration Root</returns>
		private static IConfigurationRoot GetIConfigurationRoot(TestContext testContext)
		{
			IConfigurationBuilder builder = new ConfigurationBuilder()
				.SetBasePath(testContext.DeploymentDirectory)
				.AddJsonFile("appsettings.json", false)
				.AddJsonFile("appsettings.local.json", true)
				.AddUserSecrets<Configuration>()
				.AddEnvironmentVariables();

			IConfigurationRoot root = builder.Build();
			ConfigurationServiceWeb.Configuration = root;

			return root;
		}

		/// <summary>
		/// Initialized the Configuration for the Tests
		/// </summary>
		/// <param name="testContext">The test context to use to find the appsettings files</param>
		[AssemblyInitialize]
		public static void Initialize(TestContext testContext)
		{
			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");
			Environment.SetEnvironmentVariable("HDBDOTNETCORE", Environment.CurrentDirectory);
			IConfiguration configuration = GetIConfigurationRoot(testContext);

			ServiceCollection services = new();
			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

			// Simple configuration object injection (no IOptions<T>)
			services.AddSingleton(configuration);
			services.AddSingleton<IMemoryCache, MemoryCache>();
			services.AddSingleton<PoolManagerList>();

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			ServiceProvider = services.AddLogging(builder => builder.AddConsole()).BuildServiceProvider();

			new Aspose.Cells.License().SetLicense("Aspose.Total.lic");
		}
	}
}
