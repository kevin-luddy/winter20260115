// if filter object is blank, or index=0, hide clear-filter glyphicon
function filterObjectChange(val, inStr) {
    if (val == 0 || val == -1 || val === '' || val == null || val == '(blanks)') {
        document.getElementById(inStr).hidden = "hidden";
    }
    else {
        document.getElementById(inStr).hidden = "";
    }
}

// for range filters, if either has value show filter clear glyph
// if no value in val1 or val2 hide glyph
function filterObjectRangeChange(val1, val2, inStr) {
    if ((val1 == '' || val1 == null) && (val2 == '' || val2 == null)) {
        document.getElementById(inStr).hidden = "hidden";
    }
    else {
        document.getElementById(inStr).hidden = "";
    }
}

/* Create a POST url via parameters */
function createPostURL (controllerName, actionName) {
    var postURL = window.location.protocol + '//' + window.location.host + '/' +
        controllerName + '/' + actionName + '/';

    return postURL;
}

/* Add an option to an array (for a dropdown) */
function addOption (filterArray, stringValue) {
    if (stringValue !== undefined && stringValue != null) {
        filterArray[stringValue] = { display: stringValue, value: stringValue };
    }
}

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

/*
 * This is going to be a custom RPM method to get the Max Year used for data filtering.
 * The method will return current year, if the date is in the first 3 Quarters
 * When we get into the 4th Quarter (October / November / December), it will return current year + 1
 * 
 * Input is d, a Date object, which can be created by "new Date()"
 */
var getMaxYearForDataFiltering = function (d) {
    var year = d.getFullYear();

    // getMonth starts at 0, so January = 0, October = 9.
    var currentMonth = d.getMonth() + 1;

    if (currentMonth >= 10) {
        ++year;
    }

    return year;
}
