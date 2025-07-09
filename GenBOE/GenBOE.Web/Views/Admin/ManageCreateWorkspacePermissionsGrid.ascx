<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.PermissionsGridModelView>>" %>

<script type="text/javascript">
    // use widget
    var ManageCreateWorkspacePermissionsWidget = new GridWidget("ManageCreateWorkspacePermissions", "PermissionsID");

    ManageCreateWorkspacePermissionsWidget.deleteUserPermissions = function (UserID) {
        var dataToSend = { "inEntityId": UserID,
            "inType": "User"
        };

        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                '<%:WebConstants.ACTION_CHECK_IS_USER_WILL_LOSE_THEIR_CREATE_WORKSPACE_PERMISSIONS_ACCESS%>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (result) {
                if (!result.Status) {
                    Session.confirmDialog("Delete User Confirmation", "Are you sure you want to delete this user/group?", function () {
                        ManageCreateWorkspacePermissionsWidget._deleteUser(UserID);
                    }, null);
                }
                else {
                    Session.confirmDialog("Please Confirm Action", "Performing this action will remove your abilty to create workspaces. You will no longer be able to create any new workspaces.  Are you sure you want to remove this permission?", function () {
                        ManageCreateWorkspacePermissionsWidget._deleteUser(UserID);
                    }, function () {
                        $("#Permissions-SaveEdit").removeClass("display-none");
                        $("#Permissions-LoaderEdit").addClass("display-none");
                    });
                }
            },
            error: function () {
                $("#Permissions-SaveEdit").removeClass("display-none");
                $("#Permissions-LoaderEdit").addClass("display-none");
            }
        });
    };

    ManageCreateWorkspacePermissionsWidget._deleteUser = function (UserID) {
        ManageCreateWorkspacePermissionsWidget.closeDeletePermissionsError();

        dataToSend = JSON.stringify({ "inUserID": UserID });
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                        '<%:WebConstants.ACTION_DELETE_USER_CREATE_WORKSPACE_PERMISSIONS%>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (results) {
                if (results.Status) {
                    $(document).trigger("SA_LOAD_CREATE_WORKSPACE_PERMISSIONS");
                }
                else {
                    ManageCreateWorkspacePermissionsWidget.setDeletePermissionsError(results.Message);
                    ManageCreateWorkspacePermissionsWidget.displayDeletePermissionsError();
                }
            }
        });
    };

    ManageCreateWorkspacePermissionsWidget.CloseSaveNewPermissionsDialog = function () {
        $("#AddPermissions input[name=EntityIdString]").val("");
        $('#AddPermissions').dialog('close');
    }

    ManageCreateWorkspacePermissionsWidget.setDeletePermissionsError = function (message) {
        $('#ManageCreateWorkspacePermissions-ErrorText').html(message);
    };
     
    ManageCreateWorkspacePermissionsWidget.displayDeletePermissionsError = function () {
        $('#ManageCreateWorkspacePermissions-Error').show();
        refreshModule($('.ManageCreateWorkspacePermissionsWidget-grid').parents('.module'));
    };

    ManageCreateWorkspacePermissionsWidget.closeDeletePermissionsError = function () {
        $('#ManageCreateWorkspacePermissions-Error').hide();
        refreshModule($('.ManageCreateWorkspacePermissionsWidget-grid').parents('.module'));
    };

    ManageCreateWorkspacePermissionsWidget.setAddGroupIssues = function (message) {
        $('#AddGroupIssues #ManageCreateWorkspacePermissions-MessageText').html(message);
    };

    ManageCreateWorkspacePermissionsWidget.displayAddGroupIssues = function () {
        $("#AddGroupIssues").dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Group Permission Errors', close: ManageCreateWorkspacePermissionsWidget.closeAddGroupIssues });
    };

    ManageCreateWorkspacePermissionsWidget.closeAddGroupIssues = function () {
        location.reload();
    };

    ManageCreateWorkspacePermissionsWidget.displayAddPermissionsDialog = function () {
        $("#AddPermissions").dialog({ width: 700, modal: true, resizable: false, draggable: true, title: 'Add User/Group' });
        var forms = $('[name=IrisForm]');
        if (forms.length > 1) {
            $(forms[0]).remove();
        }
    };

    ManageCreateWorkspacePermissionsWidget.LookupUserToPermission = function(){
        ActiveDirectorySearchDialog(function (results) {
            var selectedAccountName = results.AccountName;
            if (selectedAccountName != '') {
                var users = document.getElementById("EntityIdString").value;

                if (users == "") {
                    users = users.concat(selectedAccountName);
                    document.getElementById("EntityIdString").value = users;
                }
                else {
                    var users = users.concat(";");
                    users = users.concat(selectedAccountName);
                    document.getElementById("EntityIdString").value = users;
                }
            }
        });
    };

    ManageCreateWorkspacePermissionsWidget.SaveNewPermissions = function () {

        ManageCreateWorkspacePermissionsWidget.clearValidationBox($("#AddPermissionsForm ul.validation-box"));

        $("#Permissions-SaveNew").addClass("display-none");
        $("#Permissions-LoaderNew").removeClass("display-none");

        var Roles = [];
        var Users = $("#AddPermissions input[name=EntityIdString]").val().replace(/ /gi, "").replace(/;+/gi, ";").replace(/^;|;$/gi, "").split(";");

        var dataToSend = { "Roles": Roles,
            "EntityIds": Users
        }
        dataToSend = JSON.stringify(dataToSend);

        ManageCreateWorkspacePermissionsWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                            '<%:WebConstants.ACTION_SAVE_NEW_CREATE_WORKSPACE_PERMISSIONS%>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response) {

                ManageCreateWorkspacePermissionsWidget.CloseSaveNewPermissionsDialog();

                if (response.Status) {
                    if (response.Messages.toString() != "") {
                        var messages = response.Messages.toString();
                        messages = ' <ul>    <li>' + messages.replace("[", "<li>").replace(/"/g, '').replace("]", "").replace(/!,/g, '</li><li>').replace('!', '</li>') + '</ul';
                        ManageCreateWorkspacePermissionsWidget.setAddGroupIssues(messages);
                        ManageCreateWorkspacePermissionsWidget.displayAddGroupIssues();
                    }
                    else {
                        location.reload();
                    }
                }
                else {
                    Session.alertDialog('Cannot Add Group', 'The group does not contain any users. Only groups with at least one user can be added.', function () {
                        $("#Permissions-SaveNew").removeClass("display-none");
                        $("#Permissions-LoaderNew").addClass("display-none");
                    });

                }
            },
            error: function () {

                $("#Permissions-SaveNew").removeClass("display-none");
                $("#Permissions-LoaderNew").addClass("display-none");
            }
        }, $('#Permissions-SaveNew'));
    };

    $(function () {
        ManageCreateWorkspacePermissionsWidget.afterDOMLoad();
        SortableGrid('ManageCreateWorkspacePermissionsWidget-grid');
        FilterableGrid('ManageCreateWorkspacePermissionsWidget-grid');

        ManageCreateWorkspacePermissionsWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageCreateWorkspacePermissionsWidget.cleanDirty(); });

        $.each($('#ManageCreateWorkspacePermissions td :input'), function () {
            $(this).change(function () {
                ManageCreateWorkspacePermissionsWidget.updateData($(this));
            });
        });

        $('#ManageCreateWorkspacePermissions a[name=ShowHideGroup]').click(function (event) {
            var row = $(this).parents('tr');
            var showHide = row.find("a[name=ShowHideGroup]").text();
            if (showHide == 'View Users') {
                var row = $(this).parents('tr');
                var dataToSend = {};
                dataToSend.groupName = row.find('input[name="GroupNameValue"]').val();
                dataToSend = JSON.stringify(dataToSend);

                ManageCreateWorkspacePermissionsWidget.ajaxRequest({
                    type: 'POST',
                    url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>',
                                '<%:WebConstants.ACTION_GET_GROUP_MEMBERS%>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (response) {
                        var html = "";
                        $.each(response, function (i, item) {
                            html += '<div>' + item + '</div>';
                        });
                        row.find('div[name="GroupUsersArea"]').html(html);
                        row.find("a[name=ShowHideGroup]").text('Hide Users');
                        row.find('div[name="GroupUsersArea"]').show();
                    }
                });

            } else {
                row.find("a[name=ShowHideGroup]").text('View Users');
                row.find('div[name="GroupUsersArea"]').hide();
            }

       });

    });

    Helper.preventFormPostingFromEnterKey('AddPermissionsForm');
</script>


 <div class="create-workspace-permissions section">
        <div class="title">Create Workspace Permissions</div>
        <div class="data">
            
 <div class="ManageCreateWorkspacePermissionsWidget-grid">
    <div id="ManageCreateWorkspacePermissions-Error" class="validation-box" style="width: 826px; margin-bottom: 10px; display: none;">
        <div>
            <div id="ManageCreateWorkspacePermissions-ErrorText">
            </div>
            <div id="ManageCreateWorkspacePermissions-ErrorText-Users">
            </div>
        </div>
        <div class="clear"></div>
    </div>
   
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ManageCreateWorkspacePermissionsForm" }))
       { %>
     <ul class="validation-box"> </ul>
     <div class="buttons">
        <button onclick="ManageCreateWorkspacePermissionsWidget.displayAddPermissionsDialog()" class="ies-action" name="add-permissions-button" type="button">+ Add user/group</button>      
    </div>
    <table id="ManageCreateWorkspacePermissions" class="sortable filterable grid readonly" style="width: 100%;">
        <thead>
            <tr>
                <th class="sort filter" filter="GroupName">Active Directory Group</th>
                <th class="sort filter" filter="User">User(s)</th>
                <th class="delete last-child"></th>
            </tr>
        </thead>
        <tbody>
      <%
        if (Model.Count() == 0)
        {
            Response.Write("<TR class='emptyrow' ><TD colspan='2'>No Records</td></tr>");
        }
        else
        {
            foreach (var item in Model)
            {
                %>

                <tr>
                    <td>
                        <div name="GroupName">
                            <%if(item.isGroup) { %> 
                            <%:item.Users.First().DisplayName %> <% } %>
                        </div>
                        <input type="hidden" name="GroupNameValue" value="<%: item.Users.First().DisplayName %>" />
                    </td>                  
                    <td>
                        <div name="User">
                            <%if(!item.isGroup) { %> 
                            <%:item.Users.First().DisplayName %> 
                            <% }else
                               {%>
                                <a name="ShowHideGroup">View Users</a>                                
                            <%}%>
                            <div name="GroupUsersArea" type="hidden"></div>
                        </div>
                     <td><div class="delete" onclick="ManageCreateWorkspacePermissionsWidget.deleteUserPermissions(<%:item.Users.First().UserID%>)"></div></td>
                </tr>
        <%
             }
          }
       } %>
        </tbody>
    </table>
</div>
  </div>
    </div>
   

    <div style="display:none; width: auto; min-height: 107px; height: auto;" id="AddPermissions" class="ui-dialog-content ui-widget-content">
        <% Html.BeginForm("", "", FormMethod.Post, new { name = "IrisForm", id = "AddPermissionsForm" }); %>
             <ul class="validation-box">
            </ul>
            <div class="dialog-text" id="AddPermissionsText">
                <div>
                    <div class="Add-Permissions-Dialog-Description-Column">
                        <div class="title">NTID or Active Directory Groups *</div>
                        <div class="subtext">Separate multiples with semicolons.</div>
                    </div>
                    <div class="Add-Permissions-Dialog-input-Column">
                        <%:Html.TextBox("EntityIdString", "", new { maxlength = 50, @class = "ad-lookup-box" })%>
                        <a href="#" onclick="ManageCreateWorkspacePermissionsWidget.LookupUserToPermission(); return false;" title="Lookup">Lookup...</a>
                    </div>
                </div>
            </div>
         
        <div class="buttons" >
            <button id="Permissions-SaveNew" onclick="ManageCreateWorkspacePermissionsWidget.SaveNewPermissions()" class="ies-action" name="save-button" type="button">Save</button>
            <div id="Permissions-LoaderNew" class="loader display-none"></div>
            <button id="Permissions-CancelNew" onclick="ManageCreateWorkspacePermissionsWidget.CloseSaveNewPermissionsDialog();" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
          <% Html.EndForm(); %> 
    </div>

   
    <button id="Div2" onclick="ManageCreateWorkspacePermissionsWidget.displayAddGroupIssues()" style="display:none;" class="ies-action" name="add-permissions-button" type="button">+ Add user/group</button>    

    <div style="display:none; width: auto; min-height: 107px; height: auto;" id="AddGroupIssues" class="ui-dialog-content ui-widget-content">
        <% Html.BeginForm("", "", FormMethod.Post, new { name = "AddGroupIssuesForm", id = "AddGroupIssuesForm" }); %>
            <div class="dialog-text" id="MessageText">
                <div class="title">The following permissions were not added:</div>
                <div id="ManageCreateWorkspacePermissions-Message">
                    <div>
                        <div id="ManageCreateWorkspacePermissions-MessageText">
                        </div>
                    </div>
                </div>
            </div>
         
        <div class="buttons" >
            <button id="Messages-Close" onclick="ManageCreateWorkspacePermissionsWidget.closeAddGroupIssues();" class="ies" name="ok-button" type="button">OK</button>
        </div>
          <% Html.EndForm(); %> 
    </div>
     
