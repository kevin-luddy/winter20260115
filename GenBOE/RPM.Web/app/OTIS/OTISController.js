angular.module('RPM').controller('OTISController', ['$scope', '$http', '$filter', '$cookies', '$window', function ($scope, $http, $filter, $cookies, $window) {
    $scope.isOTISError = false;
    $scope.firstLoad = true;
    $scope.isOTISLoading = true;
    $scope.selectedOTIS = null;
    $scope.hideOTISDetails = true;
    $scope.filteredOTISCount = 0;
    $scope.filteredOTISValue = 0.0;
    $scope.allOTISData = [];
    $scope.ganttOTISData = [];
    $scope.predicateOTIS = 'Title';
    $scope.reverseOTIS = false;
    $scope.pageSizeOTIS = 17;
    $scope.currentOTISPage = 0;

    // Filter arrays for OTIS filter dropdowns    
    $scope.filterOTIS = {
        contractType: {}, oppType: {}, status: {}, lob: {}, customer: {}, referenceNumber: {},
        classified: {}, competitive: {}, category: {}, pa: {},
        captMgr: {}, propMgr: {}, role: {}, bdLead: {}, ceLead: {}
    };

    $scope.boolArray = function (val) {
        if (val == true) return 'Yes';
        if (val == false) return 'No';
        return val;
    }

    // Selected values for OTIS Filters
    $scope.selectedOTISFilters = {
        contractType: blank,
        oppType: blank,
        status: blank,
        lob: blank,
        customer: blank,
        referenceNumber: blank,
        classified: blank,
        competitive: blank,
        category: blank,
        pa: blank,
        captMgr: blank,
        propMgr: blank,
        role: blank,
        bdLead: blank,
        ceLead: blank,
        nameContains: blank,
        includePropMgr: false,
        includeCELead: false,
        includeValue: false,
        startDate: '',
        endDate: '',
        startDateAsDate: undefined,
        endDateAsDate: undefined,
        minValue: '',
        maxValue: '',
        international: blank
    };

    /* Options for the OTIS Gantt Chart */
    $scope.optionsOTIS =
        {
            scale: 'week',
            sortMode: undefined,
            sideMode: 'Table',
            columns: ['model.Title'],
            columnsHeaders: { 'model.Title': 'Opportunity' },
            columnsClasses: { 'model.Title': 'gantt-column-name-wide', },
            columnsHeaderContents: {
                'model.Title': '<span class="gantt-column-sortable-header" ng-click="toggleSort(\'Title\')">{{getHeader()}}</span>'//,
            },
            fromDate: null,
            toDate: null,
            taskContent: '',
            allowSideResizing: true,
            api: function (api) {
                // Important: Define function in gantt scope
                api.gantt.$scope.toggleSort = function (column) {
                    if ($scope.predicateOTIS == column) {
                        $scope.reverseOTIS = !$scope.reverseOTIS;
                    } else {
                        $scope.reverseOTIS = false;
                    }

                    $scope.predicateOTIS = column;
                    $scope.refreshOTISData(true);
                }

                // Set api object on $scope so that we can refresh data later
                $scope.api = api;

                api.core.on.ready($scope, function () {

                    // Load the data then add the events (it has to be this order for the directives to bind correctly)
                    loadOTISData().then(function () {

                        //Add some DOM events
                        api.directives.on.new($scope, function (directiveName, directiveScope, element) {
                            if (directiveName === 'ganttTask') {
                                element.bind('click', function (event) {
                                    event.stopPropagation();
                                    $scope.$apply($scope.selectedOTIS = directiveScope.task.row);
                                    $scope.$apply($scope.hideOTISDetails = false);
                                });
                            } else if (directiveName === 'ganttRow') {
                                element.bind('click', function (event) {
                                    event.stopPropagation();
                                    $scope.$apply($scope.selectedOTIS = directiveScope.row);
                                    $scope.$apply($scope.hideOTISDetails = false);
                                });
                            } else if (directiveName === 'ganttRowLabel') {
                                element.bind('click', function () {
                                    $scope.$apply($scope.selectedOTIS = directiveScope.row);
                                    $scope.$apply($scope.hideOTISDetails = false);
                                });
                            }
                        })
                    });

                    api.side.setWidth(360);
                })
            }
        };

    /* Refresh the OTIS gantt data */
    $scope.refreshOTISData = function (resetCurrentPage) {
        if (resetCurrentPage) {
            $scope.currentOTISPage = 0;
        }

        $scope.filterOTISGanttRows();
        if (angular.isDefined($scope.api)) {
            $scope.api.rows.refresh();
        }
    }

    /* Push data through filters before adding into the OTIS Gantt chart */
    $scope.filterOTISGanttRows = function () {
        $scope.filteredOTISCount = 0;
        $scope.filteredOTISValue = 0.0;
        var data = [];


        data = $scope.allOTISData.filter(function (row) {
            return $scope.filterOTISOpportunities(row);
        });

        data = $filter('orderBy')(data, $scope.predicateOTIS, $scope.reverseOTIS);

        $scope.filteredOTISCount = data.length;
        data.forEach(function (row) {
            $scope.filteredOTISValue += row.EVal;
        });

        // paging
        data = $filter('limitTo')(data, $scope.pageSizeOTIS, $scope.currentOTISPage * $scope.pageSizeOTIS);
        var minDate = null;
        var maxDate = null;
        if (data.length > 0) {
            data.forEach(function (row) {
                if (row.StartDate !== undefined && row.StartDate != null && (minDate == null || row.StartDate < minDate)) {
                    minDate = new Date(row.StartDate);
                }
                if (row.EndDate !== undefined && row.EndDate != null && (maxDate == null || row.EndDate > maxDate)) {
                    maxDate = new Date(row.EndDate);
                }
            });
        }

        // always add 2 months on the end so that annotation will display without being truncated
        if (maxDate != null) {
            maxDate.setMonth(maxDate.getMonth() + 2);

            if (minDate != null) {
                // check that we have at least 32 weeks showing
                var sevenMonths = new Date();
                sevenMonths.setFullYear(minDate.getFullYear());
                // add 32 weeks
                sevenMonths.setMonth(minDate.getMonth());
                sevenMonths.setDate(minDate.getDate() + 7 * 32);

                if (maxDate < sevenMonths) {
                    maxDate = sevenMonths;
                }
            }
        }

        if (minDate == null || maxDate == null) {
            $scope.optionsOTIS.fromDate = new Date();
            var sevenMonths = new Date();
            sevenMonths.setFullYear($scope.optionsOTIS.fromDate.getFullYear());
            // check that we have at least 32 weeks showing
            sevenMonths.setMonth($scope.optionsOTIS.fromDate.getMonth());
            sevenMonths.setDate($scope.optionsOTIS.fromDate.getDate() + 7 * 32);
            $scope.optionsOTIS.toDate = sevenMonths;
        } else {
            $scope.optionsOTIS.fromDate = minDate;
            $scope.optionsOTIS.toDate = maxDate;
        }

        // Here we are going to use a copy/clone of the real data because subsequent sets will cause problems as angular-gantt messes with the data
        if (data.length > 0) {
            data = JSON.parse(JSON.stringify(data));
        }

        $scope.ganttOTISData = data;

        $scope.api.scroll.to(0);
        var todaysDate = new Date();
        if (todaysDate >= $scope.optionsOTIS.fromDate && todaysDate <= $scope.optionsOTIS.toDate) {
            $window.setTimeout(function () {
                $scope.api.scroll.toDate(todaysDate);
            }, 100);
        }
    }


    /* Update whether to show/hide the PropMgr, CELead or Value based on user checkbox selections in the OTIS Gantt grid */
    $scope.annotationOTISUpdate = function () {
        $scope.optionsOTIS.taskContent = '<span class="annotation">';
        if ($scope.selectedOTISFilters.includePropMgr) { $scope.optionsOTIS.taskContent += '{{task.row.model.annotation1}}'; }
        if ($scope.selectedOTISFilters.includeCELead) { $scope.optionsOTIS.taskContent += '{{task.row.model.annotation2}}'; }
        if ($scope.selectedOTISFilters.includeValue) { $scope.optionsOTIS.taskContent += '{{task.row.model.annotation3}}'; }
        $scope.optionsOTIS.taskContent += '</span>';

    }

    /* Loads the PTM filters from the cookie */
    $scope.loadFiltersFromCookieOTIS = function () {
        var cookies = $cookies.getAll();
        $scope.selectedOTISFilters.classified = $scope.LoadJSON($scope.selectedOTISFilters.classified, cookies['classifiedOTIS'], 'ClassifiedFilter');
        $scope.selectedOTISFilters.competitive = $scope.LoadJSON($scope.selectedOTISFilters.competitive, cookies['competitiveOTIS'], 'CompetitiveFilter');
        $scope.selectedOTISFilters.category = $scope.LoadJSON($scope.selectedOTISFilters.category, cookies['categoryOTIS'], 'CategoryFilter');
        $scope.selectedOTISFilters.pa = $scope.LoadJSON($scope.selectedOTISFilters.pa, cookies['paOTIS'], 'PAFilter');
        $scope.selectedOTISFilters.captMgr = $scope.LoadJSON($scope.selectedOTISFilters.captMgr, cookies['captMgrOTIS'], 'CaptMgrFilter');
        $scope.selectedOTISFilters.propMgr = $scope.LoadJSON($scope.selectedOTISFilters.propMgr, cookies['propMgrOTIS'], 'PropMgrFilter');
        $scope.selectedOTISFilters.role = $scope.LoadJSON($scope.selectedOTISFilters.role, cookies['roleOTIS'], 'RoleFilter');
        $scope.selectedOTISFilters.bdLead = $scope.LoadJSON($scope.selectedOTISFilters.bdLead, cookies['bdLeadOTIS'], 'BDLeadFilter');
        $scope.selectedOTISFilters.ceLead = $scope.LoadJSON($scope.selectedOTISFilters.ceLead, cookies['ceLeadOTIS'], 'CELeadFilter');
        $scope.selectedOTISFilters.nameContains = $scope.LoadJSON($scope.selectedOTISFilters.nameContains, cookies['nameContainsOTIS']);
        $scope.selectedOTISFilters.includePropMgr = $scope.LoadJSON($scope.selectedOTISFilters.includePropMgr, cookies['includePropMgrOTIS']);
        $scope.selectedOTISFilters.includeCELead = $scope.LoadJSON($scope.selectedOTISFilters.includeCELead, cookies['includeCELeadOTIS']);
        $scope.selectedOTISFilters.includeValue = $scope.LoadJSON($scope.selectedOTISFilters.includeValue, cookies['includeValueOTIS']);
        $scope.selectedOTISFilters.international = $scope.LoadJSON($scope.selectedOTISFilters.international, cookies['internationalOTIS'], 'InternationalFilter');
        $scope.selectedOTISFilters.startDate = $scope.LoadJSON($scope.selectedOTISFilters.startDate, cookies['startDateOTIS']);
        $scope.selectedOTISFilters.endDate = $scope.LoadJSON($scope.selectedOTISFilters.endDate, cookies['endDateOTIS']);
        $scope.selectedOTISFilters.minValue = $scope.LoadJSON($scope.selectedOTISFilters.minValue, cookies['minValueOTIS']);
        $scope.selectedOTISFilters.maxValue = $scope.LoadJSON($scope.selectedOTISFilters.maxValue, cookies['maxValueOTIS']);
        $scope.selectedOTISFilters.contractType = $scope.LoadJSON($scope.selectedOTISFilters.contractType, cookies['contractTypeOTIS'], 'ContractTypeFilter');
        $scope.selectedOTISFilters.oppType = $scope.LoadJSON($scope.selectedOTISFilters.oppType, cookies['oppTypeOTIS'], 'OppTypeFilter');
        $scope.selectedOTISFilters.status = $scope.LoadJSON($scope.selectedOTISFilters.status, cookies['statusOTIS'], 'StatusFilter');
        $scope.selectedOTISFilters.lob = $scope.LoadJSON($scope.selectedOTISFilters.lob, cookies['lobOTIS'], 'LOBFilter');
        $scope.selectedOTISFilters.customer = $scope.LoadJSON($scope.selectedOTISFilters.customer, cookies['customerOTIS'], 'CustomerFilter');
        $scope.selectedOTISFilters.referenceNumber = $scope.LoadJSON($scope.selectedOTISFilters.referenceNumber, cookies['referenceNumberOTIS'], 'ReferenceNumberFilter');

        $scope.updateDates();

        // now show/hide the clear filter icons
        filterObjectRangeChange($scope.selectedOTISFilters.endDate, $scope.selectedOTISFilters.startDate, 'DateFilterClear');
        filterObjectRangeChange($scope.selectedOTISFilters.minValue, $scope.selectedOTISFilters.maxValue, 'ValueFilterClear');

        filterObjectChange($scope.selectedOTISFilters.nameContains, 'NameFilterClear');
        $scope.annotationOTISUpdate();
    }

    /* Load the JSON object out of the string*/
    $scope.LoadJSON = function (originalValue, jsonString, filterName) {

        if (jsonString === undefined || jsonString === null || jsonString === "null" || jsonString == "\"\"") {
            return originalValue;
        }
        var returnValue = JSON.parse(jsonString);
        if (typeof (originalValue) === "object") {
            // if returnValue is a json object, and not a array of json objects,
            // convert it to the array.
            if (typeof (returnValue) === "object" && !Array.isArray(returnValue)) {
                returnValue = returnValue.value !== "(blanks)" ? new Array(returnValue) : new Array();
            }

            filterObjectChange(returnValue.length > 0 ? returnValue[0] : -1, filterName + 'Clear');
        }

        return returnValue;
    }

    /* Loads the OTIS filters from the cookie */
    $scope.saveFiltersIntoCookieOTIS = function () {

        var now = new $window.Date(),
        // this will set the expiration to 6 months
        exp = new $window.Date(now.getFullYear(), now.getMonth() + 6, now.getDate());
        // if user has made filtering selection, write cookie, otherwise remove cookie (user may have cleared a filter)
        if ($scope.selectedOTISFilters.classified.length > 0)
            $cookies.put('classifiedOTIS', angular.toJson($scope.selectedOTISFilters.classified), { expires: exp });
        else
            $cookies.remove('classifiedOTIS');
        if ($scope.selectedOTISFilters.competitive.length > 0)
            $cookies.put('competitiveOTIS', angular.toJson($scope.selectedOTISFilters.competitive), { expires: exp });
        else
            $cookies.remove('competitiveOTIS');
        if ($scope.selectedOTISFilters.category.length > 0)
            $cookies.put('categoryOTIS', angular.toJson($scope.selectedOTISFilters.category), { expires: exp });
        else
            $cookies.remove('categoryOTIS');
        if ($scope.selectedOTISFilters.pa.length > 0)
            $cookies.put('paOTIS', angular.toJson($scope.selectedOTISFilters.pa), { expires: exp });
        else
            $cookies.remove('paOTIS');
        if ($scope.selectedOTISFilters.captMgr.length > 0)
            $cookies.put('captMgrOTIS', angular.toJson($scope.selectedOTISFilters.captMgr), { expires: exp });
        else
            $cookies.remove('captMgrOTIS');
        if ($scope.selectedOTISFilters.propMgr.length > 0)
            $cookies.put('propMgrOTIS', angular.toJson($scope.selectedOTISFilters.propMgr), { expires: exp });
        else
            $cookies.remove('propMgrOTIS');
        if ($scope.selectedOTISFilters.role.length > 0)
            $cookies.put('roleOTIS', angular.toJson($scope.selectedOTISFilters.role), { expires: exp });
        else
            $cookies.remove('roleOTIS');
        if ($scope.selectedOTISFilters.bdLead.length > 0)
            $cookies.put('bdLeadOTIS', angular.toJson($scope.selectedOTISFilters.bdLead), { expires: exp });
        else
            $cookies.remove('bdLeadOTIS');
        if ($scope.selectedOTISFilters.ceLead.length > 0)
            $cookies.put('ceLeadOTIS', angular.toJson($scope.selectedOTISFilters.ceLead), { expires: exp });
        else
            $cookies.remove('ceLeadOTIS');
        if ($scope.selectedOTISFilters.nameContains.length > 0)
            $cookies.put('nameContainsOTIS', angular.toJson($scope.selectedOTISFilters.nameContains), { expires: exp });
        else
            $cookies.remove('nameContainsOTIS');
        if ($scope.selectedOTISFilters.oppType.length > 0)
            $cookies.put('oppTypeOTIS', angular.toJson($scope.selectedOTISFilters.oppType), { expires: exp });
        else
            $cookies.remove('oppTypeOTIS');
        if ($scope.selectedOTISFilters.international.length > 0)
            $cookies.put('internationalOTIS', angular.toJson($scope.selectedOTISFilters.international), { expires: exp });
        else
            $cookies.remove('internationalOTIS');
        if ($scope.selectedOTISFilters.status.length > 0)
            $cookies.put('statusOTIS', angular.toJson($scope.selectedOTISFilters.status), { expires: exp });
        else
            $cookies.remove('statusOTIS');
        if ($scope.selectedOTISFilters.lob.length > 0)
            $cookies.put('lobOTIS', angular.toJson($scope.selectedOTISFilters.lob), { expires: exp });
        else
            $cookies.remove('lobOTIS');
        if ($scope.selectedOTISFilters.contractType.length > 0)
            $cookies.put('contractTypeOTIS', angular.toJson($scope.selectedOTISFilters.contractType), { expires: exp });
        else
            $cookies.remove('contractTypeOTIS');
        if ($scope.selectedOTISFilters.customer.length > 0)
            $cookies.put('customerOTIS', angular.toJson($scope.selectedOTISFilters.customer), { expires: exp });
        else
            $cookies.remove('customerOTIS');
        if ($scope.selectedOTISFilters.referenceNumber.length > 0)
            $cookies.put('referenceNumberOTIS', angular.toJson($scope.selectedOTISFilters.referenceNumber), { expires: exp });
        else
            $cookies.remove('referenceNumberOTIS');
        if ($scope.selectedOTISFilters.minValue != '')
            $cookies.put('minValueOTIS', angular.toJson($scope.selectedOTISFilters.minValue), { expires: exp });
        else
            $cookies.remove('minValueOTIS');
        if ($scope.selectedOTISFilters.maxValue != '')
            $cookies.put('maxValueOTIS', angular.toJson($scope.selectedOTISFilters.maxValue), { expires: exp });
        else
            $cookies.remove('maxValueOTIS');
        if ($scope.selectedOTISFilters.startDate != '')
            $cookies.put('startDateOTIS', angular.toJson($scope.selectedOTISFilters.startDate), { expires: exp });
        else
            $cookies.remove('startDateOTIS');
        if ($scope.selectedOTISFilters.endDate != '')
            $cookies.put('endDateOTIS', angular.toJson($scope.selectedOTISFilters.endDate), { expires: exp });
        else
            $cookies.remove('endDateOTIS');
        $cookies.put('includePropMgrOTIS', angular.toJson($scope.selectedOTISFilters.includePropMgr), { expires: exp });
        $cookies.put('includeCELeadOTIS', angular.toJson($scope.selectedOTISFilters.includeCELead), { expires: exp });
        $cookies.put('includeValueOTIS', angular.toJson($scope.selectedOTISFilters.includeValue), { expires: exp });
    }

    /* reset all OTIS filters to be empty and unselected */
    $scope.clearAllOTISFilters = function () {
        $scope.selectedOTISFilters.contractType = [];
        $scope.selectedOTISFilters.oppType = [];
        $scope.selectedOTISFilters.status = [];
        $scope.selectedOTISFilters.lob = [];
        $scope.selectedOTISFilters.customer = [];
        $scope.selectedOTISFilters.referenceNumber = [];
        $scope.selectedOTISFilters.classified = [];
        $scope.selectedOTISFilters.competitive = [];
        $scope.selectedOTISFilters.category = [];
        $scope.selectedOTISFilters.pa = [];
        $scope.selectedOTISFilters.captMgr = [];
        $scope.selectedOTISFilters.propMgr = [];
        $scope.selectedOTISFilters.role = [];
        $scope.selectedOTISFilters.bdLead = [];
        $scope.selectedOTISFilters.ceLead = [];
        $scope.selectedOTISFilters.international = [];
        $scope.selectedOTISFilters.nameContains = '';
        $scope.selectedOTISFilters.startDate = '';
        $scope.selectedOTISFilters.endDate = '';
        $scope.selectedOTISFilters.startDateAsDate = undefined;
        $scope.selectedOTISFilters.endDateAsDate = undefined;
        $scope.selectedOTISFilters.minValue = '';
        $scope.selectedOTISFilters.maxValue = '';

        // refresh filter clear glyphs since we're clearing filters
        // this was added so glyphs to clear filter will only show when filter is active
        angular.element(document.querySelector('#ContractTypeFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#OppTypeFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#StatusFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#LOBFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CustomerFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ReferenceNumberFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ClassifiedFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CompetitiveFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CategoryFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PAFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CaptMgrFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PropMgrFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#RoleFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#BDLeadFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CELeadFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#NameFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#DurationFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#DateFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ValueFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#InternationalFilterClear')).attr("Hidden", "Hidden");

        if (!$scope.isOTISLoading) {
            $scope.refreshOTISData(true);
        }
    };

    /* Update the real Date objects from the strings */
    $scope.updateDates = function () {
        if ($scope.selectedOTISFilters.startDate !== undefined && $scope.selectedOTISFilters.startDate != '') {
            $scope.selectedOTISFilters.startDateAsDate = new Date($scope.selectedOTISFilters.startDate);
            $scope.selectedOTISFilters.startDateAsDate.setDate($scope.selectedOTISFilters.startDateAsDate.getDate() - 1);
        } else {
            $scope.selectedOTISFilters.startDateAsDate = undefined;
        }

        if ($scope.selectedOTISFilters.endDate !== undefined && $scope.selectedOTISFilters.endDate != '') {
            $scope.selectedOTISFilters.endDateAsDate = new Date($scope.selectedOTISFilters.endDate);
            $scope.selectedOTISFilters.endDateAsDate.setDate($scope.selectedOTISFilters.endDateAsDate.getDate() + 1);
        } else {
            $scope.selectedOTISFilters.endDateAsDate = undefined;
        }
    }

    /* Called when there has been an update to the date range for OTIS Filters */
    $scope.daterangeOTISUpdate = function () {
        $scope.updateDates();
        $scope.refreshOTISData(true);
    }

    /* helper method used by filterPTMProposals; checks whether item matches filter or not; returns: false if row dos not match, true if it does; */
    $scope.checkMultiSelectFilter = function (filterValue, propertyValue) {
        if (filterValue != undefined && filterValue.length > 0) {
            return filterValue.some(function (selectedItem) { // return true if match, false otherwise
                if (selectedItem.value == null && selectedItem.value == propertyValue) return true; // if 'No Value' selected
                if (selectedItem.display == propertyValue) return true; // if Prime or Sub is selected
                return false;
            });
        }
    }

    /* helper method used by filterPTMProposals; checks whether item matches filter or not; returns: false if row dos not match, true if it does; */
    // contractType and Classified contain multiple values sep. by commas, drop down filter has split values so match on substring
    $scope.checkMultiSelectFilter2 = function (filterValue, propertyValue) {
        if (filterValue != undefined && filterValue.length > 0) {
            return filterValue.some(function (selectedItem) { // return true if match, false otherwise
                if (propertyValue != null && propertyValue.toLowerCase().indexOf(selectedItem.value.toLowerCase()) > -1) return true;
                return false;
            });
        }
    };

    /* Method used to filter the OTIS Opportunities */
    $scope.filterOTISOpportunities = function (value) {
        if ($scope.checkMultiSelectFilter2($scope.selectedOTISFilters.contractType, value.ContractType) == false) return false;
        if ($scope.checkMultiSelectFilter2($scope.selectedOTISFilters.classified, value.Classified) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.lob, value.LOB) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.oppType, value.OppType) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.status, value.Status) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.customer, value.Customer) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.referenceNumber, value.OTISNumber) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.competitive, value.Competitive) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.category, value.Category) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.pa, value.PA) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.captMgr, value.CaptMgr) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.propMgr, value.PM) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.role, value.Role) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.bdLead, value.BDLead) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.ceLead, value.CELead) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedOTISFilters.international, value.International) == false) return false;
        if ($scope.selectedOTISFilters.nameContains !== undefined && $scope.selectedOTISFilters.nameContains != '' && value.Title.toLowerCase().indexOf($scope.selectedOTISFilters.nameContains.toLowerCase()) == -1) { return false; }
        var RFPDate = new Date(value.RFPDateString)
        if ($scope.selectedOTISFilters.startDateAsDate !== undefined && $scope.selectedOTISFilters.endDateAsDate !== undefined) {
            if ($scope.selectedOTISFilters.endDateAsDate < $scope.selectedOTISFilters.startDateAsDate)
                return false;
            // start and end filters are valid dates, and they do not intersect this row's from/to dates
            if ($scope.selectedOTISFilters.startDateAsDate <= $scope.selectedOTISFilters.endDateAsDate && (RFPDate > $scope.selectedOTISFilters.endDateAsDate || RFPDate < $scope.selectedOTISFilters.startDateAsDate)) {
                return false;
            }
        }
        if ($scope.selectedOTISFilters.startDateAsDate != undefined)
            if (RFPDate <= $scope.selectedOTISFilters.startDateAsDate)
                return false;
        if ($scope.selectedOTISFilters.endDateAsDate != undefined)
            if (RFPDate >= $scope.selectedOTISFilters.endDateAsDate)
                return false;
        // filter Min Value
        if ($scope.selectedOTISFilters.minValue != '' && !isNaN($scope.selectedOTISFilters.minValue)) {
            if (!value.EVal > 0 || $scope.selectedOTISFilters.minValue > value.EVal) {
                return false;
            }
        }
        // filter Max Value
        if ($scope.selectedOTISFilters.maxValue != '' && !isNaN($scope.selectedOTISFilters.maxValue)) {
            if (!value.EVal > 0 || $scope.selectedOTISFilters.maxValue < value.EVal) {
                return false;
            }
        }

        // none of the filters threw out the row, so it must match
        return true;
    };

    /* Refresh the OTIS Opportunities from the Backend Server */
    $scope.refreshOTISOpportunities = function () {
        loadOTISData();
    }

    /* Refresh the OTIS gantt chart when the user clicks the paging controls */
    $scope.$watch(function (scope) { return scope.currentOTISPage },
        function (newValue, oldValue) {
            if (newValue !== oldValue) {
                $scope.refreshOTISData(false);
            }
        }
    );

    /* Find the number of pages based on the total number of Rows and pageSize */
    $scope.numberOfPages = function (numFilteredRows, pageSize) {
        return Math.ceil(numFilteredRows / pageSize);
    }

    /*
     * *********** NOTE ************
     * Private variables & functions 
     * *****************************
     */
    var blank = "(blanks)";
    var oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds

    /* Converts a hash table to an array */
    var convertToArray = function (data, sortByDisplayName) {
        var newData = [];

        for (var key in data) {
            var item = data[key];

            // set the value to lowercase for strings
            if (angular.isDefined(item.value) && item.value !== null && angular.isString(item.value)) {
                item.value = item.value.toLowerCase();
            }

            newData.push(item);
        }

        if (sortByDisplayName) {
            newData.sort(function (a, b) { return a.display.localeCompare(b.display) });
        }

        return newData;
    };

    /* Loads the OTIS Data */
    var loadOTISData = function () {
        $scope.isOTISLoading = true;
        $scope.isOTISError = false;
        $scope.hideOTISDetails = true;
        $scope.ganttOTISData = [];
        $scope.filteredOTISCount = 0;
        $scope.filteredOTISValue = 0.0;
        $scope.allOTISData = [];

        return $http({
            method: 'POST',
            url: createPostURL('Home', 'RetrieveOpportunityData')
        }).then(function successCallback(response) {
            if (response.data.success == false) { // checks for error return from controller catch
                $scope.isOTISLoading = false;
                $scope.isOTISError = true;
                return;
            }
            // this callback will be called asynchronously
            // when the response is available
            $scope.filterOTIS.Title = {};
            $scope.filterOTIS.contractType = {};
            $scope.filterOTIS.oppType = {};
            $scope.filterOTIS.status = {};
            $scope.filterOTIS.lob = {};
            $scope.filterOTIS.customer = {};
            $scope.filterOTIS.referenceNumber = {};
            $scope.filterOTIS.classified = {};
            $scope.filterOTIS.competitive = {};
            $scope.filterOTIS.category = {};
            $scope.filterOTIS.pa = {};
            $scope.filterOTIS.captMgr = {};
            $scope.filterOTIS.propMgr = {};
            $scope.filterOTIS.role = {};
            $scope.filterOTIS.bdLead = {};
            $scope.filterOTIS.ceLead = {};
            $scope.filterOTIS.international = {};

            // Setup other dropdowns based on existing values in data
            response.data.Opportunities.forEach(function (item) {
                // build filter sets based on the data returned

                // for contractType, need to split array using ',' -- add individually
                if (item.ContractType != null && item.ContractType.length > 0) {
                    var arrayOfContractTypes = item.ContractType.split(',');
                    for (i = 0; i < arrayOfContractTypes.length; i++) {
                        addOption($scope.filterOTIS.contractType, arrayOfContractTypes[i]);
                    }
                }
                // for classified, need to split array using ',' -- add individually
                if (item.Classified != null && item.Classified.length > 0) {
                    var arrayOfClassifieds = item.Classified.split(',');
                    for (i = 0; i < arrayOfClassifieds.length; i++) {
                        addOption($scope.filterOTIS.classified, arrayOfClassifieds[i]);
                    }
                }


                addOption($scope.filterOTIS.oppType, item.OppType);
                addOption($scope.filterOTIS.status, item.Status);
                addOption($scope.filterOTIS.lob, item.LOB);
                addOption($scope.filterOTIS.customer, item.Customer);
                addOption($scope.filterOTIS.referenceNumber, item.OTISNumber);
                addOption($scope.filterOTIS.competitive, item.Competitive);
                addOption($scope.filterOTIS.category, item.Category);
                addOption($scope.filterOTIS.captMgr, item.CaptMgr);
                addOption($scope.filterOTIS.propMgr, item.PM);
                addOption($scope.filterOTIS.ceLead, item.CELead);
                addOption($scope.filterOTIS.bdLead, item.BDLead);
                addOption($scope.filterOTIS.role, item.Role);
                addOption($scope.filterOTIS.pa, item.PA);
                addOption($scope.filterOTIS.international, item.International);



                // build annotation content

                item.annotation1 = item.PM == null || item.PM == '' ? '' : ' PM: ' + item.PM + ' ';
                item.annotation2 = item.CELead == null || item.CELead == '' ? '' : ' CE Lead: ' + item.CELead + ' ';
                item.annotation3 = item.EVal == 0 || item.EVal == null || item.EVal == '' ? '' : ' Val: ' + item.EVal + ' ';

                item.StartDate = item.RFPDateString;
                item.EndDate = item.RFPEndDateString;

                // setup tasks in gantt chart
                if (item.StartDate != null && item.EndDate != null && item.StartDate !== undefined && item.EndDate !== undefined) {
                    // pull out the year for later filtering, date in format yyyy-mm-dd
                    item.tasks = [{ name: '', color: "#9FC5F8", from: item.StartDate, to: item.EndDate }];
                }

                if (item.StartDate != null) {
                    item.StartDate = new Date(item.StartDate);
                }

                if (item.EndDate != null) {
                    item.EndDate = new Date(item.EndDate);
                }

                // set Duration
                if (item.SubDate !== undefined && item.CrDate !== undefined) {
                    var submittal = new Date(item.SubDate);
                    var created = new Date(item.CrDate);
                    item.Duration = Math.ceil(Math.abs((submittal.getTime() - created.getTime()) / (oneDay)));
                }
            });


            // need to convert to arrays and sort the data
            $scope.filterOTIS.status = convertToArray($scope.filterOTIS.status, true);
            $scope.filterOTIS.contractType = convertToArray($scope.filterOTIS.contractType, true);
            $scope.filterOTIS.oppType = convertToArray($scope.filterOTIS.oppType, true);
            $scope.filterOTIS.lob = convertToArray($scope.filterOTIS.lob, true);
            $scope.filterOTIS.customer = convertToArray($scope.filterOTIS.customer, true);
            $scope.filterOTIS.referenceNumber = convertToArray($scope.filterOTIS.referenceNumber, true);
            $scope.filterOTIS.classified = convertToArray($scope.filterOTIS.classified, true);
            $scope.filterOTIS.competitive = convertToArray($scope.filterOTIS.competitive, true);
            $scope.filterOTIS.category = convertToArray($scope.filterOTIS.category, true);
            $scope.filterOTIS.pa = convertToArray($scope.filterOTIS.pa, true);
            $scope.filterOTIS.captMgr = convertToArray($scope.filterOTIS.captMgr, true);
            $scope.filterOTIS.propMgr = convertToArray($scope.filterOTIS.propMgr, true);
            $scope.filterOTIS.role = convertToArray($scope.filterOTIS.role, true);
            $scope.filterOTIS.bdLead = convertToArray($scope.filterOTIS.bdLead, true);
            $scope.filterOTIS.ceLead = convertToArray($scope.filterOTIS.ceLead, true);
            $scope.filterOTIS.international = convertToArray($scope.filterOTIS.international, false);


            // Add blank value here because sorting puts blank value at end for integers
            $scope.filterOTIS.status.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.oppType.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.contractType.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.lob.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.customer.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.referenceNumber.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.classified.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.competitive.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.category.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.pa.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.captMgr.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.propMgr.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.role.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.bdLead.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterOTIS.ceLead.splice(0, 0, { display: 'No Value', value: null });
            //     $scope.filterOTIS.international.splice(0, 0, { display: 'No Value', value: null });


            if ($scope.firstLoad) {
                $scope.firstLoad = false;
                $scope.clearAllOTISFilters();
                $scope.loadFiltersFromCookieOTIS();
            }

            $scope.allOTISData = response.data.Opportunities;
            $scope.filterOTISGanttRows();
            $scope.isOTISLoading = false;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            $scope.isOTISLoading = false;
            $scope.isOTISError = true;
        });
    }

}]);