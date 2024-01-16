// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Specialized;
using System.Configuration;
using IES.Common.Core.Enums;

namespace IES.Common.Core.Configuration
{
	/// <summary>
	/// Individual company configuration and override settings
	/// </summary>
	/// <seealso cref="SystemConfigurationSection"/>
	public class CompanyConfigurationSection
	{
		private NameValueCollection _AppSettings;

		/// <summary>
		/// Constructor
		/// </summary>
		public CompanyConfigurationSection()
		{
			Company = CompanyConfiguration.None;
			_AppSettings = new NameValueCollection();
			ConnectionStrings = new ConnectionStringSettingsCollection();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="company">Company</param>
		public CompanyConfigurationSection(CompanyConfiguration company) : this()
		{
			Company = company;
		}

		/// <summary>
		/// Company identifier
		/// </summary>
		public CompanyConfiguration Company { get; }

		/// <summary>
		/// Override values for application settings
		/// </summary>
		public NameValueCollection AppSettings { get { return _AppSettings; } }

		/// <summary>
		/// Override values for connection strings
		/// </summary>
		public ConnectionStringSettingsCollection ConnectionStrings { get; }
	}
}
