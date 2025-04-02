<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.WorkspaceVariableModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>

<% int WorkspaceID = Convert.ToInt32(ViewData["WorkspaceID"]);
   JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
   bool readOnly = bool.Parse(ViewData["READONLY"] as string); %>

<script type="text/javascript">
    var DiscreteWorkspaceVariableGridWidget = new GridWidget("DiscreteWorkspaceVariables", "WorkspaceVariableID", '<%: readOnly %>'.isTrue());
    
    DiscreteWorkspaceVariableGridWidget.resetElement = function (input) {
        $(input).val($(input).attr("originalvalue"));
    };

    DiscreteWorkspaceVariableGridWidget.registerForEvent('discrete-workspace-variables_NEW_ROW_CREATED', function (e) {
        DiscreteWorkspaceVariableGridWidget.newItemCount--;
        $('#WorkspaceVariableGrid .blank').attr("pkid", DiscreteWorkspaceVariableGridWidget.newItemCount);
        $('#WorkspaceVariableGrid tr:not(.blank) td div.delete').removeClass('display-none');
        
        var valueTypeSelect = $('#WorkspaceVariableGrid .blank select[name*=WorkspaceVariableValueType]');

        if (valueTypeSelect.length) {
            valueTypeSelect.change(function () {
                DiscreteWorkspaceVariableGridWidget.ValueTypeChanged($(this));
                DiscreteWorkspaceVariableGridWidget.updateData(this);
            });
        }

        refreshModule($('.discrete-workspace-variables.module'));
    });

    DiscreteWorkspaceVariableGridWidget.variableValueChanged = function (input) {
       
        $(input).parents('td').focusout();
        $(input).val(Helper.addCommas(Helper.removeCommas($(input).val())));

        var parentRow = $(input).parents('tr');
        var variableInUse = parentRow.find('input[name=InUse]').val().isTrue();

        if (variableInUse) {

            var body = parentRow.find("input[name=WorkspaceVariableName]").val() +
                        " is used in at least one BOE. Are you sure you want to change the value? Click \"YES\" to keep the new value." +
                        " Once saved the Authors will be notified of the change. Click \"No\" to keep the old value.";
            Session.confirmDialog("Change Workspace Variable", body, function () { DiscreteWorkspaceVariableGridWidget.updateData($(input)); }, function () { DiscreteWorkspaceVariableGridWidget.resetElement(input); });
        } else {
            DiscreteWorkspaceVariableGridWidget.updateData($(input));
            $(input).parents('td').focusin();           
        }
    };

    DiscreteWorkspaceVariableGridWidget.specialUpdateInstructions = function (input, jsonIndex) {
        var ElementName = $(input).attr('name');
        var ElementValue = $(input).val();

        if (ElementName == 'WorkspaceVariableValue') {
            DiscreteWorkspaceVariableGridWidget.data[jsonIndex].IsPercentage = (ElementValue.match(/%/) != null);
        }
    };

    DiscreteWorkspaceVariableGridWidget.variableDeleted = function () {
        var parentRow = $(this).parents('tr');
        var variableInUse = parentRow.find('input[name=InUse]').val().isTrue();
        
        if (variableInUse) {
            var body = parentRow.find("input[name=WorkspaceVariableName]").val() +
                        " is used in at least one BOE and cannot be deleted.";
            Session.alertDialog("Cannot Delete Variable", body);
        } else {
            var toDelete = $(this);
            var isExistingVariable = parseInt(parentRow.attr('pkid')) > 0;
            var confirmationMsg = "Are you sure you want to delete this Workspace Variable?";
            if (DiscreteWorkspaceVariableGridWidget.isDirty() && isExistingVariable) {
                confirmationMsg += "<br/><br/>Note: Any changes to Variable Names or Values will also be saved.";
            }
            Session.confirmDialog("Delete Workspace Variable", confirmationMsg, function () {
                // Set the value to 0, to pass validation
                parentRow.find('input[name=WorkspaceVariableValue]').val('0');
                
                DiscreteWorkspaceVariableGridWidget.deleteRecord(toDelete);
                if (isExistingVariable) {
                    $('#Save-DiscreteWorkspaceVariables').removeClass('disabled');
                    $('#Save-DiscreteWorkspaceVariables').click();
                }                
            });
        }
    };
    
    DiscreteWorkspaceVariableGridWidget.ValueTypeChanged = function (valueTypeElement) {
        var selectedValue = valueTypeElement.val();
        var parentRow = valueTypeElement.parents('tr');

        if (selectedValue == '<%: (int)VarValueType.Discrete %>') {
            valueTypeElement.removeClass('default-text');
            parentRow.find('div[name=DiscreteValueField]').removeClass('display-none').siblings().addClass('display-none');
        }

        else {
            valueTypeElement.addClass('default-text');
            parentRow.find('td.variable-value').children().addClass('display-none');
        }
    };

    DiscreteWorkspaceVariableGridWidget.GetVariableRow = function(pkid) {
        return $('#WorkspaceVariableGrid tbody tr[pkid=' + pkid + ']');
    }
    
    DiscreteWorkspaceVariableGridWidget.BackToJumpPage = function() {
        DiscreteWorkspaceVariableGridWidget.cleanDirty('DiscreteWorkspaceVariablesForm');
        window.location.hash = '#';
    };

    $(function () {
        DiscreteWorkspaceVariableGridWidget.afterDOMLoad();

        DiscreteWorkspaceVariableGridWidget.newItemCount = 0;

        createModule($('#DiscreteWorkspaceVariables'));
        AddableGrid('discrete-workspace-variables');

        DiscreteWorkspaceVariableGridWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () {
            DiscreteWorkspaceVariableGridWidget.cleanDirty('DiscreteWorkspaceVariablesForm');
        });

        $('select[name*=WorkspaceVariableValueType]').each(function () {
            $(this).change(function () {
                DiscreteWorkspaceVariableGridWidget.ValueTypeChanged($(this));
                DiscreteWorkspaceVariableGridWidget.updateData(this);
            });

            DiscreteWorkspaceVariableGridWidget.ValueTypeChanged($(this));
        });

        DiscreteWorkspaceVariableGridWidget.registerForDelegateEvent('focusin', 'td :input', function () {
            if (!$(this).parent().children('div').hasClass('delete')) {
                $(this).parent().addClass('selected');
            }
        }, '#DiscreteWorkspaceVariables');

        DiscreteWorkspaceVariableGridWidget.registerForDelegateEvent('focusout', '.discrete-workspace-variables td :input', function () {
            $(this).parent().removeClass('selected');
        }, '#DiscreteWorkspaceVariables');

        $('#Cancel-DiscreteWorkspaceVariables, #Back-DiscreteWorkspaceVariables').click(function () {
            if (DiscreteWorkspaceVariableGridWidget.isDirty()) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function () {
                        DiscreteWorkspaceVariableGridWidget.BackToJumpPage();
                    },
                    null);
            }
            else {
                DiscreteWorkspaceVariableGridWidget.BackToJumpPage();
            }
        });

        $("#WorkspaceVariableGridHeader .delete").click(function () {
            Session.confirmDialog("Delete Workspace Variables", "Are you sure you want to delete all Discrete Workspace Variables not in use?", function () {

                var dataToSend = JSON.stringify({valueType: '<%:VarValueType.Discrete%>'});
                    
                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DELETE_ALL_WORKSPACE_VARIABLES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) {
                        $(document).trigger("WS_LOAD_DISCRETE_VARIABLES");
                    }
                });
            });
        });

        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('click', '#WorkspaceVariableGrid tr:not(.blank) td div.delete', DiscreteWorkspaceVariableGridWidget.variableDeleted);


        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('click', '#WorkspaceVariableGrid tbody td.variable-name, #WorkspaceVariableGrid tbody td.variable-value', function () {
            $(this).find('.default-text').addClass('display-none');
            $(this).find('input').removeClass('display-none').focus();
        });

        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('focusin', '#WorkspaceVariableGrid tbody td.variable-name :input, #WorkspaceVariableGrid tbody td.variable-value :input', function () {
            $(this).parent().find('.default-text').addClass('display-none');
            $(this).parent().find('input').removeClass('display-none');
        });

        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('focusout', '#WorkspaceVariableGrid tbody td.variable-name :input, #WorkspaceVariableGrid tbody td.variable-value :input', function () {
            if (!$.trim($(this).parent().find('input').val()).length) {
                $(this).parent().find('.default-text').removeClass('display-none');
            }
        });

        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('change', '#WorkspaceVariableGrid tbody td.variable-name :input', function () {
            $(this).parents('td').focusout(); 
            var valueType = $(this).parents('tr').find('select[name^=WorkspaceVariableValueType]');
            valueType.val('<%:(int)VarValueType.Discrete%>');
            valueType.change();
            DiscreteWorkspaceVariableGridWidget.updateData(this);
        });

        DiscreteWorkspaceVariableGridWidget.registerForLiveEvent('click', '#Save-DiscreteWorkspaceVariables:not(.disabled)', function () {
            $('#Save-DiscreteWorkspaceVariables').addClass('display-none');
            $('#Loader-DiscreteWorkspaceVariables').removeClass('display-none');
                
            // Set ID arrays as empty - needed for save method (valid model state), but not used by discrete variables
            for (var ndx = 0; ndx < DiscreteWorkspaceVariableGridWidget.data.length; ndx++) {
                DiscreteWorkspaceVariableGridWidget.data[ndx].BOEToSum = {};
                DiscreteWorkspaceVariableGridWidget.data[ndx].WBSToSum = {};
                DiscreteWorkspaceVariableGridWidget.data[ndx].CLINToSum = {};
                DiscreteWorkspaceVariableGridWidget.data[ndx].ResourceTypes = {};
            }

            var dataToSend = JSON.stringify(DiscreteWorkspaceVariableGridWidget.data);

            DiscreteWorkspaceVariableGridWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_SAVE_WORKSPACE_VARIABLES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function () {
                    $(document).trigger("CLEAN_WORKSPACE_SETTINGS_DIRTY");
                    $(document).trigger("WS_LOAD_DISCRETE_VARIABLES");
                },
                error: function () {
                    $('#Save-DiscreteWorkspaceVariables').removeClass('display-none');
                    $('#Save-DiscreteWorkspaceVariables').addClass('disabled');
                    $('#Loader-DiscreteWorkspaceVariables').addClass('display-none');
                }
            });
        });

        DiscreteWorkspaceVariableGridWidget.ContainsOCI = '<%= ViewData["ContainsOCI"] %>'.isTrue();

		if (DiscreteWorkspaceVariableGridWidget.ContainsOCI == true) {
			var text = <%: SiteMasterUtilities.GetBannerText(true) %>;
            $('#OCINote').html('<b>Note:</b> ' + text);
        }
		else {
			var text = <%: SiteMasterUtilities.GetBannerText() %>;
            $('#OCINote').html('<b>Note:</b> ' + text);
        }

        refreshModule($('.discrete-workspace-variables.module'));
    });
    
</script>

<div id="DiscreteWorkspaceVariables" class="discrete-workspace-variables module">
    <div class="module-header-data">Discrete Workspace Variables</div>
    <div class="module-content-data">
        <div class="form-row">
            Define discrete variables that can be used across all BOEs within the Workspace.  
            If the value of a Workspace Variable changes, the Author will be notified of the change.  
            Changes can only be made when the Workspace is in the Initialization or Working State.
        </div>

        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "DiscreteWorkspaceVariablesForm" }))
        { %>

        <ul class="validation-box"></ul>
        <table id="WorkspaceVariableGrid" class="addable grid editable">
            <thead id="WorkspaceVariableGridHeader">
                <tr>
                    <th class="variable-name">Workspace Variable *</th>
                    <th class="variable-valueType" style="display:none">Value Type *</th>
                    <th class="variable-value">Value *</th>
                    <% if (readOnly)
                       { %>
                            <th class="last-child" style="width:35px"></th>
                    <% }
                       else
                       { %>
                            <th class="delete last-child" style="width:35px"><div class="delete"></div></th>
                    <% } %>
                </tr>
            </thead>
            <tbody>
                <%  foreach (var item in Model) { %>
                        <tr pkid="<%: item.WorkspaceVariableID %>">
                            <td class="variable-name" >
                                <% if (readOnly)
                                   { %>
                                        <%: item.WorkspaceVariableName %>
                                <% }
                                   else
                                   { %>
                                        <input name="WorkspaceVariableName" value="<%:item.WorkspaceVariableName %>" originalvalue="<%:item.WorkspaceVariableName%>" maxlength="20" />
                                <% } %>
                            </td>
                            <td class="variable-valueType" style="display:none">
                                <% if (readOnly)
                                   { %>
                                        <%: item.WorkspaceVariableValueTypes.First(t => t.Value == item.WorkspaceVariableValueType.ToString()).Text %>
                                <% }
                                   else
                                   { %>
                                        <%: Html.DropDownList("WorkspaceVariableValueType", item.WorkspaceVariableValueTypes, new { @onchange = "DiscreteWorkspaceVariableGridWidget.updateData(this);" })%>
                                <% } %>
                            </td>             
                            <td class="variable-value">
                                <% if (readOnly)
                                   {
                                       if (item.WorkspaceVariableValueType == (int)VarValueType.SumOfBOEs)
                                       { %>
                                            <div name="SumOfBOEsValueField">
                                                <a><%:item.WorkspaceVariableValue%></a>
                                            </div>
                                    <% }
                                       else
                                       { %>
                                            <div name="DiscreteValueField">
                                                <%:item.WorkspaceVariableValue%>
                                            </div>
                                    <% }
                                   }
                                   else
                                   { %>
                                        <div name="DiscreteValueField" class="display-none">
                                            <input name="WorkspaceVariableValue" onchange="DiscreteWorkspaceVariableGridWidget.variableValueChanged(this);" value="<%:item.WorkspaceVariableValue %>" originalvalue="<%:item.WorkspaceVariableValue%>"/>
                                        </div>
                                        <div name="SumOfBOEsValueField" class="display-none">
                                            <a><%:item.WorkspaceVariableValue %></a>
                                        </div>
                                        <div name="SumOfBOEsLinkField" class="display-none">
                                            <a>Select BOEs</a>
                                        </div>
                                <% } %>
                            </td>
                            <td>
                                <%: Html.Hidden("InUse", item.InUse) %>
                                <%: Html.Hidden("WorkspaceID", WorkspaceID)%>
                                <%: Html.Hidden("Deleted", item.Deleted) %>
                                <%: Html.Hidden("UpdateDateLong", item.UpdateDateLong)%>
                                <% if (readOnly)
                                   { %>
                                <% }
                                   else if (item.InUse)
                                   { %>
                                        <div class="in-use">In Use</div>
                                <% }
                                   else
                                   { %>
                                        <div class="delete"></div>
                                <% } %>
                            </td>
                        </tr>
                <% }
                   
                   if (!readOnly)
                   { %>
                        <tr class="blank">
                            <td class="variable-name"><div><div class="default-text">Add a workspace variable name</div><input name="WorkspaceVariableName" /></div></td>
                            <td class="variable-valueType" style="display:none"><%: Html.DropDownList("WorkspaceVariableValueType", new WorkspaceVariableModelView().WorkspaceVariableValueTypes, new { @onchange = "DiscreteWorkspaceVariableGridWidget.updateData(this);", @class = "default-text" })%></td>  
                            <td class="variable-value">
                                <div name="DiscreteValueField" class="display-none">
                                    <div class="default-text">Add a value</div><input name="WorkspaceVariableValue" onchange="DiscreteWorkspaceVariableGridWidget.variableValueChanged(this);"/>
                                </div>
                                <div name="SumOfBOEsValueField" class="display-none">
                                    <a></a>
                                </div>
                                <div name="SumOfBOEsLinkField" class="display-none">
                                    <a>Select BOEs</a>
                                </div>
                            </td>
                            <td>
                                <%: Html.Hidden("InUse", "False")%>
                                <%: Html.Hidden("WorkspaceID", WorkspaceID)%>
                                <%: Html.Hidden("Deleted", "False")%>
                            <div class="delete display-none"></div>
                        </td>
                        </tr>
                <% } %>
            </tbody>
        </table>

        <div id="OCINote" class="oci-note"></div>
        <% if (readOnly)
        { %>
            <button id="Back-DiscreteWorkspaceVariables" class="ies back-to-workspace-settings-button" type="button">Back to Workspace Settings</button>
        <% }
        else
        { %>
            <div class="buttons">
                <button id="Save-DiscreteWorkspaceVariables" class="ies-action disabled" name="save-button" type="button">Save</button>
                <div id="Loader-DiscreteWorkspaceVariables" class="loader display-none"></div>
                <button id="Cancel-DiscreteWorkspaceVariables" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        <% }
        }%>
    </div>
</div>