<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.BOECustomFieldsInUseGridModelView>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>

<% var readOnly = bool.Parse(ViewData["READONLY"] as string); %>

<script type="text/javascript">
    BOECustomFieldsGridWidget = new Widget("BOECustomFieldsGrid", '<%: ViewData["READONLY"] %>'.isTrue());

    BOECustomFieldsGridWidget.DeleteCustomField = function (customFieldID, updateDateLong) {

        var dataToSend = JSON.stringify({ CustomFieldID: customFieldID, UpdateDateLong: updateDateLong });

        $.ajax({
            type: "POST",
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                               '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                               '<%: WebConstants.ACTION_DELETE_CUSTOM_FIELD %>', ''),
            contentType: 'application/json; charset=utf-8',
            data: dataToSend,
            success: function (response) {
                if (response.Status) {
                    $(document).trigger("WS_LOAD_CUSTOM_FIELDS");
                } 
            }
        });
    };

    $(function () {

        if (BOECustomFieldsGridWidget.isReadOnly()) {
            $('#Add-BOECustomFieldsGrid').parent().addClass('display-none');
        }
        
        var isProjectMapWorkspace = '<%: ViewData["IsProjectMapWorkspace"] %>';

        if (isProjectMapWorkspace == 'True') {
            $('#Add-BOECustomFieldsGrid').addClass('display-none disabled');
            $('#Add-BOECustomFieldsGrid').attr('disabled', 'disabled').off('click');
            $('#Add-SikorskyCustomFields').addClass('display-none disabled');
            $('#Add-SikorskyCustomFields').attr('disabled', 'disabled').off('click');
        }

        if ('<%: ViewData["UsingTemplateBoe"] %>' == 'False') {
            $('.moq-type-table').addClass('display-none');
        }

        BOECustomFieldsGridWidget.Module = $('#BOECustomFieldsGrid');
        BOECustomFieldsGridWidget.ContentDiv = BOECustomFieldsGridWidget.Module.parent();

        BOECustomFieldsGridWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { BOECustomFieldsGridWidget.cleanDirty(); });

        createModule($(BOECustomFieldsGridWidget.Module));
        refreshModule($(BOECustomFieldsGridWidget.Module));

        BOECustomFieldsGridWidget.Module.find('tr a').click(function () {
            BOECustomFieldsGridWidget.data.boeCustomFieldID = $(this).parents('tr').attr('pkid');

            if (BOECustomFieldsGridWidget.data.boeCustomFieldID != undefined) {
                var actionURL = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD %>', '');

                var dataToSend = JSON.stringify(BOECustomFieldsGridWidget.data);

                $.ajax({
                    type: "POST",
                    url: actionURL,
                    contentType: 'application/json; charset=utf-8',
                    data: dataToSend,
                    dataType: 'html',
                    success: function (response) {
                        BOECustomFieldsGridWidget.ContentDiv.html(response);
                        $(document).trigger("CUSTOM_FIELDS_LOADED");
                    }
                });
            }
        });

        $('#Add-BOECustomFieldsGrid').click(function () {
            if (isProjectMapWorkspace == 'False') {
                var actionURL = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_DISPLAY_BOE_CUSTOM_FIELD %>', '');

                $.ajax({
                    type: "POST",
                    url: actionURL,
                    dataType: 'html',
                    success: function (response) {
                        BOECustomFieldsGridWidget.ContentDiv.html(response);
                        $(document).trigger("CUSTOM_FIELDS_LOADED");
                    }
                });
            }
            else {
                // Add button is hidden/disabled for Project Map, but preventing add just in case 
                Session.alertDialog("Cannot add Custom Field","Custom Fields cannot be added for a Project Map Workspace.");
            }
        });

        $('#Add-SikorskyCustomFields').click(function () {
            GenSession.commonDialog('Create Custom Fields', 'Select which Custom Fields to create.',
                [{
                    buttonClass: 'ies-action',
                    ButtonText: 'PROPRICER Standard Custom Fields',
                    ButtonName: 'propricer-button',
                    callbackMethod: function () { CreateCustomFields('<%: WebConstants.ACTION_CREATE_PROPRICER_CUSTOM_FIELDS %>'); }
                }, {
                    buttonClass: 'ies-action',
                    ButtonText: 'Sikorsky Custom Fields',
                    ButtonName: 'sikorsky-button',
                    callbackMethod: function () { CreateCustomFields('<%: WebConstants.ACTION_CREATE_SIKORSKY_CUSTOM_FIELDS %>'); }
                }, {
                    buttonClass: 'ies',
                    ButtonText: 'Cancel',
                    ButtonName: "cancel-button",
                    callbackMethod: null
                }], 600, null, null);
        });

        CreateCustomFields = function (action) {
            if (isProjectMapWorkspace == 'False') {
                var actionURL = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    action, '');
                ShowLoadingBox();

                $.ajax({
                    type: "POST",
                    url: actionURL,
                    dataType: 'html',
                    success: function (response) {
                        HideLoadingBox();
                        $(document).trigger("WS_LOAD_CUSTOM_FIELDS");
                    },
                    error: function () {
                        HideLoadingBox();
                    }
                });
            }
            else {
                // Add button is hidden/disabled for Project Map, but preventing add just in case 
                Session.alertDialog("Cannot add Custom Field", "Custom Fields cannot be added for a Project Map Workspace.");
            }
        };

        BOECustomFieldsGridWidget.Module.find('tbody div.delete').click(function () {
            var parentRow = $(this).parents('tr');
            var id = parentRow.attr('pkid');
            var updateDateLong = parentRow.attr('updateDateLong').toString();
            var inUse = parentRow.attr('inUse').isTrue();
            var fieldName = parentRow.attr('fieldName');

            if (inUse) {
                Session.confirmDialog(
                'Delete \'' + fieldName + '\'',
                'At least one BOE Author has supplied a value for this field. Deleting this field will delete the field for the selected levels. All values supplied by Authors for the field will also be deleted. <br/><br/>Are you sure you want to delete this field?',
                function () {
                    BOECustomFieldsGridWidget.DeleteCustomField(id, updateDateLong);
                },
                null);
            }
            else {
                Session.confirmDialog(
                'Delete \'' + fieldName + '\'',
                'Are you sure you want to delete this field?',
                function () {
                    BOECustomFieldsGridWidget.DeleteCustomField(id, updateDateLong);
                },
                null);
            }
        });
    });

</script>

<div id="BOECustomFieldsGrid" class="boe-custom-fields-grid module">
    <div class="module-header-data">BOE Custom Fields</div>
    <div class="module-content-data">
        <div class="form-row">Edit the properties for the Resource and Performing Orgs fields.  Create and edit custom BOE fields.</div>
        <div class="buttons">
            <button id="Add-BOECustomFieldsGrid" class="ies-action" type="button">+ Add custom field</button>
            <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) { %>
            <button id="Add-SikorskyCustomFields" class="ies-action float-right" style="background:linear-gradient(to bottom, #5a86d5, #b1caf6 1px, #6495ed)" type="button">+ Add standard custom fields</button>
            <% } %>
        </div>
        <table class="grid readonly">
            <thead>
                <th class="field-name">Field Name</th>
                <th class="boe">BOE</th>
                <th class="task">Task</th>
                <th class="labor-type">Resource Types</th>
                <th class="moq-type-table">MOQ Table</th>
                <th class="required">Required</th>
                <th class="open-ended">Open Ended</th>
                <th class="in-use-column">In Use</th>
                <th class="delete last-child"></th>
            </thead>
            <tbody>
                <tr>
                    <td><a id="ResourceLink" href="#Resources">Resource</a></td>
                    <td></td>
                    <td></td>
                    <td><div class="checkmark"></div></td>
                    <td class="moq-type-table"></td>
                    <td><div class="checkmark"></div></td>
                    <td></td>
                    <td></td>
                    <td></td>
                </tr>
                <tr>
                    <td><a id="PerformingOrgLink" href="#PerformingOrgs">Performing Organization</a></td>
                    <td></td>
                    <td></td>
                    <td><div class="checkmark"></div></td>
                    <td class="moq-type-table"></td>
                    <td><div class="checkmark"></div></td>
                    <td></td>
                    <td></td>
                    <td></td>
                </tr>
                <% foreach (BOECustomFieldsInUseGridModelView item in Model) {
                        Response.Write("<tr pkid=\"" + item.CustomFieldID + "\" inUse=\"" + item.inUse + "\" fieldName=\"" + item.FieldName + "\" updateDateLong=\"" + item.UpdateDateLong + "\">");
                        Response.Write("<td><a>" + item.FieldName + "</a></td>");
                        if (item.CustomFieldDisplayID == CustomFieldType.BoeDisplay)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }
                        if (item.CustomFieldDisplayID == CustomFieldType.TaskDisplay)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }
                        if (item.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }
                        if(item.CustomFieldDisplayID == CustomFieldType.MoqTypeTableDataDisplay)
                        {
                            Response.Write("<td class=\"moq-type-table\"><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td class=\"moq-type-table\"></td>");
                        }
                        if (item.isRequired)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }
                        if (item.isOpenEnded)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }
                        if (item.inUse)
                        {
                            Response.Write("<td><div class=\"checkmark\"></div></td>");
                        }
                        else
                        {
                            Response.Write("<td></td>");
                        }

                        // if readOnly do not show delete buttons
                        if (readOnly)
                        {
                            Response.Write("<td></td>");
                        }
                        else
                        {
                            Response.Write("<td class=\"delete\"><div class=\"delete\"></div></td>");
                        }

                        Response.Write("</tr>");
                    } %>
            </tbody>
        </table>
        <button id="Back-BOECustomFieldsGrid" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
    </div>
</div>