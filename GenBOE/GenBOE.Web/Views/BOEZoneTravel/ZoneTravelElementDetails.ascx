<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOETravelElementDetailsModelView>" %>

<%
    int boeState = ViewData.ContainsKey("BOEState") ? (int)ViewData["BOEState"] : (int)BOEState.None;
    IEnumerable<GenBOE.ActionLogic.ModelView.BOECustomFieldModelView> customfields = Model.CustomFieldOptions;
    int rteFieldSize = ViewBag.RteFieldSize;
%>

<script type="text/javascript">
    //create base js object;
    var boeId = <%:Model.BOEID %>;
    var zoneTravelId = <%:Model.TravelID %>;
    var zoneTravelUpdateDateLong = '<%:Model.UpdateDateLong %>';
    var ZoneTravelElementDetails_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var Permissions_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    
    var ZoneTravelElementDetails = InitializeZoneTravelElementDetailsWidget(ZoneTravelElementDetails_ReadOnly, boeId, zoneTravelId, zoneTravelUpdateDateLong);

    $(function () {
        var boeStateIsDraft = '<%:boeState%>' == '<%:(int)BOEState.Draft%>';
        var WorkspaceState = '<%: ViewData["WorkspaceState"] %>';
    
        AfterDomLoadZoneTravelElementDetailsWidget(ZoneTravelElementDetails, boeStateIsDraft, WorkspaceState, <%: rteFieldSize %>);    
    });
</script>

<div class="zone-travel-element-details module expanded">
    <div class="module-header-data">
        <div class="float-left">
            Travel Details</div>
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
    
    <div class="module-content-data expanded-content" id="ZoneTravelElementDetailsFormContainer">
  
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ZoneTravelElementDetailsForm" }))
           { %>
        <ul class="validation-box"></ul>

        <%: Html.HiddenFor(model => model.BOEID) %>
        <%: Html.HiddenFor(model => model.TravelID) %>

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
                <input id="ZoneTravelTaskTitle" name="ZoneTravelTaskTitle" spellcheck="true" type="text" maxlength="100" value="<%:Model.TaskTitle %>" style="width:375px"/> 
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Description
            </div>
            <div class="form-element">
                <textarea rows="2" cols="20" id="ZoneTravelTaskDescription" name="ZoneTravelTaskDescription" onkeyup="Helper.textAreaLimit(this, <%:Constants.MAX_RTE_LENGTH %>)"><%:Model.TravelTaskDescription %></textarea>
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-label task-start-date-label">
                Task Start Date *</div>
            <div class="form-element task-start-date">
                <%: Html.TextBox("ZoneTravelStartDate", Model.StartDate.ToString(), new { @class = "normal-date" })%>
            </div>
            <div class="form-label task-end-date-label">
                Task End Date *</div>
            <div class="form-element task-end-date">
                <%: Html.TextBox("ZoneTravelEndDate", Model.EndDate.ToString(), new { @class = "normal-date" })%>
            </div>
        </div>
        <!-- BEGIN CUSTOM FIELDS -->
        <%  if (customfields != null && customfields.Any()) { %>
        <%      foreach (GenBOE.ActionLogic.ModelView.BOECustomFieldModelView customField in customfields) { %>
                    <div class="form-row">
                        <div class="form-label">
                            <%:customField.CustomFieldMetaData.FieldName%><%if (customField.CustomFieldMetaData.isRequired) {%>*<%} %>
                        </div>
                        <div class="form-element" id="TaskElementCustomFieldID">
                            <% // PKID of the Xref in the DB
                            int selectedID = -1;
                            //option ID for the selected value
                            int selectedOptionID = -1;
                            string updateDateLong = "0";
                            int openEndedId = -1;
                            string openEndedValue = string.Empty;

                            foreach (GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView option in customField.CustomFieldOptions)
                            {
                                IEnumerable<GenBOE.ActionLogic.ModelView.CustomFieldSelectionModelView> anyItems =
                                    from IDs in Model.CustomFieldValues
                                    where IDs.CustomFieldValueID == option.CustomFieldOptionID
                                    select new GenBOE.ActionLogic.ModelView.CustomFieldSelectionModelView()
                                    {
                                        SelectionID = IDs.SelectionID,
                                        UpdateDateLong = IDs.UpdateDateLong,
                                        CustomFieldValueID = IDs.CustomFieldValueID,
                                        OpenEndedValue = IDs.OpenEndedValue
                                    };

                                if (anyItems.Any())
                                {
                                    selectedID = anyItems.First().SelectionID;
                                    selectedOptionID = option.CustomFieldOptionID;
                                    updateDateLong = anyItems.First().UpdateDateLong;
                                    openEndedId = anyItems.First().CustomFieldValueID;
                                    openEndedValue = anyItems.First().OpenEndedValue;
                                    break;
                                }
                            } 
                            if (customField.CustomFieldMetaData.isOpenEnded)
                            {%>
                                <input type="text" customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" customfieldvalueid="<%: openEndedId %>" selectionid="<%:selectedID%>" updatedatelong="<%:updateDateLong %>" openended="true" class="customField TaskElementCustomField" maxlength="250" value="<%: openEndedValue %>" />  
                            <%}
                            else
                            {%>
                                <select customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" updatedatelong="<%:updateDateLong %>" openended="false" class="customField TaskElementCustomField" selectionid="<%:selectedID%>">
                                    <option value=""></option>
                                    <% foreach (GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView option in customField.CustomFieldOptions) { %>
                                    <option value="<%:option.CustomFieldOptionID %>"<%if (selectedOptionID == option.CustomFieldOptionID){%>selected="selected"<% } %>><%:option.ID%>-<%:option.Description%></option>
                                    <% } %>
                                </select>
                            <%}%>
                        </div>
                    </div>
        <%      }  // end foreach %>
        <%  }  // end if %>
        <!-- END CUSTOM FIELDS -->
        <% } // end form %>
    </div>
</div>