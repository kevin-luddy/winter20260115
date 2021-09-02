// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using GenTRAC.ActionLogic;
    using GenTRAC.ActionLogic.CacheWarming;
    using GenTRAC.ActionLogic.Email;
    using GenTRAC.ActionLogic.GeneralHelper;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.DataBridge.DTO.Reports;
    using GenTRAC.Objects;
    using GenTRAC.Web.Common;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.PickList;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    /// <summary>
    /// Global Asax file/class
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        #region Standard Setup
        /// <summary>
        /// Error Logger
        /// </summary>
        private readonly Logger logger = new Logger(typeof(MvcApplication));

        /// <summary>
        /// Lifetime managers
        /// </summary>
        private readonly List<ContainerControlledLifetimeManager> lifetimeManagers = new List<ContainerControlledLifetimeManager>();

        /// <summary>
        /// Registers Global Filters
        /// </summary>
        /// <param name="filters">Filters to register</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }
        }

        /// <summary>
        /// Application Start
        /// </summary>
        /// NOTE: Message suppressed because we NEED to catch ALL exceptions in our thread or it will bring down the system if we have a failure
        ///       in our internal threads.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Application_Start()
        {
            this.logger.Info("Starting Application...");
            ConfigureWebApi();

            AreaRegistration.RegisterAllAreas();

            this.InitializeContainer();
            IControllerFactory factory = new GenTRAC.Web.Controllers.Unity.UnityFactory(GenBOEUnityContainer.Container);
            ControllerBuilder.Current.SetControllerFactory(factory);

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // start a thread to warm the cache up ... we don't care about the 'lost'
            // resources enough to check everytime a user starts a session .... just let
            // it go until the app pool recycles which will reclaim the thread
            CacheWarmer warm = GenBOEUnityContainer.Container.Resolve(typeof(CacheWarmer)) as CacheWarmer;
            Thread warmThread = new Thread(delegate ()
            {
                Stopwatch timespent = new Stopwatch();
                timespent.Start();
                warm.DoWarmCache();
                timespent.Stop();
                this.logger.Info("Finished warming cache.. took " + timespent.Elapsed.TotalSeconds + " seconds.");
            });
            warmThread.Start();
        }

        /// <summary>
        /// Configures Web Api 2 "things" to work in an MVC application
        /// </summary>
        private static void ConfigureWebApi()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(GenBOEUnityContainer.Container);
        }

        /// <summary>
        /// Application End
        /// </summary>
        protected void Application_End()
        {
            this.logger.Info("Ending Application...");

            IES.Common.classes.GenBOEUnityContainer.Container.Dispose();

            foreach (ContainerControlledLifetimeManager manager in this.lifetimeManagers)
            {
                manager.Dispose();
            }
        }

        #endregion Standard Setup

        #region Route Table

        /// <summary>
        /// Registers Routes
        /// </summary>
        /// <param name="routes">Routes to register</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allasp}", new { allasp = @".*\.asp(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(./*)?" });
            routes.IgnoreRoute("Content/{*pathInfo}");

            routes.MapRoute(
                "EmptyRoute",
                string.Empty,
                new
                {
                    controller = WebConstants.Controller.HOME,
                    action = WebConstants.Action.HOME_DISPLAY_GENTRAC_HOME
                }); // Parameter defaults

            routes.MapRoute(
                WebConstants.Route.Name.HOME, // Route name
                WebConstants.Route.HOME_ROUTE, // URL with parameters
                new
                {
                    controller = WebConstants.Controller.HOME,
                    action = WebConstants.View.HOME_GENTRAC_HOME
                }); // Parameter defaults

            routes.MapRoute(
                WebConstants.Route.Name.ADMIN,
                WebConstants.Route.ADMIN_ROUTE,
                new
                {
                    controller = WebConstants.Controller.ADMIN
                });

            routes.MapRoute(
                WebConstants.Route.Name.GENTRAC_BASE,
                WebConstants.Route.GENTRAC_BASE_ROUTE,
                new
                {
                    controller = WebConstants.Controller.GEN_TRAC
                });

            routes.MapRoute(
                WebConstants.Route.Name.PROPOSAL,
                WebConstants.Route.PROPOSAL_ROUTE,
                new
                {
                    controller = WebConstants.Controller.PROPOSAL
                });

            routes.MapRoute(
                WebConstants.Route.Name.CHECKLIST,
                WebConstants.Route.CHECKLIST_ROUTE,
                new
                {
                    controller = WebConstants.Controller.CHECKLIST
                });

            routes.MapRoute(
                WebConstants.Route.Name.PROPOSAL_ID,
                WebConstants.Route.PROPOSAL_ID_ROUTE);

            routes.MapRoute(
                WebConstants.Route.Name.REPORTS,
                WebConstants.Route.REPORTS_ROUTE,
                new
                {
                    controller = WebConstants.Controller.REPORTS
                });
            routes.MapRoute(
               WebConstants.Route.Name.ERROR,
               WebConstants.Route.ERROR_ROUTE,
               new
               {
                   controller = WebConstants.Controller.GEN_TRAC
               });
        }
        #endregion Route Table

        #region Unity Container

        /// <summary>
        /// Initialize Container
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private void InitializeContainer()
        {
            //// for more info see http://msdn.microsoft.com/en-us/library/ff660882%28PandP.20%29.aspx

            GenBOEUnityContainer.Container.AddNewExtension<Interception>();

            #region Register ICache, Cache and Non Cache Data Loader
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ICache), typeof(MemoryCache), this.GetLifetimeManager(), new InjectionMember[] { });

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), -1));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), "GenBOEMetricsCache", this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), 43200));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(NonCacheDataLoader), typeof(NonCacheDataLoader), this.GetLifetimeManager(), new InjectionConstructor());

            #endregion

            #region Register Loaders

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IApprovalsLoader), typeof(ApprovalsLoader), this.GetLifetimeManager(), new InjectionConstructor());

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader), typeof(GenBOE.DataBridge.Common.SecurityUserAuthorizationsDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityUserAuthorizationsDataLoader), typeof(SecurityUserAuthorizationsDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IUserLoader), typeof(UserLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)))).Configure<Interception>().SetInterceptorFor<IUserLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalLoader), typeof(ProposalLoader), this.GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<IProposalLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISystemPermissionLoader), typeof(SystemPermissionLoader), this.GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<ISystemPermissionLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalPermissionLoader), typeof(ProposalPermissionLoader), this.GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<IProposalPermissionLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalChecklistLoader), typeof(ProposalChecklistLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IProposalChecklistLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(PARChecklistContentLoader), typeof(PARChecklistContentLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IChecklistContentLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(PPRChecklistContentLoader), typeof(PPRChecklistContentLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IChecklistContentLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IManageProposalInfoLoader), typeof(ManageProposalInfoLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IBulkArchiveLoader), typeof(BulkArchiveLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IBulkArchiveLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IReportsLoader), typeof(ReportsLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IReportsLoader>(new InterfaceInterceptor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ProposalTypeLULoader), typeof(ProposalTypeLULoader), this.GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader), typeof(GenBOE.DataBridge.DTO.WorkspaceDTODataLoader), this.GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.DataBridge.DTO.IPermissionsDTODataLoader), typeof(GenBOE.DataBridge.DTO.PermissionsDTODataLoader), this.GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<GenBOE.DataBridge.DTO.IPermissionsDTODataLoader>(new InterfaceInterceptor());

            #endregion

            #region Register Mappers

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityMapper), typeof(SecurityMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityUserAuthorizationsDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IUserMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IUserMapper), typeof(UserMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IUserLoader)), new ResolvedParameter(typeof(CacheDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IOrgStructureDataMapper), typeof(OrgStructureDataMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(LineOfBusinessDataLoader)), new ResolvedParameter(typeof(ProgramAreaDataLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISystemPermissionMapper), typeof(SystemPermissionMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISystemPermissionLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(CacheWarmingUserMapper), typeof(CacheWarmingUserMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IUserLoader)), new ResolvedParameter(typeof(CacheDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalPermissionMapper), typeof(ProposalPermissionMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IProposalPermissionLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(PARChecklistContentMapper), typeof(PARChecklistContentMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(PARChecklistContentLoader)), new ResolvedParameter(typeof(CacheDataLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(PPRChecklistContentMapper), typeof(PPRChecklistContentMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(PPRChecklistContentLoader)), new ResolvedParameter(typeof(CacheDataLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IPickListMapper), typeof(PtmPickListMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ProposalTypeLULoader)), new ResolvedParameter(typeof(ProposalClassLULoader)), new ResolvedParameter(typeof(TypeOfRequestLULoader)), new ResolvedParameter(typeof(LineOfBusinessDataLoader)), new ResolvedParameter(typeof(ProgramAreaDataLoader)), new ResolvedParameter(typeof(ContractTypeLULoader)), new ResolvedParameter(typeof(ContractTypeGroupLULoader))));

            #endregion

            #region Register Mediators

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalMediator), typeof(ProposalMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ProposalLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISystemPermissionMediator), typeof(SystemPermissionMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(SystemPermissionMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IUserMediator), typeof(UserMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(UserMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IProposalPermissionMediator), typeof(ProposalPermissionMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ProposalPermissionMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IChecklistMediator), typeof(ChecklistMediator), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ProposalChecklistLoader))));
            #endregion

            #region Register Action Logic

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(AdminControllerLogic), typeof(AdminControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IHtmlHelper)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IUserMediator)), new ResolvedParameter(typeof(ISystemPermissionMapper)), new ResolvedParameter(typeof(ISystemPermissionMediator)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IOrgStructureDataMapper)), new ResolvedParameter(typeof(IManageProposalInfoLoader)), new ResolvedParameter(typeof(IBulkArchiveLoader)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader)), new ResolvedParameter(typeof(ApprovalsControllerLogic))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(HomeControllerLogic), typeof(HomeControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IOrgStructureDataMapper)), new ResolvedParameter(typeof(ICache)), new ResolvedParameter(typeof(ICacheWarmer)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader)), new ResolvedParameter(typeof(GenBOE.DataBridge.Common.Interfaces.ISecurityUserAuthorizationsDataLoader)), new ResolvedParameter(typeof(GenBOE.DataBridge.Common.Interfaces.ISecurityAccess))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ReportsControllerLogic), typeof(ReportsControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IReportsLoader)), new ResolvedParameter(typeof(IOrgStructureDataMapper)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(IProposalMediator))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ProposalControllerLogic), typeof(ProposalControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IValidationMethods)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IOrgStructureDataMapper)), new ResolvedParameter(typeof(IProposalPermissionMediator)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(CacheDataLoader)), new ResolvedParameter(typeof(IPickListMapper)), new ResolvedParameter(typeof(IUserLoader)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader)), new ResolvedParameter(typeof(GenBOE.DataBridge.DTO.IPermissionsDTODataLoader)), new ResolvedParameter(typeof(IPtmEmailer))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ChecklistControllerLogic), typeof(ChecklistControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ApprovalsControllerLogic), typeof(ApprovalsControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IUserLoader)), new ResolvedParameter(typeof(IPtmEmailer)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(ApprovalEmailer)), new ResolvedParameter(typeof(AttachmentLoader)), new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(PostSubmittalAttachmentsControllerLogic), typeof(PostSubmittalAttachmentsControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityAccess)), new ResolvedParameter(typeof(IProposalLoader)), new ResolvedParameter(typeof(IUserMapper)), new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(IApprovalsLoader)), new ResolvedParameter(typeof(IProposalChecklistLoader)), new ResolvedParameter(typeof(IChecklistMediator)), new ResolvedParameter(typeof(IProposalMediator)), new ResolvedParameter(typeof(AttachmentLoader))));

            #endregion

            #region Register Misc Classes

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(SiteMasterUtilities), typeof(SiteMasterUtilities), this.GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformation), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), this.GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IValidationMethods), typeof(ValidationMethods), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityAccess), typeof(SecurityAccess), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityMapper)), new ResolvedParameter(typeof(IProposalLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.DataBridge.Common.Interfaces.ISecurityAccess), typeof(GenBOE.DataBridge.Common.SecurityAccess), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(GenBOE.DataBridge.DTO.IBoeDTODataLoader))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.DataBridge.DTO.IBoeDTODataLoader), typeof(GenBOE.DataBridge.DTO.BoeDTODataLoader), this.GetLifetimeManager(), new InjectionMember[] { });
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IHtmlHelper), typeof(ActionLogic.GeneralHelper.HtmlHelper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(IUserMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IPtmEmailer), typeof(PtmEmailer), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IDataFetchingScheduler))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ICacheWarmer), typeof(CacheWarmer), this.GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                        new ResolvedParameter(typeof(CacheWarmingUserMapper))));

            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IFullObjectFactory), typeof(FullObjectFactory), this.GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IEmailInformationLoader), typeof(EmailInformationLoader), this.GetLifetimeManager(), new InjectionConstructor());
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IRetriever), typeof(Retriever), this.GetLifetimeManager(), new InjectionConstructor(
                new ResolvedParameter(typeof(IUserMapper)),
                new ResolvedParameter(typeof(IProposalPermissionMapper)),
                new ResolvedParameter(typeof(IProposalChecklistLoader)),
                new ResolvedParameter(typeof(PPRChecklistContentMapper)),
                new ResolvedParameter(typeof(PARChecklistContentMapper))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(ApprovalEmailer), typeof(ApprovalEmailer), this.GetLifetimeManager(), new InjectionConstructor(
                new ResolvedParameter(typeof(IEmailInformationLoader)),
                new ResolvedParameter(typeof(IPtmEmailer))));
            IES.Common.classes.GenBOEUnityContainer.Container.RegisterType(typeof(IDataFetchingScheduler), typeof(DataFetchingScheduler), this.GetLifetimeManager(), new InjectionMember[] { });

            #endregion
        }

        /// <summary>
        /// Get Lifetime Manager
        /// </summary>
        /// <returns>Returns a lifetime manager</returns>
        private ContainerControlledLifetimeManager GetLifetimeManager()
        {
            ContainerControlledLifetimeManager manager = new ContainerControlledLifetimeManager();
            this.lifetimeManagers.Add(manager);
            return manager;
        }

        #endregion Unity Container
    }
}