<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Collections.Generic.ICollection<IES.Common.UserData>>" %>

<table style="border: 0;">
    <tr><td style="border: 0;">Click a name to select a person:</td></tr>
</table>
<br />
<table>
    <colgroup>
        <col width="125px" />
        <col width="75px" />
        <col width="175px" />
        <col width="175px" />
        <col width="50px" />
        <col width="50px" />
    </colgroup>
    <thead>
        <tr class="header" style="height: 36px">
            <td>Name</td>
            <td>Work Phone</td>
            <td>Company</td>
            <td>Email</td>
            <td>State</td>
            <td>Country</td>
        </tr>
    </thead>
    <tbody>
        <%
        int rownum = 0;

        foreach (IES.Common.UserData user in Model)
        {
            string rowClass = (++rownum % 2 == 0) ? "even" : "odd";

            System.Text.StringBuilder sbResults = new System.Text.StringBuilder();
            sbResults.Append("{ ");
            sbResults.AppendFormat("AccountName : '{0}', ", user.Ntid ?? string.Empty);
            sbResults.AppendFormat("DisplayName : '{0}', ", Html.Encode(HttpUtility.JavaScriptStringEncode(user.DisplayName)) ?? string.Empty);
            sbResults.AppendFormat("WorkPhone : '{0}', ", user.Phone ?? string.Empty);
            sbResults.AppendFormat("Company : '{0}', ", user.Company.Replace("'", "\\'") ?? string.Empty);
            sbResults.AppendFormat("Email : '{0}', ",  Html.Encode(HttpUtility.JavaScriptStringEncode(user.Email)) ?? string.Empty);
            sbResults.AppendFormat("State : '{0}', ", user.State ?? string.Empty);
            sbResults.AppendFormat("Country : '{0}'", user.Country ?? string.Empty);
            sbResults.Append(" }");
            string jsResultsString = sbResults.ToString();
            %>

            <tr class="<%=rowClass%>">
                <td><a href="#" onclick="ProcessActiveDirectorySelection(<%=jsResultsString%>); return false;"><%=user.DisplayName%></a></td>
                <td><%=user.Phone%></td>
                <td><%=user.Company%></td>
                <td><%=user.Email%></td>
                <td><%=user.State%></td>
                <td><%=user.Country%></td>
            </tr>
        <%}%>
    </tbody>
</table>