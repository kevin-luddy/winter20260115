<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.BOEAdvancedSearchModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.Validation" %>
<script type="text/javascript">
    <% var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }; %>
    BOEAdvancedSearchWidget = new Widget("AdvancedForm", <%: ViewData["READONLY"] %>);
    BOEAdvancedSearchWidget.WSPerfOrgs =  <%= serializer.Serialize(ViewData["WSPERFORGS"])%>
    BOEAdvancedSearchWidget.ClearForm = function () {
        $('#Search-AdvancedSearch').removeClass('display-none').addClass('disabled');
        $('#Loader-AdvancedSearch').addClass('display-none');
        $('#AdvancedForm :input[name=SelectedCategory]').val(-1);
        $('#AdvancedForm #WorkspaceName').val('');
        $('#AdvancedForm #WorkspaceDescription').val('');
        $('#AdvancedForm #StartDate').val('');
        $('#AdvancedForm #EndDate').val('');
        $('#AdvancedForm #RFP').val('');
        $('#AdvancedForm #BOETitle').val('');
        $('#AdvancedForm #BOEDescription').val('');
        $('#AdvancedForm #WBSNumber').val('');
        $('#AdvancedForm #WBSTitle').val('');
        $('#AdvancedForm #CLINNumber').val('');
        $('#AdvancedForm #CLINTitle').val('');
        $('#AdvancedForm #TaskTitleSearch').val('');
        $('#AdvancedForm #TaskDescriptionSearch').val('');
        $('#AdvancedForm #SourcesOfData').val('');
        $('#AdvancedForm #PerformingOrg').val('');
        $('#AdvancedForm #CostVolumeLead').val('');
        $('#AdvancedForm #Author').val('');
        $('#AdvancedForm #Approver').val(''); 
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

     BOEAdvancedSearchWidget.autocompletePerformingOrgs = function( request, response ){
         var toSearch = request.term.toLowerCase();
         returned = $.grep(BOEAdvancedSearchWidget.WSPerfOrgs, function(p){
             return p.PerformingOrgName.toLowerCase().indexOf(toSearch) != -1 ||
            p.PerformingOrgDesc.toLowerCase().indexOf(toSearch) != -1 ;})

         response($.map(returned, function( item ) {
             return {
                 label: item.PerformingOrgName+"-"+item.PerformingOrgDesc,
                 value: item.PerformingOrgName
             }
         }));
     }


    $(function () {
        BOEAdvancedSearchWidget.afterDOMLoad();

        $( "#AdvancedForm input[name=PerformingOrg]" ).autocomplete({
            source: BOEAdvancedSearchWidget.autocompletePerformingOrgs,
            minLength:2,
            delay:400
        });
 

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
                BOESearchWidget.AdvancedData.SelectedCategory = $('#AdvancedForm :input[name=SelectedCategory]').val();
                BOESearchWidget.AdvancedData.WorkspaceName = $('#WorkspaceName', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WorkspaceDescription = $('#WorkspaceDescription', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.StartDate = $('#StartDate', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.EndDate = $('#EndDate', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.RFP = $('#RFP', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.BOETitle = $('#BOETitle', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.BoeDescription = $('#BOEDescription', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WBSNumber = $('#WBSNumber', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.WBSTitle = $('#WBSTitle', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.CLINNumber = $('#CLINNumber', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.CLINTitle = $('#CLINTitle', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.TaskTitle = $('#TaskTitleSearch', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.TaskDescription = $('#TaskDescriptionSearch', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.SourcesOfData = $('#SourcesOfData', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.PerformingOrg = $('#PerformingOrg', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.CostVolumeLeadNTID = $('#CostVolumeLead', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.AuthorNTID = $('#Author', boeSearchDiv).val();
                BOESearchWidget.AdvancedData.ApproverNTID = $('#Approver', boeSearchDiv).val();
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
                        '<%: WebConstants.ACTION_ADVANCED_SEARCH_FOR_BOES %>', 'boe/' + BOESearchWidget.BoeID),
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
        <div class="form-label">Category</div>
        <div class="form-element">
            <%: Html.DropDownList("SelectedCategory", Model.Categories)%>
        </div>
    </div>
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
        <div class="form-label">Proposal Submittal Date - between</div>
        <div class="form-element">
            <div class="inline-block">
                <div class="default-text">mm/dd/yyyy</div>
                <%: Html.TextBox("StartDate", null, new { maxlength = "10" })%>
            </div>
            and
            <div class="inline-block">
                <div class="default-text">mm/dd/yyyy</div>    
                <%: Html.TextBox("EndDate", null, new { maxlength = "10" })%>
            </div>
        </div>
    </div>
    <div class="form-row" <% if(!Model.ShowRFP) Response.Write("style='display: none;'"); %>>
        <div class="form-label">RFP #</div>
        <div class="form-element full">
            <%: Html.TextBox("RFP", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">BOE Title</div>
        <div class="form-element full">
            <%: Html.TextBox("BOETitle", null, new { maxlength = ValidationConstants.MAX_BOE_TITLE_LENGTH })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">BOE Description</div>
        <div class="form-element full">
            <%: Html.TextBox("BOEDescription", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">WBS #</div>
        <div class="form-element full">
            <%: Html.TextBox("WBSNumber", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">WBS Title</div>
        <div class="form-element full">
            <%: Html.TextBox("WBSTitle", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">CLIN #</div>
        <div class="form-element full">
            <%: Html.TextBox("CLINNumber", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">CLIN Title</div>
        <div class="form-element full">
            <%: Html.TextBox("CLINTitle", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Task Title</div>
        <div class="form-element full">
            <%: Html.TextBox("TaskTitleSearch", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Task Description</div>
        <div class="form-element full">
            <%: Html.TextBox("TaskDescriptionSearch", null, new { maxlength = Constants.MAX_RTE_LENGTH })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Sources of Data</div>
        <div class="form-element full">
            <%: Html.TextBox("SourcesOfData", null, new { maxlength = "100" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Performing Org</div>
        <div class="form-element">
            <%: Html.TextBox("PerformingOrg", null, new { maxlength = "30" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"><%: Model.LabelLeadPricer %></div>
        <div class="form-element full">
            <div class="default-text">Lastname, Firstname</div>
            <%: Html.TextBox("CostVolumeLead", null, new { maxlength = "40" })%>            
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Author</div>
        <div class="form-element full">
            <div class="default-text">Lastname, Firstname</div>
            <%: Html.TextBox("Author", null, new { maxlength = "40" })%>            
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Approver</div>
        <div class="form-element full">
            <div class="default-text">Lastname, Firstname</div>
            <%: Html.TextBox("Approver", null, new { maxlength = "40" })%>
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