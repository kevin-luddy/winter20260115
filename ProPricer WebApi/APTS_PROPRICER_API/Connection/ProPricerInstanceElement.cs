/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Connection
{
    using System.Configuration;

    /// <summary>
    /// A Pro Pricer Instance Element in web.config
    /// </summary>
    /// <seealso cref="System.Configuration.ConfigurationElement" />
    public class ProPricerInstanceElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerInstanceElement" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="company">The company.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="numberConnections">The number of allowed connections.</param>
        /// <param name="isBackup">if set to <c>true</c> [is backup].</param>
        public ProPricerInstanceElement(string name, string company, string friendlyName, string server, int port, int numberConnections, bool isBackup, bool isProduction)
        {
            this.Company = company;
            this.FriendlyName = friendlyName;
            this.ConnectionName = name;
            this.Server = server;
            this.Port = port;
            this.NumberConnections = numberConnections;
            this.IsBackup = isBackup;
            this.IsProduction = isProduction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerInstanceElement"/> class.
        /// </summary>
        public ProPricerInstanceElement()
        {

        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [ConfigurationProperty("id",
            IsRequired = true, IsKey = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 20, ExcludeRange = false)]
        public int Id
        {
            get
            {
                return (int)this["id"];
            }
            set
            {
                this["id"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        [ConfigurationProperty("company",
            IsRequired = true, IsKey = false)]
        public string Company
        {
            get
            {
                return (string)this["company"];
            }
            set
            {
                this["company"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the instanceId.
        /// </summary>
        [ConfigurationProperty("instanceId",
            IsRequired = true, IsKey = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 20, ExcludeRange = false)]
        public int InstanceId
        {
            get
            {
                return (int)this["instanceId"];
            }
            set
            {
                this["instanceId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [ConfigurationProperty("connectionName", 
            IsRequired = true, IsKey = true)]
        public string ConnectionName
        {
            get
            {
                return (string)this["connectionName"];
            }
            set
            {
                this["connectionName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the friendly Name.
        /// </summary>
        [ConfigurationProperty("friendlyName",
            IsRequired = true, IsKey = false)]
        public string FriendlyName
        {
            get
            {
                return (string)this["friendlyName"];
            }
            set
            {
                this["friendlyName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        [ConfigurationProperty("server",
            IsRequired = true, IsKey = false)]
        public string Server
        {
            get
            {
                return (string)this["server"];
            }
            set
            {
                this["server"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of allowed connections.
        /// </summary>
        [ConfigurationProperty("numberConnections", DefaultValue = (int)3, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 20, ExcludeRange = false)]
        public int NumberConnections
        {
            get
            {
                return (int)this["numberConnections"];
            }
            set
            {
                this["numberConnections"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is backup.
        /// </summary>
        [ConfigurationProperty("isBackup", DefaultValue = false, IsRequired = false)]
        public bool IsBackup
        {
            get
            {
                return (bool)this["isBackup"];
            }
            set
            {
                this["isBackup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is for Production.
        /// </summary>
        [ConfigurationProperty("isProduction", DefaultValue = false, IsRequired = false)]
        public bool IsProduction
        {
            get
            {
                return (bool)this["isProduction"];
            }
            set
            {
                this["isProduction"] = value;
            }
        }
    }
}