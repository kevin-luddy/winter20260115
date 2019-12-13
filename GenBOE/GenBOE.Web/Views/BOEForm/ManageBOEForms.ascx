<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView>" %>

<script type="text/javascript">
    $(function() {
    // Create widget
    var widgetConfig = {};
    widgetConfig.ContextID = "ManageBOEFormsForm";
    widgetConfig.isReadOnly = false; // used so that way we can manually override readonly behaviors
    widgetConfig.IsModule = true;
    widgetConfig.FormConfigs = []; //formConfigs;
    widgetConfig.DialogConfigs = []; //dialogConfigs;

    var ManageBOEFormsWidget = new GenWidget(widgetConfig);
    
    ManageBOEFormsWidget.BindEvents = function() {
        // Bind events for single UI elements

        $('#AddButton-BOE-Forms').click(function() { 
            var url = "/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/BOEForm/CreateForm"; 
            window.location.href = url; 
        });
        ManageBOEFormsWidget.registerForEvent('EDIT_BOE_FORMS', function(event, data) {
            var url = "/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/BOEForm/UpdateForm?boeFormId=" +data.id+ "&boeFormType=" +data.type;
            window.location.href = url; 
        });

        $('#DeleteButton-BOE-Forms').click(function () {
            if (!$('#DeleteButton-BOE-Forms').hasClass('disabled')){
                $(document).trigger('GET_DELETED_BOE_FORMS');
            }
        });
        
        ManageBOEFormsWidget.registerForEvent('SYNC_STATE_BOE_FORMS_DELETE_BUTTON', function() { 
            $('#DeleteButton-BOE-Forms').toggleClass('disabled', $("#BOEFormsGrid input:checkbox:checked").length === 0);
        });

        ManageBOEFormsWidget.registerForEvent('DELETE_BOE_FORMS', function(event, data) {
            Session.confirmDialog("Delete Confirmation", "Are you sure you want to delete these items?", function(){ ManageBOEFormsWidget.DeleteRecords(data); });
        });
    }
    
    ManageBOEFormsWidget.BackToJumpPage = function() {
        window.location.hash = '#';
    }
    
    ManageBOEFormsWidget.ReloadGridData = function() {
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_FORMS %>',
                '<%:WebConstants.ACTION_DISPLAY_MANAGE_BOE_FORMS_GRID %>', ''),
            dataType: 'html',
            success: function(response)
            {             
                $('#BOEFormsGridContent').html(response);
                $('#DeleteLoader-BOE-Forms').addClass('display-none');
                $('#DeleteButton-BOE-Forms').removeClass('display-none');
                $('#DeleteButton-BOE-Forms').addClass('disabled');
            }
        });
    }

    ManageBOEFormsWidget.DeleteRecords = function (data) {
        $('#DeleteButton-BOE-Forms').addClass('display-none');
        $('#DeleteLoader-BOE-Forms').removeClass('display-none');

        var dataToSend = JSON.stringify(data.CheckedItems);
 
        ManageBOEFormsWidget.ajaxRequest({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE_FORMS %>',
                '<%: WebConstants.ACTION_DELETE_BOE_FORMS %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response) {
                ManageBOEFormsWidget.ReloadGridData();
                Session.alertDialog("Success", "Your changes have been saved.");
            },
            error: function(response) {
                $(document).trigger('SYNC_STATE_BOE_FORMS_DELETE_BUTTON');
                Session.alertDialog("There was a problem.", "Your operation did not complete. Please try again.");
                $('#DeleteButton-BOE-Forms').removeClass('display-none');
                $('#DeleteLoader-BOE-Forms').addClass('display-none');
            }
        });
      
        $('#DeleteButton-BOE-Forms').addClass('disabled');
        $('#DeleteButton-BOE-Forms').removeClass('display-none');
        $('#DeleteLoader-BOE-Forms').addClass('display-none');
    }
    
    ManageBOEFormsWidget.BindEvents();

    var readOnly = <%: ViewData["READONLY"] %>;
    if (readOnly) {
        $('#ManageBOEFormsForm').find('input[type=checkbox]').addClass('display-none');
        $('#AddButton-BOE-Forms').addClass('display-none');
    }
});
     
</script>

<div id="ManageBOEFormsForm" class="manage-BOEForms module">
    <div class="module-header-data">Manage Integrated Non-Labor Forms</div>
    <div class="module-content-data">
        <div class="form-row">
            <div class="manage-boe-forms-buttons form-element">
                <div class="buttons inline">
                    <button id="DeleteButton-BOE-Forms" class="ies-action disabled" name="delete-button" type="button">Delete</button>
                    <div id="DeleteLoader-BOE-Forms" class="loader display-none"></div>
                    <button id="AddButton-BOE-Forms" class="ies-action" type="button">+ Add</button>
                </div>
            </div>
        </div>

        <div class="form-row">
            <div id="BOEFormsGridContent" class="form-element">
                <% Html.RenderAction(WebConstants.ACTION_DISPLAY_MANAGE_BOE_FORMS_GRID, WebConstants.CONTROLLER_BOE_FORMS); %>
            </div>
        </div>
    </div>
</div>