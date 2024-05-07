<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.IBOEHeaderModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import Namespace="IES.Common.classes" %>
<%
    WorkspaceState workspaceStateValue = Html.GetViewDataValue<WorkspaceState>("WorkspaceState", WorkspaceState.None);
    BOEState boeStateValue = Html.GetViewDataValue<BOEState>("BOEState", BOEState.None);
    int boeState = (int)boeStateValue;
    IEnumerable<BOECustomFieldModelView> customfields = Html.GetViewDataObject<IEnumerable<BOECustomFieldModelView>>("CustomFields", new List<BOECustomFieldModelView>(0));
    
    string boeStateDisplay = (boeStateValue == BOEState.DraftLocked) ? BOEState.Draft.GetDescription() : boeStateValue.GetDescription();
    string lockUnlockImage = null;
    if (workspaceStateValue == WorkspaceState.Locked)
    {
        if (boeStateValue == BOEState.Draft)
        {
            lockUnlockImage = "/Resources/css/images/unlock_resource_toggle.png";
        }
        else if (boeStateValue == BOEState.DraftLocked)
        {
            lockUnlockImage = "/Resources/css/images/lock_resource_toggle.png";
        }
    }

    bool allowDateShift = Html.GetViewDataValue<bool>("ALLOW_DATE_SHIFT", false);
    int rteFieldSize = ViewBag.RteFieldSize;
    var DataSourceAnswers = Model.HeaderRteTemplateAnswers.Where(a => a.SourceId == (int)RteTemplateSource.BoeSources).ToList();
    bool showCustomQuestions = DataSourceAnswers.Any();
    int numberQuestions = showCustomQuestions ? DataSourceAnswers.Count : 1;
    bool readOnlyMode = Html.GetViewDataValue<bool>("IsReadOnlyMode", false);
    bool missingBrcCodes = Html.GetViewDataValue<bool>("MissingBrcCodes", false);

%>

<script type="text/javascript">
    var BoeHeaderWidget;

    $(function() {

        var allowDateShift = '<%:allowDateShift%>'.isTrue();
        var boeId = '<%: Model.BOEID %>';
        var boeController = '<%:WebConstants.CONTROLLER_BOE %>';
        var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
        var containsOCI = <%= ViewData["ContainsOCI"] %>;
        var readOnly = <%: ViewData["READONLY"] %>;
        var workspaceState = '<%: ViewData["WorkspaceState"] %>';
        var saveEditBoeHeaderUrl = GenSession.CreateUrl({
            workspace: workspace,
            controller: boeController,
            action: '<%:WebConstants.ACTION_SAVE_EDIT_BOE_HEADER%>',
            boe: boeId });
        var dateShiftUrl = CreatePostURL(
        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        '<%: WebConstants.CONTROLLER_DATESHIFT %>',
        '<%: WebConstants.ACTION_INDEX %>',
        'id/<%: Model.BOEID%>/level/<%: ((int)IES.Common.Level.BOE).ToString() %>');
        
        var boeStateNotDraft = '<%:boeState%>' != '<%:(int)BOEState.Draft%>';
        var findAdjacentBoesUrl = CreatePostURL(
            '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%: WebConstants.CONTROLLER_WORKSPACE %>',
            '<%: WebConstants.ACTION_FIND_ADJACENT_BOES %>');

        var newBoeUrl = CreatePostURL(
            '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%: WebConstants.CONTROLLER_BOE %>',
            '<%: WebConstants.VIEW_EDIT_BOE_INDEX %>',
        'boe/');

        BoeHeaderWidget = AfterDomLoadBoeHeaderWidget(containsOCI, readOnly, workspaceState, saveEditBoeHeaderUrl, 
            boeStateNotDraft, allowDateShift, <%:rteFieldSize%>, dateShiftUrl, findAdjacentBoesUrl, boeId, newBoeUrl, '<%:showCustomQuestions.ToString()%>'.isTrue(), <%:numberQuestions%>);
    });
</script>

<div id="BoeHeader" class="boe-header module expanded" style="width: 720px;">
    <div class="module-header-data">
        <div class="wbs-display">
            WBS <span title=" <%: Html.DisplayFor(model => model.WBS)%>"><% if (Model.WBS.Length > 50)
                                                                            {
                                                                                var wbsString = Model.WBS.Remove(50) + "...";
            %><%: Html.DisplayFor(model => wbsString)%>
                <% }
                                                                            else
                                                                            {%>
                <%: Html.DisplayFor(model => model.WBS)%>
                <%} %>
            </span>

        </div>
        <div class="clin-display">
            CLIN <span title=" <%: Html.DisplayFor(model => model.CLIN)%>"><% if (Model.CLIN.Length > 20)
                                                                              {
                                                                                  var clinString = Model.CLIN.Remove(20) + "...";
            %><%: Html.DisplayFor(model => clinString)%>
                <% }
                                                                              else
                                                                              {%>
                <%: Html.DisplayFor(model => model.CLIN)%>
                <%} %>
            </span>
        </div>
    </div>
    <div class="module-content-data" id="BoeHeaderTopContent">
        <div class="form-row">
            <div class="form-label">BOE Start Date</div>
            <div class="form-element">
                <input type="text" class="small-date" disabled="disabled" value="<%: Model.StartDate.Value.ToMonthString() %>" name="StartDate" id="StartDate" />
            </div>
            <div class="form-label" style="min-width: 70px !important;"></div>
            <div class="form-label" style="min-width: 70px !important">BOE End Date</div>
            <div class="form-element">
                <input type="text" class="small-date" disabled="disabled" value="<%: Model.EndDate.Value.ToMonthString() %>" name="EndDate" id="EndDate" />&nbsp;&nbsp;
                <a id="AdjustDatesLink" style="float: right;">Adjust Dates</a>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">State</div>
            <div class="form-element"><%:boeStateDisplay%><%if (lockUnlockImage != null) {%>&nbsp;&nbsp;<img src="<%:lockUnlockImage%>"><%}%></div>
        </div>
    </div>
    <div class="module-content-data expanded-content">

        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "BoeHeaderForm", onSubmit = "return false" }))
           { %>
        <div class="form-row">
            <ul class="validation-box"></ul>
             <%if (missingBrcCodes) { %>
                <div class="validation-box" style="display:block;">
                    Warning: The workspace period of performance crosses into the 1LMX common disclosure period and requires Business Resource Codes (BRCs) for estimating. BOE authoring will be restricted to textual updates until BRCs have been loaded. Please notify your Workspace Administrator.
                </div>
             <% } %>
        </div>
        <%: Html.HiddenFor(model => model.BOEID) %>
        <%: Html.HiddenFor(model => model.UpdateDateLong) %>
        <%  
            if(Model.IsTitleRequired)
                Html.RenderPartial(WebConstants.VIEW_BOE_HEADER_SPACE_TITLE);
            else
                Html.RenderPartial(WebConstants.VIEW_BOE_HEADER_ISGS_TITLE);

            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_HEADER_DESCRIPTION, WebConstants.CONTROLLER_BOE, new { id = ViewData["BOEID"] }); %>
        <div id="SourcesOfDataRow" class="form-row">
            <div class="form-label">
                <%: Model.LabelSourcesOfData %>
                <%if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) { %>
                <div class="nonPrintableLabel" title="This field is not printed in any reports">Non-Printable</div>
                <%} %>
            </div>
            <% if (!readOnlyMode)
                { %>
                <div class="form-element" id="sources-element">
                    <% Html.RenderPartial(WebConstants.VIEW_RTE_TEMPLATE, new GenBOE.Web.ModelView.RteTemplateModelView(DataSourceAnswers, "DataSource", Model.DataSource));  %>
                </div>
            <% } %>
        </div>
        <div class="form-row<% if (!Model.ShowHistoricMetricCheck)
                               { %> display-none<% } %>">
            <div class="form-label"></div>
            <div class="form-element">
                <div style="width: 525px; display: inline-block" id="DisclosureCheck">
                    <input type="checkbox" id="MetricUsed" style="display: inline-block" <%: Model.HistoricMetricDisclosureChecked ? "checked " : string.Empty %> name="HistoricMetricDisclosureChecked" value="true"/>
                    If a program name is used, the appropriate Lockheed Martin Contract Department confirmed that the heritage
                         program name associated with the historical data cited in this BOE may be disclosed in this BOE.
                </div>
            </div>
        </div>
        <%if (customfields != null)
          {
              foreach (BOECustomFieldModelView customField in customfields)
              { 
        %>
        <div class="form-row custom-field-row">
            <div class="form-label" style="overflow: hidden; text-overflow: ellipsis;" title="<%:customField.CustomFieldMetaData.FieldName%>">
                <%:customField.CustomFieldMetaData.FieldName%><%if (customField.CustomFieldMetaData.isRequired)
                                                                {%>*<%} %>
            </div>
            <div class="form-element" style="width:250px" id="customFieldID">
                <%
                                                                //PK ID of the Xref in the DB
                                                                int selectedID = -1;
                                                                //option ID for the selected value
                                                                int selectedOptionID = -1;
                                                                string updateDateLong = "0";  // i.e. DateTime.MinValue
                                                                int openEndedId = -1;
                                                                string openEndedValue = string.Empty;

                                                                foreach (BOECustomFieldOptionModelView option in customField.CustomFieldOptions)
                                                                {
                                                                    IEnumerable<CustomFieldSelectionModelView> anyItems = from IDs in Model.CustomFieldValues
                                                                                                                          where IDs.CustomFieldValueID == option.CustomFieldOptionID
                                                                                                                          select new CustomFieldSelectionModelView()
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
                     <input type="text" customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" customfieldvalueid="<%: openEndedId %>" selectionid="<%:selectedID%>" updatedatelong="<%:updateDateLong %>" openended="true" class="customField" maxlength="250" value="<%: openEndedValue %>" />  
                   <%}
                       else
                       {%>
                <select name="CustomFieldValues" customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" updatedatelong="<%:updateDateLong %>" openended="false" class="customField" selectionid="<%:selectedID%>">
                    <option value=""></option>
                    <%foreach (BOECustomFieldOptionModelView option in customField.CustomFieldOptions) { %>
                        <!-- keep option tag on one line to avoid insertion of line breaks (br) when page is read-only -->
                        <option value="<%:option.CustomFieldOptionID %>" <%if (selectedOptionID == option.CustomFieldOptionID) {%>selected="selected"<% } %>><%:option.ID%>-<%:option.Description%></option>
                    <%}%>
                </select>
                <%}%>
            </div>
        </div>
        <% }
          } %>
    <div class="required-header-note">
        * required for saving as draft.<br />
        ** required for validating and submitting for approval        
    </div>
    <div class="oci-note required-header-note"></div>
    <% } %>
    </div>
</div>
