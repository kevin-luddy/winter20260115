(function () {
    'use strict';

    var rdmApp = angular.module('RDM', [
        // Angular modules 
        //'ngRoute'
        'ngAria',
        'ngAnimate',
        'ngSanitize',
        'ui.grid',
        'ui.grid.edit',
        'ui.grid.cellNav', 
        'ui.grid.pinning',
        'ui.grid.selection',
        'ui.grid.exporter',
        'ui.grid.moveColumns',
        'ui.grid.resizeColumns',
        'ui.grid.pagination',
        'ui.sortable',
        'ui.tree',
        'ui.bootstrap',
        'ui.tinymce',
        // Custom modules 
        'gen.directives',
        'ngMaterial'
        // 3rd Party Modules
        
    ]);

    // Intercept ajax responses for global error handling.
    rdmApp.factory('globalErrorInterceptor', ['$rootScope', '$q', function ($rootScope, $q) {
        return {
            responseError: function (response) {
                $rootScope.errors = [];
                $rootScope.modalErrors = [];
                if (response.data !== undefined) {
                    if (response.data.ReturnType !== 'GenValidationException') {
                        DisplayExceptionDialog(response.data);
                    } else if (response.data.MessageList !== undefined) {
                        if (response.data.Message === "modal") {
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
    }]);

    // Global RateFormatter service for exporting with formatting.
    rdmApp.factory('RateFormatter', ['RATE_FORMATS', function (RATE_FORMATS) {

        return {
            //Format: function (grid, row, col, input) {
            //    var dictionaryKey = grid.target + row.entity.RateCategoryDescription;
            Format: function (target, row, input) {
                if (input === null) {
                    return null;
                }

                var format = RATE_FORMATS[target + 'None'];
                if (row !== undefined && row.entity !== undefined && row.entity.RCD !== undefined) {
                    format = RATE_FORMATS[target + row.entity.RCD];
                }

                if (format !== undefined) {
                    var numericVal = Number(input);
                    if (!isNaN(numericVal) && isFinite(numericVal)) {
                        var multiplier = Number(format.multiplier);
                        if (!isNaN(multiplier) && isFinite(multiplier) && multiplier > 0) {
                            // Adjust the precision by the decimal places of the multiplier(assuming it is always and even base 10 logarithm).
                            var newPrecision = format.precision - Math.log(format.multiplier) * Math.LOG10E;

                            // This multiplication can result in rounding errors.
                            numericVal *= multiplier;

                            // Round to new precision - if this is not currency, then remove trailing zeros.
                            numericVal = numericVal.toFixed(newPrecision);
                            if (!(format.prefix === '$' && format.precision === "2")) {
                                numericVal = Number(numericVal);
                            }
                        } else {
                            // Pad currency to the configured precision.
                            if (format.prefix === '$' && format.precision === "2") {
                                numericVal = numericVal.toFixed(format.precision);
                            }
                        }

                        // Including non-printable character keeps all the precision in excel, and the math still works in excel.
                        return String.fromCharCode(32) + format.prefix + numericVal + format.suffix;
                    }
                }
                return input;
            }
        };
    }]);

    /* This is needed because ui.grid doesn't behave well with angular 1.6.x yet */
    rdmApp.config(['$qProvider', function ($qProvider) {
        $qProvider.errorOnUnhandledRejections(false);
    }]);

    rdmApp.config(['$httpProvider', function ($httpProvider) {
        // Include this header with all ajax calls made by $http.  This allows "Request.IsAjaxRequest()" to work on the server.
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';

        $httpProvider.interceptors.push('globalErrorInterceptor');
    }]);

    rdmApp.config(['$mdThemingProvider', function ($mdThemingProvider) {
        $mdThemingProvider.theme('default')
            .primaryPalette('teal')
            .accentPalette('teal');
    }]);

    // Trying to improve IE performance - https://github.com/angular/angular.js/issues/15005 , https://docs.angularjs.org/guide/production
    rdmApp.config(['$compileProvider', function ($compileProvider) {
        $compileProvider.debugInfoEnabled(false);
        $compileProvider.commentDirectivesEnabled(false);
        $compileProvider.cssClassDirectivesEnabled(false);
    }]);

    rdmApp.filter('true_false', function () {
        return function (text, length, end) {
            if (text) {
                return 'Yes';
            }
            return 'No';
        };
    });

    /* Filter to convert a JSON Date to the specified date format for angular. */
    rdmApp.filter('jsonDate', ['$filter', function ($filter) {
        return function (dateInput, format) {
            return (dateInput) ? $filter('date')(parseInt(dateInput.substr(6)), format) : '';
        };
    }]);

    /* Filter for text dates */
    rdmApp.filter('textDate', ['$filter', function ($filter) {
        return function (dateInput, format) {
            var date = new Date(dateInput);
            return $filter('date')(date, format);
        };
    }]);

    // Filter to format rates displayed in grids.
    rdmApp.filter('formattedRate', ['RateFormatter', function (RateFormatter) {
        return function (value, row, target) {
            return RateFormatter.Format(target, row, value);
        };
    }]);

})();