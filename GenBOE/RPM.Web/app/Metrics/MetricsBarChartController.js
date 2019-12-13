angular.module('RPM').controller('MetricsBarChartController', ['$scope', '$http', '$filter', function ($scope, $http, $filter) {
    
    $scope.caption = 'Submitted Proposals';
    $scope.barType = "Prop";
    $scope.labels = $scope.model.selectedOrgChildren;
    $scope.showTopXNote = false;
    
    /* Options that show the Legend */
    $scope.options = {
        stacked: true,
        scales: {
            xAxes: [{
                stacked: true
            }],
            yAxes: [{
                stacked: true
            }]
        },
        legend: {
            display: true,
            position: 'bottom',
        },
        tooltips: {
            callbacks: {
                label: function (tooltipItem, data) {
                    return $scope.series[tooltipItem.datasetIndex] + $scope.tooltips[tooltipItem.datasetIndex][tooltipItem.index];
                },
                title: function (tooltipItem, data) {
                    return $scope.labels[tooltipItem[0].index];
                }
            }
        },
        colors: ['#45b7cd', '#ff6384', '#ff8e72']
    };

    /* Calculate the tooltips based on the bin values */
    $scope.CalculateTooltips = function (bins) {
        var tooltips = [];
        bins.forEach(function (innerBin) {
            var innerToolTips = [];

            innerBin.forEach(function (bin) {
                var tooltip;
                if (bin < 0) {
                    tooltip = ': ($' + Math.abs(Math.round(bin)) + ')';

                } else {
                    tooltip = ': $' + Math.round(bin);
                }

                innerToolTips.push(tooltip);
            });
            tooltips.push(innerToolTips);
        });

        return tooltips;
    }

    /* Retrieve the Estimator from a proposal */
    GetEstimator = function (proposal) {
        return proposal.Pricer;
    }

    /* Retrieve the Contact from a proposal */
    GetContact = function (proposal) {
        return proposal.Contact;
    }

    /* Retrieve the PropType from a proposal */
    GetPropType = function (proposal) {
        return proposal.PropType;
    }

    /* Prop radio button clicked */
    $scope.PropClicked = function () {
        $scope.series = $scope.model.propTypes;
        LoadBins(GetPropType);
    };

    /* Contact radio button clicked */
    $scope.ContactClicked = function () {
        $scope.series = $scope.model.contacts;
        LoadBins(GetContact);
    };

    /* Estimator radio button clicked */
    $scope.EstimatorClicked = function () {
        $scope.series = $scope.model.pricers;
        LoadBins(GetEstimator);
    };

    /* Load the data into bins for the chart, update captions and tooltips */
    LoadBins = function (GetSeriesFunction) {
        $scope.RefreshChartCaptions();
        $scope.showTopXNote = false;
        var seriesToBins = {};
        var total = 0;
        var seriesToTotal = {};
        $scope.series.forEach(function (series) {
            var labelsToBins = {};
            seriesToTotal[series] = 0;
            $scope.labels.forEach(function (label) {
                labelsToBins[label] = 0;
            });

            seriesToBins[series] = labelsToBins;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            var seriesName = GetSeriesFunction(proposal);
            if ($scope.model.selectedOrgChildrenIsLOB) {
                seriesToBins[seriesName][proposal.LOB] += proposal.SVal;
            } else {
                seriesToBins[seriesName][proposal.PA] += proposal.SVal;
            }
            total += proposal.SVal;
            seriesToTotal[seriesName] += proposal.SVal;
        });

        $scope.series.forEach(function (series) {
            seriesToBins[series] = convertToArray(seriesToBins[series]);
        });

        var bins = convertToArray(seriesToBins);

        // Now remove any bins that have zero from both bins and series 
        // when showing names because there are so many names.  If still too many, 
        // then only show top 8
        if ($scope.barType == 'Contact' || $scope.barType == 'Estimator') {
            seriesToTotal = convertToArray(seriesToTotal);
            
            var sortedSeriesToTotal = seriesToTotal.slice(0).sort(function (a, b) { return b - a });
            var cutoffValue = 0;

            // Create a cutoff value such that only the top 8 series are shown in the chart
            if (sortedSeriesToTotal.length > 8 && sortedSeriesToTotal[8] > 0) {
                cutoffValue = sortedSeriesToTotal[8];
            }

            var nonZeroBins = [];
            var nonZeroSeries = [];

            for (var i = 0; i < seriesToTotal.length; i++) {
                if (seriesToTotal[i] > cutoffValue && Math.round((seriesToTotal[i] * 100 / total)) > 0) {
                    nonZeroBins.push(bins[i]);
                    nonZeroSeries.push($scope.series[i]);
                }
            }

            if (cutoffValue > 0 && nonZeroBins.length == 8) {
                $scope.showTopXNote = true;
            }

            $scope.series = nonZeroSeries;
            bins = nonZeroBins;
        }

        $scope.data = bins;
        $scope.tooltips = $scope.CalculateTooltips(bins);
    }

    /* Refresh the chart title and labels */
    $scope.RefreshChartCaptions = function () {
        $scope.title = $scope.model.selectedOrg + ' - Value ($M) by ' + $scope.barType + ' - ' + $scope.model.selectedYear;
        $scope.labels = $scope.model.selectedOrgChildren;
    };

    /* Refresh the charts called by parent controller */
    $scope.$on('RefreshCharts', function (e) {
        switch ($scope.barType) {
            case 'Prop':
                $scope.PropClicked();
                break;
            case 'Contact':
                $scope.ContactClicked();
                break;
            case 'Estimator':
                $scope.EstimatorClicked();
                break;
        }
    });

}]);