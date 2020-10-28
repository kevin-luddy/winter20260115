<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.WorkspaceStatusModelView>" %>

<script type="text/javascript">

    WorkspaceStatusWidget = new Widget("WorkspaceStatus", <%: ViewData["READONLY"] %>);

    WorkspaceStatusWidget.data.WorkspaceStatus = "<%:Model.WorkspaceStatus %>";
    WorkspaceStatusWidget.valid = true;

    WorkspaceStatusWidget.getData = function () {
        return $("input[name='WorkspaceStatus']:checked").val();
    };
   

    WorkspaceStatusWidget.disableStatuses = function (status) {
        <% if (!ViewBag.IsProjectMapWs)
    { %>
        switch (status) {
            case "Initialization":
                $("input[name='WorkspaceStatus']")
                .not("input[value='Working']")
                .not("input[value='Initialization']")
                .not("input[value='Closed']")
                .prop('disabled', true);
                break;
            case "Working":
                $("input[name='WorkspaceStatus']")
                .not("input[value='Working']")
                .not("input[value='Initialization']")
                .not("input[value='Locked']")
                .not("input[value='Closed']")
                .prop('disabled', true);
                break;
            case "Locked":
                $("input[name='WorkspaceStatus']")
                .not("input[value='Locked']")
                .not("input[value='Working']")
                .not("input[value='Complete']")
                .not("input[value='Closed']")
                .prop('disabled', true);
                break;
            case "Complete":
                $("input[name='WorkspaceStatus']")
                .not("input[value='Complete']")
                .not("input[value='Locked']")
                .not("input[value='Working']")
                .prop('disabled', true);
                break;
            case "Closed":
                $("input[name='WorkspaceStatus']")
                .not("input[value='Closed']")
                .not("input[value='Initialization']")
                .prop('disabled', true);
                break;
        }
        <% } %>
    };

    WorkspaceStatusWidget.undoChange = function () {
        var fromState = "<%:Model.WorkspaceStatus %>";
        $("#WorkspaceStatusForm :input[value='" + fromState + "']").prop('checked', true);
        WorkspaceStatusWidget.data.WorkspaceStatus = fromState;
        WorkspaceStatusWidget.cleanDirty();
    };

    WorkspaceStatusWidget.preparedForSubmit = function () {
        WorkspaceStatusWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';
        WorkspaceStatusWidget.data.WorkspaceStatus = WorkspaceStatusWidget.getData();
        return WorkspaceStatusWidget.valid;
    };

    WorkspaceStatusWidget.BackToJumpPage = function() {
        WorkspaceStatusWidget.cleanDirty();
        window.location.hash = '#';
    };

    WorkspaceStatusWidget.registerForEvent('ReloadWSStatusPage', function() { 
        WorkspaceStatusWidget.cleanDirty();
        WorkspaceSettings.LoadWorkspaceStatus();
    });

    WorkspaceStatusWidget.SaveWorkspaceStatus = function () {
        if (WorkspaceStatusWidget.preparedForSubmit()) {
            $('#Save-WorkspaceStatus').addClass('display-none');
            $('#Loader-WorkspaceStatus').removeClass('display-none');

            WorkspaceStatusWidget.ContinueSaveWorkspaceStatus();
        }
    };

    WorkspaceStatusWidget.ContinueSaveWorkspaceStatus = function () {
        var dataToSend = JSON.stringify(WorkspaceStatusWidget.data);  
        
        WorkspaceStatusWidget.ajaxRequest({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_SAVE_WORKSPACE_STATUS %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function () {
                $(document).trigger("UPDATE_STATUS", WorkspaceStatusWidget.data.WorkspaceStatus);
                $(document).trigger('ReloadWSStatusPage');

            },
            error: function () {
                refreshModule($('.workspace-status.module'));
                $('#Save-WorkspaceStatus').removeClass('display-none');
                $('#Loader-WorkspaceStatus').addClass('display-none');
            }
        }, $(this));
    }

    $(function () {
        WorkspaceStatusWidget.afterDOMLoad();

        WorkspaceStatusWidget.disableStatuses("<%:Model.WorkspaceStatus %>");
        WorkspaceStatusWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { WorkspaceStatusWidget.cleanDirty(); });

        createModule($('.workspace-status.module'));        

        $('#Cancel-WorkspaceStatus, #Back-WorkspaceStatus').click(function () {
            if (WorkspaceStatusWidget.isDirty()) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function() {
                        WorkspaceStatusWidget.BackToJumpPage();
                    },
                    null);
            }
            else {
                WorkspaceStatusWidget.BackToJumpPage();
            }
        });


        WorkspaceStatusWidget.registerForLiveEvent('click', '#Save-WorkspaceStatus:not(.disabled)', WorkspaceStatusWidget.SaveWorkspaceStatus);
        
        WorkspaceStatusWidget.applyReadOnly();

        if (WorkspaceStatusWidget.isReadOnly()) {
            $('#Back-WorkspaceStatus').removeClass('display-none');
        }

        refreshModule($('.workspace-status.module'));
    });    

</script>

<div id="WorkspaceStatus" class="workspace-status module">
    <div class="module-header-data">Workspace Status</div>

    <div class="module-content-data">
        
<% using (Html.BeginForm("", "", FormMethod.Post, new { id = "WorkspaceStatusForm" }))
   { %>
           <ul class="validation-box"> </ul>
           <%: Html.ValidationMessageFor(model => model.WorkspaceStatus)%>
        <div class="form-row">Change the status of the Workspace.</div>
        <div class="form-row">
            <% if (!ViewBag.IsProjectMapWs) { %>
            <div class="form-element radio">
                <%: Html.RadioButton("WorkspaceStatus", WorkspaceState.Initialization, Model.WorkspaceStatus == WorkspaceState.Initialization, new { id = "WorkspaceStatus-Initialization" })%>
                <label for="WorkspaceStatus-Initialization">Initialization - Allow Workspace Administrator to manage CLINs, WBS and BOEs. Prevents edits to BOEs.</label>
            </div>
            <% } %>
        </div>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("WorkspaceStatus", WorkspaceState.Working, (Model.WorkspaceStatus == WorkspaceState.Working || (Model.WorkspaceStatus == WorkspaceState.Initialization && (bool)ViewBag.IsProjectMapWs)), new { id = "WorkspaceStatus-Working" })%>
                <label for="WorkspaceStatus-Working">Working - Allow BOEs to be edited<% if (!ViewBag.IsProjectMapWs) { %> by Authors, reviewed by Reviewers and approved/rejected by Approvers<% } %>.</label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("WorkspaceStatus", WorkspaceState.Locked, Model.WorkspaceStatus == WorkspaceState.Locked, new { id = "WorkspaceStatus-Locked" })%>
                <label for="WorkspaceStatus-Locked">Locked - 
                    <% if (!ViewBag.IsProjectMapWs) { %> 
                    Lock travel rates, Labor rates, and hour and cost estimates in BOEs. Authors will not be able to edit hour and cost estimates but will be able to edit the text for Draft BOEs. Workspace Administrators will not be able to edit T&M Subcontractor or IWTA rates.
                    <% } else { %>
                    Lock all BOEs. Users will not be able to edit BOEs.
                    <% } %>
                </label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("WorkspaceStatus", WorkspaceState.Complete, Model.WorkspaceStatus == WorkspaceState.Complete, new { id = "WorkspaceStatus-Complete" })%>
                <label for="WorkspaceStatus-Complete">Complete - Lock all <% if (!ViewBag.IsProjectMapWs) { %>travel rates, Labor rates, and <% } %>BOEs. No further work needs to be performed in the Workspace. BOEs are ready to be delivered.</label>
            </div>
        </div>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("WorkspaceStatus", WorkspaceState.Closed, Model.WorkspaceStatus == WorkspaceState.Closed, new { id = "WorkspaceStatus-Closed" })%>
                <label for="WorkspaceStatus-Closed">Closed - Lock all <% if (!ViewBag.IsProjectMapWs) { %>travel rates, Labor rates, and <% } %>BOEs. No further work needs to be performed in the Workspace. BOEs no longer needed for proposal. (i.e. No longer bidding on proposal.)</label>
            </div>
        </div>
        <div class="buttons">
            <button id="Save-WorkspaceStatus" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-WorkspaceStatus" class="loader display-none"></div>
            <button id="Cancel-WorkspaceStatus" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
        <button id="Back-WorkspaceStatus" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
    <%}%>
    </div>
</div>

