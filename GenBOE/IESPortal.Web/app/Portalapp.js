(function () {
    'use strict';

    var portalApp = angular.module('portal', [
        // Angular modules 
        'ngAnimate',
        'ngSanitize',
        // Custom modules 
        'gen.directives',
        // 3rd Party Modules
        'ui.bootstrap'
    ]);

    // Intercept ajax responses for global error handling.
    portalApp.factory('globalErrorInterceptor', ['$rootScope', '$q', function ($rootScope, $q) {
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

    portalApp.config(['$httpProvider', '$qProvider', function ($httpProvider, $qProvider) {
        // Include this header with all ajax calls made by $http.  This allows "Request.IsAjaxRequest()" to work on the server.
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

        $httpProvider.interceptors.push('globalErrorInterceptor');

        //Adding to suppress javascript errors due to Angular 1.6 not being supported by ui-grid
        //http://stackoverflow.com/a/41993170/1359742
        //https://github.com/angular-ui/ui-grid/issues/5890
        $qProvider.errorOnUnhandledRejections(false);
    }]);
})();