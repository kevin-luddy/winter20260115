// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("ProPricer.UnitTests")]

namespace APTSPropricerApi
{
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Hosting;
	using Serilog;

	/// <summary>
	/// The Program
	/// </summary>
	public class Program
	{
		/// <summary>
		/// App Entry Point
		/// </summary>
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		/// <summary>
		/// Create Host Builder
		/// </summary>
		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseSerilog()
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
		}
	}
}
