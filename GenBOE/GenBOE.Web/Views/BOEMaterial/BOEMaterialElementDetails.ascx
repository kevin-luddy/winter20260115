<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.MaterialDetailsModelView>" %>

<script type="text/javascript">
    // Get the read-only attribute passed in from the controller
    var MaterialElementDetails_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var WorkspaceState = '<%: ViewData["WorkspaceState"] %>';
        
    //create base js object;
    var MaterialElementDetails = new Widget("MaterialElementDetailsForm", MaterialElementDetails_ReadOnly);


    MaterialElementDetails.preparedForSubmit = function () {
            
                MaterialElementDetails.data.BOEID = <%: Model.BOEID %>;
                MaterialElementDetails.data.TaskID = $("#MaterialElementDetailsForm :input[name=TaskID]").val();
                MaterialElementDetails.data.MaterialID = $("#MaterialElementDetailsForm :input[name=MaterialID]").val();
                MaterialElementDetails.data.TaskTitle = $("#MaterialElementDetailsForm :input[name=TaskTitle]").val();
                MaterialElementDetails.data.TaskDescription = $("#MaterialElementDetailsForm :input[name=TaskDescription]").val();

                MaterialElementDetails.data.MoqText = $("#MaterialElementDetailsForm :input[name=MoqText]").val();
                MaterialElementDetails.data.UpdateDateLong = $("#MaterialElementDetailsForm :input[name=UpdateDateLong]").val().toString();
               
           return true;
    };


    MaterialElementDetails.updateDate = function (inMaterialDetails){
        MaterialElementDetails.data.UpdateDateLong = inMaterialDetails.UpdateDateLong;
    };

    //bind any events that the objects need to observe to and member functions.
    $(function () {
        MaterialElementDetails.afterDOMLoad();
        createModule($('.material-element-details.module'));

        MaterialElementDetails.registerForEvent('CLEAN_BOE_DETAILS_DIRTY', function () { MaterialElementDetails.cleanDirty(); });

        refreshModule($('.material-element-details.module'));

        CollapsibleModule($('.material-element-details.module'));      

// DP: For whatever reason, these fields are always editable, even if the rest of the page is read only.. I left the code here, in case that changes..
//        if(!MaterialElementDetails.isReadOnly())
//        {
        InitializeRTE('TaskDescription', { maxlen: 500000 }, MaterialElementDetails);
        InitializeRTE('MoqText', { maxlen: 500000 }, MaterialElementDetails);
//        }
//        else
//        {
//            HandleRTEDataForReadOnly("#TaskDescription", ".replacedWidgetText");
//            HandleRTEDataForReadOnly("#MoqText", ".replacedWidgetText");
//        }    

    });

    MaterialElementDetails.registerForEvent("AllWidgetLoaded", function(e){
     
    });
</script>
<div class="material-element-details module expanded">
    <div class="module-header-data">
        <div class="float-left">
            Task Element Details</div>
        <div class="float-right right-header">
            <div id="TaskStartDateLabel" style="display: None">
                Task Start Date:
                <div id="TaskStartDate">
                </div>
            </div>
            |
            <div id="TaskEndDateLabel" style="display: None">
                Task End Date:
                <div id="TaskEndDate">
                </div>
            </div>
        </div>
    </div>
    <div class="module-content-data expanded-content">
        
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "MaterialElementDetailsForm" }))
           { %>
        <ul class="validation-box"></ul>
        <%: Html.HiddenFor(model => model.MaterialID) %>
        <%: Html.HiddenFor(model => model.UpdateDateLong) %>
        <%: Html.HiddenFor(model => model.BOEID)%>
        <div class="form-row">
            <div class="form-label">
                Task ID
            </div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.TaskID, new { @class = "half", @maxlength = "3" })%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Title *
            </div>
            <div class="form-element">
                <%: Html.TextBoxFor(model => model.TaskTitle, new { @class = "half", @maxlength="100", spellcheck="true" })%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Description
            </div>
            <div class="form-element">
                <%: Html.TextAreaFor(model => model.TaskDescription, new { onkeyup="Helper.textAreaLimit(this, 500000)" })%>
            </div>
        </div>

        <div class="form-row">
            <div class="form-label">
                 <%: Model.MOQTextLabel %> **
            </div>
            <div class="form-element">
                <%: Html.TextAreaFor(model => model.MoqText, new { onkeyup = "Helper.textAreaLimit(this, 500000)" })%>
            </div>
        </div>
        <%}
      %> 
    </div>
</div>