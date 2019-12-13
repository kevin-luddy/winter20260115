angular.module('RPM').controller('MetricsPieChartController', ['$scope', function ($scope) {
    
    $scope.options = {
        legend: {
            display: true,
            position: 'bottom'
        },
        tooltips: {
            callbacks: {
                label: function (tooltipItem, data) {
                    return $scope.tooltips[tooltipItem.index];
                },
                title: function (tooltipItem, data) {
                    return $scope.labels[tooltipItem[0].index];
                }
            }
        }
    };

    /* Refresh the charts based on which type of Pie chart this is */
    $scope.RefreshCharts = function () {
        switch ($scope.type)
        {
            case "Value":
                $scope.caption = 'Submitted Proposals by Value Range';
                $scope.title = $scope.model.selectedOrg + ' - Total Value ($M) by Range - ' + $scope.model.selectedYear; 
                $scope.data = $scope.GetValueChartData();
                break;
            case "Org":
                if ($scope.model.selectedOrgChildrenIsLOB) {
                    $scope.caption = 'Submitted Proposals by LOB ($M)';
                    $scope.title = $scope.model.selectedOrg + ' - Total Value ($M) by LOB - ' + $scope.model.selectedYear; 
                    $scope.data = $scope.GetLOBChartData();
                } else {
                    // LOB is selected, so show PA
                    $scope.caption = 'Submitted Proposals by PA ($M)';
                    $scope.title = $scope.model.selectedOrg + ' - Total Value ($M) by PA - ' + $scope.model.selectedYear; 
                    $scope.data = $scope.GetPAChartData();
                }
                break;
            case "OrgNum":
                if ($scope.model.selectedOrgChildrenIsLOB) {
                    $scope.caption = 'Submitted Proposals by LOB (#)';
                    $scope.title = $scope.model.selectedOrg + ' - Total Number by LOB - ' + $scope.model.selectedYear;
                    $scope.data = $scope.GetLOBNumberChartData();
                } else {
                    // LOB is selected, so show PA
                    $scope.caption = 'Submitted Proposals by PA (#)';
                    $scope.title = $scope.model.selectedOrg + ' - Total Number by PA - ' + $scope.model.selectedYear;
                    $scope.data = $scope.GetPANumberChartData();
                }
                break;
            case "Contract":
                $scope.caption = 'Submitted Proposals by Contract Type ($M)';
                $scope.title = $scope.model.selectedOrg + ' - Total Value ($M) by Type - ' + $scope.model.selectedYear; 
                $scope.data = $scope.GetContractChartData();
                break;
            case "ContractNum":
                $scope.caption = 'Submitted Proposals by Contract Type (#)';
                $scope.title = $scope.model.selectedOrg + ' - Total Number by Type - ' + $scope.model.selectedYear;
                $scope.data = $scope.GetContractNumberChartData();
                break;
            default:
                break;
        }
    };

    /* Calculate the tooltips based on the bin values and the total */
    $scope.CalculateTooltips = function (bins, total) {
        var tooltips = [];
        bins.forEach(function (bin) {
            var tooltip;
            var percentage = Math.round(bin * 100 / total);

            if ($scope.type === "ContractNum" || $scope.type === "OrgNum") {
                tooltip = bin + ' , ' + percentage + '%';
            } else {
                
                if (bin < 0) {
                    tooltip = '($' + Math.round(bin) + ') , ' + Math.abs(percentage) + '%';

                } else {
                    tooltip = '$' + Math.round(bin) + ' , ' + percentage + '%';
                }
            }
            tooltips.push(tooltip);
        });

        return tooltips;
    }

    /* Get the Chart Data binned into Value Ranges */
    $scope.GetValueChartData = function () {
        $scope.labels = ['Credit Proposal', '$0 - $700K', '$700K - $5M', '$5M - $50M', '> $50M'];
        var bins = [0, 0, 0, 0, 0];
        var total = 0;

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            if (proposal.SVal < 0) {
                bins[0] += proposal.SVal;
            } else if (proposal.SVal < 0.7) {
                bins[1] += proposal.SVal;
            } else if (proposal.SVal < 5) {
                bins[2] += proposal.SVal;
            } else if (proposal.SVal < 50) {
                bins[3] += proposal.SVal;
            } else {
                bins[4] += proposal.SVal;
            }

            total += Math.abs(proposal.SVal);
        });

        $scope.tooltips = $scope.CalculateTooltips(bins, total);  

        // The first bin is negative, make this positive so that chart.js does not break
        bins[0] *= -1;
        return bins;
    };

    /* Get the Chart Data binned into LOBs by Value */
    $scope.GetLOBChartData = function () {
        $scope.labels = $scope.model.selectedOrgChildren;

        var total = 0;
        var LOBsToBins = {};
        $scope.model.LOBs.forEach(function (lob) {
            LOBsToBins[lob] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            LOBsToBins[proposal.LOB] += proposal.SVal;
            total += proposal.SVal;
        });

        var bins = convertToArray(LOBsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total);
        return bins; 
    };

    /* Get the Chart Data binned into LOBs by Number */
    $scope.GetLOBNumberChartData = function () {
        $scope.labels = $scope.model.selectedOrgChildren;

        var total = 0;
        var LOBsToBins = {};
        $scope.model.LOBs.forEach(function (lob) {
            LOBsToBins[lob] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            LOBsToBins[proposal.LOB] += 1;
            total += 1;
        });

        var bins = convertToArray(LOBsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total);
        return bins;
    };

    /* Get the Chart Data binned into Program Areas by Value */
    $scope.GetPAChartData = function() {
        $scope.labels = $scope.model.selectedOrgChildren;

        var total = 0;
        var labelsToBins = {};
        $scope.labels.forEach(function (pa) {
            labelsToBins[pa] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            labelsToBins[proposal.PA] += proposal.SVal;
            total += proposal.SVal;
        });

        var bins = convertToArray(labelsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total); 
        return bins; 
    };

    /* Get the Chart Data binned into Program Areas by Number*/
    $scope.GetPANumberChartData = function () {
        $scope.labels = $scope.model.selectedOrgChildren;

        var total = 0;
        var labelsToBins = {};
        $scope.labels.forEach(function (pa) {
            labelsToBins[pa] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            labelsToBins[proposal.PA] += 1;
            total += 1;
        });

        var bins = convertToArray(labelsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total);
        return bins;
    };

    /* Get the Chart Data binned into Contract Types showing value */
    $scope.GetContractChartData = function() {
        $scope.labels = ['CP', 'FP', 'Hybrid', 'IDIQ', 'IWTA', 'T&M', 'Other'];
        var labelsToBins = {};
        var total = 0;
        $scope.labels.forEach(function (name) {
            labelsToBins[name] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            labelsToBins[proposal.ContType] += proposal.SVal;
            total += Math.abs(proposal.SVal);
        });

        var bins = convertToArray(labelsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total);  
        return bins; 
    };

    /* Get the Chart Data binned into Contract Types showing Number*/
    $scope.GetContractNumberChartData = function () {
        $scope.labels = ['CP', 'FP', 'Hybrid', 'IDIQ', 'IWTA', 'T&M', 'Other'];
        var labelsToBins = {};
        var total = 0;
        $scope.labels.forEach(function (name) {
            labelsToBins[name] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            labelsToBins[proposal.ContType] += 1;
            total += 1;
        });

        var bins = convertToArray(labelsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins, total);
        return bins;
    };


    /* Refresh the charts called by parent controller */
    $scope.$on('RefreshCharts', function (e) {
        $scope.RefreshCharts();
    });
}]);