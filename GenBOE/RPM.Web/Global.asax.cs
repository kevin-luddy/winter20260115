// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.Web
{
	using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
	using System.Web.Http;
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
        /// Application Start.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
			GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(GenBOEUnityContainer.Container);
			GenBOEUnityContainer.Container.AddNewExtension<Interception>();
			GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls
			GenBOEUnityContainer.Container.Dispose();
		}

		protected ContainerControlledLifetimeManager GetLifetimeManager()
		{
			ContainerControlledLifetimeManager manager = new ContainerControlledLifetimeManager();
			return manager;
		}
	}
}
