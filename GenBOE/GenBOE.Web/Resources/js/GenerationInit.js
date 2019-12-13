/**************** JQUERY UI AutoComplete Combo Box **************/

$.widget("ui.combobox", {
    _create: function () {
        var self = this,
					select = this.element.hide(),
					selected = select.children(":selected"),
					value = selected.val() ? selected.text() : "";
        var input = $("<input>")
					.insertAfter(select)
					.val(value)
					.autocomplete({
					    delay: 0,
					    minLength: 0,
					    source: function (request, response) {
					        var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
					        response(select.children("option").map(function () {
					            var text = $(this).text();
					            if (this.value && (!request.term || matcher.test(text)))
					                return {
					                    label: text.replace(
											new RegExp(
												"(?![^&;]+;)(?!<[^<>]*)(" +
												$.ui.autocomplete.escapeRegex(request.term) +
												")(?![^<>]*>)(?![^&;]+;)", "gi"
											), "<strong>$1</strong>"),
					                    value: text,
					                    option: this
					                };
					        }));
					    },
					    select: function (event, ui) {
					        ui.item.option.selected = true;
					        self._trigger("selected", event, {
					            item: ui.item.option
					        });
					    },
					    change: function (event, ui) {
					        if (!ui.item) {
					            var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex($(this).val()) + "$", "i"),
									valid = false;
					            select.children("option").each(function () {
					                if (this.value.match(matcher)) {
					                    this.selected = valid = true;
					                    return false;
					                }
					            });
					            if (!valid) {
					                // remove invalid value, as it didn't match anything
					                $(this).val("");
					                select.val("");
					                return false;
					            }
					        }
					    }
					})
					.addClass("ui-widget ui-widget-content ui-corner-left");

        input.data("autocomplete")._renderItem = function (ul, item) {
            return $("<li></li>")
						.data("item.autocomplete", item)
						.append("<a>" + item.label + "</a>")
						.appendTo(ul);
        };

        $("<button>&nbsp;</button>")
					.attr("tabIndex", -1)
					.attr("title", "Show All Items")
					.insertAfter(input)
					.button({
					    icons: {
					        primary: "ui-icon-triangle-1-s"
					    },
					    text: false
					})
					.removeClass("ui-corner-all")
					.addClass("ui-corner-right autocomplete-button ui-button-icon")
					.click(function () {
					    // close if already visible
					    if (input.autocomplete("widget").is(":visible")) {
					        input.autocomplete("close");
					        return;
					    }

					    // pass empty string as value to search for, displaying all results
					    input.autocomplete("search", "");
					    input.focus();
					});
    }
});

/************** End JQUERY UI AutoComplete Combo Box *************/

/** 
 * Starts a profiler for timing javascript execution, and prints the execution time if the profiler already exists
 *
 * @param name
 *           name of profiler
 */
$.profile = function (name) {
    var date = new Date();
    if (this.profile[name] == undefined) {
        this.profile[name] = date.getTime();
    } else {
        var timeElapsed = date.getTime() - this.profile[name];
        console.log(name + ": " + timeElapsed + " ms");
    }
}

/** 
 * Resets the time of the profiler to 0
 *
 * @param name
 *           name of profiler
 */
$.resetProfile = function (name) {
    this.profile[name] = new Date().getTime();
}

/*********************
    Settings
***********************/

//Creates a blank log method if there is no logging in the browser.
if (!window.console) {
    window.console = {};
    window.console.log = function (message) { };
}


var GenSession;

/*************************************
    Page Level Initializations
**************************************/
$(function () {

    //This is a backwards compatibility line to create a GenPageManager.  this can be removed one all projects configure this in there own js file.
    GenSession = new GenPageManager({});
    // turn caching off for all of our AJAX calls
    $.ajaxSetup({ cache: false });

    $.datepicker.setDefaults({
        dateFormat: 'mm/yy',
        changeMonth: true,
        changeYear: true,
        showAnim: 'fadeIn',
        showOtherMonths: true,
        selectOtherMonths: true,
        showOn: 'both',
        yearRange: "c-10:c+40",
        buttonImageOnly: false,
        buttonText: '',
        onSelect: function (dateText, inst) {
            GenSession.OnDatepickerSelect(dateText, inst, $(this));
        }
    });

    $(window).bind('beforeunload', function () {
        if (GenSession.isDirty()) {
            return "Changes have not been saved. Are you sure you want to navigate away?";
        }
    });

    $(document).bind('DATA_DIRTY', function (e, formName) {
        GenSession.setDirty(formName);
    });

    $(document).bind('DATA_CLEANED', function (e, formName) {
        GenSession.clearDirty(formName);
    });

    $('.tabs a').click(function () {
        $(this).parents('.tabs').find('a').removeClass('active');

        $(this).addClass('active');
    });

    // Creates a popup whenever an ajax requests errors
    $(document).ajaxError(function (event, request) {
        //ignore all readyStates before a response was received.
        if (request.readyState > 2) {
            var error = {};
            if (request.responseText.indexOf('{') < 0 || request.responseText.indexOf('{') > 2) {
                error.Message = "An error has occurred. Any changes you made recently might be lost. Please copy your changes, refresh the page and try again. If the error persists, please post on this application's support stream.";
                error.Details = "No details";
                error.Title = "Application Error";
            }
            else {
                error = $.parseJSON(request.responseText);
            }

            if (error.ReturnType != "GenValidationException") {
                GenSession.DisplayExceptionDialog(error);
            }
            GenSession.HideLoadingBox();
        }
    });

    $("#ToggleErrorDetails").click(function () {
        $("#ErrorDetails").toggle('slow');
    });
});



