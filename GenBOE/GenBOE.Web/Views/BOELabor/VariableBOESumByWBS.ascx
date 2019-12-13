<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.VariableBOESumByWBSModelView>>" %>
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

    var VariableBOESumByWBS = new Widget("VariableBOESumByWBS");

    VariableBOESumByWBS.Dialog = {};
    VariableBOESumByWBS.EventData = {};

    VariableBOESumByWBS.Initialize = function () {
        VariableBOESumByWBS.Dialog.Element = $('#VariableBOESumByWBS');
        VariableBOESumByWBS.Dialog.Params = { title: "Select BOEs to Sum", modal: true, resizable: false, width: 680 };
        VariableBOESumByWBS.InitializeDialog(VariableBOESumByWBS.Dialog);
    };

    VariableBOESumByWBS.ResetDialog = function (dialogTitle) {
        $('select[name=VariableBOESumByWBS-SortType] option[value="0"]').prop('selected', true);
        VariableBOESumByWBS.ChangeDialogTitle(VariableBOESumByWBS.Dialog, dialogTitle);
        $('#VariableBOESumByWBS div[name=createVariableItems], #VariableBOESumByWBS div.create-item').show();
        $('#VariableBOESumByWBS table tbody input.checkbox').prop('checked', false).prop('disabled', false);
        $('#VariableBOESumByWBS-CreateButton').addClass('disabled').html('Create Variable with selected BOEs');
        $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]').prop('disabled', false).prop('checked', false);
    };

    VariableBOESumByWBS.ResetDialogReadOnly = function (dialogTitle) {
        $('select[name=VariableBOESumByWBS-SortType] option[value="0"]').prop('selected', true);

        if (dialogTitle != undefined) {
            VariableBOESumByWBS.ChangeDialogTitle(VariableBOESumByWBS.Dialog, dialogTitle);
        }
        else {
            VariableBOESumByWBS.ChangeDialogTitle(VariableBOESumByWBS.Dialog, 'BOEs Selected to Sum');
        }

        $('#VariableBOESumByWBS table tbody div.circular-reference').remove();

        $('#VariableBOESumByWBS div[name=createVariableItems], #VariableBOESumByWBS div.create-item').hide();

        $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]').prop('disabled', true);
    };

    VariableBOESumByWBS.SetDisabledElements = function () {
        $('#VariableBOESumByWBS table tbody tr td div.circular-reference').remove();

        var disabledCheckboxes = $('#VariableBOESumByWBS table tbody input.checkbox.disabled');

        if (disabledCheckboxes.length) {
            $('#VariableBOESumByWBS div[name=CircularReferenceNote]').removeClass('display-none');
        }
        else {
            $('#VariableBOESumByWBS div[name=CircularReferenceNote]').addClass('display-none');
        }

        disabledCheckboxes.each(function () {
            $(this).prop('disabled', true).after('<div class="circular-reference"></div>');

            var parentRow = $(this).parents('tr');

            var thisWBS = parentRow.attr('wbs');
            var thisLevel = parentRow.attr('level');
            
            var preceding = parentRow.prevAll('tr');

            preceding.each(function () {
                var selectedWBSRegExp = new RegExp('^' + $(this).attr('wbs'), 'i');

                if ($(this).attr('level') < thisLevel && thisWBS.match(selectedWBSRegExp) && !$(this).find('div.circular-reference').length) {
                    $(this).find('input.checkbox').prop('disabled', true).after('<div class="circular-reference"></div>');
                }
            });
        });
    };

    VariableBOESumByWBS.SetCreateButtonState = function () {
        if ($('#VariableBOESumByWBS-BOEToCopyGrid tbody input:checked').length > 0) {
            $('#VariableBOESumByWBS-CreateButton').removeClass('disabled');
        }
        else {
            $('#VariableBOESumByWBS-CreateButton').addClass('disabled');
        }
    };

    VariableBOESumByWBS.CalculateTotal = function () {
        var total = 0;

        var boeLineTotals = $('tbody input:checked', $('#VariableBOESumByWBS-BOEToCopyGrid')).parents('tr[itemtype="BOE"]').find('td[name=LineTotal]');

        boeLineTotals.each(function () {
            total += parseFloat($(this).text());
        });
        
        //set Total with decimal precision to correct floating point rounding errors, parse float to remove trailing zeros
        $('#VariableBOESumByWBS-TotalSum').html(parseFloat(total.toFixed(<%:decimalPrecision%>)));
    };

    VariableBOESumByWBS.GoToSelectedBOE = function (boeID) {
        // Get the selected BOE ID 
        window.open(CreatePostURL(
            '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
            '<%: WebConstants.CONTROLLER_BOE %>',
            '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>',
            'boe/' + boeID));
    };

    VariableBOESumByWBS.ChangeToSortByCLIN = function () {
        VariableBOESumByWBS.EventData.SortBOEBy = '<%:(int)VarSortBOEBy.CLIN%>';

        if (VariableBOESumByWBS.EventData.ValidBOEs != undefined) {
            VariableBOESumByWBS.EventData.BOEToSum = VariableBOESumByWBS.EventData.WBSToSum = VariableBOESumByWBS.EventData.CLINToSum = [];
            VariableBOESumByWBS.EventData.ResourceTypes = VariableBOESumByWBS.GatherResourceTypesToCalculate();

            $(document).trigger('VariableBOESumByCLIN_Open', VariableBOESumByWBS.EventData);
        }
        else {
            var selectionsExist = (VariableBOESumByWBS.EventData.BOEToSum.length > 0);

            $(document).trigger('VariableBOESumByCLIN_OpenNew', VariableBOESumByWBS.EventData);

            if (selectionsExist) {
                $('#VariableBOESumByCLIN-CreateButton').removeClass('disabled').html('Update Variable with selected BOEs');
            }
        }

        $(document).trigger('VariableBOESumByWBS_Close');
    };

    VariableBOESumByWBS.BindEvents = function () {
        // BIND Element events

        $('select[name=VariableBOESumByWBS-SortType]').change(function () {
            if ($(this).val() == 1) {
                if ($('#VariableBOESumByWBS-BOEToCopyGrid tbody input:checked').length) {
                    var that = $(this);
                    Session.confirmDialog(
                        "BOE Selections Will be Lost",
                        "Changing the sort view will clear all selections made.  Are you sure you want to sort by CLIN?",
                        function () {
                            VariableBOESumByWBS.ChangeToSortByCLIN();
                        },
                        function () {
                            that.val(0);
                        });
                }
                else {
                    VariableBOESumByWBS.ChangeToSortByCLIN();
                }
            }
        });

        $('#VariableBOESumByWBS-BOEToCopyGrid tbody input[name=WBSSelect]').change(function () {
            var isChecked = $(this).is(':checked');
            var selectedRow = $(this).parents('tr');
            var selectedLevel = selectedRow.attr('level');
            var selectedWBSRegExp = new RegExp('^' + selectedRow.attr('wbs'), 'i');
            var complete = false;

            var currentRow = selectedRow.next();

            while (currentRow.length && !complete) {
                if (currentRow.attr('level') > selectedLevel && currentRow.attr('wbs').match(selectedWBSRegExp)) {
                    if (isChecked) {
                        currentRow.find('input[name=BOESelect], input[name=WBSSelect]').prop('checked', true).prop('disabled', true);
                    }
                    else {
                        currentRow.find('input[name=BOESelect], input[name=WBSSelect]').prop('checked', false).prop('disabled', false);
                    }
                }
                else {
                    complete = true;
                }

                currentRow = currentRow.next('tr');
            }

            VariableBOESumByWBS.SetCreateButtonState();
            VariableBOESumByWBS.CalculateTotal();
        });

        $('#VariableBOESumByWBS-BOEToCopyGrid tbody input[name=BOESelect]').change(function () {
            VariableBOESumByWBS.SetCreateButtonState();
            VariableBOESumByWBS.CalculateTotal();
        });

        $('#VariableBOESumByWBS-BOEToCopyGrid input#selectAll').change(function () {
            if ($(this).is(':checked')) {
                $('input[name=BOESelect]:not([disabled]), input[name=WBSSelect]:not([disabled])').prop('checked', true);

                $('[itemtype="WBS"]').each(function () {
                    if ($(this).find('input[name=BOESelect], input[name=WBSSelect]').is(':checked')) {
                        $('[wbs="' + $(this).attr('wbs') + '"]:not([itemtype="WBS"])').find('input[name=BOESelect], input[name=WBSSelect]').prop('disabled', true);
                    }
                });
            }
            else {
                $('input[name=BOESelect], input[name=WBSSelect]').prop('checked', false).prop('disabled', false);
            }

            VariableBOESumByWBS.SetDisabledElements();
            VariableBOESumByWBS.CalculateTotal();
            VariableBOESumByWBS.SetCreateButtonState();
        });

        $('#VariableBOESumByWBS-BOEToCopyGrid tbody tr td a[name=boeLink]').click(function () {
            VariableBOESumByWBS.GoToSelectedBOE($(this).attr('boeID'));
        });

        VariableBOESumByWBS.registerForLiveEvent('click', '#VariableBOESumByWBS-CreateButton:not(.disabled)', function () {
            var eventData = {};
            eventData.pkid = VariableBOESumByWBS.EventData.pkid;
            eventData.Sum = parseFloat($('#VariableBOESumByWBS-TotalSum').text());
            eventData.WBSToSum = [];
            eventData.SortBOEBy = '<%:(int)VarSortBOEBy.WBS%>';

            $('#VariableBOESumByWBS-BOEToCopyGrid tbody input[name=WBSSelect]:checked:not(input[disabled])').each(function () {
                eventData.WBSToSum.push($(this).parents('tr').attr('pkid'));
            });

            eventData.BOEToSum = [];

            $('#VariableBOESumByWBS-BOEToCopyGrid tbody input[name=BOESelect]:checked:not(input[disabled])').each(function () {
                eventData.BOEToSum.push($(this).parents('tr').attr('pkid'));
            });

            eventData.ResourceTypes = [];

            $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]:checked').each(function () {
                eventData.ResourceTypes.push($(this).val());
            });

            VariableBOESumByWBS.CloseDialog(VariableBOESumByWBS.Dialog);
            
            $(document).trigger('VariableBOESumByWBS_Create', eventData);
        });

        $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]').change(function () {
            VariableBOESumByWBS.RefreshTable(undefined);
        });
    };

    VariableBOESumByWBS.RegisterForTriggeredEvents = function () {

        VariableBOESumByWBS.registerForEvent('VariableBOESumByWBS_OpenNew', function (event, eventData) {
            VariableBOESumByWBS.EventData = eventData;

            VariableBOESumByWBS.ResetDialog('Select BOEs to Sum');

            var tableBody = $('#VariableBOESumByWBS table tbody');

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

            $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]').prop('checked', true);

            VariableBOESumByWBS.SetDisabledElements();
            // this call is needed since the default call only gets default calculations. 
            VariableBOESumByWBS.RefreshTable(eventData);
            VariableBOESumByWBS.OpenDialogAfterInitialize(VariableBOESumByWBS.Dialog);
        });

        VariableBOESumByWBS.registerForEvent('VariableBOESumByWBS_Open', function (event, eventData) {
            VariableBOESumByWBS.EventData = eventData;

            VariableBOESumByWBS.ResetDialog('Select BOEs to Sum');

            var tableBody = $('#VariableBOESumByWBS table tbody');


            if (eventData.WorkspaceVariables != undefined) {
                if (eventData.ValidBOEs != undefined) {
                    $('tr[fkid]', tableBody).find('input.checkbox').addClass('disabled');

                    for (var ndx in eventData.ValidBOEs) {
                        $('tr[pkid=' + eventData.ValidBOEs[ndx] + ']', tableBody).find('input.checkbox').removeClass('disabled');
                    }
                }
                else {
                    $('input.checkbox', tableBody).removeClass('disabled');
                }
            }
 
            $('#VariableBOESumByWBS-CreateButton').html('Update Variable with selected BOEs');

            for (var ndx in eventData.BOEToSum) {
                $('tr[pkid=' + eventData.BOEToSum[ndx] + ']', tableBody).find('input[name=BOESelect]').prop('checked', true);
            }

            for (var ndx in eventData.WBSToSum) {
                var thisRow = $('tr[pkid=' + eventData.WBSToSum[ndx] + ']', tableBody);
                thisRow.find('input[name=WBSSelect]').prop('checked', true);
                
                
                var selectedLevel = thisRow.attr('level');
                var selectedWBSRegExp = new RegExp('^' + thisRow.attr('wbs'), 'i');
                var complete = false;

                var currentRow = thisRow.next();

                while (currentRow.length && !complete) {
                    if (currentRow.attr('level') > selectedLevel && currentRow.attr('wbs').match(selectedWBSRegExp)) {
                        currentRow.find('input[name=BOESelect], input[name=WBSSelect]').prop('checked', true).prop('disabled', true);                       
                    }
                    else {
                        complete = true;
                    }

                    currentRow = currentRow.next('tr');
                }
            }

            // opening based on the select link will default to selecting all resource types
            if (eventData.SelectBOELink) {
                $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]').prop('checked', true);
            } else {
                // opening based on the value link will select the saved resource types
                for (var ndx in eventData.ResourceTypes) {
                    $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox][value="' + eventData.ResourceTypes[ndx] + '"]').prop('checked', true);
                }
            }

            if (eventData.BOEToSum.length || eventData.WBSToSum.length) {
                VariableBOESumByWBS.SetCreateButtonState();
            }

            VariableBOESumByWBS.SetDisabledElements();
            // this call is needed since the default call only gets default calculations. this is called with the user's specific data inputs
            VariableBOESumByWBS.RefreshTable(eventData);
            VariableBOESumByWBS.OpenDialogAfterInitialize(VariableBOESumByWBS.Dialog);

        });

        VariableBOESumByWBS.registerForEvent('VariableBOESumByWBS_OpenReadOnly', function (event, eventData) {
            VariableBOESumByWBS.EventData = eventData;

            VariableBOESumByWBS.ResetDialog('Select BOEs to Sum');
            VariableBOESumByWBS.ResetDialogReadOnly(eventData.dialogTitle);

            var tableBody = $('#VariableBOESumByWBS table tbody');

            $('input.checkbox', tableBody).prop('disabled', true);

            for (var ndx in eventData.BOEToSum) {
                $('tr[pkid=' + eventData.BOEToSum[ndx] + ']', tableBody).find('input[name=BOESelect]').prop('checked', true);
            }

            for (var ndx in eventData.WBSToSum) {
                var thisRow = $('tr[pkid=' + eventData.WBSToSum[ndx] + ']', tableBody);
                thisRow.find('input[name=WBSSelect]').prop('checked', true);
                var selectedLevel = thisRow.attr('level');
                var selectedWBSRegExp = new RegExp('^' + thisRow.attr('wbs'), 'i');
                var complete = false;

                var currentRow = thisRow.next();

                while (currentRow.length && !complete) {
                    if (currentRow.attr('level') > selectedLevel && currentRow.attr('wbs').match(selectedWBSRegExp)) {
                        currentRow.find('input[name=BOESelect], input[name=WBSSelect]').prop('checked', true).prop('disabled', true);
                    }
                    else {
                        complete = true;
                    }

                    currentRow = currentRow.next('tr');
                }
            }
            for (var ndx in eventData.ResourceTypes) {
                $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox][value="' + eventData.ResourceTypes[ndx] + '"]').prop('checked', true);
            }

            $('input.checkbox', tableBody).prop('disabled', true);

            // this call is needed since the default call only gets default calculations. this is called with the user's specific data inputs
            VariableBOESumByWBS.RefreshTable(eventData);
            VariableBOESumByWBS.OpenDialogAfterInitialize(VariableBOESumByWBS.Dialog);
        });

        VariableBOESumByWBS.registerForEvent('VariableBOESumByWBS_Close', function () {
            VariableBOESumByWBS.CloseDialog(VariableBOESumByWBS.Dialog);
        });
    };

    VariableBOESumByWBS.RefreshTable = function (eventData) {

        var actionURL = CreatePostURL(
                        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE_LABOR %>',
                        '<%: WebConstants.ACTION_REFRESH_VARIABLE_BOE_SUM_BY_WBS %>',
                        '');

        var dataToSend = VariableBOESumByWBS.GatherResourceTypesToCalculate();
        dataToSend = JSON.stringify(dataToSend);

        ShowLoadingBox();

        VariableBOESumByWBS.ajaxRequest({
            type: "POST",
            url: actionURL,
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            dataType: 'json',
            success: function (response) {
                VariableBOESumByWBS.UpdateTotals(response, eventData);
                HideLoadingBox();
            },
            error: function () {
                HideLoadingBox();
            }
        });

    };

    VariableBOESumByWBS.GatherResourceTypesToCalculate = function () {
        var toReturn = [];

        var resources = $('input[name=VariableBOESumByWBS-ResourceTypeCheckbox]:checked');
        resources.each(function () {
            toReturn.push($(this).val());
        });
        return toReturn;
    };

    VariableBOESumByWBS.UpdateTotals = function (response, eventData) {
        var selectedBOEs;
        var selectedWBSs;
        var readOnly;

        if (eventData === undefined) {
            selectedBOEs = [];
            selectedWBSs = [];
            readOnly = true;
        } else {
            selectedBOEs = Helper.undefinedOrNull(eventData.BOEToSum, []);
            selectedWBSs = Helper.undefinedOrNull(eventData.WBSToSum, []);
            readOnly = eventData.ReadOnly;
        }

        var tableBody = $('#VariableBOESumByWBS-BOEToCopyGrid').find('tbody');

        for (var wbsIndex in response) {
            if (response[wbsIndex].WBSID > 0) {
                var currentWBSID = response[wbsIndex].WBSID;
                var isSelectedWBS = Helper.ArrayContains(currentWBSID, selectedWBSs);

                var wbsRow = $('tr[pkid=' + currentWBSID + ']:not([fkid])', tableBody);

                if (wbsRow != undefined) {
                    var wbsTotal = wbsRow.children('td[name=LineTotal]');

                    if (wbsTotal != undefined) {
                        wbsTotal.html('<b>' + response[wbsIndex].WBSTotal + '</b>');
                    }

                    if (isSelectedWBS) {
                        wbsRow.find(':checkbox').prop("checked", true);
                    }
                }

                if (response[wbsIndex].BOEs != undefined) {
                    for (var boeIndex in response[wbsIndex].BOEs) {
                        var currentBOEID = response[wbsIndex].BOEs[boeIndex].BOEID;

                        var boeRow = $('tr[pkid=' + currentBOEID + ']', tableBody);

                        if (boeRow != undefined) {
                            var boeTotal = boeRow.children('td[name=LineTotal]');

                            if (boeTotal != undefined) {
                                boeTotal.html(response[wbsIndex].BOEs[boeIndex].BOETotal);
                            }

                            if (isSelectedWBS || Helper.ArrayContains(currentBOEID, selectedBOEs)) {
                                boeRow.find(':checkbox').prop("checked", true).prop("disabled", isSelectedWBS || readOnly);
                            }
                        }

                    }
                }
            }
            else {

                // it's possible the WBS ID is negative because it's a level 1 WBS
                // in that case, there could be BOEs associated with it that needs to have it's total updated
                if (response[wbsIndex].BOEs != undefined) {
                    for (var boeIndex in response[wbsIndex].BOEs) {
                        var boeRow = $('tr[pkid=' + response[wbsIndex].BOEs[boeIndex].BOEID + '][fkid]', tableBody);

                        if (boeRow != undefined) {
                            var boeTotal = boeRow.children('td[name=LineTotal]');

                            if (boeTotal != undefined) {
                               boeTotal.html(response[wbsIndex].BOEs[boeIndex].BOETotal);
                            }
                        }
                    }
                 
                }
            }
        }

        VariableBOESumByWBS.CalculateTotal();
    };

    $('.checkbox').change(function () {
        VariableBOESumByWBS.FormUpdated = true;
    });

    $(function () {
        VariableBOESumByWBS.Initialize();
        VariableBOESumByWBS.BindEvents();
        VariableBOESumByWBS.RegisterForTriggeredEvents();

        $('#VariableBOESumByWBS-CancelButton').click(function () {
            if (VariableBOESumByWBS.FormUpdated) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function () {
                        VariableBOESumByWBS.CloseDialog(VariableBOESumByWBS.Dialog);
                    },
                    null);
            }
            else {
                VariableBOESumByWBS.CloseDialog(VariableBOESumByWBS.Dialog);
            }
        });
    });

</script>

<div id="VariableBOESumByWBS" class="variable-boe-sum-by-wbs display-none">
    <div class="container">
        <div name="createVariableItems">
            <div>
                Select the BOEs you would like to sum up as the value for the Variable. If a selected
                BOE <i>Total</i> changes while the BOE using this variable is <i>Awaiting Approval</i>
                or <i>Approved</i>, the Author will be required to review and resubmit the BOE.</div>
            <br />
            <div>
                Sort BOEs by
                <select name="VariableBOESumByWBS-SortType">
                    <option value="0" selected="selected">WBS</option>
                    <option value="1">CLIN</option>
                </select>
            </div>
        </div>
        <br />
        <div>
            <b>Include these resources:</b>
            <br />
            <% foreach (SumVariableResourceTypeModelView sumVariableResourceType in sumVariableResourceTypes)
                { %>
                    <input id="VariableBOESumByWBS-ResourceTypeCheckbox-<%: sumVariableResourceType.SumVariableResourceTypeID %>" name="VariableBOESumByWBS-ResourceTypeCheckbox" type="checkbox" title="<%: sumVariableResourceType.SumVariableResourceTypeName %>" value="<%: sumVariableResourceType.SumVariableResourceTypeID %>" />
                    <label for="VariableBOESumByWBS-ResourceTypeCheckbox-<%: sumVariableResourceType.SumVariableResourceTypeID %>" ><%: sumVariableResourceType.SumVariableResourceTypeName %></label>
            <% } %>
        </div>
        <br />
        <div name="createVariableItems">
            <div>
                If a summary WBS is selected, any new BOEs created under the summary WBS will also
                be included in its <i>Total</i>.
            </div>
            <br />
            <div name="CircularReferenceNote" class="display-none">
                <div class="circular-reference"></div> - BOEs and Summary WBS elements with a <div class="circular-reference"></div> cannot be selected because they will cause a circular reference.
                <div id="VariableBOESumByWBS-CircularReferenceHelp" class="help-icon" style="margin-left: 0px;"
                    onclick="VariableBOESumByWBS.ToggleHelp(this, 'left');">
                </div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="VariableBOESumByWBS-CircularReferenceHelpDialog" class="help-dialog" style="width: 250px;">
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
            <table id="VariableBOESumByWBS-BOEToCopyGrid" class="variable-boe-sum-by-wbs-grid grid" style="width: 100%;">
                <thead>
                    <tr>
                        <th class="wbs sum-by-wbs-column">
                            <input id="selectAll" type="checkbox" class="checkbox" />
                            WBS
                        </th>
                        <th class="clin sum-by-wbs-column">
                            CLIN
                        </th>
                        <th class="boe-status sum-by-wbs-column">
                            BOE Status
                        </th>
                        <th class="boe-total last-child sum-by-wbs-column">
                            Total
                            <div id="VariableBOESumByWBS-TotalHelp" class="help-icon" style="margin-left: 0px;" onclick="VariableBOESumByWBS.ToggleHelp(this, 'left');"></div>
                            <!-- This comment is needed for the jquery animation to work in IE8... -->
                            <div id="VariableBOESumByWBS-TotalHelpDialog" class="help-dialog" style="width: 250px;">
                                <div class="help-dialog-close"></div>
                                <div class="help-dialog-text">Total labor <%: ViewData["HoursLabel"]%> estimated for the CLIN or BOE.</div>
                            </div>
                        </th>
                    </tr>
                </thead>
                <tbody>                
                <% foreach (VariableBOESumByWBSModelView item in Model)
                   {
                       if (item.WBSID >= 0 && (item.BOEs.Any()))
                       { %>
                        <tr pkid="<%: item.WBSID %>" level="<%: item.WBSLevel %>" wbs="<%: item.WBSNumber%>" itemtype="WBS">
                            <td>
                                <div style="font-weight: bold; padding-left: <%: item.WBSLevel * 10 %>px;">
                                    <input name="WBSSelect" type="checkbox" class="checkbox" />
                                    <%: item.WBSNumber%> <%: item.WBSName%>
                                </div>
                            </td>
                            <td>
                            </td>
                            <td>
                            </td>
                            <td name="LineTotal">
                                <b><%: item.WBSTotal%></b>
                            </td>
                        </tr>
                    <% }
                       foreach (VariableBOESumBOEElement nestedItem in item.BOEs)
                       { %>
                            <tr pkid="<%: nestedItem.BOEID %>" fkid="<%: item.WBSID %>" level="<%: item.WBSLevel + 1 %>" wbs="<%: item.WBSNumber%>" itemtype="BOE">
                                <td class="text">
                                    <div style="padding-left: <%: (item.WBSLevel + 1) * 10 %>px;">
                                        <input name="BOESelect" type="checkbox" class="checkbox <% if(nestedItem.Disabled) { %>disabled<% } %>" />
                                        <a name="boeLink" boeid="<%: nestedItem.BOEID %>"><%: nestedItem.WBSTitle%></a>
                                    </div>
                                </td>
                                <td class="text">
                                    <a name="boeLink" boeid="<%: nestedItem.BOEID %>"><%: nestedItem.CLINTitle%></a>
                                </td>
                                <td class="text">
                                    <%: nestedItem.BOEStatus %>
                                </td>
                                <td class="text" name="LineTotal">
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
            <b>Current sum for selected BOEs: <span id="VariableBOESumByWBS-TotalSum">0</span></b></div>
        <div name="createVariableItems">
            <br /><br />
            <div class="clear buttons">
                <button id="VariableBOESumByWBS-CreateButton" class="ies-action disabled" type="button">Create Variable with selected BOEs</button>
                <button id="VariableBOESumByWBS-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
    </div>
</div>
