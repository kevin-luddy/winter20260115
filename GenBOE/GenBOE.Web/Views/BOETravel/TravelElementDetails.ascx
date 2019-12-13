<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOETravelElementDetailsModelView>" %>

<%
    int boeState = ViewData.ContainsKey("BOEState") ? (int)ViewData["BOEState"] : (int)BOEState.None;
    IEnumerable<GenBOE.ActionLogic.ModelView.BOECustomFieldModelView> customfields = Model.CustomFieldOptions;
%>

<script type="text/javascript">

    //create base js object;
    var boeId = <%:Model.BOEID %>;
    var travelId =<%:Model.TravelID %>;
    var travelUpdateDateLong = '<%:Model.UpdateDateLong %>';
    var TravelElementDetails_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var Permissions_ReadOnly = <%= ViewData["READONLY"] %>;
    
    var TravelElementDetails =InitializeTravelElementDetailsWidget(TravelElementDetails_ReadOnly, boeId, travelId, travelUpdateDateLong);

    $(function () {

        var boeStateIsDraft = '<%:boeState%>' == '<%:(int)BOEState.Draft%>';
        var WorkspaceState = '<%: ViewData["WorkspaceState"] %>';
    
        AfterDomLoadTravelElementDetailsWidget(TravelElementDetails, boeStateIsDraft, WorkspaceState);

        
  });
    
</script>

<div class="travel-element-details module expanded">
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
    
    <div class="module-content-data expanded-content" id="TravelElementDetailsFormContainer">
  
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "TravelElementDetailsForm" }))
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
                <input id="TravelTaskTitle" name="TravelTaskTitle" spellcheck="true" type="text" maxlength="100" value="<%:Model.TaskTitle %>" style="width:375px"/> 
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Task Description
            </div>
            <div class="form-element">
                <textarea rows="2" cols="20" id="TravelTaskDescription" name="TravelTaskDescription" onkeyup="Helper.textAreaLimit(this, 500000)"><%:Model.TravelTaskDescription %></textarea>
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-label task-start-date-label">
                Task Start Date *</div>
            <div class="form-element task-start-date">
                <%: Html.TextBox("TravelStartDate", Model.StartDate.ToString(), new { @class = "normal-date" })%>
            </div>
            <div class="form-label task-end-date-label">
                Task End Date *</div>
            <div class="form-element task-end-date">
                <%: Html.TextBox("TravelEndDate", Model.EndDate.ToString(), new { @class = "normal-date" })%>
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
                            foreach (GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView option in customField.CustomFieldOptions)
                            {
                                IEnumerable<GenBOE.ActionLogic.ModelView.CustomFieldSelectionModelView> anyItems =
                                    from IDs in Model.CustomFieldValues
                                    where IDs.CustomFieldValueID == option.CustomFieldOptionID
                                    select new GenBOE.ActionLogic.ModelView.CustomFieldSelectionModelView()
                                    {
                                        SelectionID = IDs.SelectionID,
                                        UpdateDateLong = IDs.UpdateDateLong,
                                        CustomFieldValueID = IDs.CustomFieldValueID
                                    };

                                if (anyItems.Any())
                                {
                                    selectedID = anyItems.First().SelectionID;
                                    selectedOptionID = option.CustomFieldOptionID;
                                    updateDateLong = anyItems.First().UpdateDateLong;
                                    break;
                                }
                            } %>
                            <select customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" updatedatelong="<%:updateDateLong %>" class="customField TaskElementCustomField" selectionid="<%:selectedID%>">
                                <option value=""></option>
                                <% foreach (GenBOE.ActionLogic.ModelView.BOECustomFieldOptionModelView option in customField.CustomFieldOptions) { %>
                                <option value="<%:option.CustomFieldOptionID %>"<%if (selectedOptionID == option.CustomFieldOptionID){%>selected="selected"<% } %>><%:option.ID%>-<%:option.Description%></option>
        <% } %>
                            </select>
                        </div>
                    </div>
        <%      }  // end foreach %>
        <%  }  // end if %>
        <!-- END CUSTOM FIELDS -->
        <% } // end form %>
    </div>
</div>