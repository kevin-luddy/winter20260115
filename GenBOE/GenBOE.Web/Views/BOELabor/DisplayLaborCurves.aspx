<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Spread Curves</title>
</head>
<body class="module-content-center" onLoad="window.focus();">
    <div class="curve-page-holder" >
    <div class="curve-table-holder">
    <table style="border:none">
    <tr>
    <%
        for (int x = 1; x <= 53; x++)
        {
           
            %>
            <td>Curve <%:x %><br /><img src="<%= this.ResolveClientUrl("~/Resources/css/images/curves/Curve")%><%:x%>.jpg" /></td>
            <%
             if (x % 5 == 0)
            {
                %></tr><tr><%
            }
        } 
     %>
     </tr>
     </table>
    </div>
    <button onclick="window.close();" class="ies" name="close-button" type="button">Close</button>
    </div>
</body>
</html>
