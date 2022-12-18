<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.GenBOEMasterMenuItemModelView>>" %>
<script type="text/javascript">
    $(function () {
        if (window.location.pathname.split('/')[3] != undefined) {
            $('#Sub_ShowGettingStarted').addClass('display-none');
        }

        if ('<%: ViewData["DisplayINLForms"] %>' == 'False') {
            $('a[name=ManageINLFormsMenu]').parent().addClass('display-none');
		}

		if ('<%: ViewData["EnableSAP"] %>' == 'False') {
			$('a[name=CalculateActualsMenuLink]').parent().addClass('display-none');
		}

        if ('<%: ViewData["DisplayProjectMapOnly"] %>' == 'True') {
            $('a[name=ImportIMSMenuLink]').parent().addClass('display-none');
            $('a[name=ManageCLINsMenuLink]').parent().addClass('display-none');
            $('a[name=ManageWBSMenuLink]').parent().addClass('display-none');
            $('a[name=ManageBOEsMenuLink]').parent().addClass('display-none');
            $('a[name=FindReplaceBOEMenuLink]').parent().addClass('display-none');
			$('a[name=BOEBulkSubmitMenuLink]').parent().addClass('display-none');
			$('a[name=CalculateActualsMenuLink]').parent().addClass('display-none');
            // Hidden for now since there is only BOE Bulk Submit in the Menu 
            $('a[name=BOEBulkSubmitMenuLink]').parent().parent().parent().addClass('display-none');
        }

        if ('<%: ViewBag.DisplayZoneTravelUpdateRates %>' == 'False') {
            $('a[name=UpdateZoneTravelRatesLink]').parent().addClass('display-none');
        }

        $('a[name=UpdateZoneTravelRatesLink]').removeAttr('href').click(function () {
            Session.confirmDialog('Update Zone Travel Rates', 'Are you sure you want to update the Zone Travel with the current rates?  This may impact Escalation Rate, Travel Agency Rates as well as Misc/Other Daily Rate.  If Yes, you will not be able to revert to the old rates.', function () {
                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_UPDATE_ZONE_TRAVEL_RATES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: null,
                    success: function (response) {
                        RaiseNotification('The Zone Travel Rates were successfully updated');
                        $('a[name=UpdateZoneTravelRatesLink]').parent().addClass('display-none');
                    },
                    error: function (response) {
                        RaiseNotification('The update to the Zone Travel Rates failed');
                    }
                });
            });
        });

        if ('<%: ViewBag.DisplayOffloadUpdateRates %>' == 'False') {
            $('a[name=UpdateOffloadRatesLink]').parent().addClass('display-none');
        }

        $('a[name=UpdateOffloadRatesLink]').removeAttr('href').click(function () {
            Session.confirmDialog('Update Offload Rates', 'Are you sure you want to update the Offload Rates with the current rates?  If Yes, you will not be able to revert to the old rates.', function () {
                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_UPDATE_OFFLOAD_RATES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: null,
                    success: function (response) {
                        RaiseNotification('The Offload Rates were successfully updated');
                        $('a[name=UpdateOffloadRatesLink]').parent().addClass('display-none');
                    },
                    error: function (response) {
                        RaiseNotification('The update to the Offload Rates failed');
                    }
                });
            });
        });
    });
</script>

<div class="nav-container site-nav-container">
    <div class="nav-bar left">
        <% foreach (var item in Model.Where(x => x.menuLocation == MenuLocation.Left))
           {
               if (item.subMenuItems.Count == 0)
               { %>
                <div <%: MvcHtmlString.Create(SiteMasterUtilities.CurrentActionMatches(ViewContext, item.controllerName, item.actionName)
                            ? "class=\"selected\""
                            : String.Empty) %>>
                         <%:Html.RouteLink(item.linkText, item.routeName, new { controller = item.controllerName, action = item.actionName }, item.htmlAttributes) %>
                </div>
            <% }
               else
               { %>
                <div>
                    <a class="inactive"><%: item.linkText %></a>
                    <div class="submenu">
                    <% foreach (var subItem in item.subMenuItems)
                        {
                            if (subItem.actionName.Length > 0)
                            { %>
                                <div <%: MvcHtmlString.Create(
                                        GenBOE.Web.Common.SiteMasterUtilities.CurrentActionMatches(ViewContext, subItem.controllerName, subItem.actionName)
                                            ? "class=\"selected\""
                                            : String.Empty) %>>
                                   
                                   <%: Html.RouteLink(subItem.linkText, subItem.routeName, new { controller = subItem.controllerName, action = subItem.actionName }, subItem.htmlAttributes)%>
                              
                                </div>
                            <% }
                        } %>
                    </div>
                </div>
            <% }
           } %>
    </div>
    <div class="nav-bar right">
        <% foreach (var item in Model.Where(x => x.menuLocation == MenuLocation.Right)) {
                if (!item.subMenuItems.Any()) {
                    if (item.linkUrl != null) { %>
                        <div style="width:205px;"><a href="<%: item.linkUrl%>" target="_blank"><%: item.linkText%></a> </div>
                        <% } else { %>
                <div>
                    <%:Html.RouteLink(item.linkText, item.routeName, new { controller = item.controllerName, action = item.actionName }, item.htmlAttributes) %>
                </div>
             <% } %>
            <% } else { %>
                <div>
                    <a class="inactive"><%: item.linkText %></a>
                    <div class="submenu">
                    <% foreach (var subItem in item.subMenuItems)
                        {
                            if (subItem.actionName.Length > 0)
                            { %>
                                <div <%: MvcHtmlString.Create(SiteMasterUtilities.CurrentActionMatches(ViewContext, subItem.controllerName, subItem.actionName)
                                            ? "class=\"selected\""
                                            : String.Empty) %> id="Sub_<%: subItem.linkText.Replace(" ", "") %>">
                                        <%:Html.RouteLink(subItem.linkText, subItem.routeName, new { controller = subItem.controllerName, action = subItem.actionName }, subItem.htmlAttributes)%>
                                </div>
                            <% } else {
                                if (subItem.linkUrl != null) {
                                 %>
                                    <div style="width:205px;"><a href="<%: subItem.linkUrl%>" target="_blank"><%: subItem.linkText%></a> </div>
                                <%
                                }
                                else if (SiteMasterUtilities.CurrentActionMatches(ViewContext, WebConstants.CONTROLLER_WORKSPACE, WebConstants.ACTION_INDEX))
                                {
                                %>
                                    <div> <a><%: subItem.linkText%></a></div>
                                <% }
                            }
                        } %>
                    </div>
                </div>
            <% }
           } %>
    </div>
</div>
