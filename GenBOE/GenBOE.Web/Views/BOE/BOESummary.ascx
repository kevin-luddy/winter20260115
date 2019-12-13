<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Dtos.BOESummaryGridModelView>>"%>

<script type="text/javascript">

    var displayBoeSummaryUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
           '<%:WebConstants.CONTROLLER_BOE%>',
           '<%:WebConstants.ACTION_DISPLAY_BOE_SUMMARY %>',
           'boe/<%: ViewData["BOEID"]%>');

    var BOESummary = InitializeBOESummaryWidget(displayBoeSummaryUrl);

    // OnLoad
    $(function () {

        var hoursUpdatedEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_HOURS_UPDATED %>';
        var travelHoursUpdatedEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_TRAVEL_HOURS_UPDATED %>';
        var reloadSummaryEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';
        AfterDomLoadBOESummaryWidget(BOESummary, hoursUpdatedEvent, travelHoursUpdatedEvent, reloadSummaryEvent);
    });
</script>

<div class="boesummary module expanded">

    <div class="module-header-data">BOE Summary</div>
    <div class="module-content-data">
        <table id="LaborTypeSummaryHeaderTable">
        <thead>
            <tr>
                <th class="labor-type">
                    Resource Type
                </th>
                <th class="labor-hours">
                    <%: ViewData["HoursLabel"] %>
                </th>
                <th class="labor-cost">
                    Cost
                </th>
            </tr>
            </thead>
          </table>
          </div>
    <div class="module-content-data expanded-content">
     <table id="LaborTypeSummaryContentTable">
    
    <% 
       decimal? hourTotal = 0;
       decimal costTotal = 0;
       string decimalPrecisionStringFormat = "F0";
       int decimalPrecision = 0;

       %>
        <tbody>
        <%
        if (Model.Any())
        { 
           foreach (var item in Model.Where(x => x.ModeID != MSTTravelMode.ZoneNoAirfare && x.ModeID != MSTTravelMode.ZoneAirfare))
           {               
               %>
                <tr class="BOESummary-LaborType" nameofelement="<%: item.LaborType %>" resourcetype="<%:item.LaborType%>" category="<%:(int)item.Category %>" rollupcount="<%:item.RollupCount%>" precision="<%:item.ResourceDecimalPrecision%>">
                    <td class="labor-type">
                    <div class="summaryText"><%: item.LaborType%></div>
                    </td>
                        <%decimalPrecisionStringFormat = item.DecimalPrecisionStringFormat;
                          decimalPrecision = item.ResourceDecimalPrecision;
                          if (item.TotalHours != 0)
                          {
                              hourTotal += item.TotalHours;
                         %>
                            <td title="<%: item.TotalHoursFormatted %>" class="labor-hours" value="<%: item.TotalHoursFormatted%>">
                            <div class="summaryText"><%: item.TotalHoursFormatted%></div>
                        <%}
                          else
                          { %>
                            <td title="0" class="labor-hours" value="0">
                            <div class="summaryText"></div>
                        <%} %>
                    </td>
                    <%if (item.TotalCost.HasValue && item.TotalCost != 0)
                      {
                          costTotal += item.TotalCost.Value;
                            %>
                             <td title="<%: item.TotalCost.Value.ToString("N2")%>" class="labor-cost" value="<%: item.TotalCost%>">
                                <div class="summaryText"><%: item.TotalCost.Value.ToString("N2")%></div>
                        <%}
                      else { %> <td class="labor-cost" value="0"><div class="summaryText"></div><%} %>
                    </td>
                </tr>
         <%}
       } %>
            </tbody>
        </table>
    </div>
    <div class="module-content-data">
        <input type="hidden" id="BOESummary-decimal-precision" value="<%:decimalPrecision%>"/>
        <table id="LaborTypeSummaryTotalHours">
            <tr id="BOESummary-TotalHours" class="total">
                <td class="labor-type">
                    <div class="summaryText">Total</div>
                </td>
                <% string hourTotalString = hourTotal.HasValue ? hourTotal.Value.ToString(decimalPrecisionStringFormat) : string.Empty; %>
                <td title="<%: hourTotalString %>" id="BOESummary-TotalHours-Total" class="labor-hours" value="<%: hourTotalString %>">
                    <div class="summaryText"><%: hourTotalString %></div>
                </td>
                <td title="<%: costTotal.ToString("N2")%>" id="BOESummary-TotalCost-Total" class="labor-cost" value="<%: costTotal %>">
                    <div class="summaryText"> <%: costTotal.ToString("N2")%></div>
                </td>
            </tr>
        </table>
 
    <%-- conditional based on if there are any zone travel trips otherwise hide entire zone travel boe summary--%>
    <%if ( Model.Any(x => x.ModeID == MSTTravelMode.ZoneNoAirfare || x.ModeID == MSTTravelMode.ZoneAirfare) )
    { 
    %>
   
    <div class="module-zonetravelheader-data"></div>
        
        <div class="module-content-data">
        <table>
        <thead>
            <tr> <td colspan="3"><br /></td></tr>
            <tr>
                <th class="zonelabor-type">Zone Travel <br />Resource Type </th>
                <th class="zonelabor-people"># People </th>
                <th class="zonelabor-days"> # Days </th>
            </tr>
            </thead>
          </table>
          </div>
      <div class="module-content-data expanded-content">
     <table  >
   
        <tbody>
        <%

            foreach (var item in Model.Where(x => x.ModeID == MSTTravelMode.ZoneNoAirfare || x.ModeID == MSTTravelMode.ZoneAirfare))
            {
                if (item.ModeID == MSTTravelMode.ZoneAirfare) item.NumDays = null; // don't show days (or 0) for zone air
               %>
                <tr >
                    <td><div class="zonelabor-type"><%: item.LaborType   %></div></td>
                    <td><div class="zonelabor-people"><%: item.NumPeople%></div></td>
                    <td><div class="zonelabor-days"><%: item.NumDays%></div></td>
                </tr>
         <%}%>
            </tbody>
        </table>
    </div>
  
<% } %>
        </div>
</div>