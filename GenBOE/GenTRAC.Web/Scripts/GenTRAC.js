var GenTRAC;

$(function () {
    GenTRAC = new GenTRACUtilities();
});

function GenTRACUtilities() {
};

/*
* Will handle sorting. PLEASE FOLLOW THESE DIRECTIONS
*
* It assumes that there is a hidden field for SortField and Order, which shows what the current sortfield and Order are.
* Please derive your MV from SortableModelView.
*
* You need to put sortable class on the table that contains the data;
* You need to implement a function named 'ReloadGrid' in your widget to be called when the sort is clicked.
* You need to put sortID attribute on each of the th. This should be the property name by which you want to sort.
*
* For a working example, please see Manage Skill Mix
*/
GenTRACUtilities.prototype.sortChanged = function (widget, header) {
    var newSortField = header.attr('sortby');
    var newSortDirection = 'Ascending';

    var oldSortField = $('#SortField', widget.context).val();
    var oldSortDirection = $('#Order', widget.context).val();

    $('table.sortable th[sortby=' + oldSortField + ']').removeClass('bold');
    $('table.sortable th[sortby=' + newSortField + ']').addClass('bold');

    if (oldSortField == newSortField) {
        if (oldSortDirection == 'Ascending') {
            newSortDirection = 'Descending';
        }
    }

    $('#SortField', widget.context).val(newSortField);
    $('#Order', widget.context).val(newSortDirection);

    widget.ReloadGrid();
};

GenTRACUtilities.prototype.bindSorting = function (widget) {
    var table = $('table.sortable', widget.context);

    if (table.length > 0) {
        $('th[sortby]', table).each(function () {
            var thisHeader = $(this);
            var thisSortID = thisHeader.attr('sortID');

            // Make each sort header appear styled as a link
            thisHeader.addClass('link');
        });

        var currentSort = $('#SortField', widget.context).val();
        $('th[sortby=' + currentSort + ']', table).addClass('bold');

        widget.on('click', 'th[sortby]', table, function (e) {
            if (!$(e.target).hasClass("help-dialog-text") && !$(e.target).hasClass("help-icon")) {
                GenTRAC.sortChanged(widget, $(this))
            }
        });
    }
};

GenTRACUtilities.prototype.validateDateStringFormatMMDDYYYY = function (dateText) {
    var valid;

    var dateItems = dateText.split('/');
    if (dateItems.length == 3) {
        var month = new Number(dateItems[0]);
        var day = new Number(dateItems[1]);
        var year = new Number(dateItems[2]);

        var dateValue = new Date();
        dateValue.setMonth(month - 1);  // 0-based index
        dateValue.setDate(day);
        dateValue.setFullYear(year);

        var resultingMonth = dateValue.getMonth() + 1;  // 0-based index

        if (dateValue == undefined || dateValue == NaN) {
            valid = false;
        } else if (resultingMonth == month) {
            valid = true;
        } else {
            valid = false;
        }
    } else {
        valid = false;
    }

    return valid;
}

GenTRACUtilities.prototype.numericCommaSeparator = function (widget, dollarAmount, element, readOnly) {
    if (!isNaN(parseFloat(dollarAmount)) && isFinite(dollarAmount)) {
        dollarAmount = String(dollarAmount).replace(/\B(?=(\d{3})+(?!\d))/g, ","); // add comma separator for every three numerical digits
    }

    if (readOnly) {
        var readOnlyValue = widget.getElement(element).next('.replacedWidgetText');
        readOnlyValue.html(dollarAmount);
        readOnlyValue.css('text-align', 'right');
        readOnlyValue.css('width', '125');
    } else {
        widget.getElement(element).val(dollarAmount);
    }
}

GenTRACUtilities.prototype.SetCheckAttr = function (el) {
    if(el) {
        if(el.checked)  
            el.setAttribute('checked','checked');
        else            
            el.removeAttribute("checked");
    }
}

/*
 Allow the user to select/deselect an element from drop down list with the mouse button.  
 This is being used on all of the genTRAC report filters
*/
GenTRACUtilities.prototype.UpdateSelect = function (list, row, id) {
    list[row.selectedIndex] = !list[row.selectedIndex];
    rebuildList(list, id);
}

/*
 Helper function for function UpdateSelect()
*/
function rebuildList(list, id) {
    var selectIds = document.getElementById(id);
    for (var i = 0; i < list.length; i++) {
        selectIds.options[i].selected = list[i];
    }
}

/*
 This controls when the free form text boxes are shown/hidden
 The selectors in this function are specific to the ProposalGeneralInformation widget
*/
GenTRACUtilities.prototype.toggleTextBox = function (widget) {
    widget.registerForDelegateEvent('change', function (e) {
        if (e.target.id == 'BOETool') {
            $('#BOEToolName').toggle((e.target.value) == 6);
        }
        else if (e.target.id == 'PricingTool') {
            $('#PricingToolName').toggle((e.target.value) == 3);
        }
        else if (e.target.id == 'ProposalLocation') {
            $('#ProposalLocationName').toggle((e.target.value) == 9);
        }
    });
}

/*
    * This is used on on any input that needs to have their number input have a maximum length that ignores commas and decimal places
    *   By default the following is allowed: numbers, comma, period. 
    *   If 
    */
GenTRACUtilities.prototype.applyMaxLengthFilter = function (widget) {
    widget.registerForDelegateEvent('keypress', 'input[maxInt], input[numDecimals], input[numPostSlash]', function (e) {
        // Figure out which character was pressed
        var charCode = e.which;
        var charEntered = String.fromCharCode(charCode);
        var element = $(this);
        var curserPosition = this.selectionStart;
        var originalString = element.val();

        // charcode 0 is for arrow keys and other things that we want to let go
        // charcode 8 is for backspace, again, we do nothing
        if (charCode != 0 && charCode != 8) {
            // Set timeout forces this code to execute after the event is able to modify the textbox
            // This is so that way we can deal w/ cases where the user highlights the max length string and types a letter over it, in essence erasing the original content
            setTimeout(function () {
                // We allow enter, backspace, and functional keys
                if (charCode == 13 || charCode == 0) {
                    // Note: "element" will only ever "belong under" the selector (above) used to register the delegate event
                    element.blur();  // use "blur" instead of "change" to avoid duplicate events
                }
                else {
                    // Figure out which element has focus. Keypress can only happen inside of that element. 
                    var maxIntegers = element.attr('maxInt');
                    var numDecimals = element.attr('numDecimals');
                    var numPostSlash = element.attr('numPostSlash');

                    if (maxIntegers != undefined) {
                        var validateInts = true;
                    }

                    if (numDecimals != undefined) {
                        var validateDec = true;
                    }

                    if (numPostSlash != undefined) {
                        var validatePostSlash = true;
                    }

                    if ((validateDec || validateInts || validatePostSlash) && charCode >= 32 && charCode <= 126 && !charEntered.match(/[0-9.,\-\/]/)) {
                        // it's not a number, comma or period or negative sign or forward slash, we prevent it.
                        element.val(originalString);
                    }

                    var inputString = element.val();
                    inputString = inputString.replace(/\,/g, '');

                    // only allow a leading '-' to be input
                    if (inputString.indexOf('-') >= 0) {
                        var s = inputString.substring(1);
                        if (s.indexOf('-') >= 0) {
                            element.val(originalString);
                        }
                    }

                    // hide leading '-' character if it exists
                    inputString = inputString.replace(/\-/g, '');

                    // If the value already contains a decimal.  We assume this will only ever have 0 or 1 decimal or this check has failed.
                    if (inputString.indexOf('.') >= 0) {
                        //Prevent a second decimal
                        if ((charEntered == '.' && numDecimals == '0') || ((inputString.split(".").length - 1) > 1) || charEntered == '-') {
                            element.val(originalString);
                        }

                        var stringParts = inputString.split('.');
                        var integers = stringParts[0];
                        var decimals = stringParts[1];

                        if (validateInts && integers.length > maxIntegers && integers != '') {
                            element.val(originalString);
                        }
                        if (validateDec && decimals.length > numDecimals && decimals != '') {
                            element.val(originalString);
                        }
                    // Handle the '/' divider, be sure there is only one
                    } else if (inputString.indexOf('/') >= 0) {
                        // Prevent multiple occurances of '/' and be sure it is not in position 0
                        var pos = inputString.indexOf('/');
                        if ((charEntered == '/' && numPostSlash == '0') || ((inputString.split("/").length - 1) > 1) || charEntered == '-' || pos == 0) {
                            element.val(originalString);
                        }

                        var stringParts = inputString.split('/');
                        var integers = stringParts[0];
                        var postSlash = stringParts[1];

                        if (validatePostSlash && integers.length > maxIntegers && integers != '') {
                            element.val(originalString);
                        }
                        if (validatePostSlash && postSlash.length > numPostSlash && postSlash != '') {
                            element.val(originalString);
                        }
                    } else {
                        // No decimal or forward slash so only need to validate integers.
                        if (validateInts && inputString.length > maxIntegers && charEntered != '.' && charEntered != ',' && charEntered != '-' && charEntered != '+'
                            && charEntered != '/' || validatePostSlash && charEntered == '-') {
                            element.val(originalString);
                        }
                    }
                }
            }, 1);
        }
    });
}

