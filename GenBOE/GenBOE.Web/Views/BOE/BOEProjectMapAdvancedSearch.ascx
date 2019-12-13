<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.BOEAdvancedSearchModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.Validation" %>
<script type="text/javascript">
    <% var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }; %>
    BOEAdvancedSearchWidget = new Widget("AdvancedForm", <%: ViewData["READONLY"] %>);
    BOEAdvancedSearchWidget.ClearForm = function () {
        $('#Search-AdvancedSearch').removeClass('display-none').addClass('disabled');
        $('#Loader-AdvancedSearch').addClass('display-none');
        $('#AdvancedForm #WorkspaceName').val('');
        $('#AdvancedForm #WorkspaceDescription').val('');  
        $('#AdvancedForm #Task').val('');
        $('#AdvancedForm #Rationale').val('');
        $('#AdvancedForm #ActivityId').val('');
        $('#AdvancedForm #ActivityName').val('');
        $('#AdvancedForm #SowTitle').val('');
        $('#AdvancedForm #WBSNumber').val('');
        $('#AdvancedForm #WBSTitle').val('');
        $('#AdvancedForm #CamName').val('');
        $('#AdvancedForm #Category').val('');
        $('#AdvancedForm .default-text').removeClass('display-none');
        BOEAdvancedSearchWidget.clearValidationBox();
    }

    BOEAdvancedSearchWidget.removeDefaultText =  function() {
            $(this).siblings('.default-text').addClass('display-none');
    };

    BOEAdvancedSearchWidget.displayDefaultText =  function() {
        if ($(this).val() == "")
        {
            $(this).siblings('.default-text').removeClass('display-none');
        }
    };

     BOEAdvancedSearchWidget.focusonInput =  function() {
            $(this).siblings('input').focus;
    };

    $(function () {
        BOEAdvancedSearchWidget.afterDOMLoad();

        $('#AdvancedForm input').focusin(BOEAdvancedSearchWidget.removeDefaultText);

        $('#AdvancedForm input').focusout(BOEAdvancedSearchWidget.displayDefaultText);

        $('#AdvancedForm div.default-text').click(function() {
            $(this).siblings('input').focusin();
            $(this).siblings('input').focus();
        });
  
        BOEAdvancedSearchWidget.registerForEvent("CLEAR_FORM", BOEAdvancedSearchWidget.ClearForm);

        // Click events get bound on page load and when .dialog is called on the parent, call unbind to prevent them from happening twice.
        $('#Cancel-AdvancedSearch').unbind('click');
        $('#Cancel-AdvancedSearch').click(function () { $(document).trigger("CANCEL_SEARCH"); });
       
        BOEAdvancedSearchWidget.registerForDelegateEvent('click', '#Search-AdvancedSearch:not(.disabled)', function (e) {
            var button=this;

                var boeSearchDiv = $('#BOESearch');

                BOESearchWidget.AdvancedData = {};
                BOESearchWidget.AdvancedData.IsQuickSearch = false;
                BOESearchWidget.AdvancedData.WorkspaceName = $('#WorkspaceName', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WorkspaceDescription = $('#WorkspaceDescription', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.ActivityId = $('#ActivityId', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.ActivityName = $('#ActivityName', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.SowTitle = $('#SowTitle', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.Task = $('#Task', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.Rationale = $('#Rationale', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WBSNumber = $('#WBSNumber', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WBSTitle = $('#WBSTitle', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.CamName = $('#CamName', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.Category = $('#Category', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.IsCopyFromBoeContext = $('#IsCopyFromBoeContext', boeSearchDiv).val();
                BOESearchWidget.data = BOESearchWidget.AdvancedData;
                BOESearchWidget.type = "AdvancedSearch";

                $('#Search-AdvancedSearch').addClass('display-none');
                $('#Loader-AdvancedSearch').removeClass('display-none');
                BOEAdvancedSearchWidget.clearValidationBox();

                var dataToSend = JSON.stringify(BOESearchWidget.data);

                BOEAdvancedSearchWidget.ajaxUpdatedRequest({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE %>',
                        '<%: WebConstants.ACTION_PROJECTMAP_ADVANCED_SEARCH_FOR_BOES %>', 'boe/' + BOESearchWidget.BoeID),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html',
                    data: dataToSend,
                    success: function (response) {
                        $(document).trigger("DISPLAY_RESULTS", response);
                        BOESearchWidget.clearValidationBox();                    },
                    error: function () {
                        $('#Search-AdvancedSearch').removeClass('display-none');
                        $('#Loader-AdvancedSearch').addClass('display-none');
                    }
                }, $(button));
      
         });
    });

</script>

<div id="AdvancedSearchDiv">

<% Html.BeginForm("", "", FormMethod.Post, new { name = "AdvancedForm", id = "AdvancedForm" }); %>
    <div class="form-header">Advanced Search</div>
    <ul class="validation-box"></ul>
    <div class="form-row">
        <div class="form-label">Workspace/Proposal Name</div>
        <div class="form-element full">
            <%: Html.TextBox("WorkspaceName", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Workspace Description</div>
        <div class="form-element full">
            <%: Html.TextBox("WorkspaceDescription", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Activity Id</div>
        <div class="form-element full">
            <%: Html.TextBox("ActivityId", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">ActivityName</div>
        <div class="form-element full">
            <%: Html.TextBox("ActivityName", null, new { maxlength = ValidationConstants.MAX_BOE_TITLE_LENGTH })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">SOW Title</div>
        <div class="form-element full">
            <%: Html.TextBox("SowTitle", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">WBS #</div>
        <div class="form-element full">
            <%: Html.TextBox("WBSNumber", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">WBS Element Title</div>
        <div class="form-element full">
            <%: Html.TextBox("WBSTitle", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Task</div>
        <div class="form-element full">
            <%: Html.TextBox("Task", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"><%: Model.IsProjectMapDiscrete ? "Basis of Estimate" : "Rationale" %></div>
        <div class="form-element full">
            <%: Html.TextBox("Rationale", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Cam Name</div>
        <div class="form-element full">
            <%: Html.TextBox("CamName", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Category</div>
        <div class="form-element full">
            <%: Html.TextBox("Category", null, new { maxlength = Constants.MAX_RTE_LENGTH })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="buttons">
                <button id="Search-AdvancedSearch" class="ies-action disabled" type="button" name="save-button">Search</button>
                <div id="Loader-AdvancedSearch" class="loader display-none"></div>
                <button id="Cancel-AdvancedSearch" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
    </div>
<% Html.EndForm(); %>
</div>