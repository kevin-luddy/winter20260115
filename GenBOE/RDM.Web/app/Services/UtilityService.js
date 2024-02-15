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

			// 1LMX Rate Descriptions
			rate.RD11 = undefined;
			rate.RD12 = undefined;
			rate.RD13 = undefined;
			rate.RD14 = undefined;
			rate.RD15 = undefined;
			rate.RD21 = undefined;
			rate.RD22 = undefined;
			rate.RD23 = undefined;
			rate.RD24 = undefined;
			rate.RD25 = undefined;
			rate.RD31 = undefined;
			rate.RD32 = undefined;
			rate.RD33 = undefined;
			rate.RD34 = undefined;
			rate.RD35 = undefined;
			rate.RD41 = undefined;
			rate.RD42 = undefined;
			rate.RD43 = undefined;
			rate.RD44 = undefined;
			rate.RD45 = undefined;
			rate.RD51 = undefined;
			rate.RD52 = undefined;
			rate.RD53 = undefined;
			rate.RD54 = undefined;
			rate.RD55 = undefined;
			rate.RD61 = undefined;
			rate.RD62 = undefined;
			rate.RD63 = undefined;
			rate.RD64 = undefined;
			rate.RD65 = undefined;
			rate.RD71 = undefined;
			rate.RD72 = undefined;
			rate.RD73 = undefined;
			rate.RD74 = undefined;
			rate.RD75 = undefined;
			rate.RD81 = undefined;
			rate.RD82 = undefined;
			rate.RD83 = undefined;
			rate.RD84 = undefined;
			rate.RD85 = undefined;
			rate.RD91 = undefined;
			rate.RD92 = undefined;
			rate.RD93 = undefined;
			rate.RD94 = undefined;
			rate.RD95 = undefined;

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

			// 1LMX Resource Classes
			rate.RCId11 = undefined;
			rate.RCId12 = undefined;
			rate.RCId13 = undefined;
			rate.RCId14 = undefined;
			rate.RCId15 = undefined;
			rate.RCId21 = undefined;
			rate.RCId22 = undefined;
			rate.RCId23 = undefined;
			rate.RCId24 = undefined;
			rate.RCId25 = undefined;
			rate.RCId31 = undefined;
			rate.RCId32 = undefined;
			rate.RCId33 = undefined;
			rate.RCId34 = undefined;
			rate.RCId35 = undefined;
			rate.RCId41 = undefined;
			rate.RCId42 = undefined;
			rate.RCId43 = undefined;
			rate.RCId44 = undefined;
			rate.RCId45 = undefined;
			rate.RCId51 = undefined;
			rate.RCId52 = undefined;
			rate.RCId53 = undefined;
			rate.RCId54 = undefined;
			rate.RCId55 = undefined;
			rate.RCId61 = undefined;
			rate.RCId62 = undefined;
			rate.RCId63 = undefined;
			rate.RCId64 = undefined;
			rate.RCId65 = undefined;
			rate.RCId71 = undefined;
			rate.RCId72 = undefined;
			rate.RCId73 = undefined;
			rate.RCId74 = undefined;
			rate.RCId75 = undefined;
			rate.RCId81 = undefined;
			rate.RCId82 = undefined;
			rate.RCId83 = undefined;
			rate.RCId84 = undefined;
			rate.RCId85 = undefined;
			rate.RCId91 = undefined;
			rate.RCId92 = undefined;
			rate.RCId93 = undefined;
			rate.RCId94 = undefined;
			rate.RCId95 = undefined;

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

				// 1LMX Rates
				if (rate.RD11 !== undefined && rate.RD11 != null && rate.RD11 != '') {
					var rate11 = angular.copy(rate);
					rate11.hidePopup = true;
					rate11.Co = rate11.Co + '11';
					rate11.De = rate11.RD11;
					rate11.RCId = rate11.RCId11;
					utilityService.zeroOutExpandedRate(rate11);
					expandedRates.push(rate11);
					expanded = true;
				}

				if (rate.RD12 !== undefined && rate.RD12 != null && rate.RD12 != '') {
					var rate12 = angular.copy(rate);
					rate12.hidePopup = true;
					rate12.Co = rate12.Co + '12';
					rate12.De = rate12.RD12;
					rate12.RCId = rate12.RCId12;
					utilityService.zeroOutExpandedRate(rate12);
					expandedRates.push(rate12);
					expanded = true;
				}

				if (rate.RD13 !== undefined && rate.RD13 != null && rate.RD13 != '') {
					var rate13 = angular.copy(rate);
					rate13.hidePopup = true;
					rate13.Co = rate13.Co + '13';
					rate13.De = rate13.RD13;
					rate13.RCId = rate13.RCId13;
					utilityService.zeroOutExpandedRate(rate13);
					expandedRates.push(rate13);
					expanded = true;
				}

				if (rate.RD14 !== undefined && rate.RD14 != null && rate.RD14 != '') {
					var rate14 = angular.copy(rate);
					rate14.hidePopup = true;
					rate14.Co = rate14.Co + '14';
					rate14.De = rate14.RD14;
					rate14.RCId = rate14.RCId14;
					utilityService.zeroOutExpandedRate(rate14);
					expandedRates.push(rate14);
					expanded = true;
				}

				if (rate.RD15 !== undefined && rate.RD15 != null && rate.RD15 != '') {
					var rate15 = angular.copy(rate);
					rate15.hidePopup = true;
					rate15.Co = rate15.Co + '15';
					rate15.De = rate15.RD15;
					rate15.RCId = rate15.RCId15;
					utilityService.zeroOutExpandedRate(rate15);
					expandedRates.push(rate15);
					expanded = true;
				}

				if (rate.RD21 !== undefined && rate.RD21 != null && rate.RD21 != '') {
					var rate21 = angular.copy(rate);
					rate21.hidePopup = true;
					rate21.Co = rate21.Co + '21';
					rate21.De = rate21.RD21;
					rate21.RCId = rate11.RCId21;
					utilityService.zeroOutExpandedRate(rate21);
					expandedRates.push(rate21);
					expanded = true;
				}

				if (rate.RD22 !== undefined && rate.RD22 != null && rate.RD22 != '') {
					var rate22 = angular.copy(rate);
					rate22.hidePopup = true;
					rate22.Co = rate22.Co + '22';
					rate22.De = rate22.RD22;
					rate22.RCId = rate22.RCId22;
					utilityService.zeroOutExpandedRate(rate22);
					expandedRates.push(rate22);
					expanded = true;
				}

				if (rate.RD23 !== undefined && rate.RD23 != null && rate.RD23 != '') {
					var rate23 = angular.copy(rate);
					rate23.hidePopup = true;
					rate23.Co = rate23.Co + '23';
					rate23.De = rate23.RD23;
					rate23.RCId = rate23.RCId23;
					utilityService.zeroOutExpandedRate(rate23);
					expandedRates.push(rate23);
					expanded = true;
				}

				if (rate.RD24 !== undefined && rate.RD24 != null && rate.RD24 != '') {
					var rate24 = angular.copy(rate);
					rate24.hidePopup = true;
					rate24.Co = rate24.Co + '24';
					rate24.De = rate24.RD24;
					rate24.RCId = rate24.RCId24;
					utilityService.zeroOutExpandedRate(rate24);
					expandedRates.push(rate24);
					expanded = true;
				}

				if (rate.RD25 !== undefined && rate.RD25 != null && rate.RD25 != '') {
					var rate25 = angular.copy(rate);
					rate25.hidePopup = true;
					rate25.Co = rate25.Co + '25';
					rate25.De = rate25.RD25;
					rate25.RCId = rate25.RCId25;
					utilityService.zeroOutExpandedRate(rate25);
					expandedRates.push(rate25);
					expanded = true;
				}

				if (rate.RD31 !== undefined && rate.RD31 != null && rate.RD31 != '') {
					var rate31 = angular.copy(rate);
					rate31.hidePopup = true;
					rate31.Co = rate31.Co + '31';
					rate31.De = rate31.RD31;
					rate31.RCId = rate31.RCId31;
					utilityService.zeroOutExpandedRate(rate31);
					expandedRates.push(rate31);
					expanded = true;
				}

				if (rate.RD32 !== undefined && rate.RD32 != null && rate.RD32 != '') {
					var rate32 = angular.copy(rate);
					rate32.hidePopup = true;
					rate32.Co = rate32.Co + '32';
					rate32.De = rate32.RD32;
					rate32.RCId = rate32.RCId32;
					utilityService.zeroOutExpandedRate(rate32);
					expandedRates.push(rate32);
					expanded = true;
				}

				if (rate.RD33 !== undefined && rate.RD33 != null && rate.RD33 != '') {
					var rate33 = angular.copy(rate);
					rate33.hidePopup = true;
					rate33.Co = rate33.Co + '33';
					rate33.De = rate33.RD33;
					rate33.RCId = rate33.RCId33;
					utilityService.zeroOutExpandedRate(rate33);
					expandedRates.push(rate33);
					expanded = true;
				}

				if (rate.RD34 !== undefined && rate.RD34 != null && rate.RD34 != '') {
					var rate34 = angular.copy(rate);
					rate34.hidePopup = true;
					rate34.Co = rate34.Co + '34';
					rate34.De = rate34.RD34;
					rate34.RCId = rate34.RCId34;
					utilityService.zeroOutExpandedRate(rate34);
					expandedRates.push(rate34);
					expanded = true;
				}

				if (rate.RD35 !== undefined && rate.RD35 != null && rate.RD35 != '') {
					var rate35 = angular.copy(rate);
					rate35.hidePopup = true;
					rate35.Co = rate35.Co + '35';
					rate35.De = rate35.RD35;
					rate35.RCId = rate35.RCId35;
					utilityService.zeroOutExpandedRate(rate35);
					expandedRates.push(rate35);
					expanded = true;
				}

				if (rate.RD41 !== undefined && rate.RD41 != null && rate.RD41 != '') {
					var rate41 = angular.copy(rate);
					rate41.hidePopup = true;
					rate41.Co = rate41.Co + '41';
					rate41.De = rate41.RD41;
					rate41.RCId = rate41.RCId41;
					utilityService.zeroOutExpandedRate(rate41);
					expandedRates.push(rate41);
					expanded = true;
				}

				if (rate.RD42 !== undefined && rate.RD42 != null && rate.RD42 != '') {
					var rate42 = angular.copy(rate);
					rate42.hidePopup = true;
					rate42.Co = rate42.Co + '42';
					rate42.De = rate42.RD42;
					rate42.RCId = rate42.RCId42;
					utilityService.zeroOutExpandedRate(rate42);
					expandedRates.push(rate42);
					expanded = true;
				}

				if (rate.RD43 !== undefined && rate.RD43 != null && rate.RD43 != '') {
					var rate43 = angular.copy(rate);
					rate43.hidePopup = true;
					rate43.Co = rate43.Co + '43';
					rate43.De = rate43.RD43;
					rate43.RCId = rate43.RCId43;
					utilityService.zeroOutExpandedRate(rate43);
					expandedRates.push(rate43);
					expanded = true;
				}

				if (rate.RD44 !== undefined && rate.RD44 != null && rate.RD44 != '') {
					var rate44 = angular.copy(rate);
					rate44.hidePopup = true;
					rate44.Co = rate44.Co + '44';
					rate44.De = rate44.RD44;
					rate44.RCId = rate44.RCId44;
					utilityService.zeroOutExpandedRate(rate44);
					expandedRates.push(rate44);
					expanded = true;
				}

				if (rate.RD45 !== undefined && rate.RD45 != null && rate.RD45 != '') {
					var rate45 = angular.copy(rate);
					rate45.hidePopup = true;
					rate45.Co = rate45.Co + '45';
					rate45.De = rate45.RD45;
					rate45.RCId = rate45.RCId45;
					utilityService.zeroOutExpandedRate(rate45);
					expandedRates.push(rate45);
					expanded = true;
				}

				if (rate.RD51 !== undefined && rate.RD51 != null && rate.RD51 != '') {
					var rate51 = angular.copy(rate);
					rate51.hidePopup = true;
					rate51.Co = rate51.Co + '51';
					rate51.De = rate51.RD51;
					rate51.RCId = rate51.RCId51;
					utilityService.zeroOutExpandedRate(rate51);
					expandedRates.push(rate51);
					expanded = true;
				}

				if (rate.RD52 !== undefined && rate.RD52 != null && rate.RD52 != '') {
					var rate52 = angular.copy(rate);
					rate52.hidePopup = true;
					rate52.Co = rate52.Co + '52';
					rate52.De = rate52.RD52;
					rate52.RCId = rate52.RCId52;
					utilityService.zeroOutExpandedRate(rate52);
					expandedRates.push(rate52);
					expanded = true;
				}

				if (rate.RD53 !== undefined && rate.RD53 != null && rate.RD53 != '') {
					var rate53 = angular.copy(rate);
					rate53.hidePopup = true;
					rate53.Co = rate53.Co + '53';
					rate53.De = rate53.RD53;
					rate53.RCId = rate53.RCId53;
					utilityService.zeroOutExpandedRate(rate53);
					expandedRates.push(rate53);
					expanded = true;
				}

				if (rate.RD54 !== undefined && rate.RD54 != null && rate.RD54 != '') {
					var rate54 = angular.copy(rate);
					rate54.hidePopup = true;
					rate54.Co = rate54.Co + '54';
					rate54.De = rate54.RD54;
					rate54.RCId = rate54.RCId54;
					utilityService.zeroOutExpandedRate(rate54);
					expandedRates.push(rate54);
					expanded = true;
				}

				if (rate.RD55 !== undefined && rate.RD55 != null && rate.RD55 != '') {
					var rate55 = angular.copy(rate);
					rate55.hidePopup = true;
					rate55.Co = rate55.Co + '55';
					rate55.De = rate55.RD55;
					rate55.RCId = rate55.RCId55;
					utilityService.zeroOutExpandedRate(rate55);
					expandedRates.push(rate55);
					expanded = true;
				}

				if (rate.RD61 !== undefined && rate.RD61 != null && rate.RD61 != '') {
					var rate61 = angular.copy(rate);
					rate61.hidePopup = true;
					rate61.Co = rate61.Co + '61';
					rate61.De = rate61.RD61;
					rate61.RCId = rate61.RCId61;
					utilityService.zeroOutExpandedRate(rate61);
					expandedRates.push(rate61);
					expanded = true;
				}

				if (rate.RD62 !== undefined && rate.RD62 != null && rate.RD62 != '') {
					var rate62 = angular.copy(rate);
					rate62.hidePopup = true;
					rate62.Co = rate62.Co + '62';
					rate62.De = rate62.RD62;
					rate62.RCId = rate62.RCId62;
					utilityService.zeroOutExpandedRate(rate62);
					expandedRates.push(rate62);
					expanded = true;
				}

				if (rate.RD63 !== undefined && rate.RD63 != null && rate.RD63 != '') {
					var rate63 = angular.copy(rate);
					rate63.hidePopup = true;
					rate63.Co = rate63.Co + '63';
					rate63.De = rate63.RD63;
					rate63.RCId = rate63.RCId63;
					utilityService.zeroOutExpandedRate(rate63);
					expandedRates.push(rate63);
					expanded = true;
				}

				if (rate.RD64 !== undefined && rate.RD64 != null && rate.RD64 != '') {
					var rate64 = angular.copy(rate);
					rate64.hidePopup = true;
					rate64.Co = rate64.Co + '64';
					rate64.De = rate64.RD64;
					rate64.RCId = rate64.RCId64;
					utilityService.zeroOutExpandedRate(rate64);
					expandedRates.push(rate64);
					expanded = true;
				}

				if (rate.RD65 !== undefined && rate.RD65 != null && rate.RD65 != '') {
					var rate65 = angular.copy(rate);
					rate65.hidePopup = true;
					rate65.Co = rate65.Co + '65';
					rate65.De = rate65.RD65;
					rate65.RCId = rate65.RCId65;
					utilityService.zeroOutExpandedRate(rate65);
					expandedRates.push(rate65);
					expanded = true;
				}

				if (rate.RD71 !== undefined && rate.RD71 != null && rate.RD71 != '') {
					var rate71 = angular.copy(rate);
					rate71.hidePopup = true;
					rate71.Co = rate71.Co + '71';
					rate71.De = rate71.RD71;
					rate71.RCId = rate71.RCId71;
					utilityService.zeroOutExpandedRate(rate71);
					expandedRates.push(rate71);
					expanded = true;
				}

				if (rate.RD72 !== undefined && rate.RD72 != null && rate.RD72 != '') {
					var rate72 = angular.copy(rate);
					rate72.hidePopup = true;
					rate72.Co = rate72.Co + '72';
					rate72.De = rate72.RD72;
					rate72.RCId = rate72.RCId72;
					utilityService.zeroOutExpandedRate(rate72);
					expandedRates.push(rate72);
					expanded = true;
				}

				if (rate.RD73 !== undefined && rate.RD73 != null && rate.RD73 != '') {
					var rate73 = angular.copy(rate);
					rate73.hidePopup = true;
					rate73.Co = rate73.Co + '73';
					rate73.De = rate73.RD73;
					rate73.RCId = rate73.RCId73;
					utilityService.zeroOutExpandedRate(rate73);
					expandedRates.push(rate73);
					expanded = true;
				}

				if (rate.RD74 !== undefined && rate.RD74 != null && rate.RD74 != '') {
					var rate74 = angular.copy(rate);
					rate74.hidePopup = true;
					rate74.Co = rate74.Co + '74';
					rate74.De = rate74.RD74;
					rate74.RCId = rate74.RCId74;
					utilityService.zeroOutExpandedRate(rate74);
					expandedRates.push(rate74);
					expanded = true;
				}

				if (rate.RD75 !== undefined && rate.RD75 != null && rate.RD75 != '') {
					var rate75 = angular.copy(rate);
					rate75.hidePopup = true;
					rate75.Co = rate75.Co + '75';
					rate75.De = rate75.RD75;
					rate75.RCId = rate75.RCId75;
					utilityService.zeroOutExpandedRate(rate75);
					expandedRates.push(rate75);
					expanded = true;
				}

				if (rate.RD81 !== undefined && rate.RD81 != null && rate.RD81 != '') {
					var rate81 = angular.copy(rate);
					rate81.hidePopup = true;
					rate81.Co = rate81.Co + '81';
					rate81.De = rate81.RD81;
					rate81.RCId = rate81.RCId81;
					utilityService.zeroOutExpandedRate(rate81);
					expandedRates.push(rate81);
					expanded = true;
				}

				if (rate.RD82 !== undefined && rate.RD82 != null && rate.RD82 != '') {
					var rate82 = angular.copy(rate);
					rate82.hidePopup = true;
					rate82.Co = rate82.Co + '82';
					rate82.De = rate82.RD82;
					rate82.RCId = rate82.RCId82;
					utilityService.zeroOutExpandedRate(rate82);
					expandedRates.push(rate82);
					expanded = true;
				}

				if (rate.RD83 !== undefined && rate.RD83 != null && rate.RD83 != '') {
					var rate83 = angular.copy(rate);
					rate83.hidePopup = true;
					rate83.Co = rate83.Co + '83';
					rate83.De = rate83.RD83;
					rate83.RCId = rate83.RCId83;
					utilityService.zeroOutExpandedRate(rate83);
					expandedRates.push(rate83);
					expanded = true;
				}

				if (rate.RD84 !== undefined && rate.RD84 != null && rate.RD84 != '') {
					var rate84 = angular.copy(rate);
					rate84.hidePopup = true;
					rate84.Co = rate84.Co + '84';
					rate84.De = rate84.RD84;
					rate84.RCId = rate84.RCId84;
					utilityService.zeroOutExpandedRate(rate84);
					expandedRates.push(rate84);
					expanded = true;
				}

				if (rate.RD85 !== undefined && rate.RD85 != null && rate.RD85 != '') {
					var rate85 = angular.copy(rate);
					rate85.hidePopup = true;
					rate85.Co = rate85.Co + '85';
					rate85.De = rate85.RD85;
					rate85.RCId = rate85.RCId85;
					utilityService.zeroOutExpandedRate(rate85);
					expandedRates.push(rate85);
					expanded = true;
				}

				if (rate.RD91 !== undefined && rate.RD91 != null && rate.RD91 != '') {
					var rate91 = angular.copy(rate);
					rate91.hidePopup = true;
					rate91.Co = rate91.Co + '91';
					rate91.De = rate91.RD91;
					rate91.RCId = rate91.RCId91;
					utilityService.zeroOutExpandedRate(rate91);
					expandedRates.push(rate91);
					expanded = true;
				}

				if (rate.RD92 !== undefined && rate.RD92 != null && rate.RD92 != '') {
					var rate92 = angular.copy(rate);
					rate92.hidePopup = true;
					rate92.Co = rate92.Co + '92';
					rate92.De = rate92.RD92;
					rate92.RCId = rate92.RCId92;
					utilityService.zeroOutExpandedRate(rate92);
					expandedRates.push(rate92);
					expanded = true;
				}

				if (rate.RD93 !== undefined && rate.RD93 != null && rate.RD93 != '') {
					var rate93 = angular.copy(rate);
					rate93.hidePopup = true;
					rate93.Co = rate93.Co + '93';
					rate93.De = rate93.RD93;
					rate93.RCId = rate93.RCId93;
					utilityService.zeroOutExpandedRate(rate93);
					expandedRates.push(rate93);
					expanded = true;
				}

				if (rate.RD94 !== undefined && rate.RD94 != null && rate.RD94 != '') {
					var rate94 = angular.copy(rate);
					rate94.hidePopup = true;
					rate94.Co = rate94.Co + '94';
					rate94.De = rate94.RD94;
					rate94.RCId = rate94.RCId94;
					utilityService.zeroOutExpandedRate(rate94);
					expandedRates.push(rate94);
					expanded = true;
				}

				if (rate.RD95 !== undefined && rate.RD95 != null && rate.RD95 != '') {
					var rate95 = angular.copy(rate);
					rate95.hidePopup = true;
					rate95.Co = rate95.Co + '95';
					rate95.De = rate95.RD95;
					rate95.RCId = rate95.RCId95;
					utilityService.zeroOutExpandedRate(rate95);
					expandedRates.push(rate95);
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
				function (rate) {
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
