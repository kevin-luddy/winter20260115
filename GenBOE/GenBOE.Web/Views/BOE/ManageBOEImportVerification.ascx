<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.ImportBoeResultsModelView>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>

<% bool errorsOccurred = ViewData.ContainsKey("ERRORS_OCCURRED") ? (bool)ViewData["ERRORS_OCCURRED"] : false; %>

<%: Scripts.Render("~/bundles/verification") %>

<script type="text/javascript">
    $(function () {
        ManageBoeImportVerificationWidget = {};
        ManageBoeImportVerificationWidget.data = {};

        ManageBoeImportVerificationWidget.data.importResults = <%= ViewData["SERIALIZED_DATA"] %>;
        ManageBoeImportVerificationWidget.invalidData = <%:errorsOccurred || !Model.Any() ? "true" : "false"%>;
    });
</script>

<div id="WbsImportVerification" class="import-verification">
    <% if (Model.Count() > 0) { %>
    <div class="import-results">
        <div>The import file will make the following updates. To continue with the import, click Complete Import, otherwise click Back to import a different file or close this dialog window to not import a file.</div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.CreateBoe).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.CreateBoe).Count()%> BOEs will be added:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.CreateBoe))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.UpdateBoe).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.UpdateBoe).Count()%> BOEs will be updated. The BOE Authors will be notified. BOEs that are Awaiting Approval/Approved will be moved back to Draft.</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.UpdateBoe))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.DeleteBoe).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.DeleteBoe).Count()%> BOEs will be deleted. The BOE Authors and Approvers will be notified.</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.DeleteBoe))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.MissingWbsClinAuthorOrApproverOrMaterial).Count()%> BOEs will not be added/updated because a WBS/CLIN, Material, Author and/or Approver is missing:</div>
            <ul><li>See import file for missing values.</li></ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorAndApproverAreTheSame).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorAndApproverAreTheSame).Count()%> BOEs will not be added/updated because Author and Approver cannot be the same user:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorAndApproverAreTheSame))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.WbsDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.WbsDoesNotExist).Count()%> BOEs will not be added/updated because the WBS does not exist:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.WbsDoesNotExist))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.ClinDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.ClinDoesNotExist).Count()%> BOEs will not be added/updated because the CLIN does not exist:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.ClinDoesNotExist))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidMaterial).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidMaterial).Count()%> BOEs will not be added/updated because the Material is an invalid entry:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidMaterial))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorDoesNotExist).Count()%> BOEs will not be added/updated because the Author does not exist:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.AuthorDoesNotExist))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.SubcontractorAuthorDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.SubcontractorAuthorDoesNotExist).Count()%> BOEs will not be added/updated because the Subcontractor Author does not exist:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.SubcontractorAuthorDoesNotExist))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.ApproverDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.ApproverDoesNotExist).Count()%> BOEs will not be added/updated because the Approver does not exist:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.ApproverDoesNotExist))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.NonUniqueBoeID).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.NonUniqueBoeID).Distinct().Count()%> BOEs will not be added/updated because the genBOE BOE ID must be unique:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.NonUniqueBoeID))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDate).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDate).Distinct().Count()%> BOEs will not be added/updated because the Start Date must be on or after the CLIN Start Date if it exists or the Contract Start Date:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDate))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDateFormat).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDateFormat).Distinct().Count()%> BOEs will not be added/updated because the Start Date is an invalid format:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidStartDateFormat))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDate).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDate).Distinct().Count()%> BOEs will not be added/updated because the End Date must be on or before the CLIN End Date if it exists or the Contract End Date:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDate))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDateFormat).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDateFormat).Distinct().Count()%> BOEs will not be added/updated because the End Date is an invalid format:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.InvalidEndDateFormat))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.BoeAlreadyExists).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.BoeAlreadyExists).Distinct().Count()%> BOEs will not be added/updated because a BOE already exists or will exist for it at a summary WBS level or at a lower WBS level:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.BoeAlreadyExists))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.CircularReferences).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.CircularReferences).Distinct().Count()%> BOEs will not be added/updated because they will cause circular references:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.CircularReferences))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.BoeCannotBeChangedToMaterial).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.BoeCannotBeChangedToMaterial).Distinct().Count()%> BOEs will not be added/updated because BOE cannot be changed to a Material BOE:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.BoeCannotBeChangedToMaterial))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.MaterialBoeCannotBeChanged).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.MaterialBoeCannotBeChanged).Distinct().Count()%> BOEs will not be updated because BOE cannot be changed from a Material BOE:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.MaterialBoeCannotBeChanged))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.WBSDoesNotExistForMaterial).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.WBSDoesNotExistForMaterial).Distinct().Count()%> BOEs will not be added/updated because WBS must be populated when Materials exist within a BOE:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.WBSDoesNotExistForMaterial))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToMaterialBoe).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToMaterialBoe).Distinct().Count()%> BOEs will not be added/updated due to Subcontractor restrictions on Material BOEs:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToMaterialBoe))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToExistingBoe).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToExistingBoe).Distinct().Count()%> BOEs will not be added/updated due to Subcontractor restrictions on existing BOEs:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.CannotAssignSubAuthorToExistingBoe))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)BoeImportResult.WorkspaceNotInWorkingState).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)BoeImportResult.WorkspaceNotInWorkingState).Count()%> BOEs will not be added/updated because the workspace must be in the Working state, your workspace is in the Initialization state. Unable to perform action:</div>
            <ul>
                <% foreach (ImportBoeResultsModelView result in Model.Where(m => m.ImportType == (int)BoeImportResult.WorkspaceNotInWorkingState))
                   { %>
                <li><%: result.WbsString%>&nbsp;<%: result.ClinString%></li>
                <% } %>
            </ul>
        </div>
    </div>
    <% } else  { %>
    <div class="import-results"><div class="title">There are no changes to import.</div></div>
    <% } %>
</div>
