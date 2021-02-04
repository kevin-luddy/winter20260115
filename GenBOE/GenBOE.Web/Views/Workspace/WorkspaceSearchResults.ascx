<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<WorkspaceSearchResultModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.Workspace" %>
<%@ Import Namespace="IES.Common.classes" %>

<script type="text/javascript">
    var WorkspaceSearchResultsWidget = new Widget('WorkspaceSearchResultsContainer');

    WorkspaceSearchResultsWidget.CopyWorkspace = function () {
        var data = {};
        data.workspaceID = $(this).attr('wsID');
        data.workspaceName = $(this).attr('wsName');
        data.isProjectMap = $(this).attr('isProjectMap');
        $(document).trigger('WorkspaceToCopyChosen', data);
        $('#WorkspaceSearchResults').dialog('close');
    };

    $(function () {
        $('a[name=CopyWorkspace]').click(WorkspaceSearchResultsWidget.CopyWorkspace);
        
        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('[name=PageControls]');
        pagingData.action = function(data) {
            delete data.WorkspaceResults;
            $(document).trigger('PageWorkspaceSearch', data);
        };
        pagingData.type = "PageNumber";

        WorkspaceSearchResultsWidget.AddPaging(pagingData);
    });

</script>

<div name="PageControls" style="margin-bottom: 20px;"></div>
<hr />

<%-- ReSharper disable once Html.TagNotClosed --%>
<div id="WorkspaceSearchResultsContainer">
    <% if (Model.WorkspaceResults.Count == 0)
       { %>
            <div style="margin: 20px;">
                <b>No results found.</b>
            </div>
            <hr />
    <% }
       else
       {
           foreach (WorkspaceSearchResult result in Model.WorkspaceResults)
           { %>
                    <div style="margin: 10px;">
                    <a style="float: right;" name="CopyWorkspace" wsID="<%: result.WorkspaceID %>" wsName="<%: result.WorkspaceName %>" isProjectMap="<%: result.IsProjectMapWorkspace.ToString().ToLower() %>">Copy</a>
                    <b>Workspace:</b> <%: result.WorkspaceName%><br />
                    <b>Submittal Date:</b> <%: Html.DisplayFor(model => result.SubmittalDate)%><br />
                    <b>Contract Period of Performance:</b> <%: Html.DisplayFor(model => result.ContractStartDate)%> to <%: Html.DisplayFor(model => result.ContractEndDate)%><br />
                    <%: result.Description%><% if (!string.IsNullOrWhiteSpace(result.Description)){ %><br /><% } %>
                    <b><%: Model.LabelLeadPricer %></b> <%: result.CostVolumeLeadPricer%><br />
                    <% if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST
                            && SiteMasterUtilities.IsProjectMapEnabled) { %>
                    <b>Workspace Type:</b> <%: result.ProjectMapType.GetDescription() %><br />
                    <% } %>
                </div>
                <hr />
        <%
           }
       } %>
</div>

<div name="PageControls" style="margin-top: 10px;"></div>