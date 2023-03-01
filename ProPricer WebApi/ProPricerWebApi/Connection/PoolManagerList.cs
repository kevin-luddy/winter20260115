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

	/// <summary>
	/// Houses the list of Pool Managers
	/// </summary>
	public sealed class PoolManagerList
	{
		/// <summary>
		/// The logger
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// Dictionary of pool managers
		/// </summary>
		private readonly Dictionary<int, PoolManager> poolManagers = new();

		/// <summary>
		/// lock object
		/// </summary>
		private readonly object ObjLock = new();

		/// <summary>
		/// #ctor
		/// </summary>
		/// <param name="logger">The Logger</param>
		public PoolManagerList(ILogger<PoolManagerList> logger)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Static property to retrieve the instance of the Pool Manager
		/// </summary>
		public PoolManager GetInstance(int id)
		{
			if (poolManagers.None())
			{
				lock (ObjLock)
				{
					if (poolManagers.None())
					{
						ResetPoolManagers();
					}
				}
			}

			return poolManagers[id];
		}

		/// <summary>
		/// Resets the pool managers.
		/// </summary>
		public void ResetPoolManagers()
		{
			lock (ObjLock)
			{
				if (poolManagers.Any())
				{
					// No need to close the current connections since the pool managers will not allow any old connections to be added to them (since they are already full)
					poolManagers.Clear();
				}

				ProPricerInstanceElement[] proPricerInstances = ConfigurationServiceWeb.Configuration.GetSection("ProPricerInstanceConfig").Get<ProPricerInstanceElement[]>();

				bool isUsingBackup = ConfigurationServiceWeb.Configuration.GetValue<bool>("UseProPricerBackup");
				string company = SystemConfiguration.Instance().CompanyMode.GetDescription();
				bool isProduction = ConfigurationServiceWeb.Configuration.GetValue<bool>("IsProduction");

				foreach (ProPricerInstanceElement element in proPricerInstances)
				{
					if (isProduction == element.IsProduction && element.IsBackup == isUsingBackup && element.Company == company)
					{
						try
						{
							PoolManager poolManager = new(element.InstanceId, element.FriendlyName, element.NumberConnections);
							for (int i = 0; i < poolManager.poolSize; i++)
							{
								IProPricerConnection ppc = new ProPricerConnection(element.InstanceId, element.ConnectionName, element.Server, element.Port, this);
								if (ppc.Workspace != null)
								{
									poolManager.AddObject(ppc);
									System.Diagnostics.Debug.WriteLine("Success");
								}
								else
								{
									logger.LogError("Error creating Connection to ProPricer, the Connection worked, but the Workspace is null.");
								}
							}

							if (poolManager.CurrentObjectsInPool > 0)
							{
								poolManagers.Add(element.InstanceId, poolManager);
							}

							System.Diagnostics.Debug.WriteLine(poolManager.CurrentObjectsInPool.ToString());
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Error creating Connection to ProPricer");
						}
					}
				}
			}
		}

		/// <summary>
		/// Static property to retrieve the instances of the Pool Manager
		/// </summary>
		public ICollection<PoolManager> Instances
		{
			get
			{
				if (poolManagers.None())
				{
					lock (ObjLock)
					{
						if (poolManagers.None())
						{
							ResetPoolManagers();
						}
					}
				}

				return poolManagers.Values;
			}
		}
	}
}
