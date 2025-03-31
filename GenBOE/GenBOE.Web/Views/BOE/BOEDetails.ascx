<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="IES.Common.classes" %>
<% 
	var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">

	var isMaterial = '<%: (bool)ViewData["isMaterial"]  %>' == "True";
	var boeDetailsReloadHistoryEvent = '<%:WebConstants.EVENT_BOEDETAILS_RELOAD_BOE_HISTORY_GRID %>';
	var boeDetailsReloadCommentEvent = '<%:WebConstants.EVENT_BOEDETAILS_RELOAD_BOE_COMMENT_GRID %>';
	var displayTaskElementDetailsEvent = '<%: WebConstants.EVENT_DISPLAY_TASK_ELEMENT_DETAILS %>';
	var displayOdcDetailsEvent = '<%: WebConstants.EVENT_DISPLAY_ODC_ELEMENT_DETAILS %>';
	var displayTravelDetailsEvent = '<%: WebConstants.EVENT_DISPLAY_TRAVEL_ELEMENT_DETAILS %>';
	var displayMaterialDetailsEvent = '<%: WebConstants.EVENT_DISPLAY_MATERIAL_ELEMENT_DETAILS %>';

	var BOEDetails = InitializeBOEDetailsWidget(isMaterial, boeDetailsReloadHistoryEvent, boeDetailsReloadCommentEvent, displayTaskElementDetailsEvent, displayOdcDetailsEvent, displayTravelDetailsEvent,
		displayMaterialDetailsEvent);
	BOEDetails.WSResources = <%= serializer.Serialize(ViewData["WSRESOURCES"])%>;
	BOEDetails.WSResourcesTM = <%= serializer.Serialize(ViewData["WSRESOURCESTM"])%>;
	BOEDetails.WSBusinessResourceCodes = <%= serializer.Serialize(ViewData["WSBUSINESSRESOURCECODES"])%>
	BOEDetails.WSPerfOrgs = <%= serializer.Serialize(ViewData["WSPERFORGS"])%>;
	BOEDetails.WSClins = <%= serializer.Serialize(ViewBag.WsClins)%>;
	BOEDetails.WSWbss = <%= serializer.Serialize(ViewBag.WsWbss)%>;

	BOEDetails.autocompletePerformingOrgs = function (request, response) {
		var toSearch = request.term.toLowerCase();
		returned = $.grep(BOEDetails.WSPerfOrgs, function (p) {
			return p.PerformingOrgName.toLowerCase().indexOf(toSearch) != -1 ||
				p.PerformingOrgDesc.toLowerCase().indexOf(toSearch) != -1;
		})

		response($.map(returned, function (item) {
			return {
				label: item.PerformingOrgName + "-" + item.PerformingOrgDesc,
				value: item.PerformingOrgName,
				ID: item.PerformingOrgID
			}
		}));
	};

	$(function () {

		var boeId = '<%: (int)ViewData["BOEID"] %>';
		var boeController = '<%:WebConstants.CONTROLLER_BOE %>';
		var boeLaborController = '<%: WebConstants.CONTROLLER_BOE_LABOR %>';
		var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
		var displayTaskElementCompositeUrl = CreatePostURL(workspace, boeLaborController,'<%: WebConstants.ACTION_DISPLAY_TASK_ELEMENT %>', 'boe/' + boeId);
		var displayOdcCompositeUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_COMPOSITE %>',
			'boe/' + boeId);
		var displayTravelCompositeUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_TRAVEL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_TRAVEL_COMPOSITE %>',
			'boe/' + boeId);
		var displayZoneTravelCompositeUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_ZONE_TRAVEL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_COMPOSITE %>',
			'boe/' + boeId);
		var displayTaskElementGridUrl = CreatePostURL(workspace, boeController, '<%: WebConstants.ACTION_DISPLAY_TASK_ELEMENT_GRID %>', 'boe/' + boeId);
		var displayBoeSummaryUrl = CreatePostURL(workspace, boeController,'<%: WebConstants.ACTION_DISPLAY_BOE_SUMMARY %>','boe/<%: (int)ViewData["BOEID"] %>');
		var displayBoeCommentUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_COMMENTS %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_COMMENTS %>',
			'boe/' + boeId);
		var displayBoeHistoryUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_HISTORY %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_HISTORY %>',
			'boe/' + boeId);
		var displayOdcGridUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_OTHER_DIRECT_COST_GRID %>',
			'boe/' + boeId);
		var displayTravelGridUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_TRAVEL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_TRAVEL_GRID %>',
			'boe/' + boeId);
		var displayZoneTravelGridUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_ZONE_TRAVEL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_GRID %>',
			'boe/' + boeId);
		var displayMaterialGridUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_MATERIAL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_MATERIAL_GRID %>',
			'boe/' + boeId);
		var displayMaterialCompositeUrl = CreatePostURL(workspace,
                '<%: WebConstants.CONTROLLER_BOE_MATERIAL %>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_MATERIAL_COMPOSITE %>',
			'boe/' + boeId);
		var isZoneTravel = false;
		<% if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
		{ %>
			isZoneTravel = true;
		<% } %>
		var displayODC = <%: ViewData["displayODCTab"] %>;
		var displayTravel = <%: ViewData["displayTravelTab"] %>;

		AfterDomLoadBOEDetailsWidget(BOEDetails, displayTaskElementDetailsEvent, displayTaskElementCompositeUrl, displayOdcDetailsEvent,
			displayOdcCompositeUrl, displayTravelDetailsEvent, displayTravelCompositeUrl, displayZoneTravelCompositeUrl, displayTaskElementGridUrl,
			displayBoeSummaryUrl, boeDetailsReloadCommentEvent, displayBoeCommentUrl, boeDetailsReloadHistoryEvent, displayBoeHistoryUrl, displayOdcGridUrl, displayTravelGridUrl,
            displayZoneTravelGridUrl, displayMaterialGridUrl, displayMaterialDetailsEvent, displayMaterialCompositeUrl, isZoneTravel, displayODC, displayTravel);
	});
</script>
<div class="boe-details">
	<div id="BOEDetailTabs">
		<div class="tabs">
			<div class="tabs-left"></div>
			<div class="tabs-center">
				<ul>
					<li class="active">
						<div class="tab-left"></div>
						<a href="#LMLabor" tab="LMLabor">LM / IWTA / Sub Labor</a><div class="tab-right"></div>
					</li>
					<% if ((bool)ViewData["displayTravelTab"] != false)
						{ %>
					<li>
						<div class="tab-left"></div>
						<a href="#Travel" tab="Travel">Travel</a><div class="tab-right"></div>
					</li>
					<% } %>
					<li>
						<div class="tab-left"></div>
						<a href="#Material" tab="Material">Material</a><div class="tab-right"></div>
					</li>
					<% if ((bool)ViewData["displayODCTab"] != false)
						{ %>
					<li>
						<div class="tab-left"></div>
						<a href="#ODC" tab="ODC">ODC</a><div class="tab-right"></div>
					</li>
					<% } %>
					<li>
						<div class="tab-left"></div>
						<a href="#Comments" tab="Comments">Comments & Approvals</a><div class="tab-right"></div>
					</li>
					<li>
						<div class="tab-left"></div>
						<a href="#History" tab="History">History</a><div class="tab-right"></div>
					</li>
				</ul>
			</div>
			<div class="tabs-right"></div>
		</div>
		<div tab="LMLabor">
			<div class="task-grid">
			</div>
			<div class="task-details">
			</div>
		</div>
		<div tab="Travel">
			<div class="travel-grid">
			</div>
			<div class="travel-details">
			</div>
		</div>
		<div tab="Material">
			<div class="material-grid">
			</div>
			<div class="material-details">
			</div>
		</div>
		<div tab="ODC">
			<div class="odc-grid">
			</div>
			<div class="odc-details">
			</div>
		</div>
		<div tab="Comments">
		</div>
		<div tab="History">
		</div>
		<div class="module-footer">
			<div class="module-footer-left"></div>
			<div class="module-footer-center"></div>
			<div class="module-footer-right"></div>
		</div>
	</div>
</div>
