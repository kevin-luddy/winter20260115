<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOEMaterialGridModelView>" %>

<script type="text/javascript">

    var BOEMaterialWidget = new GridWidget("BOEMaterial", '', '<%= ViewData["READONLY"] %>'.isTrue());

    BOEMaterialWidget.expandMaterialElement = function (MaterialID) {
        window.location.hash = 'Material/material/' + MaterialID;
    }

    BOEMaterialWidget.removeDeletedItems = function (input) {

        $(input).parents('tr').fadeOut(400, function () {
            $(input).parents('tr').remove();
            BOEMaterialWidget.checkAddNewButton();
        });

    }

    BOEMaterialWidget.removeAllDeletedItems = function (input) {
        BOEMaterialWidget.deleteAllRecords(input);
        BOEMaterialWidget.checkAddNewButton();
    }

    // check the add new button to see if there is 1 row in the grid (or more)
    // if so we don't want to allow a click to occur to add a new one
    BOEMaterialWidget.checkAddNewButton = function () {
        if ($('#BOEMaterialGrid >tbody >tr').length >= 1) {
        $('#boeMaterial-AddMaterialElement').addClass('disabled');
        $('#boeMaterial-AddMaterialElement').unbind('click', BOEMaterialWidget.addNewMaterialElement);
        } else {
        $('#boeMaterial-AddMaterialElement').removeClass('disabled');
        $('#boeMaterial-AddMaterialElement').unbind('click', BOEMaterialWidget.addNewMaterialElement).bind('click', BOEMaterialWidget.addNewMaterialElement);
        }
    }

    BOEMaterialWidget.addNewMaterialElement = function () {
        window.location.hash = 'Material/material/';
    }


    $(function () {
        BOEMaterialWidget.afterDOMLoad();
        $("#BOEMaterialGridBody .delete").click(function () {
            var input = this;
            Session.confirmDialog("Delete Material Element", "Are you sure you want to delete the selected Material Element?", function () {
                deletedTask = {};
                deletedTask.MaterialID = $(input).parents('tr').find('.hidden-material-id').val();

                BOEMaterialWidget.data = deletedTask;

                var dataToSend = JSON.stringify(BOEMaterialWidget.data);

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                '<%:WebConstants.CONTROLLER_BOE_MATERIAL%>',
                                '<%:WebConstants.ACTION_DELETE_BOE_MATERIAL%>', 'boe/<%: ViewData["BOEID"] %>'),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) { BOEMaterialWidget.removeDeletedItems(input); }
                });
            });
        });

        $("#BOEMaterialGridHeader .delete").click(function () {
            var input = this;
            Session.confirmDialog("Delete All Materials", "Are you sure you want to delete all Material Elements?", function () {
                $.ajax({
                    type: 'DELETE',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                                '<%:WebConstants.CONTROLLER_BOE_MATERIAL%>',
                                '<%:WebConstants.ACTION_DELETE_ALL_MATERIAL_TYPES%>', 'boe/<%: ViewData["BOEID"] %>'),
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) { BOEMaterialWidget.removeAllDeletedItems(input); }
                });
            });
        });

        BOEMaterialWidget.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { BOEMaterialWidget.cleanDirty(); });

        if (BOEMaterialWidget.isReadOnly()) {
            $('#BOEMaterialGrid .delete').addClass('display-none');
            $('#BOEMaterialGrid .deleteColumn').addClass('display-none');
        }

        createModule($('.boe-Material.module'));
        refreshModule($('.boe-Material.module'));

        BOEMaterialWidget.checkAddNewButton();
    });
</script>

<div class="boe-Material module inner-collapsible">
    <div class="module-header-data"></div>
    <div class="module-content-data">
        <table id="BOEMaterialGrid" class="grid readonly">
            <thead id="BOEMaterialGridHeader">
                <tr>
                    <th>Task ID</th>
                    <th>Task Title</th>
                    <th>Task Start</th>
                    <th>Task End</th>
                    <th>Total Costs</th>
                    <th class="delete last-child"><div class="delete"></div></th>
                </tr>
            </thead>
            <tbody id="BOEMaterialGridBody">
                <% foreach (var item in Model.TaskElements) { %>  
                <tr>
                    
                    <td onclick="BOEMaterialWidget.expandMaterialElement('<%= item.MaterialID %>')">
                      <a><%: Html.DisplayFor(model => item.TaskID) %></a>
                         <%: Html.HiddenFor(model => item.MaterialID, new { @class="hidden-material-id" }) %>
                         <%: Html.HiddenFor(model => item.UpdateDateLong, new { @class = "hidden-update-date" })%>
                    </td>
 
                    <td onclick="BOEMaterialWidget.expandMaterialElement('<%= item.MaterialID %>')">
                      <a><%: Html.DisplayFor(model => item.TaskTitle) %></a>
                    </td>
                     <td><%: Html.DisplayFor(model => item.StartDate) %></td>
                      <td><%: Html.DisplayFor(model => item.EndDate) %></td>
                     <td><%: Html.DisplayFor(model => item.TotalCost) %></td>
          
                     <td class="deleteColumn">
                        <input type="hidden" value="<%:item.Deleted%>" name="Deleted" />
                        <div class="delete DeleteButton" />
                    </td>
                </tr>
                <% } %>

            </tbody>
        </table>
        <div class="buttons">
            <button id="boeMaterial-AddMaterialElement" name="add-task-element" class="ies-action" type="button">+ Add task element</button>
        </div>   

    </div>
</div>
