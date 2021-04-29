<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<%
    var BOESearch_ReadOnly = Boolean.Parse(ViewData["BOESearch_ReadOnly"] as string);
    
    if (!BOESearch_ReadOnly)
    { %>
        <script type="text/javascript">
            var readOnly = <%: ViewData["READONLY"] %>;
            var boeId = <%= ViewData["BOEID"] ?? "''" %>;

            var BOESearchWidget = InitializeBOESearchWidget(readOnly, boeId);

            $(function () {

                var pageSearchResultsUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE %>',
                '<%: WebConstants.ACTION_PAGE_SEARCH_RESULTS %>', 'boe/' + BOESearchWidget.BoeID);

                AfterDomLoadBOESearchWidget(BOESearchWidget, pageSearchResultsUrl);
            });

        </script>

        <button id="CopyFromExistingBOE-Button" class="ies" type="button">Copy from existing BOE</button>

        <div id="BOESearch" style="display:none; width: auto; min-height: 107px; height: auto;" class="ui-dialog-content ui-widget-content boe-search">
            <%: Html.Hidden("IsCopyFromBoeContext", true) %>
            <% if ((bool)ViewData["UsingTemplateBoe"]) { %>
            <div id="StandardSearchText" class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that are marked searchable and do not contain OCI information will be searched.  If searching for BOEs in this Workspace, all BOEs that have a status of Draft, Awaiting Approval or Approved will be searched.</div>
            <% } else { %>
            <div id="MOQTemplateSearchText" class="form-row">If searching for BOEs in other Workspace or BOE Content Templates, only Workspaces that:
                <ol>
                    <li>Are marked searchable and do not contain OCI information</li>
                    <li>MOQ Template BOEs are set "no"</li>
                </ol>will be searched.
            </div>
            <% } %>
            <% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_QUICK_SEARCH, WebConstants.CONTROLLER_BOE, new { boeID = ViewData["BOEID"] }); %>
            <div class="divider"></div>
            <% Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ADVANCED_SEARCH, WebConstants.CONTROLLER_BOE, new { boeID = ViewData["BOEID"] }); %>
        </div>

        <div id="SearchResults" style="display:none; width: auto; min-height: 107px; height: auto;" class="ui-dialog-content ui-widget-content boe-search"></div>

<% } %>