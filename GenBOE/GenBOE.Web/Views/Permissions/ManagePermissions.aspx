<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Permissions - <%: ((GenBOE.Web.ModelView.GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%@ Import Namespace="IES.Common.classes" %>

    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/permission") %>
    <script type="text/javascript">
        function closeAddPermissionsDialog() {
            $(":ui-dialog#AddEditPermissionsDialog").dialog("close");
        }
    </script>

     <script type="text/javascript">
		 app.value('WorkspacePermissionModel', {
			 workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
		     controller: '<%:WebConstants.CONTROLLER_PERMISSIONS %>',
		     action: '<%:WebConstants.ACTION_GET_WORKSPACE_PERMISSION_MODEL %>',
		     exportAction: '<%: WebConstants.ACTION_EXPORT_PERMISSIONS %>',
		     importAction: '<%:WebConstants.ACTION_IMPORT_PERMISSIONS %>',
             workspaceState: '<%: ((GenBOEMasterModelView)Model).WorkspaceState %>'
	     });
	 </script>

    <div class="permissions module permissions-module" data-ng-controller="permissionCtrl" data-ng-cloak="">
        <div class="module-header-data">Manage Permissions</div>
        <div class="module-content-data">
            <gen-validation data-errors="errors"></gen-validation>
            <div class="buttons">
                <button class="ies-action" type="button" data-ng-disabled ="isLoading" data-ng-class="{disabled: isLoading}" data-ng-click="openNewDialog(<%:ViewBag.IsProjectMapWs ? ((int)Role.WorkspaceAdmin).ToString() : "false" %>)">+ Add user/group</button>
                <button class="ies-action" data-ng-disabled ="isLoading" data-ng-class="{disabled: isLoading}" data-ng-click="toggleImportExport()"type="button">Import/Export Permissions</button>
            </div>
            <table id="ManagePermissions" class="grid readonly" style="width: 1366px;">
                <colgroup>
                    <col width="24%" />
                    <col width="24%" />
                    <col width="24%" />
                    <col width="24%" />
                    <col width="4%" />
                </colgroup>
                <thead>
                    <tr>
                        <th>
                            <input type="text" class="filter css3pie-position-fix" placeholder="Filter by Active Directory Group" data-ng-model="filter.GroupName" /><br />
                            Active Directory Group
                        </th>
                        <th>
                            <input type="text" class="filter css3pie-position-fix" placeholder="Filter by genBOE Access" data-ng-model="filter.Access" /><br />
                            genBOE Access
                        </th>
                        <th>
                            <input type="text" class="filter css3pie-position-fix" placeholder="Filter by User" data-ng-model="filter.Users" /><br />
                            User(s)
                        </th>
                        <th>
                            <input type="text" class="filter css3pie-position-fix" placeholder="Filter by Permission" data-ng-model="filter.Roles" /><br />
                            Permission(s)
                        </th>
                        <th class="delete"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr data-ng-show="isLoading"><td colspan="5"> <div data-ng-class="{ loader: isLoading }"></div></td></tr>
                    <tr data-ng-repeat="permission in permissions | filter:filterPermissions">
                        <td data-ng-if="permission.isGroup" data-ng-click="openEditDialog(permission)">
                            <a>{{ permission.Users[0].DisplayName }}</a>
                        </td>
                            <td data-ng-if="!permission.isGroup">
                        </td>
                        <td>
                            <div style="display:inline-block; width:auto; padding-top:4px;">{{ permission.genBOEAccess }}</div><div style="display:inline-block; padding-left:5px; position:absolute; width:auto"><img data-ng-if="permission.genBOEAccess=='No'" src="/Resources/css/images/warning_small.png" height="16" width="16" /></div>
                        </td>
                        <td data-ng-if="permission.isGroup">
                            <div>
                                <div data-ng-init="showUsers=false">
                                    <div>
                                        <a data-ng-click="ViewUsersClick(permission, !showUsers); showUsers=!showUsers"><p data-ng-if="showUsers">Hide Users</p><p data-ng-if="!showUsers">View Users</p></a>
                                    </div>
                                    <div data-ng-show="showUsers">
                                        <div data-ng-class="{ loader: permission.loadingGroups }"></div>
                                        <div data-ng-repeat="(member, access) in permission.groupMembers track by $index">                           
                                            <img data-ng-if="access==false" style="display:inline-block" src="/Resources/css/images/warning_small.png" height="10" width="10" /><div style="display:inline-block">{{ member }}</div>
                                        </div>                                    
                                    </div>                            
                                </div>
                            </div>
                        </td>
                        <td data-ng-if="!permission.isGroup" data-ng-click="openEditDialog(permission)">
                            <a>{{ permission.Users[0].DisplayName }}</a>
                        </td>
                        <td>
                            <div data-ng-repeat="role in permission.Roles">
                                {{ role.RoleName }}
                            </div>
                        </td>
                        <td>
                            <div class="delete" data-ng-click="deletePermission(permission)"></div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        
        <gen-dialog id="ImportExportPermissionsDialog" class="dialog form" data-width="700" data-title="{{ importExportDialog.title }}" data-open="importExportDialog.open" data-focus-on-open="false">
            <gen-validation data-errors="importExportDialog.errors"></gen-validation>
            <div>
                <div id="Instructions">
                    <span>Select an option below to import or export Permissions.</span> 
                    <br />
                </div>
                <div class="step one">
                    <div class="title">Step 1: Select an option.</div>
                    <div>
                        <input id="Radio-Button-Export" type="radio" class="radio" data-ng-model="importExportDialog.importExport" data-ng-value="importExportDialog.exportValue"/>
                        <label for="Radio-Button-Export">Export Permissions</label>
                    </div>
                    <div>
                        <input id="Radio-Button-Import" type="radio" class="radio" data-ng-model="importExportDialog.importExport" data-ng-value="importExportDialog.importValue" />
                        <label for="Radio-Button-Import">Import Permissions</label>
                    </div>
                </div>
                <div class="step two" data-ng-show="importExportDialog.importExport === importExportDialog.exportValue">
                    <div class="important">IMPORTANT: Do not change the column headings or options in the file. </div>
                    <br />
                    <hr />
                    <br />
                    <div class="buttons css3pie-position-fix">
                        <button id="ImportExportPermissionsDialog-ExportButton" data-ng-click="exportPermissions()" type="button" class="ies" data-ng-hide="importExportDialog.importWorking">Export</button>
                        <button id="ImportExportPermissionsDialog-ExportCancelButton" data-ng-click="toggleImportExport()" type="button" class="ies" name="cancel-button">Cancel</button>
                    </div>

                </div>
                <div class="step two" data-ng-show="importExportDialog.importExport === importExportDialog.importValue">
                    <% Html.BeginRouteForm(WebConstants.ROUTE_DEFAULT, new { 
                                            controller = WebConstants.CONTROLLER_PERMISSIONS, 
                                            action = WebConstants.ACTION_IMPORT_PERMISSIONS, 
                                            workspace = SiteMasterUtilities.GetCurrentWorkspace()}, 
                                            FormMethod.Post,
                                            new { enctype = "multipart/form-data", id = "ImportExportPermissionsDialog-Form", target = "ImportExportPermissionsDialog-UploadTarget" }); %>
                    <div class="title">Step 2: Import Permissions</div>
                    <div>
                        Choose a file to import. The file you import must be an Excel file that ends in .xlsx.<br />
                        All data will be appended to current Permissiond data.
                        <br /><u>Required Fields:</u>
                        <ul>
                            <li>NtId</li>
                            <li>Role</li>
                        </ul>
                            <span class="title">File location: </span>
                            <span>
                                <input type="hidden" id="importExportPermissionsDialog-Project-DocumentDomain" name="documentDomain" />
                                <input type="hidden" name="importExportPermissionsDialog" />
                                <input type="file" id="ImportExportPermissionsDialog-File" name="ImportExportPermissionsDialog-File" size="60" onchange="angular.element(this).scope().fileUploadChange(this)"/>
                            </span>
                            <br />   
                            <span>File must contain the same headers and columns as exported prior to importing and be in .xlsx format.</span>
                            <br />
                            <hr />
                            <br />
                            <div class="important">IMPORTANT: Do not change the column headings or options in the file. </div>
                            <br /><hr /><br />
                            <div class="buttons">
                                <button id="ImportExportPermissionsDialog-ImportButton" type="button" class="ies-action" data-ng-hide="importExportDialog.importWorking" data-ng-disabled="importExportDialog.disableImport" data-ng-click="importPermissions()" name="import-button">Import</button>
                                <div id="ImportExportPermissionsDialog-ImportLoader" class="loader" data-ng-show="importExportDialog.importWorking"></div>
                                <button id="ImportExportPermissionsDialog-ImportCancelButton" type="button" class="ies" data-ng-click="toggleImportExport()">Cancel</button>
                            </div>
                    </div>
                    <% Html.EndForm(); %>
                </div>
            </div>
        </gen-dialog>

        <gen-dialog id="AddEditPermissionsDialog" data-width="700" data-title="{{ dialog.title }}" data-open="dialog.open" data-focus-on-open="false">
            <div class="dialog-text">
                <gen-validation data-errors="dialog.errors"></gen-validation>
                <form class="permission-form">
                    <div class="form-row">
                        <div class="form-label">
                            <div class="title">NTID or Active Directory Groups *</div>
                            <div class="subtext">Separate multiple with a semicolon (Ex. jsmith; jdoe).</div>
                        </div>
                        <div class="form-element">
                            <gen-user-lookup data-ntids="data.ntids" data-readonly="!data.isEdit"></gen-user-lookup>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            <div class="title">Permission *</div>
                            <div class="subtext">Choose the level of access you want the user(s) to have.</div>
                        </div>
                        <div class="form-element">
                            <div class="role-box">
                                <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.WorkspaceAdmin%>" id="AddWorkspaceRole" data-ng-model="data.WorkspaceAdmin" />
                                <div class="subtext">
                                    <label for="AddWorkspaceRole">Workspace Administrator - Can manage Workspace settings. Can view and edit all BOEs in Workspace. Restricted to users with "Create Workspace" permissions.</label>
                                </div>
                            </div>
                            <% if(ViewBag.IsProjectMapWs) { %>
                                <script type="text/javascript">
                                    // 'Workspace Admin' role is disabled for Project Map Workspaces as it is selected by default.
                                    $("#AddWorkspaceRole").prop('disabled', true);
                                </script>
                            <% } else { %>
                                <div class="role-box">
                                    <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.Author%>" id="AddAuthorRole" data-ng-model="data.Author" />
                                    <div class="subtext">
                                        <label for="AddAuthorRole">Author - Can be assigned to write BOEs. Can View all BOEs in Workspace.</label>
                                    </div>
                                </div>
                                <div class="role-box">
                                    <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.SubcontractorAuthor%>" id="AddSubcontractorAuthorRole" data-ng-model="data.SubcontractorAuthor" />
                                    <div class="subtext">
                                        <label for="AddSubcontractorAuthorRole">Subcontractor Author - Can be assigned to write BOEs with limited access. Can only view and edit their assigned BOEs in the Workspace.</label>
                                    </div>
                                </div>
                                <div class="role-box">
                                    <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.WorkspaceReviewer%>" id="AddReviewerRole" data-ng-model="data.WorkspaceReviewer" />
                                    <div class="subtext">
                                        <label for="AddReviewerRole">Reviewer - Assigned to review all BOEs. Can view all BOEs in Workspace.</label>
                                    </div>
                                </div>
                                <div class="role-box">
                                    <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.Approver%>" id="AddApproverRole" data-ng-model="data.Approver" />
                                    <div class="subtext">
                                        <label for="AddApproverRole">Approver - Can be assigned to approve BOEs. can view all BOEs in Workspace.</label>
                                    </div>
                                </div>
                                <% if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                                    { %>
                                    <div class="role-box">
                                        <input type="checkbox" name="Roles" data-ng-true-value="<%: (int)Role.SubcontractAdmin%>" id="AddSubcontractAdminRole" data-ng-model="data.SubcontractAdmin" />
                                        <div class="subtext">
                                            <label for="AddApproverRole">GSCO Administrator – Can manage INL forms in Workspace.</label>
                                        </div>
                                    </div>
                                <% } %>
                            <% } %>
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label"></div>
                        <div class="form-element css3pie-position-fix inline">
                            <button id="savePermissionsButton" type="button" class="generation persist ies-action" data-ng-click="savePermission(result)" name="save-button">Save</button>
                            <div id="permissionsLoader" class="display-none loader" style="display:inline-block; position:relative; vertical-align:top; height:26px; margin-right:10px;"></div>
                            <button type="button" style="display:inline-block; position:relative; vertical-align:top;" onClick="closeAddPermissionsDialog()" class="ies" name="cancel-button">Cancel</button>
                        </div>
                    </div>
                </form>
            </div>
        </gen-dialog>
    </div>

    <script>
        angular.element(document).ready(function () {
            angular.bootstrap(document, ['genboe']);
        });
    </script>
</asp:Content>
