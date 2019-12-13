<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Workspace.ImportWorkofflineResultsModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.Workspace" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>

<% bool errorsOccurred = ViewData.ContainsKey("ERRORS_OCCURRED") ? (bool)ViewData["ERRORS_OCCURRED"] : false; %>

<script type="text/javascript">
    WorkspaceHomeWorkofflineImportVerificationWidget = {};
    WorkspaceHomeWorkofflineImportVerificationWidget.importResults = <%= ViewData["SERIALIZED_DATA"] %>;
    WorkspaceHomeWorkofflineImportVerificationWidget.invalidData = <%:errorsOccurred ? "true" : "false"%>;
</script>
<div id="WorkofflineImportVerification" class="import-verification">
    <div>The import file will make the following updates. To continue with the import, click Complete Import, otherwise click Back to import a different file or close this dialog window to not import a file.</div>
    <div class="title">Import Overview:</div>
    <% if (Model.ImportTypes.Contains((int)WorkofflineImportResult.FileError)) 
       {%>
            <div class="title">File Error:</div>
            <div>Import file does not match the required headings.  Please verify that the correct file related to this Workspace is being used.  Export a new version of the file and copy over your changes.  Try importing the file again.  BOE data has not been updated.</div>
    <% } %>
    <% if (Model.ImportTypes.Contains((int)WorkofflineImportResult.WorkspaceIDMissingOrInvalid)) 
       {%>
            <div class="title">Workspace:</div>
            <div>Workspace ID is missing or invalid.  Please verify that the correct file related to this Workspace is being used.  Export a new version of the file and copy over your changes.  Try importing the file again.  BOE data has not been updated.</div>
    <% } %>
    <% if (Model.ImportTypes.Contains((int)WorkofflineImportResult.RuntimeException)) 
       {%>
            <div class="title">Runtime Error:</div>
            <div>An error occurred while processing the import.  Contact a system administrator for assistance.</div>
    <% } %>
    <% if(Model.Boes.Any()){ %> 
        <div class="title">BOE:</div>
        <div class="title"><%= Model.BoesToUpdate %> BOE(s) will be updated:</div>
           <ul>
        <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.UpdateBoe)) { %>
               <li>
                    <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                </li>
        <% } %>
           </ul>
    <% } %>
    <% if(Model.Boes.Any()){ %> 
        <div class="title"><%= Model.BoesToNotUpdate %> BOE(s) will not be updated:</div>
           <ul>
        <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType != (int)BoeImportResult.UpdateBoe).GroupBy(g => g.BOEID).Select(f => f.First())) { %>
               <li>
                    <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                </li>
        <% } %>
           </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).Any())
       { %> 
        <div class="title">Task:</div>
        <div class="title"><%= Model.TaskElementsUpdate %> Task(s) will be updated:</div>
        <ul>
            <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements)
                    .Where(y => y.ImportType == (int)TaskElementImportResult.CreateTaskElement || y.ImportType == (int)TaskElementImportResult.UpdateTaskElement)) { %>
               <li>
                    <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
            <% } %>
        </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).Any())
       { %> 
        <div class="title"><%= Model.TaskElementsNotUpdate %> Task(s) will not be updated:</div>
           <ul>
            <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements)
                    .Where(y => y.ImportType != (int)TaskElementImportResult.CreateTaskElement && y.ImportType != (int)TaskElementImportResult.UpdateTaskElement).GroupBy(g => g.BOETaskElementID).Select(f => f.First())) { %>
               <li>
                    <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
        <% } %>
           </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Any())
       { %> 
        <div class="title">Resource Type:</div>
        <div class="title"><%= Model.ResourceTypeUpdate %> Resource Type(s) will be updated:</div>
        <ul>
            <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes)
                    .Where(y => y.ImportType == (int)LaborTypeImportResult.UpdateLaborType || y.ImportType == (int)LaborTypeImportResult.AddLaborType))
               { %>
               <li>
                    <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
            <% } %>
        </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Any())
       { %> 
        <div class="title"><%= Model.ResourceTypeNotUpdate %> Resource Type(s) will not be updated:</div>
           <ul>
            <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes)
                    .Where(y => y.ImportType != (int)LaborTypeImportResult.UpdateLaborType && y.ImportType != (int)LaborTypeImportResult.AddLaborType).GroupBy(g => g.ResourceID).Select(f => f.First()))
               { %>
               <li>
                    <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
        <% } %>
           </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).SelectMany(z => z.ResourceSpreads).Any() || Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads.ImportTypes).Any())
       { %> 
        <div class="title">Resource Spread:</div>
        <div class="title"><%= Model.ResourceSpreadUpdate %> Resource Spread(s) will be updated:</div>
        <ul>
            <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads)
                    .Where(z => z.ImportTypes.Contains((int)LaborSpreadImportResult.UpdateSpread) || z.ImportTypes.Contains((int)LaborSpreadImportResult.CreateSpread)))
               { %>
               <li>
                    <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
            <% } %>
        </ul>
    <% } %>
    <% if (Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).SelectMany(z => z.ResourceSpreads).Any() || Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads.ImportTypes).Any())
       { %> 
        <div class="title"><%= Model.ResourceSpreadNotUpdate %> Resource Spread(s) will not be updated:</div>
           <ul>
            <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).GroupBy(g => g.ResourceID).SelectMany(resTypes => resTypes.ToList()).Select(z => z.ResourceSpreads)
                    .Where(z => !(z.ImportTypes.Contains((int)LaborSpreadImportResult.UpdateSpread) || z.ImportTypes.Contains((int)LaborSpreadImportResult.CreateSpread)) && !string.IsNullOrEmpty(z.Resource)))
               { %>
               <li>
                    <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                </li>
        <% } %>
           </ul>
    <% } %>
    <br />
    <hr size="3" width="100%" />
    <br />
    <% if (!(Model.ImportTypes.Contains((int)WorkofflineImportResult.FileError) || Model.ImportTypes.Contains((int)WorkofflineImportResult.WorkspaceIDMissingOrInvalid)) && Model.BoesToNotUpdate > 0 || Model.TaskElementsNotUpdate > 0 || Model.ResourceTypeNotUpdate > 0 || Model.ResourceSpreadNotUpdate > 0) { %>
        <div class="title">Import Detailed:</div>
        <% if (Model.BoesToNotUpdate > 0) { %>
            <div class="title">BOE:</div>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeIDMissingInvalid).Any()) 
               { %> 
                    <div class="title">BOE ID is required.<br />BOE ID is missing or invalid.  Please verify that the correct file related to this Workspace is being used.  Export a new version of the file and copy over your changes.  Try importing the file again.  BOE data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeIDMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.ImportingUserNotAuthor).Any()) 
               { %> 
                    <div class="title">Current user does not have Author permission to the BOE.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.ImportingUserNotAuthor))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeNotInDraftState).Any()) 
               { %> 
                    <div class="title">The following BOEs are not currently in the Draft State.  BOEs must have a status of Draft to import changes.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeNotInDraftState))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeNotInWorkspace || x.ImportType == (int)BoeImportResult.BoeDoesNotExist).Any()) 
               { %> 
                    <div class="title">BOE was not found in the workspace.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeNotInWorkspace || x.ImportType == (int)BoeImportResult.BoeDoesNotExist))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.NonUniqueBoeID).Any())
               { %> 
                    <div class="title">Duplicate System defined BOE IDs found in the Import file.  Do not make edits to headings or hidden data.  Please correct the duplicate IDs or download an updated version of the import/export file and copy your changes to re-import.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.NonUniqueBoeID))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeTitleRequired).Any())
               { %> 
                    <div class="title">BOE Title field is required.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeTitleRequired))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeTitle100CharLimit).Any())
               { %> 
                    <div class="title">BOE Title field is a max of 100 characters.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeTitle100CharLimit))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeSourceOfData2000CharLimit).Any())
               { %> 
                    <div class="title">Sources of Data field is a max of 2000 characters.  BOE data has not been updated.</div>
                        <ul>
                    <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.BoeSourceOfData2000CharLimit))
                       { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                    <% } %>
                        </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.MissingRequiredBOECustomField).Any())
                { %>
                    <div class="title">One or more required BOE level Custom Fields are missing.  All required fields must be set.  BOE data has not been updated.</div>
                    <ul>
                        <% foreach (ImportWorkofflineBOEResultsModelView item in Model.Boes.Where(x => x.ImportType == (int)BoeImportResult.MissingRequiredBOECustomField))
                        { %>
                            <li>
                                <div><%= item.WBSNumber %>&nbsp;<%= item.WBSTitle %>&nbsp;<%=item.Title %>&nbsp;<%= item.CLINNumber %>&nbsp;<%= item.CLINTitle %></div>
                            </li>
                        <% } %>
                    </ul>
            <% } %>
        <% } //Model.BoesToNotUpdate %>
        <% if (Model.TaskElementsNotUpdate > 0) { %>
            <div class="title">Task:</div>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.IDMax3Characters).Any()) 
               { %> 
                    <div class="title">Task ID field is a max of 3 characters.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.IDMax3Characters)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.TitleRequired).Any()) 
               { %> 
                    <div class="title">Task Title is required.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.TitleRequired)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.TitleMax100Characters).Any()) 
               { %> 
                    <div class="title">Task Title field is a max of 100 characters.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.TitleMax100Characters)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.StartDateMissingInvalid).Any()) 
               { %> 
                    <div class="title">Task Start Date is missing, invalid or outside of the BOE date range.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.StartDateMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.EndDateMissingInvalid).Any()) 
               { %> 
                    <div class="title">Task End Date is missing, invalid or outside of the BOE date range.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.EndDateMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQHoursEquationMissingInvalid).Any()) 
               { %> 
                    <div class="title">MOQ <%: ViewData["HoursLabel"]%> Equation is missing or invalid.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQHoursEquationMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQHoursEquationMax250Characters).Any()) 
               { %> 
                    <div class="title">MOQ <%: ViewData["HoursLabel"]%> Equation field is a max of 250 characters.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQHoursEquationMax250Characters)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQCircularReference).Any()) 
               { %> 
                    <div class="title">MOQ <%: ViewData["HoursLabel"]%> Equation field contains a Workspace Variable that would cause a circular reference. Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MOQCircularReference)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.NonUniqueTaskID).Any()) 
               { %> 
                    <div class="title">Duplicate System defined Task IDs found in the Import file.  Do not make edits to headings or hidden data.  Please correct the duplicate IDs or download an updated version of the import/export file and copy your changes to re-import.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.NonUniqueTaskID)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MissingRequiredTaskCustomField).Any()) 
               { %> 
                    <div class="title">One or more required Task level Custom Fields are missing.  All required fields must be set.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineTaskElementResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).Where(x => x.ImportType == (int)TaskElementImportResult.MissingRequiredTaskCustomField)) 
                       { %>
                           <li>
                                <div><%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
        <% } //Model.TaskElementsNotUpdate %>
        <% if (Model.ResourceTypeNotUpdate > 0) { %>
            <div class="title">Resource Type:</div>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ElementOfCostMissingInvalid).Any()) 
               { %> 
                    <div class="title">Element of Cost is missing or invalid.  Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ElementOfCostMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceMissingInvalid).Any()) 
               { %> 
                    <div class="title">Resource is missing or invalid.  Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.PerfOrgMissingInvalid).Any()) 
               { %> 
                    <div class="title">Performing Org is missing or invalid.  Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.PerfOrgMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.HoursSpreadInvalid).Any()) 
               { %> 
                    <div class="title"><%: ViewData["HoursLabel"]%> Spread must be between - 9,999,999,999 and 9,999,999,999.  Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.HoursSpreadInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.DecimalPrecisionViolation).Any()) 
               { %> 
                    <div class="title">Resource Type <%: ViewData["HoursLabel"]%> Spread value violates Decimal Precision setting for workspace. Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.DecimalPrecisionViolation)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.CostSpreadRangeInvalid).Any()) 
               { %> 
                    <div class="title"><%: ViewData["HoursLabel"]%> Spread must be between - 9,999,999,999.99 and 9,999,999,999.99.  Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.CostSpreadRangeInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.CostDecimalPrecisionViolation).Any()) 
               { %> 
                    <div class="title">Resource Type Cost value violates Decimal Precision setting for workspace. Resource Types data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.CostDecimalPrecisionViolation)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.StartDateMissingInvalid).Any()) 
               { %> 
                    <div class="title">Resource Type Start Date is missing, invalid or outside of the Task Date Range.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.StartDateMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.EndDateMissingInvalid).Any()) 
               { %> 
                    <div class="title">Resource Type End Date is missing, invalid or outside of the Task Date Range.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.EndDateMissingInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceMultiValuesInvalid).Any()) 
               { %> 
                    <div class="title">CLIN or WBS needs to be assigned to Resource Type.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceMultiValuesInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.SpreadCurveMissing).Any()) 
               { %> 
                    <div class="title">Spread Curve is missing.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.SpreadCurveMissing)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.SpreadCurveInvalid).Any()) 
               { %> 
                    <div class="title">Spread Curve is invalid for the Resource selected.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.SpreadCurveInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.NonUniqueResourceTypeID).Any()) 
               { %> 
                    <div class="title">Duplicate System defined Resource IDs found in the Import file.  Do not make edits to headings or hidden data.  Please correct the duplicate IDs or download an updated version of the import/export file and copy your changes to re-import.  Task data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.NonUniqueResourceTypeID)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceSpreadsInvalid).Any()) 
               { %> 
                    <div class="title">One or more of the Resource Spreads is invalid.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.ResourceSpreadsInvalid)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.LaborTypeDateSpreadDateMismatch).Any()) 
               { %> 
                    <div class="title">Spread data entered into months outside of the Resource Type date range.  Either adjust the Resource date range or remove the spread values.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.LaborTypeDateSpreadDateMismatch)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.MissingRequiredResourceCustomField).Any()) 
               { %> 
                    <div class="title">One or more required Resource Type level Custom Fields are missing.  All required fields must be set.  Resource Type data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceTypeResultsModelView item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Where(x => x.ImportType == (int)LaborTypeImportResult.MissingRequiredResourceCustomField)) 
                       { %>
                           <li>
                                <div><%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
        <% } //Model.ResourceTypeNotUpdate %>
        <% if (Model.ResourceSpreadNotUpdate > 0) { %>
            <div class="title">Resource Spread:</div>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.HoursSpreadRangeInvalid)).Any()) 
               { %> 
                    <div class="title"><%: ViewData["HoursLabel"]%> Spread must be between - 9,999,999,999 and 9,999,999,999.  Resource Spread data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.HoursSpreadRangeInvalid))) 
                       { %>
                           <li>
                                <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.CostSpreadRangeInvalid)).Any()) 
               { %> 
                    <div class="title">Cost spread must be between - 9,999,999,999.99 and 9,999,999,999.99.  Resource Spread data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.CostSpreadRangeInvalid))) 
                       { %>
                           <li>
                                <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.CostDecimalPrecisionViolation)).Any()) 
               { %> 
                    <div class="title">Cost spread value violates Decimal Precision setting for workspace.  Resource Spread data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.CostDecimalPrecisionViolation))) 
                       { %>
                           <li>
                                <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DiscreteWhereCurveExpected)).Any()) 
               { %> 
                    <div class="title">Spread data entered into Resource Types with Spread type of Curve.  Please set the Resource to Discrete to enter in spread values by month.  Resource Spread data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DiscreteWhereCurveExpected))) 
                       { %>
                           <li>
                                <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DiscreteEntriesPastValidDate) || x.ImportTypes.Contains((int)LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange)).Any()) 
               { %> 
                    <div class="title">Spread data entered into months outside of the Resource Type date range.  Please adjust the Resource date range in order import these spread values.  Resource Spread data has not been updated.</div>
                       <ul>
                    <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DiscreteEntriesPastValidDate) || x.ImportTypes.Contains((int)LaborSpreadImportResult.SpreadDateOutsideOfLaborTypeDateRange))) 
                       { %>
                           <li>
                                <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                            </li>
                    <% } %>
                       </ul>
            <% } %>
            <% if (Model.Boes.Any() && Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DecimalPrecisionViolation)).Any()) 
            { %> 
                <div class="title">Resource spread value violates Decimal Precision setting for workspace.  Resource Spread data has not been updated.</div>
                    <ul>
                <% foreach (ImportWorkofflineResourceSpreadResultsModelViewCollection item in Model.Boes.SelectMany(x => x.TaskElements).SelectMany(y => y.ResourceTypes).Select(z => z.ResourceSpreads).Where(x => x.ImportTypes.Contains((int)LaborSpreadImportResult.DecimalPrecisionViolation))) 
                    { %>
                        <li>
                            <div><%= item.ElementofCost %>&nbsp;<%= item.Resource %>&nbsp;<%= item.PerformingOrg %>&nbsp;<%= item.StartDate %>&nbsp;<%= item.EndDate %>&nbsp;<%= item.SpreadCurve %>&nbsp;<%= item.TaskID %>&nbsp;<%= item.TaskTitle %>&nbsp;|&nbsp;<%= item.BOEWBSNumber %>&nbsp;<%= item.BOEWBSTitle %>&nbsp;<%=item.BOETitle %>&nbsp;<%= item.BOECLINNumber %>&nbsp;<%= item.BOECLINTitle %></div>
                        </li>
                <% } %>
                    </ul>
        <% } %>
        <% } //Model.ResourceSpreadNotUpdate %>
        <br />
        <hr size="3" width="100%" />
        <br />
    <% } %>
</div>
