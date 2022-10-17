angular.module('genboe').controller('WorkspaceCalculateActualsController', ['$scope', '$http', '$timeout', '$filter', 'WorkspaceCalculateActualsModel', function ($scope, $http, $timeout, $filter, WorkspaceCalculateActualsModel) {
	$scope.columns = {
		table: 'TableName',
        boe: 'BoeTitle',
		boePrevState: 'BoeStatePrevious',
		task: 'Task',
		hoursPrevious: 'TotalRelevantHoursPrevious',
		hours: 'TotalRelevantHours',
		order: 'Order',
		wbsHoursPrevious: 'WbsHoursPrevious',
		wbsHours: 'WbsHours'
    };
    $scope.errors = [];
    $scope.modalErrors = [];
    $scope.checkAllClicked = false;
    $scope.createBOEsDisabled = true;
    $scope.containsOCI = true;
    $scope.availableClins = [];
    $scope.isExporting = false;
    // Calculate Actuals can be done when workspace state is Working
	$scope.isWorkingState = WorkspaceCalculateActualsModel.workspaceState === 'Working';
	
    $scope.data = [];
    $scope.isLoading = true;

	$scope.reverse = false;
	$scope.predicate = [$scope.columns.order];
    $scope.pageSize = 100;
    $scope.currentPage = 0;
    $scope.searchText = '';
	$scope.colSpan = WorkspaceCalculateActualsModel.colSpan;

    // reset current page when a user searches
    $scope.searchChanged = function () {
        $scope.currentPage = 0;
	}

	/*
     * ***************** NOTE ******************
     * Functions below relate to the table logic
     * *****************************************
     */
	// determines when to show filtering message
	$scope.isDataFiltered = function () {
		return $scope.searchText.length > 0;
	};

	$scope.filterActuals = function (data) {
		const compareValue = $scope.searchText.toLowerCase();
		
		// filter based on search term
		if (compareValue === undefined || compareValue === '' ||
			(data.BoeTitle && data.BoeTitle.toLowerCase().indexOf(compareValue) !== -1) ||
			(data.BoePrevState && data.BoePrevState.toLowerCase().indexOf(compareValue) !== -1) ||
			(data.TableName && data.TableName.toLowerCase().indexOf(compareValue) !== -1) ||
			(data.Task && data.Task.toLowerCase().indexOf(compareValue) !== -1)) {
			return true;
		}

		// nothing matches so return false
		return false;
	};

	$scope.changeSorting = function (sortValue) {
        // clicking the same column reverses the sort
        if ($scope.predicate[0] === sortValue) {
            $scope.reverse = !$scope.reverse;
        } else {
            $scope.reverse = false;
            $scope.predicate = [sortValue];
        }
    };

    // returns true when the column being sorted matches the first index
    $scope.boldSort = function (sortColumn) {
        return $scope.predicate[0] === sortColumn;
    };

    $scope.numberOfPages = function (numFilteredRows) {
        const totalRows = angular.isDefined(numFilteredRows) ? numFilteredRows.length : 0;
        return Math.ceil(totalRows / $scope.pageSize);
    }

    /*
     * *********** NOTE ************
     * Private variables & functions 
     * *****************************
     */
    let loadActuals = function () {
        $scope.isLoading = true;
        $scope.data = [];
        $scope.errors = [];

        return $http({
            method: 'POST',
			url: CreatePostURL(WorkspaceCalculateActualsModel.workspace, WorkspaceCalculateActualsModel.controller, WorkspaceCalculateActualsModel.action, '')
        }).then(function (response) {

			$scope.data = response.data;

			$scope.isLoading = false;
            
        }, function errorCallback(response) {
            $scope.errors = response.data.MessageList;
            $scope.isLoading = false;
        });
    }    

    // load the main data
	loadActuals();
}]);