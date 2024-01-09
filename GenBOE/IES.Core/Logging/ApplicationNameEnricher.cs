// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2021 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
	using System;
	using Serilog;
	using Serilog.Configuration;
	using Serilog.Core;
	using Serilog.Events;

	/// <summary>
	/// Application name enricher
	/// </summary>
	public class ApplicationNameEnricher : ILogEventEnricher
	{
		/// <summary>
		/// Enrich the log event
		/// </summary>
		/// <param name="logEvent">Log event</param>
		/// <param name="propertyFactory">Property factory</param>
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(LoggerConstants.AppNameEnrichProperty, AppDomain.CurrentDomain.FriendlyName));
		}
	}

	public static class LoggerEnrichmentConfigurationExtensions
	{
		public static LoggerConfiguration WithApplicationName(this LoggerEnrichmentConfiguration enrich)
		{
			return enrich.With<ApplicationNameEnricher>();
		}
	}
}
