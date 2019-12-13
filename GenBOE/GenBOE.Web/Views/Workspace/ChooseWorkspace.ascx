<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.ChooseWorkspaceModelView>>" %>
<script type="text/javascript">

    // Event handler for changes to the dropdown menu selection
    // If a workspace is selected, this enables the GO button, otherwise it disables it
    WorkspaceChanged = function () {
        // Get the selected workspace value
        var selectedWorkspace = GetSelectedWorkspace();

        // If the workspace is not empty, enable the Go button, otherwise disable it
        if (selectedWorkspace.length > 0 && selectedWorkspace != "-1") {
            $("#ChooseWorkspace-GO").prop("disabled", false);
        }
        else {
            $("#ChooseWorkspace-GO").prop("disabled", true);
        }
    }

    // When the GO button is clicked, go to the selected workspace
    GoToSelectedWorkspace = function () {
        // Get the selected workspace value
        var selectedWorkspace = GetSelectedWorkspace();

        // If the workspace is not empty, go to the selected workspace
        if (selectedWorkspace.length > 0 && selectedWorkspace != "-1") {
            window.location = '/' + selectedWorkspace; 
        }
    }

    // Get the selected workspace value
    GetSelectedWorkspace = function () {
        return $.trim($("#ChooseWorkspace-WorkspaceDropDownList").val());
    }

    //bind any events that the objects need to observe to and member functions.
    $(function () {
        // Bind events
        $("#ChooseWorkspace-WorkspaceDropDownList").change(WorkspaceChanged);
        $("#ChooseWorkspace-GO").click(GoToSelectedWorkspace);
        $("#ChooseWorkspace-WorkspaceDropDownList, #ChooseWorkspace-GO").keypress(function (e) {
            if (e.keyCode == '13') {
                $("#ChooseWorkspace-GO").click();
            }
        });

        // Set the initial button state
        WorkspaceChanged();
        
        $("#SelectWorkspaceFilter").change(function () {
            $(document).trigger('WORKSPACE_STATUS_FILTER_CHANGED');
        });

        $('#workspaceSelectionLoader').hide();
    });
</script>
<div class="my-workspaces" id="MyWorkspaces">
    <div class="name">Workspace Status:</div>
    <select id="SelectWorkspaceFilter" class="workspaces-filter" style="margin-bottom:10px;">
        <option value="">All</option>
        <option value="-1">All Active</option>
        <option value="<%:(int)WorkspaceState.Initialization %>">Initialization</option>
        <option value="<%:(int)WorkspaceState.Working %>">Working</option>
        <option value="<%:(int)WorkspaceState.Locked %>">Locked</option>
        <option value="<%:(int)WorkspaceState.Complete %>">Complete</option>
        <option value="<%:(int)WorkspaceState.Closed %>">Closed</option>
    </select>

    <div class="name">My Workspaces</div>

    <%  // Move worskpaces from the Model into a List of SelectListItems
        List<SelectListItem> workspaceSelects =
            (from workspace in Model
                select new SelectListItem { Value = workspace.WorkspaceShortname.ToString(), Text = workspace.WorkspaceName }).ToList();

        if (workspaceSelects.Count > 0)
        {
            // Add a default element as the first item in the dropdown collection
            SelectListItem defaultItem = new SelectListItem();
            defaultItem.Value = "-1";
            defaultItem.Text = "Select a Workspace";
            workspaceSelects.Insert(0, defaultItem); 
    %>
    <%: Html.DropDownList("Workspaces", workspaceSelects, new { @id = "ChooseWorkspace-WorkspaceDropDownList", @class = "workspaces ignore-dirty" })%>
    <div id="workspaceSelectionLoader" class="loader"></div>
    <button id="ChooseWorkspace-GO" class="ies-action go-button float-right" type="button">GO</button>


    <%  }
        // If there are no workspaces we'll need to output a message to indicate this to the user
        else
        {
    %>
            <b>You do not have access to any GenBOE workspaces</b>
    <%
        }
    %>
</div>