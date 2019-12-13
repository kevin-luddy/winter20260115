<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.VariableBOESumByCLINModelView>>" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import Namespace="System.Collections.ObjectModel" %>

<%
    Collection<SumVariableResourceTypeModelView> sumVariableResourceTypes = ViewData["SumVariableResourceTypes"] as Collection<SumVariableResourceTypeModelView>;
    
    int decimalPrecision = 0;
    if (Model.Any())
    { 
        decimalPrecision = Model.First().WorkspaceDecimalPrecision ?? 0;
    }
%>

<script type="text/javascript">

    var VariableBOESumByCLIN = new Widget("VariableBOESumByCLIN");

    VariableBOESumByCLIN.Dialog = {};
    VariableBOESumByCLIN.EventData = {};

    VariableBOESumByCLIN.Initialize = function () {
        VariableBOESumByCLIN.Dialog.Element = $('#VariableBOESumByCLIN');
        VariableBOESumByCLIN.Dialog.Params = { title: "Select BOEs to Sum", modal: true, resizable: false, width: 680 };
        VariableBOESumByCLIN.InitializeDialog(VariableBOESumByCLIN.Dialog);
    };

    // Remove old dialogs left behind when jumping back to main Settings jump page
    VariableBOESumByCLIN.RemoveStaleDialogs = function () {
        $('body .ui-dialog').children('#VariableBOESumByCLIN').parent().remove();
        $('body').children('#VariableBOESumByCLIN').remove();
    }

    VariableBOESumByCLIN.ResetDialog = function (dialogTitle) {
        $('select[name=VariableBOESumByCLIN-SortType] option[value="1"]').prop('selected', true);
        VariableBOESumByCLIN.ChangeDialogTitle(VariableBOESumByCLIN.Dialog, dialogTitle);
        $('#VariableBOESumByCLIN div[name=createVariableItems], #VariableBOESumByCLIN div.create-item').show();
        $('#VariableBOESumByCLIN table tbody input.checkbox').prop('checked', false).prop('disabled', false);
        $('#VariableBOESumByCLIN-CreateButton').addClass('disabled').html('Create Variable with selected BOEs');
        $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]').prop('disabled', false).prop('checked', false);
    };

    VariableBOESumByCLIN.ResetDialogReadOnly = function (dialogTitle) {
        $('select[name=VariableBOESumByCLIN-SortType] option[value="1"]').prop('selected', true);

        if (dialogTitle != undefined) {
            VariableBOESumByCLIN.ChangeDialogTitle(VariableBOESumByCLIN.Dialog, dialogTitle);
        }
        else {
            VariableBOESumByCLIN.ChangeDialogTitle(VariableBOESumByCLIN.Dialog, 'BOEs Selected to Sum');
        }

        $('#VariableBOESumByCLIN table tbody tr td div.circular-reference').remove();

        $('#VariableBOESumByCLIN div[name=createVariableItems], #VariableBOESumByCLIN div.create-item').hide();

        $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]').prop('disabled', true);
    };

    VariableBOESumByCLIN.SetDisabledElements = function () {
        $('#VariableBOESumByCLIN table tbody tr td div.circular-reference').remove();

        var disabledCheckboxes = $('#VariableBOESumByCLIN table tbody input.checkbox.disabled');

        if (disabledCheckboxes.length) {
            $('#VariableBOESumByCLIN div[name=CircularReferenceNote]').removeClass('display-none');
        }
        else {
            $('#VariableBOESumByCLIN div[name=CircularReferenceNote]').addClass('display-none');
        }

        disabledCheckboxes.each(function () {
            $(this).prop('disabled', true).after('<div class="circular-reference"></div>');

            var parentRow = $(this).parents('tr');

            var thisFKID = parentRow.attr('fkid');

            var preceding = parentRow.prevAll('tr');

            preceding.each(function () {

                if ($(this).attr('pkid') == thisFKID && !$(this).find('div.circular-reference').length) {
                    $(this).find('input.checkbox').prop('disabled', true).after('<div class="circular-reference"></div>');
                    return false;
                }
            });
        });
    };

    VariableBOESumByCLIN.SetCreateButtonState = function () {
        if ($('#VariableBOESumByCLIN-BOEToCopyGrid tbody input:checked').length > 0) {
            $('#VariableBOESumByCLIN-CreateButton').removeClass('disabled');
        }
        else {
            $('#VariableBOESumByCLIN-CreateButton').addClass('disabled');
        }
    };

    VariableBOESumByCLIN.CalculateTotal = function () {
        var total = 0;

        var boeLineTotals = $('tbody input:checked', $('#VariableBOESumByCLIN-BOEToCopyGrid')).parents('tr[itemtype="BOE"]').find('td[name=LineTotal]');

        boeLineTotals.each(function () {
            total += parseFloat($(this).text());
        });

        //set Total with decimal precision to correct floating point rounding errors, parse float to remove trailing zeros
        $('#VariableBOESumByCLIN-TotalSum').html(parseFloat(total.toFixed(<%:decimalPrecision%>)));
    };

    VariableBOESumByCLIN.GoToSelectedBOE = function (boeID) {
        // Get the selected BOE ID 
        window.open(CreatePostURL(
            '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
            '<%: WebConstants.CONTROLLER_BOE %>',
            '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>',
            'boe/' + boeID));
    };

    VariableBOESumByCLIN.ChangeToSortByWBS = function () {
        VariableBOESumByCLIN.EventData.SortBOEBy = '<%:(int)VarSortBOEBy.WBS%>';

        if (VariableBOESumByCLIN.EventData.ValidBOEs != undefined) {
            VariableBOESumByCLIN.EventData.BOEToSum = VariableBOESumByCLIN.EventData.WBSToSum = VariableBOESumByCLIN.EventData.CLINToSum = [];
            VariableBOESumByCLIN.EventData.ResourceTypes = VariableBOESumByCLIN.GatherResourceTypesToCalculate();
            $(document).trigger('VariableBOESumByWBS_Open', VariableBOESumByCLIN.EventData);
        }
        else {
            var selectionsExist = (VariableBOESumByCLIN.EventData.BOEToSum.length > 0);

            $(document).trigger('VariableBOESumByWBS_OpenNew', VariableBOESumByCLIN.EventData);

            if (selectionsExist) {
                $('#VariableBOESumByWBS-CreateButton').removeClass('disabled').html('Update Variable with selected BOEs');
            }
        }

        $(document).trigger('VariableBOESumByCLIN_Close');
    };

    VariableBOESumByCLIN.BindEvents = function () {
        // BIND Element events
        $('select[name=VariableBOESumByCLIN-SortType]').change(function () {
            if ($(this).val() == 0) {
                if ($('#VariableBOESumByCLIN-BOEToCopyGrid tbody input:checked').length) {
                    var that = $(this);
                    Session.confirmDialog(
                        "BOE Selections Will be Lost",
                        "Changing the sort view will clear all selections made.  Are you sure you want to sort by WBS?",
                        function () {
                            VariableBOESumByCLIN.ChangeToSortByWBS();
                        },
                        function () {
                            that.val(1);
                        });
                }
                else {
                    VariableBOESumByCLIN.ChangeToSortByWBS();
                }
            }
        });

        $('#VariableBOESumByCLIN-BOEToCopyGrid tbody input[name=CLINSelect]').change(function () {
            var isChecked = $(this).is(':checked');
            var clinID = $(this).parents('tr').attr('pkid');

            if (isChecked) {
                $(this).parents('tbody').children('tr[fkid=' + clinID + ']').find('input[name=BOESelect]').prop('checked', true).prop('disabled', true);
            }
            else {
                $(this).parents('tbody').children('tr[fkid=' + clinID + ']').find('input[name=BOESelect]').prop('checked', false).prop('disabled', false);
            }

            VariableBOESumByCLIN.SetCreateButtonState();
            VariableBOESumByCLIN.CalculateTotal();
        });

        $('#VariableBOESumByCLIN-BOEToCopyGrid tbody input[name=BOESelect]').change(function () {
            VariableBOESumByCLIN.SetCreateButtonState();
            VariableBOESumByCLIN.CalculateTotal();
        });

        $('#VariableBOESumByCLIN-BOEToCopyGrid tbody tr td a[name=boeLink]').click(function () {
            VariableBOESumByCLIN.GoToSelectedBOE($(this).attr('boeID'));
        });

        VariableBOESumByCLIN.registerForLiveEvent('click', '#VariableBOESumByCLIN-CreateButton:not(.disabled)', function () {
            var eventData = {};
            eventData.pkid = VariableBOESumByCLIN.EventData.pkid;
            eventData.Sum = parseFloat($('#VariableBOESumByCLIN-TotalSum').text());
            eventData.CLINToSum = [];
            eventData.SortBOEBy = '<%:(int)VarSortBOEBy.CLIN%>';

            $('#VariableBOESumByCLIN-BOEToCopyGrid tbody input[name=CLINSelect]:checked:not(input[disabled])').each(function () {
                eventData.CLINToSum.push($(this).parents('tr').attr('pkid'));
            });

            eventData.BOEToSum = [];

            $('#VariableBOESumByCLIN-BOEToCopyGrid tbody input[name=BOESelect]:checked:not(input[disabled])').each(function () {
                eventData.BOEToSum.push($(this).parents('tr').attr('pkid'));
            });

            eventData.ResourceTypes = [];

            $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]:checked').each(function () {
                eventData.ResourceTypes.push($(this).val());
            });

            VariableBOESumByCLIN.CloseDialog(VariableBOESumByCLIN.Dialog);

            $(document).trigger('VariableBOESumByCLIN_Create', eventData);
        });

        $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]').change(function () {
            VariableBOESumByCLIN.RefreshTable(undefined);
        });
    };

    VariableBOESumByCLIN.RegisterForTriggeredEvents = function () {

        VariableBOESumByCLIN.registerForEvent('VariableBOESumByCLIN_OpenNew', function (event, eventData) {
            VariableBOESumByCLIN.EventData = eventData;

            VariableBOESumByCLIN.ResetDialog('Select BOEs to Sum');

            var tableBody = $('#VariableBOESumByCLIN table tbody');

            if (eventData.WorkspaceVariables != undefined) {
                if (eventData.ValidBOEs != undefined) {
                    $('tr[fkid]', tableBody).find('input.checkbox').addClass('disabled');

                    for (var ndx in eventData.ValidBOEs) {
                        $('tr[pkid=' + eventData.ValidBOEs[ndx] + ']', tableBody).find('input.checkbox').removeClass('disabled').prop('checked', false);
                    }
                }
                else {
                    $('input.checkbox', tableBody).removeClass('disabled');
                }
            }

            $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]').prop('checked', true);

            VariableBOESumByCLIN.SetDisabledElements();
            VariableBOESumByCLIN.RefreshTable(eventData);
            VariableBOESumByCLIN.OpenDialogAfterInitialize(VariableBOESumByCLIN.Dialog);
        });

        VariableBOESumByCLIN.registerForEvent('VariableBOESumByCLIN_Open', function (event, eventData) {

            VariableBOESumByCLIN.EventData = eventData;

            VariableBOESumByCLIN.ResetDialog('Select BOEs to Sum');

            var tableBody = $('#VariableBOESumByCLIN table tbody');

            if (eventData.WorkspaceVariables != undefined) {
                if (eventData.ValidBOEs != undefined) {
                    $('tr[fkid] input.checkbox', tableBody).addClass('disabled');

                    for (var ndx in eventData.ValidBOEs) {
                        $('tr[pkid=' + eventData.ValidBOEs[ndx] + ']', tableBody).find('input.checkbox').removeClass('disabled');
                    }
                }
                else {
                    $('input.checkbox', tableBody).removeClass('disabled');
                }
            }

            $('#VariableBOESumByCLIN-CreateButton').html('Update Variable with selected BOEs');

            for (var ndx in eventData.BOEToSum) {
                $('tr[pkid=' + eventData.BOEToSum[ndx] + ']', tableBody).find('input[name=BOESelect]').prop('checked', true);
            }

            for (var ndx in eventData.CLINToSum) {
                var thisRow = $('tr[pkid=' + eventData.CLINToSum[ndx] + ']', tableBody);
                thisRow.find('input[name=CLINSelect]').prop('checked', true);
                var clinID = thisRow.parents('tr').attr('pkid');
                thisRow.parents('tbody').children('tr[fkid=' + clinID + ']').find('input[name=BOESelect]').prop('checked', true).prop('disabled', true);
            }

            // opening based on the select link will default to selecting all resource types
            if (eventData.SelectBOELink) {
                $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]').prop('checked', true);
            } else {
                // opening based on the value link will select the saved resource types
                for (var ndx in eventData.ResourceTypes) {
                    $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox][value="' + eventData.ResourceTypes[ndx] + '"]').prop('checked', true);
                }
            }

            if (eventData.BOEToSum.length || eventData.CLINToSum.length) {
                VariableBOESumByCLIN.SetCreateButtonState();
            }

            VariableBOESumByCLIN.SetDisabledElements();
            VariableBOESumByCLIN.RefreshTable(eventData);
            VariableBOESumByCLIN.OpenDialogAfterInitialize(VariableBOESumByCLIN.Dialog);
        });

        VariableBOESumByCLIN.registerForEvent('VariableBOESumByCLIN_OpenReadOnly', function (event, eventData) {
            VariableBOESumByCLIN.EventData = eventData;

            VariableBOESumByCLIN.ResetDialog('Select BOEs to Sum');
            VariableBOESumByCLIN.ResetDialogReadOnly(eventData.dialogTitle);

            var tableBody = $('#VariableBOESumByCLIN table tbody');

            $('input.checkbox', tableBody).prop('disabled', true);

            for (var ndx in eventData.BOEToSum) {
                $('tr[pkid=' + eventData.BOEToSum[ndx] + ']', tableBody).find('input[name=BOESelect]').prop('checked', true);
            }

            for (var ndx in eventData.CLINToSum) {
                var thisRow = $('tr[pkid=' + eventData.CLINToSum[ndx] + ']', tableBody);
                thisRow.find('input[name=CLINSelect]').prop('checked', true);
                var clinID = thisRow.parents('tr').attr('pkid');
                thisRow.parents('tbody').children('tr[fkid=' + clinID + ']').find('input[name=BOESelect]').prop('checked', true).prop('disabled', true);
            }

            for (var ndx in eventData.ResourceTypes) {
                $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox][value="' + eventData.ResourceTypes[ndx] + '"]').prop('checked', true);
            }

            $('input.checkbox', tableBody).prop('disabled', true);

            VariableBOESumByCLIN.RefreshTable(eventData);
            VariableBOESumByCLIN.OpenDialogAfterInitialize(VariableBOESumByCLIN.Dialog);
        });

        VariableBOESumByCLIN.registerForEvent('VariableBOESumByCLIN_Close', function () {
            VariableBOESumByCLIN.CloseDialog(VariableBOESumByCLIN.Dialog);
        });
    };

    VariableBOESumByCLIN.RefreshTable = function (eventData) {


        var actionURL = CreatePostURL(
                        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE_LABOR %>',
                        '<%: WebConstants.ACTION_REFRESH_VARIABLE_BOE_SUM_BY_CLIN %>',
                        '');

        var dataToSend = VariableBOESumByCLIN.GatherResourceTypesToCalculate();
        dataToSend = JSON.stringify(dataToSend);

        ShowLoadingBox();

        VariableBOESumByCLIN.ajaxRequest({
            type: "POST",
            url: actionURL,
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            dataType: 'json',
            success: function (response) {
                VariableBOESumByCLIN.UpdateTotals(response, eventData);
                HideLoadingBox();
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };

    VariableBOESumByCLIN.GatherResourceTypesToCalculate = function () {
        var toReturn = [];

        var resources = $('input[name=VariableBOESumByCLIN-ResourceTypeCheckbox]:checked');
        resources.each(function () {
            toReturn.push($(this).val());
        });
        return toReturn;
    };

    VariableBOESumByCLIN.UpdateTotals = function (response, eventData) {
        var selectedBOEs;
        var selectedCLINs;
        var readOnly;

        if (eventData === undefined) {
            selectedBOEs = [];
            selectedCLINs = [];
            readOnly = true;
        } else {
            selectedBOEs = Helper.undefinedOrNull(eventData.BOEToSum, []);
            selectedCLINs = Helper.undefinedOrNull(eventData.CLINToSum, []);
            readOnly = eventData.ReadOnly;
        }

        var tableBody = $('#VariableBOESumByCLIN-BOEToCopyGrid').find('tbody');

        for (var clinIndex in response) {
            if (response[clinIndex].CLINID > 0) {
                var currentCLINID = response[clinIndex].CLINID;
                var isSelectedCLIN = Helper.ArrayContains(currentCLINID, selectedCLINs);

                var clinRow = $('tr[pkid=' + currentCLINID + ']', tableBody);

                if (clinRow != undefined) {
                    var clinTotal = clinRow.children('td[name=LineTotal]');

                    if (clinTotal != undefined) {
                        clinTotal.html('<b>' + response[clinIndex].CLINTotal + '</b>');
                    }

                    if (isSelectedCLIN) {
                        clinRow.find(':checkbox').prop("checked", true);
                    }
                }

                if (response[clinIndex].BOEs != undefined) {
                    for (var boeIndex in response[clinIndex].BOEs) {
                        var currentBOEID = response[clinIndex].BOEs[boeIndex].BOEID;

                        var boeRow = $('tr[pkid=' + currentBOEID + ']', tableBody);

                        if (boeRow != undefined) {
                            var boeTotal = boeRow.children('td[name=LineTotal]');

                            if (boeTotal != undefined) {
                                boeTotal.html(response[clinIndex].BOEs[boeIndex].BOETotal);
                            }

                            if (isSelectedCLIN || Helper.ArrayContains(currentBOEID, selectedBOEs)) {
                                boeRow.find(':checkbox').prop("checked", true).prop("disabled", isSelectedCLIN || readOnly);
                            }
                        }
                    }
                }
            }
        }

        VariableBOESumByCLIN.CalculateTotal();
    };

    $('.checkbox').change(function () {
        VariableBOESumByCLIN.FormUpdated = true;
    });

    $(function () {
        VariableBOESumByCLIN.Initialize();
        VariableBOESumByCLIN.BindEvents();
        VariableBOESumByCLIN.RegisterForTriggeredEvents();

        $('#VariableBOESumByCLIN-CancelButton').click(function () {
            if (VariableBOESumByCLIN.FormUpdated) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function () {
                        VariableBOESumByCLIN.CloseDialog(VariableBOESumByCLIN.Dialog);
                    },
                    null);
            }
            else {
                VariableBOESumByCLIN.CloseDialog(VariableBOESumByCLIN.Dialog);
            }
        });
    });

</script>

<div id="VariableBOESumByCLIN" class="variable-boe-sum-by-clin display-none">
    <div class="container">
        <div name="createVariableItems">
            <div>
                Select the BOEs you would like to sum up as the value for the Variable. If a selected
                BOE <i>Total</i> changes while the BOE using this variable is <i>Awaiting Approval</i>
                or <i>Approved</i>, the Author will be required to review and resubmit the BOE.</div>
            <br />
            <div>
                Sort BOEs by
                <select name="VariableBOESumByCLIN-SortType">
                    <option value="0">WBS</option>
                    <option value="1" selected="selected">CLIN</option>
                </select>
            </div>
            <br />
            <div>
                If a CLIN is selected, any new BOEs created with the CLIN will also be included
                in its <i>Total</i>.
            </div>
        </div>
        <br />
        <div>
            <b>Include these resources:</b>
            <br />
            <% foreach (SumVariableResourceTypeModelView sumVariableResourceType in sumVariableResourceTypes)
                { %>
                    <input id="VariableBOESumByCLIN-ResourceTypeCheckbox-<%: sumVariableResourceType.SumVariableResourceTypeID %>" name="VariableBOESumByCLIN-ResourceTypeCheckbox" type="checkbox" title="<%: sumVariableResourceType.SumVariableResourceTypeName %>" value="<%: sumVariableResourceType.SumVariableResourceTypeID %>" />
                    <label for="VariableBOESumByCLIN-ResourceTypeCheckbox-<%: sumVariableResourceType.SumVariableResourceTypeID %>" ><%: sumVariableResourceType.SumVariableResourceTypeName %></label>
            <% } %>
        </div>
        <br />
        <div name="createVariableItems">
            <div name="CircularReferenceNote" class="display-none">
                <div class="circular-reference"></div> - BOEs and CLIN elements with a <div class="circular-reference"></div> cannot be selected because they will cause a circular reference.
                <div id="VariableBOESumByCLIN-CircularReferenceHelp" class="help-icon" style="margin-left: 0px;"
                    onclick="VariableBOESumByCLIN.ToggleHelp(this, 'left');">
                </div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="VariableBOESumByCLIN-CircularReferenceHelpDialog" class="help-dialog" style="width: 250px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Circular references are any references within a formula that depend upon the results
                        of that same formula. For example, a variable that refers to the total of the BOE
                        the user is in or a variable that refers to another variable which depends on the
                        total of the BOE the user is in.</div>
                </div>
            </div>
            <br />
        </div>
        <div style="max-height: 300px; overflow: auto;">
            <table id="VariableBOESumByCLIN-BOEToCopyGrid" class="variable-boe-sum-by-clin-grid readonly grid" style="width: 100%;">
                <thead>
                    <tr>
                        <th class="clin-wbs sum-by-clin-column">
                            CLIN &nbsp;&nbsp;&nbsp;WBS
                        </th>
                        <th class="boe-status sum-by-clin-column">
                            BOE Status
                        </th>
                        <th class="boe-total last-child sum-by-clin-column">
                            Total
                            <div id="VariableBOESumByCLIN-TotalHelp" class="help-icon" style="margin-left: 0px;" onclick="VariableBOESumByCLIN.ToggleHelp(this, 'left');"></div>
                            <!-- This comment is needed for the jquery animation to work in IE8... -->
                            <div id="VariableBOESumByCLIN-TotalHelpDialog" class="help-dialog" style="width: 280px;">
                                <div class="help-dialog-close"></div>
                                <div class="help-dialog-text">Total labor <%: ViewData["HoursLabel"]%> estimated for the summary WBS or BOE.</div>
                            </div>
                        </th>
                    </tr>
                </thead>
                <tbody>
                <% foreach (VariableBOESumByCLINModelView item in Model)
                   { %>
                        <tr pkid="<%: item.CLINID %>" itemtype="CLIN">
                            <td style="font-weight: bold;">
                                <div>
                                    <input name="CLINSelect" type="checkbox" class="checkbox" />
                                    <%: item.CLINTitle%>
                                </div>
                            </td>
                            <td>
                            </td>
                            <td name="LineTotal">
                                <b><%: item.CLINTotal%></b>
                            </td>
                        </tr>
                    <% foreach (VariableBOESumBOEElement nestedItem in item.BOEs)
                       { %>
                            <tr pkid="<%: nestedItem.BOEID %>" fkid="<%: item.CLINID %>" itemtype="BOE">
                                <td>
                                    <div style="padding-left: 40px;">
                                        <input name="BOESelect" type="checkbox" class="checkbox <% if(nestedItem.Disabled) { %>disabled<% } %>" />
                                        <a name="boeLink" boeid="<%: nestedItem.BOEID %>"><%: nestedItem.WBSTitle%></a>
                                    </div>
                                </td>
                                <td>
                                    <%: nestedItem.BOEStatus %>
                                </td>
                                <td name="LineTotal">
                                    <%: nestedItem.BOETotal%>
                                </td>
                            </tr>
                    <%  }
                   } %>
                </tbody>
            </table>
        </div>
        <br />
        <div class="float-right">
            <b>Current sum for selected BOEs: <span id="VariableBOESumByCLIN-TotalSum">0</span></b></div>
        <div name="createVariableItems">
            <br /><br />
            <div class="clear buttons">
                <button id="VariableBOESumByCLIN-CreateButton" class="ies-action disabled" type="button">Create Variable with selected BOEs</button>                
                <button id="VariableBOESumByCLIN-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
    </div>
</div>