<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Workspace.WorkspaceIdentificationSpaceModelView>" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="IES.Common.PickList" %>
<%: Scripts.Render("~/bundles/identification") %>
<% 
	bool disabledBoeTemplateDropdown = Model.UsingTemplateBoe || Model.CreatedPriorToBoeTemplates;
	object dropdownParamsForBoeTemplates = new { onchange = @"WorkspaceIdentificationWidget.BoeTemplateChange()" };
	if (disabledBoeTemplateDropdown)
	{
		dropdownParamsForBoeTemplates = new { onchange = @"WorkspaceIdentificationWidget.BoeTemplateChange()", @class = "disabled", @disabled = "disabled" };
	}
%>

<script type="text/javascript">

	var formConfigs = [];
	var originalTrackingNumber = "";
	var previousTrackingNumber = "";
	var originalSapConnectionEnabled = false;

	formConfigs.push({
		ElementID: 'WorkspaceIdentificationForm',
		Buttons: [
			{
				ButtonClass: 'ies-action',
				ButtonText: 'Save',
				ButtonName: 'save-button',
				ButtonAction: function (buttonPressed) {
					if ($('#proposalSelect').val() != '' && $('#TrackingNumber').val() == '') {
						$('#TrackingNumber').val('INVALID');
					}

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

							var newJumpUrl = undefined;
							if ($('#ShortName').val() != '') {
								newJumpUrl = CreatePostURL(
									$('#ShortName').val(),
                                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                                '<%: WebConstants.ACTION_WORKSPACE_SETTINGS %>');
							}

							WorkspaceIdentificationWidget.cleanDirty('WorkspaceIdentificationForm');
							WorkspaceIdentificationWidget.BackToJumpPage(message, newJumpUrl);
						}
					}, buttonPressed);
				},
				Stateful: true
			}
            <% if (Utilities.IsPTMIntegrated)
	{ %>
			, {
				ButtonClass: 'ies-action',
				ButtonText: 'Refresh PTM Data',
				ButtonName: 'refresh-ptm-button',
				ButtonAction: function (buttonPressed) {
					WorkspaceIdentificationWidget.RefreshTrackingNumber(true);
				},
			}
            <% } %>
			, {
				ButtonClass: 'ies',
				ButtonText: 'Cancel',
				ButtonName: 'cancel-button',
				ButtonAction: function (buttonPressed) {
					WorkspaceIdentificationWidget.Cancel();
				}
			}
		],
		ContainsOCI: <%: ViewData["ContainsOCI"] %>
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
        <% if (Utilities.IsPTMIntegrated)
	{ %>

		// break up workspacename and revisionworkspacename if needed
		var trackingNumber = $('#TrackingNumber').val();
		var workspaceName = $('#WorkspaceName').val();

		// only show the workspacename broken up if it actually starts with the tracking number
		if (workspaceName.indexOf(trackingNumber) == 0) {
			if ($('#WorkspaceName').hasClass('full')) {
				// The fields could be locked already, but this needs to be shrunk and the revision workspace name shown so the user can update
				$('#WorkspaceName').removeClass('full').addClass('half');
				$('#RevisionWorkspaceName').removeClass('display-none');
			}
			$('#WorkspaceName').addClass('disabled').prop('readonly', 'readonly').addClass('labelLookFeel');

			// if there is a space, break it up by the space
			var spaceIndex = workspaceName.indexOf(' ');
			if (spaceIndex > -1) {
				$('#RevisionWorkspaceName').val(workspaceName.substr(spaceIndex + 1));
				$('#WorkspaceName').val(workspaceName.substr(0, spaceIndex + 1));
			} else {
				$('#WorkspaceName').val(workspaceName + " ");
			}
		}

		$('#RFPNumber').addClass('disabled').prop('readonly', 'readonly');
		$("#ProposalSubmittalDate").datepicker('destroy');
		$('#ProposalSubmittalDate').addClass('disabled').prop('readonly', 'readonly');
		// Proposal title is always readonly
		$('#ProposalClass').prop('disabled', 'disabled');
		// create a hidden input field for post data
		$('<input>').attr({
			type: 'hidden',
			id: 'ProposalClass',
			name: 'ProposalClassType',
			value: $('#ProposalClass').val()
		}).appendTo('form');
		$('#LineOfBusinessTypeID').prop('disabled', 'disabled');
		// create a hidden input field for post data
		$('<input>').attr({
			type: 'hidden',
			id: 'LineOfBusinessTypeID',
			name: 'LineOfBusinessTypeID',
			value: $('#LineOfBusinessTypeID').val()
		}).appendTo('form');
		$('.approver-selection-box').addClass('display-none');
		var selections = $('#contract-selections');
		selections.removeClass('display-none');
		var contracts = $('input[name=SelectedContractTypes]');
		var values = [];
		for (var i = 0; i < contracts.length; i++) {
			var checkbox = $(contracts[i]);
			if (checkbox[0].checked) {
				values.push(checkbox.attr('data-label'));
			}
		}
		WorkspaceIdentificationWidget.refreshModule();
		selections.text(values.join(','));

        <% } %>
	}

	WorkspaceIdentificationWidget.RefreshTrackingNumber = function (refreshOnly) {
		// kickoff retrieval of PTM Tracking information
		var postData = { trackingNumber: $('#TrackingNumber').val() };

		$(document).trigger("SHOW_LOADING_BOX");
		WorkspaceIdentificationWidget.ajaxRequest({
			type: 'POST',
			url: GenSession.CreateUrl({
				controller: '<%: WebConstants.CONTROLLER_WORKSPACE %>',
				action: '<%: WebConstants.ACTION_GET_NEXT_TRACKING_NUMBER_REVISION %>',
				workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>'
			}),
			performValidation: true,
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			data: JSON.stringify(postData),
			success: function (result) {
				$('input[name=SelectedContractTypes]').prop('checked', false);

				if (!refreshOnly) {
					$('#WorkspaceName').val(result.TrackingNumberRevision + " ");
					$('#ShortName').val(result.TrackingNumberRevision);
				}
				$('#RFPNumber').val(result.RFPNumber);
				$('#ProposalTitle').val(result.Title);
				$('#ProposalSubmittalDate').val(result.AnticipatedDeliveryDate);
				$('input[name=ProposalClassType], select[name=ProposalClassType]').val(result.ProposalClassId);
				$('input[name=LineOfBusinessTypeID], select[name=LineOfBusinessTypeID]').val(result.LOBId);
				$('#RevisedSubmittalDate').val(result.RevisedSubmittalDate);

				var selectedContractTypeIds = result.ContractTypes;
				for (var contractTypeIndex = 0; contractTypeIndex < selectedContractTypeIds.length; contractTypeIndex++) {
					$('input[id=ContractType_' + selectedContractTypeIds[contractTypeIndex] + ']').prop('checked', true);
				}

				var selections = $('#contract-selections');
				var contracts = $('input[name=SelectedContractTypes]');
				var values = [];
				for (var i = 0; i < contracts.length; i++) {
					var checkbox = $(contracts[i]);
					if (checkbox[0].checked) {
						values.push(checkbox.attr('data-label'));
					}
				}

				selections.text(values.join(','));

				WorkspaceIdentificationWidget.setDirty('WorkspaceIdentificationForm');
				$(document).trigger("HIDE_LOADING_BOX");
			},
			error: function () {
				$(document).trigger("HIDE_LOADING_BOX");
			}
		});
	}

	WorkspaceIdentificationWidget.OnTrackingNumberChange = function () {
		var options = $('#' + $('#proposalSelect').attr('list') + ' option');
		var selectedVal = $('#proposalSelect').val();

		// Only continue if new value is selected
		if (originalTrackingNumber === '' || !selectedVal.startsWith(originalTrackingNumber)) {
			$('#TrackingNumber').val(''); //clear previous value
			previousTrackingNumber = selectedVal;

			for (var i = 0; i < options.length; i++) {
				var option = options.eq(i);
				if (option[0].value === selectedVal || option.attr('propId') === selectedVal) {
					GenSession.confirmDialog("PTM Tracking Number Change",
						"Changing the PTM Tracking Number will change the Workspace Name, the URL, and additional data when saving the page.  Are you sure?  No will refresh the page.",
						function () {
							$('#TrackingNumber').val(option.attr('propId'));
							// lock fields if necessary
							if ($('#WorkspaceName').hasClass('full')) {
								// The fields could be locked already, but this needs to be shrunk and the revision workspace name shown so the user can update
								$('#WorkspaceName').removeClass('full').addClass('half');
								$('#RevisionWorkspaceName').removeClass('display-none');
							}
							$('#WorkspaceName').addClass('disabled').prop('readonly', 'readonly');

							if (!$('#RFPNumber').hasClass('disabled')) {
								WorkspaceIdentificationWidget.LockFields();
							}

							WorkspaceIdentificationWidget.RefreshTrackingNumber(false);
						},
						// cancel
						function () {
							WorkspaceIdentificationWidget.cleanDirty();
							window.location.reload(true);
						}
					);
					break;
				}
			}
		} else if (previousTrackingNumber != "" && !originalTrackingNumber.startsWith(previousTrackingNumber)) {
			GenSession.confirmDialog("PTM Tracking Number Change",
				"Changing the PTM Tracking Number back to its current value will refresh the page.  Are you sure?  No will cancel this selection.",
				function () {
					WorkspaceIdentificationWidget.cleanDirty();
					window.location.reload(true);
				},
				function () {
					$('#proposalSelect').val(previousTrackingNumber);
				}
			);
		} else {
			// Keep the select value the same as it was without the proposal name being added
			$('#proposalSelect').val(originalTrackingNumber);
		}
	}

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

	WorkspaceIdentificationWidget.OnSapConnectionChange = function (selection) {
		// If changing from Yes to No (and the workspace is currently set to Yes), warn the user
		if ($(selection).val() == 'False' && originalSapConnectionEnabled == 'True') {
			GenSession.confirmDialog("Enable SAP Connection Change",
				"Changing the SAP Connection Enabled from Yes to No will automatically set all MOQ Tables in this Workspace as Repository Name = Other upon the save of the table.  Meaning the MOQ table is User managed and is no longer integrated with SAP.   This will not change any existing BOE status. Do you wish to proceed?",
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

		$('input[name=ContainsOCI]').click(WorkspaceIdentificationWidget.ContainsOCIRadioClick);

		$('#AdjustDatesLink').on('click', function () {
			window.location = dateShiftUrl;
		});

		$('#Back-WorkspaceIdentification').click(WorkspaceIdentificationWidget.Cancel);

		if (WorkspaceIdentificationWidget.isReadOnly()) {
			$('#IRISLookup').addClass('display-none');
			$('#AdjustDatesLink').addClass('display-none');
			$('#Back-WorkspaceIdentification').removeClass('display-none');
			$('.form-element input').each(function () {
				if ($(this).attr("type") == "checkbox") {
					$(this).prop("disabled", true);
					$(this).removeClass("display-none");
				}
			});
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

	   var selectedContractTypeIds = $('input[name=SelectedContractTypeIds]').val().split(',');
	   for (var contractTypeIndex = 0; contractTypeIndex < selectedContractTypeIds.length; contractTypeIndex++) {
		   $('input[id=ContractType_' + selectedContractTypeIds[contractTypeIndex] + ']').prop('checked', true);
	   }

	   // The second check is necessary to make the page work well in read-only mode. It shows a textbox, which would be an issue.
	   if ($('#TrackingNumber').val() !== '' && !WorkspaceIdentificationWidget.isReadOnly()) {
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

	   originalTrackingNumber = $('#TrackingNumber').val();
	   originalSapConnectionEnabled = $('#EnableSAPConnection').val();
   });

    // Dynamically set disabled/readonly dropdown for SAP connection
    if ('<%: Model.UsingTemplateBoe %>' == "False") {
        var dropdown = $('#EnableSAPConnection');
        dropdown.addClass('disabled');
        dropdown.attr('disabled', true);
    }
    $('#UsingTemplateBoe').change(function () {
        if ($('#UsingTemplateBoe').val() == "False") {
            console.log("false/no")
            var dropdown = $('#EnableSAPConnection');
            dropdown.addClass('disabled');
            dropdown.attr('disabled', true);
            dropdown.val('False');
        } else {
            console.log("true/yes")
            var dropdown = $('#EnableSAPConnection');
            dropdown.removeClass('disabled');
            dropdown.removeAttr('disabled');
            dropdown.val('True');
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
				<input id="RevisionWorkspaceName" name="RevisionWorkspaceName" class="half display-none" type="text" maxlength="88" />
				<%: Html.Hidden("ShortName", Model.ShortName) %>
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
				Proposal Class *
			</div>
			<div class="form-element">
				<select id="ProposalClass" name="ProposalClassType">
					<option value="-1">Select Proposal Class</option>
					<% foreach (PickListDto proposalClass in (ICollection<PickListDto>)ViewData["ProposalClassTypes"])
						{
							if (proposalClass.Id == (int)Model.ProposalClassType)
							{ %>
					<option value="<%=proposalClass.Id%>" selected="selected"><%=proposalClass.Text %></option>
					<%      }
						else if (proposalClass.IsActive)
						{%>
					<option value="<%=proposalClass.Id%>"><%=proposalClass.Text %></option>
					<%      }
						} %>
				</select>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Contract Type *<span helptext="Select all applicable contract types for this workspace. ONLY those contract types selected here will be available for selection and assignment by CLIN on the Manage CLINs -> Edit CLIN Element screen."></span>
			</div>
			<div class="form-element">
				<div class="approver-selection-box">
					<% foreach (PickListDto contractType in (IEnumerable<PickListDto>)ViewData["ContractTypes"])
						{ %>
					<div>
						<input type="checkbox" value="<%: contractType.Id %>" name="SelectedContractTypes" data-label="<%: contractType.Text%>" id="ContractType_<%: contractType.Id %>" />
						<label for="ContractType_<%: contractType.Id %>"><%: contractType.Text%></label>
					</div>
					<% } %>
					<input type="hidden" name="SelectedContractTypeIds" value="<%= ViewData["SelectedContractTypes"] %>" />
				</div>
				<div id="contract-selections" class="display-none"></div>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The individual who will perform the initial setup procedures before the BOE Authors
                        begin the writing process.">Estimating Lead/Pricer *</span>
			</div>
			<div class="form-element">
				<input id="CostVolumeLeadPricerDisplayName" name="CostVolumeLeadPricerDisplayName" type="text" class="half" maxlength="50" />
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">Contract Start Date *</div>
			<div class="form-element">
				<input type="text" disabled="disabled" value="<%: Model.ContractStartDate %>" class="small-date" name="StartDate" id="StartDate" />
			</div>
			<div class="form-label contract-end-date">Contract End Date *</div>
			<div class="form-element">
				<input type="text" disabled="disabled" value="<%: Model.ContractEndDate %>" class="small-date" name="EndDate" id="EndDate" />&nbsp;&nbsp;<a id="AdjustDatesLink" style="float: right;">Adjust Dates</a>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Anticipated Delivery Date
			</div>
			<div class="form-element">
				<%: Html.TextBox("ProposalSubmittalDate", Model.ProposalSubmittalDate, new { @class = "normal-date" })%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				Revised Anticipated Delivery Date
			</div>
			<div class="form-element">
				<%: Html.TextBox("RevisedSubmittalDate", Model.RevisedSubmittalDate, new { @class = "normal-date disabled", @readonly="readonly" })%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The number of decimal places to use for resource <%: ViewData["HoursLabel"]%> in the workspace. (0-6)">Resource Decimal Precision</span>
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
				PTM Tracking #
                <div class="help-icon" onclick="WorkspaceIdentificationWidget.ToggleHelp(this);"></div>
				<!-- This comment is needed for the jquery animation to work in IE8... -->
				<div class="help-dialog" style="width: 130px;">
					<div class="help-dialog-close"></div>
					<div class="help-dialog-text">If you are unable to locate a newly created PTM record, please refresh this page.</div>
				</div>
			</div>
			<div class="form-element">
				<% if (Utilities.IsPTMIntegrated)
					{ %>
				<input id="proposalSelect" list="proposalList" type="text" maxlength="100" autocomplete="on" onchange="WorkspaceIdentificationWidget.OnTrackingNumberChange()" value="<%: Model.TrackingNumber %>">
				<datalist id="proposalList">
					<%foreach (SelectListItem item in (IEnumerable<SelectListItem>)ViewData["TrackingNumbers"])
						{ %>
					<option propid="<%: item.Value %>" value="<%: item.Text %>"></option>
					<%  } %>
				</datalist>
				<%: Html.Hidden("TrackingNumber", Model.TrackingNumber)%>
				<%  }
					else
					{ %>
				<%: Html.TextBox("TrackingNumber", Model.TrackingNumber, new { @class = "full", @maxlength="100" })%>
				<%  } %>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="Insert the RFP number.">RFP #</span>
			</div>
			<div class="form-element">
				<%: Html.TextBox("RFPNumber", Model.RFPNumber, new { @class = "half" })%>
			</div>
		</div>
		<div class="form-row">
			<div class="form-label">
				<span helptext="The linked Proposal Title.">PTM Proposal Title</span>
			</div>
			<div class="form-element">
				<%: Html.TextBox("ProposalTitle", Model.ProposalTitle, new { @class="disabled full", @readonly="readonly" })%>
			</div>
		</div>
		<% if (GenBOE.Objects.FullObjectHelper.ShowEquivalentPersonsOption)
			{ %>
		<div class="form-row">
			<div class="form-label">
				<span helptext="'No' means the proposal was created with the intent of using 
                    Hours as Labor Spread values. 'Yes' means the proposal was created with 
                    the intent of using Equivalent Person (EP) as Labor Spread values.">Is using Equivalent Person (EP)</span>
			</div>
			<div class="form-element static"><%: Model.IsUsingEquivalentPerson ? "Yes" : "No" %></div>
		</div>
		<% } %>
		<div class="form-row">
			<div class="form-label">
				<span helptext="'Yes' if this proposal was created with the intent of using T&M Subcontractor/IWTA Labor Rates to develop the PBOEs/IBOEs; 'No' otherwise.">Is using T&M Subcontractor/IWTA<br />
					Labor Rates</span>
			</div>
			<div class="form-element static"><%: Model.IsUsingTM ? "Yes" : "No" %></div>
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
		<%if (Utilities.IsSAPEnabled) { %>
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
				<%: Html.HiddenFor(c => c.EnableSAPConnection) %>
			</div>
		<% } %>
		<button id="Back-WorkspaceIdentification" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
		<% } %>
	</div>
</div>
