namespace GenBOE.Reports.Backend.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IES.Common.Core.Interfaces;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;

	[Route("[controller]")]
	public class WeatherForecastController : IES.Common.Core.IESController
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		public WeatherForecastController(ILogger<WeatherForecastController> logger, 
			ISecurityInformation securityInformation, IConfiguration configuration) : base(logger, securityInformation, configuration)
		{
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public IEnumerable<WeatherForecast> Get()
		{
			this.log.LogDebug("Inside weather forecast get");
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}
	}
}
