/*
	Copyright 2016-2018 Lockheed Martin Corporation.

	This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
	commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
	by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
	and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/
using System.Web.Http;
using APTSPropricerApi.Connection;
using Unity;
using Unity.Lifetime;
using Unity.WebApi;


namespace APTSPropricerApi
{
    public static class UnityConfig
	{
		////
		/// Using dependency injection to inject the PROPRICER connection into the controllers.
		/// 
		// See: Developer's Guide to Dependency Injection Using Unity 
		//      - https://msdn.microsoft.com/en-us/library/dn223671(v=pandp.30).aspx

		// NOTE: If the instance you are connecting to implements a session timeout limit you will need to modify
		// the pool retrieval code to check for a closed connection and reopen it.

		// Pooling Ideas: 
		// - Create first pooled connection at 4:00am M-F
		// - If last pooled connection was just given out then start async process to add another (up to configured limit)
		// - If more than two pooled connections remain in pool then start async process to close connections that haven't been used in last 10 min (leaving at least one in pool)
		//
		public static void RegisterComponents()
		{
			UnityContainer container = new UnityContainer();
			
			// register all your components with the container here
			// it is NOT necessary to register your controllers
			
			container.RegisterType<IProPricerConnection, ProPricerConnection>(
				new ContainerControlledLifetimeManager());
			
			GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);

			PoolManager.ResetPoolManagers();		
		}
	}
}