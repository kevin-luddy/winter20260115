<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<GenBOE.ActionLogic.ModelView.BOE.BoeOffloadModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.BOE" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import namespace="System.Web.Optimization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1">
    <!--  *** DO NOT PLACE ANYTHING ABOVE THIS META STATEMENT, OR YOU WILL BREAK US AGAIN. THIS HAS TO BE THE FIRST THING IN THE HEAD. IF THAT'S AN ISSUE, 
                TALK TO DUSAN OR MATT. *** 
        Force Internet Explorer 8 if its available.
        -->
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <title>BOE Offload Preview</title>

    <script type="text/jscript">
        // Global variable that stores the production URL. This is used to decide whether we are in production or not.
        // It is created/assigned in every master/layout page, so that way our JS libraries (in this solution and in common) don't have to have the URL hardcoded,
        // but they can use the web.config values instead :)
        var prodGenBoeUrl_Global = "<%: SiteMasterUtilities.ProductionUrl() %>";

        // This script is used to prevent cross-frame attacks
        if (top.frames.length != 0) {
            top.location = self.document.location;
        }
    </script>

    <%: Scripts.Render("~/bundles/results") %>
    <%: Styles.Render("~/Content/homeCss") %>
    
    <script type="text/javascript">
        var prefix = '';

        var offloadResults = new Widget("boeOffloadResults");
        var nonOffloadResults = new Widget("boeOffloadWarnings");

        $(function () {       
            createModule($('.boeOffloadResults.module'));
            refreshModule($('.boeOffloadResults.module'));
            CollapsibleModule($('.boeOffloadResults.module'));
            
            if ('<%= Model.OffloadWarnings.Any().ToString().ToLower()%>'.isTrue()) {          
                createModule($('.boeOffloadWarnings.module'));
                refreshModule($('.boeOffloadWarnings.module'));
                CollapsibleModule($('.boeOffloadWarnings.module'));
            }
        });
    </script> 
</head>
<body>
<div style="padding-top:20px" class="main">
    <div class="boeOffloadResults module expanded">

        <div class="module-header-data" >Labor Resource Offload Preview</div>
        <div class="module-content-data expanded-content">
            <% if (Model != null && Model.OffloadResources.Any()) { %>
            <div id="BOEOffloadResults-OffloadResults" class="boe-offload-results" >
                <table class="labor-types-grid grid offload-grid">
                    <thead>
                        <tr>
                            <th class="resource">Modified Resource</th>
                            <th class="resource">Offloaded Resource</th>
                            <th class="performing-org">Performing Org</th>
                            <% if (Model.IsMultiClinWbs)
                            {%>
                            <th class="resource-wbs">WBS</th>
                            <th class="resource-clin">CLIN</th>
                            <% }  %>
                            <th class="date">Start Date</th>
                            <th class="date">End Date</th>
                            <th>Modified<br />Spread Curve</th>
                            <th class="spread-hours">Modified<br />Hours Spread</th>
                            <th class="spread-hours">Hours Offloaded</th>
                            <th class="spread-hours">Offloaded Cost</th>
                            <th>Spread Type</th>
                            <% // display the header (all dates across the spread)
                            foreach (DateTime dt in Model.SpreadDates) { %>
                            <th><%:dt.ToMonthString()%></th><%
                            } %>
                        </tr>
                    </thead>
                    <tbody>
                    <%
                        foreach (BoeOffloadResourceModelView item in Model.OffloadResources)
                        {
                            %>
                        <tr>
                            <td rowspan="3" class="resources"><%: item.ExistingResource %></td>
                            <td rowspan="3" class="resources"><%: item.OffloadedResource %></td>
                            <td rowspan="3" class="performing-org"><%: item.PerformingOrg %></td>
                            <% if (Model.IsMultiClinWbs) {%>
                            <td rowspan="3" class="resource-wbs"><%: item.Wbs %></td>
                            <td rowspan="3" class="resource-clin"><%: item.Clin %></td>
                            <%} %>               
                            <td rowspan="3" class="text start-date"><%: item.StartDate %></td>
                            <td rowspan="3" class="text end-date"><%: item.EndDate %></td>
                            <td rowspan="3" ><%: item.ExistingSpreadCurve %></td>
                            <td rowspan="3" class="text lthourspread"><%: item.ExistingModifiedHours %></td>
                            <td rowspan="3" class="text lthourspread"><%: item.TotalHoursOffloaded %></td>
                            <td rowspan="3" class="text ltcost">$<%: item.TotalOffloadedCost %></td>
                            <td>Modified Hours</td>
                            <% // Now show the spread months for Existing
                                foreach (DateTime dt in Model.SpreadDates)
                                {
                                    ResourceSpreadDto spread = item.ExistingSpreads.FirstOrDefault(x => x.LaborSpreadDate.Year == dt.Year && x.LaborSpreadDate.Month == dt.Month);
                                    string spreadValue = spread == null ? string.Empty : Utilities.FormatStringWithPrecision(spread.LaborSpreadValue, Model.ResourceHoursPrecision);
                            %>
                            <td><%= spreadValue %></td>
                            <%
                            } // end foreach spread date %>
                        </tr>
                        <tr>
                            <td>Offloaded Hours</td>
                            <% // Now show the spread months for Offloaded Cost
                                foreach (DateTime dt in Model.SpreadDates)
                                {
                                    ResourceSpreadDto spread = item.OffloadHours.FirstOrDefault(x => x.LaborSpreadDate.Year == dt.Year && x.LaborSpreadDate.Month == dt.Month);
                                    string spreadValue = spread == null ? string.Empty : Utilities.FormatStringWithPrecision(spread.LaborSpreadValue, Model.ResourceHoursPrecision);
                            %>
                            <td><%= spreadValue %></td>
                            <%  } // end foreach spread date %>
                        </tr>
                        <tr>
                            <td>Offloaded Cost</td>
                            <% // Now show the spread months for Offloaded Cost
                                foreach (DateTime dt in Model.SpreadDates)
                                {
                                    ResourceSpreadDto spread = item.OffloadSpreads.FirstOrDefault(x => x.LaborSpreadDate.Year == dt.Year && x.LaborSpreadDate.Month == dt.Month);
                                    string spreadValue = spread == null ? string.Empty : "$" + Utilities.FormatStringWithPrecision(spread.LaborSpreadValue, Model.ResourceHoursPrecision);
                            %>
                            <td><%= spreadValue %></td>
                            <%  } // end foreach spread date %>
                        </tr>
                        <% }  %>
                    </tbody>
                </table>
            </div> 
            <% } else {  %>
            <div id="BOEOffloadResults-NoResults" style="text-align: center;">
                <div class="dialog-text">
                    <br />
                    <%: Model.NoResultsMessage %>
                    <br />
                    <br />
                </div>
            </div>
            <% } %>
        </div>
    </div>
    <% if (Model != null && Model.OffloadWarnings.Any()) { %>
    <br />
    <br />
    <div class="boeOffloadWarnings module expanded">
        <div class="module-header-data" >Labor Resource Offload Warnings</div>
        <div class="module-content-data expanded-content">
            <div class="boe-offload-results" >
                <table class="labor-types-grid grid offload-grid">
                    <thead>
                        <tr>
                            <th class="resource">Modified Resource</th>
                            <th class="resource">Offloaded Resource</th>
                            <th class="performing-org">Performing Org</th>
                            <th>Warning</th>
                        </tr>
                    </thead>
                    <tbody>
                    <%
                        foreach (OffloadLaborRatesValidationResults item in Model.OffloadWarnings)
                        {
                            %>
                        <tr>
                            <td class="resources"><%: item.ResourceName %></td>
                            <td class="resources"><%: item.SubResource == null ? string.Empty : item.SubResource.ResourceName %></td>
                            <td class="performing-org"><%: item.PerfOrgName %></td>
                            <td><%: item.InvalidWarningText %></td>
                        </tr>
                        <% }  %>
                    </tbody>
                </table>
            </div> 
        </div>
    </div>
    <% } %>
</div>
</body>
</html>
