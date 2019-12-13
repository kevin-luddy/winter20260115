<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.BOEFormMasterModelView>" %>
<% 
    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    $(function() {
        var isReadOnly = '<%:ViewData["READONLY"]%>'.isTrue();
        
        // Setup IWTA Resources.
        var html = "";
        var selectedResources = <%= serializer.Serialize(Model.IBOEModel.ResourceIds)%>;

        $.each(ManageBOEFormsWidget.IWTAResources, function (i, item) {
            var checked = ' checked="checked"';
            var disabled = isReadOnly ? ' disabled="disabled"' : '';
            if (selectedResources.indexOf(item.Id) < 0) {
                checked = '';
            }

            if (ManageBOEFormsWidget.IWTAInUseResourceIds.indexOf(item.Id) >= 0) {
                // this resource is in-use in another BOE Form, disable it
                disabled = ' disabled="disabled"';
                checked = '';
            }
                
            html += '<input type="checkbox" name="ResourceIds" value="' + item.Id + '" ' + checked + disabled + ' /> ' + item.ResourceDesc + '<br/>';
        });

        $("#iboeResources").html(html);

        if (!isReadOnly) {
            
            $('#manage-iboe .planned-date').prop('disabled', false).datepicker({
                dateFormat: "mm/dd/yy",
                onSelect: function () {
                    $(this).keydown();
                    $(this).blur();
                    $(this).addClass('active');
                }
            });

            $('#Poc').lookupUser({
                accountWorkPhoneElementId: 'PocPhone',
                AccountWorkPhoneInitial: '<%: Model.IBOEModel.PocPhone %>',
                accountNameElementId: 'PocNTID',
                accountDisplayNameInitial: '<%: Model.IBOEModel.Poc %>',
                enabled: true,
                readOnly: <%:ViewData["READONLY"]%>,
                fieldDisplayName: 'Poc',
                allowGroups: false,
                checkNameCallbackHandler: function(valid) {},
                onChange: function() {
                    ManageBOEFormsWidget.setDirty(); 
                }
            });

            $('#Approver').lookupUser({
                accountWorkPhoneElementId: 'ApproverPhone',
                AccountWorkPhoneInitial: '<%: Model.IBOEModel.ApproverPhone %>',
                accountNameElementId: 'ApproverNTID',
                accountDisplayNameInitial: '<%: Model.IBOEModel.Approver %>',
                enabled: true,
                readOnly: <%:ViewData["READONLY"]%>,
                fieldDisplayName: 'Approver',
                allowGroups: false,
                checkNameCallbackHandler: function(valid) {},
                onChange: function() {
                    ManageBOEFormsWidget.setDirty(); 
                }
            });
        }

    });
</script>
<div id="manage-iboe">
    <%: Html.Hidden("BOEFormId", Model.IBOEModel.BOEFormId.ToString()) %>
    <%: Html.Hidden("BOEFormType", "IBOE") %>
    <%: Html.Hidden("Version",  Model.IBOEModel.Version.ToString()) %>
    <div class="form-row">
        <div class="form-label">IBOE Form Name *</div>
        <div class="form-element"><%: Html.TextBox("BOEFormName", Model.IBOEModel.BOEFormName, new { id = "BOEFormName", maxlength="200" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label">Select IWTA Resources for inclusion in this IBOE</div>
        <div id="iboeResources" class="form-element"></div>
    </div>
    <div class="form-row">
        <div class="form-label">Revision *</div>
        <div class="form-element"><%: Html.TextBox("Revision", Model.IBOEModel.Revision, new { id = "Revision"}) %></div>
    </div>
    <div class="form-row">
        <div class="form-label">IWTA Business Area **</div>
        <div class="form-element"><%: Html.TextBox("BusinessArea", Model.IBOEModel.BusinessArea, new { id = "BusinessArea", maxlength="50" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label">IWTA Proposal Date **</div>
        <div class="form-element"><%: Html.TextBox("ProposalDate", Model.IBOEModel.ProposalDate, new { @class="planned-date", id = "ProposalDate" }) %></div>
    </div>
    <div class="form-row">
        <div class="form-label">LM Space Proposal Title / Customer RFP No **</div>
        <% if (string.IsNullOrEmpty(Model.ProposalTitleAndRfpNumber)) { %>
            <div class="form-element"><%: Html.TextBox("ProposalTitle", string.IsNullOrWhiteSpace(Model.IBOEModel.ProposalTitle) ? Model.InitialWorkspaceTitle : Model.IBOEModel.ProposalTitle, new { id = "ProposalTitle", maxlength = "200" }) %></div>
        <% } else { %>
            <div class="form-element static"><%: Model.ProposalTitleAndRfpNumber %></div>
        <% } %>
    </div>
    <div class="form-row"> 
        <div class="form-label">IWTA Contract Type per CLIN <span helptext="Select the applicable IWTA Contract Type for each CLIN. This field is required for final export of the IBOE form(s)."></span></div>
        <div class="form-element">
            <table>
                <thead>
                    <tr>
                        <td>CLIN</td>
                        <td>IWTA Contract Type</td>
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
                            if (Model.IBOEModel.ClinContractTypes.Any(c => c.ClinId == clin.Id)) {
                                clinContract = Model.IBOEModel.ClinContractTypes.First(c => c.ClinId == clin.Id).ContractType;
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
        <label class="form-label">Task Description, Products / Services to be Provided, LM Space SOW reference, etc. **</label><br /><br />
        <div class="wrapper">
            <%: Html.TextArea("Description_IBOE", Model.IBOEModel.Description, new { @maxlength = Constants.MAX_RTE_LENGTH , onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH  + ")", id = "Description_IBOE" })%>
        </div>
    </div>
    <div class="form-row">
        <label class="form-label">Basis of and Rationale for LM Space Proposed Value for IWTA **</label><br /><br />
        <div class="wrapper">
            <%: Html.TextArea("BasisAndRationale_IBOE", Model.IBOEModel.BasisAndRationale, new { @maxlength = Constants.MAX_RTE_LENGTH , onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH + ")", id = "BasisAndRationale_IBOE" })%>
        </div>
    </div>
    <div class="form-row"> 
        <span class="form-label">Points of Contact</span>
    </div>
    <div class="form-row">
        <div class="form-label">Prepared By **</div>
        <div class="form-element">
            <%: Html.TextBox("Poc", Model.IBOEModel.Poc, new { id = "Poc" })%>
            <%: Html.Hidden("PocNTID", Model.IBOEModel.Poc, new { id = "PocNTID" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Prepared By Phone **</div>
        <div class="form-element"><%: Html.TextBox("PocPhone", Model.IBOEModel.PocPhone, new { id = "PocPhone", @readonly="readonly" })%></div>
    </div>
    <div class="form-row">
        <div class="form-label">Approved By **</div>
        <div class="form-element">
            <%: Html.TextBox("Approver", Model.IBOEModel.Approver, new { id = "Approver" })%>
            <%: Html.Hidden("ApproverNTID", Model.IBOEModel.Approver, new { id = "ApproverNTID" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label">Approved By Phone **</div>
        <div class="form-element"><%: Html.TextBox("ApproverPhone", Model.IBOEModel.ApproverPhone, new { id = "ApproverPhone", @readonly="readonly"  })%></div>
    </div>
    <div>* required for saving as draft<br />** required for validating and exporting (allows partial save)</div>
    <br />
</div>