angular.module('RPM').controller('MetricsController', ['$scope', '$http', '$filter', '$cookies', '$window', function ($scope, $http, $filter, $cookies, $window) {

    $scope.firstLoad = true;
    $scope.isInitializing = true;
    $scope.isMetricsLoading = true;
    $scope.isMetricsError = false;
    $scope.years = [];
    $scope.Orgs = ['SSC'];

    $scope.model = {
        selectedOrg: 'SSC',
        selectedYear: "",
        selectedOrgChildrenIsLOB: true,  // true for LOB, false for PA
        filteredMetricsData: []
    };

    /* The Org changed, update dependent properties */
    $scope.OrgChanged = function() {
        if ($scope.model.selectedOrg == 'SSC') {
            $scope.model.selectedOrgChildren = $scope.model.LOBs;
            $scope.model.selectedOrgChildrenIsLOB = true;
        } else {
            $scope.model.selectedOrgChildrenIsLOB = false;
            $scope.model.selectedOrgChildren = $scope.model.LOBToPAs[$scope.model.selectedOrg];
        }

    }

    /* Save the filter settings */
    $scope.SaveSettings = function () {
        var now = new $window.Date(),
        // this will set the expiration to 6 months
        exp = new $window.Date(now.getFullYear(), now.getMonth() + 6, now.getDate());
        $cookies.put('yearMetrics', angular.toJson($scope.model.selectedYear), { expires: exp });
        $cookies.put('orgMetrics', angular.toJson($scope.model.selectedOrg), { expires: exp });
    }

    /* Load the filter settings */
    $scope.LoadSettings = function () {
        var cookies = $cookies.getAll();
        if ('orgMetrics' in cookies) {
            var org = JSON.parse(cookies['orgMetrics']);
            if (org != undefined && org != null) {
                $scope.model.selectedOrg = org;
                $scope.OrgChanged();
            }
        }
    }

    /* Updated the selected model fields based on the user selected Org */
    $scope.OrgClicked = function () {
        $scope.SaveSettings();
        $scope.OrgChanged();
        $scope.filterProposals();
        $scope.$broadcast('RefreshCharts');
    }

    /* Filters the proposals based on the selected Org */
    $scope.filterProposals = function () {
        var metrics = [];

        $scope.allMetricsData.forEach(function (proposal) {
            if ($scope.model.selectedOrgChildrenIsLOB || $scope.model.selectedOrg == proposal.LOB) {
                    metrics.push(proposal);
            }
        });

        $scope.model.filteredMetricsData = metrics;
    };

    /* builds up the years options for the select dropdown */
    buildYearsList = function () {
        var d = new Date();
        var maxYear = getMaxYearForDataFiltering(d);
        var currentYear = d.getFullYear();

        for (var i = 2012; i <= maxYear; i++) {
            $scope.years.push(i);
        }
        
        var cookies = $cookies.getAll();
        if ('yearMetrics' in cookies) {
            var year = JSON.parse(cookies['yearMetrics']);
            if (year != undefined && year != null) {
                currentYear = year;
            }
        }

        $scope.model.selectedYear = currentYear;

        $scope.isInitializing = false;
        $scope.loadMetricsData();
    }

    /* Converts a hash table to an array */
    convertToArray = function (data) {
        var newData = [];

        for (var key in data) {
            var item = data[key];

            newData.push(item);
        }

        return newData;
    };

    /* Loads the PTM Metrics Data */
    $scope.loadMetricsData = function () {
        if (!$scope.isInitializing) {
            $scope.isMetricsLoading = true;
            $scope.isMetricsError = false;
            $scope.allMetricsData = [];
            $scope.model.filteredMetricsData = [];

            return $http({
                method: 'POST',
                url: createPostURLWithYear('Home', 'RetrieveMetricsData', $scope.model.selectedYear)
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available

                var orgs = ['SSC'];
                var lobToPAs = {};
                var lobs = [];
                var contacts = {};
                var pricers = {};
                var propTypes = {}
                response.data.LOBs.forEach(function (lob) {
                    orgs.push(lob);
                    lobs.push(lob);
                    lobToPAs[lob] = [];
                });

                $scope.Orgs = orgs;
                $scope.model.LOBs = lobs;

                response.data.ProgramAreas.forEach(function (programArea) {
                    lobToPAs[programArea.LineOfBusiness].push(programArea.ProgramArea);
                });

                response.data.Proposals.forEach(function (proposal) {
                    addToArray(pricers, proposal.Pricer);
                    addToArray(contacts, proposal.Contact);
                    addToArray(propTypes, proposal.PropType);
                });

                $scope.model.pricers = convertToArray(pricers);
                $scope.model.contacts = convertToArray(contacts);
                $scope.model.propTypes = convertToArray(propTypes);
                $scope.model.LOBToPAs = lobToPAs;

                $scope.allMetricsData = response.data.Proposals;

                if ($scope.firstLoad) {
                    $scope.firstLoad = false;
                    $scope.LoadSettings();
                }

                $scope.isMetricsLoading = false;

                $scope.OrgClicked();
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                $scope.isMetricsLoading = false;
                $scope.isMetricsError = true;
            });
        }
    };

    /* Add a value to an array hashtable if defined and not null */
    var addToArray = function (theArray, stringValue) {
        if (stringValue !== undefined && stringValue != null) {
            theArray[stringValue] = stringValue;
        }
    }

    /* Create a POST url via parameters */
    var createPostURLWithYear = function (controllerName, actionName, yearSelected) {
        var postURL = window.location.protocol + '//' + window.location.host + '/' +
            controllerName + '/' + actionName + '/?year=' + yearSelected;

        return postURL;
    };

    // build the years dropdown list and load the data
    buildYearsList(); 
}]);