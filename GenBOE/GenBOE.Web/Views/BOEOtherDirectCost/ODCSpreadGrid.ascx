<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>"%>


<script type="text/javascript">

    var ODC_SpreadCurves = [
                    {id:"<%=((int)SpreadCurves.DiscreteCost).ToString()%>",name:"<%=SpreadCurves.DiscreteCost.ToString()%>"},
                    {id:"<%=((int)SpreadCurves.Level).ToString()%>",name:"<%=SpreadCurves.Level.ToString()%>"},
                    {id:"<%=((int)SpreadCurves.Load).ToString()%>",name:"<%=SpreadCurves.Load.ToString()%>"}
                   ];
    var odcJson = <%= ViewData["ODCSJSON"] %>;
    var spreadCurvesLoad = "<%=SpreadCurves.Load.ToString()%>";
    var ODCSpreadWidget = InitializeODCSpreadWidget(ODC_SpreadCurves, odcJson, spreadCurvesLoad);
    
    $(function () {
        AfterDomLoadODCSpreadWidget(ODCSpreadWidget);
    });
</script>

<div id="ODCSpreadWidgetContainer">

<% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ODCSpreadForm" }))
    { %>
    <ul class="validation-box"></ul>

    <table class="editable grid" id="ODCSpread">
        <thead>
            <tr>
                <th>ODC Type</th>
            </tr>
            <tr>
                <th>Performing Org</th>
            </tr>
            <tr class="totals">
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>
    <%} %>
</div>
