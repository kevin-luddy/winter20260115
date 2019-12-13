<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOEFormMasterModelView>" %>
<% 
    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    var otherTextClass = Model.PBOEModel.OtherExceptionApplies ? string.Empty : "display-none";
%>
<script type="text/javascript">
    $(function() {
        
        var isReadOnly = '<%:ViewData["READONLY"]%>'.isTrue();
        
        //setup sub resources
        var html = "";
        var selectedResources = <%= serializer.Serialize(Model.PBOEModel.ResourceIds)%>;
            
        $.each(ManageBOEFormsWidget.SubResources, function (i, item) {
            var checked = ' checked="checked"';
            var disabled = isReadOnly ? ' disabled="disabled"' : '';
            if (selectedResources.indexOf(item.Id) < 0) {
                checked = '';
            }

            if (ManageBOEFormsWidget.SubInUseResourceIds.indexOf(item.Id) >= 0) {
                // this resource is in-use in another BOE Form, disable it
                disabled = ' disabled="disabled"';
                checked = '';
            }

            html += '<input type="checkbox" name="ResourceIds" value="' + item.Id + '" ' + checked + disabled + ' /> ' + item.ResourceDesc + '<br/>';
        });

        $("#subResources").html(html);

        if (!isReadOnly) {
            
            $('#manage-pboe .schedule-event, #manage-pboe .planned-date').prop('disabled', false).datepicker({
                dateFormat: "mm/dd/yy",
                onSelect: function () {
                    $(this).keydown();
                    $(this).blur();
                    $(this).addClass('active');
                }
            });

            // setup hide/show of CCOPD Other Exception Text
            $('#OtherExceptionApplies').change(function() {
                if ($(this).is(':checked')) {
                    $("#OtherText").removeClass('display-none');
                }
                else {
                    $("#OtherText").addClass("display-none");
                    $("#OtherText").val("");
                }
            });

            // Show or hide the Date picker and text box for "Should Cost/Engineering Estimate" section's radio buttons
            ManageBOEFormsWidget.ShowHideDate = function(radioButtonValue, dateElement, textElement){
                // the next() hides/shows the calendar button
                if (radioButtonValue == 'NA') {
                    dateElement.addClass("display-none");
                    dateElement.next().addClass("display-none");
                    dateElement.val("");
                } else {
                    dateElement.removeClass('display-none');
                    dateElement.next().removeClass('display-none');
                }

                if (radioButtonValue == 'Planned'){
                    textElement.removeClass("display-none");
                } else {
                    textElement.val("");
                    textElement.addClass("display-none");
                }
            }

            // setup hide/show of Should Cost/Engineering Estimate Dates
            $('#ScheduleEvents input[type=radio]').change(function () { 
                ManageBOEFormsWidget.ShowHideDate(this.value, $('#' + this.name + 'Date'), $('#' + this.name + 'Text'));

                if (this.name == 'CID' || this.name == 'CostAnalysis'){
                    ManageBOEFormsWidget.ShowHidePlannedDates();
                }
            });

            // setup initial hide/show of Should Cost/Engineering Estimate Dates
            $.each($('#ScheduleEvents input[type=radio]'), function (i, item) {
                if (item.checked){
                    ManageBOEFormsWidget.ShowHideDate(item.value, $('#' + item.name + 'Date'), $('#' + item.name + 'Text'));
                }
            });

            // setup hide/show of Should Cost/Engineering Estimate Planning Dates
            ManageBOEFormsWidget.ShowHidePlannedDates = function(){
                var cid = $('#ScheduleEvents #CID:checked').val();
                var costAnalysis = $('#ScheduleEvents #CostAnalysis:checked').val();

                if (cid == 'Planned' || costAnalysis == 'Planned') {
                    $('div.planned-dates-row').removeClass('display-none');
                }else{
                    $('div.planned-dates-row').addClass('display-none');
                }
            }

            $('#manage-pboe #Poc').lookupUser({
                accountWorkPhoneElementId: 'PocPhone',
                AccountWorkPhoneInitial: '<%: Model.PBOEModel.PocPhone %>',
                accountNameElementId: 'PocNTID',
                accountDisplayNameInitial: '<%: Model.PBOEModel.Poc %>',
                enabled: true,
                readOnly: <%:ViewData["READONLY"]%>,
                fieldDisplayName: 'Poc',
                allowGroups: false,
                checkNameCallbackHandler: function(valid) {},
                onChange: function() {
                    ManageBOEFormsWidget.setDirty(); 
                }
            });

            $('#manage-pboe #Approver').lookupUser({
                accountWorkPhoneElementId: 'ApproverPhone',
                AccountWorkPhoneInitial: '<%: Model.PBOEModel.ApproverPhone %>',
                accountNameElementId: 'ApproverNTID',
                accountDisplayNameInitial: '<%: Model.PBOEModel.Approver %>',
                enabled: true,
                readOnly: <%:ViewData["READONLY"]%>,
                fieldDisplayName: 'Approver',
                allowGroups: false,
                checkNameCallbackHandler: function(valid) {},
                onChange: function() {
                    ManageBOEFormsWidget.setDirty(); 
                }
            });
            
            // Initial hide/show of planned dates
            ManageBOEFormsWidget.ShowHidePlannedDates();
        }

        ManageBOEFormsWidget.refreshModule();
    });
</script>
<div id="manage-pboe">
    <%: Html.Hidden("BOEFormId", Model.PBOEModel.BOEFormId.ToString()) %>
    <%: Html.Hidden("BOEFormType", "PBOE") %>
    <%: Html.Hidden("Version", Model.PBOEModel.Version.ToString()) %>
    <div class="form-row">
        <div class="form-label">PBOE Form Name *</div>
        <div class="form-element"><%: Html.TextBox("BOEFormName", Model.PBOEModel.BOEFormName, new { id = "BOEFormName", maxlength="200" }) %></div>
    </div>
    <div class="form-row"> 
        <div class="form-label">Select Sub Resources for inclusion in this PBOE</div>
        <div id="subResources" class="form-element"></div>
    </div>
    <div class="form-row">
        <div class="form-label">Revision *</div>
        <div class="form-element"><%: Html.TextBox("Revision", Model.PBOEModel.Revision, new { id = "Revision" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="Type in the supplier name.  If the supplier named has several business locations, specify the applicable location. In the case of an expected competitive acquisition, the supplier may be TBD and “competitive” selected in the Degree of Competition section. Ensure the name is accurate, consistent with the supplier proposal and if applicable accounting for recent mergers or acquisitions.">Supplier Name **</span></div>
        <div class="form-element"><%: Html.TextBox("SupplierName", Model.PBOEModel.SupplierName, new { id = "SupplierName", maxlength="50" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="Identify the LM Space RFP number provided to the supplier(s) for their quote(s).">LM Supplier RFP No **</span></div>
        <div class="form-element"><%: Html.TextBox("RFP", Model.PBOEModel.RFP, new { id = "RFP", maxlength="50" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="Supplier proposal number from which LM proposed price is derived (if applicable).">Supplier Proposal Number **</span></div>
        <div class="form-element"><%: Html.TextBox("ProposalNumber", Model.PBOEModel.ProposalNumber, new { id = "ProposalNumber", maxlength="50" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="Date of Supplier proposal from which LM proposed price is derived (if applicable). Ensure the supplier proposal is valid and the most current version.">Supplier Proposal Date **</span></div>
        <div class="form-element"><%: Html.TextBox("ProposalDate", Model.PBOEModel.ProposalDate, new { @class = "planned-date", id = "ProposalDate" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="Input the end date of supplier proposal validity (if applicable).">Supplier Proposal Validity Date **</span></div>
        <div class="form-element"><%: Html.TextBox("ValidityDate", Model.PBOEModel.ValidityDate, new { @class = "planned-date", id = "ValidityDate" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label"><span helptext="This is the commonly recognized name of the LM Space prime proposal and/or customer RFP number.">LM Proposal Title / Customer RFP No **</span></div>
        <% if (string.IsNullOrEmpty(Model.ProposalTitleAndRfpNumber)) { %>
            <div class="form-element"><%: Html.TextBox("ProposalTitle", string.IsNullOrWhiteSpace(Model.PBOEModel.ProposalTitle) ? Model.InitialWorkspaceTitle : Model.PBOEModel.ProposalTitle, new { id = "ProposalTitle", maxlength="200" }) %></div>
        <% } else { %>
            <div class="form-element static"><%: Model.ProposalTitleAndRfpNumber %></div>
        <% } %>
    </div>
    <div class="form-row"> 
        <div class="form-label">Supplier Contract Type per CLIN <span helptext="Select the applicable Supplier Contract Type for each CLIN. This field is required for final export of the PBOE form(s)."></span></div>
        <div class="form-element">
            <table>
                <thead>
                    <tr>
                        <td>CLIN</td>
                        <td>Supplier Contract Type</td>
                    </tr>
                </thead>
                <tbody>
                    <% int counter = 0;
                        foreach (GenBOE.Objects.FullClin clin in Model.Clins)
                        {
                            string contractTypeId = string.Format("ClinContractTypes[{0}].ContractType", counter);
                            string clinId = string.Format("ClinContractTypes[{0}].ClinId", counter);
                            %>
                    <tr>
                        <td><%: clin.ClinString %><%: Html.Hidden(clinId, clin.Id, new { id = clinId} ) %></td>
                        <%  int clinContract = -1;
                            if (Model.PBOEModel.ClinContractTypes.Any(c => c.ClinId == clin.Id)) {
                                clinContract = Model.PBOEModel.ClinContractTypes.First(c => c.ClinId == clin.Id).ContractType;
                            } %>
                        <td>
                            <select id="<%: contractTypeId %>" name="<%: contractTypeId %>">
                                <option value="-1">Not Set</option>
                                <% foreach (IES.Common.PickList.PickListDto contractType in (ICollection<IES.Common.PickList.PickListDto>)ViewData["ContractTypes"])
                                   {
                                       if (contractType.Id == clinContract)
                                       { %>
                                        <option value="<%=contractType.Id%>" selected="selected"><%=contractType.Text %></option>
                                <%      } 
                                        else if (contractType.IsActive) {%>
                                        <option value="<%=contractType.Id%>"><%=contractType.Text %></option>
                                <%      }
                                    } %>
                            </select>
                        </td>
                    </tr>
                    <% counter++;
                        } %>
                </tbody>
            </table>
        </div>
    </div>
    <div class="form-row">
        <label class="form-label"><span helptext="Provide a brief description of the task, products or services the supplier is providing with LM SOW/Specification reference as appropriate.">Task Description, Products/Services: To be Provided, including LM SOW / Specification Reference if Applicable **</span></label><br /><br />
        <div class="wrapper">
            <%: Html.TextArea("Description_PBOE", Model.PBOEModel.Description, new { @maxlength = Constants.MAX_RTE_LENGTH , onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH  + ")", id = "Description_PBOE" })%>
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label"><span helptext="Select which best represents the degree to which LM’s requirement has been competed among potential procurement sources for pricing purposes. If the source has not yet been selected or the basis for LM’s proposed procurement value is an internal estimate, shade the last box. Note: Engineering estimates/should-cost estimates may not be substituted for required supplier provided certified cost or pricing data merely to meet prime proposal submittal schedules.">Degree of Competition **</span></div>
        <div class="form-element"><%: Html.RadioButton("DegreeOfCompetition", DegreeOfCompetition.Competitive, Model.PBOEModel.DegreeOfCompetition == DegreeOfCompetition.Competitive, new { id = "DegreeOfCompetition" }) %><label>Competitive</label><br />
            <%: Html.RadioButton("DegreeOfCompetition", DegreeOfCompetition.NonCompetitive, Model.PBOEModel.DegreeOfCompetition == DegreeOfCompetition.NonCompetitive, new { id = "DegreeOfCompetition" }) %><label>Non-competitive</label><br />
            <%: Html.RadioButton("DegreeOfCompetition", DegreeOfCompetition.SourceNotSelected, Model.PBOEModel.DegreeOfCompetition == DegreeOfCompetition.SourceNotSelected, new { id = "DegreeOfCompetition" }) %><label>Source not yet selected; should cost estimate (SCE) / Engineering Estimate (EE) used</label>
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label"><span helptext="Select to indicate whether certified cost or pricing data is applicable to the procurement, or if not, which exception applies.">Certified Cost or Pricing Data (CCoPD) Applicability **</span></div>
        <div class="form-element">
            <input <%: Model.PBOEModel.CCoPDApplies ? "checked=\"checked\"" : string.Empty %> id="CCoPDApplies" name="CCoPDApplies" type="checkbox" value="true"><label>CCoPD Applies</label><br />
            <input <%: Model.PBOEModel.CommercialItemExceptionApplies ? "checked=\"checked\"" : string.Empty %> id="CommercialItemExceptionApplies" name="CommercialItemExceptionApplies" type="checkbox" value="true"><label>Commercial Item Exception Applies</label><br />
            <input <%: Model.PBOEModel.CompetitionExceptionApplies ? "checked=\"checked\"" : string.Empty %> id="CompetitionExceptionApplies" name="CompetitionExceptionApplies" type="checkbox" value="true"><label>Competition Exception Applies</label><br />
            <input <%: Model.PBOEModel.OtherExceptionApplies ? "checked=\"checked\"" : string.Empty %> id="OtherExceptionApplies" name="OtherExceptionApplies" type="checkbox" value="true"><label>Other Exception Applies (explain)</label><br />
            <div class="label-padding-left"><%: Html.TextBox("OtherText", Model.PBOEModel.OtherText, new { id = "OtherText", @class = otherTextClass, maxlength="100" }) %></div>
        </div>
    </div>
    <div class="form-row"> 
        <span class="form-label"><span>Status of Supporting Data (Select all that apply)</span></span>
        <div id="Div4" class="help-icon" onclick="$(this).parent().prev().click();"></div>
            <!-- This comment is needed for the jquery animation to work in IE8... -->
            <div class="help-dialog" style="width: 400px;">
                <div class="help-dialog-text">
                    This section of the form is intended to provide a quick glance status of the data included as support to the PBOE form and the overall procurement estimate. The supplier’s proposal and certified cost or pricing data must be included. A price analysis must be included (unless the procurement estimate is based on a should-cost or engineering estimate). Commercial Item Documentation is included unless approved for later submittal by the customer. Cost Analysis is included unless approved for later submittal by the customer.<br />
                    Note: The asterisked items in the Schedule of Events section relate to required customer approvals for items not included in initial prime submittal.
                </div>
            </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">a) Supplier proposal and supporting data included if > $13.5M or ≥ CCoPD threshold AND >10%  of the LM proposal?</div>
        <div class="form-element">
            <%: Html.RadioButton("SupplierProposalSupportingDataIncluded", TripleBooleanState.Yes, Model.PBOEModel.SupplierProposalSupportingDataIncluded == TripleBooleanState.Yes, new { id = "SupplierProposalSupportingDataIncluded" }) %><label>Yes</label><br />
            <%: Html.RadioButton("SupplierProposalSupportingDataIncluded", TripleBooleanState.NA, Model.PBOEModel.SupplierProposalSupportingDataIncluded == TripleBooleanState.NA, new { id = "SupplierProposalSupportingDataIncluded" }) %><label>N/A</label><br />
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">b) Price Analysis included?</div>
        <div class="form-element"><%: Html.RadioButton("PriceAnalysisIncluded", TripleBooleanState.Yes, Model.PBOEModel.PriceAnalysisIncluded == TripleBooleanState.Yes, new { id = "PriceAnalysisIncluded" }) %><label>Yes</label><br />
            <%: Html.RadioButton("PriceAnalysisIncluded", TripleBooleanState.NA, Model.PBOEModel.PriceAnalysisIncluded == TripleBooleanState.NA, new { id = "PriceAnalysisIncluded" }) %><label>N/A (SCE/EE)</label><br />
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">c) Commercial Item Documentation included?</div>
        <div class="form-element"><%: Html.RadioButton("CommercialItemDocIncluded", TripleBooleanState.Yes, Model.PBOEModel.CommercialItemDocIncluded == TripleBooleanState.Yes, new { id = "CommercialItemDocIncluded" }) %><label>Yes</label><br />
            <%: Html.RadioButton("CommercialItemDocIncluded", TripleBooleanState.No, Model.PBOEModel.CommercialItemDocIncluded == TripleBooleanState.No, new { id = "CommercialItemDocIncluded" }) %><label>No</label><br />
            <%: Html.RadioButton("CommercialItemDocIncluded", TripleBooleanState.NA, Model.PBOEModel.CommercialItemDocIncluded == TripleBooleanState.NA, new { id = "CommercialItemDocIncluded" }) %><label>N/A</label><br />
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">d) Is Cost Analysis included?</div>
        <div class="form-element"><%: Html.RadioButton("CostAnalysisIncluded", TripleBooleanState.Yes, Model.PBOEModel.CostAnalysisIncluded == TripleBooleanState.Yes, new { id = "CostAnalysisIncluded" }) %><label>Yes</label><br />
            <%: Html.RadioButton("CostAnalysisIncluded", TripleBooleanState.No, Model.PBOEModel.CostAnalysisIncluded == TripleBooleanState.No, new { id = "CostAnalysisIncluded" }) %><label>No</label><br />
            <%: Html.RadioButton("CostAnalysisIncluded", TripleBooleanState.NA, Model.PBOEModel.CostAnalysisIncluded == TripleBooleanState.NA, new { id = "CostAnalysisIncluded" }) %><label>N/A</label><br />
        </div>
    </div>
    <div class="form-row">
        <label class="form-label"><span>Basis of and Rationale for LM Proposed Value for Procurement **</span></label>
            <div id="Div5" class="help-icon" onclick="$(this).parent().prev().click();"></div>
            <!-- This comment is needed for the jquery animation to work in IE8... -->
            <div class="help-dialog" style="width: 400px;">
                <div class="help-dialog-text">
                    This area explains the LM proposed value for the subcontract and what it is based on and why – supplier proposed value, price analysis value, cost analysis value, should cost or engineering estimate value, or some other value.  The subcontract proposal manager is tasked with the responsibility to determine the proposed value for the procurement. Thoroughly explain how the proposed value was derived. Ensure you include in this rationale that:
                     <ul>
                        <li>Supplier performance and historical actuals, including all Requests for Equitable Adjustment (REA) have been considered in the proposed value</li>
                                     <li>It explains the methodology used in the price analysis and why the method was used for this PBOE</li>
                        <li>It describes how the proposed labor rate(s) compares with previous proposals from the same supplier</li>
                        <li>Any unique or specifically priced terms and conditions that affect the supplier proposed price</li>
                        <li>If no cost analysis, it addresses the reasonableness of the proposed Bills of Materials as well as reasonableness of travel (number/duration/location/number of personnel/per diems, etc.), including any adjustments made by the Technical Evaluation, have been considered in the proposed value</li>
                        <li>If no cost analysis, a summary table of the supplier proposal is included</li>
                        <li>The proposed value is realistic and attainable through supplier negotiations.</li>
                    </ul>
                    Note: In addition to the PBOE Form, provide as attachments to this document, the price analysis (if separately documented), engineering estimate, or should cost estimate.  Other attachments can be included as necessary, i.e. supplier’s proposals, commercial item documentation, cost analysis, Memorandum of Understanding, etc. or any other document that will assist in providing the source of the cost basis.
                    Be aware that, depending on the detail provided in this section, this completed PBOE form may become Third Party Proprietary Information and must be handled and labeled accordingly.
                </div>
            </div>
        <br /><br />
        <div class="wrapper">
            <%: Html.TextArea("BasisAndRationale_PBOE", Model.PBOEModel.BasisAndRationale, new { @maxlength = Constants.MAX_RTE_LENGTH , onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH  + ")", id = "BasisAndRationale_PBOE" })%>
        </div>
    </div>
    <div class="form-row">
        <span class="form-label"><span helptext="For each field in this section, a planned date, an actual date or “not applicable” is required. BE SURE TO KEEP THESE DATES CURRENT AS THEY CHANGE OR BECOME ACTUALS THROUGH THE CUSTOMER FACTFINDING AND NEGOTIATION PROCESS.">Schedule of Events (Date planned if not yet complete, if planned calendar date unknown, please describe (example, “ATP + 30 days”); Actual date if completed; Not applicable)</span></span>
    </div>
    <div id="ScheduleEvents">
        <div><b>Note:</b> If any of the fields below marked with &dagger; are set to planned, then the two planned dates at the bottom of this section are required.</div>
        <br />
        <div class="form-row"> 
            <div class="form-label">Should Cost/Engineering Estimate</div>
            <div class="form-element"><%: Html.RadioButton("ShouldCostEstimate", ScheduleEvent.NA, Model.PBOEModel.ShouldCostEstimate == ScheduleEvent.NA, new { id = "ShouldCostEstimate" }) %><label>N/A</label><br />
                <%: Html.RadioButton("ShouldCostEstimate", ScheduleEvent.Actual, Model.PBOEModel.ShouldCostEstimate == ScheduleEvent.Actual, new { id = "ShouldCostEstimate" }) %><label>Actual</label><br />
                <%: Html.RadioButton("ShouldCostEstimate", ScheduleEvent.Planned, Model.PBOEModel.ShouldCostEstimate == ScheduleEvent.Planned, new { id = "ShouldCostEstimate" }) %><label>Planned</label><br />
                <%: Html.TextBox("ShouldCostEstimateDate", !Model.PBOEModel.ShouldCostEstimateDate.HasValue ? string.Empty : Model.PBOEModel.ShouldCostEstimateDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "ShouldCostEstimateDate" })%><br />
                <%: Html.TextBox("ShouldCostEstimateText", Model.PBOEModel.ShouldCostEstimateText, new { id = "ShouldCostEstimateText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">SOW Written</div>
            <div class="form-element"><%: Html.RadioButton("SowWritten", ScheduleEvent.NA, Model.PBOEModel.SowWritten == ScheduleEvent.NA, new { id = "SowWritten" }) %><label>N/A</label><br />
                <%: Html.RadioButton("SowWritten", ScheduleEvent.Actual, Model.PBOEModel.SowWritten == ScheduleEvent.Actual, new { id = "SowWritten" }) %><label>Actual</label><br />
                <%: Html.RadioButton("SowWritten", ScheduleEvent.Planned, Model.PBOEModel.SowWritten == ScheduleEvent.Planned, new { id = "SowWritten" }) %><label>Planned</label><br />
                <%: Html.TextBox("SowWrittenDate", !Model.PBOEModel.SowWrittenDate.HasValue ? string.Empty : Model.PBOEModel.SowWrittenDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "SowWrittenDate" })%><br />
                <%: Html.TextBox("SowWrittenText", Model.PBOEModel.SowWrittenText, new { id = "SowWrittenText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">RFP Release to Supplier</div>
            <div class="form-element"><%: Html.RadioButton("RFPRelease", ScheduleEvent.NA, Model.PBOEModel.RFPRelease == ScheduleEvent.NA, new { id = "RFPRelease" }) %><label>N/A</label><br />
                <%: Html.RadioButton("RFPRelease", ScheduleEvent.Actual, Model.PBOEModel.RFPRelease == ScheduleEvent.Actual, new { id = "RFPRelease" }) %><label>Actual</label><br />
                <%: Html.RadioButton("RFPRelease", ScheduleEvent.Planned, Model.PBOEModel.RFPRelease == ScheduleEvent.Planned, new { id = "RFPRelease" }) %><label>Planned</label><br />
                <%: Html.TextBox("RFPReleaseDate", !Model.PBOEModel.RFPReleaseDate.HasValue  ? string.Empty : Model.PBOEModel.RFPReleaseDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "RFPReleaseDate" })%><br />
                <%: Html.TextBox("RFPReleaseText", Model.PBOEModel.RFPReleaseText, new { id = "RFPReleaseText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Firm Supplier Proposal(s) Receipt</div>
            <div class="form-element"><%: Html.RadioButton("FirmSupplierReceipt", ScheduleEvent.NA, Model.PBOEModel.FirmSupplierReceipt == ScheduleEvent.NA, new { id = "FirmSupplierReceipt" }) %><label>N/A</label><br />
                <%: Html.RadioButton("FirmSupplierReceipt", ScheduleEvent.Actual, Model.PBOEModel.FirmSupplierReceipt == ScheduleEvent.Actual, new { id = "FirmSupplierReceipt" }) %><label>Actual</label><br />
                <%: Html.RadioButton("FirmSupplierReceipt", ScheduleEvent.Planned, Model.PBOEModel.FirmSupplierReceipt == ScheduleEvent.Planned, new { id = "FirmSupplierReceipt" }) %><label>Planned</label><br />
                <%: Html.TextBox("FirmSupplierReceiptDate", !Model.PBOEModel.FirmSupplierReceiptDate.HasValue ? string.Empty : Model.PBOEModel.FirmSupplierReceiptDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "FirmSupplierReceiptDate" })%><br />
                <%: Html.TextBox("FirmSupplierReceiptText", Model.PBOEModel.FirmSupplierReceiptText, new { id = "FirmSupplierReceiptText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Source Selection</div>
            <div class="form-element"><%: Html.RadioButton("SourceSelection", ScheduleEvent.NA, Model.PBOEModel.SourceSelection == ScheduleEvent.NA, new { id = "SourceSelection" }) %><label>N/A</label><br />
                <%: Html.RadioButton("SourceSelection", ScheduleEvent.Actual, Model.PBOEModel.SourceSelection == ScheduleEvent.Actual, new { id = "SourceSelection" }) %><label>Actual</label><br />
                <%: Html.RadioButton("SourceSelection", ScheduleEvent.Planned, Model.PBOEModel.SourceSelection == ScheduleEvent.Planned, new { id = "SourceSelection" }) %><label>Planned</label><br />
                <%: Html.TextBox("SourceSelectionDate", !Model.PBOEModel.SourceSelectionDate.HasValue ? string.Empty : Model.PBOEModel.SourceSelectionDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "SourceSelectionDate" })%><br />
                <%: Html.TextBox("SourceSelectionText", Model.PBOEModel.SourceSelectionText, new { id = "SourceSelectionText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Commercial Item Documentation &dagger;</div>
            <div class="form-element"><%: Html.RadioButton("CID", ScheduleEvent.NA, Model.PBOEModel.CID == ScheduleEvent.NA, new { id = "CID" }) %><label>N/A</label><br />
                <%: Html.RadioButton("CID", ScheduleEvent.Actual, Model.PBOEModel.CID == ScheduleEvent.Actual, new { id = "CID" }) %><label>Actual</label><br />
                <%: Html.RadioButton("CID", ScheduleEvent.Planned, Model.PBOEModel.CID == ScheduleEvent.Planned, new { id = "CID" }) %><label>Planned</label><br />
                <%: Html.TextBox("CIDDate", !Model.PBOEModel.CIDDate.HasValue ? string.Empty : Model.PBOEModel.CIDDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "CIDDate" })%><br />
                <%: Html.TextBox("CIDText", Model.PBOEModel.CIDText, new { id = "CIDText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Govt. Review of Supplier Commercial<br />Data requested by LM</div>
            <div class="form-element"><%: Html.RadioButton("GovtReview", ScheduleEvent.NA, Model.PBOEModel.GovtReview == ScheduleEvent.NA, new { id = "GovtReview" }) %><label>N/A</label><br />
                <%: Html.RadioButton("GovtReview", ScheduleEvent.Actual, Model.PBOEModel.GovtReview == ScheduleEvent.Actual, new { id = "GovtReview" }) %><label>Actual</label><br />
                <%: Html.RadioButton("GovtReview", ScheduleEvent.Planned, Model.PBOEModel.GovtReview == ScheduleEvent.Planned, new { id = "GovtReview" }) %><label>Planned</label><br />
                <%: Html.TextBox("GovtReviewDate", !Model.PBOEModel.GovtReviewDate.HasValue ? string.Empty : Model.PBOEModel.GovtReviewDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "GovtReviewDate" })%><br />
                <%: Html.TextBox("GovtReviewText", Model.PBOEModel.GovtReviewText, new { id = "GovtReviewText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Price Analysis</div>
            <div class="form-element"><%: Html.RadioButton("PriceAnalysis", ScheduleEvent.NA, Model.PBOEModel.PriceAnalysis == ScheduleEvent.NA, new { id = "PriceAnalysis" }) %><label>N/A</label><br />
                <%: Html.RadioButton("PriceAnalysis", ScheduleEvent.Actual, Model.PBOEModel.PriceAnalysis == ScheduleEvent.Actual, new { id = "PriceAnalysis" }) %><label>Actual</label><br />
                <%: Html.RadioButton("PriceAnalysis", ScheduleEvent.Planned, Model.PBOEModel.PriceAnalysis == ScheduleEvent.Planned, new { id = "PriceAnalysis" }) %><label>Planned</label><br />
                <%: Html.TextBox("PriceAnalysisDate", !Model.PBOEModel.PriceAnalysisDate.HasValue ? string.Empty : Model.PBOEModel.PriceAnalysisDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "PriceAnalysisDate" })%><br />
                <%: Html.TextBox("PriceAnalysisText", Model.PBOEModel.PriceAnalysisText, new { id = "PriceAnalysisText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Technical Evaluation</div>
            <div class="form-element"><%: Html.RadioButton("TechnicalEvaluation", ScheduleEvent.NA, Model.PBOEModel.TechnicalEvaluation == ScheduleEvent.NA, new { id = "TechnicalEvaluation" }) %><label>N/A</label><br />
                <%: Html.RadioButton("TechnicalEvaluation", ScheduleEvent.Actual, Model.PBOEModel.TechnicalEvaluation == ScheduleEvent.Actual, new { id = "TechnicalEvaluation" }) %><label>Actual</label><br />
                <%: Html.RadioButton("TechnicalEvaluation", ScheduleEvent.Planned, Model.PBOEModel.TechnicalEvaluation == ScheduleEvent.Planned, new { id = "TechnicalEvaluation" }) %><label>Planned</label><br />
                <%: Html.TextBox("TechnicalEvaluationDate", !Model.PBOEModel.TechnicalEvaluationDate.HasValue ? string.Empty : Model.PBOEModel.TechnicalEvaluationDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "TechnicalEvaluationDate" })%><br />
                <%: Html.TextBox("TechnicalEvaluationText", Model.PBOEModel.TechnicalEvaluationText, new { id = "TechnicalEvaluationText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Fact Finding</div>
            <div class="form-element"><%: Html.RadioButton("FactFinding", ScheduleEvent.NA, Model.PBOEModel.FactFinding == ScheduleEvent.NA, new { id = "FactFinding" }) %><label>N/A</label><br />
                <%: Html.RadioButton("FactFinding", ScheduleEvent.Actual, Model.PBOEModel.FactFinding == ScheduleEvent.Actual, new { id = "FactFinding" }) %><label>Actual</label><br />
                <%: Html.RadioButton("FactFinding", ScheduleEvent.Planned, Model.PBOEModel.FactFinding == ScheduleEvent.Planned, new { id = "FactFinding" }) %><label>Planned</label><br />
                <%: Html.TextBox("FactFindingDate", !Model.PBOEModel.FactFindingDate.HasValue ? string.Empty : Model.PBOEModel.FactFindingDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "FactFindingDate" })%><br />
                <%: Html.TextBox("FactFindingText", Model.PBOEModel.FactFindingText, new { id = "FactFindingText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Cost Analysis &dagger;</div>
            <div class="form-element"><%: Html.RadioButton("CostAnalysis", ScheduleEvent.NA, Model.PBOEModel.CostAnalysis == ScheduleEvent.NA, new { id = "CostAnalysis" }) %><label>N/A</label><br />
                <%: Html.RadioButton("CostAnalysis", ScheduleEvent.Actual, Model.PBOEModel.CostAnalysis == ScheduleEvent.Actual, new { id = "CostAnalysis" }) %><label>Actual</label><br />
                <%: Html.RadioButton("CostAnalysis", ScheduleEvent.Planned, Model.PBOEModel.CostAnalysis == ScheduleEvent.Planned, new { id = "CostAnalysis" }) %><label>Planned</label><br />
                <%: Html.TextBox("CostAnalysisDate", !Model.PBOEModel.CostAnalysisDate.HasValue ? string.Empty : Model.PBOEModel.CostAnalysisDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "CostAnalysisDate" })%><br />
                <%: Html.TextBox("CostAnalysisText", Model.PBOEModel.CostAnalysisText, new { id = "CostAnalysisText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Govt. Pricing Assistance for CCoPD<br />Review requested by LM</div>
            <div class="form-element"><%: Html.RadioButton("GovtPricing", ScheduleEvent.NA, Model.PBOEModel.GovtPricing == ScheduleEvent.NA, new { id = "GovtPricing" }) %><label>N/A</label><br />
                <%: Html.RadioButton("GovtPricing", ScheduleEvent.Actual, Model.PBOEModel.GovtPricing == ScheduleEvent.Actual, new { id = "GovtPricing" }) %><label>Actual</label><br />
                <%: Html.RadioButton("GovtPricing", ScheduleEvent.Planned, Model.PBOEModel.GovtPricing == ScheduleEvent.Planned, new { id = "GovtPricing" }) %><label>Planned</label><br />
                <%: Html.TextBox("GovtPricingDate", !Model.PBOEModel.GovtPricingDate.HasValue ? string.Empty : Model.PBOEModel.GovtPricingDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "GovtPricingDate" })%><br />
                <%: Html.TextBox("GovtPricingText", Model.PBOEModel.GovtPricingText, new { id = "GovtPricingText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Supplier Negotiations Complete</div>
            <div class="form-element"><%: Html.RadioButton("SupplierNegotiations", ScheduleEvent.NA, Model.PBOEModel.SupplierNegotiations == ScheduleEvent.NA, new { id = "SupplierNegotiations" }) %><label>N/A</label><br />
                <%: Html.RadioButton("SupplierNegotiations", ScheduleEvent.Actual, Model.PBOEModel.SupplierNegotiations == ScheduleEvent.Actual, new { id = "SupplierNegotiations" }) %><label>Actual</label><br />
                <%: Html.RadioButton("SupplierNegotiations", ScheduleEvent.Planned, Model.PBOEModel.SupplierNegotiations == ScheduleEvent.Planned, new { id = "SupplierNegotiations" }) %><label>Planned</label><br />
                <%: Html.TextBox("SupplierNegotiationsDate", !Model.PBOEModel.SupplierNegotiationsDate.HasValue ? string.Empty : Model.PBOEModel.SupplierNegotiationsDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "SupplierNegotiationsDate" })%><br />
                <%: Html.TextBox("SupplierNegotiationsText", Model.PBOEModel.SupplierNegotiationsText, new { id = "SupplierNegotiationsText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Memorandum of Understanding</div>
            <div class="form-element"><%: Html.RadioButton("MOU", ScheduleEvent.NA, Model.PBOEModel.MOU == ScheduleEvent.NA, new { id = "MOU" }) %><label>N/A</label><br />
                <%: Html.RadioButton("MOU", ScheduleEvent.Actual, Model.PBOEModel.MOU == ScheduleEvent.Actual, new { id = "MOU" }) %><label>Actual</label><br />
                <%: Html.RadioButton("MOU", ScheduleEvent.Planned, Model.PBOEModel.MOU == ScheduleEvent.Planned, new { id = "MOU" }) %><label>Planned</label><br />
                <%: Html.TextBox("MOUDate", !Model.PBOEModel.MOUDate.HasValue ? string.Empty : Model.PBOEModel.MOUDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "MOUDate" })%><br />
                <%: Html.TextBox("MOUText", Model.PBOEModel.MOUText, new { id = "MOUText", maxlength="50" })%>
            </div>
        </div>
        <div class="form-row"> 
            <div class="form-label">Procurement Definitization</div>
            <div class="form-element"><%: Html.RadioButton("Procurement", ScheduleEvent.NA, Model.PBOEModel.Procurement == ScheduleEvent.NA, new { id = "Procurement" }) %><label>N/A</label><br />
                <%: Html.RadioButton("Procurement", ScheduleEvent.Actual, Model.PBOEModel.Procurement == ScheduleEvent.Actual, new { id = "Procurement" }) %><label>Actual</label><br />
                <%: Html.RadioButton("Procurement", ScheduleEvent.Planned, Model.PBOEModel.Procurement == ScheduleEvent.Planned, new { id = "Procurement" }) %><label>Planned</label><br />
                <%: Html.TextBox("ProcurementDate", !Model.PBOEModel.ProcurementDate.HasValue ? string.Empty : Model.PBOEModel.ProcurementDate.Value.ToString("MM/dd/yyyy"), new { @class="schedule-event", id = "ProcurementDate" })%><br />
                <%: Html.TextBox("ProcurementText", Model.PBOEModel.ProcurementText, new { id = "ProcurementText", maxlength="50" })%>
            </div>
        </div>
    </div>
    <div class="form-row planned-dates-row">
        <div class="form-label">&dagger; If planned date (subcontracts only):</div>
    </div>
    <div class="form-row planned-dates-row"> 
        <div class="form-label label-padding-left">a) Date of Customer Written Approval for Submission after Initial Prime Proposal *</div>
        <div class="form-element"><%: Html.TextBox("PlannedDate_WrittenApproval", !Model.PBOEModel.PlannedDate_WrittenApproval.HasValue ? string.Empty : Model.PBOEModel.PlannedDate_WrittenApproval.Value.ToString("MM/dd/yyyy"), new { @class="planned-date", id = "PlannedDate_WrittenApproval" })%>
        </div>
    </div>
    <div class="form-row planned-dates-row"> 
        <div class="form-label label-padding-left">b) Approved Date for Submission to Customer *</div>
        <div class="form-element"><%: Html.TextBox("PlannedDate_ApprovedSubmission", !Model.PBOEModel.PlannedDate_ApprovedSubmission.HasValue ? string.Empty : Model.PBOEModel.PlannedDate_ApprovedSubmission.Value.ToString("MM/dd/yyyy"), new { @class="planned-date", id = "PlannedDate_ApprovedSubmission" })%>
        </div>
    </div>
    <div class="form-row"> 
        <span class="form-label"><span helptext="The first POC is the buyer or subcontract administrator who is responsible as the LM POC with the supplier. The second POC is the Subcontract Proposal Manager, the person with the overall responsibility for the subcontract portion of LM’s proposal. These signatures do not indicate approval levels; they merely identify individuals knowledgeable about the content of the PBOE.">Points of Contact</span></span>
    </div>
    <div class="form-row"> 
        <div class="form-label">Subcontract Administrator / Buyer Name **</div>
        <div class="form-element"><%: Html.TextBox("Poc", Model.PBOEModel.Poc, new { id = "Poc" })%>
            <%: Html.Hidden("PocNTID", Model.PBOEModel.Poc, new { id = "PocNTID" })%>
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">Subcontract Administrator / Buyer Phone **</div>
        <div class="form-element"><%: Html.TextBox("PocPhone", Model.PBOEModel.PocPhone, new { id = "PocPhone", @readonly="readonly" })%>
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">Subcontract Proposal Manager Name **</div>
        <div class="form-element"><%: Html.TextBox("Approver", Model.PBOEModel.Approver, new { id = "Approver" })%>
            <%: Html.Hidden("ApproverNTID", Model.PBOEModel.Approver, new { id = "ApproverNTID" })%>
        </div>
    </div>
    <div class="form-row"> 
        <div class="form-label">Subcontract Proposal Manager Phone **</div>
        <div class="form-element"><%: Html.TextBox("ApproverPhone", Model.PBOEModel.ApproverPhone, new { id = "ApproverPhone", @readonly="readonly" })%>
        </div>
    </div>
    <div>* required for saving as draft<br />** required for validating and exporting (allows partial save)</div>
    <br />
</div>