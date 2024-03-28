/**
* @fileOverview This library requires JQuery, JSON2 and a GenPageManager Object
*               called GenSession created during the page initialization The
*               requirement for the creation of the GenSession var may be
*               removed in a future version.
* @requires GenSession
* @requires JSON2
* @requires jQuery ($)
*/

/**
 * @class
 * Constants for Generation Tools, these should not be changed programmatically but used for reference.
 */
var GenConstants = {};

/**
* The class used to denote a container for a group of buttons: "buttons".
*/
GenConstants.BUTTON_CONTAINER_CLASS = "buttons";

/**
* The class used to denote a container for a group of acknowledgeable messages
*/
GenConstants.ACK_MESSAGES_CONTAINER_CLASS = "ack-messages";

/**
*  The class used to identify a widget that has been marked as readonly.
*/
GenConstants.READ_ONLY_WIDGET_MARKER_CLASS = "read-only-applied";

/**
* The class used to denote a textbox should be a datepicker: "datepicker".
*/
GenConstants.DATEPICKER_CLASS = "datepicker";

/**
* The attribute used to add helptext to an element: "helptext".
*/
GenConstants.HELPTEXT_ATTRIBUTE = "helptext";

/**
* the delay of helptext once hover is lost.
*/
GenConstants.HELPTEXT_HIDE_DELAY = 500;

/**
* the delay before showing helptext once the user places his mouse over the help icon.
*/
GenConstants.HELPTEXT_SHOW_DELAY = 500;

/**
* The classes used to ignore changes for dirty among other features.
*/
GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES = ".filter-textbox, .ui-autocomplete-input, .paging-control input, .ignore-dirty, .select-resource-autocomplete, .select-nonzoneresource-autocomplete, .select-perforg-autocomplete";

/*
 * A list of all key codes that can't be used to edit data.
*/
GenConstants.NONEDIT_KEYS = [13, 9, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45, 144, 145,
        112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 91, 92, 93];

/**
* @class Page Manager is used for any global features of the site, for
*              example marking a page as dirty once data has changed.
* @param SessionConfig.prodURL url for production, may enable/disable some functionality if used for debuging.
* @returns {GenPageManager}
*/
function GenPageManager(SessionConfig) {
    /*
    * private variables
    */

    /**
    * @private
    */
    var dirty = [];

    /**
    * @private
    */
    var hostURL = window.location.protocol + '//' + window.location.host;

    /**
    * @private
    */
    // prodGenBoeUrl_Global is used by GenBOE and is defined there in all master pages
    var temp = 'genboe.isgs.lmco.com';
    if (typeof prodGenBoeUrl_Global !== 'undefined') {
        temp = prodGenBoeUrl_Global;
    }

    var prodURL = SessionConfig && SessionConfig.prodURL ? SessionConfig.prodURL : temp;

    /**
    * @private
    */
    var homePrefix = "default/";

    /**
    * @private
    */
    var genURL = document.location.host;

    /**
    * @private (check to see if hashchange event is supported by browser.  IE7 did not support this and therefore fails)
    */
    var isValidBrowser = Modernizr.hashchange;

    /**
    * @private
    */
    var scrollCallbacks = [];

    /**
    * @private
    */
    var hasFlash = false;

    // this code defines hasFlash but only needs to be run once.
    try {
        var flashObject = new ActiveXObject('ShockwaveFlash.ShockwaveFlash');
        if (flashObject) hasFlash = true;
    } catch (e) {
        if (navigator.mimeTypes["application/x-shockwave-flash"] != undefined) hasFlash = true;
    }


    // use jquery ui's toolkit class to fancy-up the tooltip itself
    $(document).tooltip({
        show: { effect: 'slideDown', delay: 900 },
        hide: { effect: 'slideUp', delay: 0 },
        position: { my: "left top", at: "left bottom" },
        track: false,
        content: function (callback) {
            var theTooltipText = $(this).prop('title');

            // make sure we have a valid tooltip element AND it is not just an empty string
            // also replace \r\n with html line breaks to show proper formatting where applicable
            if (undefined != theTooltipText || (theTooltipText && theTooltipText != '')) {
                theTooltipText = theTooltipText.replace(/\r\n/g, "<br/>").replace(/\n/g, "<br/>").replace(/\r/g, "<br/>");
                return theTooltipText;
            }
        }
    });


    /*
    * public variables and methods
    */

    // TODO investigate moving this into a private variable
    // Manages the registering and unregistering of events.
    this.eventBusRegistry = [];

    /**
    * Sets the home prefix name
    *@param toSet String to set Prefix too.
    */
    this.setHomePrefix = function (toSet) {
        homePrefix = toSet;
    }

    /**
    * Gets the home prefix name
    */
    this.getHomePrefix = function () {
        return homePrefix;
    }
    /**
    * prints the dirty form IDs that have been recorded. (for debug only)
    */
    this.printDirty = function () {
        var dirtyString = "";

        for (var y in dirty) {
            if (dirty.hasOwnProperty(y)) {
                if (dirty[y] == true) {
                    dirtyString += "\n" + y;
                }
            }
        }
        console.log(dirtyString);
    };

    /**
    * returns if the user has flash installed.
    */
    this.hasFlashInstalled = function () {
        return hasFlash;
    };

    /**
    * Set event to fix the title issue caused by IE and flash (# in the title)
    */
    this.setRestoreTitleEvent = function () {
        // deals w/ the #draft and #baseline in the title of the page
        // http://stackoverflow.com/questions/4562423/ie-title-changes-to-afterhash-if-the-page-has-a-url-with-and-has-flash-s
        if (document.attachEvent) {
            document.attachEvent('onpropertychange', function (evt) {
                var originalTitle = document.title.split("#")[0];

                if (evt.propertyName === 'title' && document.title !== originalTitle) {
                    setTimeout(function () {
                        document.title = originalTitle;
                    }, 1);
                }
            });
        }
    }

    // TODO refactor ID to constant
    // TODO reuse div and code here for exports, currently export code is very
    // similar to this.
    /**
    * performs a hidden download ajaxy style. (note this is very similar to the
    * Widgets export method, TODO added to revise)
    * 
    * @param url
    *            url to download from.
    * @see GenWidget#performExport
    */
    this.downloadFromURL = function (url) {
        var iframe;
        iframe = document.getElementById("hiddenDownloader");
        if (iframe === null) {
            iframe = document.createElement('iframe');
            iframe.id = "hiddenDownloader";
            iframe.style.visibility = 'hidden';
            document.body.appendChild(iframe);
        }
        iframe.src = url;
    };

    /**
    * marks a form as dirty. Also enables stateful buttons.
    * 
    * @param inFormID
    *            form ID to mark as dirty.
    */
    this.setDirty = function (inFormID) {
        dirty[inFormID] = true;

        var saveButton = findClosestStatefulButtons(inFormID);
        // if any are found then activate them.
        if ($(saveButton).length != 0) {
            $(saveButton).removeClass("disabled");
            $(saveButton).prop('disabled', false);
        }
    };

    /**
    * clears the dirty tracker on a form. Also redisabled stateful buttons.
    * 
    * @param inFormID
    *            form ID to mark as clean.
    */
    this.clearDirty = function (inFormID) {
        dirty[inFormID] = false;

        var saveButton = findClosestStatefulButtons(inFormID);
        // if any are found then deactivate them.
        if ($(saveButton).length != 0) {
            $(saveButton).addClass("disabled");
            $(saveButton).prop('disabled', true);
        }

        // TODO investigate possibly clearing all validation
    };

    // TODO modify these to have better names so that they can be used across
    // projects

    /**
    * Creates URLs for a generation tool.
    * Example: GenSession.CreateUrl({ Controller: "abc", Action: "DoStuff", Workspace: "blah", Boe: "324" }); = http://tool.com/blah/abc/DoStuff/boe/324
    * 
    * @param params
    *            collection of params.
    */
    this.CreateUrl = function (params) {
        var controller = "", action = "", shortname = "";
        var extra = [];
        var postUrl = hostURL + '/';

        for (var key in params) {
            var value = params[key];
            key = key.toLowerCase();
            switch (key) {
                case "controller":
                    controller = value;
                    break;
                case "action":
                    action = value;
                    break;
                case "workspace":
                case "capture":
                case "program":
                case "proposal":
                    shortname = value;
                    break;
                case "tool":
                    postUrl = value;
                    break;
                default:
                    extra.push({ key: key, value: value });
            }
        }

        if (shortname != "") {
            postUrl += shortname + "/";
        } else if (controller != "" || action != "") {
            postUrl += homePrefix;
        }

        if (controller != "") {
            postUrl += controller + "/";
        }

        if (action != "") {
            postUrl += action + "/";
        }

        for (var index in extra) {
            var key = extra[index].key;
            var value = extra[index].value;

            if (typeof(value) == 'string' && value.indexOf('#') == 0) {
                value = value.slice(1);
                key = '#' + key;
            }
            postUrl += key + "/";

            if (value != "") {
                postUrl += value + "/";
            }
        }

        return postUrl;
    };

    /**
    * @deprecated Replaced with GenSession.CreateUrl
    *
    * @param workspace
    * @param controllerName
    * @param actionName
    * @param params
    * @returns {String} generated url
    */
    this.CreatePostURL = function (workspace, controllerName, actionName, params) {
        var postURL = hostURL + '/' + workspace + '/' + controllerName + '/' + actionName + '/';
        if (params !== undefined) {
            postURL = postURL + params;
        }
        return postURL;
    };

    /**
    * @deprecated Replaced with GenSession.CreateUrl
    *
    * @param workspace
    * @returns {String} generated url
    */
    this.CreateNoParamsPostURL = function (workspace) {
        var postURL = hostURL + '/' + workspace;
        return postURL;
    };

    /**
  * @deprecated Replaced with GenSession.CreateUrl
  *
  * @param workspace
  * @returns {String} generated url
  */
    this.GetSiteURL = function () {
        var postURL = hostURL + '/';
        return postURL;
    };


    /**
    * @deprecated Replaced with GenSession.CreateUrl
    *
    * @param controllerName
    * @param actionName
    * @param params
    * @returns {String} generated url
    */
    this.CreateSystemAdminWithParmsPostURL = function (controllerName, actionName, params) {
        var postURL = hostURL + '/' + 'default/' + controllerName + '/' + actionName + '/' + params;
        return postURL;
    };

    /**
    * @deprecated Replaced with GenSession.CreateUrl
    *
    * @param controllerName
    * @param actionName
    * @returns {String} generated url
    */
    this.CreateSystemAdminPostURL = function (controllerName, actionName) {
        var postURL = hostURL + '/' + 'default/' + controllerName + '/' + actionName;
        return postURL;
    };

    /**
    * checks if a form is dirty. If no param is sent in it will check for any
    * dirty form.
    * 
    * @param inFormID
    *            optional param to limit dirty check to a single form.
    * @returns if dirty.
    */
    this.isDirty = function (inFormID) {
        var found = false;

        if (inFormID == undefined) {
            for (i in dirty) {
                if (dirty.hasOwnProperty(i)) {
                    if (dirty[i] == true) {
                        found = true;
                        break;
                    }
                }
            }
        } else {
            found = dirty[inFormID] != undefined && dirty[inFormID] == true;
        }

        return found;
    };

    /**
    * The supporting browser for BOE app. is MS IE version 8 and above and
    * Firefox 4.0 and later versions. NOTE: Firefox 3.6 uses Gecko 1.9.2 as the
    * rendering engine and Firefox 4.x is Gecko 2 or higher
    * 
    * @returns if the clients browser is offically supported.
    */
    this.isValidBrowser = function () {
        return isValidBrowser;
    };

    /**
    * Registers event, it is preferred that you use a widgets method of this same
    * name if applicable.
    * 
    * @param {String} type The type of event (i.e. 'click', 'MY_EVENT', 'keypress').
    * @param {String} [selector] A selector for the objects to bind
    * @param {String} [context=document] The context for the event
    * @param {function} handler The function to bind
    * @see GenWidget#on
    */
    this.on = function (type, selector, context, handler) {
        var hasSelector = true;

        if (context == null && handler == null) {
            handler = selector;
            selector = undefined;
            context = document;

            hasSelector = false;
        } else if (handler == null) {
            handler = context;
            context = document;
        }

        if (hasSelector) {
            $(context).off(type, selector);
            $(context).on(type, selector, handler);
        } else {
            $(context).off(type);
            $(context).on(type, handler);
        }
    }

    /**
    * Unregisters event, it is preferred that you use a widgets method of this same
    * name if applicable.
    * 
    * GenSession.off(type [, selector ] [, context ] [, handler(eventObject)])
    * 
    * @param {String} type The type of event (i.e. 'click', 'MY_EVENT', 'keypress').
    * @param {String} [selector] A selector for the objects to bind
    * @param {string} [context=document] The context for the event
    * @param {function} [handler] The function to bind
    * @see GenWidget#off
    */
    this.off = function (type, selector, context, handler) {
        if (context == null) {
            context = document;
        }

        if (typeof handler == 'function') {
            $(context).off(type, selector, handler);
        } else {
            $(context).off(type, selector);
        }
    }

    /**
    * @deprecated  (Replaced with GenSession.on)
    *
    * Registers event if not already bound, it is preferred that you use a
    * widgets method of this same name if applicable.
    * 
    * @param {string}
    *            eventName The name to give the event.
    * @param {string}
    *            eventOwner A owner name of the event, ensures the event only
    *            is registered once for that owner.
    * @param functionToBind
    * @see GenWidget#registerForEvent
    */
    this.registerForEvent = function (eventName, eventOwner, functionToBind) {
        if (this.eventBusRegistry[eventName + eventOwner] == null) {
            this.eventBusRegistry[eventName + eventOwner] = "BOUND";
            $(document).on(eventName, functionToBind);
        }
    };

    /**
    * @deprecated (Replaced with GenSession.on)
    *
    * Registers delegated event if not already bound, it is preferred that you
    * use a widgets method of this same name if applicable.
    * 
    * @param eventName
    *            The name to give the event.
    * @param eventOwnerSelector
    *            The jQuery Selector for the element to delegate for. Be sure
    *            to send in the selector only and not an actual object.
    * @param functionToBind
    *            The function to bind to this event.
    * @param context
    *            context of the selector
    * @see GenWidget#registerForDelegateEvent
    */
    this.registerForDelegateEvent = function (eventName, eventOwnerSelector, functionToBind, context) {
        $(context).off(eventName, eventOwnerSelector);
        $(context).on(eventName, eventOwnerSelector, functionToBind);
    };

    /**
    * @deprecated (Replaced with GenSession.off)
    *
    * Unregisters delegated event.
    * 
    * @param eventName
    *            The name to give the event.
    * @param eventOwnerSelector
    *            The jQuery Selector for the element to delegate for. Be sure
    *            to send in the selector only and not an actual object.
    * @param context
    *            context of the selector
    * @see GenWidget#unregisterForDelegateEvent
    */
    this.unregisterForDelegateEvent = function (eventName, eventOwnerSelector, context) {
        $(context).off(eventName, eventOwnerSelector);
    };

    /**
    * a common implementation of a simple dialog
    * 
    * @example
    * 
    * <pre>
    * GenSession.commonDialog(&quot;hello&quot;, &quot;looking good&quot;, [ {
    * 	buttonClass : &quot;ies&quot;,
    * 	ButtonText : &quot;OK&quot;,
    * 	ButtonName : &quot;ok-button&quot;,
    * 	callbackMethod : okCallback
    * } ], 450, closeBoxCallback, onCloseCallback);
    * </pre>
    * 
    * @param {String}
    *            title title of the dialog
    * @param {String}
    *            body the body text of the dialog
    * @param buttons
    *            and array of buttons
    * @param {Number}
    *            inWidth
    * @param {function}
    *            closeBoxCallback optional callback function to be invoked if/when the user clicks the close box
    * @param {function}
    *            onCloseCallback optional callback function to be invoked after the dialog has closed
    */
    this.commonDialog = function (title, body, buttons, inWidth, closeBoxCallback, onCloseCallback) {

        $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS).text("");

        if (buttons != null) {
            for (x in buttons) {
                if (buttons.hasOwnProperty(x)) {
                    if (typeof (buttons[x].ButtonText) === 'undefined') {
                        $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS).append('<div class="' + buttons[x].buttonClass + '"></div>');

                        // Close must occur before other callbackMethods.  If the order is reversed, you cannot trigger the display of a new dialog from within the
                        //  callback of a different dialog.
                        $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS + " ." + buttons[x].buttonClass).click(this.closeCommonDialog);
                        if (buttons[x].callbackMethod != null) {
                            $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS + " ." + buttons[x].buttonClass).click(buttons[x].callbackMethod);
                        }
                    } else {
                        var buttonName = buttons[x].ButtonName !== 'undefined' ? buttons[x].ButtonName : buttons[x].ButtonText.replace(/ /g, '');

                        $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS).append('<button class="' + buttons[x].buttonClass + '" name="' + buttonName + '" type="button">' + buttons[x].ButtonText + '</button>');

                        // Close must occur before other callbackMethods.  If the order is reversed, you cannot trigger the display of a new dialog from within the
                        //  callback of a different dialog.
                        $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS + " button[name='" + buttonName + "']").click(this.closeCommonDialog);
                        if (buttons[x].callbackMethod != null) {
                            $("#CommonDialog ." + GenConstants.BUTTON_CONTAINER_CLASS + " button[name='" + buttonName + "']").click(buttons[x].callbackMethod);
                        }
                    }
                }
            }
        }
        // TODO move ID to constant
        $("#CommonDialogBody").html(body);
        $("#CommonDialog").dialog({
            width: inWidth || 450,
            minHeight: 50,
            modal: true,
            resizable: false,
            draggable: true,
            title: title,
            beforeClose: function (evt, ui) {
                // was the close triggered by the close box?
                var triggeredByCloseBox = $(evt.currentTarget).hasClass('ui-dialog-titlebar-close');
                if (triggeredByCloseBox) {
                    if (closeBoxCallback) {
                        closeBoxCallback();
                    }
                }
            },
            close: function (event, ui) {
                if (onCloseCallback) {
                    onCloseCallback();
                }
            }
        });
        $('.ui-dialog-titlebar-close').blur();
    };

    /**
    * close common dialog.
    */
    this.closeCommonDialog = function () {
        // TODO move ID to constant
        $('#CommonDialog').dialog('close');
    };

    /**
    * an alert dialog.
    * 
    * @param title
    *            title of the dialog
    * @param alert
    *            the message for the body
    * @param okCallback
    *            optional method for the ok button.
    * @param closeBoxCallback
    *            optional callback method for when the close-box is clicked
    * @param {function}
    *            onCloseCallback optional callback function to be invoked after the dialog has closed
    */
    this.alertDialog = function (title, alert, okCallback, closeBoxCallback, onCloseCallback) {
        // TODO move class to constant
        this.commonDialog(title, alert, [{
            buttonClass: 'ies',
            ButtonText: 'OK',
            ButtonName: "ok-button",
            callbackMethod: okCallback
        }], 450, closeBoxCallback, onCloseCallback);
    };

    /**
    * a common confirm dialog.
    * 
    * @param title
    *            title of the dialog
    * @param body
    *            the message for the body
    * @param acceptCallback
    *            optional method for the yes button.
    * @param cancelCallback
    *            optional method for the no button.
    * @param closeBoxCallback
    *            optional callback method for when the close-box is clicked
    * @param {function}
    *            onCloseCallback optional callback function to be invoked after the dialog has closed
    */
    this.confirmDialog = function (title, body, acceptCallback, cancelCallback, closeBoxCallback, onCloseCallback) {
        this.commonDialog(title, body, [{
            buttonClass: "ies",
            ButtonText: 'Yes',
            ButtonName: 'yes-button',
            callbackMethod: acceptCallback
        }, {
            buttonClass: "ies",
            ButtonText: 'No',
            ButtonName: 'no-button',
            callbackMethod: cancelCallback
        }], 450, closeBoxCallback, onCloseCallback);
    };

    /**
    * A 'Proceed' dialog, with 'OK' or 'Cancel' options.
    * 
    * @param title
    *            title of the dialog
    * @param body
    *            the message for the body
    * @param proceedCallback
    *            optional method for the 'OK' button.
    * @param cancelCallback
    *            optional method for the 'Cancel' button.
    * @param closeBoxCallback
    *            optional callback method for when the close-box is clicked
    * @param {function}
    *            onCloseCallback optional callback function to be invoked after the dialog has closed
    */
    this.proceedDialog = function (title, body, proceedCallback, cancelCallback, closeBoxCallback, onCloseCallback) {
        this.commonDialog(title, body, [{
            buttonClass: 'ies',
            ButtonText: 'OK',
            ButtonName: "ok-button",
            callbackMethod: proceedCallback
        }, {
            buttonClass: 'ies',
            ButtonText: 'Cancel',
            ButtonName: "cancel-button",
            callbackMethod: cancelCallback
        }], 450, closeBoxCallback, onCloseCallback);
    };

    /**
    * Focus the page on the first visible validation box.
    * 
    * @see GenWidget#focusValidation
    */
    this.focusValidation = function () {
        var target = $('.validation-box:visible');
        if (target.length != 0) {
            $('html, body').animate({
                scrollTop: $(target).offset().top
            }, 200);
        }
    };

    /**
    * Displays Exception Dialog.
    * 
    * @param error
    *            to display.
    */
    this.DisplayExceptionDialog = function (error) {
        $("#ErrorText").html(error.Message.replace(/\n/g, '<br/>'));
        $("#ErrorDetails").html(error.Details.replace(/\n/g, '<br/>')).toggle(false); // hide
        // by
        // default

        // only show this if non prod
        // TODO modify this code to take in a configureable match.
        if (genURL.match == prodURL) {
            try {
                $("#ToggleErrorDetails").hide();
            } catch (err) {
            }
        }

        $("#ExceptionDialog").dialog({
            width: 400,
            modal: true,
            resizable: true,
            draggable: true,
            title: error.Title,
            zIndex: 2000
        });
    };

    /**
    * Closes exception dialog.
    */
    this.CloseExceptionDialog = function () {
        $('#ExceptionDialog').dialog('close');
    };

    /**
    * Displays text in notification area.
    * 
    * @param text
    *            to be displayed in notification area.
    */
    this.RaiseNotification = function (text) {
        $(document).trigger("DISPLAY_NOTIFICATION", text);
    };

    /**
    * Closes notification area.
    */
    this.CloseNotification = function () {
        $(document).trigger("CLOSE_NOTIFICATION");
    };

    /**
    * Shows full page loading indicator.
    */
    this.ShowLoadingBox = function () {
        $(document).trigger("SHOW_LOADING_BOX");
    };

    /**
    * Hides full page loading indicator.
    */
    this.HideLoadingBox = function () {
        $(document).trigger("HIDE_LOADING_BOX");
    };

    /**
    * Set the url for the Context Sensitive Help.
    * 
    * @param {String}
    *            url the url to go to.
    */
    this.setContextSensitiveHelp = function (url) {
        $("#PageHelpMenu a").attr("href", url);
    };

    /*
     * Default "select" event handler for datepickers.
     *
     * Note: This is defined here so it can still be invoked if the onSelect parameter is redefined by the developer.
     * 
     * @param {String}
     *          dateText the selected date
     * @param {jQuery datepicker}
     *          inst the datepicker instance
     * @param {jQuery object}
     *          element the input element (as jQuery object) the datepicker was created against
     */
    this.OnDatepickerSelect = function (dateText, inst, element) {
        var input = (element == undefined) ? $(inst.input) : element;
        input.keydown();
        input.blur();
        input.siblings('.default-text').addClass('display-none');
        input.addClass('active');
        if (dateText !== inst.lastVal) {
            input.change();
        }
    };

    this.evalPlaceHolderSettings = function () {
        var input = $(this);
        var placeHolderAtt = input.attr('placeholder');
        if (input.val() == '' || input.val() == placeHolderAtt) {
            input.removeClass('not-placeholder-value');
            input.val(placeHolderAtt);
        } else {
            input.addClass('not-placeholder-value');
        }
    };

    /*
    * Private Methods
    */

    /**
    * @private This method will implment placeholder functionality for the entire site.
    */
    function applyDefaultTextExtentionFunctionality() {
        $(document).on("focus", 'input[placeholder]', function (e) {
            var input = $(this);
            GenSession.evalPlaceHolderSettings.call(this);
            // If the value has the same data as the placeholder text
            if (input.val() == input.attr('placeholder')) {
                input.val('');
                input.addClass('not-placeholder-value');
            }
            input.blur(GenSession.evalPlaceHolderSettings);
        });
    }

    /*
    * @private this adds some jquery extensions
    */
    function applyJQueryExtensions() {
        /** 
         * determines if any part of an element is within the user's visible window
         *
         * @returns true if element is within the current screen positions
         */
        $.fn.isOnScreen = function () {
            var windowTop = $(window).scrollTop();
            var windowBottom = windowTop + $(window).height();

            var elementTop = $(this).offset().top;
            var elementBottom = elementTop + $(this).height();

            return ((windowTop < elementTop && elementTop < windowBottom) || (windowTop < elementBottom && elementBottom < windowBottom))
        };

        /** 
         * Queues a function to run the first time a user scrolls to a position that makes it visible
         *
         * @ param callback
         *          function to call
         * @returns true if element is within the current screen positions
         */
        $.fn.onScrollTo = function (callback) {
            // call it if it's on screen
            if (this.isOnScreen()) {
                $.proxy(callback, this)();
            } else {
                scrollCallbacks.push({ callback: callback, element: this});
            }

            // for chaining
            return this;
        };

        var scrollTimeout;

        $(window).scroll(function () {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(function () {
                // foreach callback function
                var indexToRemove = [];
                for (var i = 0; i < scrollCallbacks.length; i++) {
                    // if it moved from off screen to on screen
                    if (scrollCallbacks[i].element.isOnScreen()) {
                        // set visiblility to true
                        indexToRemove.push(i);

                        // call the callback function, with 'this' set to the element
                        $.proxy(scrollCallbacks[i].callback, scrollCallbacks[i].element)();
                    }
                }

                // remove items that have been called
                for (var i = indexToRemove.length - 1; i >= 0; i--) {
                    scrollCallbacks.splice(i, 1);
                }
            }, 200);
        });

        var timeouts = {};

        /** 
         * Queues a function to run after a set period of time.  Matches javascript's setTimeout
         *
         * @ param callback
         *          function to call
         * @ param ms
         *          amount of time to wait
         * @returns id of the timeout
         */
        $.setTimeout = function (callback, ms) {
            var id = setTimeout(function () {
                delete timeouts[id];
                callback();
            }, ms);
            timeouts[id] = callback;
            return id;
        };

        /** 
         * Immediately runs a function that is waiting for a timeout
         *
         * @ param id
         *          id of the timeout
         */
        $.runTimeout = function (id) {
            var callback = timeouts[id];
            if (callback) {
                $.clearTimeout(id);
                callback();
            }
        };

        /**
             * Clears a timeout (causing it not to run). Matches of javascript's clearTimeout
             *
             * @ param id
             *          id of the timeout
             */
        $.clearTimeout = function (id) {
            clearTimeout(id);
            delete timeouts[id];
        };
    }

    /**
    * @private This method will find the closet set of stateful buttons.
    * @param elementID
    *            the ID of the element to start at.
    * @returns {object[]} jQuery Array of closest stateful buttons.
    */
    function findClosestStatefulButtons(elementID) {
        // assume the save buttons are within the form.
        var saveButtons = $("#" + elementID + " ." + GenConstants.BUTTON_CONTAINER_CLASS + " .stateful_button");

        // if not in the first form see if you can find one in any parent divs.
        if ($(saveButtons).length == 0) {
            $("#" + elementID).parents('div, form').each(function () {
                var buttons = $(this).find("." + GenConstants.BUTTON_CONTAINER_CLASS + " .stateful_button");
                if (buttons.length != 0) {
                    saveButtons = buttons;
                    return false;
                }
            });
        }

        return saveButtons;
    }

    applyDefaultTextExtentionFunctionality();
    applyJQueryExtensions();
};

/**
* Calculates the difference in months between two dates.
* 
* @param startDateString
*            Start Date String
* @param endDateString
*            End Date String
* @returns Numeric value for the month difference.
*/
GenPageManager.prototype.MonthDifference = function (startDateString, endDateString) {
    var date1 = this.CreateDate(startDateString);
    var date2 = this.CreateDate(endDateString);

    var result = (date2.getFullYear() - date1.getFullYear()) * 12 + date2.getMonth() - date1.getMonth();

    return result;
};

/**
* Returns a date object for a string
* 
* @param dateString
*            Date String in format "m/d/yyyy" or "m/yyyy"
* @returns Date Object
*/
GenPageManager.prototype.CreateDate = function(dateString)
{
    var dateArray = dateString.split('/');
    var year, month, day;

    if (dateArray.length == 3) {
        year = dateArray[2];
        month = dateArray[0] - 1;
        day = dateArray[1];
    } else if (dateArray.length == 2) {
        year = dateArray[1];
        month = dateArray[0] - 1;
        day = '01';
    }

    return new Date(year, month, day);
}

/*
* GENWIDGET
*/

/**
* Most basic type of widget ideally used for simple form based modules. Only
* define the object in global scope, the rest should be configured inside of
* the onload scope.
* 
* @constructor
* @example
* 
* <pre>
* var myObject;
* 
*  $(function{}(
* 
*    var widgetConfig = {};
* 		widgetConfig.ContextID = &quot;testWidget1&quot;;
* 		widgetConfig.IsModule = true;
* 		widgetConfig.DialogConfigs = [];
* 		widgetConfig.DialogConfigs.push({
* 		    ElementID: &quot;thisDialog&quot;,
* 		    Params: {
* 		        width: 500,
* 		        title: &quot;this title&quot;,
* 		        height: 'auto',
* 		        modal: true,
* 		        resizable: false,
* 		        draggable: true,
* 		        closeOnEscape: false
* 		    }
* 		});
* 		widgetConfig.FormConfigs = [];
* 		widgetConfig.FormConfigs.push({
* 		    ElementID: &quot;testwidgetform&quot;,
* 		    Buttons: [{
* 		        ButtonClass: 'ies-action',
*               ButtonText: 'Save',
*               ButtonName: &quot;save-button&quot;,
* 		        Stateful: true,
* 		        ButtonAction: function (buttonPressed) {
* 		            TestWidget.saveRequest(
* 				    {
* 					    url: &quot;SimpleReturn&quot;,
* 					    success: function () {
*                             alert(&quot;SUCCESS: notice spinny is active.&quot;) 
*                         };
* 				    }, buttonPressed);
* 		        }
* 		    }],
* 		    AckMessages : [
*               {
*                   show: true,
*                   MessageId: &quot;MyFirstWarning&quot;,
*                   Message: &quot;This is the first message.&quot;,
*                   ContainerClass: &quot;ack-message-box&quot;,
*                   MessageClass: &quot;ack-message-text&quot;,
*                   ButtonClass: &quot;small-close-button&quot;,
*                   OnAcknowledge: function (messageId, element) {
*                   }
*               },
*               {
*                   show: false,
*                   MessageId: &quot;MySecondWarning&quot;,
*                   Message: &quot;This is the second message.&quot;,
*                   ButtonClass: &quot;small-close-button&quot;,
*                   OnAcknowledge: function (messageId, element) {
*                   }
*               }
*           ]
* 		});
* 
*    myObject = new GenWidget(widgetConfig);
*  ));
* </pre>
* 
* @param inConfig
* @param inConfig.ContextID
*            The ID of the div that represents your Widget
* @param inConfig.IsModule
*            Whether your widget should get the module treatment
* @param inConfig.Module
             Optional - Allows you to set the element with this ID as the module if the widget name is an outer div.
* @param inConfig.DialogConfigs
*            Array of dialog configurations for this widget. (See GenDialog for
*            more information)
* @param inConfig.FormConfigs
*            Array of form configurations for this widget. (Note: If a form
*            element is missing from the configuration it will still be added
*            to the forms member variable although it will just have default
*            configurations. See the GenForm class for more information.)
* @param inConfig.PagingData
*            configuration for paging of widget. (See PagingData section for
*            more information)
* @param inConfig.ReadOnly
*            A Boolean representing if the default ReadOnly method should be
*            run.
* @param inConfig.ApplyHelpPopouts
*           True if you would like the apply the help popouts
* @param inConfig.ApplyDatePickers
*           True to apply the date pickers setup
* @param inConfig.ApplyTextAreaLimits
*           True to apply text area limits setup
* @param inConfig.ApplyDefaultText
*           True to apply default text setup
* @param inConfig.ApplyLiveValidation
*           True to apply live validation
* @requires GenSession
* @returns {GenWidget}
*/

function GenWidget(inConfig) {

    /***************************************************************************
    * private variables
    **************************************************************************/

    /**
    * @private
    */
    var that = this;

    /**
    * @private
    */
    var isModule = false;

    /**
    * @private
    */
    var config = inConfig;

    /**
    * @private
    */
    var formConfigs;

    /**
    * @private
    */
    var importedData;

    /***************************************************************************
    * public variables
    **************************************************************************/

    /**
    * optional data container for a widgets data. For all except a Grid widget
    * data can be more easily retrieved from a forms getData method.
    * 
    * @see GenForm#getData
    */
    this.data = {};

    /**
    * All the GenForms that are contained with in the Widget.
    * 
    * @see GenForm
    */
    this.forms = [];

    /**
    * All the GenDialogs that are contained with in the Widget. NOTE: Dialogs
    * are moved outside the widget once initialized. Use getDialog to retreive
    * the widget's dialogs.
    * 
    * @see GenDialog
    */
    this.dialogs = [];

    /**
    * The name of the widget, this should be the div id that contains all
    * widget elements. If it is not you won't have a good day.
    */
    this.widgetName = null;

    /**
    * The ID of the module if it contains one.  Defaults to widgetName.
    */
    this.module = null;

    /**
    * the jquery element that is this widget.
    */
    this.context = null;

    /**
    * readonly setting.
    */
    this.readOnly = false;

    this.isFixedWidth = true;

    /**
    * This is used for protected type methods,  basically methods only inherited classes would use directly.
    */
    this.utils = {

        applyDeleteableRows: function (listMenu) {
            if (!that.readOnly) {
                $("." + GenConstants.BUTTON_CONTAINER_CLASS, listMenu).prepend('<button name="delete-button" type="button" class="ies disabled">Delete</button>');
                this.applyDeleteColToTable();

                var setDeleteButtonState = function () {
                    if (that.getElement('input[name=toDelete]:checked').length > 0) {
                        $("button[name='delete-button'].disabled", listMenu).removeClass('disabled');
                    } else {
                        $("button[name='delete-button']", listMenu).addClass('disabled');
                    }
                };

                that.registerForDelegateEvent('click', ".widget-menu button[name='delete-button']:not(.disabled)",
					    function () {
					        //perform actions
					        if ($.isFunction(config.deleteable)) {
					            config.deleteable()
					        }
					        // uncheck delete all
					        that.getElement('input[name=deleteAll]').prop("checked", false);
					        // setTheDelete button state
					        setDeleteButtonState();
					    }, "#" + that.widgetName);

                that.registerForDelegateEvent("click", "table thead th input[name=deleteAll]", function () {
                    that.getElement('table tbody tr:not(.blank) input[name=toDelete]').attr('checked', $(this).is(":checked"));
                }, "#" + that.widgetName);


                that.registerForDelegateEvent("click",
					    "table tbody td input[name=toDelete], table thead tr input[name=deleteAll]", setDeleteButtonState, "#" + that.widgetName);
            }
            else {
                that.getElement('colgroup col:first-child').remove();
            }
        },

        /**
        * Applies the deleteable col to a table. Currently exposed, Public lets non paging data grids create del column rows.
        */
        applyDeleteColToTable: function () {
            if (!that.readOnly) {
                that.getElement("thead:first tr:last").prepend(
					    "<th class='delete-checkbox'><input type='checkbox' class='ignore-dirty' name='deleteAll'></th>");

                that.getElement("thead:first tr:not(:last)").prepend(
					    "<th></th>");

                // apply to all rows with a pkid that are not inuse.
                that.getElement("tbody:first tr[pkid]:not(.inUse)").prepend(
				        "<td class='delete-checkbox'><input type='checkbox' class='ignore-dirty' name='toDelete'></td>");
                // apply to all rows that are in use.
                that.getElement("tbody:first tr.inUse").prepend("<td>In Use</td>");
            }
        }
    }

    /**
    * This method needs to be called when an import preview view is rendered.
    * 
    * @param inImportedData
    *            data usually Json to be set from the import preview's onLoad
    *            method.
    */
    this.setImportData = function (inImportedData) {
        that.importedData = inImportedData;
    };

    /**
    * This method should be used to gather import data that was retrived durign
    * an import preview with the intention of submitting this data to a import
    * completion request.
    * 
    * @returns the data set from import data.
    */
    this.getImportData = function () {
        return that.importedData;
    };

    this.setIsFixedWidth = function(inIsFixedWidth)
    {
        that.isFixedWidth = inIsFixedWidth;
    }

    this.getIsFixedWidth = function ()
    {
        return that.isFixedWidth;
    }

    /**
    * Returns the config that was passed in. Use this for debug only.
    * 
    * @returns the config of the widget
    */
    this.printConfig = function () {
        return config;
    };

    /**
    * exposes the ability to mark a form as dirty. this normally should not be
    * called unless you have chosen to manually wire up your dirty data.
    * 
    * @param: inFormID the ID of the form you wish to dirty.
    */
    this.setDirty = function (inFormID) {
        if (inFormID == null || inFormID.type != undefined)
            inFormID = that.widgetName;
        $(document).trigger('DATA_DIRTY', inFormID);
    };

    /**
    * method provided to retrive a form object from the widget
    * 
    * @param inFormID
    *            the ID of the form to retreive.
    */
    this.getForm = function (inFormID) {
        for (var n in that.forms) {
            if (that.forms.hasOwnProperty(n)) {
                var form = that.forms[n];
                if (form.formID == inFormID) {
                    return form;
                }
            }
        }
        return null;
    };

    /**
    * Closes all open dialogs on a form
    */
    this.closeAllDialogs = function () {
        for (var n in that.dialogs) {
            if (that.dialogs.hasOwnProperty(n)) {
                var dialog = that.dialogs[n];
                dialog.closeDialog();
            }
        }
    };

    /**
    * Retreives a GenDialog
    * 
    * @param {string}
    *            inDialogID The dialog ID
    * @returns the GenDialog object
    */
    this.getDialog = function (inDialogID) {
        for (var n in that.dialogs) {
            if (that.dialogs.hasOwnProperty(n)) {
                var dialog = that.dialogs[n];
                if (dialog.dialogID == inDialogID) {
                    return dialog;
                }
            }
        }
        return null;
    };

    /**
    * Cleans the dirty bit assciated with the widget.
    * 
    * @param inFormID
    *            optional If you only want to clean a single form you can give
    *            it a formID.
    */
    this.cleanDirty = function (inFormID) {
        if (inFormID == null) {
            inFormID = that.widgetName;

            for (var n in that.forms) {
                if (that.forms.hasOwnProperty(n)) {
                    $(document).trigger('DATA_CLEANED', that.forms[n].formID);
                }
            }
        }

        $(document).trigger('DATA_CLEANED', inFormID);

        that.refreshModule();
    };

    /**
    *Retrive an element that belongs to a Widget. 
    *This is recommended over a direct jquery selector since it enforces scoping the query to within the widget.
    *@param Selector the string selector to get
    *
    *@returns the element from inside the widget
    */
    this.getElement = function (Selector) {
        return $(Selector, that.context);
    };

    /**
    * checks if any form in the widget is dirty. 
    * 
    * @returns if dirty.
    */
    this.isDirty = function () {
        var thisIsDirty = false;

        for (var n in that.forms) {
            if (that.forms.hasOwnProperty(n)) {
                if (GenSession.isDirty(that.forms[n].formID)) {
                    thisIsDirty = true;
                }
            }
        }

        if (GenSession.isDirty(that.widgetName)) {
            thisIsDirty = true;
        }
        return thisIsDirty;
    };

    /**
    * Handle parent-module refesh event
    *
    * @param evt the event object
    * @param parentModuleId the widget id for the parent module
    */
    this.handleRefreshParentModule = function (evt, parentModuleId) {
        if (that.widgetName == parentModuleId) {
            that.refreshModule(true);  // continue UP the DOM so all parent modules are refreshed
        }
    };

    /**
    * Setup a [?], clicking on which will display a help dialog with text populated by attribute of helptext
    */
    this.applyHelpPopouts = function (context) {

        if (typeof context === 'undefined') {
            context = that.context;
        }

        var helpfulElements = $("[" + GenConstants.HELPTEXT_ATTRIBUTE + "]", context);

        helpfulElements.each(function () {
            var helptext = $(this).attr(GenConstants.HELPTEXT_ATTRIBUTE);
            $(this).after(
					'<div class="help-icon"></div>'
							+ '<!-- This comment is needed for the jquery animation to work in IE8... -->'
							+ '<div class="help-dialog" style="max-width: 275px;">'
							+ '<div class="help-dialog-text">' + helptext
							+ '</div>' + '</div>');
        });

        // help features
        $('.help-icon', context).each(function () {
            var icon = $(this);
            var dialog = $(this).next();
            var timeoutId = undefined;  // Note: timers are only for HIDING the help text
            var showDelayId = undefined;
            var isDisplayed = false;  // Note: used to prevent concurrent access to the toggleHelp function

            $(icon).mouseenter(function () {
                clearTimeout(timeoutId);  // cancel hiding
                showDelayId = setTimeout(function () {
                    if (!isDisplayed) {
                        isDisplayed = true;
                        toggleHelp(icon, true, undefined);
                    }
                }, GenConstants.HELPTEXT_SHOW_DELAY);
            });

            $(icon).mouseleave(function () {
                clearTimeout(showDelayId);
                if (isDisplayed) {
                    timeoutId = setTimeout(function () {
                        if (isDisplayed) {
                            isDisplayed = false;
                            toggleHelp(icon, false, undefined);
                        }
                    }, GenConstants.HELPTEXT_HIDE_DELAY);
                }
            });

            $(dialog).mouseenter(function () {
                clearTimeout(timeoutId);  // cancel hiding
                if (!isDisplayed) {
                    isDisplayed = true;
                    toggleHelp(icon, true, undefined);
                }
            });

            $(dialog).mouseleave(function () {
                if (isDisplayed) {
                    timeoutId = setTimeout(function () {
                        if (isDisplayed) {
                            isDisplayed = false;
                            toggleHelp(icon, false, undefined);
                        }
                    }, GenConstants.HELPTEXT_HIDE_DELAY);
                }
            });
        });
    }

    /**
    * Adds a form to the widget, allowing you to create forms after the widgets been initialized.
    * 
    * @param formConfigs - can be a single config or an Array.
    */
    this.addForm = function (formConfigs) {
        if (formConfigs != null && $.isArray(formConfigs)) {
            var formConfigLength = formConfigs.length;
            for (var x = 0; x < formConfigLength; x++) {
                that.forms.push(new GenForm(formConfigs[x]));
            }
        } else if (formConfigs != null) {
            that.forms.push(new GenForm(formConfigs));
        }
    };

    /**
    * Adds a dialog to the widget, allowing you to create dialogs after the widgets been initialized.
    * 
    * @param dialogConfigs - can be a single config or an Array.
    */
    this.addDialog = function (dialogConfigs) {
        if (dialogConfigs != null && $.isArray(dialogConfigs)) {
            var DialogConfigLength = DialogConfigs.length;
            for (var x = 0; x < DialogConfigLength; x++) {
                that.dialogs.push(new GenDialog(DialogConfigs[x]));
            }
        } else if (dialogConfigs != null) {
            that.dialogs.push(new GenDialog(dialogConfigs));
        }
    };

    /**
    * Shows an acknowledgeable message.
    *
    * @param messageId - Unique identifier for the message
    */
    this.showAckMessage = function (messageId) {
        for (var n in that.forms) {
            if (that.forms.hasOwnProperty(n)) {
                that.forms[n].showAckMessage(messageId);
            }
        }
    };

    /***************************************************************************
    * private methods
    **************************************************************************/
    /**
    * @private
    */
    function initialize() {

        aquireForms();

        applyTooltip();

        // TODO see about doing these in the form object
        if (typeof (inConfig.ApplyHelpPopouts) === 'undefined' || inConfig.ApplyHelpPopouts) {
            that.applyHelpPopouts();
        }
        if (typeof (inConfig.ApplyDatePickers) === 'undefined' || inConfig.ApplyDatePickers) {
            applyDatePickers();
        }
        if (typeof (inConfig.ApplyTextAreaLimits) === 'undefined' || inConfig.ApplyTextAreaLimits) {
            applyTextAreaLimits();
        }
        if (typeof (inConfig.ApplyDefaultText) === 'undefined' || inConfig.ApplyDefaultText) {
            applyDefaultText();
        }
        if (typeof (inConfig.ApplyLiveValidation) === 'undefined' || inConfig.ApplyLiveValidation) {
            applyLiveValidation();
        }
        if (typeof (inConfig.IsFixedWidth) != 'undefined' || inConfig.IsFixedWidth) {
            that.setIsFixedWidth(inConfig.IsFixedWidth);
        }

        if (that.readOnly) {
            applyReadOnly();
        }

        // perform as late as possible since dialogs modify the dom and the
        // moves elements outside of the widget's id context.
        if (inConfig.DialogConfigs != null) {
            aquireDialogs(inConfig.DialogConfigs);
        }

        // performed last since these calculate height and dialogs must be
        // removed to adjust correctly.
        if (isModule) {
            createModule();
            if ($("div.module-content-data.expanded-content", that.context).length > 0) {
                collapsibleModule();
            }
            that.refreshModule();
        }
    }

    /**
    * @private
    */
    function aquireForms() {
        // grab all configured forms
        if (formConfigs != null) {
            var formConfigLength = formConfigs.length;
            for (var x = 0; x < formConfigLength; x++) {
                that.forms.push(new GenForm(formConfigs[x], that.context));
            }
        }

        // additionally grab any unconfigured form.
        $("form", that.context).each(function () {
            if ($(this).attr('id') != undefined && $(this).attr('id') != "") {
                var found = false;
                var formsLength = that.forms.length;
                for (var x = 0; x < formsLength; x++) {
                    if (that.forms[x].formID == $(this).attr('id')) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    that.forms.push(new GenForm({
                        ElementID: $(this).attr('id')
                    }, that.context));
                }
            }
        });
    }

    /**
    * @param DialogConfigs
    * @private
    */
    function aquireDialogs(DialogConfigs) {
        var DialogConfigLength = DialogConfigs.length;
        for (var x = 0; x < DialogConfigLength; x++) {
            that.dialogs.push(new GenDialog(DialogConfigs[x]));
        }
    }

    /**
    * @private
    */
    function applyReadOnly() {
        // If this widget is read-only, we need to replace inputs with DIVS and
        // hide buttons
        if (that.isReadOnly()) {
            $(that.context).addClass(GenConstants.READ_ONLY_WIDGET_MARKER_CLASS);

            // Get all inputs, textareas and selects within the Widget's
            // container
            var widgetFields = that.context.find('input, textarea, select');

            // Iterate over each of the fields
            widgetFields.each(function (index) {
                var currentField = $(this);
                if (currentField.attr("type") != "hidden" && currentField.attr("type") != "radio"
						&& currentField.css("display") != "none" && currentField.css("visibility") != "hidden"
						&& !currentField.hasClass("display-none") && !currentField.hasClass("filter-textbox")
                        && !currentField.hasClass("skip-read-only")
						&& !currentField.hasClass("filter-dropdown")) {
                    var replacementFieldText = "";

                    if (currentField.get(0).tagName == "SELECT") {
                        replacementFieldText = GenHelper.htmlEncode($.trim(currentField.children("option:selected").text()));
                    } else if (currentField.is(':checkbox')) {
                    } else {
                        // Convert line breaks (\n) to <br/> and encode special xml characters, e.g. <, >, etc. to &lt;, &gt;, etc. 
                        replacementFieldText = GenHelper.htmlEncode(currentField.val());
                        // Decode &lt;, &gt;, etc. back to <, >, etc.
                        replacementFieldText = GenHelper.htmlDecode(replacementFieldText);
                    }

                    replacementFieldText = $.trim(replacementFieldText);

                    // Create read-only <div> element
                    var newElement = $("<div/>", {
                        "class": "replacedWidgetText",
                        title: replacementFieldText,
                        html: replacementFieldText
                    });

                    currentField.after(newElement);
                }

                if (currentField.attr("type") != "hidden" && !currentField.hasClass("filter-textbox")
                    && !currentField.hasClass("filter-dropdown") && !currentField.hasClass("skip-read-only")) {
                    currentField.addClass('display-none');
                }

                if (currentField.attr("type") == "radio") {
                    currentField.removeClass('display-none');
                    currentField.prop('disabled', true);
                }
            });

            hideButtons();

            // Hide grid delete buttons
            that.context.find('td .delete').addClass('display-none');
            that.context.find('th .delete').addClass('display-none');
            that.context.find('tr.blank').addClass('display-none');
            var headers = that.context.find('th:visible');
            $(headers[headers.length - 1]).addClass('last-child');

            // Remove grid events
            that.context.find('tbody td').unbind('click');
            that.context.find('tbody td a').unbind('click');
        }
    }

    /**
    * Searches for any elements that have the class 'auto-tooltip' and creates tooltip on them.
    * Useful for grid cells that have overflow set to ellipses so see the entire cell value.
    */
    function applyTooltip() {
        // setup first column cells to have hovertext of long text string in 
        // case things overflowed and the ellipses is shown in the cell
        var elements = $(".auto-tooltip", that.context);

        // iterate over element passed in and set 'title' attribute to be the full value of the element
        $.each(elements, function () {
            var value = $(this).text();
            $(this).attr('title', value);
        })
    };

    /**
    * @private
    */
    function applyDatePickers() {
        var datepickers = $("input." + GenConstants.DATEPICKER_CLASS, that.context);

        datepickers.each(function () {
            datepickers.datepicker({ buttonImage: null });
        });
    }



    /**
    * @private
    */
    function applyDefaultText() {
        // this needs to run for any prepopulated inputs in the widget.
        $('input[placeholder]', that.context).each(function () { GenSession.evalPlaceHolderSettings.call(this) });

        // this appears to be here to support the submit acition on forms which we don't really use
        // THis can potentially be removed.
        $('#' + that.widgetName + ' input[placeholder]').parents('form').submit(function () {
            var thisForm = this;
            $(thisForm).find('[placeholder]:not(.not-placeholder-value)').each(function () {
                var input = $(this);
                if (input.val() == input.attr('placeholder')) {
                    input.val('');
                }
            });
        });
    }

    /**
    * @private
    */
    function applyTextAreaLimits() {
        var limits = $("textarea[maxlength]", that.context);

        $(limits).each(function () {
            $(this).keyup(function () {
                textAreaLimit(this, $(this).attr("maxlength"));
            });
        });
    }

    //TODO I don't think this will work automatically.  you can't store a json object in a attr like this easily. investigate.

    /**
    * @private
    */
    function applyLiveValidation() {
        var liveElements = $(":input[livevalidation]", that.context);

        liveElements.each(function () {
            var options = $(this).attr('livevalidation');
            that.liveValidate(this, options);
        });
    }

    //TODO add buttons to a shared array so you make sure you show what you hide. Also add in the calendar button to this.

    /**
    * @private
    */
    function hideButtons() {
        // Hide buttons
        $("." + GenConstants.BUTTON_CONTAINER_CLASS, that.context).addClass('display-none');
        $(".buttons-left", that.context).addClass('display-none');
        $(".buttons-right", that.context).addClass('display-none');
        $('.ui-datepicker-trigger', that.context).addClass('display-none');
        $('.small-delete-button.remove-all.button', that.context).addClass('display-none');
        $('.small-delete-button.remove-one.button', that.context).addClass('display-none');
        that.refreshModule();
    }

    /**
    * @private
    */
    function showButtons() {
        // Show buttons
        $("." + GenConstants.BUTTON_CONTAINER_CLASS, that.context).removeClass('display-none');
        $(".buttons-left", that.context).removeClass('display-none');
        $(".buttons-right", that.context).removeClass('display-none');
        $(".ui-datepicker-trigger", that.context).removeClass('display-none');
        that.refreshModule();
    }

    /**
    * @private
    */
    function textAreaLimit(field, maxlen) {
        if (field.value.length > maxlen)
            field.value = field.value.substring(0, maxlen);
    }

    /**
    * @private
    */
    function createModule() {
        var module = $("#" + that.module);

        // identify a module using a general class name (used for parent refresh)
        $(module).removeClass('module-root').addClass('module-root');

        // content
        var contentDataBlocks = $(module).find('.module-content-data');

        $(contentDataBlocks).find("script").remove().end().wrap('<div class="module-content"><div class="module-content-center"></div></div>');

        var contentContainerBlocks = $(module).find(".module-content");

        $(contentContainerBlocks).append('<div class="module-content-right"></div>');
        $(contentContainerBlocks).prepend('<div class="module-content-left"></div>');
        $(contentContainerBlocks).each(
				function () {
				    $(this).attr(
							'class',
							$(this).find(".module-content-data").attr('class').replace('module-content-data',
									'module-content'));
				});

        // create headers
        $(module).find('.module-header-data').wrap(
				'<div class="module-header"><div class="module-header-center"></div></div>');
        $(module).find(".module-header").append('<div class="module-header-right"></div>');
        $(module).find(".module-header").prepend('<div class="module-header-left"></div>');

        // footer
        $(module)
				.append(
						'<div class="module-footer"><div class="module-footer-left">'
								+ '</div><div class="module-footer-center"></div><div class="module-footer-right"></div></div>');

        // register for parent-refresh event from (each) child
        that.registerForEvent('REFRESH_PARENT_MODULE', that.handleRefreshParentModule);
    }

    /**
    * @private
    */
    function collapsibleModule() {
        var module = $("#" + that.module);
        $(module).find('.module-header-data').prepend('<div class="collapsible-header">');

        if (!$(module).hasClass('collapsed') && !$(module).hasClass('expanded')) {
            $(module).addClass('expanded');
        }

        if ($(module).hasClass('collapsed')) {
            $(module).find('.collapsible-header').gentoggle(function () {
                $(module).removeClass('collapsed').addClass('expanded');
                $(module).find('.expanded-content').slideDown(400, function () {
                    $(document).trigger(that.module + '_EXPANDED');
                });
            }, function () {
                $(module).removeClass('expanded').addClass('collapsed');
                $(module).find('.expanded-content').slideUp(400, function () {
                    $(document).trigger(that.module + '_COLLAPSED');
                });
            });

            $(module).find('.expanded-content').hide();
        } else {
            $(module).find('.collapsible-header').gentoggle(function () {
                $(module).removeClass('expanded').addClass('collapsed');
                $(module).find('.expanded-content').slideUp(400, function () {
                    $(document).trigger(that.module + '_COLLAPSED');
                });
            }, function () {
                $(module).removeClass('collapsed').addClass('expanded');
                $(module).find('.expanded-content').slideDown(400, function () {
                    $(document).trigger(that.module + '_EXPANDED');
                });
            });
        }
    }

    /**
    * @private
    * @param helpButton
    *            the button to tie help dialog too.
    * @param show
    *            true to show; false to hide
    *            Note: Must use explicit show/hide instead of toggle to avoid event-related timing issues
    * @param side
    *            the side to align to
    * @param callbackHandler
    *            callback function handler (optional)
    */
    function toggleHelp(helpButton, show, side, callbackHandler) {
        var helpDialog = $(helpButton).next();

        if (($(helpButton).position().top - helpDialog.height()) < 0) {
            helpDialog.css("top", 0);
        } else {
            helpDialog.css("top", $(helpButton).position().top - helpDialog.height());
        }

        if (side != undefined && side.toLowerCase() == 'left') {
            helpDialog.css("left", $(helpButton).position().left - helpDialog.width() - 20);
        } else {
            helpDialog.css("left", $(helpButton).position().left + $(helpButton).width() + 5);
        }

        if (show) {
            helpDialog.show('fast', callbackHandler);
        } else {
            helpDialog.hide('fast', callbackHandler);
        }
    }

    if (inConfig != null) {

        formConfigs = inConfig.FormConfigs;

        if (inConfig.ContextID == null) {
            throw {
                name: 'ArgumentMissing',
                message: 'ContextID is required.'
            };
        } else {
            this.widgetName = inConfig.ContextID;
            this.context = $("#" + inConfig.ContextID);
        }

        if (inConfig.isReadOnly != null) {
            this.readOnly = inConfig.isReadOnly;
        }
        if (inConfig.IsModule != null) {
            isModule = inConfig.IsModule;
            this.module = this.widgetName;
        }
        if (inConfig.Module != null) {
            this.module = inConfig.Module;
            isModule = true;
        }

        initialize();
    }
}

/**
* Submits a form for upload
* 
* @param importPreviewURL
*            Url to send request to for import.
* @param importFormSelector
*            Form that contains data to submit.
* @param onSuccess
*            method to run on success.
* @param buttonPressed
*            button pressed to perform import.
* @param requiredElements
*            Optional array of required fields
*/
GenWidget.prototype.performImportPreview = function (importPreviewURL, importFormSelector, onSuccess, buttonPressed,
		requiredElements) {
    // Remove the old hidden iFrame, if it exists
    $('iframe#ImportTarget').remove();

    // Create a new hidden iFrame and set it's source to the chosen report's URL
    var targetIFrame = $('<iframe />', {
        'id': 'ImportTarget',
        'name': 'ImportTarget',
        'class': 'display-none'
    });

    targetIFrame.appendTo('body');

    var form = $(importFormSelector);

    var missingElementValues = false;
    $(requiredElements).each(function () {
        if ($(this).val().length == 0) {
            missingElementValues = true;
            return;
        }
    });

    if (!missingElementValues) {
        if (form.attr("action") == undefined || form.attr("action") == "") {
            form.attr("action", importPreviewURL);
            form.attr("target", "ImportTarget");
            form.attr("method", "post");
            form.attr("enctype", "multipart/form-data");
            form.attr("encoding", "multipart/form-data");
        }

        $(buttonPressed).addClass("loader");
        $('iframe#ImportTarget').load(function () {
            var uploadResponseElement = $("#ImportTarget").contents().find("body").html();
            onSuccess(uploadResponseElement);
            $(buttonPressed).removeClass("loader");
        });
        form.submit();
    }
};

/**
* Refreshes the widget's module borders.
* @param includeParentModules (optional)
*            whether to refresh parent container modules as well
*/
GenWidget.prototype.refreshModule = function (includeParentModules) {
    var module = $('#' + this.module);
    var isFixed = this.getIsFixedWidth();

    if (isFixed) {
        var maxContentWidth = 0;
        var headerOffset = 6;
        var footerOffset = 4;
        if (module.hasClass('composite')) {
            footerOffset = 0;
        }

        // find max header width
        var headerBlock = $(module).children('.module-header');
        var headerWidth = $(headerBlock).children('.module-header-center').css('width', '').width();

        // find max content width
        var contentBlocks = $(module).children('.module-content:not(.outer)');
        contentBlocks.each(function () {
            var thisContentWidth = $(this).children('.module-content-center').css('width', '').width();
            if (thisContentWidth > maxContentWidth) {
                maxContentWidth = thisContentWidth;
            }
        });

        // Expand max content width if necessary
        if (headerWidth > (maxContentWidth - headerOffset)) {
            maxContentWidth = headerWidth + headerOffset;
        }

        // Adjust Header
        $(headerBlock).children('.module-header-center').width(maxContentWidth - headerOffset);

        // Adjust all content widths and height
        contentBlocks.each(function () {
            $(this).children('.module-content-center').width(maxContentWidth);
        });

        // Adjust footer
        var footerBlock = $(module).children('.module-footer');
        $(footerBlock).children('.module-footer-center').width(maxContentWidth - footerOffset);

        if (includeParentModules) {
            this.refreshParentModule();  // fire event telling parent to refresh itself
        }
    }
};

/**
* Fire parent-module refesh event
*/
GenWidget.prototype.refreshParentModule = function () {
    var parentModule = this.context.parent().closest('.module-root');
    if ($(parentModule).length > 0) {
        var parentModuleId = $(parentModule).attr('id');
        $(document).trigger('REFRESH_PARENT_MODULE', parentModuleId);
    }
};

/**
* Focuses the page at the first validation box in the widget.
*/
GenWidget.prototype.focusValidation = function () {
    var target = $('#' + this.widgetName + ' .validation-box:visible');
    if (target.length != 0) {
        $('html, body').animate({
            scrollTop: $(target).offset().top
        }, 200);
    }
};

/**
* Sets up validation to occur when an inputs value is changed.
* 
* @param input
*            input that will rpovide the value to validate.
* @param options
*            used to make the ajax request to validate.
* @param message
*            to be displayed if validation fails.
*/
GenWidget.prototype.liveValidate = function (input, options, message) {
    var that = this;
    // if null make it an empty object;
    var formBeingSaved = $(input).parents("form")[0];
    var inputName = $(input).attr("name");
    var valBox = $("ul.validation-box", formBeingSaved);
    input.wrap('<div class="inline-validation"></div>');
    var validationIndicator = $("<div class='validation-status'></div>");

    $(input).after(validationIndicator);

    // if you are sent data see if it's a function, if so call the function.
    if (options.data != null && typeof options.data == 'function') {
        options.data = options.data();
    }

    $(input).change(function () {

        if (message == null) {
            if ($(input).attr("valid") == undefined) {
                $(input).width($(input).width() - $(validationIndicator).width());
            }
        } else {
            $(input).width($(input).width() - $(validationIndicator).width());
        }

        $(input).attr("valid", false);
        validationIndicator.removeClass("inline-validation-valid");
        validationIndicator.removeClass("inline-validation-invalid");
        validationIndicator.addClass("loader");
        $(formBeingSaved).find(".button.stateful_button").addClass("disabled");
        $(formBeingSaved).find("button.stateful_button").prop('disabled', true);

        $.ajax({
            type: options.type || 'POST',
            url: options.url,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: options.data || JSON.stringify({
                value: $(input).val()
            }),
            success: function (response) {
                $(valBox).find("li[name=" + inputName + "]").remove();

                if ($(valBox).find("li").length == 0) {
                    $(valBox).hide();
                }

                if (response.Status) {
                    validationIndicator.removeClass("loader");
                    if (message) {
                        $(input).width($(input).width() + $(validationIndicator).width());
                    } else {
                        validationIndicator.removeClass("inline-validation-invalid");
                        validationIndicator.addClass("inline-validation-valid");
                    }
                } else {
                    validationIndicator.removeClass("loader");
                    if (message) {
                        valBox.append("<li name='" + inputName + "'>" + message + "</li>");
                        $(valBox).show();
                        $(valBox).fadeIn(500);
                        that.focusValidation();
                        $(input).width($(input).width() + $(validationIndicator).width());
                    } else {
                        validationIndicator.removeClass("inline-validation-valid");
                        validationIndicator.addClass("inline-validation-invalid");
                    }
                }
                $(input).attr("valid", response);

                if ($("input[valid=false]", $(formBeingSaved)).length == 0) {
                    $(formBeingSaved).find(".button.stateful_button").removeClass("disabled");
                    $(formBeingSaved).find("button.stateful_button").prop('disabled', false);
                }
            },
            error: options.error || function () {
            }
        });
    });
};

/**
* Sets up validation to occur when called.
* 
* @param input
*            input that will provide the value to validate.
* @param options
*            used to make the ajax request to validate.
* @param message
*            to be displayed if validation fails.
*/
GenWidget.prototype.performCheck = function (input, output, options, message) {
    var that = this;
    // if null make it an empty object;
    var formBeingSaved = $(input).parents("form")[0];
    var inputName = $(input).attr("name");
    var valBox = $("ul.validation-box", formBeingSaved);

    // if you are sent data see if it's a function, if so call the function.
    if (options.data != null && typeof options.data == 'function') {
        options.data = options.data();
    }

    $(input).attr("valid", false);

    $(formBeingSaved).find(".button.stateful_button").addClass("disabled");
    $(formBeingSaved).find("button.stateful_button").prop('disabled', true);

    $.ajax({
        type: options.type || 'POST',
        url: options.url,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        async: false,
        data: options.data || JSON.stringify({
            value: $(input).val()
        }),
        success: function (response) {
            $(valBox).find("li[name=" + inputName + "]").remove();

            if ($(valBox).find("li").length == 0) {
                $(valBox).hide();
            }
            if (response > 0 ) {
                $(output).val(response);
                $(input).removeClass('placeholder');
            }
            else if (options.field == "resource" && response != null) {
                if (response.hasOwnProperty("ResourceID")) {
                    $(output).val(response.ResourceID);
                    $(output).attr("orgltid", $(output).attr("ltid"));
                    $(output).attr("ltid", response.ltid);
                    $(output).attr("SpreadText", response.spreadtext);
                    $(output).attr("rateType", response.rateType);
                    $(input).val(response.ResourceDesc);
                    $(input).removeClass('placeholder');
                    $(input).change();
                }
                else {
                    if (message) {
                        valBox.append("<li name='" + inputName + "'>" + message + "</li>");
                        $(valBox).show();
                        $(valBox).fadeIn(500);
                        that.refreshModule();
                        that.focusValidation();
                    }
                }
            }
            else {

                if (message) {
                    valBox.append("<li name='" + inputName + "'>" + message + "</li>");
                    $(valBox).show();
                    $(valBox).fadeIn(500);
                    that.refreshModule();
                    that.focusValidation();
                }
            }
            $(input).attr("valid", response);

            if ($("input[valid=false]", $(formBeingSaved)).length == 0) {
                $(formBeingSaved).find(".button.stateful_button").removeClass("disabled");
                $(formBeingSaved).find("button.stateful_button").prop('disabled', false);
            }

        },
        error: message || function () {
        },
        complete: function (response) {
        }
    });
};

/**
* Clears the contents of the validation box for a widget.
*/
GenWidget.prototype.clearValidationBox = function (inValBox) {
    var valBox = null;

    if (inValBox) {
        valBox = $(inValBox);
    } else {
        valBox = $("#" + this.widgetName + " ul.validation-box");
    }

    var options = $(valBox).find("li");
    options.remove();
    $(valBox).hide();
};

/**
* Processes an ajax request along with applying all default behaviors that
* a widget should perform. * These behaviors include:
* 
* <pre>
* 1. Making the button pressed provide feed back that a request is being processed (via a spinnin icon). 
* 2. Appyling an overlay the system while the action is processing. 
* 3. Catches Exceptions from the server and displays the messages in the validation box for the form it belongs too.
* </pre>
* 
* @param options
*            the options for the ajax request (these include all of the options for
*            $.ajax(options)) The only required ones are data, url, success
* @param options.performSpinnyIcon
*            Determines if we want to apply the loader class to the formElement while the server is processing 
* @param options.performPageDisabling
*            Determines if we want to apply the overlay to prevent users from editing the page during processing
* @param options.performValidation
*            Determines if we want validation errors to be displayed in the standard red validation box at the top of the form.
* @param options.performDirtyProcessing
*            Determines if we want to clear the dirty bit upon success.
* @param formElement
*            this is the formElement that was modified to execute the request.
*            This is used for two things
* 
* <pre>
*            1. The element will become a spinny icon before the save request, and return to normal after a
*            response. 
*            2. The element provides the context for the current form, so that the method can find the correct validation area.
* </pre>
*/
GenWidget.prototype.ajaxRequest = function (options, formElement) {
    var that = this;

    // Setup default options
    if (typeof options.performSpinnyIcon === 'undefined') {
        options.performSpinnyIcon = false;
    }
    if (typeof options.performPageDisabling === 'undefined') {
        options.performPageDisabling = false;
    }
    if (typeof options.performValidation === 'undefined') {
        options.performValidation = false;
    }
    if (typeof options.performRowValidation === 'undefined') {
        options.performRowValidation = false;
    }
    if (typeof options.performDirtyProcessing === 'undefined') {
        options.performDirtyProcessing = false;
    }

    if (options.performSpinnyIcon) {
        $(formElement).addClass('loader');
    }
    var formBeingSaved = $(formElement).parents("form");
    var formObject = that.getForm(formBeingSaved.attr('id'));

    if (options.data != null && typeof options.data == 'function') {
        options.data = options.data();
    }

    if (options.performPageDisabling) {
        var overlay = $("<div class='full-screen-overlay'></div>");
        $("body").append(overlay);
    }

    var gatherValidationBoxes = function () {
        //still need this for nested widgets however it won't cover nested widget dialog boxes.
        var AllValBoxes = that.context.find('ul.validation-box');

        //get all dialogs directly tied to the widget.
        $(that.dialogs).each(function () {
            var valbox = $(this.dialogElement).find('ul.validation-box')[0];
            if (valbox != undefined && !GenHelper.arrayContains(AllValBoxes, valbox)) {
                AllValBoxes.push(valbox);
            }
        });

        return AllValBoxes;
    };

    $.ajax({
        type: options.type || 'POST',
        url: options.url,
        contentType: options.contentType || 'application/json; charset=utf-8',
        dataType: options.dataType || 'json',
        data: options.data || JSON.stringify(that.getForm(formBeingSaved.attr('id')).getData()),
        success: function (data, textStatus, jqXHR) {
            if (options.performDirtyProcessing) {
                // will clear the dirty bit but will miss removing the
                // save-buttons disabled property.
                that.cleanDirty(formBeingSaved.attr('id'));
            }

            if (options.performValidation) {
                // Clear all validation boxes that are in that widget.
                // this has been upgraded to clear all forms wven those that have been moved to a dialog.
                $(that.forms).each(function () {
                    var form = this;
                    form.clearValidationBox();
                });

                var AllValBoxes = gatherValidationBoxes();

                $(AllValBoxes).each(function () {
                    that.clearValidationBox(this);
                });

                var AllValIcons = $('#' + that.widgetName + ' div.validation-row-icon');
                $(AllValIcons).each(function () {
                    $(this).remove();
                });
            }

            if (options.success != null) {
                options.success(data, textStatus, jqXHR);
            }

            that.refreshModule();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (options.performValidation) {
                var response = jqXHR.responseText;
                var error = {};
                if (!(response.indexOf('{') < 0 || response.indexOf('{') > 2)) {
                    error = $.parseJSON(response);
                    if (error.ReturnType == "GenValidationException") {
                        // First clear all validation boxes that are in that widget.
                        // this has been upgraded to clear all forms wven those that have been moved to a dialog.
                        $(that.forms).each(function () {
                            var form = this;
                            form.clearValidationBox();
                        });

                        var AllValBoxes = gatherValidationBoxes();

                        $(AllValBoxes).each(function () {
                            that.clearValidationBox(this);
                        });

                        //might need to updated to take care of dialog val-boxes
                        if (error.Level == "warning") {
                            $(AllValBoxes).addClass("warning-box");
                        } else {
                            $(AllValBoxes).removeClass("warning-box");
                        }

                        var AllValIcons = $('#' + that.widgetName + ' div.validation-row-icon');
                        $(AllValIcons).each(function () {
                            $(this).remove();
                        });

                        // All boxes that are being targeted so we can animate them once all messages are added.
                        var valBoxes = [];
                        var DefaultValBox = $("#" + that.widgetName + " ul.validation-box")[0];
                        var rowIcons = [];

                        if (formElement) {
                            DefaultValBox = $(formElement).parents("form").find("ul.validation-box")[0];
                        }

                        for (validationError in error.MessageList) {
                            if (error.MessageList.hasOwnProperty(validationError)) {
                                if (error.MessageList[validationError].FormIDToTarget != null && error.MessageList[validationError].FormIDToTarget != "") {
                                    var FormToTargetForAMessage = $("#" + error.MessageList[validationError].FormIDToTarget);
                                    var TargetedValBox = FormToTargetForAMessage.find("ul.validation-box")[0];

                                    if (!GenHelper.arrayContains(valBoxes, TargetedValBox)) {
                                        valBoxes.push(TargetedValBox);
                                    }

                                    $(TargetedValBox).append("<li>" + error.MessageList[validationError].ValidationIssue + "</li>");

                                    if (options.performRowValidation) {
                                        var indexedItemAttr = (error.MessageList[validationError].IndexedItem == null) ? '' : ' tblname="' + error.MessageList[validationError].IndexedItem + '"';

                                        if (error.MessageList[validationError].PkId != null) {
                                            var rowIcon = $('<div class="validation-row-icon" style="display: none;" pkid=' + error.MessageList[validationError].PkId + indexedItemAttr + '></div>');
                                            rowIcons.push(rowIcon);
                                            $(TargetedValBox).after(rowIcon);
                                        }
                                        else if (error.MessageList[validationError].RowIndex != null) {
                                            var rowIcon = $('<div class="validation-row-icon" style="display: none;" index=' + error.MessageList[validationError].RowIndex + indexedItemAttr + '></div>');
                                            rowIcons.push(rowIcon);
                                            $(TargetedValBox).after(rowIcon);
                                        }
                                    }
                                } else {
                                    if (!GenHelper.arrayContains(valBoxes, DefaultValBox)) {
                                        valBoxes.push(DefaultValBox);
                                    }

                                    // append to default
                                    $(DefaultValBox).append("<li>" + error.MessageList[validationError].ValidationIssue + "</li>");

                                    if (options.performRowValidation) {
                                        var indexedItemAttr = (error.MessageList[validationError].IndexedItem == null) ? '' : ' tblname="' + error.MessageList[validationError].IndexedItem + '"';

                                        if (error.MessageList[validationError].PkId != null) {
                                            var rowIcon = $('<div class="validation-row-icon" style="display: none;" pkid=' + error.MessageList[validationError].PkId + indexedItemAttr + '></div>');
                                            rowIcons.push(rowIcon);
                                            $(DefaultValBox).after(rowIcon);
                                        }
                                        else if (error.MessageList[validationError].RowIndex != null) {
                                            var rowIcon = $('<div class="validation-row-icon" style="display: none;" index=' + error.MessageList[validationError].RowIndex + indexedItemAttr + '></div>');
                                            rowIcons.push(rowIcon);
                                            $(DefaultValBox).after(rowIcon);
                                        }
                                    }
                                }
                            }
                        }

                        if (error.Level == "warning") {
                            var acknowledgementButton = $("<li class='acknowledgement-item'><a class='acknowledgement-link'>Acknowledge</a></li>");
                            $(DefaultValBox).append(acknowledgementButton);

                            $(acknowledgementButton).click(function () {
                                options.data = JSON.parse(options.data);
                                options.data.SkipWarnings = true;
                                options.data = JSON.stringify(options.data);
                                that.ajaxRequest(options, formElement);
                            });
                        }

                        $(valBoxes).fadeIn(500);
                        $(rowIcons).each(function () {
                            var tableSelector = (typeof ($(this).attr('tblname')) == 'undefined') ? 'table' : 'table[name="' + $(this).attr('tblname') + '"]';

                            if (typeof ($(this).attr('pkid')) != 'undefined') {
                                var row = $(this).closest('form').find(tableSelector + ' tbody tr[pkid="' + $(this).attr('pkid') + '"]');
                                var offset = row.offset().top;

                                $(this).css('top', offset);
                            } else {
                                var rows = $(this).closest('form').find(tableSelector + ' tbody tr[pkid]');
                                var row = rows[$(this).attr('index')];
                                var offset = $(row).offset().top;

                                $(this).css('top', offset);
                            }
                            $(this).fadeIn(500);
                        });

                        that.refreshModule();
                        that.focusValidation();
                    }
                }
            }
            if (options.error != null) {
                options.error(jqXHR, textStatus, errorThrown);
            }
        },
        complete: function (jqXHR, textStatus) {
            if (options.complete != null) {
                options.complete();
            }

            if (options.performSpinnyIcon) {
                $(formElement).removeClass('loader');
            }

            if (options.performPageDisabling) {
                overlay.remove();
            }
        }
    });
}

/**
* Processes an ajax save request along with applying all default behaviors that
* a widget should perform. * These behaviors include:
* 
* <pre>
* 1. Making the button pressed provide feed back that a request is being processed (via a spinning icon). 
* 2. Appyling an overlay the system while the action is processing. 
* 3. Catches Exceptions from the server and displays the messages in the validation box for the form it belongs too.
* </pre>
* 
* @param options
*            the options for the ajax request (these mirror the options for
*            $.ajax(options)) The only required ones are data, url, success
* @param buttonPressed
*            this is the button that was pressed to execute the save request.
*            The button is used for two things
* 
* <pre>
*            1. Button will become a spinny icon before the save request, and return to normal after a
*            response. 
*            2. The button provides the context for the current form, so that the method can find the correct validation area.
* </pre>
*/
GenWidget.prototype.saveRequest = function (options, buttonPressed) {

    if (typeof options.performSpinnyIcon === 'undefined') {
        options.performSpinnyIcon = true;
    }
    if (typeof options.performPageDisabling === 'undefined') {
        options.performPageDisabling = true;
    }
    if (typeof options.performValidation === 'undefined') {
        options.performValidation = true;
    }
    if (typeof options.performDirtyProcessing === 'undefined') {
        options.performDirtyProcessing = true;
    }

    this.ajaxRequest(options, buttonPressed);
};

/**
* Registers event, calls the PageManagers method of the same name.
* 
* @param {String} type The type of event (i.e. 'click', 'MY_EVENT', 'keypress').
* @param {String} [selector] A selector for the objects to bind
* @param {String} [context=document] The context for the event
* @param {function} handler The function to bind
* @see GenWidget#on
*/
GenWidget.prototype.on = function (type, selector, context, handler) {
    if (handler == null && context == null) {
        handler = selector;
        GenSession.on(type, handler);
    } else {
        if (handler == null) {
            handler = context;
            context = this.context;
        }

        GenSession.on(type, selector, context, handler);
    }
};

/**
* Registers event, calls the PageManagers method of the same name.
* 
* @param {String} type The type of event (i.e. 'click', 'MY_EVENT', 'keypress').
* @param {String} [selector] A selector for the objects to bind
* @param {String} [context=widget.context] The context for the event
* @param {function} [handler] The function to bind
* @see GenWidget#on
*/
GenWidget.prototype.off = function (type, selector, context, handler) {
    if (selector == null && handler == null && context == null) {
        GenSession.off(type);
    } else if (handler == null && context == null) {
        context = this.context;
        GenSession.off(type, selector, context);
    } else if (handler == null) {
        GenSession.off(type, selector, context);
    } else {
        GenSession.off(type, selector, context, handler);
    }
};

/**
* @deprecated (Replaced with GenWidget.on)
*
* This method will create a unique event tied to this widget. Calls the
* PageManagers method of the same name however it provides the widget name in
* order to insure a event is only bound once.
* 
* @param eventName
*            The name to give the event.
* @param functiontoBind
*            The function to bind to this event.
*/
GenWidget.prototype.registerForEvent = function (eventName, functiontoBind) {
    GenSession.registerForEvent(eventName, this.widgetName, functiontoBind);
};

/**
 * @deprecated (Replaced with GenWidget.on)
*
* This method will create a unique delegated (similar to a live event) event
* tied to this widget. Calls the PageManagers method of the same name however
* it can provide a context is none is given.
* 
* @param eventName
*            The name to give the event.
* @param eventOwnerSelector
*            The jQuery Selector for the element to delegate for. Be sure to
*            send in the selector only and not an actual object.
* @param functionToBind
*            The function to bind to this event.
* @param inContext
*            an optional param for a context to limit the eventOwnerSelector
*            to, if non is given it will default to the widget's context
*            (highly recommended)
*/
GenWidget.prototype.registerForDelegateEvent = function (eventName, eventOwnerSelector, functionToBind, inContext) {
    if (inContext == null) {
        inContext = this.context;
    }
    GenSession.registerForDelegateEvent(eventName, eventOwnerSelector, functionToBind, inContext);
};

/**
* @deprecated (Replaced with GenWidget.off)
*
* This method will unbind a registered delegate.
* 
* @param eventName
*            The name to give the event.
* @param eventOwnerSelector
*            The jQuery Selector for the element to delegate for. Be sure to
*            send in the selector only and not an actual object.
* @param inContext
*            an optional param for a context to limit the eventOwnerSelector
*            to, if non is given it will default to the widget's context
*            (highly recommended)
*/
GenWidget.prototype.unregisterForDelegateEvent = function (eventName, eventOwnerSelector, inContext) {
    if (inContext == null) {
        inContext = this.context;
    }
    GenSession.unregisterForDelegateEvent(eventName, eventOwnerSelector, inContext);
};

// TODO either refactor this out or make the readonly bit private

/**
* An access method for the publicly available readonly bit. obviously this
* seems unnesscary a TODO is in place to refactor this out or make the readonly
* bit private
* 
* @returns the read only bit
*/
GenWidget.prototype.isReadOnly = function () {
    return this.readOnly;
};

/**
* This will process a link in an iframe and allow the user an ajaxy way to
* trigger a download.
* 
* @param exportURL
*            the url to call to perform the export.
*/
GenWidget.prototype.performExport = function (exportURL) {
    // Remove the old hidden iFrame, if it exists
    $('iframe#ExportTarget').remove();

    // Create a new hidden iFrame and set it's source to the chosen report's URL
    var targetIFrame = $('<iframe />', {
        'id': 'ExportTarget',
        'class': 'display-none',
        'src': exportURL
    });

    targetIFrame.appendTo('body');
};

/**
* @class A widget with list functionality.
* @augments GenWidget
*/
GenListWidget.prototype = new GenWidget();

/**
* A GenListWidget is a widget with a list of data it is showing.  The widget heavily relies on backend responses to manage it's data.
* @constructor
* @augments GenWidget
* @param config
*            Configureation for this object.
* @param config.deleteable
*            applies the deleteable col and behaviors.
* @returns {GenListWidget}
*/
function GenListWidget(config) {
    // Calls Super constructor
    GenWidget.call(this, config);

    // TODO investigate making paging data private, mainly used to clear paging
    // inorder to refresh a page.
    /**
    * Currently public paging data TODO added to investigate making this
    * private.
    */
    this.pagingData = null;

    /**
    * @private
    */
    var that = this;

    /**
    * @private
    */
    var listTable = $("table", that.context);

    /**
    * context for the top menu of the widget (where the add, delete, import,
    * export buttons are normally)
    * 
    * @private
    */
    var listMenu = $(".widget-menu", that.context);

    if (config != undefined) {
        if (config.PagingData != null) {
            this.pagingData = config.PagingData;
        }

        if (config.deleteable) {
            that.utils.applyDeleteableRows(listMenu);
        }
    }

    if (that.readOnly)
        applyListWidgetReadOnly();

    if (that.pagingData != null) {

        if (that.pagingData.searchFilter) {
            applySearchFilter();
        }

        var pageControls = $(".paging-control", that.context);

        if (pageControls.length < 1) {
            $(that.pagingData.ContentDiv).before('<div class="paging-control">Error determining page</div>');
        }
    }

    /**
    * @deprecated
    *
    * This is for backwards compatibility this 
    * method should not be called directly normally.
    */
    this.applyDeleteColToTable = function () {
        that.utils.applyDeleteColToTable();
    }

    /**
    * This method needs to be called on the load of a paged data view.
    * 
    * @param inPagingData
    *            the json representation of any PageableDataModelView (MV name
    *            subject to change)
    */
    this.updatePagingData = function (inPagingData) {
        that.pagingData.data = inPagingData;
        this.addPaging();
        if (that.pagingData.data.SortChanged != undefined) {
            this.addSorting();
        }
        // this will set the actually width style using the computed.

        var isFixed = this.getIsFixedWidth();
        if (isFixed) {
            that.pagingData.ContentDiv.width(that.pagingData.ContentDiv.width());
        }
    };

    /**
    * Pages a table of data given the widgets pagingData.
    * 
    * @param animation
    *            The animation to perform, currently any direction "up",
    *            "down", "left", "right"
    */
    this.pageResults = function (animation) {

        var directionsDictionary = [["left", "right"], ["right", "left"], ["up", "down"], ["down", "up"]];
        var directionSet = null;

        if (animation != null) {
            $(directionsDictionary).each(function () {
                if (this[0] == animation) {
                    directionSet = this;
                    return false;
                }
            });
        }

        // check to see if the user has defined any additional parameters
        // if so, add them to the pagingData.data object before we stringify
        if (that.pagingData.params != undefined) {
            for (var i = 0; i < that.pagingData.params().length; i++) {
                for (name in that.pagingData.params()[i]) {
                    that.pagingData.data[name] = that.pagingData.params()[i][name];
                }
            }
        }

        var dataToSend = JSON.stringify(that.pagingData.data);

        if (directionSet != null) {
            that.pagingData.ContentDiv.children().hide("slide", {
                direction: directionSet[0]
            }, 300, function () {
                that.refreshModule();
            });
            that.pagingData.ContentDiv.addClass("loader");
        }

        GenSession.ShowLoadingBox();

        $.ajax({
            type: 'POST',
            url: that.pagingData.pagingUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                that.pagingData.ContentDiv.html(response);

                // apply delete before you start to slide in so that it doesn't
                // jump.
                if (config.deleteable) {
                    that.utils.applyDeleteColToTable();
                }

                if (that.readOnly)
                    applyListWidgetReadOnly();

                // finish animations
                if (directionSet != null) {
                    that.pagingData.ContentDiv.children().show("slide", {
                        direction: directionSet[1]
                    }, 300, function () {
                        that.refreshModule();
                    });
                    that.pagingData.ContentDiv.removeClass("loader");
                    that.refreshModule();
                } else if (animation == "pulsate") {
                    that.refreshModule();
                    that.pagingData.ContentDiv.effect("pulsate", {
                        times: 1
                    }, 250);
                } else {
                    that.refreshModule();
                }

                // disable the delete-button from the list menu
                $("button[name='delete-button']", listMenu).addClass('disabled');

                // run any injected success method.
                if (that.pagingData.success) {
                    that.pagingData.success();
                }
                GenSession.HideLoadingBox()
            }
        });
    };

    // TODO investigate makign this private.

    /**
    * Adds paging to the widget, currently exposed a TODO has been created to
    * see about hiding this.
    */
    this.addPaging = function () {
        var pagingDiv = $(".paging-control", that.context);

        if (that.pagingData.type == "Arrow") {
            var displayPreviousArrow = that.pagingData.data.StartArrayIndex > 0;
            var displayNextArrow = that.pagingData.data.EndArrayIndex < that.pagingData.data.PagedIndexes.length - 1;
            var pagingHtml = '<div class="paging"><div class="pages"';
            if (!displayPreviousArrow && !displayNextArrow) {
                pagingHtml += 'style="top: 0px"';
            }
            pagingHtml += '>';
            pagingHtml += that.pagingData.data.StartArrayIndex + 1;
            pagingHtml += ' - ';
            pagingHtml += that.pagingData.data.EndArrayIndex + 1;
            pagingHtml += ' of ';
            pagingHtml += that.pagingData.data.PagedIndexes.length;
            pagingHtml += '</div>';
            if (displayPreviousArrow) {
                pagingHtml += '<div class="previous-arrow"></div>';
            }
            if (displayPreviousArrow && displayNextArrow) {
                pagingHtml += '&nbsp;&nbsp';
            }
            if (displayNextArrow) {
                pagingHtml += '<div class="next-arrow"></div></div>';
            }

            pagingDiv.addClass('arrow');
            pagingDiv.html(pagingHtml);

            pagingDiv.find('div.previous-arrow').click(function () {
                that.pagingData.data.CurrentPage -= 1;
                that.pageResults("left");
            });

            pagingDiv.find('div.next-arrow').click(function () {
                that.pagingData.data.CurrentPage += 1;
                that.pageResults("right");
            });
        } else if (that.pagingData.type == "PageNumber") {
            var pagingHtml = '<div class="paging">';

            if (that.pagingData.data.CurrentPage < 7) {
                for (var ndx = 1; ndx < that.pagingData.data.CurrentPage; ndx++) {
                    pagingHtml += '<a class="page">' + ndx + '</a>';
                }
            } else {
                pagingHtml += '<a class="page">1</a>';
                pagingHtml += '<span class="elipsis">...</span>';
                for (var ndx = that.pagingData.data.CurrentPage - 4; ndx < that.pagingData.data.CurrentPage; ndx++) {
                    pagingHtml += '<a class="page">' + ndx + '</a>';
                }
            }

            pagingHtml += '<div class="current-page">' + that.pagingData.data.CurrentPage + '</div>';

            if (that.pagingData.data.NumPages < that.pagingData.data.CurrentPage + 6) {
                for (var ndx = that.pagingData.data.CurrentPage + 1; ndx <= that.pagingData.data.NumPages; ndx++) {
                    pagingHtml += '<a class="page">' + ndx + '</a>';
                }
            } else {
                for (var ndx = that.pagingData.data.CurrentPage + 1; ndx <= that.pagingData.data.CurrentPage + 4; ndx++) {
                    pagingHtml += '<a class="page">' + ndx + '</a>';
                }
                pagingHtml += '<span class="elipsis">...</span>';
                pagingHtml += '<a class="page">' + that.pagingData.data.NumPages + '</a>';
            }

            pagingHtml += '</div><div class="clear"></div>';

            pagingDiv.addClass('page-number');
            pagingDiv.html(pagingHtml);

            pagingDiv.find('a.page').click(function () {
                that.pagingData.data.CurrentPage = $(this).html();
                that.pageResults();
            });
        } else {
            throw 'Invalid paging type.  Please use "Arrow" or "PageNumber"';
        }
    };

    /****
     * Adds sorting to the pageable table.
     * Note: If your pagingData contains a field called "DecorateSortOrder" and it's set to true
     * this method will attempt to find a field called Order in the data object and use an alternate
     * style on the table header to show sort oder.  Order = 1 means ascending order, Order = 0 means
     * descending order.
     ****/
    this.addSorting = function () {
        $('th[sortID]').each(function () {
            var thisHeader = $(this);
            var thisSortID = thisHeader.attr('sortID');

            // Make each sort header appear styled as a link
            thisHeader.addClass('link');

            // Bold the clicked sort header, optionally decorate it showing sort order.
            if (that.pagingData.data.SortField != undefined && that.pagingData.data.SortField == thisSortID) {
                thisHeader.addClass('bold');

                // Determine if we should decorate the table header with sort order indicator as well.
                if (that.pagingData.DecorateSortOrder != undefined && that.pagingData.DecorateSortOrder == true) {
                    if (that.pagingData.data.Order != undefined && that.pagingData.data.Order == 1) {
                        thisHeader.addClass('sort_ascending');
                    }
                    else {
                        thisHeader.addClass('sort_descending');
                    }
                }
            }

            // When a sort header is clicked
            thisHeader.click(function () {
                // Page the data with sorting applied
                that.pagingData.data.SortField = thisSortID;
                that.pagingData.data.SortChanged = true;
                that.pageResults();
            });
        });
    };

    /**
    * @private
    */
    function applyListWidgetReadOnly() {
        that.getElement('tbody td a').each(function () {
            $(this).parent().append('<div style="display: inline-block">' + $(this).text() + '</div>');
            $(this).remove();
        });

        $(listMenu).addClass('readonly');
    }

    /**
    * @private
    */
    function applySearchFilter() {

        var additionalElements = "";
        if ($(".paging-control", that.context).length < 1) {
            additionalElements = '<div class="paging-control"></div>';
        }

        var searchBox = $('.search-box', that.context);

        if (searchBox.length < 1) {
            $(listMenu).append(
				'<div class="search-box">' + additionalElements
				+ '<input type="text" value="" name="SearchText" class="ignore-dirty">'
				+ '<div class="button search-magnify-glass-button"></div></div>');
        } else {
            searchBox.append(additionalElements
				+ '<input type="text" value="" name="SearchText" class="ignore-dirty">'
				+ '<div class="button search-magnify-glass-button"></div>');
        }

        that.getElement('input[name=SearchText]').keydown(function (e) {
            /** **enter key to search */
            var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
            if (keyCode == 13) {
                performSearch($(this));
                //return false so the enter key doesn't propagate to a form submit.
                return false;
            }
        });

        // search button
        that.getElement(".button.search-magnify-glass-button").click(function () {
            performSearch($(this).parent().find('input[name=SearchText]'));
        });

        that.refreshModule();
    }

    /**
    * @private
    */
    function performSearch(element) {
        that.pagingData.data = {};
        that.pagingData.data.SearchFilter = element.val();
        that.pageResults("down");
    }
}

/**
* @class GRID WIDGET
*/
GenGridWidget.prototype = new GenWidget();

/**
* A GenGridWidget is a Widget that contains a grid of data that can be modified inside the grid itself. it tracks changes to each row of data in the grid.
*
* @constructor
* @augments GenWidget
* @param config
* @param {required}
*            config.DataKeyID String of the primary key expected in the MV
* @param config.OnDataChanged
*            Method to call after data has changed.
* @param config.OnNewRowCreated
             Method to call when a new row is created.
* @returns {GenGridWidget}
*/
function GenGridWidget(config) {
    // Calls Super constructor
    GenWidget.call(this, config);

    /**
    * @private
    */
    var that = this;

    /**
    * context for the top menu of the widget (delete, import,
    * export buttons are normally)
    * 
    * @private
    */
    var listMenu = $(".widget-menu", that.context);

    /**
    * @private
    */
    var onDataChanged = null;

    /**
    * @private
    */
    var onNewRowCreated = null;

    /**
    * @private
    */
    var newRecordCount = -1;

    /** 
    * @private
    */
    var blankRowHtml = null;

    /**
    * @private
    */
    var table = null;

    if (config != undefined) {
        // key value of data.
        if (config.DataKeyID == null) {
            throw {
                name: 'ArgumentMissing',
                message: 'DataKeyID is required.'
            };
        } else {
            this.dataKeyID = config.DataKeyID;
        }
        onDataChanged = config.OnDataChanged;
        onNewRowCreated = config.OnNewRowCreated;
    }

    /**
    * json record of the data you have modified in the grid. in a grid widget
    * the data is always an array of something.
    * NOTE: In the newer way of thinking maybe this should be on a from and not the widget.
    */
    this.data = new Array();

    /**
    * @private
    */
    function initializeGrid() {
        that.registerForDelegateEvent("change","tbody :input:not(:hidden, "+GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES+")", function () {
            that.updateData(this);
        });

        if (config != undefined) {
            if (config.deleteable) {
                that.utils.applyDeleteableRows(listMenu);
            }
        }

        createAddableGrid();
        applyFilterableGrid();
        applySortableGrid();

        that.refreshModule();
    }

    // once data is saved you need to clear the data from the update array.
    // if the partialview is reloaded then we dont' have to clear the data
    // field.
    this.cleanDirty = function (inFormID) {
        if (inFormID == null) {
            // this is the only difference from a normal widget.
            that.data = new Array();
            inFormID = that.widgetName;

            for (var n in that.forms) {
                $(document).trigger('DATA_CLEANED', that.forms[n]);
            }
        }

        $(document).trigger('DATA_CLEANED', inFormID);

        that.refreshModule();
    };

    /**
    * @private
    */
    function createAddableGrid() {
        table = $($("#" + that.widgetName + " form table")[0]);

        var blankRow = table.find("tr.blank");

        // initialize the pkid counter to the blank row pkid value (if it exists)
        var blankRowPkid = blankRow.attr('pkid');
        if (blankRowPkid == undefined || blankRowPkid == '') {
            blankRow.attr('pkid', newRecordCount);
        } else {
            newRecordCount = Number(blankRowPkid);
        }

        var rowHtml = $(blankRow[0]).html();

        blankRowHtml = '<tr pkid="' + newRecordCount + '" class="blank">' + rowHtml + '</tr>';

        // Only one of these next two events will be kicked off because both only occur for tr.blank and once one occurs
        // It removed the .blank and the other one will not be a valid match.
        that.registerForDelegateEvent("keydown", "form table tr.blank td", function (e) {
            var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
            if (!GenHelper.arrayContains(GenConstants.NONEDIT_KEYS, keyCode)) {
                addBlankRow();
                that.refreshModule();
            }
        });

        //this will caught any non keydown events

        that.registerForDelegateEvent("change", "form table tr.blank td", function () {
            addBlankRow();
            that.refreshModule();
        });
    }

    /**
    * @private
    */
    function addBlankRow() {
        // For current BlankRow remove add new row event and remove the blank class
        var blankRow = table.find("tr.blank");

        blankRow.removeClass('blank');

        // Add new blank row and add new row event
        $(table).append(blankRowHtml);

        var newRow = $(table).find('tr.blank');
        newRow.attr("pkid", --newRecordCount);

        // trigger a event for the creation of a new blank row.
        if (typeof (onNewRowCreated) == 'function') {
            onNewRowCreated(newRow);
        }
    }

    /**
    * Updates the data variable with the latest change
    * 
    * @param input
    *            The input that caused the changed.
    *        dataChangedParam
    *            Used to determine if a user should skip an action.
    * @returns {Boolean}
    */
    this.updateData = function (input, dataChangedParam) {
        var ElementID = $(input).parents('tr').attr('pkid');
        var updateIndex = -1;

        // First see if you have already updated this data.
        for (var k in that.data) {
            if (that.data[k][that.dataKeyID] == ElementID) {
                updateIndex = k;
                break;
            }
        }

        if (updateIndex == -1) {
            updateIndex = that.data.length;
            that.data[updateIndex] = {};
            that.data[updateIndex][that.dataKeyID] = ElementID;
        }

        // fill in remaining inputs.
        $("#" + that.widgetName + " tr[pkid|=" + ElementID + "]").find(":input:not([name=" + that.dataKeyID + "])").each(
                function updateDataHelper(index, value) {
                    that.data[updateIndex][$(value).attr("name")] = $(value).val();
                });

        if (typeof onDataChanged == 'function') {
            onDataChanged(input, updateIndex, dataChangedParam);
        }

        var form = $('#' + that.widgetName + ' form');
        if (form.length > 0) {
            $(document).trigger('DATA_DIRTY', $(form).attr('id'));
        }
        return true;
    };

    /*
    * GRID FILTERING ----------------------------------------------------------
    */

    /**
    * @private
    */
    function applyFilterableGrid() {
        var selector = '#' + that.widgetName + ' table';

        $(selector + ' th.filter').each(function () {
            var filterBy = 'Filter by ';
            // if the th has a label, use this.
            if ($(this).attr('label') != undefined) {
                filterBy = filterText(this);
            } else {
                // else use the name of the filter
                filterBy += $(this).attr('filter');
            }
            var filterBoxHtml = '<div class="filter-left"></div><div class="filter-middle"><input type="text" class="filter-textbox placeholder" placeholder="'
					+ filterBy + '"/></div><div class="filter-right"></div><br/>';
            $(this).prepend(filterBoxHtml);
            var filterbox = $(this).find('.filter-textbox');
            filterbox.keyup(filterRow);
            //This is needed to show the placeholder text in IE
            GenSession.evalPlaceHolderSettings.call(filterbox);
        });

        $(selector + ' th.custom-filter').each(function () {
            var filterbox = $(this).find('select');
            if (filterbox.length < 1) {
                var filterBy = 'Filter by ';
                if ($(this).attr('label') != undefined) {
                    filterBy = filterText(this);
                }
                else {
                    filterBy += $(this).attr('custom-filter');
                }
                filterbox = $(this).find('.selection-popup li');
                var fakeInput = $(this).children('div.fake-input-box');
                $(fakeInput).html('<div class="placeholder">' + filterBy + '</div>');
                filterbox.click(filterRow);
                fakeInput.click(function () {
                    $(this).html('<div class="placeholder">' + filterBy + '</div>');
                    $.proxy(filterRow, this)();
                });
            }
            else {
                filterbox.change(filterRow);
            }
        });

        $(selector + ' th.filter .filter-middle').each(function () {
            $(this).width($(this).parent().width() - 8);
            $(this).find('input').width($(this).parent().width() - 20);
            $(this).find('select').width($(this).parent().width() - 8);
        });
    }

    // Scan all filter fields in the table header and create an array of
    // those fields that contain values. Then go through each filter in the
    // array and filters values in the table. This results in multi-column
    // filtering

    /**
    * @private
    */
    function filterRow() {
        // Initialize the array of filters
        var filters = new Array();
        var row = 0;
        // Get all cells in the header row that have a filter attribute defined
        $(this).parents('tr').children('th[filter]').each(function () {
            // For each filter cell in the header, get the filter input
            if ($(this).find(':input') != undefined) {
                // Get the trimmed value from the filter cell input
                var filterCell = $(this).find(':input');
                var filterVal = '';
                var placeholder = filterText(this);
                if (filterCell.val() != 'Filter...' && filterCell.val() != 'Filter by ' + $(this).attr('filter')) {
                    filterVal = $.trim(filterCell.val());
                }
                // If the filter input has a value, we'll add the value and the
                // filter name
                // to our array of filters
                if (filterVal.length > 0) {
                    filters[row] = {};
                    filters[row].FilterOn = $(this).attr('filter');
                    filters[row].FilterValue = filterVal;
                    row++;
                }
            }
        });
        //if ($(this).hasClass('tree-expandable')) {
        //    return;
        //}
        //This is for gen custom drop downs and html selects
        $(this).parents('tr').children('th[custom-filter]').each(function () {
            var filterVal = '';
            // For each filter cell in the header, get the filter value
            if ($(this).find('div:first').length > 0) {
                // Get the trimmed value from the filter cell value
                var filterCell = $(this).find('div:first');
                if (filterCell.children().length == 0) {
                    filterVal = $.trim(filterCell.html());
                }
            }
            else {
                // Get the trimmed value from the filter cell value
                var filterCell = $(this).find('select');
                if (filterCell.find('option:selected').val() != '') {
                    filterVal = $.trim(filterCell.find('option:selected').html());
                }
            }
            // If the filter has a value, we'll add the value and the
            // filter name
            // to our array of filters
            if (filterVal.length > 0) {
                filters[row] = {};
                filters[row].FilterOn = $(this).attr('custom-filter');
                filters[row].FilterValue = filterVal.replace('&amp;', '&');
                row++;
            }
        });

        // Make all rows visible each time a filter is changed. Then we'll
        // hide only the rows with invalid values against the filter. This works
        // quickly enough that the show/hide isn't visible on the UI
        $(this).parents('table').find('tr:not(.blank)').removeClass('display-none');

        // Iterate over each filter in our array
        for (var ndx in filters) {
            // Get the name and value for the current filters
            var filterOn = filters[ndx].FilterOn;
            var filterValue = filters[ndx].FilterValue;
            // Filter input and select elements
            $(this).parents('table').find(':input[name=' + filterOn + ']').each(function () {
                if (!filterCompare(filterOn, filterValue, $(this))) {
                    $(this).parents('tr:not(.blank)').addClass('display-none');
                }
            });

            // Filter text of SPANS, DIVs and Hyperlinks
            $(this).parents('table').find(
                    'span[name=' + filterOn + '],' + 'div[name=' + filterOn + '],' + 'a[name=' + filterOn + ']').each(
                    function () {
                        if (!spanCompare(filterOn, filterValue, $(this).text())) {
                            $(this).parents('tr:not(.blank)').addClass('display-none');
                        }
                    });
        }
        that.refreshModule();
    }

    /**
    * @private
    * sets the filter text
    */
    function filterText(that) {
        var filterBy = '';
        // ShortLabel is a keyword label used for filter
        // textboxes that are too small to fit the
        // "Filter by FieldName" text.
        if ($(that).attr('label') == 'ShortLabel') {
            filterBy = "Filter...";
        } else {
            // use the label
            filterBy += $(that).attr('label');
        }
        return filterBy;
    }

    /**
    * @private
    * @param filterOn
    * @param filter
    * @param currentValue
    * @returns {Boolean}
    */
    function spanCompare(filterOn, filter, currentValue) {
        filter = $.trim(filter.toLowerCase());
        var currentValue = $.trim(currentValue.toLowerCase());
        if (filter.length > 0 && currentValue.indexOf(filter) < 0 && filter != -1) {
            return false;
        }
        return true;
    }

    /**
    * This method is overridden inside some partial views. Please verify those
    * still work correctly if this needs to be changed. Currently Overriden in:
    * ManageWBSGrid ManageBOEGrid
    * 
    * @private
    * @param filterOn
    * @param filter
    * @param currentItem
    * @returns {Boolean}
    */
    function filterCompare(filterOn, filter, currentItem) {
        filter = $.trim(filter.toLowerCase());
        var currentValue = $.trim(currentItem.val().toLowerCase());
        if ($(currentItem).find(':selected').length > 0) {
            currentItem = $(currentItem).find(':selected').html();
            currentValue = $.trim(currentItem.toLowerCase());
        }
        if (filter.length > 0 && currentValue.indexOf(filter) < 0 && filter != -1) {
            return false;
        }
        return true;
    }

    /*
    * GRID SORTING ----------------------------------------------------------
    */

    /**
    * @private
    */
    function applySortableGrid() {

        var selector = '#' + that.widgetName + ' table';
        // Only add sort links for headers that have the sort class applied AND
        // their sort attribute set
        $(selector + ' th.sort').each(function () {
            $(this).html('<div class="column-sort-header">' + $(this).html() + '</div>');
            $(this).find('div.column-sort-header').click(function () {
                sortColumn(this);
            });
        });
    }

    /**
    * @private
    */
    function sortColumn(sortHeader) {
        // Set the sorting column's header to be underlined
        $(sortHeader).parents('table').find('th.sort div.column-sort-header').css('text-decoration', 'none');
        $(sortHeader).parents('th').find('div.column-sort-header').css('text-decoration', 'underline');

        // Get the sort type
        sortType = $(sortHeader).parents('th').attr('sortType');

        // Get the key to sort on
        var sortColumnIndex = $(sortHeader).parents('thead').find('th').index($(sortHeader).parents('th'));

        // Get the current sort column from the parent table
        var tableSortColumn = $(sortHeader).parents('table').attr('sortedOnColumn');

        // Get the current sort direction from the parent table
        var tableSortAscending = $(sortHeader).parents('table').attr('sortedAscending') == 'true';

        // Get all rows that will be sorted
        var rowsToSort = $(sortHeader).parents('table').find('tbody tr:not(.blank)');

        // Get the table's body element
        var body = $(sortHeader).parents('table').find('tbody');

        // Perform selection sort on the rows, using the sort key to find the
        // appropriate cell

        var rowsToSortlength = rowsToSort.length;
        for (var outerNdx = 0; outerNdx < rowsToSortlength; outerNdx++) {
            var minNdx = outerNdx;
            var minValue = grabRowValue($(rowsToSort[minNdx]), sortColumnIndex, sortType);

            for (var innerNdx = outerNdx + 1; innerNdx < rowsToSortlength; innerNdx++) {
                var currentValue = grabRowValue($(rowsToSort[innerNdx]), sortColumnIndex, sortType);

                // Perform the comparison
                if (currentValue < minValue) {
                    minNdx = innerNdx;
                    minValue = currentValue;
                }
            }

            // Swap
            var temp = rowsToSort[outerNdx];
            rowsToSort[outerNdx] = rowsToSort[minNdx];
            rowsToSort[minNdx] = temp;
        }

        // If the user clicked the same column header, then we'll reverse the
        // sort order
        if (tableSortColumn == sortColumnIndex) {
            tableSortAscending = !tableSortAscending;
        } else {
            tableSortAscending = true;
        }

        // We are using prepend to add the rows back to the tbody before any
        // .blank rows
        // Because of this we actually want to reverse the array when we want a
        // normal
        // ascending sort order. For descending, we'll leave it alone.
        if (tableSortAscending) {
            rowsToSort = $(rowsToSort.get().reverse());
        }

        // Insert the rows back into the table
        rowsToSort.each(function () {
            body.prepend($(this));
        });

        $(sortHeader).parents('table').attr('sortedOnColumn', sortColumnIndex).attr('sortedAscending',
				tableSortAscending.toString());
    }
    ;

    /**
    * @private
    * @param row
    * @param sortColumnIndex
    * @param sortType
    * @returns {String}
    */
    function grabRowValue(row, sortColumnIndex, sortType) {
        var toReturn = '';

        var sortCell = $(row.find('td')[sortColumnIndex]);

        // Get text elements for the current row
        var currentSortCellText = sortCell.find('span, div, a');
        // Get input elements for the current row
        var currentSortCellInput = sortCell.find('input');

        // Continue if the current and minimum rows both have something to
        // compare
        if (currentSortCellText.length || currentSortCellInput.length || sortCell.length) {

            // Choose current row comparison value
            if (currentSortCellText.length) {
                toReturn = currentSortCellText.first().text().toLowerCase();
            } else if (currentSortCellInput.length) {
                toReturn = currentSortCellInput.first().val().toLowerCase();
            } else {
                toReturn = sortCell.text().toLowerCase();
            }
            // Process special sort types
            if (sortType != undefined && sortType != null && sortType.length) {
                // Date sort type
                if (sortType.match(/date/i)) {
                    // Convert the current and minumum values to dates
                    toReturn = toReturn.ensureProperDate().toDate();
                }
                    // Number sort type
                else if (sortType.match(/number/i)) {
                    // Convert the current and minumum values to dates
                    toReturn = parseInt(GenHelper.removeCommas(toReturn));
                } else if (sortType.match(/cost/i)) {
                    // Convert the Cost string into a sortable number.
                    // First remove the $ symbol then remove commas for sorting.
                    toReturn = toReturn.substring(1, toReturn.length);
                    toReturn = GenHelper.removeCommas(toReturn);
                    toReturn = parseInt(toReturn);
                }
            }
        }

        return toReturn;
    }

    initializeGrid();
}

GenGridWidget.prototype.saveRequest = function (options, buttonPressed) {
    // front load data grid style.
    options.data = options.data || JSON.stringify(this.data);
    var SuccessMethod = options.success;
    var thisObject = this;
    // onsuccess clear .data
    options.success = function (data, textStatus, jqXHR) {
        SuccessMethod(data, textStatus, jqXHR);
        thisObject.data = [];
    };

    // call the super method
    GenWidget.prototype.saveRequest.call(this, options, buttonPressed);
};

/**
* Deletes all records from the grid and widget data, or calls updateData for
* deletes of saved rows
* 
* @param input
*            the delete all input
*/
GenGridWidget.prototype.deleteAllRecords = function (input) {
    var that = this;

    var allNonBlankRows = $(input).parents('table').find('tbody tr:not(.blank)');

    // Get the parent row and make sure it's not a blank row
    allNonBlankRows.each(function () {
        // Get the pkid of the current row's data
        var ElementID = $(this).attr('pkid');

        // If this was a new row (ID < 0) we'll just remove it from the widget
        // data
        if (ElementID < 0) {
            var indexToDelete = -1;
            // Get the index of the deleted data
            for (var k in that.data) {
                if (that.data[k][that.dataKeyID] == ElementID) {
                    indexToDelete = k;
                    break;
                }
            }
            // Remove new deleted row from the widget data
            that.data.splice(indexToDelete, 1);
        }
            // If this was an existing row, we'll update its data to pass back to
            // the controller
        else {
            var Deleted = $(this).find(':input[name=Deleted]');
            if (Deleted.length) {
                Deleted.val(true);
                that.updateData(Deleted);
            }
        }
    });

    allNonBlankRows.remove();
};

/**
* Deletes a record from the grid and widget data, or calls updateData for
* deletes of saved rows
* 
* @param onDataChanged
*            Method to run after .data is changed.
*/
GenGridWidget.prototype.deleteRecord = function (input, onDataChanged) {
    // Get the parent row and make sure it's not a blank row
    var parentRow = $(input).parents('tr:not(.blank)');

    if (parentRow.length > 0) {

        // Get the pkid of the data
        var ElementID = parentRow.attr('pkid');

        // If we have a pkid, we'll continue with the delete
        if (ElementID != undefined) {
            // If this was a new row (ID < 0) we'll just remove it from the
            // widget data
            if (ElementID < 0) {
                var indexToDelete = -1;
                // Get the index of the deleted data
                for (var k in this.data) {
                    if (this.data[k][this.dataKeyID] == ElementID) {
                        indexToDelete = k;
                        break;
                    }
                }

                if (onDataChanged != null) {
                    onDataChanged(input, indexToDelete);
                }
                // Remove new deleted row from the widget data
                this.data.splice(indexToDelete, 1);
            }
                // If this was an existing row, we'll update its data to pass back
                // to the controller
            else {
                var Deleted = parentRow.find(':input[name=Deleted]');
                if (Deleted.length) {
                    Deleted.val(true);
                    this.updateData(Deleted);
                }
            }

            // Remove the row from the grid and refresh the parent module
            parentRow.children('td').fadeOut(400, function () {
                var module = parentRow.parents('.module');

                parentRow.remove();

                if (module.length) {
                    refreshModule(module);
                }
            });
        }
    }
};

/**
* A GenDialog wraps a jQuery UI dialog Key Features:
* 
* <pre>
* AbilitytoOpen\Close\Destroy\Change Title on dialogs
* Automatically focus on first element when opened.
* </pre>
* 
* @constructor
* @param inDialogConfig
* @param inDialogConfig.ElementID
*            the ID of the div to use as a dialog.
* @param inDialogConfig.Params
*            Standard jQuery UI dialog parameter params.
* @param inDialogConfig.OnEnterKey
*            Optional callback function for Enter key press.
* @returns {GenDialog}
*/
function GenDialog(inDialogConfig) {

    /**
    * @private
    */
    var that = this;

    /**
    * the dialog ID.
    */
    that.dialogID = inDialogConfig.ElementID;

    // before going any further we need to wipe out any existing versions of
    // this dialog.
    destroyAnyExisitingDialog();

    // previously this variable was holding onto both the old and new versions
    // of the dialog
    // and it would recreate all previous rendered dialogs at the last line of
    // initalize.
    // now that we have wiped out those old versions we can get the new one
    // here.

    /**
    * the dialog jquery context element.
    */
    that.dialogElement = $("#" + that.dialogID);

    // the config is only used for debuging.

    /**
    * @private
    */
    var dialogConfig = inDialogConfig;

    /**
    * @private
    */
    var params = inDialogConfig.Params;

    /**
    * @private
    */
    var onEnterKey = inDialogConfig.OnEnterKey;

    /*
    * private methods
    */

    /**
    * @private
    */
    function destroyAnyExisitingDialog() {
        // Remove old dialogs left behind when jumping between jump pages
        if (that.dialogID != undefined) {
            $('body').children('#' + that.dialogID).dialog('destroy').remove();
            $('body .ui-dialog').children('#' + that.dialogID).dialog('destroy').remove();
        }
    }

    /**
    * @private
    */
    function initializeDialog() {
        // Remove display-none class
        if ($(that.dialogElement).hasClass('display-none')) {
            $(that.dialogElement).removeClass('display-none');
        }

        $(that.dialogElement).addClass('dialog');
        $(that.dialogElement).addClass('form');
        // Set the dialog disabled so that it doesn't open when initialized

        // never autoOpen on initialize
        params.autoOpen = false;

        if (params.close == undefined) {
            params.close = function () {
                var form = $(that.dialogElement).find('form');
                if (form.length != 0) {
                    $(form).each(function () {
                        GenSession.clearDirty($(this).attr("id"));
                    });
                }
            };
        }

        // default if these are not set
        params.width = params.width || 600;
        params.height = params.height || 'auto';
        if (typeof params.modal === 'undefined') {
            params.modal = true;
        }
        params.resizable = params.resizable || false;
        if (typeof params.draggable === 'undefined') {
            params.draggable = true;
        }
        params.closeOnEscape = params.closeOnEscape || false;
        // onOpen is defined by generation
        params.onOpen = params.onOpen || null;
        params.beforeClose = params.beforeClose || null;
        if (typeof params.focusOnOpen === 'undefined') {
            params.focusOnOpen = true;
        }

        /*
        * making this a dialog will rewrap the DOM elements meanign any script will be executed again.
        * this line will remove the scripts so they don't execute again.
        */
        $(that.dialogElement).find("script").remove();

        // Initialize the dialog
        $(that.dialogElement).dialog(params);

        // Detect and handle Enter key press
        if (onEnterKey != undefined) {
            $(":input", that.dialogElement).keydown(function (e) {
                var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                if (keyCode == 13) {
                    onEnterKey(that.dialogElement);  // handle the Enter keypress
                    return false;
                }
            });
        }
    }

    /*
    * public
    */

    /**
    * gets an Element in the scope of the Dialog
    *
    * @param selector
    *             the selector to look up
    */
    this.getElement = function (selector) {
        return $(selector, that.dialogElement);
    }

    /**
    * opens the Dialog.
    *
    * @param data
    *             optional set of data to pass to the handler function
    */
    this.openDialog = function (data) {
        that.clearDialogValidationBoxes();
        $(that.dialogElement).dialog('open');

        if (typeof params.onOpen == 'function') {
            params.onOpen(data);
        }

        if (params.focusOnOpen) {
            // focus on first element if one exists
            if ($(":input:visible:enabled", that.dialogElement).length > 0) {
                $($(":input:visible:enabled", that.dialogElement)[0]).focus();
            }
            else {
                $('.ui-dialog-titlebar-close').blur();
            }
        }

        // allow chaining
        return this;
    };

    /**
    * Changes the title
    * 
    * @param newTitle
    *            the desired title of the dialog.
    */
    this.changeDialogTitle = function (newTitle) {
        $(that.dialogElement).dialog({
            title: newTitle
        });

        // allow chaining
        return this;
    };

    /**
    * Closes the dialog.
    */
    this.closeDialog = function () {
        $(that.dialogElement).dialog('close');
    };

    //TODO review this, it seems like the old setup made it in here.
    /**
    * Clears dirty on all forms in the dialog and calls destroy on the dialog.
    * TODO added to review this, it seems lik the old setup made it in here.
    * @param inDialog old style dialog config
    */
    this.destroyDialog = function (inDialog) {
        var form = $(that.dialogElement).find('form');
        GenSession.clearDirty(form.attr("id"));
        $(inDialog.Element).dialog('destroy');
    };

    /**
    * clears all validation boxes in a dialog.
    */
    this.clearDialogValidationBoxes = function () {
        var valBox = $(" ul.validation-box", that.dialogElement);
        var options = valBox.find("li");
        options.remove();
        valBox.hide();
    };

    /** 
    * returns whether or not the dialog is open
    */
    this.isOpen = function () {
        return $(that.dialogElement).dialog('isOpen');
    };

    initializeDialog();
}

/**
* This object should not be instantied directly. The widget it belongs to
* should set it up for you.
* 
* <pre>
* Automatically disables the stateful buttons on initialization
* Automatically enables the stateful buttons on data changes
* Automatically creates validation boxes for all forms.
* Automatically creates buttons and wires up actions via configurations.
* Optionally buttons can be created and wired up manually however it is not recommended.
* Provides methods for retrieving form data
* Automatically sets correct tab orders.
* Ability to clear all form inputs.
* </pre>
* 
* @constructor
* @param inFormConfig
*            The configureation fo the form.
* @param inFormConfig.ElementID
*            The Forms ID.
* @param inFormConfig.Buttons
*            An array of button configs
* @param inFormConfig.AckMessages
*            An array of acknowlegeable-message configs
* @param inFormConfig.ContainsOCI
*            Changes the oci-note based on whether the current work area contains OCI data
* @param inFormConfig.HideOCI
*            Stops the oci-note from being added to the form.
* 
* <pre>
* for buttons that have images:
* [{ButtonClass:&quot;save-button&quot;,ButtonAction:function(buttonElement), Stateful:True}]
* for buttons without using images:
* [{ButtonClass:&quot;ies-action&quot;, ButtonText:&quot;Save&quot;, ButtonName:&quot;save-button&quot;, ButtonColor:&quot;Green&quot;,ButtonAction:function(buttonElement), Stateful:True}]
* 
* 
* NOTE: ButtonClass must only resolve to a single button, if another
* button can be identified by the class then these button definitions 
* will be cross wired. This is due to the fact that delegates must take 
* in a selector string rather than an object.
* NOTE: buttonElement is an automatically wired reference to the button 
* that called the function. It is used in things like widet.saveRequest(button) 
* to enable/disable the loading spinny as well as gain reference to the from element.
* </pre>
* 
* @param inContext
*            The context that the form belongs too, privided by the containing
*            widget.
* @returns {GenForm}
*/
function GenForm(inFormConfig, inContext) {

    /**
    * @private
    */
    var buttons = inFormConfig.Buttons;

    /**
    * @private
    */
    var ackMessages = inFormConfig.AckMessages;

    /**
    * @private
    */
    var containsOCI = inFormConfig.ContainsOCI;

    /**
    * @private
    */
    var hideOCI = inFormConfig.HideOCI;

    /**
    * @private
    */
    var onDataRetrieved = inFormConfig.OnDataRetrieved;

    /**
    * @private
    */
    var that = this;

    /*
    * Public variables.
    */

    /**
    * The jQuery object context for the form.
    */
    that.FormElement = null;

    /**
    * The id of the form.
    */
    that.formID = null;

    that.formID = inFormConfig.ElementID;

    if (inContext != null) {
        that.FormElement = $("#" + that.formID, inContext);
        if (that.FormElement.length === 0) {
            that.FormElement = $("#" + that.formID);
        }
    } else {
        that.FormElement = $("#" + that.formID);
    }

    /*
    * private methods
    */

    /**
    * @private
    */
    function initialize() {

        applyDirtyEvents();

        // attach new behavior
        if (buttons) {
            applyButtons();
        }

        that.reorderTabIndex();

        createValidationBox();

        // AFTER validation box is created
        if (ackMessages) {
            applyAckMessages();
        }

        GenSession.clearDirty(that.formID);
    }

    /**
    * @private
    */
    function createValidationBox() {
        var validationContainer = $(that.FormElement).find("ul.validation-box");
        if (validationContainer.length == 0) {
            validationContainer = $("<ul class='validation-box'></ul>");
            $(that.FormElement).prepend(validationContainer);
        }
    }

    /**
    * @private
    * @param buttonContainer
    */
    function applyOCI(buttonContainer) {
        if (!hideOCI) {
            var ociContainer = that.FormElement.find(".oci-note");

            if (ociContainer.length == 0) {
                ociContainer = $('<div class="oci-note"></div>')
                buttonContainer.append(ociContainer);
            }

            if (containsOCI) {
                ociContainer
					.html('<b>Note:</b> Must not contain any classified, export controlled or third party proprietary information.');
            } else {
                ociContainer
					.html('<b>Note:</b> Must not contain any OCI, classified, export controlled or third party proprietary information.');
            }
        }
    }

    /**
    * @private
    */
    function applyButtons() {
        // create button containers

        var buttonContainer = that.FormElement.find(".button-container");

        if (buttonContainer.length == 0) {
            buttonContainer = that.FormElement;
        }

        applyOCI(buttonContainer);
        var buttonsDiv = $('<div class="' + GenConstants.BUTTON_CONTAINER_CLASS + '"></div>');

        if (buttons.length > 0) {
            buttonContainer.append(buttonsDiv);
        }

        // for each defined button create a button element and apply classes and
        // actions
        $(buttons).each(
			function () {
			    if (typeof (this.ButtonText) === 'undefined') {
			        // create buttons with image backgrounds
			        var buttonElement = document.createElement('div');
			        var buttonAction = this.ButtonAction;
			        var additionalClasses = "";
			        if (this.Stateful) {
			            additionalClasses = "stateful_button disabled";
			        }

			        $(buttonElement).addClass("button " + this.ButtonClass + " " + additionalClasses);
			        buttonsDiv.append(buttonElement);

			        var buttonSelector = ".button."
                            + this.ButtonClass.replace(/ /g, ".") + ":not(.disabled, .loader)";

			        // onclick
			        GenSession.on('click', buttonSelector, that.FormElement, function () {
			            that.performFormAction(buttonAction, buttonElement);
			        });

			        // onEnter
			        GenSession.on('keydown', buttonSelector, that.FormElement, function (e) {
			            var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
			            // enter key to implicitly submit
			            if (keyCode == 13) {
			                that.performFormAction(buttonAction, buttonElement);
			            }
			        });
			    } else {

			        var buttonName = this.ButtonName !== 'undefined' ? this.ButtonName : this.ButtonText.replace(/ /g, '');
			        
			        // create buttons using strictly css.
			        var buttonElement = $('<button name="' + buttonName + '" type="button" class="' + this.ButtonClass + '">' + this.ButtonText + '</button>');
			        var buttonAction = this.ButtonAction;
			        var additionalClasses = [];
			        if (this.Stateful) {
			            additionalClasses.push('stateful_button');
			            buttonElement.prop('disabled', true);
			        }
			        if (typeof(this.Type) !== 'undefined') {
			            additionalClasses.push(this.Type.toLowerCase());
			        }

			        $(buttonElement).addClass(additionalClasses.join(' '));
			        buttonsDiv.append(buttonElement);

			        var buttonSelector = "button[name='" + buttonName + "']:not(.disabled, .loader)";
			        
			        // onclick
			        GenSession.on('click', buttonSelector, that.FormElement, function (e) {
			            that.performFormAction(buttonAction, buttonElement);
			        });

			        // onEnter
			        GenSession.on('keydown', buttonSelector, that.FormElement, function (e) {
			            var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
			            // enter key to implicitly submit
			            if (keyCode == 13) {
			                that.performFormAction(buttonAction, buttonElement);
			            }
			        });
			    }
			});
    }

    /**
    * @private
    * Create acknowledgeable messages
    */
    function applyAckMessages() {
        // the container for message messages is located with the form's validation display area
        var validationContainer = that.FormElement.find("ul.validation-box");
        var ackMessageContainer = $('<div class="' + GenConstants.ACK_MESSAGES_CONTAINER_CLASS + '"></div>');
        validationContainer.after(ackMessageContainer);

        // for each defined message create a message element and apply classes and actions
        $(ackMessages).each(function () {
            // assign configuration settings and/or defaults
            var messageId = this.MessageId;
            var acknowledgementHandler = this.OnAcknowledge;
            var show = (this.show == undefined) ? false : this.show;  // default = false
            var containerClass = this.ContainerClass ? this.ContainerClass : 'ack-message-box';
            var messageClass = this.MessageClass ? this.MessageClass : 'ack-message-text';
            var buttonClass = this.ButtonClass ? this.ButtonClass : 'small-close-button';

            // create the element
            var ackMessageElement =
                $('<div id="' + messageId + '" class="ack-message ' + containerClass + '">' +
                '<div class="' + messageClass + '">' + this.Message + '</div>' +
                '<div class="' + buttonClass + '"></div>' +
                '</div>');

            // add it to the DOM
            ackMessageContainer.append(ackMessageElement);

            if (show) {
                ackMessageElement.removeClass('display-none');
            } else {
                ackMessageElement.addClass('display-none');
            }

            // acknowledgement handler logic
            GenSession.registerForDelegateEvent('click', "#" + messageId + ".ack-message", function () {
                // invoke the callback handler (for any additional processing)
                acknowledgementHandler(messageId, ackMessageElement);
            }, that.FormElement);
        });
    }

    /**
    * @private
    */
    function applyDirtyEvents() {
        if (!that.FormElement.hasClass('ignore-dirty')) {

            that.FormElement.on("keydown", "input[type=text]:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function (e) {
                // avoid the key check if it is already dirty
                if (!GenSession.isDirty(that.formID)) {
                    var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                    if (!GenHelper.arrayContains(GenConstants.NONEDIT_KEYS, keyCode)) {
                        GenSession.setDirty(that.formID);
                    }
                }
            });

            that.FormElement.on("keydown", "textarea:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function (e) {
                if (!GenSession.isDirty(that.formID)) {
                    var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                    if (!GenHelper.arrayContains(GenConstants.NONEDIT_KEYS, keyCode)) {
                        GenSession.setDirty(that.formID);
                    }
                }
            });

            // WI 17760 - register for keydown AND change events to capture mouse input and other ways of setting field (i.e. <shift-insert>)
            that.FormElement.on("change", "input[type=text]:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                // avoid the key check if it is already dirty
                if (!GenSession.isDirty(that.formID)) {
                    GenSession.setDirty(that.formID);
                }
            });

            that.FormElement.on("change", "textarea:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                if (!GenSession.isDirty(that.formID)) {
                    GenSession.setDirty(that.formID);
                }
            });

            that.FormElement.on("change", "input[type=checkbox]:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                GenSession.setDirty(that.formID);
            });

            that.FormElement.on("change", "input[type=file]:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                GenSession.setDirty(that.formID);
            });

            that.FormElement.on("change", "input[type=radio]:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                GenSession.setDirty(that.formID);
            });

            that.FormElement.on("change", "select:not(" + GenConstants.IGNORE_CHANGE_INPUT_LIST_CLASSES + ")", function () {
                GenSession.setDirty(that.formID);
            });
        }
    };

    /*
    * Public methods
    */

    /**
    * Shows an acknowledgeable message.
    */
    this.showAckMessage = function (messageId) {
        var ackMessageElement = $('#' + messageId + '.ack-message');
        ackMessageElement.removeClass('display-none');
    };

    /**
    * Clears the Forms validation box.
    */
    this.clearValidationBox = function () {
        var valBox = $("ul.validation-box", this.FormElement);
        var options = $(valBox).find("li");
        options.remove();
        valBox.hide();
    };

    /**
    * Prints tab order (for debug only)
    */
    this.printTabOrder = function () {
        $(':input:not(input[type=hidden]),.button:visible,a:visible', this.FormElement).each(function () {
            var element = $(this);
            var identity;
            if ((identity = element.attr('id')) != undefined) {
            } else if ((identity = element.attr('name')) != undefined) {
            } else if ((identity = element.get(0).tagName) != undefined) {
                var additional;
                if (identity == 'A' && (additional = element.text()) != undefined) {
                } else if ((additional = element.attr('class')) != undefined) {
                } else {
                    additional = '';
                }
                identity += ' [' + additional + ']';
            } else {
                identity = 'unknown';
            }
            console.log(identity + ', tabindex=' + element.attr("tabindex"));
        });
    };

    /**
    * Performs the form's action. Allowed to be overridden
    */
    this.performFormAction = function (buttonAction, buttonElement) {
        buttonAction(buttonElement);
    };

    /**
    * Gets all data in a form as a json element ignoring placeholder values.
    */
    this.getData = function () {
        var placeHolders = $(this.FormElement).find('[placeholder]');

        // remove placeholder values
        $(placeHolders).each(function () {
            var input = $(this);
            if (input.val() == input.attr('placeholder')) {
                input.val('');
            }
        });

        var toReturn = $(this.FormElement).serializeObject();

        // add back in
        $(placeHolders).each(function () {
            var input = $(this);
            if (input.val() == '') {
                input.val(input.attr('placeholder'));
            }
        });

        if (typeof onDataRetrieved == "function") {
            onDataRetrieved(toReturn);
        }

        return toReturn;
    };

    initialize();
}

// TODO see about swaping this out for default tab order if we change the
// div.buttons to simply button
/**
* Generic tab reindex routine currently exposed but a TODO is created to see
* about making this OBE by switching div.buttons to simply button
*/
GenForm.prototype.reorderTabIndex = function (elements) {
    $('div.button:visible', this.FormElement).each(function () {
        $(this).prop("tabindex", 0);
    });
};

/**
* Sets all values to empty, places place holder text in boxes and cleans form
*/
GenForm.prototype.resetForm = function () {

    $('input:not([name=UpdateDateLong],[type=checkbox],[type=radio],.no-reset), textarea:not(.no-reset), select:not(.no-reset)', this.FormElement).each(function () {
        $(this).val('');
    });

    $('input[type=checkbox]', this.FormElement).each(function () {
        $(this).prop("checked", false);
    });

    var placeHolders = $(this.FormElement).find('[placeholder]');

    $(placeHolders).each(function () {
        var input = $(this);
        if (input.val('')) {
            input.val(input.attr('placeholder'));
            input.addClass('placeholder');
            input.removeClass('not-placeholder-value');
        }
    });

    this.clearValidationBox();
    GenSession.clearDirty(this.formID);
};

/**
* UTILITY Methods
*/

/**
* @constructor A static utility class used for common operations.
* @returns {GenHelper}
*/
function GenHelper() {
}

/**
* convert a json date directly to a MM/YYYY String
* 
* @param jsonDate
* @returns {String}
*/
GenHelper.convertJSONDateToString = function (jsonDate) {
    var fullDate = new Date(parseInt(jsonDate.substr(6)));
    var dateString = (fullDate.getMonth() + 1) + "/" + fullDate.getFullYear();
    return dateString;
};

/**
* convert a json date to a JS date
* 
* @param jsonDate
* @returns {Date}
*/
GenHelper.convertJSONDateToJSDate = function (jsonDate) {
    var jsDate = new Date(parseInt(jsonDate.substr(6)));
    return jsDate;
};

/**
* Add commas onto a string
* 
* @param nStr
*            String to add commas too
* @param prefix
*            An optional prefix to add to the string.
* @returns A string with commas and the prefix added.
*/
GenHelper.addCommas = function (nStr, prefix) {
    nStr += "";
    if (nStr.trim() != "") {
        if (prefix == null)
            prefix = "";
        nStr += '';
        x = nStr.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1)) {
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        }
        return prefix + x1 + x2;
    } else {
        return nStr;
    }
};

/**
* removes commas from a string
* 
* @param nStr
*            String to remove commas from
* @param prefix
*            An optional prefix to remove from string.
* @returns A string with comms and the prefix removed.
*/
GenHelper.removeCommas = function (nStr, prefix) {

    nStr += "";
    nStr = nStr.replace(/\,/g, '');

    if (prefix != null) {
        nStr = nStr.replace(/prefix/g, "");
    }

    return nStr;
};

/**
* applies special encoding to avoid jquery name issues.
* 
* @param value
*            value to encode
* @returns a jquery safe string.
*/
GenHelper.htmlEncodeForjQuery = function (value) {

    var escaped = value;
    var findReplace = [[/\"/g, ''], [/\'/g, ""], [/\./g, "\\."], [/\!/g, "\\!"], [/\#/g, "\\#"],
			[/\$/g, "\\$"], [/\%/g, "\\%"], [/\&/g, "\\&"], [/\(/g, "\\("], [/\)/g, "\\)"], [/\*/g, "\\*"],
			[/\+/g, "\\+"], [/\,/g, "\\,"], [/\//g, "\\/"], [/\:/g, "\\:"], [/\;/g, "\\;"], [/\?/g, "\\?"],
			[/\@/g, "\\@"], [/\^/g, "\\^"], [/\`/g, "\\`"], [/\{/g, "\\{"], [/\}/g, "\\}"], [/\~/g, "\\~"],
			[/\|/g, "\\|"]];

    for (var item in findReplace) {
        escaped = escaped.replace(findReplace[item][0], findReplace[item][1]);
    }

    return escaped;
};

/**
* Checks if the array contains
* 
* @param array
*            array to check.
* @param value
*            value to search for.
* @returns a boolean if the array contains the supplied value
*/
GenHelper.arrayContains = function (array, value) {
    var arrayLength = array.length;
    for (var index = 0; index < arrayLength; index++) {
        if (array[index] == value) {
            return true;
        }
    }
    return false;
};

/**
* encodes a string into html
* 
* @param value
*            value to encode
* @returns {string} an html encoded String
*/
GenHelper.htmlEncode = function (value) {
    value += "";
    // first replace < and > with &lt; and &gt; .. the browser will 'fix' this
    value = value.replace(new RegExp("<", "g"), '&lt;');
    value = value.replace(new RegExp(">", "g"), '&gt;');

    // now replace newlines with <br/>. the browser will fix this as well to
    // display the newlines
    value = value.replace(/(\r\n|\n|\r)/gm, '<br/>');

    return $('<div/>').text(value).html();
};

/**
* decodes html into a string
* 
* @param inValue
*            value to decode
* @returns {string} an html stripped String
*/
GenHelper.htmlDecode = function (inValue) {
    return $('<div/>').html(inValue).text();
};

// TODO see about unprototypeing this so it will be truly static.
/**
* @param d
*            Date to check
* @returns if date is a valid date object.
*/
GenHelper.prototype.isValidDate = function (d) {
    if (Object.prototype.toString.call(d) !== "[object Date]")
        return false;
    return !isNaN(d.getTime());
};

/**
* JS Extentions
*/

/**
* Find number of months between two dates
* 
* @param d
*            date to compare against.
* @returns number of months between the two dates.
*/
Date.prototype.getMonthsBetween = function (d) {
    // returns months between dates
    var d1 = this.getFullYear() * 12 + this.getMonth();
    var d2 = d.getFullYear() * 12 + d.getMonth();
    return d2 - d1;
};

/**
* Adds the number of months to the date. If the date is beyond what the next
* month contains then it will be down graded to the last day of the month.
* 
* @param m
*            months to adjust the Date by.
*/
Date.prototype.addMonths = function (m) {
    // clone the date
    var clonedate = this.getDate();
    // move the date cursor to next month
    this.setMonth(this.getMonth() + m);
    // month adjustment if clone date has more days than new month
    if (this.getDate() < clonedate)
        this.setDate(0);
};

/**
* Formats Date to a MM/YYYY String
* 
* @returns Formatted Date to a MM/YYYY String
*/
Date.prototype.toFormattedString = function (includeLeadingZero) {
    //default value is false
    includeLeadingZero = typeof includeLeadingZero !== 'undefined' ? includeLeadingZero : false;

    var toReturn = this.getMonth() + 1 + "/" + this.getFullYear();
    if (includeLeadingZero == true && this.getMonth() < 9) {
        // < 9 because month starts at 0 and is incremented by 1
        toReturn = "0" + toReturn;
    }
    return toReturn;
};

/**
* Copy of a nullified precision up to the date place.
* 
* @returns Nullified precision date.
*/
Date.prototype.dateOnly = function () {
    return new Date(this.getFullYear(), this.getMonth(), this.getDate());
};

/**
* Copy of a nullified precision up to the month place.
* 
* @returns Nullified precision to month.
*/
Date.prototype.monthPrecision = function () {
    // use a date in the middle of most months so we don't have to worry about
    // DLS or TZs
    return new Date(this.getFullYear(), this.getMonth(), 15);
};

/**
* returns the number of occurances of a subString
* 
* @param s1
*            the string to count.
* @returns the number of occurances of s1 in string
*/
String.prototype.count = function (s1) {
    return (this.length - this.replace(new RegExp(s1, "g"), '').length) / s1.length;
};

/**
* Returns a full date string
* 
* @returns a full date string
*/
String.prototype.ensureProperDate = function () {
    if (this.match(/^((0?[1-9]|1[012])\/((19|20)\d\d))$/)) {
        return this.substr(0, this.indexOf('/')) + '/15' + this.substr(this.indexOf('/'));
    }

    return this.valueOf();
};

/**
* Checks and converts a string to a date. otherwise returns epoch
* 
* @returns Checks converts a string to a date. otherwise returns epoch
*/
String.prototype.toDate = function () {
    if (this.isDate()) {
        var properDate = this.ensureProperDate();
        var toreturn = new Date(properDate);
        return toreturn;
    }

    return new Date(0);
};

/**
* if the String matches the pattern for a date. MM/DD/YYYY or MM/YYYY
* 
* @returns if the String matches the pattern for a date. MM/DD/YYYY or MM/YYYY
*/
String.prototype.isDate = function () {
    return this
			.match(/^((0?[1-9]|1[012])\/(0?[1-9]|[12][0-9]|3[01])\/((19|20)\d\d))$|^((0?[1-9]|1[012])\/((19|20)\d\d))$/);
};

/**
* Checks if the String value is the boolean for true
* 
* @returns Checks if the String value is the boolean for true "true".isTrue() =
*          true
*/
String.prototype.isTrue = function () {
    var matches = $.trim(this).toLowerCase().match(/^true$/);
    return matches != null && matches.length > 0;
};

/**
* Checks if a string ends with the substring.
* 
* @param text
*            substring to look for.
* @returns Checks if the String end with a substring
*/
String.prototype.endsWith = function (text) {
    var expectedIndex = this.length - text.length;
    var index = this.indexOf(text, expectedIndex);
    if (index == expectedIndex) {
        return true;
    } else {
        return false;
    }
};

/**
* Checks if the String begins with a substring
* 
* @param text
*            substring to look for.
* @returns Checks if the String begins with a substring
*/
String.prototype.beginsWith = function (text) {
    var expectedIndex = 0;
    var index = this.indexOf(text, expectedIndex);
    if (index == expectedIndex) {
        return true;
    } else {
        return false;
    }
};

/**
* See's if a String contains a substring.
* 
* @param text
*            text to check for.
* @returns Checks if the String contains a substring
*/
String.prototype.contains = function (text) {
    var index = this.indexOf(text);
    if (index != -1) {
        return true;
    } else {
        return false;
    }
};

/**
* Trims the string
* 
* @returns Trimed string
*/
String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, '');
};

/**
* Rounds number away from zero
* 
* @returns a rounded number away from zero
*/
Number.prototype.roundAwayFromZero = function () {
    if (this > 0) {
        return Math.round(this);
    } else {
        return (-1 * Math.round(-1 * this));
    }
};

/* Using this to workaround documenting the JQuery Plugins*/
/**
* @namespace jQuery
* @exports $.fn as jQuery
*/
$.fn = $.fn;

/**
* @function
* Creates a json object consisting of the form elements example useage:
* $(Form).serializeObject()
* 
* @returns a json object of a form's data.
*/
$.fn.serializeObject = function () {
    var o = {};

    // find any tables and disable the inputs
    var allTables = this.find('table:has(tr[pkid]:not(.blank))');
    var currentItem = this.find(':focus');

    // performance upgrade from "var allActiveInputs = allTables.find('td :input:not(:disabled)');"
    var allActiveInputs = [];
    allTables.each(function () {
        $.merge(allActiveInputs, $(this).find('td :input:not(:disabled)'));
    });

    $(allActiveInputs).each(function () {
        $(this).prop('disabled', true);
    });

    var a = this.serializeArray();

    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });

    // enable table inputs and store the table data as an array of objects.
    $(allActiveInputs).each(function () {
        $(this).prop('disabled', false);
    });

    if (allTables.length > 0) {
        allTables.each(function () {
            var table = $(this);
            var name = table.attr("name");
            if (name == undefined) {
                throw ("Error attempting to serialize the data in a table; table is missing the 'name' attribute.");
            }
            o[name] = [];

            table.find('tr[pkid]:not(.blank)').each(function () {
                var rowValues = $(this).serializeRow();

                // push the object into the array based on the table name.
                o[name].push(rowValues);
            });
        });
    }

    return o;
};

$.fn.serializeRow = function () {
    var rowValues = {};

    $(this).find(':input[name]').each(function () {
        if ($(this).attr('type') != 'radio' || ($(this).attr('type') == 'radio' && $(this).prop('checked') == true)) {

            var inputName = $(this).attr('name');
            var inputValue = $(this).val();

            // check radio button attribute
            if ($(this).attr('mvprop')) {
                inputName = $(this).attr('mvprop');
            }

            // check for duplicate names on row values
            var nameExists = rowValues.hasOwnProperty(inputName) || ($.inArray(inputName, rowValues) > -1);
            if (nameExists) {
                // is it already an array?
                var currValue = rowValues[inputName];
                var isArray = $.isArray(currValue);

                if (isArray) {
                    currValue.push(inputValue);
                    rowValues[inputName] = currValue;
                } else {
                    // redefine as an array
                    var newInputValue = [];
                    newInputValue.push(currValue);
                    newInputValue.push(inputValue);
                    rowValues[inputName] = newInputValue;
                }
            } else {
                rowValues[inputName] = inputValue;
            }
        }
    });

    return rowValues;
};

/**
* Clones the jsonObj sent in.
* 
* @param obj
*            obj to clone.
* @returns a clone of the object.
*/
JSON.cloneData = function (obj) {
    var stringValue = JSON.stringify(obj);
    return JSON.parse(stringValue);
};

/**
* Clones the jsonObj you sent in and returns an object with all values in the
* String format ( Non destructive leave original object alone)
* 
* @param inJsonObj
*            the object to cloned then modified.
* @returns cloned jsonObject.
*/
JSON.cloneDataAsStrings = function (inJsonObj) {
    var jsonObj = JSON.cloneData(inJsonObj);
    return JSON.changeDataToStrings(jsonObj);
};

/**
* Changes the jsonObj you sent in to a format all values into Strings
* (Destructive modifies original object)
* 
* @param jsonObj
*            the object to modify.
* @returns modifed jsonObject.
*/
JSON.changeDataToStrings = function (jsonObj) {
    if (typeof jsonObj == "object") {
        $.each(jsonObj, function (k, v) {
            // k is either an array index or object key
            if (jsonObj[k] != null) {
                jsonObj[k] = JSON.changeDataToStrings(jsonObj[k]);
            }
        });
    } else {
        // jsonOb is a number or string
        jsonObj = jsonObj + "";
    }

    return jsonObj;
};

/**
* Adds to the Jquery support object allows us to check if it supports it.
*/
jQuery.support.placeholder = (function () {
    var i = document.createElement('input');
    return 'placeholder' in i;
})();

/**
* Still not sure what this does.
* 
* @deprecated
* @param value
* @param precision
* @returns {String}
*/
function toFixed(value, precision) {
    var power = Math.pow(10, precision || 0);
    return String(Helper.roundAwayFromZero(value * power) / power);
}


/**
* @function
* jquery override due to deprecation.  jquery recommends doing exactly this
* $(div).gentoggle(fn, fn2)
* 
* see: 
* http://bugs.jquery.com/ticket/11786
* "It's not really an appropriate core function. If someone has code that uses it already 
*  the Migrate plugin will let them continue to do so without changing any code. 
*  The function with another name would still have the drawbacks it does today, 
*  including not working with delegation and having no obvious way to remove it. 
*  If someone wants the code it's there in the Migrate plugin and can be extracted."
* 
* http://stackoverflow.com/questions/14338078/equivalent-of-deprecated-jquery-toggle-event
*/
jQuery.fn.gentoggle = function (fn, fn2) {
    // Don't mess with animation or css toggles
    if (!jQuery.isFunction(fn) || !jQuery.isFunction(fn2)) {
        return oldToggle.apply(this, arguments);
    }
    // migrateWarn("jQuery.fn.toggle(handler, handler...) is deprecated");
    // Save reference to arguments for access in closure
    var args = arguments,
    guid = fn.guid || jQuery.guid++,
    i = 0,
    toggler = function (event) {
        // Figure out which function to execute
        var lastToggle = (jQuery._data(this, "lastToggle" + fn.guid) || 0) % i;
        jQuery._data(this, "lastToggle" + fn.guid, lastToggle + 1);
        // Make sure that clicks stop
        event.preventDefault();
        // and execute the function
        return args[lastToggle].apply(this, arguments) || false;
    };
    // link all the functions, so any of them can unbind this click handler
    toggler.guid = guid;
    while (i < args.length) {
        args[i++].guid = guid;
    }
    return this.click(toggler);
};

window.addEventListener('message', function (e) {
    var iframe = $(".bannerframe");
    var eventName = e.data[0];
    var data = e.data[1];
    switch (eventName) {
        case 'setHeight':
            iframe.height(data);
            break;
    }
}, false);