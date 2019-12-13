angular.module('RPM').controller('MetricsROSBarChartController', ['$scope', '$http', '$filter', function ($scope, $http, $filter) {
    
    $scope.caption = 'Submitted Proposals ROS%';
    $scope.labels = $scope.model.selectedOrgChildren;
    $scope.series = ['ROS'];

    $scope.options = {
        legend: {
            display: true,
            position: 'bottom',
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
        },
        colors: ['#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd', '#45b7cd']
    };

    $scope.CalculateTooltips = function (bins) {
        var tooltips = [];
        bins.forEach(function (bin) {
            var tooltip;
            var percentage = Math.round(bin * 10) / 10;
            if (bin < 0) {
                tooltip = '(' + Math.abs(percentage) + ') , %';

            } else {
                tooltip = percentage + '%';
            }

            tooltips.push(tooltip);
        });

        return tooltips;
    }

   /* Refresh the chart title and labels */
    $scope.RefreshCharts = function () {
        $scope.title = $scope.model.selectedOrg + ' - ROS% - ' + $scope.model.selectedYear; 
        $scope.labels = $scope.model.selectedOrgChildren;

        var labelsToBins = {};
        var labelsToTotalBins = {};

        $scope.labels.forEach(function (label) {
            labelsToBins[label] = 0;
            labelsToTotalBins[label] = 0;
        });

        $scope.model.filteredMetricsData.forEach(function (proposal) {
            if (proposal.Profit != undefined && proposal.SVal != undefined && proposal.SVal != 0) {
                if ($scope.model.selectedOrgChildrenIsLOB) {
                    labelsToBins[proposal.LOB] += proposal.Profit;
                    labelsToTotalBins[proposal.LOB] += proposal.SVal;
                } else {
                    labelsToBins[proposal.PA] += proposal.Profit;
                    labelsToTotalBins[proposal.PA] += proposal.SVal;
                }
            }
        });

        for (var i = 0; i < labelsToBins.length; i++) {
            if (labelsToTotalBins[i] == 0) {
                labelsToBins[i] = 100.0;
            } else {
                labelsToBins[i] = labelsToBins[i] / labelsToTotalBins[i];
            }
        }

        var bins = convertToArray(labelsToBins);

        $scope.tooltips = $scope.CalculateTooltips(bins); 
        $scope.data = [ bins ];

    };

    $scope.$on('RefreshCharts', function (e) {
        $scope.RefreshCharts();
    });

}]);