/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.Connection
{

	/// <summary>
	/// A Pro Pricer Instance Element in config
	/// </summary>
	public class ProPricerInstanceElement
	{
		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the company.
		/// </summary>
		public string Company { get; set; }

		/// <summary>
		/// Gets or sets the instanceId.
		/// </summary>
		public int InstanceId { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string ConnectionName { get; set; }

		/// <summary>
		/// Gets or sets the friendly Name.
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		/// Gets or sets the server.
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		/// Gets or sets the port.
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Gets or sets the number of allowed connections.
		/// </summary>
		public int NumberConnections { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is backup.
		/// </summary>
		public bool IsBackup { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is for Production.
		/// </summary>
		public bool IsProduction { get; set; }
	}
}