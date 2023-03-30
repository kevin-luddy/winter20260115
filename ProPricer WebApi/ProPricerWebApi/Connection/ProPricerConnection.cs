/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Connection
{
	using ACV.Shared;
	using EBS.ProPricer.Client;
	using EBS.ProPricer.Data;
	using EBS.ProPricer.Model;
	using EBS.ProPricer.Registration;

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

		/// <summary>
		/// The instance Id
		/// </summary>
		private int InstanceId { get; }

		/// <summary>
		/// The pool manager list
		/// </summary>
		private readonly PoolManagerList poolManagerList;

		/// <summary>
		/// True if this connection has been disposed
		/// </summary>
		private bool disposedValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProPricerConnection" /> class.
		/// </summary>
		/// <param name="instanceId">The instance identifier.</param>
		/// <param name="connectionName">Name of the connection.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		/// <param name="poolManagerList">The pool manager list</param>
		public ProPricerConnection(int instanceId, string connectionName, string server, int port,
			PoolManagerList poolManagerList)
		{
			this.InstanceId = instanceId;
			this.poolManagerList = poolManagerList;
			this.EstablishConnection(connectionName, server, port);
		}

		/// <summary>
		/// Establishes a connection to propricer
		/// </summary>
		/// <param name="connection">The connection string</param>
		/// <param name="serverName">The server name</param>
		/// <param name="port">The port number</param>
		private void EstablishConnection(string connection, string serverName, int port)
		{
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

		/// <summary>
		/// Gets if User can logon
		/// </summary>
		/// <param name="info"></param>
		/// <returns>True if user has required ProPricer permissions to access and modify the target data</returns>
		private bool GetUserLogon(LogonInfo info)
		{
			// ProPricer user with required permissions to access and modify the target data
			info.Type = LogonType.Integrated;

			return !(string.IsNullOrEmpty(info.UserName) || string.IsNullOrEmpty(info.Password));
		}

		/// <summary>
		/// Gets if registration key works
		/// </summary>
		/// <param name="registrationInfo">The registration info</param>
		/// <returns>True if propricer license key is registered</returns>
		private static bool GetRegistration(RegistrationInformation registrationInfo)
		{
			string sProPricerRegistrationKey = ConfigurationServiceWeb.Configuration.GetValue<string>("ProPricerRegistrationKey");
			registrationInfo.Key = sProPricerRegistrationKey;

			return true;
		}

		/// <summary>
		/// Tries to activate using the ProPricer key
		/// </summary>
		/// <param name="activation">The activation instance</param>
		/// <param name="changeKey">The change key</param>
		/// <returns>True if activated online</returns>
		private bool GetActivation(ProPricerActivation activation, ref bool changeKey)
		{
			if (!activation.IsActivated)
			{
				activation.ActivateOnline();
			}

			return true;
		}

		/// <summary>
		/// Dispose of managed and unmanaged objects
		/// </summary>
		/// <param name="disposing">true for disposing of managed objects</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
					this.poolManagerList.GetInstance(this.InstanceId).GiveObjectBackToPool(this);
				}

				// free unmanaged resources (unmanaged objects) and override finalizer
				// set large fields to null
				disposedValue = true;
			}
		}

		/// <summary>
		/// Dispose of this instance
		/// </summary>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}