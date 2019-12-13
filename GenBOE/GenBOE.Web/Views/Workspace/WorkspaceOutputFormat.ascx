<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.WorkspaceOutputFormatModelView>" %>

<script  type="text/javascript">

    WorkspaceOutputFormatWidget = new Widget("WorkspaceOutputFormat", <%: ViewData["READONLY"] %>);
  
    WorkspaceOutputFormatWidget.preparedForSubmit = function () {
        WorkspaceOutputFormatWidget.data.OutputFormat = $('#OutputFormat option:selected').val();
        WorkspaceOutputFormatWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';
        return true;
    }

    WorkspaceOutputFormatWidget.Initialize = function() {
        WorkspaceOutputFormatWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { WorkspaceOutputFormatWidget.cleanDirty(); }); 
    }

    WorkspaceOutputFormatWidget.ViewTemplate = function() {
        // Remove the old hidden iFrame, if it exists
        $('#DownloadTarget-OutputFormatTemplate').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'DownloadTarget-OutputFormatTemplate',
            'class': 'display-none',
            'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.ACTION_GET_OUTPUT_FORMAT_TEMPLATE %>',
                $('#WorkspaceOutputFormat select option:selected').prop("value"))
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');

    }

    WorkspaceOutputFormatWidget.BackToJumpPage = function() {
        WorkspaceOutputFormatWidget.cleanDirty();
        window.location.hash = '#';
    };

    $(function () {
        WorkspaceOutputFormatWidget.afterDOMLoad();
        WorkspaceOutputFormatWidget.Initialize();

        createModule($('#WorkspaceOutputFormat'));
        WorkspaceOutputFormatWidget.Module = $('div.workspace-output.module');
        WorkspaceOutputFormatWidget.Content = $('#WorkspaceOutputFormat');

        $('#Cancel-OutputFormat, #Back-OutputFormat').click(function () {
            if (WorkspaceOutputFormatWidget.isDirty()) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function() {
                        WorkspaceOutputFormatWidget.BackToJumpPage();
                    },
                    null);
            }
            else {
                WorkspaceOutputFormatWidget.BackToJumpPage();
            }
        });

        WorkspaceOutputFormatWidget.registerForLiveEvent('click', '#Save-OutputFormat:not(.disabled)', function () {
            if (WorkspaceOutputFormatWidget.preparedForSubmit()) {

                $('#Save-OutputFormat').addClass('display-none');
                $('#Loader-OutputFormat').removeClass('display-none');

                var dataToSend = 
                    JSON.stringify(
                    {
                        inOutputFormatId:$('#OutputFormat option:selected').prop("value"),
                        inSortById:$('#SortBy option:selected').prop("value")
                    });

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function () {
                        WorkspaceOutputFormatWidget.BackToJumpPage();
                    },
                    error: function () {
                        $('#Save-OutputFormat').removeClass('display-none');
                        $('#Loader-OutputFormat').addClass('display-none');
                    }
                });
            }
        });

        // rebind the 'View Template' link and description text when the user changes their selection
        $('#WorkspaceOutputFormat select').change(function() {
            $('#OutputFormat-Description').text($('#WorkspaceOutputFormat select option:selected').attr("title"));
            refreshModule($('.workspace-output.module'));
        });

        // setup the description of the template selected when the page loads
        $('#OutputFormat-Description').text($('#WorkspaceOutputFormat select option:selected').attr("title"));
                
        if (WorkspaceOutputFormatWidget.isReadOnly()) {
            $('#Back-OutputFormat').removeClass('display-none');
        }
        WorkspaceOutputFormatWidget.applyReadOnly();

        refreshModule($('.workspace-output.module'));        
    });


</script>

<div id="WorkspaceOutputFormat" class="workspace-output module">
    <div class="module-header-data">Output Format Template</div>
    <div class="module-content-data">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "WorkspaceOutputFormatForm" }))
           { %>
        <div class="form-row">
            <div>Select the output format template to be used on all BOEs. Click on the template name to view.</div>
        </div>
        <div class="form-row">
            <div class="form-label">Templates: </div>
            <div class="form-element">
                <select id="OutputFormat">
                    <%foreach (var listItem in (IEnumerable<GenBOE.Web.Controllers.SelectListItemWithTitle>)ViewData["ExportFormatTypes"])
                      { %>
                       <option value="<%:listItem.Value%>" 
                               <% if (listItem.Selected) Response.Write("selected=true"); %> 
                               title="<%:listItem.Title%>"><%:listItem.Text%>
                       </option>
                    <%}%>
                </select>
                <a id="ViewTemplate-OutputFormat" onclick="WorkspaceOutputFormatWidget.ViewTemplate()">View template</a>
            </div> 
        </div>
        <div class="form-row">
            <div class="form-label">Description: </div>
            <span id="OutputFormat-Description"></span>
        </div>
        <div class="form-row">
            <div class="form-label">Sort Order: </div>
            <div class="form-element">
                <select id="SortBy">
                    <%foreach (var listItem in (IEnumerable<GenBOE.Web.Controllers.SelectListItemWithTitle>)ViewData["SortOrderTypes"])
                      { %>
                       <option value="<%:listItem.Value%>" 
                               <% if (listItem.Selected) Response.Write("selected=true"); %> 
                               title="<%:listItem.Title%>"><%:listItem.Text%>
                       </option>
                    <%}%>
                </select>
            </div>
        </div>
        <div class="buttons">
            <button id="Save-OutputFormat" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-OutputFormat" class="loader display-none"></div>
            <button id="Cancel-OutputFormat" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
        <% } %>
        <button id="Back-OutputFormat" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
    </div>
</div>
