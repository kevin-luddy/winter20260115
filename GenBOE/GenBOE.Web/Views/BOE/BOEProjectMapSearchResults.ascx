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
        data.copyProjectMapId = $(this).parents('.wbs-clin').find('input[name="CopyProjectMapID"]').val();
        var dataToSend = JSON.stringify(data);
        
        BOESearchResultsWidget.clearValidationBox();
        
        BOESearchResultsWidget.ajaxRequest.apply(BOESearchResultsWidget, [{
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE%>',
                '<%: WebConstants.ACTION_SAVE_COPY_OF_PROJECTMAP%>',
                'boe/'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,            
            success: function(response) {
                location.reload();
            },
            error: function(response){
                HideLoadingBox();
            }
        }]);
     });

    $('a.Preview').click(function() {

        var projectMapId = $(this).parents('.wbs-clin').find('input[name="CopyProjectMapID"]').val();

        // Remove the old hidden iFrame, if it exists
        $('#BOEExport-DownloadTarget').remove();

        // Create a new hidden iFrame and set it's source to the chosen report's URL
        var targetIFrame = $('<iframe />', {
            'id': 'BOEExport-DownloadTarget',
            'class': 'display-none',
            'src': CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_BOE %>',
                    '<%: WebConstants.ACTION_PROJECTMAP_SEARCH_PREVIEW %>',
                    '?projectMapId=' + projectMapId)
        });

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        targetIFrame.appendTo('body');

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
    });

</script>

<div name="PageControls" style="margin-bottom: 20px;"></div>
<% if (!string.IsNullOrEmpty(Model.SearchResultsMessage)) { %>
<div class="SearchResultsMessage"><%:Model.SearchResultsMessage %></div>
<% } %>
<div id="BOESearchResults" class="boe-search-results">
   <ul class="validation-box"></ul>
   <div class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that are marked searchable and do not contain OCI information will be searched.  If searching for BOEs in this Workspace, all BOEs will be searched.</div>
   <ul>
       <li>'Preview' will open the BOE in MS Word.</li>
       <li>'Copy BOE' will copy the entire BOE.</li>
   </ul>

    <% if (Model.BOEResults.Any()) { %>
        
        <div class="text-boe-select">Select BOE to copy.</div> 

        <div class="divider"></div>

        <% foreach (BOESearchResult item in Model.BOEResults)
           { %>
            <div class="wbs-clin">WBS: <%: item.WBSNumber%> - <%: item.WBSTitle%>&nbsp;&nbsp;&nbsp;&nbsp;CLIN: <%: item.ClinTitle%>
                <% if (Model.IsCopyFromBoeContext) 
                { %> 
                <a style="float: right;" class="Copy BOE">Copy BOE</a>
                <% } %>
                <a style="float: right; padding-right:10px" class="Preview">Preview</a>
                <input type="hidden" id="CopyProjectMapID" name="CopyProjectMapID" value="<%: item.ProjectMapId %>" />
            </div>
            <div class="workspace">Workspace Name: <%: item.WorkspaceName%></div>
            <div class="boetitle">Activity Id: <%: item.BOETitle%></div>
            <div class="description">Activity Name: <%: RTEUtilities.TurnHTMLIntoPlainText(item.BOEDescription) %></div>
            <div class="description">Category: <%: item.Category %></div>
            <div class="description">Cam Name: <%: item.CamName %></div>
            <div class="TaskElementsToCopy expanded"></div>
            <div class="module-content-data TaskElementsToCopyContent" data-boe-id="<%:item.ProjectMapId%>" style="display:none">
                <table id="TaskElementsToCopyTable<%:item.ProjectMapId%>" class="TaskElementsToCopyTable">
                    <thead>
                        <tr>
                            <th>Task</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="TaskCopyRowCell"><%: RTEUtilities.TurnHTMLIntoPlainText(item.ProjectMapTask) %>
                            </td>                             
                        </tr>
                </tbody>
                </table>
            </div>
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
<div name="PageControls" style="margin-top: 10px;"></div>