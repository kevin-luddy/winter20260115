// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
// Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Tests.Core
{
	using System;
	using IES.ActionLogic.Core.Mediator;
	using IES.Common.Core;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Services;
	using IES.DataBridge.Loaders;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Caching.Memory;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Serilog;

	/// <summary>
	/// Configuration helper class
	/// </summary>
	[TestClass]
	public class Configuration
	{
		/// <summary>
		/// Test data
		/// </summary>
		private static TestData testData = TestData.GetInstance();

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

			return builder.Build();
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
			ApplicationConfigurationBase.Configuration = configuration;

			ServiceCollection services = new();
			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

			// Simple configuration object injection (no IOptions<T>)
			services.AddSingleton(configuration);		

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			ServiceProvider = services.AddLogging(builder => builder.AddConsole()).BuildServiceProvider();

			testData.Initialize();
		}

		/// <summary>
		/// Cleanup a test
		/// </summary>
		[AssemblyCleanup]
		public static void Cleanup()
		{
			testData.Cleanup();
		}
	}
}
