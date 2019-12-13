<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.GettingStartedHelpModelView>" %>

<script type="text/javascript">

    GoToPermissions = function () {
        var tempUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace()%>', '<%: WebConstants.CONTROLLER_PERMISSIONS %>', '<%: WebConstants.ACTION_MANAGE_PERMISSIONS %>', '');

        // need to remove the trailing '/' from the URL
        tempUrl = tempUrl.substring(0, tempUrl.length - 1);

        window.location = tempUrl;
    }

    GoToCustomFields = function () {
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
        '<%: WebConstants.ACTION_WORKSPACE_SETTINGS %>', '#BOECustomFields');
    }

    GoToClins = function () {
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_CLIN %>',
        '<%: WebConstants.ACTION_INDEX %>', '');
    }

    GoToWBS = function () {
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_WBS %>',
        '<%: WebConstants.ACTION_INDEX %>', '');
    }

    GoToBoes = function () {
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_BOE %>',
        '<%: WebConstants.ACTION_INDEX %>', '');
    }

    GoToWorkspaceStatus = function () {
        window.location = CreatePostURL(
        '<%:SiteMasterUtilities.GetCurrentWorkspace()%>',
        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
        '<%: WebConstants.ACTION_WORKSPACE_SETTINGS %>', '#WorkspaceStatus');
    }

    HideHelp = function () {
        // need to save help closure
        var data= {};
        data.GettingStartedHelpModelView = {};
         
        data.GettingStartedHelpModelView.HideGettingStartedHelp = true;
        data.GettingStartedHelpModelView.UpdateDateLong = '<%:Model.UpdateDateLong %>';
                
        var dataToSend = JSON.stringify(data.GettingStartedHelpModelView);
               
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE%>',
                '<%: WebConstants.ACTION_SAVE_HIDE_GETTING_STARTED_HELP_MENU%>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () { $("#helpmodule").hide(); }                    
        });    
    }

    $(function () {
        <%if (Model.HideGettingStartedHelp){%>
         $("#helpmodule").hide();
        <%} else {%>
         $("#helpmodule").show();
        <%}%>

        if ('<%: ViewData["DisplayProjectMapOnly"] %>' == 'True') {
            $('a[name=ClinsLink]').parent().addClass('display-none');
            $('a[name=WBSLink]').parent().addClass('display-none');
            $('a[name=BOELink]').parent().addClass('display-none');
            $('a[name=WorkspaceStatusLink]').parent().addClass('display-none');
            $('span[name=CustomFieldsText]').addClass('display-none');
        }
     });

</script>

<div class="getting-started-help-workspace" id="GettingStartedHelp">
            The following steps must be completed before Basis of Estimates (BOEs) can be created.  All setup pages can be accessed by the Workspace Administration menu above.
                <ol>
                    <li><a onclick="GoToPermissions()">Grant permissions to the Workspace.</a></li>
                    <li><a onclick="GoToCustomFields()">Setup options for Resources and Performing Organizations. <span name="CustomFieldsText">Create custom fields if applicable.</span></a></li>
                    <li><a name="ClinsLink" onclick="GoToClins()">Setup the Contract Line Item Numbers (CLIN) if applicable.</a></li>
                    <li><a name="WBSLink" onclick="GoToWBS()">Setup the Work Breakdown Structure (WBS) if applicable.</a></li>
                    <li><a name="BOELink" onclick="GoToBoes()">Create and assign BOEs.</a></li>
                    <li><a name="WorkspaceStatusLink" onclick="GoToWorkspaceStatus()">Change Workspace Status to Working to allow BOEs to be edited.</a></li>
                </ol>
    
 </div>