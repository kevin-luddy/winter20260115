<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<ImportedTMResourceRate>>" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>


<script type="text/javascript">

        (function() {
            var toReturn={}
            toReturn.imported = <%= ViewData["SERIALIZED_DATA"] %>;
            toReturn.importTypeString = "<%= ViewData["IMPORT_TYPE"] %>";
            window.parent.ResourceRatesTMWidget.setImportData(toReturn);
        })();
    
</script>

    <% IEnumerable<String> errors = (IEnumerable<String>)ViewData["ERRORS"];

       if (errors.Count() != 0)
       {
           %><br />Errors occurred<BR /><%
           foreach (String error in errors)
           {
               %><%:error %><BR /><%
           }
       }
       else
       {
        %>


    <% if (Model.Count == 0)
       {%>
             <div class="title">
                No results
            </div>
    <%}
       else
       { %>

    <div>
    <div>The import file will make the following updates. To continue with the import, click Complete Import, otherwise click Back to import a different file or close this dialog window to not import a file.</div>
    
    <div class="import-results-area">
    <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Added)).Count()==0) { %> class="display-none"<% } %>>
        <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Added)).Count()%> T&M Resource Rates will be added:</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Added)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
     <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Updated)).Count()==0) { %> class="display-none"<% } %>>
         <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Updated)).Count()%> T&M Resource Rates will be updated:</div>
           <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.Updated)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
       </div>
     <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.NotUpdateable)).Count()==0) { %> class="display-none"<% } %>>
         <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.NotUpdateable)).Count()%> T&M Resource Rates will not be imported because they already exist:</div>
           <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.NotUpdateable)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
       </div>
     <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingCol)).Count()==0) { %> class="display-none"<% } %>>
         <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingCol)).Count()%> T&M Resource Rates will not be added because a Resource ID, Start Date, End Date, or Rate is missing in the file: </div>
           <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingCol)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>

      <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingResource)).Count()==0) { %> class="display-none"<% } %>>
         <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingResource)).Count()%> T&M Resource Rates will not be added because the Resource ID does not exist:</div>
           <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingResource)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidStartDateFormat)).Count()==0) { %> class="display-none"<% } %>>
             <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidStartDateFormat)).Count()%> T&M Resource Rates will not be added because the Start Date format is neither mm/yyyy nor mm/dd/yyyy</div>
               <ul>
                    <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidStartDateFormat)))
                       { %>
                    <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                    <% } %>
                </ul>
          </div>
           <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidEndDateFormat)).Count()==0) { %> class="display-none"<% } %>>
             <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidEndDateFormat)).Count()%> T&M Resource Rates will not be added because the End Date format is neither mm/yyyy nor mm/dd/yyyy</div>
               <ul>
                    <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidEndDateFormat)))
                       { %>
                    <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                    <% } %>
                </ul>
          </div>
      <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidRateFormat)).Count()==0) { %> class="display-none"<% } %>>
           <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidRateFormat)).Count()%> T&M Resource Rates will not be added/updated because the Rate $ being imported is not between .01 - 9999.99</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidRateFormat)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.RateString%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.OverlappingDateRanges)).Count()==0) { %> class="display-none"<% } %>>
          <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.OverlappingDateRanges)).Count()%> T&M Resource Rates will not be added because there are overlapping date ranges:</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.OverlappingDateRanges)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingDateRanges)).Count()==0) { %> class="display-none"<% } %>>
          <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingDateRanges)).Count()%> T&M Resource Rates will not be added because there are gaps in the date ranges:</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingDateRanges)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.RateIsInUse)).Count()==0) { %> class="display-none"<% } %>>
        <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.RateIsInUse)).Count()%>  T&M Resource Rates will not be added because the following Resource Rate(s) are in use:</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.RateIsInUse)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
         <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidDateRange)).Count()==0) { %> class="display-none"<% } %>>
        <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidDateRange)).Count()%>  T&M Resource Rates will not be imported because they have an invalid date range:</div>
            <ul>
                <% foreach (ImportedTMResourceRate result in Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.InvalidDateRange)))
                   { %>
                <li><%: result.ResourceName%>&nbsp;<%: result.StartDate.HasValue ? result.StartDate.Value.ToMonthString() : result.StartDateString %>&nbsp;<%: result.EndDate.HasValue ? result.EndDate.Value.ToMonthString() : result.EndDateString%>&nbsp;<%: result.ResourceRate%></li>
                <% } %>
            </ul>
        </div>
        <div <% if (Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingRequiredData)).Count()==0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.Where(m => m.ImportTypes.Contains(ResourceRatesImportResult.MissingRequiredData)).Count()%>  T&M Resource Rates will not be add/updated because a ResourceID, Start Date, End Date or Rate is missing in the file :</div>
                <ul>
                    <li>See import file for missing T&M Resource Rates.</li>
                </ul>
            </div>
            </div>
   </div>
           
<% }
} %>
