<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.WorkspaceOutputFormatAdminModelView>" %>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var WorkspaceOutputFormat_ReadOnly = <%= ViewData["READONLY"] %>;

    var WorkspaceOutputFormatWidget = new Widget('WorkspaceOutputFormatForm', WorkspaceOutputFormat_ReadOnly);

    WorkspaceOutputFormatWidget.AddNewDialog = {};
    WorkspaceOutputFormatWidget.AddNewDialog.Element = $('#AddNewTemplateDialog');
    WorkspaceOutputFormatWidget.AddNewDialog.Params = { width: 580, modal: true, resizable: false, draggable: true, title: 'Add Output Format Template' };
    WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog = {};
    WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog.Element = $("#AddInProgressDialog");
    WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "AddInProgressDialog" };
    WorkspaceOutputFormatWidget.EditTemplateDialog = {};
    WorkspaceOutputFormatWidget.EditTemplateDialog.Element = $("#EditTemplateDialog");
    WorkspaceOutputFormatWidget.EditTemplateDialog.Params = { width: 580, modal: true, resizable: false, draggable: true, title: 'Edit Output Format Template' };

    WorkspaceOutputFormatWidget.BindEvents = function() {
        $('#AddNewTemplateDialog-SaveButton').click(WorkspaceOutputFormatWidget.SubmitUploadForm);
        $('#EditTemplateDialog-SaveButton').click(WorkspaceOutputFormatWidget.SubmitEditForm);
        $('#ManageOutputTemplates-AddButton').click(WorkspaceOutputFormatWidget.DisplayAddNewTemplate); 
        $('a[name=ViewTemplate]').click(function() { 
            WorkspaceOutputFormatGridWidget.ViewTemplate($('#EditTemplateForm [name=TemplateId]').val()); 
        });
        $('a[name=ViewWorkspaces]').click(function() {
            var workspaces = $(this).next();
            if (workspaces.hasClass('display-none')) {
                workspaces.removeClass('display-none');
                $(this).text('Hide Workspaces');
            } else {
                workspaces.addClass('display-none');
                $(this).text('View Workspaces');
            }
        });
        $('#EditTemplateForm, #AddNewTemplateDialog-Form').on('change keydown', function(e) {
            $(document).trigger('DATA_DIRTY', $(e.currentTarget).attr('id'));
        });

        WorkspaceOutputFormatWidget.registerForEvent('PAGE_OUTPUT_FORMAT', WorkspaceOutputFormatWidget.PageOutputFormat);
        WorkspaceOutputFormatWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { WorkspaceOutputFormatWidget.cleanDirty(); });
    }

    WorkspaceOutputFormatWidget.PageOutputFormat = function(event, data) {
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_PAGE_OUTPUT_FORMAT %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response)
            {             
                $('#ManageOutputTemplates-Content').html(response);
            }
        });
    }

    WorkspaceOutputFormatWidget.ReloadGridData = function(getActiveTemplates) {
        $('#ManageOutputTemplates-Content').html("<div class=\"loader\"></div>");
        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.VIEW_MANAGE_OUTPUT_TEMPLATES_GRID %>'),
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ getActiveTemplates:getActiveTemplates }),
            success: function(response) {
                $('#ManageOutputTemplates-Content').html(response);
            }
        });
    }

    // display the add new template dialog
    WorkspaceOutputFormatWidget.DisplayAddNewTemplate = function () {
        var form = $('#AddNewTemplateDialog-Form');
        form.get(0).reset();

        WorkspaceOutputFormatWidget.OpenDialogAfterInitialize(WorkspaceOutputFormatWidget.AddNewDialog);
    }

    WorkspaceOutputFormatWidget.DisplayEditTemplate = function(templateId) {
        // populate data
        var row = $('#ManageOutputFormatTemplatesGrid tr[pkid=' + templateId + ']');
        var form = $('#EditTemplateForm');

        form.get(0).reset();
        form.find('[name=TemplateName]').val(row.find('.manage-output-format-name').text().trim());
        form.find('[name=TemplateDescription]').val(row.find('.manage-output-format-description').text().trim());
        form.find('[name=TemplateId]').val(templateId);
        form.find('[name=UpdateDate]').val(row.find('input[name=UpdateDateLong]').val().toString());

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_ADMIN%>', '<%:WebConstants.ACTION_GET_ASSIGNED_WORKSPACE_INFO%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: JSON.stringify({templateId:templateId}),
            success: function (response) {
                $('#EditTemplateForm .workspace-view table tbody').html(response);                
            }
        });

        WorkspaceOutputFormatWidget.OpenDialogAfterInitialize(WorkspaceOutputFormatWidget.EditTemplateDialog);
    }

    WorkspaceOutputFormatWidget.ShowAddNewTemplateDialogError = function () {
        $("#AddNewTemplateDialog-Error").slideDown("slow");
    }

    WorkspaceOutputFormatWidget.HideAddNewTemplateDialogError = function () {
        $("#AddNewTemplateDialog-Error").slideUp("slow");
    }

    WorkspaceOutputFormatWidget.ShowEditTemplateDialogError = function () {
        $("#EditTemplateDialog-Error").slideDown("slow");
    }

    WorkspaceOutputFormatWidget.HideEditTemplateDialogError = function () {
        $("#EditTemplateDialog-Error").slideUp("slow");
    }

    WorkspaceOutputFormatWidget.SubmitUploadForm = function () {
        if (!$(this).hasClass('disabled')){
            WorkspaceOutputFormatWidget.HideAddNewTemplateDialogError();
            WorkspaceOutputFormatWidget.OpenDialogAfterInitialize(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
            
            WorkspaceOutputFormatWidget.AppendIFrameForUploadResponse();
            $("#AddNewTemplateDialog-Form").submit();
        }
    }

    WorkspaceOutputFormatWidget.AppendIFrameForUploadResponse = function() {
        // Remove the old hidden iFrame, if it exists
        $('#AddNewTemplateDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#AddNewTemplateDialog-Form').append('<iframe id="AddNewTemplateDialog-UploadTarget" name="AddNewTemplateDialog-UploadTarget" class="display-none"></iframe>');
        $('#AddNewTemplateDialog-UploadTarget').load(WorkspaceOutputFormatWidget.StopUpload);
    }

    WorkspaceOutputFormatWidget.StopUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#AddNewTemplateDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide dialogs
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialog);
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);

                // Show success notification
                RaiseNotification('Add successful');

                // reset form                
                $('#AddNewTemplateDialog-Form')[0].reset();

                // Set drop-down to Active in case it isn't already
                $('#ManageOutputTemplates-ActiveArchiveSelect').val('0');

                // Redirect back to grid page
                WorkspaceOutputFormatWidget.ReloadGridData();
            }
            else {
                $("#AddNewTemplateDialog-ErrorText").html(results.Message);
                WorkspaceOutputFormatWidget.ShowAddNewTemplateDialogError();
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
            }
        }
        else {
            WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
        }
    }

    WorkspaceOutputFormatWidget.SubmitEditForm = function() {
        if (!$(this).hasClass('disabled')) {
            WorkspaceOutputFormatWidget.HideEditTemplateDialogError();
            WorkspaceOutputFormatWidget.OpenDialogAfterInitialize(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
            
            WorkspaceOutputFormatWidget.AppendEditIFrameForUploadResponse();
            $("#EditTemplateForm").submit();
        }
    };

    WorkspaceOutputFormatWidget.AppendEditIFrameForUploadResponse = function() {
        // Remove the old hidden iFrame, if it exists
        $('#EditTemplateDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#EditTemplateForm').append('<iframe id="EditTemplateDialog-UploadTarget" name="EditTemplateDialog-UploadTarget" class="display-none"></iframe>');
        $('#EditTemplateDialog-UploadTarget').load(WorkspaceOutputFormatWidget.StopEditUpload);
    }

    WorkspaceOutputFormatWidget.StopEditUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#EditTemplateDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            var results = eval('(' + uploadResponseElement.html() + ')');

            if (results != undefined && results.Status) {
                // Hide dialogs
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.EditTemplateDialog);
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);

                // Show success notification
                RaiseNotification('Edit successful');

                // reset form                
                $('#EditTemplateForm')[0].reset();

                // Redirect back to grid page
                WorkspaceOutputFormatWidget.ReloadGridData();
            }
            else {
                console.log(results);
                $("#EditTemplateDialog-ErrorText").html(results.Message);
                WorkspaceOutputFormatWidget.ShowEditTemplateDialogError();
                WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
            }
        }
        else {
            WorkspaceOutputFormatWidget.CloseDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
        }
    }

    WorkspaceOutputFormatWidget.ActiveArchiveSelect = function(selection) {
        WorkspaceOutputFormatWidget.ReloadGridData(selection !== "Archived");
    };

    $(function () {
        WorkspaceOutputFormatWidget.afterDOMLoad();
        WorkspaceOutputFormatWidget.BindEvents();
        WorkspaceOutputFormatWidget.InitializeDialog(WorkspaceOutputFormatWidget.AddNewDialog);
        WorkspaceOutputFormatWidget.InitializeDialog(WorkspaceOutputFormatWidget.AddNewDialogInProgressDialog);
        WorkspaceOutputFormatWidget.InitializeDialog(WorkspaceOutputFormatWidget.EditTemplateDialog);
    });

    //hide the error messages when the dialog is closed so they are gone when a new dialog is opened
    $("#EditTemplateDialog").on('dialogclose', function(event) {
        WorkspaceOutputFormatWidget.HideEditTemplateDialogError();
    });

    $("#AddNewTemplateDialog").on('dialogclose', function(event) {
        WorkspaceOutputFormatWidget.HideAddNewTemplateDialogError();
    });

</script>

<div id="ManageOutputTemplates" class="manage-output-format-templates section">
    <div class="title">Manage Output Format Templates</div>
    <div class="form-row">
        <div>
            Edit the list of Workspaces that can access an output template by clicking the "Select Workspaces for template" or view a template in MS Word by clicking the "View template" link.
        </div>
    </div>
    <div class="form-row">
        <div class="buttons inline">
            <button class="ies-action" id="ManageOutputTemplates-AddButton" type="button">+ Add</button>
            <select id="ManageOutputTemplates-ActiveArchiveSelect" onchange="WorkspaceOutputFormatWidget.ActiveArchiveSelect($(this).val())">
                <option value="Active" selected>Active</option>
                <option value="Archived">Archived</option>
            </select>
        </div>
    </div>
    <div class="form-row">
        <div id="ManageOutputTemplates-Content" class="form-element">
            <% Html.RenderAction(WebConstants.VIEW_MANAGE_OUTPUT_TEMPLATES_GRID,
                    WebConstants.CONTROLLER_ADMIN); %>
        </div>
    </div>
</div>

<div id="EditTemplateDialog" class="manage-output-format-new-template">
    <div id="EditTemplateDialog-Error" class="validation-box" style="display: none;">
        <div>
            <div id="EditTemplateDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>
    <form id="EditTemplateForm" target="EditTemplateDialog-UploadTarget" method="post" enctype="multipart/form-data" action="/default/Admin/EditOutputFormatTemplate">
        <input type="hidden" name="TemplateId" />
        <input type="hidden" name="UpdateDate" />
        <div class="form-row">
            <div class="form-label">Template Name *</div>
            <div class="form-element">
                <input type="text" name="TemplateName" /></div>
        </div>
        <div class="form-row">
            <div class="form-label">Template Description *</div>
            <div class="form-element">
                <textarea name="TemplateDescription"></textarea></div>
        </div>
        <div class="form-row">
            <div class="form-label">Workspaces Using<br />
                Template:</div>
            <div class="form-element">
                <a name="ViewWorkspaces">View Workspaces</a>
                <div class="workspace-view display-none">
                    <table class="grid readonly">
                        <thead>
                            <tr>
                                <th>Workspace</th>
                                <th>State</th>
                                <th>Pricer</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>
                </div>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Current Template:</div>
            <div class="form-element"><a name="ViewTemplate">View Template</a></div>
        </div>
        <div class="form-row">
            <div class="form-label">New Template File<br />
                Location</div>
            <div class="form-element">
                <input type="file" size="78" name="TemplateFile" />
            </div>
        </div>
        <div class="form-row last-form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div class="oci-note"><b>Note:</b> All changes will apply to existing workspaces that selected this template.</div>
                <div class="buttons">
                    <div id="EditTemplateDialog-Loader" class="loader display-none"></div>
                    <button id="EditTemplateDialog-SaveButton" class="ies-action disabled" name="save-button" type="button">Save</button>
                </div>
            </div>
        </div>
    </form>
</div>

<div id="AddNewTemplateDialog" style="display: none;" class="manage-output-format-new-template">
    <div id="AddNewTemplateDialog-Error" class="validation-box" style="display: none;">
        <div>
            <div id="AddNewTemplateDialog-ErrorText">
            </div>
        </div>
        <div class="clear"></div>
    </div>

    <% using (Html.BeginForm(WebConstants.ACTION_SAVE_WORKSPACE_OUTPUT_FORMAT_NEW_TEMPLATE, WebConstants.CONTROLLER_ADMIN, FormMethod.Post, new { enctype = "multipart/form-data", id = "AddNewTemplateDialog-Form", target = "AddNewTemplateDialog-UploadTarget" }))
       { %>
    <ul class="validation-box"></ul>

    <div class="form-row">
        <div class="form-label">Template Name*</div>
        <div class="form-element">
            <%: Html.TextBoxFor(model => model.TemplateName, new { @maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Template Description*</div>
        <div class="form-element">
            <%: Html.TextAreaFor(model => model.TemplateDescription, new { onkeyup = "Helper.textAreaLimit(this, 500)" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Template File Location*</div>
        <div class="form-element">
            <input type="hidden" id="AddNewTemplateDialog-DocumentDomain" name="documentDomain" />
            <input type="file" size="78" id="TemplateFileLocation" name="TemplateFileLocation" />
        </div>
    </div>
    <div class="form-row last-form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="buttons">
                <div id="AddNewTemplateDialog-Loader" class="loader display-none"></div>
                <button id="AddNewTemplateDialog-SaveButton" class="ies-action disabled" name="save-button" type="button">Save</button>
            </div>
        </div>
    </div>
    <% } %>
</div>

<div id="AddInProgressDialog" style="display: none; font-size: 18px; font-weight: bold; font-family: Arial, Helvetica; text-align: center;">
    <div class="loader"></div>
    <br />
    Saving Template
</div>