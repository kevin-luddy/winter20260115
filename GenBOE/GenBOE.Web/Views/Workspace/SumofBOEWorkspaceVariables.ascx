<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.WorkspaceVariableModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>

<% int WorkspaceID = Convert.ToInt32(ViewData["WorkspaceID"]);
   JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
   bool readOnly = bool.Parse(ViewData["READONLY"] as string); %>

<script type="text/javascript">
    var WorkspaceVariableGridWidget = new GridWidget("WorkspaceVariables", "WorkspaceVariableID", '<%: readOnly %>'.isTrue());
    
    WorkspaceVariableGridWidget.resetElement = function (input) {
        $(input).val($(input).attr("originalvalue"));
    };

    WorkspaceVariableGridWidget.registerForEvent('workspace-variables_NEW_ROW_CREATED', function (e) {
        WorkspaceVariableGridWidget.newItemCount--;
        $('#WorkspaceVariableGrid .blank').attr("pkid", WorkspaceVariableGridWidget.newItemCount);
        $('#WorkspaceVariableGrid tr:not(.blank) td div.delete').removeClass('display-none');
        
        var valueTypeSelect = $('#WorkspaceVariableGrid .blank select[name*=WorkspaceVariableValueType]');

        if (valueTypeSelect.length) {
            valueTypeSelect.change(function () {
                WorkspaceVariableGridWidget.ValueTypeChanged($(this));
                WorkspaceVariableGridWidget.updateData(this);
            });
        }

        refreshModule($('.workspace-variables.module'));
    });

    WorkspaceVariableGridWidget.variableValueChanged = function (input) {
       
        $(input).parents('td').focusout();
        $(input).val(Helper.addCommas(Helper.removeCommas($(input).val())));

        var parentRow = $(input).parents('tr');
        var variableInUse = parentRow.find('input[name=InUse]').val().isTrue();

        if (variableInUse) {

            var body = parentRow.find("input[name=WorkspaceVariableName]").val() +
                        " is used in at least one BOE. Are you sure you want to change the value? Click \"YES\" to keep the new value." +
                        " Once saved the Authors will be notified of the change. Click \"No\" to keep the old value.";
            Session.confirmDialog("Change Workspace Variable", body, function () { WorkspaceVariableGridWidget.updateData($(input)); }, function () { WorkspaceVariableGridWidget.resetElement(input); });
        } else {
            WorkspaceVariableGridWidget.updateData($(input));
            $(input).parents('td').focusin();           
        }
    };

    WorkspaceVariableGridWidget.specialUpdateInstructions = function (input, jsonIndex) {
        var ElementName = $(input).attr('name');
        var ElementValue = $(input).val();

        if (ElementName == 'WorkspaceVariableValue') {
            WorkspaceVariableGridWidget.data[jsonIndex].IsPercentage = (ElementValue.match(/%/) != null);
        }
    };

    WorkspaceVariableGridWidget.setReadOnlyForEdit = function () {
        // Remove input to prevent additional rows from being created until new row is saved
        $("tr.blank").remove();

        $('tr[isSumming=True]').each(function () {
            $(this).find('td.variable-value').addClass("disabled").removeClass("variable-value");
            $(this).find('td div.delete').addClass("disabled").removeClass("delete");
            $(this).find('input[name=WorkspaceVariableName]').prop("disabled", true);
        });

        // Disable delete all unused button
        $("#WorkspaceVariableGridHeader .delete").off();
    }

    WorkspaceVariableGridWidget.variableDeleted = function () {
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
            if (WorkspaceVariableGridWidget.isDirty() && isExistingVariable) {
                confirmationMsg += "<br/><br/>Note: Any changes to Variable Names will also be saved.";
            }
            Session.confirmDialog("Delete Workspace Variable", confirmationMsg, function () {
                // Set the value to 0, to pass validation
                parentRow.find('input[name=WorkspaceVariableValue]').val('0');

                WorkspaceVariableGridWidget.deleteRecord(toDelete);
                if (isExistingVariable) {
                    $('#Save-WorkspaceVariables').removeClass('disabled');
                    $('#Save-WorkspaceVariables').click();
                }
            });
        }
    };

    
    WorkspaceVariableGridWidget.ValueTypeChanged = function (valueTypeElement) {
        var selectedValue = valueTypeElement.val();
        var parentRow = valueTypeElement.parents('tr');
        
        if (selectedValue == '<%: (int)VarValueType.SumOfBOEs %>') {
            var BOEToSum = JSON.parse(parentRow.find('input[name^=BOEToSum]').val());

            var WBSToSum = JSON.parse(parentRow.find('input[name^=WBSToSum]').val());

            var CLINToSum = JSON.parse(parentRow.find('input[name^=CLINToSum]').val());

            var ResourceTypes = JSON.parse(parentRow.find('input[name^=ResourceTypes]').val());

            var SortBOEBy = parentRow.find('input[name^=SortBOEBy]').val();

            if (BOEToSum.length == 0 && WBSToSum.length == 0 && CLINToSum.length == 0) {
                valueTypeElement.removeClass('default-text');
                parentRow.find('div[name=SumOfBOEsLinkField]').removeClass('display-none').siblings().addClass('display-none');
                parentRow.find('input[name=WorkspaceVariableValue]').val('');
                
                WorkspaceVariableGridWidget.setReadOnlyForEdit();
                parentRow.find('div[name=SumOfBOEsLinkField]').find('a:first').click();
            }
            else {
                valueTypeElement.removeClass('default-text');
                parentRow.find('div[name=SumOfBOEsValueField]').removeClass('display-none').siblings().addClass('display-none');
                parentRow.find('input[name=WorkspaceVariableValue]').val(parentRow.find('div[name=SumOfBOEsValueField] a').text());
            }
        }

        else {
            valueTypeElement.addClass('default-text');
            parentRow.find('td.variable-value').children().addClass('display-none');
        }
    };

    WorkspaceVariableGridWidget.GetVariableRow = function(pkid) {
        return $('#WorkspaceVariableGrid tbody tr[pkid=' + pkid + ']');
    }

    WorkspaceVariableGridWidget.GetSummedBOEForVariable = function(pkid) {
        var variableRow = WorkspaceVariableGridWidget.GetVariableRow(pkid);
        
        return JSON.parse(variableRow.find('input[name=BOEToSum]').val());
    };

    WorkspaceVariableGridWidget.GetSummedWBSForVariable = function(pkid) {
        var variableRow = WorkspaceVariableGridWidget.GetVariableRow(pkid);
        
        return JSON.parse(variableRow.find('input[name=WBSToSum]').val());
    };

    WorkspaceVariableGridWidget.GetSummedCLINForVariable = function(pkid) {
        var variableRow = WorkspaceVariableGridWidget.GetVariableRow(pkid);
        
        return JSON.parse(variableRow.find('input[name=CLINToSum]').val());
    };

    WorkspaceVariableGridWidget.GetSortBOEByForVariable = function (pkid) {
        var variableRow = WorkspaceVariableGridWidget.GetVariableRow(pkid);
        return variableRow.find('input[name=SortBOEBy]').val();
    };

    WorkspaceVariableGridWidget.GetResourceTypesForVariable = function (pkid) {
        var variableRow = WorkspaceVariableGridWidget.GetVariableRow(pkid);

        return JSON.parse(variableRow.find('input[name=ResourceTypes]').val());
    };

    WorkspaceVariableGridWidget.BackToJumpPage = function() {
        WorkspaceVariableGridWidget.cleanDirty('WorkspaceVariablesForm');
        window.location.hash = '#';
    };

    $(function () {
        WorkspaceVariableGridWidget.afterDOMLoad();

        WorkspaceVariableGridWidget.newItemCount = 0;

        createModule($('#WorkspaceVariables'));
        AddableGrid('workspace-variables');

        WorkspaceVariableGridWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () {
            WorkspaceVariableGridWidget.cleanDirty('WorkspaceVariablesForm');
        });

        $('select[name*=WorkspaceVariableValueType]').each(function () {
            $(this).change(function () {
                WorkspaceVariableGridWidget.ValueTypeChanged($(this));
                WorkspaceVariableGridWidget.updateData(this);
            });

            WorkspaceVariableGridWidget.ValueTypeChanged($(this));
        });

        WorkspaceVariableGridWidget.registerForDelegateEvent('focusin', 'td :input', function () {
            if (!$(this).parent().children('div').hasClass('delete')) {
                $(this).parent().addClass('selected');
            }
        }, '#WorkspaceVariables');

        WorkspaceVariableGridWidget.registerForDelegateEvent('focusout', '.workspace-variables td :input', function () {
            $(this).parent().removeClass('selected');
        }, '#WorkspaceVariables');

        $('#Cancel-WorkspaceVariables, #Back-WorkspaceVariables').click(function () {
            if (WorkspaceVariableGridWidget.isDirty()) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function () {
                        WorkspaceVariableGridWidget.BackToJumpPage();
                    },
                    null);
            }
            else {
                WorkspaceVariableGridWidget.BackToJumpPage();
            }
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('click', '#WorkspaceVariableGrid td.variable-value a', function () {

            var parentRow = $(this).parents('tr');

            var eventData = {};
            eventData.pkid = parentRow.attr('pkid');
            eventData.WorkspaceVariables = true;

            var sumOfBOEsEventAppend = '';

            if (WorkspaceVariableGridWidget.isReadOnly()) {
                sumOfBOEsEventAppend = 'ReadOnly';
            }

            // an id of -1 means a new row of data
            if (eventData.pkid === -1) {
                ShowLoadingBox();

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                       '<%: WebConstants.ACTION_FIND_VALID_BOES_FOR_NOT_IN_USE_WORKSPACE_VARIABLE %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) {
                        HideLoadingBox();
                        if (response != undefined && response.Status != false) {
                            eventData.ValidBOEs = response;
                            $(document).trigger('VariableBOESumByWBS_OpenNew', eventData);
                        }
                    },
                    error: function () {
                        HideLoadingBox();
                    }
                });
            } else {
                eventData.BOEToSum = WorkspaceVariableGridWidget.GetSummedBOEForVariable(eventData.pkid);
                eventData.WBSToSum = WorkspaceVariableGridWidget.GetSummedWBSForVariable(eventData.pkid);
                eventData.CLINToSum = WorkspaceVariableGridWidget.GetSummedCLINForVariable(eventData.pkid);
                eventData.ResourceTypes = WorkspaceVariableGridWidget.GetResourceTypesForVariable(eventData.pkid);
                eventData.SortBOEBy = WorkspaceVariableGridWidget.GetSortBOEByForVariable(eventData.pkid);
                eventData.SelectBOELink = $(this).parent().attr('name') === 'SumOfBOEsLinkField' ? true : false;

                var urlToUse = '';
                if (parentRow.find('input[name=InUse]').val().isTrue()) {
                    urlToUse = '<%: WebConstants.ACTION_FIND_VALID_BOES_FOR_WORKSPACE_VARIABLE %>';
                } else {
                    urlToUse ='<%: WebConstants.ACTION_FIND_VALID_BOES_FOR_NOT_IN_USE_WORKSPACE_VARIABLE %>'
                }

                ShowLoadingBox();

                var dataToSend = { workspaceVariableID: eventData.pkid };
                dataToSend = JSON.stringify(dataToSend);

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>', urlToUse, ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) {
                        HideLoadingBox();
                        if (response != undefined && response.Status != false) {
                            eventData.ValidBOEs = response;

                            if (eventData.SortBOEBy == '<%:(int)VarSortBOEBy.CLIN%>') {
                                $(document).trigger('VariableBOESumByCLIN_Open' + sumOfBOEsEventAppend, eventData);
                            }
                            else {
                                $(document).trigger('VariableBOESumByWBS_Open' + sumOfBOEsEventAppend, eventData);
                            }
                        }
                    },
                    error: function () {
                        HideLoadingBox();
                    }
                });
            }
        });

        $("#WorkspaceVariableGridHeader .delete").click(function () {
            Session.confirmDialog("Delete Workspace Variables", "Are you sure you want to delete all Sum of BOE Workspace Variables not in use?", function () {

                var dataToSend = JSON.stringify({valueType: '<%:VarValueType.SumOfBOEs%>'});

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DELETE_ALL_WORKSPACE_VARIABLES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) {
                        $(document).trigger("WS_LOAD_SUM_OF_BOE_VARIABLES");
                    }
                });
            });
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('click', '#WorkspaceVariableGrid tr:not(.blank) td div.delete', WorkspaceVariableGridWidget.variableDeleted);


        WorkspaceVariableGridWidget.registerForLiveEvent('click', '#WorkspaceVariableGrid tbody td.variable-name, #WorkspaceVariableGrid tbody td.variable-value', function () {
            $(this).find('.default-text').addClass('display-none');
            $(this).find('input').removeClass('display-none').focus();
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('focusin', '#WorkspaceVariableGrid tbody td.variable-name :input, #WorkspaceVariableGrid tbody td.variable-value :input', function () {
            $(this).parent().find('.default-text').addClass('display-none');
            $(this).parent().find('input').removeClass('display-none');
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('focusout', '#WorkspaceVariableGrid tbody td.variable-name :input, #WorkspaceVariableGrid tbody td.variable-value :input', function () {
            if (!$.trim($(this).parent().find('input').val()).length) {
                $(this).parent().find('.default-text').removeClass('display-none');
            }
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('change', '#WorkspaceVariableGrid tbody td.variable-name :input', function () {
            $(this).parents('td').focusout(); 
            var valueType = $(this).parents('tr').find('select[name^=WorkspaceVariableValueType]');
            valueType.val('<%:(int)VarValueType.SumOfBOEs%>');
            valueType.change();
            WorkspaceVariableGridWidget.updateData(this);
        });

        WorkspaceVariableGridWidget.registerForLiveEvent('click', '#Save-WorkspaceVariables:not(.disabled)', function () {
            $('#Save-WorkspaceVariables').addClass('display-none');
            $('#Loader-WorkspaceVariables').removeClass('display-none');

            // Convert all ID arrays from JSON strings to array objects
            for (var ndx = 0; ndx < WorkspaceVariableGridWidget.data.length; ndx++) {
                WorkspaceVariableGridWidget.data[ndx].BOEToSum = JSON.parse(WorkspaceVariableGridWidget.data[ndx].BOEToSum);
                WorkspaceVariableGridWidget.data[ndx].WBSToSum = JSON.parse(WorkspaceVariableGridWidget.data[ndx].WBSToSum);
                WorkspaceVariableGridWidget.data[ndx].CLINToSum = JSON.parse(WorkspaceVariableGridWidget.data[ndx].CLINToSum);
                WorkspaceVariableGridWidget.data[ndx].ResourceTypes = JSON.parse(WorkspaceVariableGridWidget.data[ndx].ResourceTypes);
            }

            var dataToSend = JSON.stringify(WorkspaceVariableGridWidget.data);

            WorkspaceVariableGridWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_SAVE_WORKSPACE_VARIABLES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: function () {
                    $(document).trigger("CLEAN_WORKSPACE_SETTINGS_DIRTY");
                    $(document).trigger("WS_LOAD_SUM_OF_BOE_VARIABLES");
                },
                error: function () {
                    $('#Save-WorkspaceVariables').removeClass('display-none');
                    $('#Save-WorkspaceVariables').addClass('disabled');
                    $('#Loader-WorkspaceVariables').addClass('display-none');
                }
            });
        });

        WorkspaceVariableGridWidget.registerForEvent('VariableBOESumByCLIN_Create', function (event, eventData) {
            var variableRow = WorkspaceVariableGridWidget.GetVariableRow(eventData.pkid);
            
            if (variableRow != undefined) {
                variableRow.find('input[name=BOEToSum]').val(JSON.stringify(eventData.BOEToSum));
                variableRow.find('input[name=WBSToSum]').val('[]');
                variableRow.find('input[name=CLINToSum]').val(JSON.stringify(eventData.CLINToSum));
                variableRow.find('input[name=ResourceTypes]').val(JSON.stringify(eventData.ResourceTypes));
                variableRow.find('input[name=SortBOEBy]').val(eventData.SortBOEBy);
                
                variableRow.find('div[name=SumOfBOEsLinkField]').addClass('display-none');
                variableRow.find('div[name=SumOfBOEsValueField] a').html(Helper.addCommas(eventData.Sum));
                variableRow.find('div[name=SumOfBOEsValueField]').removeClass('display-none');
                variableRow.find('input[name=WorkspaceVariableValue]').val(Helper.addCommas(eventData.Sum)).focusout();

                WorkspaceVariableGridWidget.updateData(variableRow.find('input[name=CLINToSum]'));

                $('#Save-WorkspaceVariables').click();
            }
        });

        WorkspaceVariableGridWidget.registerForEvent('VariableBOESumByWBS_Create', function (event, eventData) {
            var variableRow = WorkspaceVariableGridWidget.GetVariableRow(eventData.pkid);
            
            if (variableRow != undefined) {
                variableRow.find('input[name=BOEToSum]').val(JSON.stringify(eventData.BOEToSum));
                variableRow.find('input[name=WBSToSum]').val(JSON.stringify(eventData.WBSToSum));
                variableRow.find('input[name=CLINToSum]').val('[]');
                variableRow.find('input[name=ResourceTypes]').val(JSON.stringify(eventData.ResourceTypes));
                variableRow.find('input[name=SortBOEBy]').val(eventData.SortBOEBy);

                variableRow.find('div[name=SumOfBOEsLinkField]').addClass('display-none');
                variableRow.find('div[name=SumOfBOEsValueField] a').html(Helper.addCommas(eventData.Sum));
                variableRow.find('div[name=SumOfBOEsValueField]').removeClass('display-none');
                variableRow.find('input[name=WorkspaceVariableValue]').val(Helper.addCommas(eventData.Sum)).focusout();

                WorkspaceVariableGridWidget.updateData(variableRow.find('input[name=WBSToSum]'));

                $('#Save-WorkspaceVariables').click();                
            }
        });
        WorkspaceVariableGridWidget.ContainsOCI = '<%= ViewData["ContainsOCI"] %>'.isTrue();

		if (WorkspaceVariableGridWidget.ContainsOCI == true) {
			var text = '<%: SiteMasterUtilities.GetBannerText(true) %>';
            $('#OCINote').html('<b>Note:</b> ' + text);
        }
		else {
			var text = '<%: SiteMasterUtilities.GetBannerText() %>';
            $('#OCINote').html('<b>Note:</b> ' + text);
        }

        refreshModule($('.workspace-variables.module'));

        $.ajax({
            type: 'POST',
            url: GenSession.CreateUrl({
                controller: '<%: WebConstants.CONTROLLER_BOE_LABOR %>',
                action: '<%: WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS %>',
                workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>'
            }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#variableBoeByWbs').html(response);
            }
        });

        $.ajax({
            type: 'POST',
            url: GenSession.CreateUrl({
                controller: '<%: WebConstants.CONTROLLER_BOE_LABOR %>',
                action: '<%: WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN %>',
                workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>'
            }),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#variableBoeByClin').html(response);
            }
        });
    });
    
</script>

<div id="WorkspaceVariables" class="workspace-variables module">
    <div class="module-header-data">Sum of BOE Workspace Variables</div>
    <div class="module-content-data">
        <div class="form-row">
            Define sum of BOE variables that can be used across all BOEs within the Workspace.  
            If the value of a Workspace Variable changes, the Author will be notified of the change.  
            Changes can only be made when the Workspace is in the Initialization or Working State.
        </div>
        
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "WorkspaceVariablesForm" })) { %>
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
                        <tr pkid="<%: item.WorkspaceVariableID %>" isSumming="<%:item.BOEToSum.Any() || item.CLINToSum.Any() || item.WBSToSum.Any() %>">
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
                                        <%: Html.DropDownList("WorkspaceVariableValueType", item.WorkspaceVariableValueTypes, new { @onchange = "WorkspaceVariableGridWidget.updateData(this);" })%>
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
                                            <input name="WorkspaceVariableValue" onchange="WorkspaceVariableGridWidget.variableValueChanged(this);" value="<%:item.WorkspaceVariableValue %>" originalvalue="<%:item.WorkspaceVariableValue%>"/>
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
                                <%: Html.Hidden("BOEToSum", serializer.Serialize(item.BOEToSum))%>
                                <%: Html.Hidden("WBSToSum", serializer.Serialize(item.WBSToSum))%>
                                <%: Html.Hidden("CLINToSum", serializer.Serialize(item.CLINToSum))%>
                                <%: Html.Hidden("ResourceTypes", serializer.Serialize(item.ResourceTypes))%>
                                <%: Html.Hidden("SortBOEBy", (int)item.SortBOEBy) %>
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
                            <td class="variable-valueType" style="display:none"><%: Html.DropDownList("WorkspaceVariableValueType", new WorkspaceVariableModelView().WorkspaceVariableValueTypes, new { @onchange = "WorkspaceVariableGridWidget.updateData(this);", @class = "default-text" })%></td>  
                            <td class="variable-value">
                                <div name="DiscreteValueField" class="display-none">
                                    <div class="default-text">Add a value</div><input name="WorkspaceVariableValue" onchange="WorkspaceVariableGridWidget.variableValueChanged(this);"/>
                                </div>
                                <div name="SumOfBOEsValueField" class="display-none">
                                    <a></a>
                                </div>
                                <div name="SumOfBOEsLinkField" class="display-none">
                                    <a>Select BOEs</a>
                                </div>
                            </td>
                            <td>
                                <%: Html.Hidden("BOEToSum", "[]")%>
                                <%: Html.Hidden("WBSToSum", "[]")%>
                                <%: Html.Hidden("CLINToSum", "[]")%>
                                <%: Html.Hidden("ResourceTypes", "[]")%>
                                <%: Html.Hidden("SortBOEBy", (int)VarSortBOEBy.WBS) %>
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
                <button id="Back-WorkspaceVariables" class="ies back-to-workspace-settings-button" type="button">Back to Workspace Settings</button>
        <% }
           else
           { %>
                <div class="buttons">
                    <button id="Save-WorkspaceVariables" class="ies-action disabled" name="save-button" type="button">Save</button>
                    <div id="Loader-WorkspaceVariables" class="loader display-none"></div>
                    <button id="Cancel-WorkspaceVariables" class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
        <% }
        } %>
    </div>
</div>

<div id="variableBoeByWbs"></div>
<div id="variableBoeByClin"></div>
