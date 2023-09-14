angular.module("RDM").factory("utilityService", ['$http', function ($http) {
    var utilityService = {

        /* Gets a Date/Time from a JSON date. */
        getDateStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                return new Date(parseInt(jsonDate.replace(/\/Date\((.*?)\)\//gi, "$1")));
            } else {
                return "";
            }
        },

        /* Gets a date string in MM/dd/yyyy format from a JSON date. */
        getMonthDateYearDateStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                var date = new Date(parseInt(jsonDate.substr(6)));
                return (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
            }
            return "";
        },

        /* Gets a date string in MM/dd/yyyy HH:mm:ss AM format from a JSON date. */
        getLocaleDateTimeStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                var date = new Date(parseInt(jsonDate.substr(6)));
                return (date.toLocaleDateString() + " " + date.toLocaleTimeString());
            }
            return "";
        },

        /* Initialize TinyMCE to a basic set of options */
        getTinyMceOptions: function () {
            return {
                width: '100%',
                statusbar: false,
                menubar: false,
                resize: false,
                height: 450,
                skin: "lightgray",
                plugins: "paste fullscreen table image code textcolor",
                browser_spellcheck: true,
                paste_data_images: true,
                convert_urls: false,
                paste_block_drop: false,
                paste_retain_style_properties: "all",
                paste_word_valid_elements: "@[style],-strong/b,-em/i,-span,-p,-ol,-ul,-li,-h1,-h2,-h3," +
                    "-table,-tr,-td[colspan|rowspan],-th,-thead,-tfoot,-tbody,-a[href|name],-font[color],sub,sup,strike,br,u",
                style_formats: [
                    {
                        title: "Inline", items: [
                            { title: "Bold", icon: "bold", format: "bold" },
                            { title: "Italic", icon: "italic", format: "italic" },
                            { title: "Underline", icon: "underline", format: "underline" },
                            { title: "Strikethrough", icon: "strikethrough", format: "strikethrough" },
                            { title: "Superscript", icon: "superscript", format: "superscript" },
                            { title: "Subscript", icon: "subscript", format: "subscript" }
                        ]
                    },
                    {
                        title: "Alignment", items: [
                            { title: "Left", icon: "alignleft", format: "alignleft" },
                            { title: "Center", icon: "aligncenter", format: "aligncenter" },
                            { title: "Right", icon: "alignright", format: "alignright" },
                            { title: "Justify", icon: "alignjustify", format: "alignjustify" }
                        ]
                    }
                ],
                toolbar: "table | formatselect | bold italic underline forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | undo redo | removeformat | paste fullscreen | code | addPageBreak removePageBreaks",
                // This does the stripping of rich text during a paste
                paste_preprocess: function (plugin, args) {
                    var removePageBreaks = args.content.split('<br style="page-break-before: always;">').join('');
                    var encodedInitial = encodeURI(removePageBreaks);
                    var encodedFinal = encodedInitial.replace(/\+/g, '%2B'); // replace plus-sign (+) with its URL-encoding (2B)
                    var richTextEncoded = '{ "html" : "' + encodedFinal + '" }'; // must be URL-encoded for JSON

                    // call the server to do the scrub
                    $.ajax({
                        type: 'POST',
                        async: false, // call MUST be synchronous so args.content will be set before the function exits
                        url: '/IES/PreProcessRichTextPaste',
                        contentType: 'application/json; charset=utf-8',
                        data: richTextEncoded,
                        dataType: 'html', // response is HTML
                        success: function (response) {
                            args.content = response;
                        },
                        error: function (response) {
                        },
                        complete: function (response) {
                        }
                    });
                },
                //Once it's initialized, and there's a refresh module/page function, then call it
                init_instance_callback: function () {
                    // BOEJ-2849 - clear the title (tooltip) attribute for the tinymce editor iframe.
                    var mceContainer = $(this.getContainer());
                    mceContainer.find('iframe').attr('title', '');
                },
                setup: function (editor) {

                    function removePageBreaks() {
                        var html = editor.getContent();
                        var find = '<p style="page-break-before: always;">&nbsp;</p>';
                        var re = new RegExp(find, 'g');
                        html = html.replace(re, '');
                        editor.setContent(html);
                    }

                    function insertPageBreak() {
                        var html = '<p style="page-break-before: always;"></p>';
                        editor.insertContent(html);
                    }

                    editor.addButton('addPageBreak', {
                        tooltip: "Insert a Page Break",
                        icon: 'my-pagebreak',
                        onclick: insertPageBreak
                    });

                    editor.addButton('removePageBreaks', {
                        tooltip: "Remove all Page Breaks",
                        icon: 'undo-pagebreak',
                        onclick: removePageBreaks
                    });
                }
            };
        },

        /* 
            Makes a uibModal dialog draggable and resizable 
            Parameters:
                selector - JQuery selector used to find the dialog element in the DOM.
                           Note: uibModal wraps this element with an outer <div class="modal"> element. 
                minHeight - Minimum height when resizing the dialog.
                minWidth - Minimum width when resizing the dialog.
        */
        makeModalDraggableAndResizable: function (selector, minHeight, minWidth) {
            var $modal = $(selector).closest('.modal');
            $modal.find('.modal-content').resizable({
                minHeight: minHeight,
                minWidth: minWidth
            });
            $modal.find('.modal-dialog').draggable();
        },

        /* 
            Helper method for setting Rate Grid options when exporting rate data.  
            Used by HomeController.js, and ReportsController.js. 
        */
        setRateGridOptions: function (scope, onRowsRenderedFunction) {
            scope.rateGridOptions = {
                exporterCsvLinkElement: angular.element(document.querySelectorAll(".custom-csv-link-location")),
                columnDefs: [
                    { name: 'Category', field: 'RCD' },
                    { name: 'Description', field: 'De' },
                    { name: 'Rate Code', field: 'Co' }
                ],
            };

            /* Get a reference to the gridApi so can define the rowsRendered event handler */
            scope.rateGridOptions.onRegisterApi = function (gridApi) {
                scope.rateGridApi = gridApi;
                scope.rateGridApi.core.on.rowsRendered(scope, onRowsRenderedFunction);
            };
        },

        /* 
            Helper method for updating Rate Grid year/rate value column definitions when exporting rate data. 
            Also sets the exporter CSV filename.
            Used by HomeController.js, and ReportsController.js.
        */
        updateRateGridData: function (rateGridOptions, newRates, versionNumber) {
            // If you set this before onRegisterApi - you can't modify the export file name if the user changes versions.
            var dateString = new Date().toLocaleDateString();
            dateString = dateString.replace(/\//g, '-'); // replace all slashes with dashes
            rateGridOptions.exporterCsvFilename = 'Rates_RDM_' + versionNumber + '_' + dateString + '.csv';

            // setup column definitions for the Years
            var minYear = 0;
            var maxYear = 0;
            if (newRates[0] !== null) {
                minYear = newRates[0].Values[0].Yr;
                maxYear = newRates[0].Values[newRates[0].Values.length - 1].Yr;
            }

            var index = 0;
            for (var year = minYear; year <= maxYear; year++) {
                rateGridOptions.columnDefs.push(
                    {
                        name: year,
                        field: 'Values[' + index + '].Val',
                        enableSorting: false,
                        enableFiltering: false,
                    });
                index++;
            }
        },

        /*
            Helper method used to zero out the unneeded Rate information
        */
        zeroOutExpandedRate: function (rate) {
            // Rate Descriptions
            rate.RD1 = undefined; 
            rate.RD2 = undefined;
            rate.RD3 = undefined;
            rate.RD4 = undefined;
            rate.RD5 = undefined;
            rate.RD6 = undefined;
            rate.RD7 = undefined;
            rate.RD8 = undefined;
            rate.RD9 = undefined;
            // Resource Classes
            rate.RCId1 = undefined;
            rate.RCId2 = undefined;
            rate.RCId3 = undefined;
            rate.RCId4 = undefined;
            rate.RCId5 = undefined;
            rate.RCId6 = undefined;
            rate.RCId7 = undefined;
            rate.RCId8 = undefined;
            rate.RCId9 = undefined;
            rate.R = undefined; // Revision Id
            rate.S = undefined; // Section
            rate.CS = undefined; // Compare State
            rate.RT = undefined; // DirectRateMappingResourceType
            rate.Ra = undefined; // RateType
            rate.Gen = undefined; // GenerateAdditionalDirectLaborRates
            rate.GBPId = undefined; // GovernmentBurdenPoolId
            rate.CBPId = undefined; // CommercialBurdenPoolId
            rate.HM = undefined; // HasProPricerBurdenRateMappings
        },

        /*
            Helper method used to expand the rates for the read-only UI or when exporting.
            Used by RateController.js, and ReportsController.js
        */
        expandRateData: function (rates) {
            var expandedRates = [];
        
            rates.forEach(function (rate) {
                var expanded = false;

                if (rate.RD1 !== undefined && rate.RD1 != null && rate.RD1 != '') {
                    var rate1 = angular.copy(rate);
                    rate1.hidePopup = true;
                    rate1.Co = rate1.Co + '1';
                    rate1.De = rate1.RD1;
                    rate1.RCId = rate1.RCId1;
                    utilityService.zeroOutExpandedRate(rate1);
                    expandedRates.push(rate1);
                    expanded = true;
                }

                if (rate.RD2 !== undefined && rate.RD2 != null && rate.RD2 != '') {
                    var rate2 = angular.copy(rate);
                    rate2.hidePopup = true;
                    rate2.Co = rate2.Co + '2';
                    rate2.De = rate2.RD2;
                    rate2.RCId = rate2.RCId2;
                    utilityService.zeroOutExpandedRate(rate2);
                    expandedRates.push(rate2);
                    expanded = true;
                }

                if (rate.RD3 !== undefined && rate.RD3 != null && rate.RD3 != '') {
                    var rate3 = angular.copy(rate);
                    rate3.hidePopup = true;
                    rate3.Co = rate3.Co + '3';
                    rate3.De = rate3.RD3;
                    rate3.RCId = rate3.RCId3;
                    utilityService.zeroOutExpandedRate(rate3);
                    expandedRates.push(rate3);
                    expanded = true;
                }

                if (rate.RD4 !== undefined && rate.RD4 != null && rate.RD4 != '') {
                    var rate4 = angular.copy(rate);
                    rate4.hidePopup = true;
                    rate4.Co = rate4.Co + '4';
                    rate4.De = rate4.RD4;
                    rate4.RCId = rate4.RCId4;
                    utilityService.zeroOutExpandedRate(rate4);
                    expandedRates.push(rate4);
                    expanded = true;
                }

                if (rate.RD5 !== undefined && rate.RD5 != null && rate.RD5 != '') {
                    var rate5 = angular.copy(rate);
                    rate5.hidePopup = true;
                    rate5.Co = rate5.Co + '5';
                    rate5.De = rate5.RD5;
                    rate5.RCId = rate5.RCId5;
                    utilityService.zeroOutExpandedRate(rate5);
                    expandedRates.push(rate5);
                    expanded = true;
                }

                if (rate.RD6 !== undefined && rate.RD6 != null && rate.RD6 != '') {
                    var rate6 = angular.copy(rate);
                    rate6.hidePopup = true;
                    rate6.Co = rate6.Co + '6';
                    rate6.De = rate6.RD6;
                    rate6.RCId = rate6.RCId6;
                    utilityService.zeroOutExpandedRate(rate6);
                    expandedRates.push(rate6);
                    expanded = true;
                }

                if (rate.RD7 !== undefined && rate.RD7 != null && rate.RD7 != '') {
                    var rate7 = angular.copy(rate);
                    rate7.hidePopup = true;
                    rate7.Co = rate7.Co + '7';
                    rate7.De = rate7.RD7;
                    rate7.RCId = rate7.RCId7;
                    utilityService.zeroOutExpandedRate(rate7);
                    expandedRates.push(rate7);
                    expanded = true;
                }

                if (rate.RD8 !== undefined && rate.RD8 != null && rate.RD8 != '') {
                    var rate8 = angular.copy(rate);
                    rate8.hidePopup = true;
                    rate8.Co = rate8.Co + '8';
                    rate8.De = rate8.RD8;
                    rate8.RCId = rate8.RCId8;
                    utilityService.zeroOutExpandedRate(rate8);
                    expandedRates.push(rate8);
                    expanded = true;
                }

                if (rate.RD9 !== undefined && rate.RD9 != null && rate.RD9 != '') {
                    var rate9 = angular.copy(rate);
                    rate9.hidePopup = true;
                    rate9.Co = rate9.Co + '9';
                    rate9.De = rate9.RD9;
                    rate9.RCId = rate9.RCId9;
                    utilityService.zeroOutExpandedRate(rate9);
                    expandedRates.push(rate9);
                    expanded = true;
                }

                if (!expanded) {
                    expandedRates.push(rate);
                }
            });

            return expandedRates;
        },

        /*
            Validates the rates and shows either the publish dialog (if there are no warnings) or the confirmation dialog (if there are warnings).
            A null value for selectedRevision will check the current WIP.
        */
        validateRates: function (validateRatesController, validateRatesAction, selectedRevision, continueCallback) {
            $(document).trigger("SHOW_LOADING_BOX");

            // Call the validate rates controller action.
            var validateRatesUrl = createPostURL(validateRatesController, validateRatesAction);

            return $http({
                method: 'POST',
                url: validateRatesUrl,
                data: JSON.stringify({ id: selectedRevision })
            }).then(function successCallback(response) {
                $(document).trigger("HIDE_LOADING_BOX");
                if (response.data.Valid) {
                    continueCallback();
                } else {
                    utilityService.confirmPublish(response.data.InvalidRates, continueCallback);
                }
            }, function errorCallback(response) {
                $(document).trigger("DISPLAY_NOTIFICATION", 'Error validating rates.');
                $(document).trigger("HIDE_LOADING_BOX");
            });
        },

        /*
            Shows the confirmation dialog for continuing with a publish if there are warning messages.
            Parameters are:  (a function to execute if [Continue] is selected, a function to execute if [Abort] is selected).
        */
        confirmPublish: function (rates, continueCallback, abortCallback) {
            var ratesList = "<ul>";
            angular.forEach(rates,
                function(rate) {
                    ratesList += "<li>" + rate + "</li>";
                });
            ratesList += "</ul>";

            $('#CommonDialogBody').css('text-align', 'left');   // list items look better aligned left
            ConfirmDialog("Warning - Rates With Zero Yearly Values", "All displayed yearly values for the following rates are zero.<br/>" + ratesList + "Do you wish to continue?", function () {
                // Yes
                $('#CommonDialogBody').css('text-align', 'center'); // reset back to center
                if (continueCallback) {
                    continueCallback();
                }
            },
            function () {
                // No
                $('#CommonDialogBody').css('text-align', 'center'); // reset back to center
                if (abortCallback) {
                    abortCallback();
                }
            });
        }
    };

    return utilityService;

}]);
