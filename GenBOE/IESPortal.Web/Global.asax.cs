// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IESPortal.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using GenTRAC.DataBridge.DTO;
    using GenBOE.DataBridge.DTO;
    using IES.ActionLogic.Common;
    using IES.ActionLogic.ControllerLogic;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Loaders;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;

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

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

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

            GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), -1));
            GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), this.GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls

            // Register Classes and Loaders
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformation), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IES.Common.ICache))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IBannerLoader), typeof(BannerLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(BannerMediator), typeof(BannerMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBannerLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IESPortalAdminControllerLogic), typeof(IESPortalAdminControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(PtmPickListMapper)), new ResolvedParameter(typeof(BoePickListMapper)), new ResolvedParameter(typeof(OfflineApplicationLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IOfflineApplicationLoader), typeof(OfflineApplicationLoader), this.GetLifetimeManager(), new InjectionConstructor());
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
