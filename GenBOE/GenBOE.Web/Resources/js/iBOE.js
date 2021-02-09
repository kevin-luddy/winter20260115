
/*Reserved words
* When using a specific String to mark a class or attr for some function we need to define these Strings as reserved and know we can't use them anywhere else.  If we need to use them then just rename the variable here.
*/

//used when creating a new row, appended to name so that validation doesn't pick it up.
var blankElementReservedWord = "-blank";


/* PAGE MANAGER
----------------------------------------------------------*/
///<summary>Page Manager is used for any global features of the site, for example marking a page as dirty once data has changed.</summary>
function PageManager() {

	/*
	* private variables
	*/
	//an array of dirty data
	var dirty = [];
	var eventBusRegistry = [];
	var genBoeURL = document.location.href;
	/*
	* public variables and methods
	*/
  
  
	//Manages the registering and unregistering of events.
   

	this.printDirty = function(){
		var dirtyString="";

		for(var y in dirty)
		{   
			if(dirty[y]==true)
			{
				dirtyString+="\n"+y;
			}
		}

		console.log(dirtyString);
		
	};

	this.setDirty = function (formName) {
		dirty[formName] = true;

		var saveButton = findClosestSaveButtons(formName);

		//if any are found then activate them.
		if ($(saveButton).length != 0) {
			$(saveButton).removeClass("disabled");
			$(saveButton).prop('disabled', false);
		}
	}

	this.clearDirty = function (formName) {
		dirty[formName] = false;

		var saveButton = findClosestSaveButtons(formName);
		//if any are found then deactivate them.
		if ($(saveButton).length != 0) {
			$(saveButton).addClass("disabled");
		}

		//TODO possiblly clear all validation
		// 31595
	}

	this.isDirty = function (formName) {
		var found = false;
		
		if (formName == undefined) {
			///<summary>returns true if any form's dirt bit is set to true</summary>
			for (i in dirty) {
				if (dirty[i] == true) {
					found = true;
					break;
				}
			}
		}
		else {
			found = dirty[formName] != undefined && dirty[formName] == true;
		}

		return found;
	}

	this.getDirtyLength = function () {
		return dirty.length;
	}

	// use jquery ui's toolkit class to fancy-up the tooltip itself
	$(document).tooltip({
		show: { effect: 'slideDown', delay: 250 },
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

	/**
	* @private (check to see if hashchange event is supported by browser.  IE7 did not support this and therefore fails)
	*/
	var isValidBrowser = Modernizr.hashchange;

	this.isValidBrowser = function () {
	  return isValidBrowser;
	}
   
	///<summary>Registers Event if not already bound</summary>
	this.registerForEvent = function (eventName, eventOwner, functionToBind) {
		if (eventBusRegistry[eventName + eventOwner] == null) {
		   eventBusRegistry[eventName + eventOwner] = "BOUND";
			$(document).bind(eventName, functionToBind);
		}
	}
   
	///<summary>Unregisters Event if already bound</summary>
	this.unregisterForEvent = function (eventName, eventOwner, functionToBind) {
		if (eventBusRegistry[eventName + eventOwner] == "BOUND") {
			eventBusRegistry[eventName + eventOwner] = null;
			if (functionToBind != undefined) {
				$(document).unbind(eventName, functionToBind);
			}
			else {
				$(document).unbind(eventName);
			}
		}
	}

	this.registerForDelegateEvent = function (eventName, eventOwnerSelector, functionToBind, context) {
		$(context).undelegate(eventOwnerSelector, eventName);
		$(context).delegate(eventOwnerSelector, eventName, functionToBind);
	}

	this.commonDialog = function (title, body, buttons, width) {

		$("#CommonDialog .buttons").text("");

		if (buttons != null) {
			for (x in buttons) {
				if (typeof (buttons[x].ButtonText) === 'undefined') {
					$("#CommonDialog .buttons").append('<div class="' + buttons[x].buttonClass + '"></div>');
					if (buttons[x].callbackMethod != null) {
						$("#CommonDialog .buttons ." + buttons[x].buttonClass).click(buttons[x].callbackMethod);
					}
					$("#CommonDialog .buttons ." + buttons[x].buttonClass).click(this.closeCommonDialog);
				} else {
					var buttonName = buttons[x].ButtonName !== 'undefined' ? buttons[x].ButtonName : buttons[x].ButtonText.replace(/ /g, '');
					$("#CommonDialog .buttons").append('<button class="' + buttons[x].buttonClass + '" name="' + buttonName + '" type="button">' + buttons[x].ButtonText + '</button>');
					if (buttons[x].callbackMethod != null) {
						$("#CommonDialog button[name='" + buttonName + "']").click(buttons[x].callbackMethod);
					}
					$("#CommonDialog button[name='" + buttonName + "']").click(this.closeCommonDialog);
				}
			}
		}
		$("#CommonDialogBody").html(body);
		$("#CommonDialog").dialog({ width: 450, minHeight: 50, modal: true, resizable: false, draggable: true, title: title });
	}

	this.closeCommonDialog = function () {
		$('#CommonDialog').dialog('close');
	}

	this.alertDialog = function (title, alert, okCallback) {
		this.commonDialog(title, alert, [{ buttonClass: "ies", ButtonText: 'OK', ButtonName: 'ok-button', callbackMethod: okCallback}], 450);
	}

	this.confirmDialog = function (title, body, acceptCallback, cancelCallback) {
		this.commonDialog(title, body, [{ buttonClass: "ies", ButtonText: 'Yes', ButtonName: 'yes-button', callbackMethod: acceptCallback }, { buttonClass: "ies", ButtonText: 'No', ButtonName: 'no-button', callbackMethod: cancelCallback}], 450);
	}

	this.focusValidation = function () {
		var target = $('.validation-box:visible');
		if (target.length != 0) {
			$('html, body').animate({ scrollTop: $(target).offset().top }, 200);
		}
	};

	/*
	*Private Methods
	*/
	function findClosestSaveButtons(elementID) {
		//assume the save buttons are within the form.
		var saveButtons = $("#" + elementID + " .buttons button[name='save-button']");

		//if not in the first form see if you can find one in any parent divs.
		if ($(saveButtons).length == 0) {
			var forms = $("#" + elementID).parents('div, form').each(function () {
				var buttons = $(this).find(".buttons button[name='save-button']");

				if (buttons.length != 0) {
					saveButtons = buttons;
					return false;
				};
			});
		}

		// AMD - We don't need to throw an exception for a missing Save button. Dirty bit can be used
		// for things besides activating and deactivating the save, so we don't want to limit it's functionality
		// to forms that have an explicit save button.
		////if not in the form throw exception
		//if ($(saveButton).length == 0) {
		//     throw "No save button found for this element:" + elementID;
		//}
		return saveButtons;
	}

	this.setContextSensitiveHelp = function(url)
	{
		$("#PageHelpMenu a").attr("href", url);
	}
}

//instance 
var Session = new PageManager();

/* WIDGET
----------------------------------------------------------*/

///<summary>BOE Widget</summary>
function Widget(inWidgetName, inIsReadOnly) {
	//the data you wish to update.
	this.data = {};

	this.forms = new Array();
	//name of widget
	this.widgetName = inWidgetName;
	// Read-only page option
	this.readOnly = false;

	var that=this;

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

	this.setDirty = function (formName) {
		if (formName == null || formName.type != undefined) formName = that.widgetName;
		$(document).trigger('DATA_DIRTY', formName);
	}

	this.cleanDirty = function (formName) {

		if (formName == null) {
			formName = that.widgetName;

			for (var n in this.forms) {
				$(document).trigger('DATA_CLEANED', this.forms[n]);
			}

			var form = $('#' + formName).find('form');
		}

		$(document).trigger('DATA_CLEANED', formName);

		this.refreshModule();
	}

	this.isDirty = function () {
		var thisIsDirty = false;
		for(var n in this.forms)
		{
			if(Session.isDirty(this.forms[n]))
			{
			 thisIsDirty=true;
			}
		}

		if(Session.isDirty(that.widgetName))
		{
			thisIsDirty = true;
		}

		return thisIsDirty;
	}

	if (inIsReadOnly != null) {
		this.setReadOnly(inIsReadOnly);
	}

	//TODO refactor this out once all widgets call afterDOmLoad
	//31598 same on line 351
	this.wireUpActions();
}

Widget.prototype.aquireForms = function(){
   var that = this;
   $("#"+this.widgetName+" form").each(function(){
		that.forms.push($(this).attr("id"));
	});
}

Widget.prototype.afterDOMLoad = function(){
   this.aquireForms();
   this.wireUpActions();
}

Widget.prototype.wireUpActions = function () { 

	var ignoreInputList = ".filter-textbox, .ui-autocomplete-input, .paging-control input, .ignore-dirty"
	var that = this;
	var fromName = new String();
	
	for (var i = 0; i < that.forms.length; i++)
	{
		n = that.forms[i];
		if (!$(n).hasClass('.ignore-dirty')) {
			
			var context = $('#' +that.forms[n]);
			
			this.registerForDelegateEvent("keydown", "input[type=text]:not("+ignoreInputList+")", function(){
				that.setDirty($(this).parents("form").attr("id"));
			}, context);

			this.registerForDelegateEvent("keydown", "textarea:not("+ignoreInputList+")", function(){
				that.setDirty($(this).parents("form").attr("id"));
			}, context);

			this.registerForDelegateEvent("paste", "input[type=text]:not(" + ignoreInputList + ")", function () {
				that.setDirty($(this).parents("form").attr("id"));
			}, context);

			this.registerForDelegateEvent("paste", "textarea:not(" + ignoreInputList + ")", function () {
				that.setDirty($(this).parents("form").attr("id"));
			}, context);

			this.registerForDelegateEvent("change", "input[type=checkbox]:not("+ignoreInputList+")", function(){
				that.setDirty($(this).parents("form").attr("id"));
			}, context);
		 
			this.registerForDelegateEvent("change", "input[type=radio]:not("+ignoreInputList+")", function(){
				that.setDirty($(this).parents("form").attr("id"));
			}, context);

			this.registerForDelegateEvent("change", "select:not("+ignoreInputList+")", function(){
				that.setDirty($(this).parents("form").attr("id"));
			}, context);
		}
	}
	
	//TODO refactor this out once all widgets call afterDOmLoad
	//31598 same on line 291
	if(this.forms.length==0 && this.widgetName != undefined )
	{
		var widgetContext=$("#" + this.widgetName);
		var formSelector;

		//widget names are sometime realitive to a container and sometimes the form. we will pick one as a best practice later.
		if($(widgetContext).is("form"))
		{
			formSelector = "form#" + this.widgetName+ ":not(.ignore-dirty)";
		}else{
			formSelector = "#" + this.widgetName + " form:not(.ignore-dirty)";
		}
		
		var form = $(formSelector);

		if (form.length > 0) {

			this.registerForDelegateEvent("keydown", "input[type=text]:not("+ignoreInputList+")", function(){
				Session.setDirty(form.attr("id"));
			}, form);

			this.registerForDelegateEvent("keydown", "textarea:not("+ignoreInputList+")", function(){
				Session.setDirty(form.attr("id"));
			}, form);

			this.registerForDelegateEvent("change", "input[type=checkbox]:not("+ignoreInputList+")", function(){
				Session.setDirty(form.attr("id"));
			}, form);

			this.registerForDelegateEvent("change", "input[type=radio]:not("+ignoreInputList+")", function(){
				Session.setDirty(form.attr("id"));
			}, form);

			this.registerForDelegateEvent("change", "select:not("+ignoreInputList+")", function(){
				Session.setDirty(form.attr("id"));
			}, form);
		}
   }
   
};

//This method is provided if the widget it loaded after the page onload function is called. it is just a copy of the MS code that does this onLoad.
Widget.applyValidation = function () {
//    var allFormOptions = window.mvcClientValidationMetadata;
//    if (allFormOptions) {
//        while (allFormOptions.length > 0) {
//            var thisFormOptions = allFormOptions.pop();
//            __MVC_EnableClientValidation(thisFormOptions);
//        }
//    }

	
};

//TODO refactor to give this scope
//31597
/********** Element lookup based on ID or name **********/
 function lookup_element(elm) {

	var obj = null;
	var lkup_id = $(elm);
	var lkup_name = $('input[name="' + elm + '"]');

	if(lkup_id != undefined)
		obj = lkup_id;
	else if(lkup_name != undefined)
		obj = lkup_name;

	return obj;
 }

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
 Widget.prototype.performCheck = function (input, output, options, message) {
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
			 if (response > 0) {

				 $(output).val(response);
			 } else {

				 if (message) {
					 $(output).val("-1");
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
			 }

		 },
		 error: message || function () {
			 console.log("error");
		 },
		 complete: function (response) {
		 }
	 });
 };

 Widget.prototype.focusValidation = function () {
	 var target = $('#' + this.widgetName + ' .validation-box:visible');
	 if (target.length != 0) {
		 $('html, body').animate({
			 scrollTop: $(target).offset().top
		 }, 200);
	 }
 };


/*********** Generic tab index routine **********/
Widget.prototype.reorderTabindex = function (formName, elements) {

  var tabindex = 0;
  var i = 0; 
  var formname = (formName != null) ? formName: this.widgetName; 

  $( "#" + formname + " input, select").each( function() {                
		  $(this).prop("tabindex", ++tabindex); 
   }); 

   //append elements if provided - optional
   if(arguments.length == 2 && 
		  elements != null ){  
   for (i in elements) { 
	   var $elem = lookup_element(formname + ' #' + elements[i]);
		$($elem).prop("tabindex", tabindex++);
	}
  }};

Widget.prototype.preparedForSubmit = function () {
	///<summary>This should be over written by what ever you might need to do to your client data before submitting to the server (validate, Type Cast)</summary>
	return true;
};

Widget.prototype.clearValidationBox = function (inValBox) {
	var valBox = null;

	if (inValBox) {
		valBox = $(inValBox);
	} else {
		valBox = $("#" + this.widgetName + " ul.validation-box");
	}

	var options = $(valBox).find("li");
	options.remove();
	$(valBox).hide();
}

// TODO: 1. Remove Widget.prototype.ajaxRequest and rename this function to be called Widget.prototype.ajaxRequest.
//       2. Search through code for all the places that use Widget.prototype.ajaxRequest and remove the dataType so 
//          it will default to json.  Currently the way the existing ajaxRequest function is written, the dataType is
//          overwritten to json.
//       3. Change the call in BOELaborController.cs to call the new ajaxRequest.  Make sure to leave the dataType as html.
//       4. Replace "dataType: dataTypeResolved" with "dataType: options.dataType || 'json'".
// 31600
Widget.prototype.ajaxUpdatedRequest = function (options, button) {
	options.dataTypeForced = options.dataType;
	Widget.prototype.ajaxRequest(options, button);
};

Widget.prototype.ajaxRequest = function (options, button) {
	var dataTypeResolved = (options.dataTypeForced === undefined) ? 'json' : options.dataTypeForced;  // see: ajaxUpdatedRequest (above)
	var that = this;
	$.ajax({
		type: 'POST',
		url: options.url,
		contentType: 'application/json; charset=utf-8',
		dataType: dataTypeResolved,  // see: ajaxUpdatedRequest (above)
		data: options.data,
		success: options.success,
		error: function (jqXHR, textStatus, errorThrown) {
			var response = jqXHR.responseText;
			var error = {};
			if (!(response.indexOf('{') < 0 || response.indexOf('{') > 2)) {
				error = $.parseJSON(response);

				if (error.ReturnType == "GenValidationException") {
					that.processValidationErrors(error.MessageList, button);
					}

				options.error(jqXHR, textStatus, errorThrown);
			}
		}
	});
};

/**
* Display validation errors.
* 
* @param errorList   - an array of IES.Common.Exceptions.ValidationMessage
* @param formElement - (optional) any element located on the validation display area's form
*/
Widget.prototype.processValidationErrors = function (errorList, formElement) {

					//First clear all validation boxes that are in that widget.
	var that = this;
	var AllValBoxes = $("#" + that.widgetName + " ul.validation-box");

					$(AllValBoxes).each(function () {
						that.clearValidationBox(this);
					});

					//All boxes that are being targeted so we can animate them once all messages are added.
					var valBoxes = [];
					var DefaultValBox = $("#" + that.widgetName + " ul.validation-box")[0];

	if (formElement) {
		DefaultValBox = $(formElement).parents("form").find("ul.validation-box")[0];
	}

	for (validationError in errorList) {
		if (errorList.hasOwnProperty(validationError)) {

			if (errorList[validationError].FormIDToTarget != null && errorList[validationError].FormIDToTarget != "") {

				var formName = "#" + that.widgetName + " form#" + errorList[validationError].FormIDToTarget;                
				var FormToTargetForAMessage = $(formName);
				
				if (FormToTargetForAMessage.length == 0) {
					formName = "form#" + errorList[validationError].FormIDToTarget;
					FormToTargetForAMessage = $(formName);
				}

				if (formName == '#TaskElementsComposite form#LaborSpreadForm') {
					// Change the Labor Spread Section to be expanded if collapsed
					$('#ManageLaborSpread.collapsed .collapsible-header').click();
				}

				var TargetedValBox = FormToTargetForAMessage.find("ul.validation-box")[0];

				if (!Helper.ArrayContains(TargetedValBox, valBoxes)) {
					valBoxes.push(TargetedValBox);
				}

				$(TargetedValBox).append("<li>" + errorList[validationError].ValidationIssue + "</li>");

			} else {

				if (!Helper.ArrayContains(DefaultValBox, valBoxes)) {
					valBoxes.push(DefaultValBox);
				}
				//append to default
				$(DefaultValBox).append("<li>" + errorList[validationError].ValidationIssue + "</li>");
			}
		}
	}

	$(valBoxes).fadeIn(500);
	that.refreshModule();
	that.focusValidation();
};

Widget.prototype.ToggleWorkspaceDetails = function (workspaceDiv) {
	var dialog = $(workspaceDiv).next();
	dialog.toggle();
};

Widget.prototype.ToggleHelp = function (helpButton, side, modifyTop) {
	var helpDialog = $(helpButton).next();

	//modifyTop defaults to true - raises the position of the top of the dialog
	if (modifyTop == undefined || modifyTop == true) {
		helpDialog.css("top", $(helpButton).position().top - helpDialog.height());
	}

	if (side != undefined && side.toLowerCase() == 'left') {        
		helpDialog.css("left", $(helpButton).position().left - helpDialog.width() - 20);  
	}
	else {
		helpDialog.css("left", $(helpButton).position().left + $(helpButton).width() + 5);        
	}

	helpDialog.toggle("drop");
};

Widget.prototype.InitializeDialog = function(dialog) {

	var that=this;
	// Remove old dialogs left behind when jumping between jump pages
	var dialogID = dialog.Element.attr('id');
	if (dialogID != undefined)
	{
		$('body .ui-dialog').children('#' + dialogID).parent().remove();
		$('body').children('#' + dialogID).remove();
	}

	// Remove display-none class
	if (dialog.Element.hasClass('display-none')) {
		dialog.Element.removeClass('display-none');
	}

	// Set the dialog disabled so that it doesn't open when initialized
	if (dialog.Params.autoOpen == undefined) {
		dialog.Params.autoOpen = false;
	}
	if (dialog.Params.close == undefined) {
		dialog.Params.close = function(){ 
			var form = $(dialog.Element).find('form');
			that.cleanDirty(form.attr("id"));
		};
	}

	// Initialize the dialog
	$(dialog.Element).dialog(dialog.Params);
};

Widget.prototype.OpenDialogAfterInitialize = function(dialog) {
	var form = $(dialog.Element).find('form');
	$(dialog.Element).dialog('open');
};

Widget.prototype.OpenDialog = function(dialog) {

	var form = $(dialog.Element).find('form');
	$(dialog.Element).dialog(dialog.Params);
}

Widget.prototype.ChangeDialogTitle = function(dialog, newTitle) {
	$(dialog.Element).dialog({title : newTitle});
}

Widget.prototype.CloseDialog = function(dialog) {
	$(dialog.Element).dialog('close');
}

Widget.prototype.DestroyDialog = function(dialog) {
	var form = $(dialog.Element).find('form');
	this.cleanDirty(form.attr("id"));
	$(dialog.Element).dialog('destroy');
}

Widget.prototype.registerForEvent = function (eventName, functiontoBind) {
	Session.registerForEvent(eventName, this.widgetName, functiontoBind);
};

Widget.prototype.unregisterForEvent = function (eventName, functiontoBind) {
	Session.unregisterForEvent(eventName, this.widgetName, functiontoBind);
};

Widget.prototype.registerForLiveEvent = function (eventName, eventOwnerSelector, functiontoBind) {
	// using document because things like dialogs show up w/in body
	$(document).off(eventName, eventOwnerSelector);
	$(document).on(eventName, eventOwnerSelector, null, functiontoBind);
};

Widget.prototype.registerForDelegateEvent = function(eventName, eventOwnerSelector, functionToBind, context) {
	if (context == null) {
		context = $('#' + this.widgetName);
	}
	Session.registerForDelegateEvent(eventName, eventOwnerSelector, functionToBind, context);
}

Widget.prototype.setReadOnly = function (inIsReadOnly) {
	this.readOnly = inIsReadOnly;
};

Widget.prototype.isReadOnly = function () {
	return this.readOnly;
};

Widget.prototype.applyReadOnly = function () {
	// If this widget is read-only, we need to replace inputs with DIVS and hide buttons
	if (this.isReadOnly()) {
		// Get all inputs, textareas and selects within the Widget's container
		var widgetFields = $("#" + this.widgetName + " input, " + "#" + this.widgetName + " textarea, " + "#" + this.widgetName + " select");

		// Iterate over each of the fields
		widgetFields.each(function (index) {
			var currentField = $(this);
			if (currentField.attr("type") != "hidden" && currentField.css("visibility") != "hidden"
					&& currentField.css("display") != "none" && !currentField.hasClass("display-none")
					&& !currentField.hasClass("filter-textbox") && !currentField.hasClass("filter-dropdown")
					&& currentField.attr("type") != "radio") {
				var replacementFieldText = "";

				if (currentField.get(0).tagName == "SELECT") {
					replacementFieldText = Widget.prototype.htmlEncode($.trim(currentField.children("option:selected").text()));
				} else if (currentField.is(':checkbox')) {
				} else {
					// Convert line breaks (\n) to <br/> and encode special xml characters, e.g. <, >, etc. to &lt;, &gt;, etc. 
					replacementFieldText = Widget.prototype.htmlEncode(currentField.val());
					// Decode &lt;, &gt;, etc. back to <, >, etc.
					replacementFieldText = Widget.prototype.htmlDecode(replacementFieldText);
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

			if (currentField.attr("type") != "hidden" &&
				!currentField.hasClass("filter-textbox") &&
				!currentField.hasClass("filter-dropdown")) {
				currentField.addClass('display-none');
			}

			if (currentField.attr("type") == "radio"){
				currentField.removeClass('display-none');
				currentField.prop('disabled', true);
			}
		});

		this.hideButtons();

		// Hide grid delete buttons
		$("#" + this.widgetName + " td .delete").addClass('display-none');
		$("#" + this.widgetName + " th .delete").addClass('display-none');
		$("#" + this.widgetName + " tr.blank").addClass('display-none');
		var headers = $("#" + this.widgetName + " th:visible");
		$(headers[headers.length - 1]).addClass('last-child');

		// Remove grid events
		$("#" + this.widgetName + " tbody td").unbind('click');
		$("#" + this.widgetName + " tbody td a").unbind('click');
	}
}

Widget.prototype.htmlEncode = function(value) {
  // first replace < and > with &lt; and &gt; .. the browser will 'fix' this
  value = value.replace(new RegExp("<", "g"), '&lt;');
  value = value.replace(new RegExp(">", "g"), '&gt;');

  // now replace newlines with <br/>.  the browser will fix this as well to display the newlines
  value = value.replace(/(\r\n|\n|\r)/gm,'<br/>');

  return $('<div/>').text(value).html();
}

Widget.prototype.htmlDecode = function(value) {
  return $('<div/>').html(value).text();
}

Widget.prototype.hideButtons = function () {
	// Hide buttons
	$("#" + this.widgetName + " .buttons").addClass('display-none');
	$("#" + this.widgetName + " .buttons-left").addClass('display-none');
	$("#" + this.widgetName + " .buttons-right").addClass('display-none');
}

Widget.prototype.showButtons = function () {
	// Show buttons
	$("#" + this.widgetName + " .buttons").removeClass('display-none');
	$("#" + this.widgetName + " .buttons-left").removeClass('display-none');
	$("#" + this.widgetName + " .buttons-right").removeClass('display-none');
}

Widget.prototype.refreshModule = function(){
	refreshModule($("#" + this.widgetName).parents('.module'));
};

Widget.prototype.AddPaging = function(pagingData) {
	if (pagingData.type == "Arrow") {
		var displayPreviousArrow = pagingData.data.StartArrayIndex > 0;
		var displayNextArrow = pagingData.data.EndArrayIndex < pagingData.data.PagedIndexes.length - 1;
		var pagingHtml = '<div class="paging"><div class="pages"'
		if (!displayPreviousArrow && !displayNextArrow) {
			pagingHtml += 'style="top: 0px"';
		}
		pagingHtml += '>';
		pagingHtml += pagingData.data.StartArrayIndex + 1;
		pagingHtml += ' - ';
		pagingHtml += pagingData.data.EndArrayIndex + 1;
		pagingHtml += ' of ';
		pagingHtml += pagingData.data.PagedIndexes.length;
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

		pagingData.div.addClass('arrow');
		pagingData.div.html(pagingHtml);

		pagingData.div.find('div.previous-arrow').click(function() {
			pagingData.data.CurrentPage -= 1;
			pagingData.action(pagingData.data);
		});

		pagingData.div.find('div.next-arrow').click(function() {
			pagingData.data.CurrentPage += 1;
			pagingData.action(pagingData.data);
		});
	}
	else if (pagingData.type == "PageNumber") {
		var pagingHtml = '<div class="paging">';

		if (pagingData.data.CurrentPage < 7)
		{
			for (var ndx = 1; ndx < pagingData.data.CurrentPage; ndx++)
			{
				pagingHtml += '<a class="page">' + ndx + '</a>';
			}
		}
		else
		{
			pagingHtml += '<a class="page">1</a>';
			pagingHtml += '<span class="elipsis">...</span>';
			for (var ndx = pagingData.data.CurrentPage - 4; ndx < pagingData.data.CurrentPage; ndx++)
			{
				pagingHtml += '<a class="page">' + ndx + '</a>';
			}
		}

		pagingHtml += '<div class="current-page">' + pagingData.data.CurrentPage + '</div>';

		if (pagingData.data.NumPages < pagingData.data.CurrentPage + 6)
		{
			for (var ndx = pagingData.data.CurrentPage + 1; ndx <= pagingData.data.NumPages; ndx++)
			{
				pagingHtml += '<a class="page">' + ndx + '</a>';
			}
		}
		else
		{
			for (var ndx = pagingData.data.CurrentPage + 1; ndx <= pagingData.data.CurrentPage + 4; ndx++)
			{
				pagingHtml += '<a class="page">' + ndx + '</a>';
			}
			pagingHtml += '<span class="elipsis">...</span>';
			pagingHtml += '<a class="page">' + pagingData.data.NumPages + '</a>';
		}

		pagingHtml += '</div><div class="clear"></div>';

		pagingData.div.addClass('page-number');
		pagingData.div.html(pagingHtml);

		pagingData.div.find('a.page').click(function() {
			pagingData.data.CurrentPage = $(this).html();
			pagingData.action(pagingData.data);
		});
	}
	else {
		throw 'Invalid paging type.  Please use "Arrow" or "PageNumber"';
	}
}

/**
 * 
 * Quick and simple export target #table_id into a csv
 * https://stackoverflow.com/a/56370447
 * 
 * @param table_id - html id for a table
 */
Widget.prototype.DownloadCSV = function(table_id) {
    // Select rows from table_id
    var rows = document.querySelectorAll('table#' + table_id + ' tr');
    // Construct csv
    var csv = [];
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        for (var j = 0; j < cols.length; j++) {
            // Clean innertext to remove multiple spaces and jumpline (break csv)
            var data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').replace(/(\s\s)/gm, ' ')
            // Escape double-quote with double-double-quote (see https://stackoverflow.com/questions/17808511/properly-escape-a-double-quote-in-csv)
            data = data.replace(/"/g, '""');
            // Push escaped string
            row.push('"' + data + '"');
        }
        csv.push(row.join(','));
    }
    var csv_string = csv.join('\n');
    // Download it
    var filename = 'export_' + table_id + '_' + new Date().toLocaleDateString() + '.csv';
    
    // IE10+
    if (navigator.msSaveBlob) {
        var blob = new Blob([csv_string], { type: 'data:text/csv;charset=utf-8' });
        return navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement('a');
        link.style.display = 'none';
        link.setAttribute('target', '_blank');
        link.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv_string));
        link.setAttribute('download', filename);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
}

/* GRID WIDGET
----------------------------------------------------------*/

GridWidget.prototype = new Widget();
function GridWidget(inWidgetName, inDataKeyID, inIsReadOnly) {
	//Calls Super constructor
	Widget.call(this, inWidgetName, inIsReadOnly);

	//key value of data.
	this.dataKeyID = inDataKeyID;
	//in a grid widget the data is always an array of something.
	this.data = new Array();

	//once data is saved you need to clear the data from the update array.
	//if the partialview is reloaded then we dont' have to clear the data field.
	this.cleanDirty = function (formName) {
		if(formName==null)
		{
			this.data = new Array();
			formName=this.widgetName;

			for(var n in this.forms)
			{
				$(document).trigger('DATA_CLEANED', this.forms[n]);  
			}
		
			var form = $('#'+formName).find('form');
		}

		$(document).trigger('DATA_CLEANED', formName);

		//you want to refresh because resetting validation might have caused it to adjust.
		this.refreshModule();
		
	};

  
	// Bind a focus handler to all cells that will focus that cell's input
	// when the cell is clicked anywhere
	$("#" + this.widgetName + " tbody td").on("click", function () {
		var cellInput = $(this).children(":input:visible");

		if (cellInput != null && cellInput != undefined) {
			cellInput.focus();
		}
	});    
}

GridWidget.prototype.specialUpdateInstructions = function (input, jsonindex) {
	//Overwrite to use.
};

GridWidget.prototype.updateData = function (input) {
	var ElementID = $(input).parents('tr').attr('pkid');
	var updateIndex = -1;
	var that = this;

	//First see if you have already updated this data.     
	for (var k in this.data) {
		if (this.data[k][this.dataKeyID] == ElementID) {
			updateIndex = k;
			break;
		}
	}

	if (updateIndex == -1) {
		updateIndex = this.data.length;
		this.data[updateIndex] = {};
		this.data[updateIndex][this.dataKeyID] = ElementID;
	}

	//fill in remaining inputs.
	$("#" + this.widgetName + " tr[pkid|=" + ElementID + "]").find(":input").each(function updateDataHelper(index, value) {
		that.data[updateIndex][$(value).attr("name").replace(blankElementReservedWord, "")] = $(value).val();
	});

	this.specialUpdateInstructions(input, updateIndex);

	var form = $('#' + this.widgetName + ' form');
	if (form.length > 0) {
		$(document).trigger('DATA_DIRTY', $(form).attr('id'));
	}
	return true;
};

// Deletes all records from the grid and widget data, or calls updateData for deletes of saved rows
GridWidget.prototype.deleteAllRecords = function (input) {
	var that = this;

	var allNonBlankRows = $(input).parents('table').find('tbody tr:not(.blank)');

	// Get the parent row and make sure it's not a blank row
	allNonBlankRows.each(function() {
		// Get the pkid of the current row's data
		var ElementID = $(this).attr('pkid');

		// If this was a new row (ID < 0) we'll just remove it from the widget data
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
			that.data.splice(indexToDelete,1);
		}
		// If this was an existing row, we'll update its data to pass back to the controller
		else {
			var Deleted = $(this).find(':input[name=Deleted]');
			if (Deleted.length)
			{
				Deleted.val(true);
				that.updateData(Deleted);
			}
		}
	});

	allNonBlankRows.remove();
};

// Deletes a record from the grid and widget data, or calls updateData for deletes of saved rows
GridWidget.prototype.deleteRecord = function (input) {
	// Get the parent row and make sure it's not a blank row
	var parentRow = $(input).parents('tr:not(.blank)');

	if (parentRow.length > 0) {

		// Get the pkid of the data
		var ElementID = parentRow.attr('pkid');

		// If we have a pkid, we'll continue with the delete
		if (ElementID != undefined)
		{
			// If this was a new row (ID < 0) we'll just remove it from the widget data
			if (ElementID < 0) {
				var indexToDelete = -1;
				// Get the index of the deleted data
				for (var k in this.data) {
					if (this.data[k][this.dataKeyID] == ElementID) {
						indexToDelete = k;
						break;
					}
				}
				this.specialUpdateInstructions(input, indexToDelete);
				// Remove new deleted row from the widget data
				this.data.splice(indexToDelete,1);
			}
			// If this was an existing row, we'll update its data to pass back to the controller
			else {
				var Deleted = parentRow.find(':input[name=Deleted]');
				if (Deleted.length)
				{
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

/* UTILITY FUNCTIONS
----------------------------------------------------------*/

function Helper() {
}

Helper.ConvertJSONDateToString = function (jsonDate, useLeadingZeros) {
	var fullDate = new Date(parseInt(jsonDate.substr(6)));
	fullDate = fullDate.unadjustForTimeZone();
	var month = fullDate.getMonth() + 1;
	var monthString = (useLeadingZeros == true && month < 10) ? "0" + month : month;
	var dateString = monthString + "/" + fullDate.getFullYear();
	return dateString;
};

Helper.ConvertJSONDateToJSDate = function (jsonDate) {
	var jsDate = new Date(parseInt(jsonDate.substr(6)));
	return jsDate.unadjustForTimeZone();
};

Helper.addCommas = function (nStr, prefix) {
	nStr += "";
	if(nStr.trim()!="")
	{
		if (prefix == null) prefix = "";
		nStr += '';
		x = nStr.split('.');
		x1 = x[0];
		x2 = x.length > 1 ? '.' + x[1] : '';
		var rgx = /(\d+)(\d{3})/;
		while (rgx.test(x1)) {
			x1 = x1.replace(rgx, '$1' + ',' + '$2');
		}
		return prefix + x1 + x2;
	}
	else{
		return nStr;
	}
};

Helper.removeCommas = function (nStr, prefix) {

	nStr = nStr.replace(/\,/g,'');

	if(prefix!=null){
	   nStr = nStr.replace(/prefix/g, "");
	}

	return nStr;
}

Helper.htmlEncodeForjQuery = function(value){

	var escaped = value;
	var findReplace = [
					   [/\"/g, ''], 
					   [/\'/g, ""],
					   [/\./g, "\\."],
					   [/\!/g, "\\!"] ,
					   [/\#/g, "\\#"],
					   [/\$/g, "\\$"],
					   [/\%/g, "\\%"],
					   [/\&/g, "\\&"],
					   [/\(/g, "\\("],
					   [/\)/g, "\\)"],
					   [/\*/g, "\\*"],
					   [/\+/g, "\\+"],
					   [/\,/g, "\\,"],
					   [/\//g, "\\/"],
					   [/\:/g, "\\:"],
					   [/\;/g, "\\;"],
					   [/\?/g, "\\?"],
					   [/\@/g, "\\@"],
					   [/\^/g, "\\^"],
					   [/\`/g, "\\`"],
					   [/\{/g, "\\{"],
					   [/\}/g, "\\}"],
					   [/\~/g, "\\~"],
					   [/\|/g, "\\|"]
					  ];

	if (escaped != undefined && escaped != null) {
		for(var item in findReplace) {
			 escaped = escaped.replace(findReplace[item][0], findReplace[item][1]);
		}
	}

  return escaped;
}

Helper.htmlEncode = function(value){
  return $('<div/>').text(value).html();
}

Helper.htmlDecode = function(value){
  return $('<div/>').html(value).text();
}

Helper.ArrayContains = function(value, array) {
	for (var index = 0; index < array.length; index++) {
		if (array[index] == value) {
			return true;
		}
	}
	return false;
}

Helper.isValidDate = function (d) {
	if (Object.prototype.toString.call(d) !== "[object Date]")
		return false;
	return !isNaN(d.getTime());
}

Helper.checkDateOrder = function (sdString, edString) {
	var startDate = sdString.toDate();
	var endDate = edString.toDate();
	return startDate <= endDate;
}

Helper.checkDateWithinParentRange = function (dateToCheckString, pSDString, pEDstring) {
	var DateToCheck = new Date(dateToCheckString.split('/')[1], (dateToCheckString.split('/')[0]) - 1);
	var ParentStartDate = new Date(pSDString.split('/')[1], (pSDString.split('/')[0]) - 1);
	var ParentEndDate = new Date(pEDstring.split('/')[1], (pEDstring.split('/')[0]) - 1);
	return DateToCheck >= ParentStartDate && DateToCheck <= ParentEndDate;
}

//To use do somethging like this: $(input).keyup(Helper.textAreaLimit(this, myLimit));
Helper.textAreaLimit = function(field, maxlen){
	if (field.value.length > maxlen)
	field.value = field.value.substring(0, maxlen);
};

Helper.roundAwayFromZero = function(number) {
	if (number > 0) {
		return Math.round(number);
	} else {
		return (-1 * Math.round(-1 * number));
	}
}

Helper.undefinedOrNull = function (value, defaultValue) {
	if (value === undefined || value == null) {
		return defaultValue;
	} else {
		return value;
	}
}

// Prevents the specified form from being posted when the [Enter] key is pressed.
Helper.preventFormPostingFromEnterKey = function (formId) {
	// Bind event.
	$("#" + formId).keypress(function (e) {
		// If [Enter] key is pressed, do not post form.
		if (e.keyCode == '13') {
			return false;
		}
	});
}

//TODO move this into Helper, it is used everywhere so to I'm putting this off until we have more time so we can test better. 31678
function CreatePostURL(workspace, controllerName, actionName, params) {
	var postURL = window.location.protocol + '//' + window.location.host + '/' +
		workspace + '/' + controllerName + '/' + actionName + '/';

	if (params !== undefined) {
		postURL = postURL + params;
	}
	return postURL;
}

function CreateSystemAdminWithParmsPostURL(controllerName, actionName, params) {
	var postURL = window.location.protocol + '//' + window.location.host + '/' + "default" +
		'/' + controllerName + '/' + actionName + '/' + params;
	return postURL;
}

function CreateSystemAdminPostURL(controllerName, actionName) {
	var postURL = window.location.protocol + '//' + window.location.host + '/' + "default" +
		'/' + controllerName + '/' + actionName;
	return postURL;
}

/* GRID ADDABLE ROW   
----------------------------------------------------------*/

//TODO add this to the GridWidget. 31679
var blankRow = {};

function AddableGrid(cssClass) {
	var selector = '.' + cssClass + ' .addable tr.blank';
	var html = $(selector).html();
	if (html != undefined) {
		blankRow[cssClass] = '<tr class="blank">' + $('.' + cssClass + ' .addable tr.blank').html() + '</tr>';

		// remove blank row and add again, so it is blank after a refresh.
		$('.' + cssClass + ' .addable .blank').remove();
		addBlankRow(cssClass, blankRow[cssClass]);
	}
}

//this make the word blank a reserveword
function addBlankRow(cssClass, blankRowHtml) {
	//For current BlankRow remove add new row event and remove the blank class
	$('.' + cssClass + ' .addable .blank').children('td').unbind('change');
	//denotes the input elements are intentionally blank, this way validation doesn't count them.
	$('.' + cssClass + ' .addable .blank').children('td').find(":input").each(function () {
		$(this).attr("name", $(this).attr("name").replace(blankElementReservedWord, ""));
	});
	$('.' + cssClass + ' .addable .blank').removeClass('blank');
	//Add new blank row and add new row event
	$('.' + cssClass + ' .addable').append(blankRowHtml);
	$('.' + cssClass + ' .addable .blank').children('td').change(function () {
		if ($(this).children(':input').val() != '') {
			addBlankRow(cssClass, blankRowHtml);
		}
	});
	$('.' + cssClass + ' .addable .blank').children('td').find(":input").each(function () {
		$(this).attr("name", $(this).attr("name") + blankElementReservedWord);
	});
	
	//trigger a event for the creation of a new blank row.
	$(document).trigger(cssClass + '_NEW_ROW_CREATED');
}

/* GRID FILTERING
----------------------------------------------------------*/

function FilterableGrid(cssClass) {
	var selector = '.' + cssClass + ' .filterable';
	$(selector + ' th.filter').each(function () {
		var filterBy = 'Filter by ';
		// if the th has a label, use this.
		if ($(this).attr('label') != undefined) {
			// ShortLabel is a keyword label used for filter textboxes that are too small to fit the "Filter by FieldName" text.
			if ($(this).attr('label') == 'ShortLabel') {
				filterBy = "Filter...";
			}
			else {
				// use the label
				filterBy += $(this).attr('label');                
			}
		}
		else {
			// else use the name of the filter
			filterBy += $(this).attr('filter');
		}
		var filterBoxHtml = '<div class="filter-left"></div><div class="filter-middle"><input type="text" class="filter-textbox default-text" placeholder="' + filterBy + '"/></div><div class="filter-right"></div><br/>';
		$(this).prepend(filterBoxHtml);
		var filterbox = $(this).find('.filter-textbox');
		filterbox.keyup(filterRow);
		filterbox.click(function () {
			$(this).val('');
			$(this).removeClass('default-text');
			$(this).unbind('click');
		});
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
			if (!filterCell.hasClass('default-text')) {
				filterVal = $.trim(filterCell.val());

				if (filterVal.indexOf("Filter by ") == 0 || filterVal.indexOf("Filter...") == 0) {
					// The filter was created, then deleted but the Placeholder text is being seen as the value
					filterVal = '';
				}
			}

			// If the filter input has a value, we'll add the value and the filter name
			// to our array of filters
			if (filterVal.length > 0) {
				filters[row] = {};
				filters[row].FilterOn = $(this).attr('filter');
				filters[row].FilterValue = filterVal;
				row++;
			}
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

		// Filter input elements
		$(this).parents('table').find(':input[name=' + filterOn + ']').each(function () {
			if (!filterCompare(filterOn, filterValue, $(this))) {
				$(this).parents('tr:not(.blank)').addClass('display-none');
			}
		});

		// Filter text of SPANS, DIVs and Hyperlinks
		$(this).parents('table').find(
									'span[name=' + filterOn + '],' +
									'div[name=' + filterOn + '],' +
									'a[name=' + filterOn + ']').each(function () {
			if (!spanCompare(filterOn, filterValue, $(this).text())) {
				$(this).parents('tr:not(.blank)').addClass('display-none');
			}
		});
	}

	refreshModule($(this).parents('.module'));
}

function spanCompare(filterOn, filter, currentValue) {
	filter = $.trim(filter.toLowerCase());
	var filterLength = filter.length;
	var currentValue = $.trim(currentValue.toLowerCase());
	if (filter.length > 0 && currentValue.indexOf(filter) < 0 && filter != -1) {
		return false;
	}
	return true;
}

// This method is overridden inside some partial views.  Please verify those still work correctly if this needs to be changed.
// Currently Overriden in: 
//          ManageWBSGrid
//          ManageBOEGrid
function filterCompare(filterOn, filter, currentItem) {
	filter = $.trim(filter.toLowerCase());
	var filterLength = filter.length;
	var currentValue = $.trim(currentItem.val().toLowerCase());
	if (filter.length > 0 && currentValue.indexOf(filter) < 0 && filter != -1) {
		return false;
	}
	return true;
}

var originalFilterCompare = window.filterCompare;

/* GRID SORTING
----------------------------------------------------------*/

function SortableGrid(cssClass) {
	var selector = cssClass + ' .sortable';
	// Only add sort links for headers that have the sort class applied AND
	// their sort attribute set
	$(selector + ' th.sort').each(function () {
		$(this).html('<div class="column-sort-header">' + $(this).html() + '</div>');
		$(this).find('div.column-sort-header').click(sortColumn);
	});
}

function sortColumn() {
	// Set the sorting column's header to be underlined
	$(this).parents('table').find('th.sort div.column-sort-header').css('text-decoration', 'none');
	$(this).parents('th').find('div.column-sort-header').css('text-decoration', 'underline');

	// Get the sort type
	sortType = $(this).parents('th').attr('sortType');

	// Get the key to sort on
	var sortColumnIndex = $(this).parents('thead').find('th').index($(this).parents('th'));

	// Get the current sort column from the parent table
	var tableSortColumn = $(this).parents('table').attr('sortedOnColumn');

	// Get the current sort direction from the parent table
	var tableSortAscending = $(this).parents('table').attr('sortedAscending') == 'true';

	// Get all rows that will be sorted
	var rowsToSort = $(this).parents('table').find('tbody tr:not(.blank)');

	// Get the table's body element
	var body = $(this).parents('table').find('tbody');

	// Perform selection sort on the rows, using the sort key to find the appropriate cell
	for (var outerNdx = 0; outerNdx < rowsToSort.length; outerNdx++) {
		var minNdx = outerNdx;
		var minValue = grabRowValue($(rowsToSort[minNdx]), sortColumnIndex, sortType);

		for (var innerNdx = outerNdx + 1; innerNdx < rowsToSort.length; innerNdx++) {
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

	// If the user clicked the same column header, then we'll reverse the sort order
	if (tableSortColumn == sortColumnIndex) {
		tableSortAscending = !tableSortAscending;
	}
	else {
		tableSortAscending = true;
	}

	// We are using prepend to add the rows back to the tbody before any .blank rows
	// Because of this we actually want to reverse the array when we want a normal
	// ascending sort order. For descending, we'll leave it alone.
	if (tableSortAscending) {
		rowsToSort = $(rowsToSort.get().reverse());
	}

	// Insert the rows back into the table
	rowsToSort.each(function () {
		body.prepend($(this));
	});

	$(this).parents('table').attr('sortedOnColumn', sortColumnIndex).attr('sortedAscending', tableSortAscending.toString());
}

function grabRowValue(row, sortColumnIndex, sortType) {
	var toReturn = '';

	var sortCell = $(row.find('td')[sortColumnIndex]);

	// Get text elements for the current row
	var currentSortCellText = sortCell.find('span, div, a');
	// Get input elements for the current row
	var currentSortCellInput = sortCell.find('input');

	// Continue if the current and minimum rows both have something to compare
	if (currentSortCellText.length || currentSortCellInput.length || sortCell.length) {

		// Choose current row comparison value
		if (currentSortCellText.length) {
			toReturn = currentSortCellText.first().text().toLowerCase();
		}
		else if (currentSortCellInput.length) {
			toReturn = currentSortCellInput.first().val().toLowerCase();
		}
		else {
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
				toReturn = parseInt(Helper.removeCommas(toReturn));
			}
			else if (sortType.match(/cost/i)) {
				// Convert the Cost string into a sortable number.
				//First remove the $ symbol then remove commas for sorting.
				toReturn = toReturn.substring(1, toReturn.length);
				toReturn = Helper.removeCommas(toReturn);
				toReturn = parseInt(toReturn);
				
			}
		}
	}

	return toReturn;
}

function createModule(module) {
	if ($(module).children('.module-header').length == 0) {
		// clean script
		$(module).find('script').remove();

		//prefix for contentblocks
		var elementid = null;
		if ($(module).attr('class') != null) {
			var elementid = $(module).attr('class').replace(/ /gi, "");
		}

		//header first
		var headerData = $(module).children('.module-header-data');
		$(module).children('.module-header-data').remove();
		$(module).append('<div class="module-header"><div class="module-header-left"></div><div class="module-header-center">' +
			'</div><div class="module-header-right"></div></div>');
		$(module).find('.module-header-center').append(headerData);


		//all data blocks
		var contentData = [];
		$(module).children('.module-content-data').each(function () {
			contentData.push($(this));
		});

		for (var i=0; i<contentData.length; i++) {
			var classes = contentData[i].attr('class').replace('module-content-data', 'module-content');
			$(module).append('<div id="' + elementid + 'ModuleContent' + i + '" class="' + classes + '"><div class="module-content-left"></div><div class="module-content-center"></div><div class="module-content-right"></div></div>');
			$(module).find('#' + elementid + 'ModuleContent' + i + ' .module-content-center').append(contentData[i]);
		}

		$(module).append('<div class="module-footer"><div class="module-footer-left"></div><div class="module-footer-center"></div><div class="module-footer-right"></div></div>');
	}
}

function refreshModule(module) {
	var contentWidth = 0;
	var temp = $(module).find('.module-content');
	temp.each(function () {
		var thisContentWidth = $(this).find('.module-content-center').width();
		if (thisContentWidth > contentWidth) {
			contentWidth = thisContentWidth;
		}

	});

	if ($(module).find('.module-header').length > 0) {
		var headerWidth = $(module).find('.module-header-center').width();

		if (headerWidth > (contentWidth - 6)) {
			$(module).find('.module-content-center').width(headerWidth + 6);
			$(module).find('.module-footer-center').width(headerWidth + 2);
		}
		else {
			$(module).find('.module-header-center').width(contentWidth - 6);
			$(module).find('.module-content-center').width(contentWidth);
			$(module).find('.module-footer-center').width(contentWidth - 4);
		}
	}
}

/* COLLAPSIBLE HEADERS  
----------------------------------------------------------*/

function CollapsibleModule(module) {
	$(module).find('.module-header-data').prepend('<div class="collapsible-header">');

	if ($(module).hasClass('collapsed')) {
		$(module).find('.collapsible-header').gentoggle(function () {
			$(module).find('.expanded-content').slideDown(400, function () {
				$(module).removeClass('collapsed');
				$(module).addClass('expanded');
				refreshModule(module);
			});
		}, function () {
			$(module).find('.expanded-content').slideUp(400, function () {
				$(module).addClass('collapsed');
				$(module).removeClass('expanded');
				refreshModule(module);
			});
		});

		$(module).find('.expanded-content').hide();
	}
	if ($(module).hasClass('expanded')) {
		$(module).find('.collapsible-header').gentoggle(function () {
			$(module).find('.expanded-content').slideUp(400, function () {
				$(module).addClass('collapsed');
				$(module).removeClass('expanded');
				refreshModule(module);
			});
		}, function () {
			$(module).find('.expanded-content').slideDown(400, function () {
				$(module).removeClass('collapsed');
				$(module).addClass('expanded');
				refreshModule(module);
			});
		});
	}
}

/* Row deletion
----------------------------------------------------------*/

function softDeleteAllRecords(input) {
	var allNonBlankRows = $(input).parents('table').find('tbody tr:not(.blank)');

	// Get the parent row and make sure it's not a blank row
	allNonBlankRows.each(function () {
		softDeleteRecord($(this));
	});
}

function softDeleteRecord(input, onDataChanged) {
	// Get the parent row and make sure it's not a blank row
	var parentRow = $(input).parents('tr:not(.blank)');

	if (parentRow.length > 0) {
		// Must have a pkid to be deleted
		var pkid = parentRow.attr('pkid');
		if (pkid != undefined) {
			// mark row as "deleted"
			$(':input[name=Deleted]', parentRow).val(true);

			// "soft delete" the record - i.e. hide but do not remove
			parentRow.addClass('display-none');
			var module = parentRow.parents('.module');
			if (module.length) {
				refreshModule(module);
			}

			// callback
			if (onDataChanged != null) {
				onDataChanged(input);
			}
		}
	}
}

/* Added Extentions to JS objects.
----------------------------------------------------------*/
Date.prototype.getMonthsBetween = function (d) {
	//returns months between dates
	var d1 = this.getFullYear() * 12 + this.getMonth();
	var d2 = d.getFullYear() * 12 + d.getMonth();
	return d2 - d1;
};

Date.prototype.unadjustForTimeZone = function () {
	/*
	this maybe OBE. I belive the new solution is instead of 
	attempting to deal with adjustments we are goign to use the 
	15th of the month to avoid percision issues.
	*/
	var offset = 0;//(this.getTimezoneOffset() - 240) * 60000;
	return new Date(this.getTime() + offset);
};

 /* Adds the number of months to date. */
Date.prototype.addMonths = function (m) {
   //clone the date
	var clonedate = this.getDate();
	//move the date cursor to next month
	this.setMonth(this.getMonth() + m);
	//month adjustment if clone date has more days than new month
	 if (this.getDate() < clonedate)
		this.setDate(0);
};

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

Date.prototype.DateOnly = function () {
	return new Date(this.getFullYear(), this.getMonth(), this.getDate());
}

Date.prototype.MonthPrecision = function () {
	//use a date in the middle of most months so we don't have to worry about DLS or TZs
	return new Date(this.getFullYear(), this.getMonth(), 15);
}

String.prototype.count=function(s1) {    
	return (this.length - this.replace(new RegExp(s1,"g"), '').length) / s1.length;
};

String.prototype.ensureProperDate = function () {
	if (this.match(/^((0?[1-9]|1[012])\/((19|20)\d\d))$/)) {
		return this.substr(0, this.indexOf('/')) + '/15' + this.substr(this.indexOf('/'));
	}

	return this.valueOf();
};

String.prototype.toDate = function () {
	if (this.isDate()) {
		var properDate = this.ensureProperDate();
		var toreturn = new Date(properDate);
		return toreturn;
	}

	return new Date(0);
};

String.prototype.isDate = function () {
	return this.match(/^((0?[1-9]|1[012])\/(0?[1-9]|[12][0-9]|3[01])\/((19|20)\d\d))$|^((0?[1-9]|1[012])\/((19|20)\d\d))$/);
};

String.prototype.isTrue = function () {
	var matches = $.trim(this).toLowerCase().match(/^true$/);
	return matches != null && matches.length > 0;
};

function toFixed(value, precision) {
	var power = Math.pow(10, precision || 0);
	return String(Helper.roundAwayFromZero(value * power) / power);
}

String.prototype.endsWith = function(text) {
	var expectedIndex = this.length - text.length;
	var index = this.indexOf(text, expectedIndex);
	if (index == expectedIndex) {
		return true;
	} else {
		return false;
	}
};

String.prototype.trim = function (delimiter) {
	var s = this.toString();
	if (delimiter == undefined) {
		delimiter = ' ';
	}
	var leading = new RegExp('^' + delimiter + '*');
	var trailing = new RegExp(delimiter + '*' + '$');
	return s.replace(leading, '').replace(trailing, '');
};

String.prototype.removeFromList = function (item, separator) {
	var sep = (separator == undefined) ? ',' : separator;
	var arrayList = this.toString().split(sep);
	var resultingArrayList = [];
	for (i in arrayList) {
		var arrayListItem = arrayList[i];
		if (arrayListItem != item) {
			resultingArrayList.push(arrayListItem);
		}
	}
	return resultingArrayList.join(sep).trim(sep);
};

String.prototype.addToList = function (item, separator) {
	var sep = (separator == undefined) ? ',' : separator;
	var arrayList = this.toString().split(sep);
	var found = false;
	for (i in arrayList) {
		var arrayListItem = arrayList[i];
		if (arrayListItem == item) {
			found = true;
			break;
		}
	}
	if (!found) {
		arrayList.push(item);
	}
	return arrayList.join(sep).trim(sep);
};

//
// Note: This function was added as custom logic for the Add Historical Metrics view.  It was originally named "serializeObject".
// However, that name was causing calls to serializeObject (from within the GenCommon project's GenForm.getData function) to be
// erroneously resolved to THIS function, and NOT the GenCommon project's serializeObject function.
//
$.fn.mySerializeObject = function ()
{
	var o = {};
	var a = this.serializeArray();
	$.each(a, function() {
		if (o[this.name] !== undefined) {
			if (!o[this.name].push) {
				o[this.name] = [o[this.name]];
			}
			o[this.name].push(this.value || '');
		} else {
			o[this.name] = this.value || '';
		}
	});
	return o;
};

/****************End Js extentions**********************************/

function DisplayExceptionDialog(error) {
	$("#ErrorText").html(error.Message.replace(/\n/g, '<br/>'));
	$("#ErrorDetails").html(error.Details.replace(/\n/g, '<br/>')).toggle(false); // hide by default

	// if you are in prod environment, hide the details from the user
	if (typeof document.location.host != 'undefined') {
		if (document.location.host == prodGenBoeUrl_Global) {
			try {
				$("#ToggleErrorDetails").hide();
			} catch (err) { }
		}
	}

	$("#ExceptionDialog").dialog({ width: 300, modal: true, resizable: true, draggable: true, title: error.Title });
}

CloseExceptionDialog = function () {
	$('#ExceptionDialog').dialog('close');
}

function RaiseNotification(text) {
	$(document).trigger("DISPLAY_NOTIFICATION", text);
}

function ShowLoadingBox(id) {
	if (typeof id === 'undefined') {
		$(document).trigger("SHOW_LOADING_BOX");
	} else {
		var loadingBox = $('#' + id);
		if (loadingBox.length > 0) {
			loadingBox.removeClass('display-none');
		} else {
			$('body').append($('<div id="' + id + '" class="page-loading"><div class="loading-box"><div class="loader white"></div><span>Loading...</span></div></div>'));
		}
	}
}

function HideLoadingBox(id) {
	if (typeof id === 'undefined') {
		$(document).trigger("HIDE_LOADING_BOX");
	} else {
		$('#' + id).addClass('display-none');
	}
}

/*
* Edit handler logic for tinyMCE edits.
* @param mce: tinyMCE editor
* @param options: Additional (custom) configuration options
* @param callbackFunction: Callback handler
*/
function updateRTECharacterCount(mce, options, callbackFunction) {
	var maxlen = options.maxlen;
	var enableCharCounting = options.enableCharCounting || false;

	var richTextContent = mce.getContent();

	var selectedItem = $(mce.getElement());

	// check "text-only" content (i.e. no markup) length against maxlen
	var richTextEncoded = '{ "html" : "' + encodeURI(richTextContent) + '" }';  // must be URL-encoded for JSON

	// SJR - 6/25/2014
	// Note: Client-side regular-expression-based detection of empty markup tags was causing the UI to hang on IE.
	//       I moved this capability into the server-side ConvertHtmlToText logic.  See "WhiteSpace" property below.
	$.ajax({
		type: 'POST',
		url: '/default/GenBOE/ConvertHtmlToText?returnConvertedText=false',
		data: richTextEncoded,
		contentType: 'application/json; charset=utf-8',
		dataType: 'json',
		success: function (result) {
			if (result.WhiteSpace) {
				richTextContent = ''; // only remove it if it would result in an empty field
			} 
			
			if (enableCharCounting) {
				var mceContainer = $(mce.getContainer());
				if (mceContainer.context) { // When working on MOQ Types, and possibly other places, when a number of RTEs are being initialized/destroyed, this can take long enough to where it becomes undefined
					var charCounter = $('.mce-char-count', mceContainer);
					charCounter.text(result.Length.toLocaleString('en'));
					var lendiff = maxlen - result.Length;

					if (lendiff < 0) {
						charCounter.css('color', '#FF0000').css('font-weight', 'bold');  // error
						charCounter.text(result.Length.toLocaleString('en') + ' (' + lendiff.toLocaleString('en') + ')');
						$('#' + mceContainer.context.id).css('border-color', 'red')
					} else {
						charCounter.css('color', '#000000').css('font-weight', 'normal');  // normal
						$('#' + mceContainer.context.id).css('border-color', 'black');
					}
				}
			}
		},
		complete: function () {
			selectedItem.val(richTextContent);

			if (callbackFunction && typeof callbackFunction == "function") {
				callbackFunction(mce);
			}
		}
	});
}

//
// Initializes the Tiny MCE RichText Editor
// @param elementName: name of the text field which will be replaced by the editor
// @param options: additional (custom) configuration options
//        - maxlen: [int] Maximum text-only content length
//        - enableCharCounting: [boolean] Whether to display/track text-only character count
// @param widget: the widget containing the editor (Note: Works with iBOE.js's Widget and generation.js's GenWidget
//
function InitializeRTE(elementName, options, widget, skipInitialClean) {
	var maxlen = options.maxlen;
	var enableCharCounting = options.enableCharCounting || false;

	// Remove any existing editor with the same ID (elementName).  If we don't, it will sometimes cause problems during partial page refreshes.
	tinymce.EditorManager.execCommand('mceRemoveEditor', true, elementName);

	// Setup the RTE (tinymce === EditorManager)
	tinymce.init({
		cache_suffix: "?v=4.7.13",
		selector: eval("'textarea' + '[name=' + elementName + ']'"),
		theme: "modern",
		theme_url: '/Scripts/tinymce/themes/modern/theme.min.js',
		skin: 'lightgray',
		skin_url: '/Scripts/tinymce/skins/lightgray',
		plugins: [
			"advlist autolink link image lists charmap preview hr anchor pagebreak, code",
			"searchreplace visualblocks visualchars fullscreen insertdatetime nonbreaking",
			"save table contextmenu directionality template paste textcolor",
			"fullscreen placeholder"
		],
		content_css: "/Resources/css/tinymce.css",
		menu: {
			edit: {title: 'Edit', items: 'undo redo | cut copy paste pastetext | selectall | searchreplace'},  
			insert: {title: 'Insert', items: 'image link | charmap hr anchor pagebreak insertdatetime nonbreaking template'},  
			view: {title: 'View', items: 'visualchars visualblocks | preview fullscreen'},  
			format: {title: 'Format', items: 'bold italic underline strikethrough superscript subscript | formats | removeformat'},  
			table: { title: 'Table', items: 'inserttable tableprops deletetable cell row column' }
			//,tools: {title: 'Tools', items: 'code'} //uncomment this and code in toolbar string to add a tool to view the html code for debugging
		},
		toolbar: "insertfile undo redo | styleselect | bold italic underline | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | preview fullpage | forecolor backcolor | table" /*+ " | code"*/,
		browser_spellcheck: true,
		file_picker_types: 'image',
		file_picker_callback: function (cb, value, meta) {
			// Create image file picker with a hidden input field.
			var input = document.createElement('input');
			input.setAttribute('type', 'file');
			input.setAttribute('accept', 'image/*');

			// Make the 'Source' textbox readonly so that the blobUri cannot be edited.
			$('.mce-has-open .mce-textbox').attr('readonly', 'true');

			input.onchange = function() {
				var file = this.files[0];
	  
				var reader = new FileReader();
				reader.readAsDataURL(file);
				reader.onload = function () {
					// Note: Now we need to register the blob in TinyMCEs image blob
					// registry. In the next release this part hopefully won't be
					// necessary, as they are looking to handle it internally.
					var id = 'blobid' + (new Date()).getTime();
					var blobCache =  tinymce.activeEditor.editorUpload.blobCache;
					var base64 = reader.result.split(',')[1];
					var blobInfo = blobCache.create(id, file, base64);
					blobCache.add(blobInfo);

					// Call the callback and populate the alt field with the file name.
					cb(blobInfo.blobUri(), { alt: file.name });
				};
			};
	
			input.click();
		},
		//fontselect fontsizeselect emoticons | 
		// needed to customize the formats menu
		paste_data_images: true,
		convert_urls: false,
		paste_block_drop: false,
		paste_retain_style_properties: 'all',
		paste_word_valid_elements: '@[style],-strong/b,-em/i,-span,-p,-ol,-ul,-li,-h1,-h2,-h3,-h4,-h5,-h6,' +
						'-table,-tr,-td[colspan|rowspan],-th,-thead,-tfoot,-tbody,-a[href|name],-font[color],sub,sup,strike,br,u',
		style_formats: [ 
		{title: 'Inline', items: [
			{title: 'Bold', icon: 'bold', format: 'bold'},
			{title: 'Italic', icon: 'italic', format: 'italic'},
			{title: 'Underline', icon: 'underline', format: 'underline'},
			{title: 'Strikethrough', icon: 'strikethrough', format: 'strikethrough'},
			{title: 'Superscript', icon: 'superscript', format: 'superscript'},
			{title: 'Subscript', icon: 'subscript', format: 'subscript'}
			]},
		{title: 'Alignment', items: [
			{title: 'Left', icon: 'alignleft', format: 'alignleft'},
			{title: 'Center', icon: 'aligncenter', format: 'aligncenter'},
			{title: 'Right', icon: 'alignright', format: 'alignright'},
			{title: 'Justify', icon: 'alignjustify', format: 'alignjustify'}
			]}
		],
		visual: false, // turns off visual aid that draws dotted lines around tables with no borders.  With this on pasted in tables from Excel don't display an outer border
		setup: function (ed) {
			ed.on('Change Redo Undo', function (evt) {
				updateRTECharacterCount(this, options, function (mce) {
					var form = $(mce.getElement()).closest('form');
					widget.setDirty(form.attr('id'));
				});
			});

			ed.on('keyup', function (evt) {
				if (enableCharCounting) {
					updateRTECharacterCount(this, options, function (mce) {
						var form = $(mce.getElement()).closest('form');
						widget.setDirty(form.attr('id'));
					});
				}
			});

			// set the location so that plug-ins load correctly
			ed.editorManager.AddOnManager.baseURL = '/Scripts/tinymce';
		},
		// This does the stripping of rich text during a paste
		paste_preprocess: function (plugin, args) {
			var encodedInitial = encodeURI(args.content);
			var encodedFinal = encodedInitial.replace(/\+/g, '%2B');  // replace plus-sign (+) with its URL-encoding (2B)
			var richTextEncoded = '{ "html" : "' + encodedFinal + '" }';  // must be URL-encoded for JSON

			// call the server to do the scrub
			$.ajax({
				type: 'POST',
				async: false,  // call MUST be synchronous so args.content will be set before the function exits
				url: '/default/GenBOE/PreProcessRichTextPaste',
				contentType: 'application/json; charset=utf-8',
				data: richTextEncoded,
				dataType: 'html',  // response is HTML
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
			var mceContainer = $(this.getContainer());
			if (enableCharCounting) {
				// add a custom character counter to the status bar
				var resizehandle = $('.mce-resizehandle', mceContainer);
				var charCounterHtml = '<div style="float: right; margin-top: 8px; margin-right: 8px;">' + 
								'<div class="mce-char-count" style="color: #000000; float: right; font-size:11px;"></div><div style="float: right; font-size:11px;">Chars:&nbsp;</div></div>';
				resizehandle.before(charCounterHtml);
			}

			// BOEJ-2849 - clear the title (tooltip) attribute for the tinymce editor iframe.
			mceContainer.find('iframe').attr('title', '');

			updateRTECharacterCount(this, options, function () { if (!skipInitialClean) { widget.cleanDirty(); }});
		}
	});
}

//
// Make RTE Data Read Only
// @param elementNameContainingData: name of the text field which contains HTML encoded data
// @param elementNameDisplayedDuringReadOnly: name or class of the element that will be displayed during read only
//
function HandleRTEDataForReadOnly(elementNameContainingData, elementNameDisplayedDuringReadOnly) {
    var decoded = $("<div/>").html($(elementNameContainingData).html()).text();
	$(elementNameContainingData).next(elementNameDisplayedDuringReadOnly).html(decoded).attr('title', '');
}

$(function () {

	window.onbeforeunload = function () {
		if (Session.isDirty()) {
			return "Changes have not been saved. Are you sure you want to navigate away?";
		}
	};

	$(document).bind('DATA_DIRTY', function (e, formName) {
		Session.setDirty(formName);
	});

	$(document).bind('DATA_CLEANED', function (e, formName) {
		Session.clearDirty(formName);
	});
	
	$(document).delegate('.help-dialog-close', 'click', function () {
		$(this).parent().prev().click();
	});

	/**
	* @function
	* Creates a genPopUp element from a div
	* $(div).genPopUp()
	* 
	* @returns a GenPopUpDiv Object
	*/
	$.fn.genPopUp = function (options) {
		var that = this;

		var button = options.button;
		var onHide = options.onHide;
		var onShow = options.onShow;
		var propagateOnClick = options.propagateOnClick || false;
		var leftOffSet = options.leftOffSet || 0;
		var addingViaSelector = false;

		that.addClass("popup-div");

		function GenPopUpDiv(context, button) {
			var genPopUp = this;

			genPopUp.popupContext = $(that).click(function (event) {
				if (!propagateOnClick) {
					event.stopPropagation();
				}
			}).hide();

			var propagationTriggerPoint = null;
			var selectorTarget = null;

			if ($.type(button) === "string") {
				propagationTriggerPoint = $("body")
				selectorTarget = button;
				addingViaSelector = true;
			} else {
				propagationTriggerPoint = $(button);
			}

			//popup div code
			$(propagationTriggerPoint).on("click", selectorTarget, function (event) {
				var buttonPressed = this;

				event.stopPropagation();
				if ($(genPopUp.popupContext).is(":visible")) {
					$(genPopUp.popupContext).hide();
					if (onHide != null && typeof onHide == "function") {
						onHide($(buttonPressed), event);
					}
				} else {
					$('body').one('click', function () {
						$(genPopUp.popupContext).hide();
					});
					$(genPopUp.popupContext).show();
					if (onShow != null && typeof onShow == "function") {
						onShow($(buttonPressed), event);
					}
				}

				if ($(button).length > 1 || addingViaSelector) {
					var offset = $(buttonPressed).offset();
					offset.top += $(buttonPressed).height();
					offset.left += leftOffSet;
					$(genPopUp.popupContext).offset(offset);
				}
			});
		}

		var toReturn = new GenPopUpDiv(that, button);

		return toReturn;
	};
});


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