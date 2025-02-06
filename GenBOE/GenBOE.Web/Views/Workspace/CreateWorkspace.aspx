<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.ActionLogic.ModelView.Workspace.CreateWorkspacePageModelView>" %>
<%@ Import namespace="IES.Common.classes" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import namespace="Newtonsoft.Json" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	genBOE - Create A Workspace
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Scripts.Render("~/bundles/angularapp") %>
    <%: Scripts.Render("~/bundles/genboe") %>
    <%: Scripts.Render("~/bundles/createWorkspace") %>

    <script type="text/javascript">
        angular.module('genboe').value('CreateWorkspaceModelView',
        {
            IsSSC: <%=(SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems).ToString().ToLower()%>,
            LOBs: <%= JsonConvert.SerializeObject(Model.LineOfBusinessTypes)%>, // Array of PickListDto
            TrackingNumbers: <%= JsonConvert.SerializeObject(Model.TrackingNumbers)%>, // Array of SelectListItem
            ContractTypes: <%= JsonConvert.SerializeObject(Model.ContractTypes)%>, // Array of PickListDto
            ProposalClassTypes: <%= JsonConvert.SerializeObject(Model.ProposalClassTypes)%>, // Array of PickListDto
            ProjectMapTypes: <%= JsonConvert.SerializeObject(Model.ProjectMapTypes)%>, // Array of SelectListItem
            ApplicationUrl: '<%: Model.ApplicationUrl.ToString() %>',
            Controller: '<%= WebConstants.CONTROLLER_WORKSPACE %>',
            ValidateIndentificationAction: '<%= WebConstants.ACTION_VALIDATE_CREATE_WORKSPACE_STEP_ONE%>',
            CreateWorkspaceAction: '<%= WebConstants.ACTION_SAVE_NEW_WORKSPACE%>',
            ShowProjectMap: <%= SiteMasterUtilities.IsProjectMapEnabled.ToString().ToLower()%>,
            ShowEquivalentPersonsOption: <%= GenBOE.Objects.FullObjectHelper.ShowEquivalentPersonsOption.ToString().ToLower()%>,
            LabelLeadPricer: '<%= CommonConstants.LABEL_TEXT_LEAD_PRICER_SSC%>',
            ProjectMapPrecisionDefault: <%= Constants.PROJECT_MAP_DECIMAL_PRECISION_DEFAULT%>,
            CostVolumeLeadPricerNTID: '<%= Model.CostVolumeLeadPricerNTID%>',
            CostVolumeLeadPricerDisplayName: "<%= Model.CostVolumeLeadPricerDisplayName%>",
            GetExactCopyDataAction: '<%:WebConstants.ACTION_GET_EXACT_COPY_DATA %>',
            GetWorkspaceToCopyBOEListAction: '<%:WebConstants.ACTION_GET_BOES_FOR_WORKSPACE_TO_COPY%>',
            GetWorkspaceToCopyDetailsAction: '<%:WebConstants.ACTION_GET_DETAILS_FOR_WORKSPACE_TO_COPY%>',
            UpdateCurrentWorkspaceIdentification: '<%:WebConstants.ACTION_UPDATE_CURRENT_WORKSPACE_IDENTIFICATION%>',
            WorkSpaceNameURL: window.location.protocol + '//' + window.location.host + '/' + 'default/' + 
                '<%= WebConstants.CONTROLLER_WORKSPACE %>' + '/' + '<%:WebConstants.ACTION_IS_WORKSPACE_NAME_AVAILABLE %>',
            WorkSpaceShortNameURL: window.location.protocol + '//' + window.location.host + '/' + 'default/' +
                '<%= WebConstants.CONTROLLER_WORKSPACE %>' + '/' + '<%:WebConstants.ACTION_IS_WORKSPACE_SHORT_NAME_AVAILABLE %>',
            TrackingNumber: '<%= Request.QueryString["TrackingNumber"]%>',
            IsPTMIntegrated: <%= Utilities.IsPTMIntegrated.ToString().ToLower()%>,
            IsAdmin: <%= Model.IsAdmin.ToString().ToLower()%>,
            PtmTrackingNumberNotRequired: <%= Model.PtmTrackingNumberNotRequired.ToString().ToLower() %>,
            GetNextTrackingNumberRevisionAction: '<%= WebConstants.ACTION_GET_NEXT_TRACKING_NUMBER_REVISION%>',
            IsSAPConnectionEnabled: <%: Model.IsSAPConnectionEnabled.ToString().ToLower() %>,
			EnableAssignTaskAuthor: <%: Model.IsAuthorAssignableAtTaskLevelEnabled.ToString().ToLower() %>,
			IsAssignTaskAuthorEnabled: <%: Model.IsAssignTaskAuthorEnabled.ToString().ToLower() %>
        });

        var CreateWorkspace = new Widget("CreateWorkspaceForm");

        CreateWorkspace.CommonBindings = function () {
            GenWidget.prototype.liveValidate($("#WorkspaceName"), { url: event_WorkSpaceNameURL }, "invalid workspace name");
            GenWidget.prototype.liveValidate($("#ShortName"), { url: event_WorkSpaceShortNameURL }, "invalid workspace short name");
        };

        var event_WorkSpaceNameURL = window.location.protocol + '//' + window.location.host + '/' + 'default/' + 
                '<%= WebConstants.CONTROLLER_WORKSPACE %>' + '/' + '<%:WebConstants.ACTION_IS_WORKSPACE_NAME_AVAILABLE %>';
        var event_WorkSpaceShortNameURL = window.location.protocol + '//' + window.location.host + '/' + 'default/' +
                '<%= WebConstants.CONTROLLER_WORKSPACE %>' + '/' + '<%:WebConstants.ACTION_IS_WORKSPACE_SHORT_NAME_AVAILABLE %>';
        $(function () {
            $('.header .title').addClass('genBOE');
            $('.main').addClass('create-workspace');

            $('.help-dialog-close').on('click', function () {
                $(this).parent().prev().click();
            });

            
        });
	</script>

    <div data-ng-app="genboe" data-ng-controller="createWorkspaceController" id="CreateWorkspaceContent">
        <div class="module create-workspace-form">
            <div class="module-content-data create-workspace-form section" id="CreateWorkspace">
                <div class="title">Create Workspace: <span id="CreateWorkspace-StepTitle">{{model.stepTitle}}</span></div>
                <div class="data">
                    <%--Form is needed so that the old error messages work--%>
                    <% Html.BeginForm("", "", FormMethod.Post, new { id = "CreateWorkspaceForm", name = "CreateWorkspaceForm" }); %>
                    <div class="wizard-breadcrumb" id="CreateWorkspace-Breadcrumb">
                        <span data-ng-click="step == 1 || goToStep(1)" data-ng-class="{'prev-step': step> 1, 'current-step': step == 1}">Workspace Type</span> 
                        <span data-ng-show='data.IsAttemptingToImport'>&gt; <span data-ng-click="step<= 2 || goToStep(2)" data-ng-class="{'prev-step': step> 2, 'current-step': step == 2}">Search & Copy Existing Workspace</span> </span>
                        <span data-ng-hide="data.IsAttemptingToImport && data.WSExactCopy">&gt; <span data-ng-click="step<= 3 || goToStep(3)" data-ng-class="{'prev-step': step> 3, 'current-step': step == 3}">Workspace Identification</span> &gt; 
                        <span data-ng-click="step<= 4 || goToStep(4)" data-ng-class="{'prev-step': step> 4, 'current-step': step == 4}">Share &amp; Allow Search Settings</span> </span>&gt; 
                        <span data-ng-class="{'current-step': step == 5}">Verify</span>
                    </div>
                    <gen-validation data-errors="errors"></gen-validation>
                    <ul id="urlValidationBox" class="validation-box"></ul>
                    <div id="CreateWorkspace-Wizard">
                        <div data-ng-cloak data-ng-hide="isDataLoading">
                            <div data-ng-switch="step">
                                <div data-ng-switch-when="1">
                                    <div data-ng-include src="'/Resources/CreateWorkspaceStep1.html'"></div>
                                </div>
                                <div data-ng-switch-when="2">
                                    <div data-ng-include src="'/Resources/CreateWorkspaceStep2.html'"></div>
                                </div>
                                <div data-ng-switch-when="3">
                                    <div data-ng-include src="'/Resources/CreateWorkspaceStep3.html'"></div>
                                </div>
                                <div data-ng-switch-when="4">
                                    <div data-ng-include src="'/Resources/CreateWorkspaceStep4.html'"></div>
                                </div>
                                <div data-ng-switch-when="5">
                                    <div data-ng-include src="'/Resources/CreateWorkspaceStep5.html'"></div>
                                </div>
                            </div>
                            <div id="WorkspaceSearchLoadingBox" data-ng-class="{'display-none': step !== 2 || !model.showWorkspaceSearchBOELoader}" class="page-loading"><div class="loading-box"><div class="loader white"></div><span>Loading...</span></div></div>
                            <div id="WorkspaceSearchBOEList" data-ng-class="{'display-none': step !== 2 || data.WSExactCopy !== false}" style="margin-bottom: 15px;"></div>
                        </div>
                        <div class="loader" data-ng-show="isDataLoading"></div>
                    </div>
         
                    <div class="create-workspace buttons full">
                        <button name="back-button" class="ies" data-ng-show="model.showBackButton" id="CreateWorkspace-Back" data-ng-click="back()" type="button">Back</button>
                        <button name="create-workspace-button" class="ies-action" data-ng-hide="model.showButtonLoader || model.showNextButton" data-ng-click="create()" type="button">Create Workspace</button>
                        <div class="loader" data-ng-show="model.showButtonLoader" id="CreateWorkspace-Loader"></div>
                        <button name="next-button" class="ies-action" data-ng-class="{'disabled':nextButtonDisabled()}" data-ng-show="!model.showButtonLoader && model.showNextButton" data-ng-click="next()" type="button">Next</button>
                        <button name="cancel-button" class="ies left-margin" data-ng-click="cancel()" type="button">Cancel</button>
                    </div>
                    <div data-ng-show="model.showOCINote" class="create-oci-note">
                        <b>Note:</b> Information on this form must not contain<br/>
                        any OCI, classified, export controlled or third party<br/>
                        proprietary information.
                    </div>
                    <% Html.EndForm(); %>
                </div>
            </div>
        </div>
    </div>
    <% Html.RenderAction(WebConstants.ACTION_DISPLAY_WORKSPACE_SEARCH, WebConstants.CONTROLLER_WORKSPACE); %>
</asp:Content>