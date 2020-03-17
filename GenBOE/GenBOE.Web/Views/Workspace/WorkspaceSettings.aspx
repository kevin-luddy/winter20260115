<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    WorkspaceSettings - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        var WorkspaceSettings = new Widget();

        angular.module('genboe').value('WorkspaceEmailModelView', 
        {
            controller: '<%: WebConstants.CONTROLLER_WORKSPACE %>',
            getWorkspaceEmailsAction: '<%: WebConstants.ACTION_GET_WORKSPACE_EMAIL_PREFERENCES %>',
            saveAction: '<%: WebConstants.ACTION_SAVE_WORKSPACE_EMAIL_PREFERENCES %>',
            workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace()%>'
        });

        WorkspaceSettings.LoadWorkspaceSettingsJump = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_SETTINGS_JUMP %>', ''), "JUMP_LOADED");
        }

        WorkspaceSettings.LoadWorkspaceIdentification = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_IDENTIFICATION %>', ''), "ID_LOADED");
        }

        WorkspaceSettings.LoadOutputFormat = function() {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_OUTPUT_FORMAT %>', ''), "OUTPUT_LOADED");
        }

        WorkspaceSettings.LoadWorkspaceStatus = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS %>', ''), "STATUS_LOADED");
        }

        WorkspaceSettings.LoadWorkspaceStatusHistory = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_STATUS_HISTORY_GRID %>', ''), "STATUS_HISTORY_LOADED");
        }

        WorkspaceSettings.LoadSumofBOEWorkspaceVariables = function() {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_SUM_OF_BOE_VARIABLES %>', ''), "VARIABLES_LOADED");
        }

        WorkspaceSettings.LoadDiscreteWorkspaceVariables = function() {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_DISCRETE_VARIABLES %>', ''), "VARIABLES_LOADED");
        }

        WorkspaceSettings.LoadWorkspaceAllowSearch = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_ALLOW_SEARCH %>', ''), "ALLOW_SEARCH_LOADED");
        }

        WorkspaceSettings.LoadCustomFields = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_GRID %>', ''), "CUSTOM_FIELDS_LOADED");
        }
        WorkspaceSettings.LoadCustomFieldResource = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_RESOURCE %>', ''), "CUSTOM_FIELD_RESOURCE_LOADED");
        }
        WorkspaceSettings.LoadCustomFieldPerformingOrg = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD_PERFORMING_ORG %>', ''), "CUSTOM_FIELD_PERFORMING_ORG_LOADED");
        }
        WorkspaceSettings.LoadBackupVersions = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_BACKUP_VERSIONS %>', ''), "BACKUP_VERSIONS_LOADED");
        }
        WorkspaceSettings.LoadResourceRatesTM = function() {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_TM %>', ''), "RESOURCE_RATES_TM_LOADED");
        }
        WorkspaceSettings.LoadAddResourceRateTM = function () {
            var urlHash = WorkspaceSettings.currentHash.substring(1).split('/');
            
            if (urlHash[2] && !isNaN(Number(urlHash[2]))) {
                WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_ADD_WORKSPACE_RESOURCE_RATE_TM %>', 'rate/' + urlHash[2]), "ADD_RESOURCE_RATE_TM_LOADED");
            }
            else {
                WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_ADD_WORKSPACE_RESOURCE_RATE_TM %>', ''), "ADD_RESOURCE_RATE_TM_LOADED");
            }
        }
        WorkspaceSettings.LoadWorkspaceEmailPreferences = function () {
            WorkspaceSettings.RetrievePage(CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_WORKSPACE_EMAIL_PREFERENCES %>', ''), "WORKSPACE_EMAIL_PREFERENCES_LOADED");
        };

        WorkspaceSettings.LoadManageRTETemplates = function () {
            ShowLoadingBox();
            window.location = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_RTE_TEMPLATES %>',
                '<%: WebConstants.ACTION_DISPLAY_MANAGE_RTE_TEMPLATES %>', '');
        };


        WorkspaceSettings.RetrievePage = function (actionURL, trigger) {
            ShowLoadingBox();
             $.ajax({
                type: "POST",
                url: actionURL,
                dataType: 'html',
                success: function (response) {
                    HideLoadingBox();
                    $(".ui-dialog-content").dialog("close");
                    WorkspaceSettings.Content.html(response);

                    WorkspaceSettings.Content.each(function () {
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

        WorkspaceSettings.LoadPage = function () {
            if (Session.isDirty() && window.location.hash != WorkspaceSettings.currentHash) {

                Session.confirmDialog(
                 "Unsaved Changes",
                 "You have selected to leave this tab without saving your current changes.  If you wish to leave the unsaved changes select Yes.  To stay on the page select No.",
                 WorkspaceSettings.FinishLoadPage,
                 WorkspaceSettings.RevertLoadPage);
            } else {
                WorkspaceSettings.FinishLoadPage();
            }
        }

        WorkspaceSettings.RevertLoadPage = function () {
            window.location.hash = WorkspaceSettings.currentHash;
        }

        WorkspaceSettings.FinishLoadPage = function () {

            if (WorkspaceSettings.currentHash != window.location.hash) {

                $(document).trigger('CLEAN_WORKSPACE_SETTINGS_DIRTY');

                WorkspaceSettings.currentHash = window.location.hash;
                var currentPage = WorkspaceSettings.currentHash.substring(1).split('/')[0];

                switch (currentPage) {
                    case "WorkspaceIdentification":
                        WorkspaceSettings.LoadWorkspaceIdentification();
                        break;
                    case "OutputFormat":
                        WorkspaceSettings.LoadOutputFormat();
                        break;
                    case "WorkspaceStatus":
                        WorkspaceSettings.LoadWorkspaceStatus();
                        break;
                    case "WorkspaceStatusHistory":
                        WorkspaceSettings.LoadWorkspaceStatusHistory();
                        break;
                    case "SumofBOEWorkspaceVariables":
                        WorkspaceSettings.LoadSumofBOEWorkspaceVariables();
                        break;
                    case "DiscreteWorkspaceVariables":
                        WorkspaceSettings.LoadDiscreteWorkspaceVariables();
                        break;
                    case "AllowSearch":
                        WorkspaceSettings.LoadWorkspaceAllowSearch();
                        break;
                    case "BOECustomFields":
                        WorkspaceSettings.LoadCustomFields();
                        break;
                    case "Resources":
                        WorkspaceSettings.LoadCustomFieldResource();
                        break;
                    case "PerformingOrgs":
                        WorkspaceSettings.LoadCustomFieldPerformingOrg();
                        break;
                    case "BackupVersions":
                        WorkspaceSettings.LoadBackupVersions();
                        break;
                    case "ResourceRatesTM":
                        WorkspaceSettings.LoadResourceRatesTM();
                        break;
                    case "AddResourceRateTM":
                        WorkspaceSettings.LoadAddResourceRateTM();
                        break;
                    case "WorkspaceEmailPreferences":
                        WorkspaceSettings.LoadWorkspaceEmailPreferences();
                        break;
                    case "ManageRTETemplates":
                        WorkspaceSettings.LoadManageRTETemplates();
                        break;
                    default:
                        WorkspaceSettings.LoadWorkspaceSettingsJump();
                }
            }
        }

        $(function () {
            $('div.main').addClass('workspace-settings');

            WorkspaceSettings.Content = $('#WorkspaceSettingsContent');

            WorkspaceSettings.registerForEvent("WS_LOAD_JUMP", WorkspaceSettings.LoadWorkspaceSettingsJump);
            WorkspaceSettings.registerForEvent("WS_LOAD_ID", WorkspaceSettings.LoadWorkspaceIdentification);
            WorkspaceSettings.registerForEvent("WS_LOAD_OUTPUT", WorkspaceSettings.LoadOutputFormat);
            WorkspaceSettings.registerForEvent("WS_LOAD_STATUS", WorkspaceSettings.LoadWorkspaceStatus);
            WorkspaceSettings.registerForEvent("WS_LOAD_STATUS_HISTORY", WorkspaceSettings.LoadWorkspaceStatusHistory);
            WorkspaceSettings.registerForEvent("WS_LOAD_SUM_OF_BOE_VARIABLES", WorkspaceSettings.LoadSumofBOEWorkspaceVariables);
            WorkspaceSettings.registerForEvent("WS_LOAD_DISCRETE_VARIABLES", WorkspaceSettings.LoadDiscreteWorkspaceVariables);
            WorkspaceSettings.registerForEvent("WS_LOAD_CUSTOM_FIELDS", WorkspaceSettings.LoadCustomFields);
            WorkspaceSettings.registerForEvent("WS_LOAD_CUSTOM_FIELD_RESOURCE", WorkspaceSettings.LoadCustomFieldResource);
            WorkspaceSettings.registerForEvent("WS_LOAD_CUSTOM_FIELD_PERFORMING_ORG", WorkspaceSettings.LoadCustomFieldPerformingOrg);
            WorkspaceSettings.registerForEvent("WS_LOAD_ALLOW_SEARCH", WorkspaceSettings.LoadWorkspaceAllowSearch);
            WorkspaceSettings.registerForEvent("WS_LOAD_BACKUP_VERSIONS", WorkspaceSettings.LoadBackupVersions);
            WorkspaceSettings.registerForEvent("WS_LOAD_RESOURCE_RATES_TM", WorkspaceSettings.LoadResourceRatesTM);
            WorkspaceSettings.registerForEvent("WS_LOAD_ADD_RESOURCE_RATE_TM", WorkspaceSettings.LoadAddResourceRateTM);
            WorkspaceSettings.registerForEvent("WS_LOAD_WORKSPACE_EMAIL_PREFERENCES", WorkspaceSettings.LoadWorkspaceEmailPreferences);
            WorkspaceSettings.registerForEvent("WS_LOAD_MANAGE_RTE_TEMPLATES", WorkspaceSettings.LoadManageRTETemplates);
            
            window.onhashchange = WorkspaceSettings.LoadPage;

            WorkspaceSettings.LoadPage();

            angular.bootstrap(document, ['genboe']);
        });

    </script>

    <div id="WorkspaceSettingsContent"></div>
        <script type="text/javascript">
            // Takes a 5 click to launch the game
            // And you have to be in chrome
            window.addEventListener('click', function (evt) {
                if (evt.detail === 5) {
                    PlayEasterEggGame();
                }
            });

            function KillEasterEggGame() {
                $('.eggs').remove();
                $('.bunny').remove();

                $(document).unbind('keydown');
            }

            function PlayEasterEggGame() {
                var i = 1;
                var animationTimer = 10;
                var lastEgg = 0;
                var currentEgg = 0;

                var randomTop = (Math.floor(Math.random() * (window.screen.availHeight - 300)) + 1);
                var randomLeft = (Math.floor(Math.random() * (window.screen.availWidth - 400)) + 1);

                $('<div class="bunny flipped" style="top: ' + randomTop + 'px; left: ' + randomLeft + 'px;"></div>').insertAfter('.left-column');

                $(document).ready(function(){
                    $(document).bind('keydown',function(e){
                        key  = e.keyCode;
                        if (key == 37) {
                            $(".bunny").removeClass('flipped').animate({ left: "-=10px" }, animationTimer);
                        } else if (key == 38) {
                            $(".bunny").animate({ top: "-=10px" }, animationTimer);
                        } else if (key == 39) {
                            $(".bunny").addClass('flipped').animate({ left: "+=10px" }, animationTimer);
                        } else if (key == 40) {
                            $(".bunny").animate({ top: "+=10px" }, animationTimer);
                        } else if(key == 32) {
                            // SPACE BAR - lay an egg
                            var left = $(".bunny").offset().left;
                            var top = $(".bunny").offset().top + 30;
                            var locationStyle = ' style="top: ' + top + 'px; left: ' + left + 'px;" ';
                            var animationTime = 50;

                            var quitGame = Math.floor(Math.random() * 100) + 1;

                            var eggClass = '';
                            var exit = false;

                            if (quitGame <= 3) {
                                eggClass = 'goldenEgg';
                                exit = true;
                            }
                            else if (quitGame <= 5) {
                                eggClass = "rottenEgg";
                                exit = true;
                            }
                            else {
                                while (currentEgg == lastEgg) { currentEgg = (Math.floor(Math.random() * 5) + 1); }
                                lastEgg = currentEgg;

                                eggClass = 'egg' + currentEgg;
                                animationTime = 2000;
                            }

                            $('<div id="' + 'egg_' + i + '" class="eggs ' + eggClass + '"' + locationStyle + '></div>').insertBefore('#Notification');

                            $('#egg_' + i).hide().show("scale", {}, animationTime);
                            i++;

                            if (exit === true) {
                                setTimeout(
                                  function () {
                                      KillEasterEggGame();
                                  }, 5000);
                            }
                        } else if (key == 27) {
                            // ESCAPE - shut it down
                            KillEasterEggGame();
                        }
                    });
                });
            }
    </script>

</asp:Content>
