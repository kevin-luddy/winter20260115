// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE
{
    using System;
    using System.Collections.Generic;
    using System.IO.IsolatedStorage;
    using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.Common.Search;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.CopyBOE;
    using GenBOE.ActionLogic.CustomFields;
	using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.NewValidation;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.ValidationAttributes;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.ActionLogic.WorkspaceTransitions;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.Common.security;
    using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Reference;
    using GenBOE.Objects;
    using GenBOE.Web;
    using GenBOE.Web.Common;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.InterceptionExtension;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private readonly List<ContainerControlledLifetimeManager> _lifetimeManagers = new List<ContainerControlledLifetimeManager>();
        private readonly Logger _log = new Logger(typeof(MvcApplication));
		private readonly HttpClient _SapHttpClient = new HttpClient();

        public MvcApplication()
        { }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allasp}", new { allasp = @".*\.asp(/.*)?" });
            routes.IgnoreRoute("genboe/Content/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(./*)?" });
            routes.IgnoreRoute("Resources/{*pathInfo}");

            routes.MapRoute("HomeRouteGenBOEEmpty", WebConstants.URL_PATTERN_EMPTY,
                new
                {
                    controller = WebConstants.CONTROLLER_HOME,
                    action = WebConstants.ACTION_INDEX
                } // Parameter defaults
            );

            routes.MapRoute("ErrorRoute", WebConstants.URL_PATTERN_ERROR,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_ERROR
                }
            );

            routes.MapRoute(WebConstants.ROUTE_DEFAULT, WebConstants.URL_PATTERN_DEFAULT, // URL with parameters
                new
                {
                    controller = WebConstants.CONTROLLER_WORKSPACE,
                    action = WebConstants.ACTION_INDEX
                } // Parameter defaults
            );

            routes.MapRoute(WebConstants.ROUTE_WORKSPACE, WebConstants.URL_PATTERN_WORKSPACE, // URL with parameters
                new
                {
                    controller = WebConstants.CONTROLLER_WORKSPACE,
                    action = WebConstants.ACTION_INDEX,
                    id = UrlParameter.Optional
                } // Parameter defaults
            );

            routes.MapRoute("CustomFieldRoute", WebConstants.URL_PATTERN_CUSTOM_FIELD,
                new
                {
                    controller = WebConstants.CONTROLLER_WORKSPACE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    customFieldID = UrlParameter.Optional
                }
            );

            routes.MapRoute("WorkspaceResourceRateRoute", WebConstants.URL_PATTERN_WORKSPACE_RATE,
                new
                {
                    controller = WebConstants.CONTROLLER_WORKSPACE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    workspaceResourceRateID = UrlParameter.Optional
                }
            );

            routes.MapRoute("DuplicateTasksRoute", WebConstants.URL_PATTERN_DUPLICATE_TASKS,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    boeID = UrlParameter.Optional,
                    taskType = UrlParameter.Optional
                }
            );

            routes.MapRoute(WebConstants.ROUTE_BOE, WebConstants.URL_PATTERN_BOE,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    boeID = UrlParameter.Optional
                }
            );

            routes.MapRoute("BoeTaskElementRoute", WebConstants.URL_PATTERN_BOE_TASK_ELEMENT,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    boeID = UrlParameter.Optional,
                    taskElementDetailID = UrlParameter.Optional
                }
            );

            routes.MapRoute("BoeTaskElementCopyRoute", WebConstants.URL_PATTERN_BOE_TASK_ELEMENT_COPY_MOQ,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_COPY_MOQ_EQUATION,
                    boeID = UrlParameter.Optional,
                    copyBoeId = UrlParameter.Optional,
                    taskElementDetailID = UrlParameter.Optional,
                    destinationTaskElementId = UrlParameter.Optional
                }
            );

            routes.MapRoute("BoeODCRoute", WebConstants.URL_PATTERN_BOE_ODC,
                new
                {
                    controller = WebConstants.CONTROLLER_GENBOE,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    boeID = UrlParameter.Optional,
                    odcElementID = UrlParameter.Optional
                }
            );

            routes.MapRoute("BoeTravelRoute", WebConstants.URL_PATTERN_BOE_TRAVEL,
               new
               {
                   controller = WebConstants.CONTROLLER_GENBOE,
                   action = WebConstants.ACTION_INVALID_REQUEST,
                   boeID = UrlParameter.Optional,
                   travelElementID = UrlParameter.Optional
               }
           );

            routes.MapRoute("DateShiftRoute", WebConstants.URL_PATTERN_DATESHIFT,
                new
                {
                    controller = WebConstants.CONTROLLER_DATESHIFT,
                    action = WebConstants.ACTION_INDEX,
                    id = UrlParameter.Optional,
                    level = UrlParameter.Optional
                }
           );

            routes.MapRoute(WebConstants.ROUTE_REPORT, WebConstants.URL_PATTERN_REPORTS,
                new
                {
                    controller = WebConstants.CONTROLLER_REPORTS,
                    action = WebConstants.ACTION_INDEX,
                    reportID = UrlParameter.Optional,
                    scopeParam = UrlParameter.Optional,
                    scope = UrlParameter.Optional
                }
            );

            routes.MapRoute("ExportAdminResources", WebConstants.URL_PATTERN_EXPORT_ADMIN_CUSTOM_RESOURCES,
                new
                {
                    controller = WebConstants.CONTROLLER_ADMIN,
                    searchText = UrlParameter.Optional
                });

            routes.MapRoute("ExportWSResources", WebConstants.URL_PATTERN_EXPORT_WS_CUSTOM_RESOURCES,
                new
                {
                    searchText = UrlParameter.Optional
                });

            routes.MapRoute("BoeMaterialRoute", WebConstants.URL_PATTERN_BOE_MATERIAL,
                new
                {
                    controller = WebConstants.CONTROLLER_BOE_MATERIAL,
                    action = WebConstants.ACTION_INVALID_REQUEST,
                    boeID = UrlParameter.Optional,
                    materialID = UrlParameter.Optional
                }
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Application_Start()
        {
            _log.Debug("Starting Application..");
            ConfigureWebApi();

            try { _log.Debug("GetMachineStoreForAssembly : " + string.Join(",", IsolatedStorageFile.GetMachineStoreForAssembly().GetDirectoryNames())); }
            catch { _log.Error("GetMachineStoreForAssembly : <security exception>"); }

            try { _log.Debug("GetMachineStoreForDomain : " + string.Join(",", IsolatedStorageFile.GetMachineStoreForDomain().GetDirectoryNames())); }
            catch { _log.Error("GetMachineStoreForDomain : <security exception>"); }

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            try { AreaRegistration.RegisterAllAreas(); }
            catch (Exception ex) { _log.Error(ex, "FATAL - AreaRegistration.RegisterAllAreas failed."); throw; }

            try { this.InitializeContainer(); }
            catch (Exception ex) { _log.Error(ex, "FATAL - InitializeContainer failed."); throw; }

            try
            {
                IControllerFactory factory = new UnityFactory(GenBOEUnityContainer.Container);
                ControllerBuilder.Current.SetControllerFactory(factory);
            }
            catch (Exception ex) { _log.Error(ex, "FATAL - Unity Factory setup failed."); throw; }

            try { RegisterRoutes(RouteTable.Routes); }
            catch (Exception ex) { _log.Error(ex, "FATAL - RegisterRoutes failed."); throw; }

            try
            {
                DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ServerValidationAttribute), typeof(ServerValidator));
                DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(StartEndDateValidationAttribute), typeof(StartEndDateValidationAttributeAdapter));
                DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            }
            catch (Exception ex) { _log.Error(ex, "FATAL - DataAnnotationsModelValidatorProvider setup failed."); throw; }

            try
            {
                JsonValueProviderFactory vpfJson;
                if ((vpfJson = ValueProviderFactories.Factories.OfType<System.Web.Mvc.JsonValueProviderFactory>().FirstOrDefault()) != null)
                {
                    ValueProviderFactories.Factories.Remove(vpfJson);
                }
                ValueProviderFactories.Factories.Add(new MyJsonValueProviderFactory());  // override MaxJsonLength
            }
            catch (Exception ex) { _log.Error(ex, "FATAL - JsonValueProviderFactory setup failed."); throw; }

            try
            {
                // jim 5/14/2011 - this is kind of a workaround to get the Instance property in
                // the validation factory valued since this is a special class that doesn't fit into
                // the 'normal' Dependency Injection model [ask adam about this]
                GenBOEUnityContainer.Container.Resolve(typeof(ValidationFactory));

            }
            catch (Exception ex) { _log.Error(ex, "FATAL - ValidationFactory setup failed."); throw; }

            try
            {
                ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
                ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
                ModelBinders.Binders.Add(typeof(long), new LongModelBinder());
                ModelBinders.Binders.Add(typeof(long?), new LongModelBinder());
                ModelBinders.Binders.Add(typeof(string), new StringNonPrintableCharRemovalModelBinder());

                ModelBinders.Binders.Add(typeof(RateType), new EnumBinder<RateType>(RateType.NotSet));
                ModelBinders.Binders.Add(typeof(SpreadType), new EnumBinder<SpreadType>(SpreadType.NotSet));
                
                // Uncomment to debug model binding issues.
                //ModelBinders.Binders.DefaultBinder = new DebugModelBinder();
            }
            catch (Exception ex) { _log.Error(ex, "FATAL - ModelBinders setup failed."); throw; }

			try
			{
				// Increase the size of the Regex cache due to the size of the application and number of regular expressions
				Regex.CacheSize = 50;
			}
			catch (Exception ex)
			{
				_log.Error(ex, "Error setting Regex cache size.");
			}
        }

        /// <summary>
        /// Configures Web Api 2 "things" to work in an MVC application
        /// </summary>
        private static void ConfigureWebApi()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(GenBOEUnityContainer.Container);
        }

        // Suppressed because all of these classes are needed to set up dependency injection.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected virtual void InitializeContainer()
        {
            // for more info see http://msdn.microsoft.com/en-us/library/ff660882%28PandP.20%29.aspx

            GenBOEUnityContainer.Container.AddNewExtension<Interception>();
            SystemConfiguration SysConfig = SystemConfiguration.Instance();

            // Register ICache, Cache and Non Cache Data Loader
            GenBOEUnityContainer.Container.RegisterType(typeof(ICache), typeof(MemoryCache), this.GetLifetimeManager(), new InjectionMember[] { });

            GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), -1));
            GenBOEUnityContainer.Container.RegisterType(typeof(CacheDataLoader), typeof(CacheDataLoader), "GenBOEMetricsCache", GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache)), 43200));
            GenBOEUnityContainer.Container.RegisterType(typeof(NonCacheDataLoader), typeof(NonCacheDataLoader), GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(TokenHandling), typeof(TokenHandling), this.GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ITokenService), typeof(TokenService), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICache))));

            // Register Loaders
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEFormIBOEDTODataLoader), typeof(BOEFormIBOEDTODataLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEFormPBOEDTODataLoader), typeof(BOEFormPBOEDTODataLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(ICommonDataLoader), typeof(CommonDataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTMetricLoader), typeof(MSTMetricLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityUserAuthorizationsDataLoader), typeof(SecurityUserAuthorizationsDataLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(SecurityUserAuthorizationsDataLoader), typeof(SecurityUserAuthorizationsDataLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceDTODataLoader), typeof(WorkspaceDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IPermissionsDTODataLoader), typeof(PermissionsDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IPermissionsDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeDTODataLoader), typeof(BoeDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IClinDTODataLoader), typeof(ClinDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IGroupDTODataLoader), typeof(GroupDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IUserDTODataLoader), typeof(UserDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceExportFormatDTODataLoader), typeof(WorkspaceExportFormatDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWbsDTODataLoader), typeof(WbsDTODataLoader), GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEHistoryDTODataLoader), typeof(BOEHistoryDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOECommentDTODataLoader), typeof(BOECommentDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IResourceTypeLoader), typeof(ResourceTypeLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOESearchDTODataLoader), typeof(BOESearchDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceHistoryDTODataLoader), typeof(WorkspaceHistoryDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IResourceDTODataLoader), typeof(ResourceDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ITMResourceRateDTODataLoader), typeof(TMResourceRateDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IPerformingOrgDTODataLoader), typeof(PerformingOrgDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IPerformingOrgListDTODataLoader), typeof(PerformingOrgListDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IResourceListDTODataLoader), typeof(ResourceListDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ICustomFieldDTODataLoader), typeof(CustomFieldDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ICustomFieldValueDTODataLoader), typeof(CustomFieldValueDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceSearchDTODataLoader), typeof(WorkspaceSearchDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceVariableDTODataLoader), typeof(WorkspaceVariableDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IFindReplaceDTODataLoader), typeof(FindReplaceDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IGenBOEMetricsDataLoader), typeof(GenBOEMetricsDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeTaskElementCustomFieldValueXREFLoader), typeof(BoeTaskElementCustomFieldValueXREFLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTypeTableCustomFieldValueXREFLoader), typeof(MoqTypeTableCustomFieldValueXREFLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ITravelTripCustomFieldValueXREFLoader), typeof(TravelTripCustomFieldValueXREFLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ITravelTripTaskElementCustomFieldValueXREFLoader), typeof(TravelTripTaskElementCustomFieldValueXREFLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ILaborTypeCustomFieldValueXREFLoader), typeof(LaborTypeCustomFieldValueXREFLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IResourceTypeLoader), typeof(ResourceTypeLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IResourceSpreadLoader), typeof(ResourceSpreadLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IOrdinaryVariableLoader), typeof(OrdinaryVariableLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTZoneTravelOriginDTODataLoader), typeof(MSTZoneTravelOriginDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IMSTZoneTravelOriginDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTZoneTravelDestinationDTODataLoader), typeof(MSTZoneTravelDestinationDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IMSTZoneTravelDestinationDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTZoneTravelResourceDTODataLoader), typeof(MSTZoneTravelResourceDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IMSTZoneTravelResourceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTTravelNonzoneFeesAndCostsDTODataLoader), typeof(MSTTravelNonzoneFeesAndCostsDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IMSTTravelNonzoneFeesAndCostsDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IProjectMapSpreadLoader), typeof(ProjectMapSpreadLoader), GetLifetimeManager(), new InjectionMember[] { });
            GenBOEUnityContainer.Container.RegisterType(typeof(IProjectMapDataLoader), typeof(ProjectMapDataLoader), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IProjectMapSpreadLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(GenTRAC.DataBridge.DTO.IProposalLoader), typeof(GenTRAC.DataBridge.DTO.ProposalLoader), this.GetLifetimeManager(), new InjectionConstructor()).Configure<Interception>().SetInterceptorFor<GenTRAC.DataBridge.DTO.IProposalLoader>(new InterfaceInterceptor());
            
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeTaskElementDTODataLoader), typeof(BoeTaskElementDTODataLoader), GetLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter(typeof(IResourceTypeLoader)),
                    new ResolvedParameter(typeof(IResourceSpreadLoader)),
                    new ResolvedParameter(typeof(IOrdinaryVariableLoader)),
                    new ResolvedParameter(typeof(IBoeTaskElementCustomFieldValueXREFLoader)),
                    new ResolvedParameter(typeof(ILaborTypeCustomFieldValueXREFLoader)))).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());

            GenBOEUnityContainer.Container.RegisterType(typeof(IProPricerDTODataLoader), typeof(ProPricerDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceVersionMetaDataDTODataLoader), typeof(WorkspaceVersionMetaDataDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IOtherDirectCostDTODataLoader), typeof(OtherDirectCostDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMiscTravelRateDTOLoader), typeof(MiscTravelRateDTOLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IEscalationRatesDTOLoader), typeof(EscalationRatesDTOLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IOffloadRatesDTOLoader), typeof(OffloadRatesDTOLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMileReimbursementRateDTOLoader), typeof(MileReimbursementRateDTOLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMaterialDTODataLoader), typeof(MaterialDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ITripDTODataLoader), typeof(TripDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(RMSZoneTravelRatesFeesDataLoader), typeof(RMSZoneTravelRatesFeesDataLoader), GetLifetimeManager(), new InjectionConstructor(
                    new ResolvedParameter(typeof(IEscalationRatesDTOLoader)),
                    new ResolvedParameter(typeof(IMSTTravelNonzoneFeesAndCostsDTODataLoader)))).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
			GenBOEUnityContainer.Container.RegisterType(typeof(ISkillMixDTOLoader), typeof(SkillMixDTOLoader), GetLifetimeManager(), new InjectionMember[] { });

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelDTODataLoader), typeof(MSTTravelDTODataLoader), GetLifetimeManager(), new InjectionConstructor(
                    new ResolvedParameter(typeof(ITravelTripTaskElementCustomFieldValueXREFLoader)),
                    new ResolvedParameter(typeof(ITravelTripCustomFieldValueXREFLoader)))).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());

                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelExtendedCostExporter), typeof(TravelExtendedCostExporterRMS), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelUnitCostExporter), typeof(TravelUnitCostExporterRMS), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
                    break;
                case CompanyConfiguration.SpaceSystems:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelDTODataLoader), typeof(TravelDTODataLoader), GetLifetimeManager(), new InjectionConstructor(
                    new ResolvedParameter(typeof(ITravelTripTaskElementCustomFieldValueXREFLoader)),
                    new ResolvedParameter(typeof(ITravelTripCustomFieldValueXREFLoader)))).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());

                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelExtendedCostExporter), typeof(TravelExtendedCostExporter), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelUnitCostExporter), typeof(TravelUnitCostExporter), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(ILocationDTODataLoader), typeof(LocationDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IPerDiemDTODataLoader), typeof(PerDiemDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeApproverResponseDTODataLoader), typeof(BoeApproverResponseDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IInUseDataLoader), typeof(InUseDataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IBOESearchDTODataLoader), typeof(BOESearchDTODataLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IBOESearchDTODataLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(IOrdinaryVariableLoader), typeof(OrdinaryVariableLoader), GetLifetimeManager(), new InjectionMember[] { }).Configure<Interception>().SetInterceptorFor<IOrdinaryVariableLoader>(new InterfaceInterceptor());

            // Register Mappers
            // If using cache, use typeof(NonCacheDataLoader) to inject into the constructor
            // If not using cache use typeof(NonCacheDataLoader) to inject into the constructor
            GenBOEUnityContainer.Container.RegisterType(typeof(ICommonDataMapper), typeof(CommonDataMapper), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICommonDataLoader)), new ResolvedParameter(typeof(CacheDataLoader))));

            // Mediator Classes
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeTaskElementMediator), typeof(BoeTaskElementMediator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeMediator), typeof(BoeMediator), GetLifetimeManager(), new InjectionConstructor(
                new ResolvedParameter(typeof(UserDTODataLoader)), 
                new ResolvedParameter(typeof(IBoeDTODataLoader)),
                new ResolvedParameter(typeof(IPermissionsDTODataLoader))));

            _SapHttpClient.Timeout = Constants.HTTP_TIMEOUT;
			GenBOEUnityContainer.Container.RegisterType(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient), typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient), GetLifetimeManager(), new InjectionConstructor(ConfigurationUtilities.GetAppSetting("IESSAPUrl"), _SapHttpClient));

            // Controller Logic
            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOELaborControllerLogic), typeof(BOELaborControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                        new ResolvedParameter(typeof(IBOEStateMachine)),
                        new ResolvedParameter(typeof(IBoeMediator)),
                        new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                        new ResolvedParameter(typeof(IFullObjectFactory)),
                        new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader)),
                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                        new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                        new ResolvedParameter(typeof(IOrdinaryVariableLoader)),
                        new ResolvedParameter(typeof(IMSTMetricLoader)),
                        new ResolvedParameter(typeof(TaskElementValidation)),
                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
                        new ResolvedParameter(typeof(ICommonDataMapper)),
                        new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                        new ResolvedParameter(typeof(IMoqTypeDataLoader)),
                        new ResolvedParameter(typeof(IValidateBOE)),
                        new ResolvedParameter(typeof(IMoqTableExporter)),
                        new ResolvedParameter(typeof(IMoqTableImporter)),
                        new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
                        new ResolvedParameter(typeof(ITokenService)),
						new ResolvedParameter(typeof(ICache)),
						new ResolvedParameter(typeof(ISkillMixDTOLoader))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEControllerLogic), typeof(BOEControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IMSTMetricLoader)),
                        new ResolvedParameter(typeof(IBOESummary)),
                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                        new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                        new ResolvedParameter(typeof(IFullObjectFactory)),
                        new ResolvedParameter(typeof(IBOEExporter)),
                        new ResolvedParameter(typeof(IBOECustomExporter)),
                        new ResolvedParameter(typeof(IGenBOEControllerLogic)),
                        new ResolvedParameter(typeof(IBoeMediator)),
                        new ResolvedParameter(typeof(IValidationHelper)),
                        new ResolvedParameter(typeof(IBOECommentDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeEmailer)),
                        new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                        new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEStateMachine)),
                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                        new ResolvedParameter(typeof(IBOELaborControllerLogic)),
                        new ResolvedParameter(typeof(IValidateBOE)),
                        new ResolvedParameter(typeof(ISecurityInformation)),
                        new ResolvedParameter(typeof(IBOESearchDTODataLoader)),
                        new ResolvedParameter(typeof(ISecurityAccess)),
                        new ResolvedParameter(typeof(IBoeTaskElementRecalculation)),
                        new ResolvedParameter(typeof(IBOEImporter)),
                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
                        new ResolvedParameter(typeof(IConflictBOE)),
                        new ResolvedParameter(typeof(INestedWBSUtilities)),
                        new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                        new ResolvedParameter(typeof(IProjectMapDataLoader)),
                        new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader)),
                        new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                        new ResolvedParameter(typeof(IMoqTypeDataLoader)),
                        new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                        new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
                        new ResolvedParameter(typeof(ITokenService))));
					GenBOEUnityContainer.Container.RegisterType(typeof(IBOEOtherDirectCostControllerLogic), typeof(BOEOtherDirectCostControllerLogicMST), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEMaterialControllerLogic), typeof(BOEMaterialControllerLogicMST), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IGenBOEControllerLogic), typeof(GenBOEControllerLogicMST), GetLifetimeManager(), new InjectionMember[] { });
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelControllerLogic), typeof(TravelControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                         new ResolvedParameter(typeof(ITravelDTODataLoader)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));

                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOELaborControllerLogic), typeof(BOELaborControllerLogicSpace), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                        new ResolvedParameter(typeof(IBOEStateMachine)),
                        new ResolvedParameter(typeof(IBoeMediator)),
                        new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                        new ResolvedParameter(typeof(IFullObjectFactory)),
                        new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader)),
                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                        new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                        new ResolvedParameter(typeof(IOrdinaryVariableLoader)),
                        new ResolvedParameter(typeof(TaskElementValidation)),
                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
                        new ResolvedParameter(typeof(ICommonDataMapper)),
                        new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                        new ResolvedParameter(typeof(IMoqTypeDataLoader)),
                        new ResolvedParameter(typeof(IValidateBOE)),
                        new ResolvedParameter(typeof(IMoqTableExporter)),
                        new ResolvedParameter(typeof(IMoqTableImporter)),
                        new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
                        new ResolvedParameter(typeof(ITokenService)),
						new ResolvedParameter(typeof(ICache)),
						new ResolvedParameter(typeof(ISkillMixDTOLoader))));
					GenBOEUnityContainer.Container.RegisterType(typeof(IBOEControllerLogic), typeof(BOEControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
						new ResolvedParameter(typeof(IBOESummary)),
						new ResolvedParameter(typeof(IUserDTODataLoader)),
						new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
						new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
						new ResolvedParameter(typeof(IFullObjectFactory)),
						new ResolvedParameter(typeof(IBOEExporter)),
						new ResolvedParameter(typeof(IBOECustomExporter)),
						new ResolvedParameter(typeof(IGenBOEControllerLogic)),
						new ResolvedParameter(typeof(IBoeMediator)),
						new ResolvedParameter(typeof(IValidationHelper)),
						new ResolvedParameter(typeof(IBOECommentDTODataLoader)),
						new ResolvedParameter(typeof(IBoeEmailer)),
						new ResolvedParameter(typeof(IBoeTaskElementMediator)),
						new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
						new ResolvedParameter(typeof(IBOEStateMachine)),
						new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
						new ResolvedParameter(typeof(IBOELaborControllerLogic)),
						new ResolvedParameter(typeof(IValidateBOE)),
						new ResolvedParameter(typeof(ISecurityInformation)),
						new ResolvedParameter(typeof(IBOESearchDTODataLoader)),
						new ResolvedParameter(typeof(ISecurityAccess)),
						new ResolvedParameter(typeof(IBoeTaskElementRecalculation)),
						new ResolvedParameter(typeof(IBOEImporter)),
						new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
						new ResolvedParameter(typeof(IConflictBOE)),
						new ResolvedParameter(typeof(INestedWBSUtilities)),
						new ResolvedParameter(typeof(IProjectMapDataLoader)),
						new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader)),
						new ResolvedParameter(typeof(IRteTemplateDataLoader)),
						new ResolvedParameter(typeof(IMoqTypeDataLoader)),
						new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
						new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
						new ResolvedParameter(typeof(ITokenService))));
					GenBOEUnityContainer.Container.RegisterType(typeof(IBOEOtherDirectCostControllerLogic), typeof(BOEOtherDirectCostControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEMaterialControllerLogic), typeof(BOEMaterialControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IGenBOEControllerLogic), typeof(GenBOEControllerLogic), GetLifetimeManager(), new InjectionConstructor());

                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelControllerLogic), typeof(TravelControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(ITravelDTODataLoader)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOELaborControllerLogic), typeof(BOELaborControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                        new ResolvedParameter(typeof(IBOEStateMachine)),
                        new ResolvedParameter(typeof(IBoeMediator)),
                        new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                        new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                        new ResolvedParameter(typeof(IFullObjectFactory)),
                        new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader)),
                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                        new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                        new ResolvedParameter(typeof(IOrdinaryVariableLoader)),
                        new ResolvedParameter(typeof(TaskElementValidation)),
                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
                        new ResolvedParameter(typeof(ICommonDataMapper)),
                        new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                        new ResolvedParameter(typeof(IMoqTypeDataLoader)),
                        new ResolvedParameter(typeof(IValidateBOE)),
                        new ResolvedParameter(typeof(IMoqTableExporter)),
                        new ResolvedParameter(typeof(IMoqTableImporter)),
                        new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
                        new ResolvedParameter(typeof(ITokenService)),
						new ResolvedParameter(typeof(ICache)),
						new ResolvedParameter(typeof(ISkillMixDTOLoader))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEControllerLogic), typeof(BOEControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                        new ResolvedParameter(typeof(IBOESummary)),
                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                        new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                        new ResolvedParameter(typeof(IFullObjectFactory)),
                        new ResolvedParameter(typeof(IBOEExporter)),
                        new ResolvedParameter(typeof(IBOECustomExporter)),
                        new ResolvedParameter(typeof(IGenBOEControllerLogic)),
                        new ResolvedParameter(typeof(IBoeMediator)),
                        new ResolvedParameter(typeof(IValidationHelper)),
                        new ResolvedParameter(typeof(IBOECommentDTODataLoader)),
                        new ResolvedParameter(typeof(IBoeEmailer)),
                        new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                        new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEStateMachine)),
                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                        new ResolvedParameter(typeof(IBOELaborControllerLogic)),
                        new ResolvedParameter(typeof(IValidateBOE)),
                        new ResolvedParameter(typeof(ISecurityInformation)),
                        new ResolvedParameter(typeof(IBOESearchDTODataLoader)),
                        new ResolvedParameter(typeof(ISecurityAccess)),
                        new ResolvedParameter(typeof(IBoeTaskElementRecalculation)),
                        new ResolvedParameter(typeof(IBOEImporter)),
                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker)),
                        new ResolvedParameter(typeof(IConflictBOE)),
                        new ResolvedParameter(typeof(INestedWBSUtilities)),
                        new ResolvedParameter(typeof(IMoqTypeDataLoader)),
                        new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                        new ResolvedParameter(typeof(GenBOE.ActionLogic.IESSAPClient.IESSAPClient)),
                        new ResolvedParameter(typeof(ITokenService))));
					GenBOEUnityContainer.Container.RegisterType(typeof(IBOEOtherDirectCostControllerLogic), typeof(BOEOtherDirectCostControllerLogic), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEMaterialControllerLogic), typeof(BOEMaterialControllerLogic), GetLifetimeManager(), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IGenBOEControllerLogic), typeof(GenBOEControllerLogic), GetLifetimeManager(), new InjectionMember[] { });
                    GenBOEUnityContainer.Container.RegisterType(typeof(ITravelControllerLogic), typeof(TravelControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(ITravelDTODataLoader)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));
                    break;
            }

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IHomeControllerLogic), typeof(HomeControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(IActiveDirectoryUtilities))
                                                                                                                             ));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceControllerLogic), typeof(WorkspaceControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(IWorkspaceDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ITMResourceRateDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                                                                                                                              new ResolvedParameter(typeof(IInUseDataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                              new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                              new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(FullBoeDataImporter)),
                                                                                                                              new ResolvedParameter(typeof(IBOELaborControllerLogic)),
                                                                                                                              new ResolvedParameter(typeof(IFullWorkspaceRecalculation)),
                                                                                                                              new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IEscalationRatesDTOLoader)),
                                                                                                                              new ResolvedParameter(typeof(IMSTTravelNonzoneFeesAndCostsDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ICustomFieldDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                                                                                                                              new ResolvedParameter(typeof(IProjectMapDataLoader)),
                                                                                                                              new ResolvedParameter(typeof(BoePickListMapper)),
                                                                                                                              new ResolvedParameter(typeof(GenTRAC.DataBridge.DTO.PtmPickListMapper)),
                                                                                                                              new ResolvedParameter(typeof(ContractTypeLoader)),
                                                                                                                              new ResolvedParameter(typeof(WorkspaceExporter)),
                                                                                                                              new ResolvedParameter(typeof(IMoqTypeDataLoader)),
																															  new ResolvedParameter(typeof(IBOEStateMachine)),
																															  new ResolvedParameter(typeof(IBoeMediator))));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IAdminControllerLogic), typeof(AdminControllerLogicMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                              new ResolvedParameter(typeof(IResourceListDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IMSTZoneTravelOriginDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IMSTZoneTravelDestinationDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IMSTTravelNonzoneFeesAndCostsDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IPermissionsDTODataLoader))));
                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IAdminControllerLogic), typeof(AdminControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                              new ResolvedParameter(typeof(IResourceListDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IPermissionsDTODataLoader))));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceControllerLogic), typeof(WorkspaceControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(IWorkspaceDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ITMResourceRateDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                                                                                                                              new ResolvedParameter(typeof(IInUseDataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                              new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                              new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(FullBoeDataImporter)),
                                                                                                                              new ResolvedParameter(typeof(IBOELaborControllerLogic)),
                                                                                                                              new ResolvedParameter(typeof(IFullWorkspaceRecalculation)),
                                                                                                                              new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(ICustomFieldDTODataLoader)),
                                                                                                                              new ResolvedParameter(typeof(IProjectMapDataLoader)),
                                                                                                                              new ResolvedParameter(typeof(BoePickListMapper)),
                                                                                                                              new ResolvedParameter(typeof(GenTRAC.DataBridge.DTO.PtmPickListMapper)),
                                                                                                                              new ResolvedParameter(typeof(ContractTypeLoader)),
                                                                                                                              new ResolvedParameter(typeof(WorkspaceExporter)),
                                                                                                                              new ResolvedParameter(typeof(IMoqTypeDataLoader)),
																															  new ResolvedParameter(typeof(IBOEStateMachine)),
																															  new ResolvedParameter(typeof(IBoeMediator))));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IHomeControllerLogic), typeof(HomeControllerLogicSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(IActiveDirectoryUtilities))
                                                                                                                             ));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IHomeControllerLogic), typeof(HomeControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                              new ResolvedParameter(typeof(IActiveDirectoryUtilities))
                                                                                                                             ));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IAdminControllerLogic), typeof(AdminControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                          new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                          new ResolvedParameter(typeof(IResourceListDTODataLoader))));
                    break;
            }
            GenBOEUnityContainer.Container.RegisterType(typeof(IWBSControllerLogic), typeof(WBSControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                         new ResolvedParameter(typeof(BoeEmailer)),
                                                                                                                         new ResolvedParameter(typeof(BOEStateMachine)),
                                                                                                                         new ResolvedParameter(typeof(BoeTaskElementRecalculation)),
                                                                                                                         new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                         new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                         new ResolvedParameter(typeof(BoeTaskElementMediator)),
                                                                                                                         new ResolvedParameter(typeof(BoeMediator)),
                                                                                                                         new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                         new ResolvedParameter(typeof(IBoeDTODataLoader)),
                                                                                                                         new ResolvedParameter(typeof(IWbsDTODataLoader))
                                                                                                                            ));

            GenBOEUnityContainer.Container.RegisterType(typeof(PermissionControllerLogic), typeof(PermissionControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                         new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                         new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                         new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                                                                                                                         new ResolvedParameter(typeof(ISecurityInformation)),
                                                                                                                         new ResolvedParameter(typeof(IFullObjectFactory))
                                                                                                                        ));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOECommentsControllerLogic), typeof(BOECommentsControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                          new ResolvedParameter(typeof(BOECommentDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(UserDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(ISecurityInformation)),
                                                                                                                          new ResolvedParameter(typeof(BoeEmailer)),
                                                                                                                          new ResolvedParameter(typeof(BoeApproverResponseDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(BoeMediator)),
                                                                                                                          new ResolvedParameter(typeof(BOEStateMachine)),
                                                                                                                          new ResolvedParameter(typeof(IPermissionsDTODataLoader))
                                                                                                                         ));

            GenBOEUnityContainer.Container.RegisterType(typeof(BOEZoneTravelControllerLogic), typeof(BOEZoneTravelControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                          new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                          new ResolvedParameter(typeof(IBOELaborControllerLogic)),
                                                                                                                          new ResolvedParameter(typeof(ITravelDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IMSTZoneTravelOriginDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IMSTZoneTravelDestinationDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IMSTZoneTravelResourceDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IGenBOEControllerLogic)),
                                                                                                                          new ResolvedParameter(typeof(IMSTZoneTravelValidator)),
                                                                                                                          new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader))
                                                                                                                          ));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISSRSControllerLogic), typeof(SSRSControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                          new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                                                                                                                          new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(IWorkspaceDTODataLoader)),
                                                                                                                          new ResolvedParameter(typeof(ICommonDataMapper))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IRTETemplatesControllerLogic), typeof(RTETemplatesControllerLogic), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IWorkspaceVersionMetaDataDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IBoeDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IBoeMediator)),
                                                                                                                            new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                                                                                                                            new ResolvedParameter(typeof(IBoeEmailer)),
                                                                                                                            new ResolvedParameter(typeof(IBOEStateMachine)),
                                                                                                                            new ResolvedParameter(typeof(IMoqTypeDataLoader))));

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IReportsControllerLogic), typeof(ReportsControllerLogicMST), this.GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IBOEExporter)),
                        new ResolvedParameter(typeof(BOESummary)),
                        new ResolvedParameter(typeof(IBOECustomExporter)),
                        new ResolvedParameter(typeof(IWorkspaceExportFormatDTODataLoader)),
                        new ResolvedParameter(typeof(IMSTMetricLoader)),
                        new ResolvedParameter(typeof(BOEDiscrepancyReport)),
                        new ResolvedParameter(typeof(ICommonDataMapper)),
                        new ResolvedParameter(typeof(IProposalLoader)),
                        new ResolvedParameter(typeof(IWorkspaceControllerLogic)),
                        new ResolvedParameter(typeof(TravelTripCostCalculation))
                        ));
                    break;
                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IReportsControllerLogic), typeof(ReportsControllerLogicSpaceSystems), this.GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IBOEExporter)),
                        new ResolvedParameter(typeof(BOESummary)),
                        new ResolvedParameter(typeof(IBOECustomExporter)),
                        new ResolvedParameter(typeof(IWorkspaceExportFormatDTODataLoader)),
                        new ResolvedParameter(typeof(BOEDiscrepancyReport)),
                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEFormIBOEDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEFormPBOEDTODataLoader)),
                        new ResolvedParameter(typeof(IInUseDataLoader)),
                        new ResolvedParameter(typeof(IProposalLoader)),
                        new ResolvedParameter(typeof(IWorkspaceControllerLogic)),
                        new ResolvedParameter(typeof(TravelTripCostCalculation))
                        ));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IReportsControllerLogic), typeof(ReportsControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IBOEExporter)),
                        new ResolvedParameter(typeof(BOESummary)),
                        new ResolvedParameter(typeof(IBOECustomExporter)),
                        new ResolvedParameter(typeof(IWorkspaceExportFormatDTODataLoader)),
                        new ResolvedParameter(typeof(BOEDiscrepancyReport)),
                        new ResolvedParameter(typeof(IProposalLoader)),
                        new ResolvedParameter(typeof(IWorkspaceControllerLogic)),
                        new ResolvedParameter(typeof(TravelTripCostCalculation))
                        ));
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEFormControllerLogic), typeof(BOEFormControllerLogic), this.GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IBOEFormIBOEDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEFormPBOEDTODataLoader)),
                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                        new ResolvedParameter(typeof(ITMResourceRateDTODataLoader)),
                        new ResolvedParameter(typeof(IBOEFormExporter)),
                        new ResolvedParameter(typeof(PBOEFormExporter)),
                        new ResolvedParameter(typeof(TMCalculator))
                        ));

            // Register Misc Classes
            GenBOEUnityContainer.Container.RegisterType(typeof(IDataFetchingScheduler), typeof(DataFetchingScheduler), GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityInformation), typeof(SecurityInformation), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    // The SSC security class can be used here.  The MST requirements are identical.  If this changes an MST specific security class should be created.
                    goto case CompanyConfiguration.SpaceSystems;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityAccess), typeof(SecurityAccessSpaceSystems), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeDTODataLoader))));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ISecurityAccess), typeof(SecurityAccess), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeDTODataLoader))));
                    break;
            }
            GenBOEUnityContainer.Container.RegisterType(typeof(IActiveDirectoryUtilities), typeof(ActiveDirectoryUtilities), GetLifetimeManager(), new InjectionConstructor(120)); // time to cache AD calls
            GenBOEUnityContainer.Container.RegisterType(typeof(SiteMasterUtilities), typeof(SiteMasterUtilities), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeEmailer), typeof(BoeEmailer), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                new ResolvedParameter(typeof(ISecurityInformation)),
                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IBOECommentDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IDataFetchingScheduler)),
                                                                                                                                new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                                new ResolvedParameter(typeof(IBoeDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IBOESummary), typeof(BOESummary), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(TravelTripCostCalculation)), new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IValidationHelper), typeof(ValidationHelper), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IBoeTaskElementRecalculation), typeof(BoeTaskElementRecalculation), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)), new ResolvedParameter(typeof(IFullObjectFactory))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEImporter), typeof(BOEImporter), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                new ResolvedParameter(typeof(IUserDTODataLoader)),
                new ResolvedParameter(typeof(IFullObjectFactory)),
                new ResolvedParameter(typeof(IActiveDirectoryUtilities))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IMSTZoneTravelValidator), typeof(MSTZoneTravelValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(ITravelTripCustomFieldValueXREFLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IVariableCircularReferenceChecker), typeof(VariableCircularReferenceChecker), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IConflictBOE), typeof(ConflictBOE), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IResourceDTODataLoader)),
                new ResolvedParameter(typeof(VariableCircularReferenceChecker)),
                new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(INestedWBSUtilities), typeof(NestedWBSUtilities), GetLifetimeManager(), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceStateMachine), typeof(WorkspaceStateMachine), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(NoneToInitializationTransition)),
                new ResolvedParameter(typeof(InitializationToWorkingTransition)),
                new ResolvedParameter(typeof(WorkingToInitTransition)),
                new ResolvedParameter(typeof(InitializationToClosedTransition)),
                new ResolvedParameter(typeof(ClosedToInitializationTransition)),
                new ResolvedParameter(typeof(WorkingToLockedTransition)),
                new ResolvedParameter(typeof(LockedToWorkingTransition)),
                new ResolvedParameter(typeof(WorkingToClosedTransition)),
                new ResolvedParameter(typeof(LockedToCompleteTransition)),
                new ResolvedParameter(typeof(CompleteToLockedTransition)),
                new ResolvedParameter(typeof(LockedToClosedTransition)),
                new ResolvedParameter(typeof(CompleteToWorkingTransition))));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOESummary), typeof(BOESummary), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                            new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader))
                                                                                                                            ));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOEExportConverter), typeof(BOEExportConverter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(TravelTripCostCalculation))
                                                                                                                                    ));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOEExportConverterRMS), typeof(BOEExportConverterRMS), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(MSTZoneTravelResourceDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader))
                                                                                                                                    ));
            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEExporter), typeof(BOEExporterMSTDecorator), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(BOEExporter)),
                                                                                                                                    new ResolvedParameter(typeof(BOEExporterMST))
                                                                                                                                    ));
                    GenBOEUnityContainer.Container.RegisterType(typeof(BOEExporter), typeof(BOEExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                                                                                                                                    new ResolvedParameter(typeof(BOEExportConverterRMS))
                                                                                                                                    ));
                    GenBOEUnityContainer.Container.RegisterType(typeof(BOEExporterMST), typeof(BOEExporterMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                                                                                                                                    new ResolvedParameter(typeof(BOEExportConverterRMS))
                                                                                                                                    ));
                    break;

                case CompanyConfiguration.SpaceSystems: // The ISGS behavior can be used here.  The SSC requirements are identical.  If this changes SSC specific classes should be created.
                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEExporter), typeof(BOEExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                    new ResolvedParameter(typeof(IActiveDirectoryUtilities)),
                                                                                                                                    new ResolvedParameter(typeof(BOEExportConverter))
                                                                                                                                    ));
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(WorkofflineExporter), typeof(WorkofflineExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                            new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(BOEDiscrepancyReport), typeof(BOEDiscrepancyReport), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                new ResolvedParameter(typeof(IFullWorkspaceRecalculation)),
                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IBOEFormExporter), typeof(IBOEFormExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                new ResolvedParameter(typeof(TMCalculator))));
            GenBOEUnityContainer.Container.RegisterType(typeof(PBOEFormExporter), typeof(PBOEFormExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                new ResolvedParameter(typeof(TMCalculator))));

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOECustomExporter), typeof(BOECustomExporterMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                    new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                    new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                    new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                    new ResolvedParameter(typeof(MSTZoneTravelResourceDTODataLoader)),
                                                                                                                    new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader))));
                    // The ISGS logic can be used here.  The MST requirements are identical.  If this changes MST specific classes should be created.
                    GenBOEUnityContainer.Container.RegisterType(typeof(IWorkOfflineImporter), typeof(WorkOfflineImporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableCircularReferenceChecker))));
                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOECustomExporter), typeof(BOECustomExporterSSC), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                        new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IWorkOfflineImporter), typeof(WorkOfflineImporterSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(IVariableCircularReferenceChecker))));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOECustomExporter), typeof(BOECustomExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                                new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IWorkOfflineImporter), typeof(WorkOfflineImporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                    new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                    new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                                    new ResolvedParameter(typeof(IVariableCircularReferenceChecker))));
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(BOEImporter), typeof(BOEImporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                            new ResolvedParameter(typeof(IActiveDirectoryUtilities))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IFullWorkspaceRecalculation), typeof(FullWorkspaceRecalculation), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                new ResolvedParameter(typeof(IBoeTaskElementRecalculation)),
                                                                                                                new ResolvedParameter(typeof(IBOEStateMachine)),
                                                                                                                new ResolvedParameter(typeof(IBoeMediator)),
                                                                                                                new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                                                                                                                new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation))));



            GenBOEUnityContainer.Container.RegisterType(typeof(ProPricerExporter), typeof(ProPricerExporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                        new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                                        new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(IRetriever)),
                                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper))));
            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ICLINImporter), typeof(CLINImporter), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(ICLINExporter), typeof(CLINExporter), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTableExporter), typeof(MoqTableExporterRMS), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTableImporter), typeof(MoqTableImporterRMS), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(WorkspaceExporter), typeof(WorkspaceExporterMST), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                                new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBOEStatusReport)),
                                                                                                                                                new ResolvedParameter(typeof(WbsExporter)),
                                                                                                                                                new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICLINExporter)),
                                                                                                                                                new ResolvedParameter(typeof(TravelUnitCostExporterRMS)),
                                                                                                                                                new ResolvedParameter(typeof(TravelExtendedCostExporterRMS))));
                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(ICLINImporter), typeof(CLINImporterSSC), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(ICLINExporter), typeof(CLINExporterSSC), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTableExporter), typeof(MoqTableExporter), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTableImporter), typeof(MoqTableImporter), new InjectionConstructor());
                    GenBOEUnityContainer.Container.RegisterType(typeof(WorkspaceExporter), typeof(WorkspaceExporterSpaceSystems), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                                new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBOEStatusReport)),
                                                                                                                                                new ResolvedParameter(typeof(WbsExporter)),
                                                                                                                                                new ResolvedParameter(typeof(TravelTripCostCalculation)),
                                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICLINExporter))));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(ITraceTableExporter), typeof(TraceTableExporter), GetLifetimeManager(), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(TripsExporter), typeof(TripsExporter), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IPerDiemDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(TripsImporter), typeof(TripsImporter), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader))));

           GenBOEUnityContainer.Container.RegisterType(typeof(ArtemisImporter), typeof(ArtemisImporter), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(ProjectImporter), typeof(ProjectImporter), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(ResourcesImporter), typeof(ResourcesImporter), new InjectionConstructor(new ResolvedParameter(typeof(IResourceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(PerformingOrgImporter), typeof(PerformingOrgImporter), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(CustomFieldImporter), typeof(CustomFieldImporter), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(LaborTypeAndSpreadImporter), typeof(LaborTypeAndSpreadImporter), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                        new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WbsImporter), typeof(WbsImporter), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                                                 new ResolvedParameter(typeof(IWbsDTODataLoader)),
                                                                                                                                                 new ResolvedParameter(typeof(IRetriever))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WbsExporter), typeof(WbsExporter), GetLifetimeManager(), new InjectionConstructor());

			GenBOEUnityContainer.Container.RegisterType(typeof(IBOEConfidenceReportExporter), typeof(BOEConfidenceReportExporter), GetLifetimeManager(), new InjectionConstructor());

            //Import/merge classes
            GenBOEUnityContainer.Container.RegisterType(typeof(BOEImportMerge), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeDTODataLoader)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(BoeTaskElementImportMerge), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ResourceTypeImportMerge), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)), new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(FullBoeDataImporter), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeMediator)),
                                                                                                                                    new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                                                                                                                                    new ResolvedParameter(typeof(BOEImportMerge)),
                                                                                                                                    new ResolvedParameter(typeof(BoeTaskElementImportMerge)),
                                                                                                                                    new ResolvedParameter(typeof(ResourceTypeImportMerge))));



            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStatusReport), typeof(BOEStatusReportMST), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                            new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                            new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                            new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                            new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                            new ResolvedParameter(typeof(TravelTripCostCalculation))
                                                                                                                                            ));
                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStatusReport), typeof(BOEStatusReportSpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                            new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                            new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                            new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                            new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                            new ResolvedParameter(typeof(TravelTripCostCalculation))
                                                                                                                                            ));
                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStatusReport), typeof(BOEStatusReport), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                        new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                        new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                        new ResolvedParameter(typeof(TravelTripCostCalculation))));
                    break;
            }

			GenBOEUnityContainer.Container.RegisterType(typeof(IBOEConfidenceReport), typeof(BOEConfidenceReport), GetLifetimeManager(), new InjectionConstructor(
																																		new ResolvedParameter(typeof(IBoeDTODataLoader)),
																																		new ResolvedParameter(typeof(IMoqTypeDataLoader)),
																																		new ResolvedParameter(typeof(IRteTemplateDataLoader))));

			GenBOEUnityContainer.Container.RegisterType(typeof(InitializationToWorkingTransition), typeof(InitializationToWorkingTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(LockedToCompleteTransition), typeof(LockedToCompleteTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(ClosedToInitializationTransition), typeof(ClosedToInitializationTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(NoneToInitializationTransition), typeof(NoneToInitializationTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(CompleteToLockedTransition), typeof(CompleteToLockedTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WorkingToInitTransition), typeof(WorkingToInitTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(CompleteToWorkingTransition), typeof(CompleteToWorkingTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(InitializationToClosedTransition), typeof(InitializationToClosedTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WorkingToLockedTransition), typeof(WorkingToLockedTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WorkingToClosedTransition), typeof(WorkingToClosedTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(LockedToWorkingTransition), typeof(LockedToWorkingTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(LockedToClosedTransition), typeof(LockedToClosedTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(IWorkspaceStateTransition), typeof(DefaultWorkspaceTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)), new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(DefaultBOETransition), typeof(DefaultBOETransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(BoeEmailer)),
                                                                                                                                              new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader))));

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    // The SSC BOE transition logic can be used here.  The MST requirements are identical.  If this changes MST specific transition classes should be created.
                    goto case CompanyConfiguration.SpaceSystems;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStateTransition), typeof(DraftToAwaitingApprovalTransitionSpaceSystems), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeEmailer)),
                                                                                                                                                                            new ResolvedParameter(typeof(ISecurityInformation)),
                                                                                                                                                                            new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                                                            new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                                                                                                                                                                            new ResolvedParameter(typeof(IPermissionsDTODataLoader)),
                                                                                                                                                                            new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStateMachine), typeof(BOEStateMachine), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(DefaultBOETransition)), // none to unassigned
                                                                                                                                         new ResolvedParameter(typeof(DefaultBOETransition)), // unassiged to draft
                                                                                                                                         new ResolvedParameter(typeof(DraftToAwaitingApprovalTransitionSpaceSystems)), // draft to awaiting approval
                                                                                                                                         new ResolvedParameter(typeof(AwaitingApprovalToDraftTransition)), // awaiting approval to draft
                                                                                                                                         new ResolvedParameter(typeof(DraftLockedToAwaitingApprovalTransitionSpaceSystems)), // draft-locked to awaiting approval
                                                                                                                                         new ResolvedParameter(typeof(DraftLockedToDraftTransition)), // draft-locked to draft
                                                                                                                                         new ResolvedParameter(typeof(DraftToDraftLockedTransition)), // draft to draft-locked
                                                                                                                                         new ResolvedParameter(typeof(DefaultBOETransition)), // awaiting approval to approved
                                                                                                                                         new ResolvedParameter(typeof(ApprovedToDraftTransition)), // approved to draft
                                                                                                                                         new ResolvedParameter(typeof(IBoeDTODataLoader))));

                    break;

                case CompanyConfiguration.ISGS:
                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStateTransition), typeof(DraftToAwaitingApprovalTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeEmailer)),
                                                                                                                                                                            new ResolvedParameter(typeof(ISecurityInformation)),
                                                                                                                                                                            new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                                                            new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                                                                                                                                                                            new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOEStateMachine), typeof(BOEStateMachine), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(DefaultBOETransition)), // none to unassigned
                                                                                                                                             new ResolvedParameter(typeof(DefaultBOETransition)), // unassigned to draft
                                                                                                                                             new ResolvedParameter(typeof(DraftToAwaitingApprovalTransition)), // draft to awaiting approval
                                                                                                                                             new ResolvedParameter(typeof(AwaitingApprovalToDraftTransition)), // awaiting approval to draft
                                                                                                                                             new ResolvedParameter(typeof(DraftLockedToAwaitingApprovalTransitionSpaceSystems)), // draft-locked to awaiting approval
                                                                                                                                             new ResolvedParameter(typeof(DraftLockedToDraftTransition)), // draft-locked to draft
                                                                                                                                             new ResolvedParameter(typeof(DraftToDraftLockedTransition)), // draft to draft-locked
                                                                                                                                             new ResolvedParameter(typeof(DefaultBOETransition)), // awaiting approval to approved
                                                                                                                                             new ResolvedParameter(typeof(ApprovedToDraftTransition)), // approved to draft
                                                                                                                                             new ResolvedParameter(typeof(IBoeDTODataLoader))));
                    break;
            }
            GenBOEUnityContainer.Container.RegisterType(typeof(AwaitingApprovalToDraftTransition), typeof(AwaitingApprovalToDraftTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeEmailer)),
                                                                                                                                                                        new ResolvedParameter(typeof(ICommonDataMapper)),
                                                                                                                                                                        new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                                                                                                                                                                        new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(ApprovedToDraftTransition), typeof(ApprovedToDraftTransition), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IBoeEmailer)),
                                                                                                                                                        new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                                                                                                                                                        new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(PackageUtilities), typeof(PackageUtilities), GetLifetimeManager(), new InjectionConstructor());



            // BOE Validation
            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IValidateBOE), typeof(ValidateBOEMst), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                new ResolvedParameter(typeof(IMSTMetricLoader)),
                                                                                                                                new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                new ResolvedParameter(typeof(BOECommentsResponsesValidator)),
                                                                                                                                new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMSTZoneTravelValidator)),
                                                                                                                                new ResolvedParameter(typeof(RMSZoneTravelRatesFeesDataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(IRteTemplateDataLoader))));
                    break;
                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IValidateBOE), typeof(ValidateBOESpaceSystems), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                new ResolvedParameter(typeof(BOECommentsResponsesValidator)),
                                                                                                                                new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(IRteTemplateDataLoader))));
                    break;

                default:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IValidateBOE), typeof(ValidateBOE), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                new ResolvedParameter(typeof(BOECommentsResponsesValidator)),
                                                                                                                                new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                new ResolvedParameter(typeof(IOffloadRatesDTOLoader)),
                                                                                                                                new ResolvedParameter(typeof(IRteTemplateDataLoader))));
                    break;
            }

            GenBOEUnityContainer.Container.RegisterType(typeof(ValidationFactory), typeof(ValidationFactory), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(WorkspaceUniqueNameValidator)),
                                                                                                                                        new ResolvedParameter(typeof(WorkspaceUniqueShortnameValidator)),
                                                                                                                                        new ResolvedParameter(typeof(IsUserNotGroupValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BoeTaskIDUniqueValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BOEDateValidator)),
                                                                                                                                        new ResolvedParameter(typeof(WBSUniqueNumberValidator)),
                                                                                                                                        new ResolvedParameter(typeof(ResourceUniqueIDValidator)),
                                                                                                                                        new ResolvedParameter(typeof(PerformingOrgUniqueIDValidator)),
                                                                                                                                        new ResolvedParameter(typeof(WBSRenumberValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BOEWBSMoveValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BOECLINMoveValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BoeLaborCostElementExistsValidator)),
                                                                                                                                        new ResolvedParameter(typeof(BOEMaterialElementExistsValidator)),
                                                                                                                                        new ResolvedParameter(typeof(ResourceUniqueDescValidator)),
                                                                                                                                        new ResolvedParameter(typeof(IsUserNotSubcontractorValidator))));

            GenBOEUnityContainer.Container.RegisterType(typeof(WorkspaceUniqueNameValidator), typeof(WorkspaceUniqueNameValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(WorkspaceUniqueShortnameValidator), typeof(WorkspaceUniqueShortnameValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IWorkspaceDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IsUserNotGroupValidator), typeof(IsUserNotGroupValidator), GetLifetimeManager(), new InjectionConstructor());
            GenBOEUnityContainer.Container.RegisterType(typeof(BoeTaskIDUniqueValidator), typeof(BoeTaskIDUniqueValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(WorkspaceVariableUniqueNameValidator), typeof(WorkspaceVariableUniqueNameValidator), GetLifetimeManager(), new InjectionConstructor());

            GenBOEUnityContainer.Container.RegisterType(typeof(BOEDateValidator), typeof(BOEDateValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(WBSUniqueNumberValidator), typeof(WBSUniqueNumberValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IWbsDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ResourceUniqueIDValidator), typeof(ResourceUniqueIDValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ResourceDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(PerformingOrgUniqueIDValidator), typeof(PerformingOrgUniqueIDValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(WBSRenumberValidator), typeof(WBSRenumberValidator), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(VariableCircularReferenceChecker)),
                                                                                                                            new ResolvedParameter(typeof(IFullObjectFactory))
                                                                                                                            ));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOEWBSMoveValidator), typeof(BOEWBSMoveValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(VariableCircularReferenceChecker)), new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOECLINMoveValidator), typeof(BOECLINMoveValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(VariableCircularReferenceChecker)), new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(BOEMaterialElementExistsValidator), typeof(BOEMaterialElementExistsValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(BoeLaborCostElementExistsValidator), typeof(BoeLaborCostElementExistsValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IFullObjectFactory))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ResourceUniqueDescValidator), typeof(ResourceUniqueDescValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IResourceDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(IsUserNotSubcontractorValidator), typeof(IsUserNotSubcontractorValidator), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IUserDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(RestoreDefaultOptions), typeof(RestoreDefaultOptions), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                                    new ResolvedParameter(typeof(IInUseDataLoader)),
                                                                                                                                                    new ResolvedParameter(typeof(IWorkspaceDTODataLoader)),
                                                                                                                                                    new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
																																					new ResolvedParameter(typeof(IResourceDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(PerfOrgSearch), typeof(PerfOrgSearch), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));
            GenBOEUnityContainer.Container.RegisterType(typeof(NestedWBSUtilities), typeof(NestedWBSUtilities), GetLifetimeManager(), new InjectionMember[] { });

            switch (SysConfig.CompanyMode)
            {
                case CompanyConfiguration.MST:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IVariableSelectBOEtoSumCalculation), typeof(VariableSelectBOEtoSumCalculationMST), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOECopierCompany), typeof(BOECopierCompanyMST), GetLifetimeManager(), new InjectionConstructor(
                        new ResolvedParameter(typeof(IMSTMetricLoader))));
                    break;

                case CompanyConfiguration.SpaceSystems:
                    GenBOEUnityContainer.Container.RegisterType(typeof(IVariableSelectBOEtoSumCalculation), typeof(VariableSelectBOEtoSumCalculationSpaceSystems), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));
                    GenBOEUnityContainer.Container.RegisterType(typeof(IBOECopierCompany), typeof(BOECopierCompanySpace), GetLifetimeManager(), new InjectionConstructor());
                    break;                
            }
            GenBOEUnityContainer.Container.RegisterType(typeof(VariableCircularReferenceChecker), typeof(VariableCircularReferenceChecker), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                                                                      new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(BoeTaskElementRecalculation), typeof(BoeTaskElementRecalculation), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                                                                                 new ResolvedParameter(typeof(IFullObjectFactory))));

            // BOE Copy Conflict    

            GenBOEUnityContainer.Container.RegisterType(typeof(ConflictBOE), typeof(ConflictBOE), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(VariableCircularReferenceChecker)),
                                                                                                                            new ResolvedParameter(typeof(IPerformingOrgDTODataLoader))));

            GenBOEUnityContainer.Container.RegisterType(typeof(ActionLogic.CopyBOE.BOECopier), typeof(ActionLogic.CopyBOE.BOECopier), GetLifetimeManager(), new InjectionConstructor(
                                                                                                                            new ResolvedParameter(typeof(IBoeDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IBoeTaskElementMediator)),
                                                                                                                            new ResolvedParameter(typeof(IBoeMediator)),
                                                                                                                            new ResolvedParameter(typeof(VariableCircularReferenceChecker)),
                                                                                                                            new ResolvedParameter(typeof(IVariableSelectBOEtoSumCalculation)),
                                                                                                                            new ResolvedParameter(typeof(IFullObjectFactory)),
                                                                                                                            new ResolvedParameter(typeof(IBOECopierCompany)),
                                                                                                                            new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IBoeTaskElementRecalculation)),
                                                                                                                            new ResolvedParameter(typeof(IClinDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IWbsDTODataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                                                                                                                            new ResolvedParameter(typeof(IMoqTypeDataLoader)),
		                                    new ResolvedParameter(typeof(IValidateBOE))));

            GenBOEUnityContainer.Container.RegisterType(typeof(TravelTripCostCalculation), typeof(TravelTripCostCalculation), GetLifetimeManager());
            GenBOEUnityContainer.Container.RegisterType(typeof(TMCalculator), typeof(TMCalculator), GetLifetimeManager());

            GenBOEUnityContainer.Container.RegisterType(typeof(IRetriever), typeof(Retriever), GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IClinDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWbsDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWorkspaceDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBoeDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWorkspaceVariableDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWorkspaceHistoryDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IMaterialDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBOECommentDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IResourceTypeLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBoeTaskElementDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IOtherDirectCostDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IResourceDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ITravelDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBoeApproverResponseDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWorkspaceExportFormatDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICustomFieldDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IWorkspaceVersionMetaDataDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IProPricerDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IPerformingOrgDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IPerformingOrgListDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IEscalationRatesDTOLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ITripDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IPerDiemDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IMiscTravelRateDTOLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IUserDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ILocationDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ICustomFieldValueDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IBOEHistoryDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(ITMResourceRateDTODataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IProjectMapDataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IRteTemplateDataLoader)),
                                                                                                                                                new ResolvedParameter(typeof(IMoqTypeDataLoader))));


            GenBOEUnityContainer.Container.RegisterType(typeof(IFullObjectFactory), typeof(FullObjectFactory), GetLifetimeManager());

            GenBOEUnityContainer.Container.RegisterType(typeof(GenTRAC.DataBridge.Common.Security.ISecurityMapper), typeof(GenTRAC.DataBridge.Common.Security.SecurityMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(GenTRAC.DataBridge.Common.Security.ISecurityUserAuthorizationsDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(GenTRAC.DataBridge.DTO.IUserMapper))));
            GenBOEUnityContainer.Container.RegisterType(typeof(GenTRAC.DataBridge.Common.Security.ISecurityUserAuthorizationsDataLoader), typeof(GenTRAC.DataBridge.Common.Security.SecurityUserAuthorizationsDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities))));
            GenBOEUnityContainer.Container.RegisterType(typeof(GenTRAC.DataBridge.DTO.IUserLoader), typeof(GenTRAC.DataBridge.DTO.UserLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IActiveDirectoryUtilities)))).Configure<Interception>().SetInterceptorFor<GenTRAC.DataBridge.DTO.IUserLoader>(new InterfaceInterceptor());
            GenBOEUnityContainer.Container.RegisterType(typeof(GenTRAC.DataBridge.DTO.IUserMapper), typeof(GenTRAC.DataBridge.DTO.UserMapper), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(GenTRAC.DataBridge.DTO.IUserLoader)), new ResolvedParameter(typeof(CacheDataLoader)), new ResolvedParameter(typeof(ISecurityInformation)), new ResolvedParameter(typeof(IActiveDirectoryUtilities)), new ResolvedParameter(typeof(ICache))));
            GenBOEUnityContainer.Container.RegisterType(typeof(ISystemSettingDTODataLoader), typeof(SystemSettingDTODataLoader), GetLifetimeManager());
            GenBOEUnityContainer.Container.RegisterType(typeof(IRteTemplateDataLoader), typeof(RteTemplateDataLoader), GetLifetimeManager());
            GenBOEUnityContainer.Container.RegisterType(typeof(IMoqTypeDataLoader), typeof(MoqTypeDataLoader), this.GetLifetimeManager(), new InjectionConstructor(new ResolvedParameter(typeof(IMoqTypeTableCustomFieldValueXREFLoader)))).Configure<Interception>().SetInterceptorFor<IWorkspaceDTODataLoader>(new InterfaceInterceptor());

        }

        protected ContainerControlledLifetimeManager GetLifetimeManager()
        {
            ContainerControlledLifetimeManager manager = new ContainerControlledLifetimeManager();
            _lifetimeManagers.Add(manager);
            return manager;
        }

        protected void Application_End()
        {
            _log.Debug("Ending Application..");

            GenBOEUnityContainer.Container.Dispose();

            foreach (ContainerControlledLifetimeManager manager in _lifetimeManagers)
            {
                manager.Dispose();
            }
        }
    }
}