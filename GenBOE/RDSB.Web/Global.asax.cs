// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RDSB.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using IES.ActionLogic.ControllerLogic;
    using IES.ActionLogic.IO.Export;
    using IES.Common;
    using IES.Common.classes;
    using IES.DataBridge.Loaders;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;

    /// <summary>
    /// Global MVC Application setup.
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

            GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), -1));
            GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), this.GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls

            // Register PTM Classes
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformation), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IES.Common.ICache))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityMapper), typeof(SecurityMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityUserAuthorizationsDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IUserMapper))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityUserAuthorizationsDataLoader), typeof(SecurityUserAuthorizationsDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IUserLoader), typeof(UserLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)))).Configure<Interception>().SetInterceptorFor<IUserLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IUserMapper), typeof(UserMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IUserLoader)), new ResolvedParameter(typeof(CacheDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            
            // Register Action Logic
            GenBOEUnityContainer.Container.RegisterType(typeof(IDocumentControllerLogic), typeof(DocumentControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IDocumentLoader)), new ResolvedParameter(typeof(IDocumentDetailLoader)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IES.Common.ISecurityInformation)), new ResolvedParameter(typeof(IRevisionLoader)), new ResolvedParameter(typeof(ISectionLoader)), new ResolvedParameter(typeof(IRateDetailLoader)), new ResolvedParameter(typeof(IFileAttachmentLoader)), new ResolvedParameter(typeof(IPPRDExporter))));

            // Register Loaders
            GenBOEUnityContainer.Container.RegisterType(typeof(IAreaLockingLoader), typeof(AreaLockingLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IDocumentLoader), typeof(DocumentLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRevisionLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IDocumentDetailLoader), typeof(DocumentDetailLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRdsbRateCodeXrefLoader)), new ResolvedParameter(typeof(IRdsbSectionXrefLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IRdsbRateCodeXrefLoader), typeof(RdsbRateCodeXrefLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IRdsbSectionXrefLoader), typeof(RdsbSectionXrefLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IProposalLoader), typeof(ProposalLoader), this.GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<IProposalLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateCodeYearLoader), typeof(RateCodeYearLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IProPricerRateCodeXrefLoader), typeof(ProPricerRateCodeXrefLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateConfigLoader), typeof(RateConfigLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IRateDetailLoader), typeof(RateDetailLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IRateCodeYearLoader)), new ResolvedParameter(typeof(IProPricerRateCodeXrefLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IRevisionLoader), typeof(RevisionLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(ISectionLoader), typeof(SectionLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWhosOnlineLoader), typeof(WhosOnlineLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IFileAttachmentLoader), typeof(FileAttachmentLoader), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IPPRDExporter), typeof(PPRDExporter), this.GetLifetimeManager(), new InjectionConstructor());
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
