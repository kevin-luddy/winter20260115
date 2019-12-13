<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<WorkspaceVersionModelView>>" %>


<script type="text/javascript">
    ManageBackupVersionsGridWidget = new Widget('ManageBackupVersionsGrid', <%= ViewData["READONLY"] %>);

    $(function() {
        ManageBackupVersionsGridWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { ManageBackupVersionsGridWidget.cleanDirty(); });

        $('#ManageBackupVersionsGridTable a[name=Restore]').click(function() {
            ManageBackupVersionsWidget.ConfirmRestore($(this).parents('tr').attr('pkid'));
        });

        $('#ManageBackupVersionsGridTable input[name=DeleteVersion]').click(function() {
            ManageBackupVersionsWidget.enableDelete();
        });

        $('#ManageBackupVersionsGridTable input#DeleteAllVersion').click(function() {
            if($('#ManageBackupVersionsGridTable input#DeleteAllVersion').is(':checked'))
            {
                $("div#ManageBackupVersionsContainer input[name=DeleteVersion]").prop("checked",true);
            }else{
                $("div#ManageBackupVersionsContainer input[name=DeleteVersion]").prop("checked", false);
            }
            ManageBackupVersionsWidget.enableDelete();
        });

    });
</script>

<div id="ManageBackupVersionsGrid">
    <table id="ManageBackupVersionsGridTable" class="grid readonly">
        <thead>
            <tr>
                <th class="delete-checkbox"><input type="checkbox" id="DeleteAllVersion" /></th>                   
                <th class="versionID">Version Name</th>
                <th class="dateCreated">Date Created</th>
                <th class="createdBy">Created By</th>
                <th class="restore last-child">Restore<div id="PageControls"></div></th>
            </tr>
        </thead>
        <tbody>
            <% foreach (WorkspaceVersionModelView item in Model) {%>
                <tr pkid="<%: item.VersionID %>" name="<%: item.VersionName%>" data-restore-version="<%: item.RestoreToWorkspaceState %>">
                    <td class="delete-checkbox">
                        <%if (ViewBag.IsSystemAdmin || !item.IsSystemBackup) { %>
                        <input type="checkbox" name="DeleteVersion" />
                        <% } %>
                    </td>
                    <td name="versionName">
                        <%:item.VersionName %>
                    </td>
                    <td class="" nowrap="nowrap">
                        <%: item.DateCreated %>
                    </td>
                    <td class="">
                        <%: item.CreatedByDisplayName %>
                    </td>
                    <td class="">
                        <a name="Restore">Restore</a>
                    </td>
                </tr>
            <% } %>
        </tbody>
    </table>
</div>