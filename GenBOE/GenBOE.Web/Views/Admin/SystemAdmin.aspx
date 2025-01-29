<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    System Administration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Scripts.Render("~/bundles/angularapp") %>
    <%: Scripts.Render("~/bundles/systemAdmin") %>
    <%: Styles.Render("~/Content/siteCss") %>
    <script type="text/javascript">
        var SystemAdmin = new Widget();

        angular.module('genboe').value('SystemEmailModelView', 
        {
            controller: '<%: WebConstants.CONTROLLER_ADMIN %>',
            getSystemEmailsAction: '<%: WebConstants.ACTION_SYSTEM_EMAIL_PREFERENCES %>',
            saveAction: '<%: WebConstants.ACTION_SAVE_SYSTEM_EMAIL_PREFERENCES %>'
        });
    
        angular.module('genboe').value('ManageSystemSettingsModelView',
        {
            controller: '<%: WebConstants.CONTROLLER_ADMIN %>',
            getSystemSettingsAction: '<%: WebConstants.ACTION_SYSTEM_SETTINGS %>',
            saveSystemSettingsAction: '<%: WebConstants.ACTION_SAVE_SYSTEM_SETTINGS %>'
        });

        angular.module('genboe').value('ManageOverdueTrainingModelView',
        {
            controller: '<%: WebConstants.CONTROLLER_ADMIN %>',
            importOverdueTrainingAction: '<%: WebConstants.ACTION_IMPORT_OVERDUE_TRAINING %>'
        });
    
        SystemAdmin.LoadSystemAdminJump = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_SYSTEM_ADMIN_JUMP %>'), "JUMP_LOADED");
        }

        SystemAdmin.LoadManageResources = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_DEFAULT_RESOURCES %>'), "RESOURCE_LOADED");
        }

        SystemAdmin.LoadManageSystemPermissions = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_SYSTEM_PERMISSIONS %>'), "PERMISSIONS_LOADED");
        }

        SystemAdmin.LoadManageCreateWorkspacePermissions = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_CREATE_WORKSPACE_PERMISSIONS %>'), "CREATE_WORKSPACE_PERMISSIONS_LOADED");
        }

        SystemAdmin.LoadManagePerfOrgs = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_DEFAULT_PERF_ORGS %>'), "PERF_ORGS_LOADED");
        }

        SystemAdmin.LoadManageOutputFormatTemplates = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_OUTPUT_FORMAT_TEMPLATE %>'), "MANAGE_OUTPUT_FORMAT_TEMPLATE");
        }

        SystemAdmin.LoadManageTripsForTravel = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_TRIPS %>'), "TRIPS_LOADED");
        }

        SystemAdmin.LoadManageZoneTravel = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_ZONE_TRAVEL %>'), "ZONE_TRAVEL_LOADED");
        }

        SystemAdmin.LoadManageZoneTravelDestinations = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_ZONE_TRAVEL_DESTINATIONS %>'), "ZONE_TRAVEL_DESTINATIONS_LOADED");
        }

        SystemAdmin.LoadManageNonzoneFeesAndCosts = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                '<%: WebConstants.CONTROLLER_ADMIN %>',
                '<%: WebConstants.ACTION_DISPLAY_MANAGE_NONZONE_FEES_AND_COSTS %>'), "NONZONE_FEES_AND_COSTS_LOADED");
        }

        SystemAdmin.LoadManageOffloadRates = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
               '<%: WebConstants.CONTROLLER_ADMIN %>',
               '<%: WebConstants.ACTION_DISPLAY_OFFLOAD_RATES %>'), "OFFLOAD_RATES_LOADED");
        }

        SystemAdmin.LoadManageLegacyResources = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
               '<%: WebConstants.CONTROLLER_ADMIN %>',
               '<%: WebConstants.ACTION_DISPLAY_LEGACY_RESOURCES %>'), "LEGACY_RESOURCES_LOADED");
        }

        SystemAdmin.LoadManageMiscellaneousRates = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_MISC_RATES %>'), "MISC_RATES_LOADED");
        }

        SystemAdmin.LoadManageEscalationRates = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_ESCALATION_RATES %>'), "ESCALATION_RATES_LOADED");
        }

        SystemAdmin.LoadManageReimbursementRate = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_REIMBURSEMENT_RATE %>'), "REIMBURSEMENT_RATE_LOADED");
        }

        SystemAdmin.LoadManageEmailPreferences = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_EMAIL_PREFERENCES %>'), "EMAIL_PREFERENCES_LOADED");
        }

        SystemAdmin.LoadManageSystemSettings = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_MANAGE_SYSTEM_SETTINGS %>'), "MANAGE_SYSTEM_SETTINGS_LOADED");
        }

        SystemAdmin.LoadManageOverdueTraining = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_DISPLAY_OVERDUE_TRAINING %>'), "MANAGE_OVERDUE_TRAINING_LOADED");
        }

        SystemAdmin.LoadManageSystemProPricerExports = function () {
            SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                '<%: WebConstants.ACTION_DISPLAY_SYSTEM_PROPRICER_EXPORTS %>'), "MANAGE_SYSTEM_PROPRICER_EXPORTS_LOADED");
        }

		SystemAdmin.LoadManageUCOT = function () {
			SystemAdmin.RetrievePage(CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
				'<%: WebConstants.ACTION_DISPLAY_MANAGE_UCOT %>'), "MANAGE_UCOT_LOADED");
		};

        SystemAdmin.RetrievePage = function (actionURL, trigger) {
            ShowLoadingBox();
            $.ajax({
                type: "POST",
                url: actionURL,
                dataType: 'html',
                success: function (response) {
                    HideLoadingBox();
                    SystemAdmin.Content.html(response);

                    SystemAdmin.Content.each(function () {
                        var content = $(this);
                        angular.element(document).injector().invoke(
                        [
                            "$compile", function ($compile) {
                                var scope = angular.element(content).scope();
                                $compile(content)(scope);
                            }
                        ]);
                    });

                    $(document).trigger(trigger);
                }
            });
        }

        SystemAdmin.LoadPage = function () {
            if (Session.isDirty() && window.location.hash != SystemAdmin.currentHash) {

                Session.confirmDialog(
                 "Unsaved Changes",
                 "You have selected to leave this tab without saving your current changes.  If you wish to leave the unsaved changes select Yes.  To stay on the page select No.",
                 SystemAdmin.FinishLoadPage,
                 SystemAdmin.RevertLoadPage);
            } else {
                SystemAdmin.FinishLoadPage();
            }
        }

        SystemAdmin.RevertLoadPage = function () {
            window.location.hash = SystemAdmin.currentHash;
        }

        SystemAdmin.FinishLoadPage = function () {
            if (SystemAdmin.currentHash != window.location.hash) {

                $(document).trigger('CLEAN_SYSTEM_ADMIN_DIRTY');

                SystemAdmin.currentHash = window.location.hash;

                var currentPage = window.location.hash.substring(1);
           
                switch (currentPage) {
                    case "ManageDefaultResources":
                        SystemAdmin.LoadManageResources();
                        break;
                    case "SystemAdminPermissions":
                        SystemAdmin.LoadManageSystemPermissions();
                        break;
                    case "CreateWorkspacePermissions":
                        SystemAdmin.LoadManageCreateWorkspacePermissions();
                        break;
                    case "ManageDefaultPerfOrgs":
                        SystemAdmin.LoadManagePerfOrgs();
                        break;
                    case "ManageOutputFormatTemplates":
                        SystemAdmin.LoadManageOutputFormatTemplates();
                        break;
                    case "ManageTripsForTravel":
                        SystemAdmin.LoadManageTripsForTravel();
                        break;
                    case "ManageZoneTravel":
                        SystemAdmin.LoadManageZoneTravel();
                        break;
                    case "ManageZoneTravelDestinations":
                        SystemAdmin.LoadManageZoneTravelDestinations();
                        break;
                    case "ManageNonzoneFeesAndCosts":
                        SystemAdmin.LoadManageNonzoneFeesAndCosts();
                        break;
                    case "ManageMiscRates":
                        SystemAdmin.LoadManageMiscellaneousRates();
                        break;
                    case "ManageEscalationRates":
                        SystemAdmin.LoadManageEscalationRates();
                        break;
                    case "ManageOffloadRates":
                        SystemAdmin.LoadManageOffloadRates();
                        break;
                    case "ManageLegacyResources":
                        SystemAdmin.LoadManageLegacyResources();
                        break;
                    case "ManageMileageReimbursement":
                        SystemAdmin.LoadManageReimbursementRate();
                        break;
                    case "ManageEmailPreferences":
                        SystemAdmin.LoadManageEmailPreferences();
                        break;
                    case "ManageSystemSettings":
                        SystemAdmin.LoadManageSystemSettings();
                        break;
                    case "ManageOverdueTraining":
                        SystemAdmin.LoadManageOverdueTraining();
                        break;
                    case "ManageSystemProPricerExports":
                        SystemAdmin.LoadManageSystemProPricerExports();
                        break;
					case "ManageUCOT":
						SystemAdmin.LoadManageUCOT();
						break;
                    default:
                        SystemAdmin.LoadSystemAdminJump();
                }
            }
        }

        $(function () {
          $('div.main').addClass('system-admin');

            SystemAdmin.Content = $('#SystemAdminContent');

            SystemAdmin.registerForEvent("SA_LOAD_JUMP", SystemAdmin.LoadSystemAdminJump);
            SystemAdmin.registerForEvent("SA_LOAD_RESOURCE", SystemAdmin.LoadManageResources);
            SystemAdmin.registerForEvent("SA_LOAD_PERMISSIONS", SystemAdmin.LoadManageSystemPermissions);
            SystemAdmin.registerForEvent("SA_LOAD_CREATE_WORKSPACE_PERMISSIONS", SystemAdmin.LoadManageCreateWorkspacePermissions);
            SystemAdmin.registerForEvent("SA_LOAD_PERF_ORGS", SystemAdmin.LoadManagePerfOrgs);
            SystemAdmin.registerForEvent("SA_LOAD_TEMPLATES", SystemAdmin.LoadManageOutputFormatTemplates);
            SystemAdmin.registerForEvent("SA_LOAD_TRIPS", SystemAdmin.LoadManageOutputFormatTemplates);
            SystemAdmin.registerForEvent("SA_LOAD_MISC_RATES", SystemAdmin.LoadManageMiscellaneousRates);
            SystemAdmin.registerForEvent("SA_LOAD_ESCALATION_RATES", SystemAdmin.LoadManageEscalationRates);
            SystemAdmin.registerForEvent("SA_LOAD_REIMBURSEMENT_RATE", SystemAdmin.LoadManageReimbursementRate);
            SystemAdmin.registerForEvent("SA_LOAD_EMAIL_PREFERENCES", SystemAdmin.LoadManageEmailPreferences);
            SystemAdmin.registerForEvent("SA_LOAD_MANAGE_SYSTEM_SETTINGS", SystemAdmin.LoadManageSystemSettings);
            SystemAdmin.registerForEvent("SA_LOAD_MANAGE_UCOT", SystemAdmin.LoadManageUCOT);
            window.onhashchange = SystemAdmin.LoadPage;

            SystemAdmin.LoadPage();

            angular.bootstrap(document, ['genboe']);
        });
	</script>
    <div class="module systemAdminContent">
        <div class="module-content-data" id="SystemAdminContent"></div>
    </div>
</asp:Content>
