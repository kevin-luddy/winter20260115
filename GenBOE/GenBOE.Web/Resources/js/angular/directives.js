angular.module('gen.directives', [])

    /** 
    * Displays a validation box including the given error array, and hides the box when no errors exist.
    *
    * @example <gen-validation data-errors="errors"></gen-validation>
    * 
    * @param {array} errors - Array of errors objects display.  Each error must contain a message property.
    */
    .directive('genValidation', ['$sce', '$timeout', function ($sce, $timeout) {
        return {
            restrict: 'A, E',
            replace: true,
            transclude: false,
            scope: {
                errors: '=',
                data: '=classtype'
            },
            template:
                '<ul class="hidden-validation validation-directive {{data}}validation-box" data-ng-show="errors.length">' +
                '<li data-ng-repeat="error in errors">' +
                '<div data-ng-bind-html="displayError(error.ValidationIssue)"></div>' +
                '</li>' +
                '</ul>',
            link: function (scope, element, attrs) {
                $timeout(function () {
                    $('.hidden-validation').removeClass('hidden-validation');
                }, 1);

                scope.displayError = function (val) {
                    return $sce.trustAsHtml(val);
                };

                scope.getValidationType = function () {
                    if (isWarning === 'true') {
                        return 'warning-validation-box';
                    } else {
                        return 'validation-box';
                    }
                };

                scope.$on('$destroy', function () { });

            }
        };
    }])

    /** 
    * Displays a user lookup text box and lookup link
    *
    * @example <gen-user-lookup data-ntids="ntids" data-readonly="false"></gen-user-lookup>
    * 
    * @param {string} ntids - ntids from the AD lookup
    * @param {bool} readonly - true to put the control in readonly mode.
    */
    .directive('genUserLookup', function () {
        return {
            restrict: 'A, E',
            replace: true,
            transclude: false,
            scope: {
                ntids: '=',
                readonly: '='
            },
            template: '<div><input class="ad-lookup-box" data-ng-model="ntids" data-ng-disabled="!readonly" placeholder="NTID or Active Directory Group" />' +
                '<a data-ng-click="lookup()" data-ng-hide="!readonly" title="Lookup">Lookup...</a></div>',
            link: function (scope, element, attrs) {
                scope.lookup = function () {
                    ActiveDirectorySearchDialog(function (results) {
                        scope.$apply(function () {
                            var selectedAccountName = results.AccountName;
                            if (selectedAccountName != '') {

                                if ($.trim(scope.ntids).length == 0) {
                                    scope.ntids = selectedAccountName;
                                } else {
                                    // If there isn't already a semicolon on the end of the user/group string, we'll add one
                                    if (!$.trim(scope.ntids).endsWith(';')) {
                                        scope.ntids += ';';
                                    }
                                    scope.ntids += selectedAccountName;
                                }
                            }
                        });
                    });
                };

                scope.$on('$destroy', function () { });
            }
        };
    })

    /**
    * Wraps the jQuery UI dialog in a directive.
    * 
    * @example <gen-dialog><dialog content></gen-dialog>
    *
    * @param {bool} [autoOpen=false] - Auto open on declaration.
    * @param {string} title
    * @param [width=600] 
    * @param [height=auto]
    * @param {string} [show] - Animation when the dialog is opened.
    * @param {string} [hide] - Animation when the dialog is closed.
    * @param {bool} [draggable=true]
    * @param {bool} [resizable=false]
    * @param {bool} [closeOnEscape=false]
    * @param {bool} open - bi-directional boolean to open and close the dialog.
    * @param {bool} [focusOnOpen=true] - Focus into the first visible and enabled input.
    * @param {function} [onOpen] - Method to call when the dialog opens.
    * @param {function} [onClose] - Method to call when the dialog closes.
    * @param {function} [onEnter] - Method to call when the user hits the enter key.
    */
    .directive('genDialog', ['$timeout', function ($timeout) {
        return {
            restrict: 'A, E',
            replace: false,
            transclude: true,
            scope: {
                autoOpen: '@',
                title: '@',
                width: '@',
                height: '@',
                show: '@',
                hide: '@',
                draggable: '@',
                resizable: '@',
                closeOnEscape: '@',
                open: '=',
                focusOnOpen: '@',
                onOpen: '&',
                onClose: '&',
                onEnter: '&'
            },
            template: '<div data-ng-transclude></div>',
            link: function (scope, element, attrs) {
                var exists = false;
                attrs.focusOnOpen = attrs.focusOnOpen ? attrs.focusOnOpen : true;

                // by default we want to hide the dialog window. this line will set the display attribute to none
                // upon each creation of this dialog directive
                element.css('display', 'none');

                // set parent's $.open to false when the dialog is closed.
                var onClose = function () {
                    if (scope.open === true) {
                        scope.$apply(function () {
                            scope.open = false;
                        });
                        // prevent the onClose method from firing twice by preventing this close 
                        // and relying on the scope.open observer to perform the close.
                        return false;
                    }
                };

                var dialogOptions = {
                    autoOpen: attrs.autoOpen ? attrs.autoOpen : false,
                    title: attrs.title,
                    width: attrs.width || 600,
                    height: attrs.height || 'auto',
                    modal: attrs.modal ? attrs.modal.toLowerCase() == 'true' : true,
                    show: attrs.show || null,
                    hide: attrs.hide || null,
                    draggable: attrs.draggable ? attrs.draggable.toLowerCase() == 'true' : true,
                    resizable: attrs.resizable ? attrs.resizable.toLowerCase() == 'true' : false,
                    closeOnEscape: attrs.closeOnEscape ? attrs.closeOnEscape.toLowerCase() == 'true' : false,
                    beforeClose: onClose,
                    close: scope.onClose || null,
                    open: scope.onOpen || null
                };

                // Initialize the element as a dialog
                // Timeout is required to support multiple dialogs
                $timeout(function () {
                    element.dialog(dialogOptions);
                    exists = true;
                }, 0);

                // watch the parent scope's open property to open and close the dialog.
                scope.$watch('open', function (val) {
                    if (exists) {
                        if (val === true) {
                            element.dialog('open');

                            // focus on open
                            var inputs = element.find(':input:visible:enabled');
                            if (attrs.focusOnOpen === true && inputs.length > 0) {
                                inputs[0].focus();
                            }
                            else {
                                // Focus was still being set for some reason, so blur it to remove it.
                                if (inputs.length > 0) {
                                    inputs[0].blur();
                                }
                            }
                        }
                        else {
                            element.dialog('close');
                        }
                    }
                });

                // This allows title to bind and change.
                attrs.$observe('title', function (val) {
                    if (exists) {
                        element.dialog('option', 'title', val);
                    }
                });

                // Setup the onEnter function.
                if (scope.onEnter) {
                    element.on('keydown', function (e) {
                        var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                        if (keyCode == 13) {
                            scope.onEnter();
                        }
                    });
                }

                // Destory the dialog on directive destruction.
                scope.$on('$destroy', function () {
                    element.dialog('destroy');
                });
            }
        };
    }])

    /**
    * Directive for the common error dialog.
    * 
    * @example <gen-error-dialog></gen-error-dialog>
    *
    * @param {string} text
    * @param {string} details - Animation when the dialog is opened.
    * @param {bool} open - bi-directional boolean to open and close the dialog.
    */
    .directive('genErrorDialog', ['$sce', function ($sce) {
        return {
            restrict: 'A, E',
            replace: true,
            scope: {
                open: '=',
                text: '@',
                details: '@'
            },
            template:
                '<div>' + // required since this and gen-dialog both need isolate scope, using <div gen-dialg> instead of <gen-dialog> because IE8 breaks.
                '<div gen-dialog style="display: none; text-align: center;" data-width="300", data-title="Application Error" data-open="open" data-focus-on-open="false">' +
                '<div class="dialog-text" data-ng-bind-html="displayText()"></div>' +
                '<div data-ng-init="animate.visible=false">' +
                '<span class="underline pointer" data-ng-click="animate.visible=!animate.visible">Error Details</span>' +
                '<div class="vertical-slide error-details" data-ng-bind-html="displayDetails()" data-ng-hide="!animate.visible" style="margin-bottom: 0px"></div>' +
                '</div>' +
                '<div class="error-buttons" style="margin-top: 10px">' +
                '<button type="button" class="ies" name="ok-button" data-ng-click="closeDialog()">OK</button>' +
                '</div>' +
                '</div>' +
                '</div>',
            link: function (scope, element, attrs) {
                scope.closeDialog = function () {
                    scope.open = false;
                };

                scope.displayText = function () {
                    return $sce.trustAsHtml(scope.text);
                };

                scope.displayDetails = function () {
                    return $sce.trustAsHtml(scope.details);
                };
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
    * Wraps the jQuery UI datepicker in a directive.
    * 
    * @example <gen-datepicker></gen-datepicker>
    */
    .directive('genDatepicker', function () {
    })

    /**
    * Wraps the jQuery UI datepicker in a directive to be used for monthly datepicker inside ui-grid.
    * 
    */
    .directive('jqdatepicker', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, ctrl) {
                $(element).datepicker({
                    changeMonth: true,
                    changeYear: true,
                    dateFormat: 'mm/yy',
                    showButtonPanel: true,
                    showOn: "button",
                    closeText: "Save",
                    beforeShow: function (input, inst) {
                        $('#ui-datepicker-div').addClass('hide-calendar');

                        var selDate;
                        if ((selDate = $(this).val()).length > 0) {
                            var iYear = selDate.substring(selDate.length - 4, selDate.length);
                            var iMonth = parseInt(selDate.substring(0, selDate.length - 5));
                            // change iMonth from 1-based to 0-based
                            iMonth = iMonth - 1;
                            var date = new Date(iYear, iMonth, 1);
                            $(this).datepicker('option', 'monthNames');
                            $(this).datepicker('option', 'defaultDate', date);
                            $(this).datepicker('setDate', date);
                        }
                    },
                    onClose: function (dateText, inst) {

                        function isDonePressed() {
                            return ($('#ui-datepicker-div').html().indexOf('ui-datepicker-close ui-state-default ui-priority-primary ui-corner-all ui-state-hover') > -1);
                        }

                        if (isDonePressed()) {
                            var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
                            var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
                            var date = new Date(year, month, 15);
                            $(this).datepicker('setDate', date).trigger('change');
                            scope.$apply();
                        }

                        scope.$emit('uiGridEventEndCellEdit');
                    }
                });
            }
        };
    })

    /**
    * Directive for paging
    * 
    * @example <div paging></div>
    *
    * @param {int} numPages - total number or pages to be displayed
    * @param {int} currentPage - currently displayed page of results
    */
    .directive('genpaging', function ($sce) {
        return {
            restrict: "A",
            replace: true,
            scope: {
                numPages: '@',
                currentPage: '='
            },
            template: '<div class="paging">' +
                '<a data-ng-class="{\'current-page\': currentPage==0, page:currentPage!=0}" data-ng-click="pageNumClicked(0)")>1</a>' +
                '<span class="elipsis" data-ng-show="showFirstElipsis()">...</span>' +
                '<a data-ng-class="{\'current-page\': currentPage==page, page:currentPage!=page}" data-ng-repeat="page in getPageNumbers(numPages)" data-ng-click="pageNumClicked(page)">{{ page + 1 }}</a>' +
                '<span class="elipsis" data-ng-show="showSecondElipsis()">...</span>' +
                '<a data-ng-class="{\'current-page\': currentPage==numPages-1, page:currentPage!=numPages-1}" data-ng-show="showLastPageNum()" data-ng-click="clickLastPage()">{{numPages}}</a>' +
                '</div>',
            link: function (scope, element, attrs) {
                scope.firstElipsis = false;
                scope.secondElipsis = false;

                scope.showLastPageNum = function () {
                    return (scope.numPages > 1 && scope.currentPage != scope.numPages - 1);
                };
                scope.showFirstElipsis = function () {
                    return scope.firstElipsis;
                };
                scope.showSecondElipsis = function () {
                    return scope.secondElipsis;
                };

                scope.pageNumClicked = function (page) {
                    scope.currentPage = page;
                };

                scope.clickLastPage = function () {
                    scope.currentPage = scope.numPages - 1;
                };

                scope.getPageNumbers = function (n) {
                    scope.firstElipsis = false;
                    scope.secondElipsis = false;
                    var pagenumbers = [];
                    var currentPage = scope.currentPage;
                    var numPages = scope.numPages;
                    if (currentPage < 7) {
                        for (var i = 1; i < currentPage; i++) {
                            pagenumbers.push(i);
                        }
                    }
                    else {
                        scope.firstElipsis = true;
                        for (var i = currentPage - 4; i < currentPage; i++) {
                            pagenumbers.push(i);
                        }
                    }
                    if (currentPage > 0 && currentPage < numPages) {
                        pagenumbers.push(currentPage);
                    }

                    if (numPages < currentPage + 6) {
                        for (var i = currentPage + 1; i < numPages - 1; i++) {
                            pagenumbers.push(i);
                        }
                    }
                    else {
                        for (var i = currentPage + 1; i < currentPage + 4; i++) {
                            pagenumbers.push(i);
                        }
                        scope.secondElipsis = true;
                    }
                    return pagenumbers;
                };
            }
        };
    })

    .directive('customOnChange', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var onChangeHandler = scope.$eval(attrs.customOnChange);
                element.bind('change', onChangeHandler);
            }
        };
    })

    .directive('jqSpinner', function () {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, element, attrs, c) {
                element.spinner({
                    spin: function (event, ui) {
                        c.$setViewValue(ui.value);
                    }
                });
            }
        };
    })

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
    }])

    /**
    * Filter for showing PickList Items that are set to Active or are initially selected
    * Note: If an item is inactive, but initially selected then it will show up (in the dropdown), but if it is unselected it will disappear!!
    * 
    * @param {json[]} items - array of json objects, automatically set by angular
    * @param {int} initial - The initial Pick List ID (which can be in-active but will still show up while selected)
    */
    .filter('isActiveOrInitialPickListItem', function () {
        return function (items, initial) {
            var filtered = [];
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item.IsActive || item.Id === initial) {
                    filtered.push(item);
                }
            }
            return filtered;
        };
    })


    /**
     * Filter for UI converting boolean to Yes or No
     */
    .filter('yesNo', function () {
        return function (input) {
            return input ? 'Yes' : 'No';
        }
    })

    /**
    * Filter for showing values in a ui-grid drop-down cell, when options containing Id and Label properties are being used.
    * 
    * @param {int} input - The Id of the selected option
    * @param {json[]} map - array of json objects, automatically set by angular
    * @param {string} idField - The map property containing the id, i.e. "Id"
    * @param {string} valueField - The map property containing the value, i.e. "Label"
    * @param {string} initial - The initial value of the cell
    */
    .filter('griddropdown', function () {
        return function (input, map, idField, valueField, initial) {
            if (typeof map !== "undefined") {
                for (var i = 0; i < map.length; i++) {
                    if (map[i][idField] == input) {
                        return map[i][valueField];
                    }
                }
            } else if (initial) {
                return initial;
            }
            return input;
        };
    })

    // filter used by MOQ Types
    .filter('moqTypesFilter', function () {
        return function (moqOptions, selectedMoqTypes) {
            var out = [];

            for (var i = 0; i < moqOptions.length; i += 1) {
                var id = moqOptions[i].SelectedMOQType;
                if (selectedMoqTypes.map(e => e.SelectedMOQType.toString()).indexOf(id.toString()) < 0) {
                    out.push(moqOptions[i]);
                }
            }
            return out;
        };
    });
