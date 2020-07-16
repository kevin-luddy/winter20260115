// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDM.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using IES.ActionLogic.Common;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.IO.Export;
    using IES.ActionLogic.Mediator;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Common;
    using IES.DataBridge.Loaders;
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
        /// Applications the start.
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

            // Register Mapper
            GenBOEUnityContainer.Container.RegisterType(typeof(ICommonDataMapper), typeof(CommonDataMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRateDetailLoader)), new ResolvedParameter(typeof(ICacheDataLoader))));
            
            // Register Loaders
            GenBOEUnityContainer.Container.RegisterType(typeof(IFileAttachmentLoader), typeof(FileAttachmentLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(ICobraDetailLoader), typeof(CobraDetailLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(ICobraYearsLoader), typeof(CobraYearsLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IBurdenPoolLoader), typeof(BurdenPoolLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRevisionLoader), typeof(RevisionLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRevisionMediator), typeof(RevisionMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRevisionLoader)), new ResolvedParameter(typeof(ICacheDataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IAreaLockingLoader), typeof(AreaLockingLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateCodeYearLoader), typeof(RateCodeYearLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IProPricerRateCodeXrefLoader), typeof(ProPricerRateCodeXrefLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateConfigLoader), typeof(RateConfigLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateCodeReplicationLoader), typeof(RateCodeReplicationLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateDetailLoader), typeof(RateDetailLoader), this.GetLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter(typeof(IRateCodeYearLoader)),
                    new ResolvedParameter(typeof(IProPricerRateCodeXrefLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISectionLoader), typeof(SectionLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWhosOnlineLoader), typeof(WhosOnlineLoader),
                this.GetLifetimeManager(), new InjectionConstructor());

            // Register Misc Classes
            GenBOEUnityContainer.Container.RegisterType(typeof(IDataFetchingScheduler), typeof(DataFetchingScheduler), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IIESEmailer), typeof(IESEmailer), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IDataFetchingScheduler))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformation), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IPPRDExporter), typeof(PPRDExporter), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IPPRDExporter>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IRdmRevisionExporter), typeof(RdmRevisionExporter), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IRdmRevisionExporter>(new InterfaceInterceptor());

            // Register Action Logic
            GenBOEUnityContainer.Container.RegisterType(typeof(IAdminControllerLogic), typeof(AdminControllerLogic));
            GenBOEUnityContainer.Container.RegisterType(typeof(IBurdenPoolControllerLogic), typeof(BurdenPoolControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IHomeControllerLogic), typeof(HomeControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IPPRDControllerLogic), typeof(PPRDControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFileAttachmentLoader)), new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateControllerLogic), typeof(RateControllerLogic), this.GetLifetimeManager());
            GenBOEUnityContainer.Container.RegisterType(typeof(IReportsControllerLogic), typeof(ReportsControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPPRDExporter)), new ResolvedParameter(typeof(IRdmRevisionExporter)), new ResolvedParameter(typeof(IRateDetailLoader)), new ResolvedParameter(typeof(ICobraDetailLoader)), new ResolvedParameter(typeof(ISectionLoader)), new ResolvedParameter(typeof(IFileAttachmentLoader)), new ResolvedParameter(typeof(IBurdenPoolLoader)), new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IVersionControllerLogic), typeof(VersionControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRateDetailLoader)), new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IIESEmailer))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IFileAttachmentControllerLogic), typeof(FileAttachmentControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IAreaLockingLoader)), new ResolvedParameter(typeof(IRevisionMediator)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ISecurityInformation))));
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
