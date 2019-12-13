<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.ODCElementDetailsModelView>" %>

<%
    int boeState = ViewData.ContainsKey("BOEState") ? (int)ViewData["BOEState"] : (int)BOEState.None;
%>

<script type="text/javascript">

    var WorkspaceState = '<%: ViewData["WorkspaceState"] %>';
    var boeId = <%:Model.BOEID %>;
    var updateDateLong = '<%:Model.UpdateDateLong %>';
    //create base js object;
    var ODCElementDetails = InitializeODCElementDetailsWidget(boeId, updateDateLong);

    $(function () {
        var boeStateIsDraft = '<%:boeState%>' == '<%:(int)BOEState.Draft%>';

        AfterDomLoadODCElementDetailsWidget(ODCElementDetails, boeStateIsDraft);
    });
</script>

<div class="odc-element-details module expanded">
    <div class="module-header-data">
        <div class="float-left">
            ODC Details</div>
        <div class="float-right right-header">
            <div id="ODCStartDateLabel" style="display: None">
                ODC Start Date:
                <div id="TaskStartDate">
                </div>
            </div>
            |
            <div id="ODCEndDateLabel" style="display: None">
                ODC End Date:
                <div id="TaskEndDate">
                </div>
            </div>
        </div>        
    </div>
    <div class="module-content-data expanded-content" id="ODCElementDetails">
   
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ODCElementDetailsForm" }))
           { %>
        <ul class="validation-box"></ul>

        <%: Html.HiddenFor(model => model.BOEID)%>

        <div class="form-row">
            <div class="form-label">
                Task ID
            </div>
            <div class="form-element">
                <input name="TaskID" type="text" maxlength="3" value="<%:Model.TaskID %>" style="width:375px"/> 
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Title *
            </div>
            <div class="form-element">
                <input id="ODCTaskTitle" name="ODCTaskTitle" spellcheck="true" type="text" maxlength="100" value="<%:Model.TaskTitle %>" style="width:375px"/> 
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Description *
            </div>
            <div class="form-element">
                <textarea rows="2" cols="20" id="ODCTaskDescription" name="ODCTaskDescription" onkeyup="Helper.textAreaLimit(this, 500000)"><%:Model.ODCTaskDescription%></textarea>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label odc-start-date-label">
                ODC Start Date *</div>
            <div class="form-element odc-start-date">
                <%: Html.TextBox("ODCStartDate", Model.StartDate.ToString(), new { @class = "normal-date" })%>
            </div>
            <div class="form-label odc-end-date-label">
                ODC End Date *</div>
            <div class="form-element odc-end-date">
                <%: Html.TextBox("ODCEndDate", Model.EndDate.ToString(), new { @class = "normal-date" })%>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <%: Model.MOQTextLabel %> **
            </div>
            <div class="form-element">
                <%: Html.TextAreaFor(model => model.ODCMOQText, new { id = "ODCMOQText", onkeyup = "Helper.textAreaLimit(this, 500000)" })%>
            </div>
            <input type="hidden" name="ODCID" id="ODCID" value="<%:Model.ODCID %>" />
        </div>
        <%} %>
    </div>
</div>
