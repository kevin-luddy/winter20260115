// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.Web
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Http;
	using System.Web.Mvc;
	using System.Web.Optimization;
	using System.Web.Routing;
	using IES.Common;
	using IES.Common.classes;
	using Microsoft.Practices.Unity;
	using Microsoft.Practices.Unity.InterceptionExtension;

	/// <summary>
	/// The MVC Application initializer.
	/// </summary>
	/// <seealso cref="System.Web.HttpApplication" />
	public class MvcApplication : System.Web.HttpApplication
	{
		/// <summary>
		/// The lifetime managers.
		/// </summary>
		private List<ContainerControlledLifetimeManager> lifetimeManagers = new List<ContainerControlledLifetimeManager>();

		/// <summary>
		/// The logger.
		/// </summary>
		private Logger log = new Logger(typeof(MvcApplication));

		/// <summary>
		/// Initializes a new instance of the <see cref="MvcApplication"/> class.
		/// </summary>
		public MvcApplication()
		{
		}


		/// <summary>
		/// Application Start.
		/// </summary>
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			try
			{
				AreaRegistration.RegisterAllAreas();
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - AreaRegistration.RegisterAllAreas failed.");
				throw;
			}

			try
			{
				this.InitializeContainer();
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - InitializeContainer failed.");
				throw;
			}

			try
			{
				IControllerFactory factory = new UnityFactory(GenBOEUnityContainer.Container);
				ControllerBuilder.Current.SetControllerFactory(factory);
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - Unity Factory setup failed.");
				throw;
			}

			try
			{
				RouteConfig.RegisterRoutes(RouteTable.Routes);
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - RegisterRoutes failed.");
				throw;
			}

			try
			{
				DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - DataAnnotationsModelValidatorProvider setup failed.");
				throw;
			}

			try
			{
				JsonValueProviderFactory vpfJson;
				if ((vpfJson = ValueProviderFactories.Factories.OfType<System.Web.Mvc.JsonValueProviderFactory>().FirstOrDefault()) != null)
				{
					ValueProviderFactories.Factories.Remove(vpfJson);
				}

				ValueProviderFactories.Factories.Add(new MyJsonValueProviderFactory());  // override MaxJsonLength
			}
			catch (Exception ex)
			{
				this.log.Error(ex, "FATAL - JsonValueProviderFactory setup failed.");
				throw;
			}
		}


		/// <summary>
		/// Initializes the container.
		/// </summary>
		protected virtual void InitializeContainer()
		{
			// for more info see http://msdn.microsoft.com/en-us/library/ff660882%28PandP.20%29.aspx

			GenBOEUnityContainer.Container.AddNewExtension<Interception>();

			// Register ICache, Cache and Non Cache Data Loader
			GenBOEUnityContainer.Container.RegisterType(typeof(ICache), typeof(MemoryCache), this.GetLifetimeManager(), new InjectionMember[] { });
			GenBOEUnityContainer.Container.RegisterType(typeof(ICacheDataLoader), typeof(CacheDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), -1));
			GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), this.GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls
		}

		/// <summary>
		/// Gets the lifetime manager.
		/// </summary>
		/// <returns>The liftetime container.</returns>
		private ContainerControlledLifetimeManager GetLifetimeManager()
		{
			ContainerControlledLifetimeManager manager = new ContainerControlledLifetimeManager();
			this.lifetimeManagers.Add(manager);
			return manager;
		}

		/// <summary>
		/// Applications the end.
		/// </summary>
		protected void Application_End()
		{
			this.log.Debug("Ending Application..");

			GenBOEUnityContainer.Container.Dispose();

			foreach (ContainerControlledLifetimeManager manager in this.lifetimeManagers)
			{
				manager.Dispose();
			}
		}
	}
}
