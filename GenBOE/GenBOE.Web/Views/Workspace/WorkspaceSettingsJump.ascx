<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">
    var WorkspaceSettingsJumpWidget = new Widget('WorkspaceSettingsJump');

    $(function () {
        WorkspaceSettingsJumpWidget.Module = $('div.workspace-settings-jump.module');
        
        var DisplayLaborCostTM = '<%:ViewData["DisplayLaborCostTM"] %>';

        if (DisplayLaborCostTM == 'True') {
            $("#ResourceRatesTM").parent().show();
        }
        else {
            $("#ResourceRatesTM").parent().hide();
        }

        if ('<%: ViewData["DisplayProjectMapOnly"] %>' == 'True') {
            $('#SumofBOEWorkspaceVariables').parent().addClass('display-none');
            $('#DiscreteWorkspaceVariables').parent().addClass('display-none');
        }

        createModule(WorkspaceSettingsJumpWidget.Module);

        // pulls out the error message from the session..
        var errorMessage = sessionStorage.errorMessage;

        if (errorMessage) {
            $('#errorMessage').addClass('validation-box').css('display', 'block').html(errorMessage);
        }

        sessionStorage.errorMessage = '';

        refreshModule(WorkspaceSettingsJumpWidget.Module);
    });

</script>

<div id="WorkspaceSettingsJump" class="workspace-settings-jump module">
    <div class="module-header-data">Workspace Settings</div>
    <div class="module-content-data">
        <div id="errorMessage"></div>
        <div class="left-column">
            <div>
                <a id="WorkspaceIdentification" href="#WorkspaceIdentification">Workspace Identification</a>
                <div>Edit your workspace's name, description, cost volume lead/pricer,
                    contract period of performance, proposal submittal date and RFP #.  Indicate if Workspace contains OCI 
                    information.</div>
            </div>
            <div>
                <a id="OutputFormat" href="#OutputFormat">Output Format Template</a>
                <div>Select the format template to be used for all BOEs.</div>
            </div>
            <div>
                <a id="WorkspaceStatus" href="#WorkspaceStatus">Workspace Status</a>
                <div>Change the status (<% if (!ViewBag.IsProjectMapWs) { %>Initialization, <%}%>Working, Locked, Complete, Closed) of the Workspace.  The status will determine when users can work on BOEs.</div>
            </div>
            <div>
                <a id="WorkspaceStatusHistory" href="#WorkspaceStatusHistory">Workspace Status History</a>
                <div>View a log of all the Workspace status changes.</div>
            </div>
            <div>
                <a id="SumofBOEWorkspaceVariables" href="#SumofBOEWorkspaceVariables">Sum of BOE Workspace Variables</a>
                <div>Define sum of BOE variables that can be used across all BOEs within the Workspace.</div>
            </div>            
            <div>
                <a id="DiscreteWorkspaceVariables" href="#DiscreteWorkspaceVariables">Discrete Workspace Variables</a>
                <div>Define discrete variables that can be used across all BOEs within the Workspace.</div>
            </div>
        </div>
        <div class="right-column">
            <div>
                <a id="BOECustomFields" href="#BOECustomFields">BOE Custom Fields</a>
                <div>Edit the options for the Resource and Performing Organizations fields.  Create and edit custom fields for BOEs.</div>
            </div>
            <div>
                <a id="ShareandAllowSearch" href="#AllowSearch">Share & Allow Search</a>
                <div>Specify if all information within the Workspace can be shared with all genBOE users once
                    the Workspace status is Complete. This will allow all genBOE users to search an reuse Workspace
                    and BOE Data. Sharing information is encouraged to promote efficiency. If the Workspace contains
                    Organizational Conflict of Interest (OCI) information, it cannot be shared and searched.
                </div>
            </div>
             <div>
                <a id="BackupVersions" href="#BackupVersions">Manage Backup Versions</a>
                <div>Create, delete and restore versions of the Workspace.</div>
            </div>
            <div>
                <a id="ResourceRatesTM" href="#ResourceRatesTM"><%= Model.WorkspaceResourceRateTMHeadingText %></a>
                <div><%= Model.WorkspaceResourceRateTMJumpDescription%></div>
            </div>
            <% if (!ViewBag.IsProjectMapWs) { %>
            <div>
                <a id="WorkspaceEmailPreferences" href="#WorkspaceEmailPreferences">Manage Workspace Email Preferences</a>
                <div>View System Default and Forced Email Preferences for Workspaces. Manage your Workspace Email Overrides (where applicable).</div>
            </div>
            <% } %>
            <% if (ViewBag.IsSpace && ViewBag.EnableUCOT) { %>
            <div>
                <a id="ManageUCOT" href="#ManageUCOT">Manage Uncompensated Overtime</a>
                <div>Manage and edit the UCOT (Uncompensated Overtime) Factor.</div>
            </div>
            <% } %>
        </div>
    </div>
</div>

