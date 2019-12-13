<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.ImportWbsResultsModelView>>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>

<% bool errorsOccurred = ViewData.ContainsKey("ERRORS_OCCURRED") ? (bool)ViewData["ERRORS_OCCURRED"] : false; %>

<%: Scripts.Render("~/bundles/verification") %>

<script type="text/javascript">
    $(function () {
        WbsImportVerificationWidget = {};
        WbsImportVerificationWidget.data = {};

        WbsImportVerificationWidget.data.importResults = <%= ViewData["SERIALIZED_DATA"] %>;
        WbsImportVerificationWidget.invalidData = <%:errorsOccurred || !Model.Any() ? "true" : "false"%>;
        
    });
</script>

<div id="WbsImportVerification" class="import-verification">
    <div class="import-results">
        <div <% if (Model.Any()) { %> class="display-none"<% } %>>
            <div>No updates from the import file. Click Back to import a different file or close this dialog window to not import a file.</div>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.CreateWbs).Any()) { %> class="display-none"<% } %>>
            <div>The import file will make the following updates. To continue with the import, click Complete Import, otherwise click Back to import a different file or close this dialog window to not import a file.</div>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.CreateWbs).Count() %> WBS elements will be added:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.CreateWbs)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.UpdateWbs).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.UpdateWbs).Count() %> WBS elements will be updated. If the WBS # and/or WBS Title was updated, Authors of BOEs associated with those WBS elements will be notified. BOEs associated with these WBS elements that are Awaiting Approval/Approved will be moved back to Draft.</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.UpdateWbs)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.CreateBoe).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.CreateBoe).Count() %> BOEs will be created because new CLINs were added to WBS elements  that have the Create BOE checkbox selected for them in genBOE:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.CreateBoe)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %>, <%: result.ClinNumber %>&nbsp;<%: result.ClinTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.MissingNumberOrTitle).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.MissingNumberOrTitle).Count() %> WBS elements will not be added/updated because a WBS # and/or WBS Title is missing</div>
            <ul><li>See import file for missing WBS #’s and/or WBS Titles.</li></ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsNumber).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsNumber).Count() %> WBS elements will not be added/updated because the WBS # must be unique:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsNumber)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsID).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsID).Count() %> WBS elements will not be added/updated because the genBOE WBS ID must be unique:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.NonUniqueWbsID)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.TruncateTitle).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.TruncateTitle).Count() %> WBS elements will have their titles truncated because they exceed 100 characters:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.TruncateTitle)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.InvalidWbsNumberFormat).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.InvalidWbsNumberFormat).Count() %> WBS elements will not be added/updated because the WBS # ends in a period, has more than 10 levels, has more than 5 characters for each level, and/or is more than 30 characters:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.InvalidWbsNumberFormat)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.ClinsDoNotExist).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.ClinsDoNotExist).Distinct().Count() %> CLINs will not be associated to WBS elements because they do not exist in genBOE:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.ClinsDoNotExist)) { %>
                <li><%: result.ClinNumber %>&nbsp;<%: result.ClinTitle %></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.CircularReferences).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.CircularReferences).Distinct().Count()%> WBSs will not be added/updated because they will cause circular references:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.CircularReferences))
                   { %>
                <li><%: result.WbsNumber%>&nbsp;<%: result.WbsTitle%></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.InUseClinRemoved).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.InUseClinRemoved).Distinct().Count()%> CLINs will not be removed from WBSs because they are in use:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.InUseClinRemoved))
                   { %>
                <li><%: result.WbsNumber%>&nbsp;<%: result.WbsTitle%>, <%: result.ClinNumber %>&nbsp;<%: result.ClinTitle %></li>
                <% } %>
            </ul> 
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.DeleteWbs).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.DeleteWbs).Count() %> WBS elements will be deleted. If a WBS # was deleted from the imported Excel spreadsheet, Authors of BOEs associated with those WBS elements will be notified.</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.DeleteWbs)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (!Model.Where(m => m.ImportType == (int)WbsImportResult.WbsInUse).Any()) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)WbsImportResult.WbsInUse).Count() %> WBS elements will not be deleted because they are in use:</div>
            <ul>
                <% foreach (ImportWbsResultsModelView result in Model.Where(m => m.ImportType == (int)WbsImportResult.WbsInUse)) { %>
                <li><%: result.WbsNumber %>&nbsp;<%: result.WbsTitle %></li>
                <% } %>
            </ul>
        </div>
    </div>
</div>
