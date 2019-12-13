<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.GenBOEMasterMenuItemModelView>>" %>
<%@ Import namespace="IES.Common" %>
<%@ Import namespace="IES.Common.classes" %>

<div class="nav-container">
    <div class="nav-bar left">
        <% foreach (var item in Model) {
               if (!item.subMenuItems.Any()) {
                    if (item.actionName.Length > 0) { %>
                        <div <%: MvcHtmlString.Create(SiteMasterUtilities.CurrentActionMatches(ViewContext, item.controllerName, item.actionName)
                                    ? "class=\"selected\"" : String.Empty) %>>
                            <%: Html.ActionLink(item.linkText, item.actionName, item.controllerName, item.routeValues, item.htmlAttributes)%>
                        </div>
                <% }
                   else if (item.linkUrl != null) { %>
                        <div><a href="<%: item.linkUrl%>" target="_blank"><%: item.linkText %></a> </div>
                <% } else { %>  
                        <div><a><%: item.linkText %></a></div>
                <%
                   }
               } else { %>
                    <div>
                        <a class="inactive"><%: item.linkText %></a>
                        <div class="submenu">
                            <% foreach (var subItem in item.subMenuItems) {
                                    if (subItem.actionName.Length > 0) { %>
                                        <div <%: MvcHtmlString.Create(SiteMasterUtilities.CurrentActionMatches(ViewContext, subItem.controllerName, subItem.actionName)
                                                    ? "class=\"selected\"" : String.Empty) %>>
                                            <%: Html.ActionLink(subItem.linkText, subItem.actionName, subItem.controllerName, subItem.routeValues, subItem.htmlAttributes)%>
                                        </div>
                                 <% } else if (subItem.linkUrl != null) { %>
                                        <div><a href="<%: subItem.linkUrl%>" target="_blank"><%: subItem.linkText %></a> </div>
                                 <% } else { %>
                                        <div><a><%: subItem.linkText%></a></div>
                                 <% }
                               } %>
                        </div>
                    </div>
            <% }
           } %>
    </div>
    <div class="<%= (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST) ? "nav-bar " : string.Empty %>right">
    </div>
</div>