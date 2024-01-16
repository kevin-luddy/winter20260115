// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using IES.Common.Core.Enums;

namespace IES.Common.Core.Configuration
{
	/// <summary>
	/// Web.config section handler for "systemConfiguration"
	/// </summary>
	public class SystemConfigurationSection : IConfigurationSectionHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SystemConfigurationSection()
		{
			CompanyConfigurations = new HashSet<CompanyConfigurationSection>();
		}

		/// <summary>
		/// Read and return the section configuration
		/// </summary>
		/// <returns></returns>
		public static SystemConfigurationSection Section
		{
			get
			{
				return ConfigurationManager.GetSection("systemConfiguration") as SystemConfigurationSection ?? new SystemConfigurationSection();
			}
		}

		/// <summary>
		/// Retrieve the configuration for the designated company.
		/// </summary>
		/// <param name="company">Company</param>
		/// <returns>The company configuration section</returns>
		public CompanyConfigurationSection this[CompanyConfiguration company]
		{
			get
			{
				return CompanyConfigurations.FirstOrDefault(c => c.Company == company) ?? new CompanyConfigurationSection(company);
			}
		}

		/// <summary>
		/// Collection of individual company configurations and override settings
		/// </summary>
		public ICollection<CompanyConfigurationSection> CompanyConfigurations { get; private set; }

		/// <summary>
		/// Configuration section handler.  Reads the Web.config file and loads the corresponding configuration collection.
		/// </summary>
		/// <param name="parent">Parent object</param>
		/// <param name="configContext">Configuration context object</param>
		/// <param name="section">Section XML node</param>
		/// <returns>The created section handler object.  <see cref="SystemConfigurationSection"/></returns>
		public object Create(object parent, object configContext, XmlNode section)
		{
			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			SystemConfigurationSection configuration = new SystemConfigurationSection();

			foreach (XmlNode companyConfigurationNode in section.ChildNodes)
			{
				if (companyConfigurationNode.NodeType == XmlNodeType.Element)
				{
					CompanyConfiguration company = GetAttributeValue(companyConfigurationNode, "company").GetEnumeratedValue<CompanyConfiguration>(CompanyConfiguration.None);

					CompanyConfigurationSection companyConfiguration = new CompanyConfigurationSection(company);

					foreach (XmlNode companySettingNode in companyConfigurationNode.ChildNodes)
					{
						if (companySettingNode.NodeType == XmlNodeType.Element)
						{
							if (companySettingNode.Name == "appSettings")
							{
								foreach (XmlNode appSettingNode in companySettingNode.ChildNodes)
								{
									if (appSettingNode.NodeType == XmlNodeType.Element && appSettingNode.Name == "add")
									{
										string appSettingName = GetAttributeValue(appSettingNode, "key");
										string appSettingValue = GetAttributeValue(appSettingNode, "value");

										companyConfiguration.AppSettings.Add(appSettingName, appSettingValue);
									}
								}
							}
							else if (companySettingNode.Name == "connectionStrings")
							{
								foreach (XmlNode connectionStringNode in companySettingNode.ChildNodes)
								{
									if (connectionStringNode.NodeType == XmlNodeType.Element && connectionStringNode.Name == "add")
									{
										ConnectionStringSettings connectionString = new ConnectionStringSettings();

										connectionString.Name = GetAttributeValue(connectionStringNode, "name");
										connectionString.ConnectionString = GetAttributeValue(connectionStringNode, "connectionString");
										connectionString.ProviderName = GetAttributeValue(connectionStringNode, "providerName");

										companyConfiguration.ConnectionStrings.Add(connectionString);
									}
								}
							}
						}
					}

					configuration.CompanyConfigurations.Add(companyConfiguration);
				}
			}

			return configuration;
		}

		/// <summary>
		/// Helper method for getting attribute values from an XML node
		/// </summary>
		/// <param name="node">XML node</param>
		/// <param name="attributeName">Attribute name</param>
		/// <returns>Attribute value, or null if none exists</returns>
		private string GetAttributeValue(XmlNode node, string attributeName)
		{
			string value = null;

			XmlAttribute attr;

			if ((attr = node.Attributes[attributeName]) != null)
			{
				value = attr.Value;
			}

			return value;
		}
	}
}
