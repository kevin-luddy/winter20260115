<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ExportToProPricerModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%
    JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    // Create Grid Widget
    var ManageSystemProPricerGridWidget = new GridWidget("ExportToProPricerGrid", "pkid", '<%= ViewData["READONLY"] %>'.isTrue());
    ManageSystemProPricerGridWidget.Module = {};
    
    ManageSystemProPricerGridWidget.Initialize = function () {
        ManageSystemProPricerGridWidget.Module = $('.export-to-propricer-grid').parents('.module');
        ManageSystemProPricerGridWidget.SetCopyFromFormatNameSelections();
    };

    ManageSystemProPricerGridWidget.BindEvents = function () {
        // Bind Select All Checkbox change event
        $('#ExportToProPricerGrid thead th.select-checkbox input.checkbox').change(function () {
            if ($(this).is(':checked')) {
                $('#ExportToProPricerGrid tbody input.checkbox').prop('checked', true);
            }
            else {
                $('#ExportToProPricerGrid tbody input.checkbox').prop('checked', false);
            }

            ManageSystemProPricerGridWidget.CheckForDeleteButtonEnabled();
        });

        // Bind individual row Checkbox change events
        $('#ExportToProPricerGrid tbody input.checkbox').change(function () {
            ManageSystemProPricerGridWidget.CheckForDeleteButtonEnabled();
        });

        $('#ExportToProPricerGrid tbody tr[pkid] td[name=Edit]').click(function () {
            $(document).trigger('EDIT_PROPRICER_EXPORT_FORMAT', $(this).parents('tr').get());
        });
    };

    // Trigger an event to tell the main UI which formats can be copied from
    ManageSystemProPricerGridWidget.SetCopyFromFormatNameSelections = function () {
        var availableFormatsJSON = '[<%= String.Join(",", Model.Select(x =>  "{ \"ID\" : " + x.ID + ", \"Name\" : \"" + x.Name + "\"}")) %>]';

        $(document).trigger('SET_PROPRICER_COPY_FROM_FORMATS', availableFormatsJSON);
    };

    ManageSystemProPricerGridWidget.CheckForDeleteButtonEnabled = function () {
        $(document).trigger('ENABLE_PROPRICER_DELETE_BUTTON', $('#ExportToProPricerGrid tbody input.checkbox:checked').length > 0);
    };

    $(function () {
        ManageSystemProPricerGridWidget.Initialize();
        ManageSystemProPricerGridWidget.BindEvents();
        ManageSystemProPricerGridWidget.refreshModule();
    });
</script>
<div>
    <gen-validation data-errors="errors"></gen-validation>
    <table id="ExportToProPricerGrid" class="export-to-propricer-grid readonly grid full-width">
        <thead>
            <tr>
                <th class="select-checkbox"><input type="checkbox" class="checkbox" id="SelectAllCheckbox" /></th>
                <th class="scope">
                    Scope
                    <div id="ExportToProPricerGrid-ScopeHelp" class="help-icon" style="margin-left: 0px;"
                        onclick="ManageSystemProPricerGridWidget.ToggleHelp(this);">
                    </div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="ExportToProPricerGrid-ScopeHelpDialog" class="help-dialog" style="width: 250px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Scope indicates if the defined ProPricer Export Format is for use only in the current Workspace or for the all Workspaces in the genBOE System.
                        </div>
                    </div>
                </th>
                <th class="format">Format</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var item in Model)
               { %>
               <tr pkid="<%: item.ID %>">
                    <td>
                        <input type="checkbox" class="checkbox" />
                        <input type="hidden" name="Name" value="<%: item.Name %>" />
                        <input type="hidden" name="Scope" value="<%: (int)item.Scope %>" />
                        <input type="hidden" name="OrderedTasks" value="[<%: String.Join(",", item.SystemOrderedTasks.Select(t => "\"" + t + "\"")) %>]" />
                        <input type="hidden" name="OrderedResources" value="[<%: String.Join(",", item.SystemOrderedResources.Select(t => "\"" + t + "\"")) %>]" />
                        <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                    </td>
                    <td><%: item.Scope.ToString() %></td>
                    <td name="Edit"><a><%: item.Name%></a></td>                
               </tr>
            <% } %>
        </tbody>
    </table>
</div>        