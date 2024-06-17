angular.module('genboe').controller('BOEConfidenceReportController', ['$scope', '$window', 'ConfidenceReportModel', function ($scope, $window, ConfidenceReportModel) {
	$scope.confidenceScore = ConfidenceReportModel.model.ConfidenceScore;
	$scope.data = ConfidenceReportModel.model.ConfidenceReportData;
	$scope.displayErrors = $scope.data.length > 0;
	$scope.noTitle = "No Title";
	$scope.isExporting = false;
	$scope.columns = {
		boeTitle: 'BoeTitle',
		taskTitle: 'TaskTitle',
		moqTypes: 'MoqTypes',
		rteFields: 'RteFields',
		errors: 'ErrorText'
	};

	$scope.predicate = [$scope.columns.boeTitle];
	$scope.reverse = false;
	$scope.search = {};
	$scope.search.text = '';

	$scope.openBoe = function (boeId) {
		var url = CreatePostURL(ConfidenceReportModel.workspace, ConfidenceReportModel.boeController, ConfidenceReportModel.editBoeAction, 'boe/' + boeId);
		$window.open(url);
	}

	$scope.openTask = function (boeId, taskId) {
		var url = CreatePostURL(ConfidenceReportModel.workspace, ConfidenceReportModel.boeController, ConfidenceReportModel.editBoeAction, 'boe/' + boeId + "#LMLabor/task/" + taskId);
		$window.open(url);
	}

	$scope.sort = function (sortValue) {
		// clicking the same column reverses the sort
		if ($scope.predicate[0] === sortValue) {
			$scope.reverse = !$scope.reverse;
		} else {
			$scope.reverse = false;
			$scope.predicate = [sortValue];
		}

		$scope.SaveFilterToCookies();
	};

	$scope.boldSort = function (sortColumn) {
		return $scope.predicate[0] === sortColumn;
	};

	$scope.filterItems = function (data) {
		var searchText = $scope.search.text.toLowerCase();

		return (data.BoeTitle && data.BoeTitle.toLowerCase().indexOf(searchText) !== -1)
			|| ((!data.BoeTitle || data.BoeTitle == '') && $scope.noTitle.toLowerCase().indexOf(searchText) !== -1)
			|| (data.TaskTitle && data.TaskTitle.toLowerCase().indexOf(searchText) !== -1)
			|| (data.MoqTypes && data.MoqTypes.toLowerCase().indexOf(searchText) !== -1)
			|| (data.RteFields && data.RteFields.toString().indexOf(searchText) !== -1)
			|| (data.ErrorText && data.ErrorText.toLowerCase().indexOf(searchText) !== -1);
	}

	$scope.exportConfidenceReport = function () {
		var timeoutTime = 2000;
		$scope.isExporting = true;

		var exportUrl = CreatePostURL(ConfidenceReportModel.workspace, ConfidenceReportModel.reportsController, ConfidenceReportModel.exportConfidenceReportAction, '');
		GenWidget.prototype.performExport(exportUrl);

		// export is done via attaching an iframe, wait an arbitrary # of seconds (2-3) until showing the export button again to stop double-click
		$timeout(function () {
			$scope.isExporting = false;
		}, timeoutTime);

		// this is needed so IE does not stop animating gif(s) on the page
		return false;
	}
}]);