<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Dtos.MSTMetricDetailsDTO>" %>
<script type="text/javascript">

    var AddHistoricalMetricToBOE = new Widget("AddHistoricalMetricToBOE");

    /*
    * Return to Search Results from detail Measure view
    */
    AddHistoricalMetricToBOE.BackToResults = function () {
        $(document).trigger('BACK_TO_RESULTS_MST');
    };

    /*
    * Add metric to BOE
    */
    AddHistoricalMetricToBOE.AddMetricToBOE = function () {
        var AddMetricdata = {};
        AddMetricdata.ID = $("#MetricID").val();

        //Data needed to populate MOQ Rationale field
        var metricData = {};
        metricData["Measure Name:"] = $("#MetricTitle").text(); //Always include the Measure Name

        //Only copy the Basic Info into the BOE
        $("#BasicInfo li").each(function () {
            var label = $(this).find('.metric-label').text();
            var data = $(this).find('.metric-data').text();
            metricData[label] = data;
        });

        AddMetricdata.metricData = metricData;

        //Data needed for Historical Measures Used table
        AddMetricdata.MeasureName = $("#MetricTitle").text();
        AddMetricdata.ProgramName = $("#ProgramName").text();
        AddMetricdata.DateApplied = $.datepicker.formatDate('mm/dd/yy', new Date());//gets current date

        $(document).trigger('ADD_METRIC_TO_BOE_MST', AddMetricdata);
    };

    /*
    *After DOM Load
    */
    $(function () {
        AddHistoricalMetricToBOE.afterDOMLoad();
        $('#AddHistoricalMetricToBOE-BackToResults').click(AddHistoricalMetricToBOE.BackToResults);
        $('#AddHistoricalMetricToBOE-AddMetricToBOE').click(AddHistoricalMetricToBOE.AddMetricToBOE);

        // find the parent with the title so we can change it
        var titlebar = $(".ui-dialog-titlebar");
        if (titlebar.length > 0) {
            titlebar.find(".ui-dialog-title").text("Historical Measures: Measure Detail");
        }
    });

</script>
<div id="AddHistoricalMetricToBOE" class="AddHistoricalMetricToBOE" style="width: 800px; min-height: 320px; max-height: 740px; overflow: auto; overflow-x: hidden">
    <div id="AddDialogDescription"><a id="MeasureResult-HelpLink" class="info-link" tabindex="-1" title="Learn more about Historical Measure Details" href="<%:Model.MSTMetricDetailHelpLink%>" target="_blank"></a> Measure details are displayed below.</div>
    <br/>
    <br/>
    <%: Html.HiddenFor(x => Model.Id, new { @id="MetricID" }) %>
    <div style="font-weight: bold; font-size: medium">
        <div class="inLineBlock" id="MetricTitle"><%:Model.MeasureName %></div>
    </div>
    <div class="historical-metrics-search-results">
        <div class="full-text-width">
            <div class="divider"></div>
        </div>
        <div class="metric-label">Basic Information</div>
        <div id="BasicInfo">
            <div class="first-two-column">
                <ul class="historical-metric-search-results-list">
                    <li><span class="metric-label">Measure Description:</span>&nbsp;<span class="metric-data"><%: Model.MeasureDescription %></span></li>
                    <li><span class="metric-label">Program/Project Name:</span>&nbsp;<span id="ProgramName" class="metric-data"><%: Model.ProgramName %></span></li>
                    <li><span class="metric-label">Contract Number:</span>&nbsp;<span class="metric-data"><%: Model.ContractNumber %></span></li>
                    <%--Display the shortened version of the charge numbers, but leave the full list hidden so it can be inserted into the MOQ Rationale field--%>
                    <li><span class="metric-label">SAP Charge Numbers:</span>&nbsp;<span class="metric-data display-none"><%: Model.WorkPackages %></span><%: Model.WorkPackagesShort %></li>
                </ul>
            </div>
            <div class="second-two-column">
                <ul class="historical-metric-search-results-list">
                    <li><span class="metric-label">PoP Start Date:</span>&nbsp;<span class="metric-data"><%: Model.StartDate != null ? Model.StartDate.Value.ToString("MM/dd/yyyy") : "N/A" %></span></li>
                    <li><span class="metric-label">PoP End Date:</span>&nbsp;<span class="metric-data"><%: Model.EndDate != null ? Model.EndDate.Value.ToString("MM/dd/yyyy") : "N/A" %></span></li>
                    <li><span class="metric-label">Base Measures:</span>&nbsp;<span class="metric-data"><%: Model.BaseMeasures %></span></li>                    
                    <li><span class="metric-label">Equation:</span>&nbsp;<span class="metric-data"><%: Model.Equation %></span></li>
                    <li><span class="metric-label">Measure Data:</span>&nbsp;<span class="metric-data"><%: Model.MeasureDataFormatted %></span></li>
                    <li><span class="metric-label">Data Source:</span>&nbsp;<span class="metric-data"><%: Model.DataSource %></span></li>              
                </ul>
            </div>
        </div>
        <div class="full-text-width">
            <div class="divider"></div>
        </div>
        <div class="full-text-width">
            <div class="metric-label">Additional Information</div>
        </div>
        <div id="AdditionalInfo">
            <div class="first-two-column">
                <ul class="historical-metric-search-results-list">
                    <li><span class="metric-label">Scope Name:</span>&nbsp;<span class="metric-data"><%: Model.ScopeName %></span></li>
                    <li><span class="metric-label">Business Area:</span>&nbsp;<span class="metric-data"><%: Model.BusinessArea %></span></li>
                    <li><span class="metric-label">Line of Business:</span>&nbsp;<span class="metric-data"><%: Model.LineOfBusiness %></span></li>
                    <li><span class="metric-label">Measure Group:</span>&nbsp;<span class="metric-data"><%: Model.MeasureGroupName %></span></li>
                    <li><span class="metric-label">Measure Category:</span>&nbsp;<span class="metric-data"><%: Model.MeasureCategoryName %></span></li>
                    <li><span class="metric-label">Comments:</span>&nbsp;<span class="metric-data"><%: Model.Comment %></span></li>
                </ul>
            </div>
            <div class="second-two-column">
                <ul class="historical-metric-search-results-list">
                    <li><span class="metric-label">Measure Qualifier:</span>&nbsp;<span class="metric-data"><%: Model.MeasureQualifier %></span></li>
                    <li><span class="metric-label">Measure Link:</span>&nbsp;<span class="metric-data"><a tabindex="-1" href="<%: Model.MeasureLink %>" target="_blank"><%: Model.MeasureLink %></a></span></li>
                    <li><span class="metric-label">Measure Function:</span>&nbsp;<span class="metric-data"><%: Model.MeasureFunction %></span></li>
                    <li><span class="metric-label">Measure Validation:</span>&nbsp;<span class="metric-data"><%: Model.MeasureValidationDate %></span></li>
                    <li><span class="metric-label">Program/Project Description:</span>&nbsp;<span class="metric-data"><%: Model.ProgramDescription %></span></li>
                </ul>
            </div>
        </div>
    </div>
    <div class="full-text-width">
        <div class="divider"></div>
    </div>
    <div id="MetricDetailButtons" style="text-align: center; display: block;">
        <div class="form-row">
            <div class="buttons">
                <button id="AddHistoricalMetricToBOE-BackToResults" class="ies" type="button">Back to Search Results</button>
                <button id="AddHistoricalMetricToBOE-AddMetricToBOE" class="ies-action" type="button">Copy to BOE</button>
            </div>
        </div>
    </div>
</div>
