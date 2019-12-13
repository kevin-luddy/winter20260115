<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ICollection<GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView>>" %>

<script type="text/javascript">
    var BOEFormsGridWidget = new Widget('BOEFormsGrid', <%= ViewData["READONLY"] %>);

    // Updates grid data
    BOEFormsGridWidget.UpdateGridData = function (input) {
        $(document).trigger('SYNC_STATE_BOE_FORMS_DELETE_BUTTON');
    };

    BOEFormsGridWidget.GetRowData = function(rowElement) {
        return {
            BOEFormId: rowElement.attr('data-pkid'),
            BOEFormType: rowElement.attr('data-formType'),
            BOEFormName: rowElement.find('a[name="FormName"]').text(),
            UpdateDateLong: rowElement.attr('data-UpdateDateLong').toString(),
            UpdateType: <%: (int)IES.Common.UpdateType.Deleted %>
        };
    }

    BOEFormsGridWidget.CheckAllRecords = function () {   
        // check all 'Delete' buttons under the delete column and enable/disable the 'Delete' button.
        $('#BOEFormsGrid tbody input[name="DeleteBOEForms"]').prop('checked', $(this).prop('checked'));
        $(document).trigger('SYNC_STATE_BOE_FORMS_DELETE_BUTTON');
    }

    $(function () {
        BOEFormsGridWidget.afterDOMLoad();

        // event fires if any input box data changes
        $("#BOEFormsGrid tbody :input").change(function () { BOEFormsGridWidget.UpdateGridData($(this)) });

        // gets record data for every item that is checked for deletion, and then prompts user if theyre sure.
        BOEFormsGridWidget.registerForEvent('GET_DELETED_BOE_FORMS', function () {
            
            BOEFormsGridWidget.data.CheckedItems = new Array();
            $('#BOEFormsGrid').find('tr input[name=DeleteBOEForms]:checked').each(function() {
                BOEFormsGridWidget.data.CheckedItems.push(BOEFormsGridWidget.GetRowData($(this).closest('tr')) );
            });

            $(document).trigger('DELETE_BOE_FORMS', BOEFormsGridWidget.data); 
        });
        
        // click event for editing a item
        $('#BOEFormsGrid td.edit-boe-forms-link').click(function() {
            var $row = $(this).parents('tr');
            $(document).trigger('EDIT_BOE_FORMS', { id: $row.attr('data-pkid'), type: $row.attr('data-formType') }); 
        });

        SortableGrid('.manage-boe-forms-grid');

        refreshModule($('.manage-boe-forms-grid'));
    });
</script>

<div class="manage-boe-forms-grid">
    <table id="BOEFormsGrid" class="sortable grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input class="display-none" type="checkbox" id="DeleteAllBOEForms" /></th>                   
                <th class="sort name">Name</th>
                <th class="sort type">Type</th>
                <th class="sort cost">Cost</th>
                <th>Validation Status</th>
                <% 
                    bool isUsingTM = ViewBag.IsUsingTM;
                    if (isUsingTM)
                    { %>
                        <th class="sort type">T&amp;M Cost</th>
                        <th class="sort cost">Total Cost</th>
                <% }
                        %>
            </tr>
        </thead>
        <tbody>
            <% foreach (GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView item in Model) { %>
                <tr data-pkid="<%: item.BOEFormId %>" data-formType="<%: item.BOEFormType.ToString() %>" data-UpdateDateLong="<%: item.UpdateDateLong %>">
                    <%: Html.Hidden("BOEFormId", item.BOEFormId) %>
                    
                    <td class="delete-checkbox">
                        <input type="checkbox" name="DeleteBOEForms" />
                    </td>
                    <td class="name edit-boe-forms-link">
                        <a name="FormName"><%: item.BOEFormName %></a>
                    </td>
                    <td class="name edit-boe-forms-link">
                        <span name="FormType"><%: item.BOEFormType.GetDescription() %></span>
                    </td>
                    <td class="name edit-boe-forms-link">
                        <span name="Cost"><%: String.Format(Constants.MONEY_FORMATTING, item.TotalCost) %></span>
                    </td>
                    <td>
                        <% if (item.IsIncomplete)
                        {
                                %>
                        Incomplete data found
                        <% } %>
                    </td>
                <% if (isUsingTM)
                    { %>
                    <% if (item.HasValidTMRates)
                       { %>
                        <td class="name edit-boe-forms-link">
                            <span name="Cost"><%: String.Format(Constants.MONEY_FORMATTING, item.TMCost) %></span>
                        </td>
                        <td class="name edit-boe-forms-link">
                            <span name="Cost"><%: String.Format(Constants.MONEY_FORMATTING, (item.TotalCost + item.TMCost) ) %></span>
                        </td>
                    <% }
                       else
                       { %>
                        <td class="name edit-boe-forms-link" colspan="2">T&amp;M rates incomplete or unavailable</td>
                    <% }
                    } %>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>