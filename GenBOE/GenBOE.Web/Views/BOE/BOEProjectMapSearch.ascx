<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%
    var BOESearch_ReadOnly = bool.Parse(ViewData["BOESearch_ReadOnly"] as string);
    
    if (!BOESearch_ReadOnly)
    { %>
        <script type="text/javascript">
            var readOnly = <%: ViewData["READONLY"] %>;
            var boeId = '';

            var BOESearchWidget = InitializeBOESearchWidget(readOnly, boeId);

            $(function () {

                var pageSearchResultsUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE %>',
                '<%: WebConstants.ACTION_PAGE_SEARCH_RESULTS %>', 'boe/');

                AfterDomLoadBOESearchWidget(BOESearchWidget, pageSearchResultsUrl);

                $("#QuickSearchCategoryRow select[name='SelectedCategory']").remove();
                $("#QuickSearchCategoryRow").append('<input type="hidden" name="SelectedCategory" value="1" />');
            });

        </script>

        <button id="CopyFromExistingBOE-Button" class="ies" type="button">Copy from existing BOE</button>

        <div id="BOESearch" style="display:none; width: auto; min-height: 107px; height: auto;" class="ui-dialog-content ui-widget-content boe-search">
            <%: Html.Hidden("IsCopyFromBoeContext", true) %>
            <div class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that are marked searchable and do not contain OCI information will be searched.  If searching for BOEs in this Workspace, all BOEs will be searched.</div>
            <% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_QUICK_SEARCH, WebConstants.CONTROLLER_BOE, new { boeID = ViewData["BOEID"] }); %>
            <div class="divider"></div>
            <% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_PROJECTMAP_ADVANCED_SEARCH, WebConstants.CONTROLLER_BOE); %>
        </div>

        <div id="SearchResults" style="display:none; width: auto; min-height: 107px; height: auto;" class="ui-dialog-content ui-widget-content boe-search"></div>

<% } %>