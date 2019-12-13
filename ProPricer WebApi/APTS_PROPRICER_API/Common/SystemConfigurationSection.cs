/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web.Configuration;
    using System.Xml;

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
            this.CompanyConfigurations = new HashSet<CompanyConfigurationSection>();
        }

        /// <summary>
        /// Read and return the section configuration
        /// </summary>
        /// <returns></returns>
        public static SystemConfigurationSection Section
        {
            get
            {
                return WebConfigurationManager.GetWebApplicationSection("systemConfiguration") as SystemConfigurationSection ?? new SystemConfigurationSection();
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
                return this.CompanyConfigurations.FirstOrDefault(c => c.Company == company) ?? new CompanyConfigurationSection(company);
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
                throw new ArgumentNullException("section");
            }

            SystemConfigurationSection configuration = new SystemConfigurationSection();

            foreach (XmlNode companyConfigurationNode in section.ChildNodes)
            {
                if (companyConfigurationNode.NodeType == XmlNodeType.Element)
                {
                    CompanyConfiguration company = this.GetAttributeValue(companyConfigurationNode, "company").GetEnumeratedValue<CompanyConfiguration>(CompanyConfiguration.None);

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
                                        string appSettingName = this.GetAttributeValue(appSettingNode, "key");
                                        string appSettingValue = this.GetAttributeValue(appSettingNode, "value");

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
                                        ConnectionStringSettings connectionString = new ConnectionStringSettings
                                        {
                                            Name = this.GetAttributeValue(connectionStringNode, "name"),
                                            ConnectionString = this.GetAttributeValue(connectionStringNode, "connectionString"),
                                            ProviderName = this.GetAttributeValue(connectionStringNode, "providerName")
                                        };

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
