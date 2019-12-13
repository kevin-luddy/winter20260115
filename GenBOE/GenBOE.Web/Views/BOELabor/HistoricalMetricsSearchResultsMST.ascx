<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.HistoricalMetricsFromMSTModelView>" %>

<script type="text/javascript">

    var HistoricalMetricsResultsWidget = new Widget("HistoricalMetricsResults");
    
    /*
    * Display detailed information for a specific Measure
    */
    HistoricalMetricsResultsWidget.DisplayResult = function () {
        var data = {};
        data.metricId = $(this).attr('metricID');
        data.getFromSource = true;
        $(document).trigger('METRIC_SELECTED', data);
    };

    /*
    * Return to search dialog
    */
    HistoricalMetricsResultsWidget.SearchAgain = function () {
        $(document).trigger('SEARCH_METRICS_AGAIN_MST');
    };

    /*
    * Cancel search results dialog
    */
    HistoricalMetricsResultsWidget.Cancel = function () {
        $(document).trigger('CANCEL_SEARCH_METRICS');
    };

    $(function () {
        HistoricalMetricsResultsWidget.afterDOMLoad();
        $('a[name=DisplayResult]').click(HistoricalMetricsResultsWidget.DisplayResult);

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('#PageControls');
        pagingData.action = function(data) {
            delete data.HistoricalMetricsResults;
            $(document).trigger('PageHistoricalMetricSearch', data);
        };
        pagingData.type = "PageNumber";

        HistoricalMetricsResultsWidget.AddPaging(pagingData);

        $('#HistoricalMetricsResults-SearchAgin').click(HistoricalMetricsResultsWidget.SearchAgain);
        $('#HistoricalMetricsResults-CancelButton').click(HistoricalMetricsResultsWidget.Cancel);
    });
    
</script>


<% if (Model.MetricsSearchResults.Count > 0)
   { %>
<div style="text-align: right;">
    <a id="MeasureResult-HelpLink" class="info-link" title="Learn more about Historical Measure Search Results" tabindex="-1" href="<%:Model.MSTSearchResultsHelpLink%>" target="_blank"></a>
    <div id="PageControls" class="paging-control"></div>
</div>

<div style="width: 800px; min-height: 200px; max-height: 540px; overflow: auto; overflow-x: hidden">

    <div id="HistoricalMetricsResults" class="historical-metrics-search-results">
        <div class="form-row">
            <div class="form-element">Measures matching your search criteria are displayed below. Select the Measure name for additional information. Options for copying this measure into your BOE are provided on the additional Measure Details display. 
                <br/>Up to the first 1000 Measures are shown. If you do not find the Measure you are looking for, try refining your search.</div>
        </div>
        <div class="full-text-width">

            <div class="divider historical-metrics-divider"></div>
        </div>
        <% foreach (GenBOE.Dtos.MSTMetricDetailsDTO item in Model.MetricsSearchResults)
           { %>

        <div class="full-text-width">
            <a name="DisplayResult" tabindex="1" metricid="<%:item.Id %>"><%:item.MeasureName%></a><br />
        </div>
        <div class="first-two-column">
            <ul class="historical-metric-search-results-list">
                <li><span class="metric-label">Measure Description:</span>&nbsp;<%: item.MeasureDescription %></li>
                <li><span class="metric-label">Program/Project Name:</span>&nbsp;<%: item.ProgramName %></li>
                <li><span class="metric-label">Contract Number:</span>&nbsp;<%: item.ContractNumber %></li>
                <li><span class="metric-label">SAP Charge Numbers:</span>&nbsp;<%: item.WorkPackagesShort %></li>  
            </ul>
        </div>
        <div class="second-two-column">
            <ul class="historical-metric-search-results-list">
                <li><span class="metric-label">PoP Start Date:</span>&nbsp;<%:item.StartDate != null ? item.StartDate.Value.ToString("MM/dd/yyyy") : "N/A" %></li>
                <li><span class="metric-label">PoP End Date:</span>&nbsp;<%: item.EndDate != null ? item.EndDate.Value.ToString("MM/dd/yyyy") : "N/A" %></li>
                <li><span class="metric-label">Base Measures:</span>&nbsp;<%: item.BaseMeasures %></li>
                <li><span class="metric-label">Equation:</span>&nbsp;<%: item.Equation %></li>
                <li><span class="metric-label">Measure Data:</span>&nbsp;<%: item.MeasureDataFormatted %></li>                
                <li><span class="metric-label">Data Source:</span>&nbsp;<%: item.DataSource %></li>
            </ul>
        </div>
        <div class="full-text-width">
            <div class="divider"></div>
        </div>
        <% } %>
    </div>
</div>
<% }
   else
   { %>
<div class="full-text-width">
    <div class="text-noresults">No results found.</div>
    <div class="divider"></div>
</div>
<% } %>
<div class="form-row">
    <div class="form-label"></div>
    <div class="form-element" style="display: block;">
        <div class="buttons">
            <button id="HistoricalMetricsResults-SearchAgin" class="ies" type="button">Search again</button>
            <button id="HistoricalMetricsResults-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
    </div>
</div>

<%--<div name="PageControls" style="margin-top: 10px;"></div>--%>
