angular.module('gen.directives', [])

/** 
* Displays a validation box including the given error array, and hides the box when no errors exist.
*
* @example <gen-validation data-errors="errors"></gen-validation>
* 
* @param {array} errors - Array of errors objects display.  Each error must contain a message property.
*/
.directive('genValidation', ['$sce', function ($sce) {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: false,
        scope: { errors: '=' },
        template:
            '<ul class="validation-box validation-directive" data-ng-show="errors.length">' +
                '<li data-ng-repeat="error in errors">' +
                    '<div data-ng-bind-html="displayError(error.ValidationIssue)"></div>' +
                '</li>' +
            '</ul>',
        link: function (scope, element, attrs) {

            scope.displayError = function (val) {
                return $sce.trustAsHtml(val);
            };

            scope.$on('$destroy', function () { });
        }
    };
}])

/**
* Directive for a submit button (any button that requires a loading image).
* 
* @example <gen-submit-button></gen-submit-button>
*
* @param {css} any css classes to add to the button
* @param {function} click - the function to call on click
*/
.directive('genSubmitButton', ['$q', function ($q) {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: true,
        scope: {
            css: '@',
            click: '&'
        },
        template: '<button type="button" class="{{ css }}" data-ng-transclude data-ng-click="load()">',
        link: function (scope, element, attrs) {
            scope.load = function () {
                element.addClass('loader');
                element.prev('css3pie').addClass('display-none');
                var defer = $q.defer();

                // When the promise is resolved, this code runs to remove the loader and display the background in ie.
                defer.promise.then(function () {
                    element.removeClass('loader');
                    element.prev('css3pie').removeClass('display-none');
                });

                // pass the promise to the click
                scope.click({ result: defer });
            };
        }
    };
}])

/**
 * Checklist-model
 * AngularJS directive for list of checkboxes
 * https://github.com/vitalets/checklist-model
 * License: MIT http://opensource.org/licenses/MIT
 */

.directive('checklistModel', ['$parse', '$compile', function ($parse, $compile) {
    // contains
    function contains(arr, item, comparator) {
        if (angular.isArray(arr)) {
            for (var i = arr.length; i--;) {
                if (comparator(arr[i], item)) {
                    return true;
                }
            }
        }
        return false;
    }

    // add
    function add(arr, item, comparator) {
        arr = angular.isArray(arr) ? arr : [];
        if (!contains(arr, item, comparator)) {
            arr.push(item);
        }
        return arr;
    }

    // remove
    function remove(arr, item, comparator) {
        if (angular.isArray(arr)) {
            for (var i = arr.length; i--;) {
                if (comparator(arr[i], item)) {
                    arr.splice(i, 1);
                    break;
                }
            }
        }
        return arr;
    }

    // http://stackoverflow.com/a/19228302/1458162
    function postLinkFn(scope, elem, attrs) {
        // exclude recursion, but still keep the model
        var checklistModel = attrs.checklistModel;
        attrs.$set("checklistModel", null);
        // compile with `ng-model` pointing to `checked`
        $compile(elem)(scope);
        attrs.$set("checklistModel", checklistModel);

        // getter for original model
        var checklistModelGetter = $parse(checklistModel);
        var checklistChange = $parse(attrs.checklistChange);
        var checklistBeforeChange = $parse(attrs.checklistBeforeChange);
        var ngModelGetter = $parse(attrs.ngModel);



        var comparator = function (a, b) {
            if (!isNaN(a) && !isNaN(b)) {
                return String(a) === String(b);
            } else {
                return angular.equals(a, b);
            }
        };

        if (attrs.hasOwnProperty('checklistComparator')) {
            if (attrs.checklistComparator[0] == '.') {
                var comparatorExpression = attrs.checklistComparator.substring(1);
                comparator = function (a, b) {
                    return a[comparatorExpression] === b[comparatorExpression];
                };

            } else {
                comparator = $parse(attrs.checklistComparator)(scope.$parent);
            }
        }

        // watch UI checked change
        var unbindModel = scope.$watch(attrs.ngModel, function (newValue, oldValue) {
            if (newValue === oldValue) {
                return;
            }

            if (checklistBeforeChange && (checklistBeforeChange(scope) === false)) {
                ngModelGetter.assign(scope, contains(checklistModelGetter(scope.$parent), getChecklistValue(), comparator));
                return;
            }

            setValueInChecklistModel(getChecklistValue(), newValue);

            if (checklistChange) {
                checklistChange(scope);
            }
        });

        // watches for value change of checklistValue
        var unbindCheckListValue = scope.$watch(getChecklistValue, function (newValue, oldValue) {
            if (newValue != oldValue && angular.isDefined(oldValue) && scope[attrs.ngModel] === true) {
                var current = checklistModelGetter(scope.$parent);
                checklistModelGetter.assign(scope.$parent, remove(current, oldValue, comparator));
                checklistModelGetter.assign(scope.$parent, add(current, newValue, comparator));
            }
        }, true);

        var unbindDestroy = scope.$on('$destroy', destroy);

        function destroy() {
            unbindModel();
            unbindCheckListValue();
            unbindDestroy();
        }

        function getChecklistValue() {
            return attrs.checklistValue ? $parse(attrs.checklistValue)(scope.$parent) : attrs.value;
        }

        function setValueInChecklistModel(value, checked) {
            var current = checklistModelGetter(scope.$parent);
            if (angular.isFunction(checklistModelGetter.assign)) {
                if (checked === true) {
                    checklistModelGetter.assign(scope.$parent, add(current, value, comparator));
                } else {
                    checklistModelGetter.assign(scope.$parent, remove(current, value, comparator));
                }
            }

        }

        // declare one function to be used for both $watch functions
        function setChecked(newArr, oldArr) {
            if (checklistBeforeChange && (checklistBeforeChange(scope) === false)) {
                setValueInChecklistModel(getChecklistValue(), ngModelGetter(scope));
                return;
            }
            ngModelGetter.assign(scope, contains(newArr, getChecklistValue(), comparator));
        }

        // watch original model change
        // use the faster $watchCollection method if it's available
        if (angular.isFunction(scope.$parent.$watchCollection)) {
            scope.$parent.$watchCollection(checklistModel, setChecked);
        } else {
            scope.$parent.$watch(checklistModel, setChecked, true);
        }
    }

    return {
        restrict: 'A',
        priority: 1000,
        terminal: true,
        scope: true,
        compile: function (tElement, tAttrs) {

            if (!tAttrs.checklistValue && !tAttrs.value) {
                throw 'You should provide `value` or `checklist-value`.';
            }

            // by default ngModel is 'checked', so we set it if not specified
            if (!tAttrs.ngModel) {
                // local scope var storing individual checkbox model
                tAttrs.$set("ngModel", "checked");
            }

            return postLinkFn;
        }
    };
}]);