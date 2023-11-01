// The controller for the Editing of a Banner
angular.module('portal').controller('editBannerController', ['$scope', '$http', '$window', 'BannerModelView', '$rootScope', function ($scope, $http, $window, BannerModelView, $rootScope) {
    $scope.model = BannerModelView.data;
    $scope.isDataLoading = true;
    $rootScope.errors = [];
    $scope.isSaving = false;
    $scope.date = new Date();
    $scope.time = new Date();
    $scope.offset = '';
    $scope.selectedApps = [
        {
            Text: 'BOE RMS',
            Value: 'BOERMS'
        },
        {
            Text: 'BOE SSC',
            Value: 'BOESSC'
        },
        {
            Text: 'BOE SSC International',
            Value: 'BOESSC_INTL'
        },
        {
            Text: 'PTM',
            Value: 'PTM'
        },
        {
            Text: 'RDM',
            Value: 'RDM'
        },
        {
            Text: 'RDSB',
            Value: 'RDSB'
        },
        {
            Text: 'RPM',
            Value: 'RPM'
        },
        {
            Text: 'IES Portal',
            Value: 'IESPortal'
        }
    ];


    $scope.popupDatePickerStart = {
        opened: false
    };

    $scope.openDatePickerStart = function () {
        $scope.popupDatePickerStart.opened = true;
    };

    $scope.dateOptions = {
        formatYear: 'yyyy',
        startingDay: 1
    };

    $scope.saveBanner = function () {
        if ($scope.bannerForm.$valid) {

            $scope.isSaving = true;
            $rootScope.errors = [];

            // Combine the date and time to get the real Start Time as a String
            // Reminder that javascript Date.getMonth() returns 0-11
            var start = ($scope.date.getMonth() + 1).toString() + "/" + $scope.date.getDate() + "/" + $scope.date.getFullYear() + " " + $scope.time.getHours() + ":" + $scope.time.getMinutes() + ":00";
            $scope.model.StartDateAsString = start;

            var url = createPostURL(BannerModelView.controller, BannerModelView.saveBannerAction, $scope.model.Id);

            var model = angular.copy($scope.model);
            model.SelectedApps = [];

            if (!model.TurnOffTicker) {
                model.BannerText = '';
            }

            // Since there are multiple checkboxes, for some reason there are values of '' that need stripped out
            if ($scope.model.SelectedApps !== undefined && $scope.model.SelectedApps.length > 0) {
                $scope.model.SelectedApps.forEach(function (item) {
                    if (item !== undefined && item !== '') {
                        model.SelectedApps.push(item);
                    }
                });
            }

            $http({
                method: "POST",
                url: url,
                data: model
            }).then(function successCallback(response) {
                $(document).trigger("DISPLAY_NOTIFICATION", 'Banner changes saved');
                $scope.bannerForm.$setPristine();
                $window.location.href = createPostURL(BannerModelView.controller, BannerModelView.bannersView);
            }, function errorCallback(response) {
                $scope.isSaving = false;
            });
        }
    };

    $scope.cancel = function () {
        $window.location.href = createPostURL(BannerModelView.controller, BannerModelView.bannersView);
    };

    $scope.initialize = function () {
        $scope.isInitializing = false;
        $scope.loadData();
    };

    $scope.loadData = function () {
        if (!$scope.isInitializing) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Clear error messages.
            $rootScope.errors = [];
            $scope.isDataLoading = true;

            var date = BannerModelView.data.StartDateAsString;
            var d = Date.parse(date);
            $scope.date = new Date(d);
            $scope.time = new Date(d);
            if ($scope.bannerForm) {
                $scope.bannerForm.$setPristine();
            }

            $scope.resetEndDate();
            $(document).trigger("HIDE_LOADING_BOX");
            $scope.isDataLoading = false;
        }
    };

    $scope.resetEndDate = function () {
        if ($scope.model.HoursToShow <= 0) {
            $scope.offset = "No End Date";
        } else {
            var start = ($scope.date.getMonth() + 1).toString() + "/" + $scope.date.getDate() + "/" + $scope.date.getFullYear() + " " + $scope.time.getHours() + ":" + $scope.time.getMinutes() + ":00";
            var d = Date.parse(start);
            var offset = new moment(new Date(d)).add($scope.model.HoursToShow, 'hours').toDate();
            $scope.offset = offset.toLocaleDateString() + " " + offset.toLocaleTimeString();
        }
    }

    window.onbeforeunload = function () {
        // Confirm leaving/refreshing page if the page is dirty
        if ($scope.bannerForm.$dirty) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    };

    // Initialization functions
    $scope.initialize();
}]);