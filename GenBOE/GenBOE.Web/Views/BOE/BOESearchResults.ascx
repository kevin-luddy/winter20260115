<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.SearchResultsModelView>" %>

<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="IES.Common.OfficeUtilities" %>
<%@ Import Namespace="GenBOE.DataBridge.DTO" %>
<% var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }; %>

<script type="text/javascript">

    var BOESearchResultsWidget = new Widget("BOESearchResults", <%: ViewData["READONLY"] %>);

     BOESearchResultsWidget.Initialize = function() {
        BOESearchResultsWidget.Module = $('#BOESearchResults');
     }

     $('a.Copy').click(function() {
         ShowLoadingBox();
         var data = {};
         data.copyBOEID = $(this).parents('.wbs-clin').find('input[name="CopyBoeID"]').val();

         data.taskElementsToCopy = [];
         $('#TaskElementsToCopyTable' + data.copyBOEID + ' tbody :input:checked').each(function () {
             data.taskElementsToCopy.push($(this).data('copy-moq-id'));
         });

         data.travelElementsToCopy = [];
         $('#TravelElementsToCopyTable' + data.copyBOEID + ' tbody :input:checked').each(function () {
             data.travelElementsToCopy.push($(this).data('copy-travel-id'));
         });
         
         var dataToSend = JSON.stringify(data);

         $.ajax({
             type: 'POST',
             url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE%>',
                '<%: WebConstants.ACTION_BOE_COPY_CONFLICTS%>',
                'boe/<%: (int)ViewData["BOEID"] %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function(response) {
                HideLoadingBox();   
                $('[name=PageControls]').hide();
                $('.SearchResultsMessage').hide();
                $('#BOECopyConflicts').html(response).show();
            }
        });

     });

    $('a.Preview').click(function() {

         var boeID = $(this).parents('.wbs-clin').find('input[name="CopyBoeID"]').val();

        // Remove the old hidden iFrame, if it exists
        $('#BOEExport-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'BOEExport-DownloadTarget',
            'class': 'display-none',
            'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_BOE %>',
                    '<%: WebConstants.ACTION_BOE_SEARCH_PREVIEW %>',
                    '?boeID=' + boeID)
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');

    });

    /*
    * Gets a MOQ Equation partial view with MOQ data copied from another TaskElement.  This is 
    * triggered when the user clicks "Copy MOQ" link from the search results.
    */
    $('a.CopyMoqLink').click(function() {
        ShowLoadingBox();
        var copyMoqTaskIdData = $(this).data('copy-moq-id');
        var copyboeId = $(this).closest('.TaskElementsToCopyContent').data('boe-id');
        var destinationTaskId = $('#CopyMoqFromBoeLink').data('moq-task-id');
    
        $.ajax({
            type: 'POST',
            url: GenSession.CreateUrl({ workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                controller: '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                action: '<%:WebConstants.ACTION_COPY_MOQ_EQUATION %>',
                boe: '<%: (int)ViewData["BOEID"] %>',
                copyBoe: copyboeId,
                taskElement: copyMoqTaskIdData,
                destinationTaskElement: destinationTaskId }),
            dataType: 'html',
            success: function (response) {
                // A new ChildWidget is created with the MOQ Equation partial view.  Reinitialize, reload the partial
                // view and trigger a change event so recomputations occur.
                TaskElementDetailsWidget.ChildWidgets = [];
                $('#MOQEquationFieldContent').html(response);
                $('#MOQEquationFieldContent').each(function () {
                    var content = $(this);
                    angular.element(document).injector().invoke(
                    [
                        "$compile", function ($compile) {
                            var scope = angular.element(content).scope();
                            $compile(content)(scope);
                            scope.$apply();
                        }
                    ]);
                });
                $('#MOQEquation').trigger('change');
                HideLoadingBox();
            },
            error: function () {
                HideLoadingBox();
            }
        });
        $(document).trigger("CLOSE_SEARCH_RESULTS");
    });

    $(function () {
        BOESearchResultsWidget.afterDOMLoad();
        BOESearchResultsWidget.Initialize();
        
        $('#SearchAgain-SearchResults').click(function () {
            $(document).trigger("SEARCH_AGAIN");
        });

        $('#Close-SearchResults').click(function() {
            $(document).trigger("CLOSE_SEARCH_RESULTS");
        });

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%= Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;
        pagingData.data.IsCopyFromBoeContext = '<%: Model.IsCopyFromBoeContext %>';

        pagingData.div = $('[name=PageControls]');
        pagingData.action = function(data) {
            delete data.BOESearchResults;
            $(document).trigger('PAGE_RESULTS', data);
        };
        pagingData.type = "PageNumber";

        BOESearchResultsWidget.AddPaging(pagingData);

        $('.TaskElementsToCopy').click(function() {
            if ($(this).hasClass('expanded')) {
                $(this).removeClass('expanded').addClass('collapsed');
                $(this).next('.TaskElementsToCopyContent').slideUp(400);
            }  
            else {
                $(this).removeClass('collapsed').addClass('expanded');
                $(this).next('.TaskElementsToCopyContent').slideDown(400);
            }
        });

        $('.TasksToCopySelectAll').click(function() {
            // Uncheck or check all task elements according to the new setting of the overall checkbox.
            $(this).closest('table').find('input:checkbox').prop('checked', $(this).prop('checked'));
        });

        $('.CopyTaskElementCheck').click(function() {
            if ($(this).prop('checked') == false) {
                // Clear the "Copy All" checkbox if any individual checkbox is unchecked.
                $('.TasksToCopySelectAll').prop('checked', false);
            }
        });

        // Travel
        $('.TravelElementsToCopy').click(function() {
            if ($(this).hasClass('expanded')) {
                $(this).removeClass('expanded').addClass('collapsed');
                $(this).next('.TravelElementsToCopyContent').slideUp(400);
            }  
            else {
                $(this).removeClass('collapsed').addClass('expanded');
                $(this).next('.TravelElementsToCopyContent').slideDown(400);
            }
        });

        $('.TravelToCopySelectAll').click(function() {
            // Uncheck or check all travel elements according to the new setting of the overall checkbox.
            $(this).closest('table').find('input:checkbox').prop('checked', $(this).prop('checked'));
        });

        $('.CopyTravelElementCheck').click(function() {
            if ($(this).prop('checked') == false) {
                // Clear the "Copy All" checkbox if any individual checkbox is unchecked.
                $('.TravelToCopySelectAll').prop('checked', false);
            }
        });
    });

</script>

<div name="PageControls" style="margin-bottom: 20px;"></div>
<% if (!string.IsNullOrEmpty(Model.SearchResultsMessage)) { %>
<div class="SearchResultsMessage"><%:Model.SearchResultsMessage %></div>
<% } %>
<div id="BOESearchResults" class="boe-search-results">
    <% if ((bool)ViewData["UsingTemplateBoe"]) { %>
    <div class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that are marked searchable and do not contain OCI information will be searched.  If searching for BOEs in this Workspace, all BOEs that have a status of Draft, Awaiting Approval or Approved will be searched.</div>
    <% } else { %>
    <div class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that:
        <ol>
            <li>Are marked searchable and do not contain OCI information</li>
            <li>MOQ Template BOEs are set "no"</li>
        </ol>will be searched.
        <div class="color-red italic">BOEs from Workspaces with MOQ Template BOEs set to "Yes" CANNOT be copied to BOEs in Workspaces with MOQ Template BOEs set to "No."</div>
        If searching for BOEs in this Workspace, all BOEs that have a status of Draft, Awaiting Approval or Approved will be searched.
    </div>
    <% } %>
    <ul>
       <li>'Preview' will open the BOE in MS Word.</li>
       <li>'Copy BOE' will copy the entire BOE excluding any un-selected Task Elements (check boxes).</li>
       <li>'Copy MOQ' will copy the selected Task Elements MOQ information only.</li>
   </ul>

    <% if (Model.BOEResults.Any()) { %>
        
        <div class="text-boe-select">Select BOE to copy.</div> 

        <div class="divider"></div>

        <% foreach (BOESearchResult item in Model.BOEResults)
           { %>
            <div class="wbs-clin">WBS: <%: item.WBSNumber%> - <%: item.WBSTitle%>&nbsp;&nbsp;&nbsp;&nbsp;CLIN: <%: item.ClinNumber%> - <%: item.ClinTitle%> 
                <% if (Model.IsCopyFromBoeContext) 
                { %> 
                <a style="float: right;" class="Copy BOE">Copy BOE</a>
                <% } %>
                <a style="float: right; padding-right:10px" class="Preview">Preview</a>
                <input type="hidden" id="CopyBoeID" name="CopyBoeID" value="<%: item.BOEID %>" />
            </div>
            <div class="boetitle">BOE Title: <%: item.BOETitle%></div>
            <div class="workspace">Workspace Name: <%: item.WorkspaceName%>&nbsp;&nbsp;&nbsp;&nbsp;Submittal Date: <%: item.SubmittalDate.HasValue ? item.SubmittalDate.Value.ToShortDateString() : string.Empty%></div>
            <div class="description"><%: RTEUtilities.TurnHTMLIntoPlainText(item.BOEDescription) %></div>
            <div class="author">Author(s): <span class="display-name"><%: item.AuthorDisplayName%></span></div>
            <% if (item.TaskElements.Any())
               { %>
                <div class="TaskElementsToCopy collapsed"></div>
                <div class="module-content-data TaskElementsToCopyContent" data-boe-id="<%:item.BOEID%>" style="display:none">
                    <table id="TaskElementsToCopyTable<%:item.BOEID%>" class="TaskElementsToCopyTable">
                        <thead>
                            <tr>
                                <th class="TaskCopyHeaderTitle">Task Title</th>
                                <th class="TaskCopyHeaderEquation">MOQ Equation</th>
                                <th class="TaskCopyHeaderResult">MOQ Result</th>
                                <% if (Model.IsCopyFromBoeContext) 
                                   { %>
                                    <th style="text-align:center"><input type="checkbox" class="TasksToCopySelectAll" checked="checked"/></th>
                                <% }
                                   else 
                                   { %>
                                    <th>&nbsp</th>
                                <% } %>
                            </tr>
                        </thead>
                        <tbody>
                    <% foreach (TaskElementModelView taskElement in item.TaskElements)
                    { %>
                            <tr>
                                <td class="TaskCopyRowCell"><%:taskElement.TaskTitle%></td>
                                <td class="TaskCopyRowCell"><%:taskElement.MoqEquation%></td>
                                <td class="TaskCopyRowCell"><%:taskElement.MoqResult%></td>
                                <% if (Model.IsCopyFromBoeContext) 
                                   { %>
                                    <td class="TaskCopyRowActionCell"><input class="CopyTaskElementCheck" data-copy-moq-id="<%:taskElement.Id%>" type="checkbox" checked="checked"/></td>
                                <% }
                                   else 
                                   { %>
                                    <td class="TaskCopyRowActionCell"><a class="CopyMoqLink" data-copy-moq-id="<%:taskElement.Id%>">Copy MOQ</a></td>
                                <% } %>                                
                            </tr>
                    <% } %>
                    </tbody>
                    </table>
                </div>
            <% } 

               if (item.TravelElements.Any() && Model.IsCopyFromBoeContext)
               { %>
                <div class="TravelElementsToCopy collapsed"></div>
                <div class="module-content-data TravelElementsToCopyContent" data-boe-id="<%:item.BOEID%>" style="display:none">
                    <table id="TravelElementsToCopyTable<%:item.BOEID%>" class="TravelElementsToCopyTable">
                        <thead>
                            <tr>
                                <th>Travel Title</th>
                                <th style="text-align:center"><input type="checkbox" class="TravelToCopySelectAll" checked="checked"/></th>
                            </tr>
                        </thead>
                        <tbody>
                    <% foreach (TravelElementModelView travelElement in item.TravelElements)
                    { %>
                            <tr>
                                <td class="TaskCopyRowCell"><%:travelElement.TaskTitle%></td>
                                <td class="TaskCopyRowActionCell"><input class="CopyTravelElementCheck" data-copy-travel-id="<%:travelElement.Id%>" type="checkbox" checked="checked"/></td>
                            </tr>
                    <% } %>
                    </tbody>
                    </table>
                </div>
            <% } %>
            <div class="divider"></div>
        <% } %>


    <% } else { %>
        <div class="text-noresults">No results found.</div>
       
        <div class="divider"></div>
    <% } %>

    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="buttons">
                <button id="SearchAgain-SearchResults" class="ies" type="button">Search again</button>
                <button id="Close-SearchResults" class="ies" name="close-button" type="button">Close</button>
            </div>
        </div>
    </div>
</div>

<div id="BOECopyConflicts" class="display-none"></div>


<div name="PageControls" style="margin-top: 10px;"></div>