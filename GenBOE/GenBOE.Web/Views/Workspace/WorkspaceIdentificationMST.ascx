<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Workspace.WorkspaceIdentificationMSTModelView>" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="IES.Common.PickList" %>
<%: Scripts.Render("~/bundles/identification") %>
<%
	bool disabledBoeTemplateDropdown = Model.UsingTemplateBoe || Model.CreatedPriorToBoeTemplates;
	bool disabledEnableSAPConnectionDropdown = false;
	object dropdownParamsForBoeTemplates = new { onchange = @"WorkspaceIdentificationWidget.BoeTemplateChange()" };
	if (disabledBoeTemplateDropdown)
	{
		dropdownParamsForBoeTemplates = new { onchange = @"WorkspaceIdentificationWidget.BoeTemplateChange()", @class = "disabled", @disabled = "disabled" };
	}
%>

<script type="text/javascript">
	var formConfigs = [];
	var originalSapConnectionEnabled = false;
	var onLoadAssignAuthorsValue = '<%:Model.EnableAssignTaskAuthor%>';
	var workspaceContainsTaskAuthor = '<%:Model.WorkspaceContainsTaskAuthor%>';

	formConfigs.push({
		ElementID: 'WorkspaceIdentificationForm',
		Buttons: [
			{
				ButtonClass: 'ies-action',
				ButtonText: 'Save',
				ButtonName: 'save-button',
				ButtonAction: function (buttonPressed) {
					var dataToSend = JSON.stringify(WorkspaceIdentificationWidget.getForm('WorkspaceIdentificationForm').getData());

					WorkspaceIdentificationWidget.saveRequest({
						url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                            '<%: WebConstants.ACTION_SAVE_WORKSPACE_IDENTIFICATION %>', ''),
						data: dataToSend,
						success: function (result) {
							var message = '';
							if (result.Status == true && result.Message) {
								message = result.Message;
							}

							WorkspaceIdentificationWidget.BackToJumpPage(message);
						}
					}, buttonPressed);
				},
				Stateful: true
			}, {
				ButtonClass: 'ies',
				ButtonText: 'Cancel',
				ButtonName: 'cancel-button',
				ButtonAction: function (buttonPressed) {
					WorkspaceIdentificationWidget.Cancel();
				}
			}
		],
		ContainsOCI: <%: ViewData["ContainsOCI"] %>,
		HideOCI: false,
		// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
		BannerTextWithOCI: '<%: SiteMasterUtilities.GetBannerText() %>',
		BannerTextWithoutOCI: '<%: SiteMasterUtilities.GetBannerText(true) %>'
	});

	var dialogConfigs = [];

	var widgetConfig = {};
	widgetConfig.ContextID = "WorkspaceIdentification";
	widgetConfig.isReadOnly = <%: ViewData["READONLY"] %>;
	widgetConfig.IsModule = true;
	widgetConfig.FormConfigs = formConfigs;
	widgetConfig.DialogConfigs = dialogConfigs;

	var dateShiftUrl = CreatePostURL(
        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        '<%: WebConstants.CONTROLLER_DATESHIFT %>',
        '<%: WebConstants.ACTION_INDEX %>',
        'id/<%: Model.WorkspaceID%>/level/<%: ((int)IES.Common.Level.Workspace).ToString() %>');
	var jumpUrl = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
        '<%: WebConstants.ACTION_WORKSPACE_SETTINGS %>');
	WorkspaceIdentificationWidget = InitializeWorkspaceIdentificationWidget(widgetConfig, jumpUrl);

	WorkspaceIdentificationWidget.LockFields = function () {
		<% if (Utilities.ShowPLDIsIntegrated)
	{ %>
		// break up WorkspaceName and WorkspaceNameInput if needed
		var paNumber = $('#PaNumber').val();
		var paTitle = $('#PaTitle').val();
		var workspaceName = $('#WorkspaceName').val();

		// only show the workspacename broken up if it actually starts with the tracking number
		if (workspaceName.indexOf(paNumber) == 0) {
			if ($('#WorkspaceName').hasClass('full')) {
				// The fields could be locked already, but this needs to be shrunk and the workspace name input shown so the user can update
				$('#WorkspaceName').removeClass('full').addClass('half');
				$('#WorkspaceNameInput').removeClass('display-none');
			}

			// Get length of PA Number and Title, +1 for space between them
			var paNumberTitleLength = paNumber.length + paTitle.length + 1; 

			// Set width based on character length since it's variable unlike space
			$('#WorkspaceName').addClass('disabled').prop('readonly', 'readonly').addClass('labelLookFeelRms');
			$('#WorkspaceName')[0].style.width = paNumberTitleLength + "ch";

			if (workspaceName.length > paNumberTitleLength) {
				$('#WorkspaceNameInput').val(workspaceName.substr(paNumberTitleLength + 1));
				$('#WorkspaceName').val(workspaceName.substr(0, paNumberTitleLength + 1));
			} else {
				$('#WorkspaceName').val(workspaceName + " ");
			}
		}
		<% } %>
	}

	$('#RteSizeLimit').keyup(function () {
		displayApproxPages($(this).val());
	});

	displayApproxPages = function (input) {
		if (!input) {
			// if nothing entered, remove text
			$("#approxPages").text("");
		} else {
			input = Number(input);

			if (!isNaN(input) && Number.isInteger(input) && input >= 0) {
				// 3000 characters per page
				var approxPages = input / 3000;
				var pagesToDisplay = 0;

				if (approxPages < 0.1) {
					// if less than 0.1 pages, display 0 (instead of 0.0)
					pagesToDisplay = 0;
				} else if (approxPages < 0.5) {
					// if less than half a page, round to nearest tenth
					pagesToDisplay = approxPages.toFixed(1);
				} else {
					// round to nearest .25 if more than half a page
					pagesToDisplay = Math.round(approxPages * 4) / 4;
				}

				$("#approxPages").text("Approximately " + pagesToDisplay + " pages");
			} else {
				$("#approxPages").text("Invalid value");
			}
		}
	};

	WorkspaceIdentificationWidget.BoeTemplateChange = function () {
		GenSession.confirmDialog("Template BOE Change",
			"Are you sure you want to change Template BOE to 'Yes'? This cannot be undone.",
			function () { },
			function () {
				WorkspaceIdentificationWidget.cleanDirty();
				window.location.reload(true);
			}
		);
	}

	WorkspaceIdentificationWidget.OnSapConnectionChange = function (selection) {
		// If changing from Yes to No (and the workspace is currently set to Yes), warn the user
		if ($(selection).val() == 'False' && originalSapConnectionEnabled == 'True') {
			GenSession.confirmDialog("Enable SAP Connection Change",
				"Changing the SAP Connection Enabled from Yes to No will automatically set all MOQ Tables in this Workspace as Source = User upon the Save of a Task.  Meaning the MOQ table is User managed and is no longer integrated with SAP.  This update will not automatically change any existing BOE status. Do you wish to proceed?",
				function () {
					// do nothing on confirm, let the change happen
				},
				function () {
					// reset value on cancel
					$('#EnableSAPConnection').val('True');
				}
			);
		}
	};

	$(function () {
		WorkspaceIdentificationWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { WorkspaceIdentificationWidget.cleanDirty('WorkspaceIdentificationForm'); });

		if (!WorkspaceIdentificationWidget.isReadOnly()) {
			$("#ProposalSubmittalDate").datepicker({ dateFormat: 'mm/dd/yy' });
			$('#ResourceDecimalPrecision').on('change', WorkspaceIdentificationWidget.WarnPrecisionChange);
			$('#CostDecimalPrecision').on('change', WorkspaceIdentificationWidget.WarnCostPrecisionChange);
		}

		$('#WorkspaceIdentification').find('select[name=Segment]').on('change', function () {
			// Need to put up warning message
			$("#WorkspaceSegmentWarning").show();
		});

		$('#WorkspaceIdentification').find('input[name=AllowGridEdit]').on('change', function () {
			// Need to put up warning message
			if ($('#WorkspaceIdentification').find('input[name=AllowGridEdit]:checked').val() == "True") {
				$("#ProjectMapEditGridWarning").show();
			} else {
				$("#ProjectMapEditGridWarning").hide();
			}
		});

		$('input[name=ContainsOCI]').click(WorkspaceIdentificationWidget.ContainsOCIRadioClick);

		$('#AdjustDatesLink').on('click', function () {
			window.location = dateShiftUrl;
		});

		$('#Back-WorkspaceIdentification').click(WorkspaceIdentificationWidget.Cancel);

		if (WorkspaceIdentificationWidget.isReadOnly()) {
			$('#IRISLookup').addClass('display-none');
			$('#AdjustDatesLink').addClass('display-none');
			$('#Back-WorkspaceIdentification').removeClass('display-none');
		}

		refreshModule($('.workspace-identification.module'));

		// create user lookup widget (before module creation)
		$('#CostVolumeLeadPricerDisplayName').lookupUser({
			accountNameElementId: 'CostVolumeLeadPricerNTID',
			accountNameInitial: '<%:Model.CostVolumeLeadPricerNTID%>',
			accountDisplayNameInitial: "<%=Model.CostVolumeLeadPricerDisplayName%>",
			enabled: true,
			readOnly: <%:ViewData["READONLY"]%>,
			fieldDisplayName: 'Cost Volume Lead/Pricer',
			allowGroups: false,
			checkNameCallbackHandler: function (valid) { },
			onChange: function () {
				WorkspaceIdentificationWidget.setDirty('WorkspaceIdentificationForm');
			}
		});

		// Need to put up warning message if set to true
		if ($('#WorkspaceIdentification').find('input[name=AllowGridEdit]:checked').val() == "True") {
			$("#ProjectMapEditGridWarning").show();
		} else {
			$("#ProjectMapEditGridWarning").hide();
		}

		if ($('#PaNumber').val() !== '' && !WorkspaceIdentificationWidget.isReadOnly()) {
			WorkspaceIdentificationWidget.LockFields();
		}

		if ('<%: Model.RteSizeLimit.HasValue %>' === "True") {
			displayApproxPages('<%: Model.RteSizeLimit %>');
		}

		if ('<%: Model.EnableTemplateBoeSelect %>' == "True") {
			var dropdown = $('#UsingTemplateBoe');
			dropdown.removeClass('disabled');
			dropdown.removeAttr('disabled');
		}

		originalSapConnectionEnabled = $('#EnableSAPConnection').val();
	});

	// Dynamically set disabled/readonly dropdown for SAP connection
	var usingTemplateBoeInit = '<%:Model.UsingTemplateBoe%>'.isTrue();
	var enableSAPDropdown = $('#EnableSAPConnection');

	if (!usingTemplateBoeInit) {
		enableSAPDropdown.addClass('disabled').attr('disabled', true);
	}

	$('#UsingTemplateBoe').change(function () {
		if ($('#UsingTemplateBoe').val() === 'False') {
			enableSAPDropdown.addClass('disabled').attr('disabled', true);
			enableSAPDropdown.val('False');
			disabledEnableSAPConnectionDropdown = true;
		} else {
			enableSAPDropdown.removeClass('disabled').removeAttr('disabled');
			enableSAPDropdown.val('True');
			disabledEnableSAPConnectionDropdown = false;
		}
	});

	// Show the popup if the load value is set to true, but user changes it to false
	$('#EnableAssignTaskAuthor').change(function () {
		var isAuthorsAssignedSetToTrue = $('#EnableAssignTaskAuthor').val() === 'True';
		if (onLoadAssignAuthorsValue === 'True' && !isAuthorsAssignedSetToTrue && workspaceContainsTaskAuthor === 'True') {
			Session.confirmDialog(
				'Delete Assigned Task Authors',
				'At least one Task has an Author assigned. All assigned Authors for Tasks will be deleted and the BOEs containing those Tasks will have their Status changed to Draft.<br/><br/>Are you sure you want to delete the Assigned Task Authors?',
				function () {
					// This is a confirmation, do nothing
					null
				},
				// Revert back to Yes if canceled
				function () {
					$('#EnableAssignTaskAuthor').val('True')
				}
			);
		}
	});
</script>

<div id="WorkspaceIdentification" class="workspace-identification module ">
	<div class="module-header-data">
		Workspace Identification
	</div>
	<%: Html.Hidden("OriginalDecimalPrecision", Model.ResourceDecimalPrecision)%>
	<%: Html.Hidden("OriginalCostDecimalPrecision", Model.CostDecimalPrecision)%>
	<div class="module-content-data">

		<% using (Html.BeginForm("", "", FormMethod.Post, new { name = "WorkspaceIdentificationForm", id = "WorkspaceIdentificationForm" }))
			{ %>
		<ul class="validation-box"></ul>
		<div class="form-row">
			<%: Html.Hidden("UpdateDateLong", Model.UpdateDateLong)%>
			<%: Html.Hidden("WorkspaceID", Model.WorkspaceID)%>
			<div class="form-label">
				<span helptext="Proposal or group name. Name is used in BOEs, reports and to identify your workspace.">Workspace/Proposal Name *</span>
			</div>
			<div class="form-element">
				<%: Html.TextBox("WorkspaceName", Model.WorkspaceName, new { @class = "full", @maxlength="100" })%>
				<input id="WorkspaceNameInput" name="WorkspaceNameInput" class="half display-none" type="text" maxlength="40" />
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Description
			</div>
			<div class="form-element">
				<%: Html.TextArea("Description", Model.Description, new { @class = "full", onkeyup="Helper.textAreaLimit(this, 1000)" })%>
			</div>
		</div>
		<% if (Utilities.ShowPLDIsIntegrated)
			{ %>
		<%-- TODO - PA Number field will go here in PROPH-2986 --%>
		<%: Html.Hidden("PaNumber", Model.TrackingNumber) %>
		<%: Html.Hidden("PaTitle", Model.ProposalTitle) %>
		<% } %>
		<%if (SiteMasterUtilities.IsProjectMapEnabled)
			{%>

		<div class="form-row">
			<div class="form-label">Workspace Type</div>
			<div class="form-element">
				<%: Model.ProjectMapType.ToDescription() %>
			</div>
		</div>
		<% } %>
		<div class="form-row">
			<div class="form-label">
				Line of Business *
			</div>
			<div class="form-element">
				<select id="LineOfBusinessTypeID" name="LineOfBusinessTypeID">
					<option value="-1">Select Line of Business</option>
					<% foreach (PickListDto lob in (ICollection<PickListDto>)ViewData["LineOfBusinessTypes"])
						{
							if (lob.Id == Model.LineOfBusinessTypeID)
							{ %>
					<option value="<%=lob.Id%>" selected="selected"><%=lob.Text %></option>
					<%      }
						else if (lob.IsActive)
						{%>
					<option value="<%=lob.Id%>"><%=lob.Text %></option>
					<%      }
						} %>
				</select>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The individual who will perform the initial setup procedures before the BOE Authors
                        begin the writing process."><%: Model.LabelLeadPricer %> *</span>
			</div>
			<div class="form-element">
				<input id="CostVolumeLeadPricerDisplayName" name="CostVolumeLeadPricerDisplayName" type="text" class="half" maxlength="50" />
			</div>
		</div>
		<%if (Model.IsProjectMapWorkspace)
			{%>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Allows Project Map data to be edited within the web interface grid.">Allow Grid Edit *</span>
			</div>
			<div class="form-element radio">
				<%: Html.RadioButton("AllowGridEdit", true, Model.AllowGridEdit, new { id="AllowGridEdit-Yes" })%>
				<label for="AllowGridEdit-Yes">Yes</label>
				<%: Html.RadioButton("AllowGridEdit", false, !Model.AllowGridEdit, new { id="AllowGridEdit-No" })%>
				<label for="AllowGridEdit-No">No</label>
			</div>
		</div>
		<div id="ProjectMapEditGridWarning" class="warning-box">
			<div class="warning-message">
				When set to Yes loading the grid may be a little slower for large workspaces due to editing capabilities being turned on.
			</div>
			<div class="small-close-button" onclick="$('#ProjectMapEditGridWarning').hide()"></div>
		</div>
		<%}
			else
			{%>
		<div class="form-row">
			<div class="form-label">Contract Start Date *</div>
			<div class="form-element">
				<input type="text" disabled="disabled" value="<%: Model.ContractStartDate %>" class="small-date" name="StartDate" />
			</div>
			<div class="form-label contract-end-date">Contract End Date *</div>
			<div class="form-element">
				<input type="text" disabled="disabled" value="<%: Model.ContractEndDate %>" class="small-date" name="EndDate" />
				&nbsp;&nbsp;<a id="AdjustDatesLink" style="float: right;">Adjust Dates</a>

			</div>
		</div>
		<%}%>
		<div class="form-row">
			<div class="form-label">
				Proposal Submittal Date
			</div>
			<div class="form-element">
				<%: Html.TextBox("ProposalSubmittalDate", Model.ProposalSubmittalDate, new { @class = "normal-date" })%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The number of decimal places to use for resource hours in the workspace. (0-6)">Resource Decimal Precision</span>
			</div>
			<div class="form-element">
				<%: Html.TextBox("ResourceDecimalPrecision", Model.ResourceDecimalPrecision )%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The number of decimal places to use for resource costs in the workspace. (0 or 2)">Cost Decimal Precision</span>
			</div>
			<div class="form-element" id="CostPrecisionSelect">
				<%: Html.DropDownListFor(c => c.CostDecimalPrecision, Model.CostPrecisionSelect)%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				RFP#
			</div>
			<div class="form-element">
				<%: Html.TextBox("RFPNumber", Model.RFPNumber, new { @class = "full", @maxlength="100" })%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Information subject to contractual organization conflict of interest (OCI) limitations.
                        Access must be restricted to authorized employees who have executed non-disclosure
                        agreements. The information is limited for use solely in the performance of the
                        contract on which it was provided or created.">Contains OCI Information *</span>
			</div>
			<div class="form-element radio">
				<%: Html.RadioButton("ContainsOCI", true, Model.ContainsOCI, new { id="ContainsOCI-Yes" })%>
				<label for="ContainsOCI-Yes">Yes</label>
				<%: Html.RadioButton("ContainsOCI", false, !Model.ContainsOCI, new { id="ContainsOCI-No" })%>
				<label for="ContainsOCI-No">No</label>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span>Proposal Status *</span>
			</div>
			<div class="form-element radio">
				<%: Html.RadioButton("ProposalStatus", ProposalStatusType.Won, Model.ProposalStatus == ProposalStatusType.Won, new { id="ProposalStatus-Won" })%>
				<label for="ProposalStatus-Won">Won</label>
				<%: Html.RadioButton("ProposalStatus", ProposalStatusType.Lost, Model.ProposalStatus == ProposalStatusType.Lost, new { id="ProposalStatus-Lost" })%>
				<label for="ProposalStatus-Lost">Lost</label>
				<%: Html.RadioButton("ProposalStatus", ProposalStatusType.None, Model.ProposalStatus != ProposalStatusType.Won && Model.ProposalStatus != ProposalStatusType.Lost, new { id="ProposalStatus-NA" })%>
				<label for="ProposalStatus-NA">NA</label>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Status Comments
			</div>
			<div class="form-element">
				<%: Html.TextArea("StatusComments", Model.StatusComments, new { @class = "full", onkeyup="Helper.textAreaLimit(this, 1000)"})%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Custom Field Sorting
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.CustomFieldSorting, Model.CustomFieldSortingSelect)%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Resource Sorting
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.ResourceSorting, Model.CustomFieldSortingSelect)%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Performing Org Sorting
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.PerfOrgSorting, Model.CustomFieldSortingSelect)%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The maximum number of characters per dialog box using Rich Text, excluding any HTML markup. One single-spaced page is approximately 3000 characters. This limit does not account for images and tables that an author may include which will increase the page count.">Rich Text Editor Character
					<br />
					Limit</span>
			</div>
			<div class="form-element">
				<%: Html.TextBox("RteSizeLimit", Model.RteSizeLimit) %>
				<span id="approxPages"></span>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Does this Workspace use the MOQ Template in its BOEs?">MOQ Template BOEs</span>
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.UsingTemplateBoe, new List<SelectListItem>()
                    {
                        new SelectListItem() { Text = "Yes", Value = "True" },
                        new SelectListItem() { Text = "No", Value = "False" }
                    }, dropdownParamsForBoeTemplates) %>
				<%if (disabledBoeTemplateDropdown)
					{ %>
				<%: Html.HiddenFor(c => c.UsingTemplateBoe) %>
				<%} %>
			</div>
		</div>
		<%if ((bool)ViewData["ShowSAP"])
			{  %>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Does this Workspace use the SAP in its BOEs?">SAP Connection Enabled</span>
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.EnableSAPConnection, new List<SelectListItem>()
                        {
                            new SelectListItem() { Text = "Yes", Value = "True" },
                            new SelectListItem() { Text = "No", Value = "False" }
                        }, new { onchange="WorkspaceIdentificationWidget.OnSapConnectionChange(this)" }) %>
			</div>
			<%if (disabledEnableSAPConnectionDropdown)
				{%>
			<%: Html.HiddenFor(c => c.EnableSAPConnection) %>
			<%} %>
		</div>
		<% } %>
		<% if ((bool)Utilities.IsAssignTaskAuthorEnabledForSystem)
			{ %>
		<div class="form-row">
			<div class="form-label">
				<span helptext="When &quot;Yes&quot; is selected, Authors are prompted to choose their name from the BOE Author list and assign themselves to the task as the Task Author. 
						When &quot;No&quot; is selected, Author Assignment is BOE Level only. No additional Author steps.">Authors Assignable at Task
					<br />
					Level *</span>
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.EnableAssignTaskAuthor, new List<SelectListItem>()
					{
						new SelectListItem() { Text = "Yes", Value = "True" },
						new SelectListItem() { Text = "No", Value = "False" }
					}) %>
			</div>
		</div>
		<% } %>
		<% if ((bool)Utilities.IsLmNavigatorRteLinkEnabledForSystem)
			{ %>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Display a link to LM Navigator in all Rich Text Editors?"><%--TODO - update if able to pass in text--%>
				LM Navigator Enabled
				</span>
			</div>
			<div class="form-element">
				<%: Html.DropDownListFor(c => c.EnableLmNavigator, new List<SelectListItem>()
				{
					new SelectListItem() { Text = "Yes", Value = "True" },
					new SelectListItem() { Text = "No", Value = "False" }
				}) %>
			</div>
		</div>
		<% } %>
		<button id="Back-WorkspaceIdentification" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
		<% } %>
	</div>
</div>
