<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CustomReportSelectorModelView>" %>

<div id="CustomReportSelectorContainer" class="display-none">
    <div id="CustomReportSelectorDialog" class="custom-report-selector-dialog">
        <div class="container">
        <%
            Html.RenderPartial(WebConstants.VIEW_BOE_CUSTOM_REPORT_SELECTOR_FORM, Model, ViewData);
        %>
        </div>
    </div>
</div>


<script type="text/javascript">
    var CustomReportSelectorWidget;

    InitializeCustomReportSelector = function () {
        var DialogConfigs = [];

        DialogConfigs.push({
            ElementID: "CustomReportSelectorContainer",
            Params: {
                width: 850,
                title: "All BOEs Report: Custom Export",
                disabled: false,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false
            }
        });

        var FormConfigs = [];

        FormConfigs.push({
            ElementID: "CustomReportSelectorForm",
            Buttons: [
            {
                ButtonClass: 'ies',
                ButtonText: 'Export',
                ButtonName: "export-button",
                ButtonAction: function (buttonPressed) {
                    CustomReportSelectorWidget.DoExport();
                },
                Stateful: false
            }, {
                ButtonClass: 'ies',
                ButtonText: 'Cancel',
                ButtonName: "cancel-button",
                ButtonAction: function (buttonPressed) {
                    CustomReportSelectorWidget.getDialog("CustomReportSelectorContainer").closeDialog();
                },
                Stateful: false
            }],
			ContainsOCI: <%= ViewData["ContainsOCI"] %>,
			HideOCI: false,
			// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
			BannerTextWithOCI: <%: SiteMasterUtilities.GetBannerText(true) %>,
			BannerTextWithoutOCI: <%: SiteMasterUtilities.GetBannerText() %>
        });

        var widgetConfig = {};
        widgetConfig.ContextID = "CustomReportSelectorDialog";
        widgetConfig.IsModule = false;
        widgetConfig.FormConfigs = FormConfigs;
        widgetConfig.DialogConfigs = DialogConfigs;

        CustomReportSelectorWidget = new GenWidget(widgetConfig);

        CustomReportSelectorWidget.GetSelectedFormData = function () {
            // set the hidden summarizeByCustomField value from the SummarizeByCustomField dialog (if present) 
            $('#CustomReportSelectorForm [name="summarizeByCustomField"]').val($('#SummarizeByCustomField').val());

            // extract form data
            $('#CustomReportSelectorDialog #BoesSelected option, #CustomReportSelectorDialog #ComponentsSelected option').prop('selected', true);
            var formData = CustomReportSelectorWidget.getForm("CustomReportSelectorForm").getData();

            // redefine BOE selections list as a dictionary - values AND text display
            formData.BoesSelected = [];
            $('#CustomReportSelectorDialog #BoesSelected option').each(function () {
                var entry = { 'Key': $(this).val(), 'Value': $(this).text() };
                formData.BoesSelected.push(entry);
            });

            return formData;
        };

        CustomReportSelectorWidget.DoExport = function () {
            // close the dialog window - Note: The contents of the dialog will still exist in the DOM, but they will not be visible to the user
            CustomReportSelectorWidget.getDialog("CustomReportSelectorContainer").closeDialog();

            // Force a refresh of the 'selected' data before exporting.
            CustomReportSelectorWidget.GetSelectedFormData();

            // set up the form to trigger creation of the report
            var exportActionUrl = '/<%:Model.WorkspaceName%>/<%:WebConstants.CONTROLLER_REPORTS%>/<%:WebConstants.ACTION_EXPORT_BOE_CUSTOM_REPORT%>';
            $('#CustomReportSelectorForm').attr("action", exportActionUrl);

            // submit form (synchronously) to cause document to open
            $('#CustomReportSelectorForm').submit();
        };

        // Re-populate the excluded BOE list, as well as the filter-by-value options.
        // Optionally, pass in any parameter (provided arguments.length == 1 is true) to use the parent (Exports.ascx) 
        // hidden field to get the saved filter-by-value.
        CustomReportSelectorWidget.FilterSelectorDisplay = function (getFilterByValueFromHiddenSpan) {
            $('#CustomReportSelectorDialogMainDialog').css("display", "none");  // hide the Main portion of the dialog
            $('#CustomReportSelectorDialogIsBusy').removeClass('display-none'); // show the Spinner portion of the dialog 

            // extract form data
            $('#CustomReportSelectorDialog #BoesSelected option, #CustomReportSelectorDialog #ComponentsSelected option').prop('selected', true);
            var formData = CustomReportSelectorWidget.GetSelectedFormData();

            // Get the Filter Value via the hidden span element
            if (arguments.length == 1) {
                formData['BoeSortByValue'] = $("#hiddenFilterByValue").text();
                formData['BoeSecondarySortByValue'] = $("#hiddenSecondaryFilterByValue").text();
            }

            var formDataJSON = JSON.stringify(formData);

            // assemble URL for re-display of the dialog
            var reloadUrl = CreatePostURL('<%:Model.WorkspaceName%>', '<%:WebConstants.CONTROLLER_REPORTS%>', '<%:WebConstants.ACTION_DISPLAY_BOE_CUSTOM_REPORT_SELECTOR%>');

            // submit the request (asynchronously) to apply filters and re-display the dialog contents
            $.ajax({
                type: 'post',
                url: reloadUrl,
                contentType: 'application/json; charset=utf-8',
                data: formDataJSON,
                dataType: 'html',
                success: function (response) {
                    $('#CustomReportSelectorDialogIsBusy').addClass('display-none');      // hide the Spinner portion of the dialog
                    $('#CustomReportSelectorDialogMainDialog').css("display", "inline");  // show the main portion of the dialog
                    // reload the dialog
                    $('#BOECustomReportSelectorOuterContainer').html(response);
                }
            });
        };

        // Opens the Format Element dialog with fields cleared and ready to create a new format
        CustomReportSelectorWidget.OpenDialog = function () {
            $('#CustomReportSelectorDialog #BoesSelected option, #CustomReportSelectorDialog #ComponentsSelected option').prop('selected', false);
            CustomReportSelectorWidget.getDialog("CustomReportSelectorContainer").openDialog();
        };

        // Sorts the items in a select list
        CustomReportSelectorWidget.SortSelectList = function sortSelect(list) {
            list = $(list);

            var optionArray = list.children('option');

            optionArray.sort(CustomReportSelectorWidget.CompareSelectListOptions);

            list.children().remove();

            for (var ndx = 0; ndx < optionArray.length; ndx++) {
                list.append(optionArray[ndx]);
            }

            return;
        };

        // Function to compare two Select List Option Items for equality
        CustomReportSelectorWidget.CompareSelectListOptions = function (a, b) {
            var aText = $(a).text().toLowerCase();
            var bText = $(b).text().toLowerCase();

            if (aText < bText)
                return -1;
            else if (aText > bText)
                return 1;
            else
                return 0;
        };

        CustomReportSelectorWidget.MoveSelectedItems = function (fromList, toList) {
            $(fromList).children('option:selected').each(function () {
                CustomReportSelectorWidget.MoveSelectListItem(this, toList);
            });
        };

        CustomReportSelectorWidget.MoveSelectListItem = function (item, toList) {
            item = $(item);

            // Do not move required fields
            if (!item.attr('required')) {
                var value = item.val();
                $(toList).append(item);
            }

            // Unselect each item
            item.removeAttr('selected');
        };

        // Moves selected list items up in the list
        CustomReportSelectorWidget.ShiftSelectedItemsUp = function (list) {
            $(list).children('option:selected').each(function () {
                if ($(this).prev().length == 0) {
                    return false;
                }

                $(this).prev().before($(this));
            });
        };

        // Moves selected list items down in the list
        CustomReportSelectorWidget.ShiftSelectedItemsDown = function (list) {
            $($(list).children('option:selected').get().reverse()).each(function () {
                if ($(this).next().length == 0) {
                    return false;
                }

                $(this).next().after($(this));
            });
        };

        CustomReportSelectorWidget.BindEvents = function () {

            $('#CustomReportSelectorDialog #BoeSortBy').change(function () {
                //  unselect all sort-by-value options which will set first (empty) option
                $('#CustomReportSelectorDialog #BoeSortByValue option').prop('selected', false);
                $('#CustomReportSelectorDialog #BoeSecondarySortByValue option').prop('selected', false);

                // Changing the sort-by-value option means that the filter-by-value drop-down 
                // value should now be blank to get "Filter by Criteria" as the first choice
                $("#CustomReportSelectorDialog #BoeSortByValue option").filter(function () {
                    return $(this).val() == '';
                }).prop('selected', true);
                $("#CustomReportSelectorDialog #BoeSecondarySortByValue option").filter(function () {
                    return $(this).val() == '';
                }).prop('selected', true);

                // Clear the filter-by-value since a new sort-by-value was picked
                $("#hiddenFilterByValue").text('');
                $("#hiddenSecondaryFilterByValue").text('');
                CustomReportSelectorWidget.FilterSelectorDisplay();
            });

            $('#CustomReportSelectorDialog #BoeSortByValue').change(function () {
                // When changing the filter-by-value option, save its state in the parent page's hidden <span>               
                var boeSortByVal = $('#BoeSortByValue option:selected').val();
                $("#hiddenFilterByValue").text(boeSortByVal);

                //Clear the secondary filter - if left, could result in zero BOEs to select
                $('#CustomReportSelectorDialog #BoeSecondarySortByValue option').prop('selected', false);
                $("#CustomReportSelectorDialog #BoeSecondarySortByValue option").filter(function () {
                    return $(this).val() == '';
                }).prop('selected', true);
                $("#hiddenSecondaryFilterByValue").text('');

                CustomReportSelectorWidget.FilterSelectorDisplay();
            });

            $('#CustomReportSelectorDialog #BoeSecondarySortBy').change(function () {
                //  unselect all sort-by-value options which will set first (empty) option
                $('#CustomReportSelectorDialog #BoeSecondarySortByValue option').prop('selected', false);

                // Changing the sort-by-value option means that the filter-by-value drop-down 
                // value should now be blank to get "Filter by Criteria" as the first choice
                $("#CustomReportSelectorDialog #BoeSecondarySortByValue option").filter(function () {
                    return $(this).val() == '';
                }).prop('selected', true);

                // Clear the filter-by-value since a new sort-by-value was picked
                $("#hiddenSecondaryFilterByValue").text('');
                CustomReportSelectorWidget.FilterSelectorDisplay();
            });

            $('#CustomReportSelectorDialog #BoeSecondarySortByValue').change(function () {
                // When changing the filter-by-value option, save its state in the parent page's hidden <span>               
                var boeSecondarySortByVal = $('#BoeSecondarySortByValue option:selected').val();
                $("#hiddenSecondaryFilterByValue").text(boeSecondarySortByVal);

                CustomReportSelectorWidget.FilterSelectorDisplay();
            });

            $('#CustomReportSelectorDialog #BoesMoveToSelected').click(function () {
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #BoesUnselected'), $('#CustomReportSelectorDialog #BoesSelected'));
            });

            $('#CustomReportSelectorDialog #BoesMoveAllToSelected').click(function () {
                $('#BoesUnselected option').prop('selected', 'selected');
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #BoesUnselected'), $('#CustomReportSelectorDialog #BoesSelected'));
            });

            $('#CustomReportSelectorDialog #BoesMoveToUnselected').click(function () {
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #BoesSelected'), $('#CustomReportSelectorDialog #BoesUnselected'));
                CustomReportSelectorWidget.SortSelectList($('#CustomReportSelectorDialog #BoesUnselected'));

                CustomReportSelectorWidget.FilterSelectorDisplay(true);
            });

            $('#CustomReportSelectorDialog #BoesMoveAllToUnselected').click(function () {
                $('#BoesSelected option').prop('selected', 'selected');
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #BoesSelected'), $('#CustomReportSelectorDialog #BoesUnselected'));
                CustomReportSelectorWidget.SortSelectList($('#CustomReportSelectorDialog #BoesUnselected'));

                CustomReportSelectorWidget.FilterSelectorDisplay(true);
            });

            $('#CustomReportSelectorDialog #BoesMoveItemsUp').click(function () {
                CustomReportSelectorWidget.ShiftSelectedItemsUp($('#CustomReportSelectorDialog #BoesSelected'));
            });

            $('#CustomReportSelectorDialog #BoesMoveItemsDown').click(function () {
                CustomReportSelectorWidget.ShiftSelectedItemsDown($('#CustomReportSelectorDialog #BoesSelected'));
            });

            $('#CustomReportSelectorDialog #ComponentsMoveToSelected').click(function () {
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #ComponentsUnselected'), $('#CustomReportSelectorDialog #ComponentsSelected'));
            });

            // move all components to selected
            $('#CustomReportSelectorDialog #ComponentsMoveAllToSelected').click(function () {
                $('#ComponentsUnselected option').prop('selected', 'selected');
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #ComponentsUnselected'), $('#CustomReportSelectorDialog #ComponentsSelected'));
            });

            $('#CustomReportSelectorDialog #ComponentsMoveToUnselected').click(function () {
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #ComponentsSelected'), $('#CustomReportSelectorDialog #ComponentsUnselected'));
                CustomReportSelectorWidget.SortSelectList($('#CustomReportSelectorDialog #ComponentsUnselected'));
            });

            // move all components to un-selected
            $('#CustomReportSelectorDialog #ComponentsMoveAllToUnselected').click(function () {
                $('#ComponentsSelected option').prop('selected', 'selected');
                CustomReportSelectorWidget.MoveSelectedItems($('#CustomReportSelectorDialog #ComponentsSelected'), $('#CustomReportSelectorDialog #ComponentsUnselected'));
            });

            $('#CustomReportSelectorDialog #ComponentsMoveItemsUp').click(function () {
                CustomReportSelectorWidget.ShiftSelectedItemsUp($('#CustomReportSelectorDialog #ComponentsSelected'));
            });

            $('#CustomReportSelectorDialog #ComponentsMoveItemsDown').click(function () {
                CustomReportSelectorWidget.ShiftSelectedItemsDown($('#CustomReportSelectorDialog #ComponentsSelected'));
            });
        };

        $.each($('#BoesSelected option, #BoesUnselected option, #ComponentsSelected option, #ComponentsUnselected option'), function () {
            $(this).attr('title', $(this).text());
        });

        CustomReportSelectorWidget.BindEvents();
        CustomReportSelectorWidget.OpenDialog();
    };

    $(function () {
        InitializeCustomReportSelector();

        $("#CustomReportSelectorForm button[name='export-button']").css('margin-left', '280px');
    });
</script>
