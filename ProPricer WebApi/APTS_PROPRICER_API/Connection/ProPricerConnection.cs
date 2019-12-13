/*
    Copyright 2016-2018 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

using System;
using System.Configuration;
using EBS.ProPricer.Client;
using EBS.ProPricer.Configuration.Client;
using EBS.ProPricer.Data;
using EBS.ProPricer.Model;
using EBS.ProPricer.Registration;

namespace APTSPropricerApi.Connection
{
    /// <summary>
    /// A unique Pro Pricer Connection
    /// </summary>
    /// <seealso cref="APTSPropricerApi.Connection.IProPricerConnection" />
    public class ProPricerConnection : IProPricerConnection
    {
        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        public Workspace Workspace { get; set; }

        /// <summary>
        /// Gets or sets the data server.
        /// </summary>
        private DataServer DataServer { get; set; }

        private int InstanceId { get; }

        /// <summary>
        /// The logger
        /// </summary>
        private readonly Logger logger = new Logger(typeof(ProPricerConnection));

        /// <summary>
        /// Initializes a new instance of the <see cref="ProPricerConnection" /> class.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        public ProPricerConnection(int instanceId, string connectionName, string server, int port)
        {
            this.InstanceId = instanceId;
            this.EstablishConnection(connectionName, server, port);
        }

        private bool EstablishConnection(string connection, string serverName, int port)
        {
            try
            {
                string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                System.Diagnostics.Debug.WriteLine(currentUser);
                // Assigning the server name and port to the datacenter
                DataCenter datacenter = DataCenter.Open(serverName, port);

                // Creating the dataserver and finding the connection

                // ToDo: Dusan - this picks which instance / connection we are going to work with
                // I wonder how expensive it is to switch between these.. this could help us w/ connection pooling.. maybe?
                this.DataServer = datacenter.DataServers.Find(connection);

                //Opening the workspace.  Activation provider set to null.  If not activated, opening the workspace will fail.
                //This assumes that PROPRICER is installed and activated.  This also assumes that the PROPRICER .dlls are in use, not API dlls.
                //                workspace = dataServer.OpenWorkspace(GetUserLogon, null, null);

                //This assumes that PROPRICER is installed as API DLL's and the PROPRICER app is not installed on this machine.
                this.Workspace = this.DataServer.OpenWorkspace(this.GetUserLogon, GetRegistration, this.GetActivation);
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                //    var message = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                //    message.Content = new System.Net.Http.StringContent("Error with connection" + e.Message);
                //    throw new System.Web.Http.HttpResponseException(message);
                return false;
            }

            return true;
        }

        private bool GetUserLogon(LogonInfo info)
        {
            // ProPricer user with required permissions to access and modify the target data
            info.Type = LogonType.Integrated;

            return !(string.IsNullOrEmpty(info.UserName) || string.IsNullOrEmpty(info.Password));
        }

        private static bool GetRegistration(RegistrationInformation registrationInfo)
        {
            string sProPricerRegistrationKey = ConfigurationUtilities.GetAppSetting("ProPricerRegistrationKey");
            registrationInfo.Key = sProPricerRegistrationKey;

            return true;
        }

        private bool GetActivation(ProPricerActivation activation, ref bool changeKey)
        {
            try
            {
                if (!activation.IsActivated)
                {
                    activation.ActivateOnline();
                }
            }
            catch (Exception e)
            {
                this.logger.Error(e);
            }

            return true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            PoolManager.GetInstance(this.InstanceId).GiveObjectBackToPool(this);
        }
    }
}