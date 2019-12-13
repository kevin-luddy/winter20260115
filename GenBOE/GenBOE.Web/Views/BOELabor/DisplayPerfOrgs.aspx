<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<BOECustomFieldOptionModelView>>" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import Namespace="System.Collections.ObjectModel" %>
<%@ Import namespace="System.Web.Optimization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Performing Orgs</title>
    <%: Styles.Render("~/Content/siteCss") %>
</head>
<body class="module-content-center" style="margin-bottom:0" onLoad="window.focus();">
    <div class="curve-page-holder" >
    <div class="resource-table-holder">
        <div id="BOECustomFieldsPerformingOrgData" class="boe-custom-field-performing-org-grid">
            <table id="BOECustomFieldPerformingOrgTable" class="readonly grid">
                <thead>
                    <tr>
                        <th class="performing-organization-id">ID</th>
                        <th class="performing-organization-description last-child">Description</th>
                    </tr>
                </thead>
                <tbody>
                    <%  foreach (BOECustomFieldOptionModelView item in Model) { %>     
                        <tr pkid="<%:item.CustomFieldOptionID%>" class="in-use">
                            <td>
                                <div><%: item.ID %></div>
                            </td>
                            <td>
                                <div><%: item.Description %></div>
                            </td>
                        </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>
    <button onclick="window.close();" class="ies" name="close-button" type="button">Close</button>
    </div>
</body>
</html>
