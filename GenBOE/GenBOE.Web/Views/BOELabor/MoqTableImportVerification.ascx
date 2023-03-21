<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.ImportMoqTableResultsModelView>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>
<%@ Import Namespace="IES.Common.classes" %>

<% bool errorsOccurred = ViewData.ContainsKey("ERRORS_OCCURRED") ? (bool)ViewData["ERRORS_OCCURRED"] : false; %>

<%: Scripts.Render("~/bundles/verification") %>

<script type="text/javascript">
    $(function () {
        MoqTableImportVerificationWidget = {};
        MoqTableImportVerificationWidget.data = {};

        MoqTableImportVerificationWidget.data.importResults = <%= ViewData["SERIALIZED_DATA"] %>;
        MoqTableImportVerificationWidget.invalidData = <%:errorsOccurred || !Model.Any() ? "true" : "false"%>;
    });
</script>

<div id="MoqTableImportVerification" class="import-verification">
    <% if (Model.Count() > 0) { %>
    <div class="moq-table-import-results">
        <div>The import file will make the following updates. To continue with the import, click Complete Import, otherwise click Back to import a different file or close this dialog window to not import a file.</div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.CreateMoqTable).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.CreateMoqTable).Count()%> MOQ Table(s) will be added:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.CreateMoqTable))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.MissingTableName).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.MissingTableName).Count()%> MOQ Table(s) will not be added because a Table Name is missing:</div>
            <ul><li>See import file for missing Table Name values.</li></ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.MissingRequiredField).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.MissingRequiredField).Count()%> MOQ Table(s) will not be added because one or more required fields are missing:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.MissingRequiredField))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeTableName).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeTableName).Count()%> MOQ Table(s) will not be added because the Table Name is over the maximum <%: Constants.MOQ_TYPE_TEXT_FIELD_LENGTH %> characters:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeTableName))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeRepositoryName).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeRepositoryName).Count()%> MOQ Table(s) will not be added because the Repository Name is over the maximum <%: Constants.MOQ_REPOSITORY_NAME_FIELD_LENGTH %> characters:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeRepositoryName))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidQueryType).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidQueryType).Count()%> MOQ Table(s) will not be added because the Query Type must be <%: MoqTableData.MONTHLY %> or <%: MoqTableData.WEEKLY %>:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidQueryType))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeContractNumber).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeContractNumber).Count()%> MOQ Table(s) will not be added because the Contract Number is over the maximum <%: Constants.MOQ_TYPE_TEXT_FIELD_LENGTH %> characters:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeContractNumber))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidDateOfReport).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidDateOfReport).Count()%> MOQ Table(s) will not be added because the Date of Report must be a valid date and be on or before today's date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidDateOfReport))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeHistoricalProgramName).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeHistoricalProgramName).Count()%> MOQ Table(s) will not be added because the Historical Program Name is over the maximum <%: Constants.MOQ_HISTORICAL_PROG_NAME_FIELD_LENGTH %> characters:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeHistoricalProgramName))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeWBSElement).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeWBSElement).Count()%> MOQ Table(s) will not be added because the WBS/WBS Element is over the maximum <%: SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST && !Model.First().WsSapConnectionEnabled ? Constants.MOQ_WBS_ELEMENT_RMS_SAP_DISABLED_FIELD_LENGTH : Constants.MOQ_WBS_ELEMENT_FIELD_LENGTH %> characters:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.LargeWBSElement))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStart).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStart).Count()%> MOQ Table(s) will not be added because the PoP Start Date must be a valid date and on or before today's date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStart))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartSunday).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartSunday).Count()%> MOQ Table(s) will not be added because the PoP Start Date must be a valid date on a Sunday and on or before today's date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartSunday))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartFW).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartFW).Count()%> MOQ Table(s) will not be added because the PoP Start Date must be a valid Fiscal Week (fw/yyyy) with a week value of 1-53:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopStartFW))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEnd).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEnd).Count()%> MOQ Table(s) will not be added because the PoP End Date must be a valid date and be on or before today's date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEnd))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
         <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndSunday).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndSunday).Count()%> MOQ Table(s) will not be added because the PoP End Date must be a valid date on a Sunday and be on or before today's date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndSunday))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndFW).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndFW).Count()%> MOQ Table(s) will not be added because the PoP End Date must a valid Fiscal Week (fw/yyyy) with a week value of 1-53:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopEndFW))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopRange).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopRange).Count()%> MOQ Table(s) will not be added because the PoP Start Date must be on or before the PoP End date:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidPopRange))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalRelevantHours).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalRelevantHours).Count()%> MOQ Table(s) will not be added because the Total Relevant Hours must be a numerical value:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalRelevantHours))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
		 <div <% if (Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalWbsHours).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalWbsHours).Count()%> MOQ Table(s) will not be added because the Total Wbs Hours must be a numerical value:</div>
            <ul>
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidTotalWbsHours))
                   { %>
                <li><%: result.TableName %></li>
                <% } %>
            </ul>
        </div>
		<div <% if (!Model.Any(m => m.ImportType == (int)MoqTableImportType.InvalidSapCalculation)) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidSapCalculation).Count()%> MOQ Table(s) will not be added because the Table did not pass SAP Validation/Calculation:</div>
            
                <% foreach (ImportMoqTableResultsModelView result in Model.Where(m => m.ImportType == (int)MoqTableImportType.InvalidSapCalculation))
                   { %>
                <ul><strong>Table Name:</strong> <%: result.TableName %>
						<% foreach(string errorMessage in result.ErrorMessages) 
						{  %>
							<li><%: errorMessage %></li>
						<% } %>
				</ul>
				<br />
                <% } %>
            
        </div>
    </div>
    <% } else { %>
        <div class="moq-table-import-results">
            <div class="title">
                There are no changes to import.
                <%if(errorsOccurred) { %>
                    <div class="error">The file contains invalid data. Please review the file. If you cannot identify the issue, please contact the administrator.</div>
                <%} %>
            </div>
        </div>
    <% } %>
</div>
