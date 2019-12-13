(function () {
    'use strict';

    var rdsbApp = angular.module('rdsb', [
        // Angular modules 
        'ui.grid',
        //'ngRoute'

        // Custom modules 
        'gen.directives',
        // 3rd Party Modules
        'ui.bootstrap'
        
    ]);

    // Intercept ajax responses for global error handling.
    rdsbApp.factory('globalErrorInterceptor', ['$rootScope', '$q', function ($rootScope, $q) {
        return {
            responseError: function (response) {
                $rootScope.errors = [];
                $rootScope.modalErrors = [];
                if (response.data != undefined) {
                    if (response.data.ReturnType != 'GenValidationException') {
                        DisplayExceptionDialog(response.data);
                    } else if (response.data.MessageList != undefined) {
                        if (response.data.Message == "modal") {
                            // these are validation messages for a modal popup
                            $rootScope.modalErrors = response.data.MessageList;
                        } else {
                            $rootScope.errors = response.data.MessageList;
                        }
                    }
                }

                $(document).trigger("HIDE_LOADING_BOX");
                return $q.reject(response);
            }
        };
    }])

    rdsbApp.config(['$httpProvider', '$qProvider', function ($httpProvider, $qProvider) {
        // Include this header with all ajax calls made by $http.  This allows "Request.IsAjaxRequest()" to work on the server.
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

        $httpProvider.interceptors.push('globalErrorInterceptor');

        //Adding to suppress javascript errors due to Angular 1.6 not being supported by ui-grid
        //http://stackoverflow.com/a/41993170/1359742
        //https://github.com/angular-ui/ui-grid/issues/5890
        $qProvider.errorOnUnhandledRejections(false);
    }]);

    rdsbApp.filter('true_false', function () {
        return function(text, length, end) {
            if (text) {
                return 'Yes';
            }
            return 'No';
        }
    });

    rdsbApp.directive('jqdatepicker', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function(scope, element, attrs, ctrl) {
                $(element).datepicker({
                    changeMonth: true,
                    changeYear: true,
                    dateFormat: 'mm/dd/yy',
                    onSelect: function(date) {
                        ctrl.$setViewValue(date);
                        ctrl.$render();
                        scope.$apply();
                    }
                });
            }
        };
    });

    // Element attribute to prevent the attached field from triggering the dirty state.
    rdsbApp.directive('ignoreDirty', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ctrl) {
                ctrl.$setPristine = function () { };
                ctrl.$pristine = false;
            }
        };
    });

})();