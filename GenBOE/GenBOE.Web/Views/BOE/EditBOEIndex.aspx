<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	Edit BOE - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<% 
		var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
		int rteFieldSize = ViewBag.RteFieldSize;
	%>
	<%: Scripts.Render("~/bundles/manageTask") %>
	<script type="text/javascript">
		app.value('ManageTaskModel', {
			BaseUrl: '<%=this.ResolveClientUrl("~/")%>',
			workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
			controller: '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
			action: '<%:WebConstants.ACTION_GET_TASK_DATA_MODEL %>',
			saveAction: '<%:WebConstants.ACTION_SAVE_TASK_DATA_MODEL %>',
			exportAction: '<%: WebConstants.ACTION_EXPORT_LABOR_TYPE_AND_SPREAD %>',
			exportOffloadAction: '<%: WebConstants.ACTION_EXPORT_OFFLOAD_RATES %>',
			calculateSpreadAction: '<%: WebConstants.ACTION_CALCULATE_SPREAD %>',
			ImportMoqTablesAction: '<%: WebConstants.ACTION_IMPORT_MOQ_TABLES %>',
			CompleteImportMoqTablesAction: '<%: WebConstants.ACTION_COMPLETE_IMPORT_MOQ_TABLES %>',
			ExportMoqTablesAction: '<%: WebConstants.ACTION_EXPORT_MOQ_TABLES %>',
			boeId: '<%=ViewData["BOEID"]%>',
			ElementsOfCost: <%=serializer.Serialize(ViewData["ElementsOfCost"])%>,
			WBSElements: <%=serializer.Serialize(ViewData["WBSElements"])%>,
			CLINElements: <%=serializer.Serialize(ViewData["CLINElements"])%>,
			SpreadCurvesCost: <%=serializer.Serialize(ViewData["SpreadCurvesCost"])%>,
			SpreadCurvesHours: <%=serializer.Serialize(ViewData["SpreadCurvesHours"])%>,
			RateTypeCost: <%=((int)RateType.Cost).ToString()%>,
			RateTypeHours: <%=((int)RateType.Hours).ToString()%>,
			CostDecimalPrecision: <%=ViewData["CostDecimalPrecision"]%>,
			DecimalPrecision: <%=ViewData["DecimalPrecision"]%>,
			SpreadCurvesDiscreteHours: '<%= (int)SpreadCurves.DiscreteHours %>',
			SpreadCurvesDiscreteCost: '<%= (int)SpreadCurves.DiscreteCost %>',
			RteFieldSize: <%= rteFieldSize %>,
			IsSpace: <%= ((bool)(IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)).ToString().ToLower() %>,
			SapFields: <%=serializer.Serialize(ViewData["SapFields"])%>,
			SapOperators: <%=serializer.Serialize(ViewData["SapOperators"])%>,
			ParseSapFilterAction: '<%: WebConstants.ACTION_PARSE_SAP_FILTER %>',
			ConvertSapFilterAction: '<%: WebConstants.ACTION_CONVERT_SAP_FILTER %>',
			CalculateAllActualsSapAction: '<%: WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP %>',
			CalculateAllActualsSapWithSkillMixAction: '<%: WebConstants.ACTION_CALCULATE_ALL_ACTUALS_SAP_WITH_SKILL_MIX %>',
			ExportActualsSapAction: '<%: WebConstants.ACTION_EXPORT_ACTUALS_SAP %>',
			RefreshSkillMixTableAction: '<%: WebConstants.ACTION_REFRESH_SKILL_MIX_TABLES %>',
            CheckTMRatesAction: '<%: WebConstants.ACTION_CHECK_TM_RATES %>',
			DisableSave: false,
			DisableSaveText: '',
			SapWebiRepository: '<%=RepositoryName.SapWebi.GetDescription()%>',
			SapConnectionEnabled: '<%=ViewData["EnableSAP"]%>'.isTrue(),
			RmsSapDisabledSource: '<%=RepositoryName.User.GetDescription()%>',
			SscSapDisabledSource: '<%=RepositoryName.ConnectionDisabledSapWebi.GetDescription()%>',
			PoPMonthsDivisor: '<%: Constants.POP_MONTHS_DIVISOR %>',
			OneLMXCutOffDate: '<%: Utilities.OneLmxStartDate %>',
			IsBRCEnabled: '<%= Utilities.IsBRCEnabledForWorkspace(SiteMasterUtilities.GetCurrentWorkspace()) %>'.isTrue(),
			IsSkillMixEnabled: '<%= (bool)ViewData["IsSkillMixEnabled"] %>'.isTrue(),
			IsUcotEnabled: '<%= Utilities.IsUCOTEnabled %>'.isTrue(),
			UcotFactor: <%=ViewData["UcotFactor"]%>,
            SkillMixTableHelpUrls: <%=serializer.Serialize(ViewData["SkillMixTableHelpUrls"])%>
		});

		$(function () {
			$(".main").addClass("boe");

			// Get the read-only attribute passed in from the controller
			var EditBOEIndex_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();

			// Create Widget
			var EditBOEIndex = new Widget("EditBOEIndex", EditBOEIndex_ReadOnly);

			if (EditBOEIndex.isReadOnly()) {
				$("#SubmitForReview").hide();
				$("#SubmitForApproval").hide();
			}
		});
    </script>

	<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_HEADER, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>

	<div id="BOESummary" class="boe-summary">
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_SUMMARY, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
	</div>

	<div class="divider"></div>

	<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_DETAILS, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>

	<div class="divider"></div>

	<div class="buttons specialBOEActions">
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_EXPORT, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_VALIDATE, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_OFFLOAD, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_SUBMIT_FOR_REVIEW, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_SEARCH, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
		<% if (Utilities.IsConfidenceReportEnabled) { Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_CONFIDENCE_REPORT_BUTTON, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); } %>
		<% Html.RenderAction(WebConstants.ACTION_DISPLAY_SUBMIT_FOR_APPROVAL, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
	</div>
	<script type="text/javascript">
		angular.element(document).ready(function () {
			angular.bootstrap(document, ['genboe']);
		});
	</script>
</asp:Content>
