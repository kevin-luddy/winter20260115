<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<IEnumerable<BOECustomFieldResourceModelView>>" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import Namespace="GenBOE.Web.ModelView" %>
<%@ Import Namespace="System.Collections.ObjectModel" %>
<%@ Import namespace="System.Web.Optimization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Resources</title>
    <%: Styles.Render("~/Content/siteCss") %>
</head>
<body class="module-content-center" style="margin-bottom:0;max-width:1000px" onLoad="window.focus();">
    <div class="curve-page-holder" >
    <div class="resource-table-holder">
        <div id="BOECustomFieldsResourceData" class="boe-custom-field-resource-grid">
            <table id="BOECustomFieldsResourceTable" class="grid readonly sortable">
                <thead>
                    <tr>
                        <th class="resource-id sort">ID</th>
                        <th class="description sort">Description</th>
                        <th class="segment sort">Rate Type</th>
                        <th class="element-of-cost last-child sort">Element of Cost</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (BOECustomFieldResourceModelView item in Model) {
                   
                           %>
                            <tr pkid="<%: item.CustomFieldOptionID %>">
                                <td class="resource-id"><%: item.ID %></td>
                                <td class="description"><%: item.Description %></td>
                                <td class="rate-type"><%: item.RateTypeDisplay %></td>
                                <td class="element-of-cost"><%: item.ElementOfCostDisplay %></td>
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
