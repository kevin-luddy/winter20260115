var IdentitySwappingClassInstance;

function InitializeIdentitySwappingClassInstance() {
    IdentitySwappingClassInstance = new IdentitySwappingClass();
}

// Creates an instance of the IdentitySwappingClass; its constructor will write the IdentitySwapping HTML to the page
if (document.addEventListener) { // for normal browsers
    document.addEventListener('DOMContentLoaded', InitializeIdentitySwappingClassInstance, false);
} else { // for IE
    document.attachEvent("onreadystatechange", function () {
        if (document.readyState == "complete") {
            InitializeIdentitySwappingClassInstance();
        }
    });
}

/*
* @constructor
* Automatically runs IdentitySwapping
*/
function IdentitySwappingClass() {
    this.CreateIdentitySwappingHTML();
};

IdentitySwappingClass.prototype = (function () {
    var IdentitySwappingUserCookieName = 'IdentitySwappingCookieUser';
    var IdentitySwappingRolesCookieName = 'IdentitySwappingCookieRoles';
    var cookieValueString = 'value';
    var numberOfMinutesCookieIsGoodFor = 30;

    var styleForPage = '    .hiddenIdentitySwapping' +
                        '    {' +
                        '       visibility:hidden;' +
                        '       display:none;' +
                        '    }' +
                        '    #IdentitySwappingSettingDiv' +
                        '    {' +
                        '       background-color:#66FF99;' +
                        '       font-style:italic;' +
                        '       text-align:center; ' +
                        '       vertical-align:middle;' +
                        '       line-height:20px;' +
                        '    }' +
                        '    #IdentitySwappingDiv input[type=button]' +
                        '    {' +
                        '       margin-left:30px;' +
                        '    }' +
                        '    #IdentitySwappingBannerDiv' +
                        '    {' +
                        '       background-color:#FF4D4D;' +
                        '       font-weight:bold;' +
                        '       text-align:center;' +
                        '       vertical-align:middle' +
                        '    }';

    var htmlForPage = '<div id="IdentitySwappingSettingDiv">' +
                        '    Userid (with domain) <input type="text" id="impersonationUserName" />' +
                        '    Roles (optional) <input type="text" id="impersonationRoles" />' +
                        '    <input type="button" id="impersonationImpersonateButton" onclick="IdentitySwappingClassInstance.Impersonate()" value="Swap Identities" />' +
                        '</div>' +
                        '<div id="IdentitySwappingBannerDiv" class="hiddenIdentitySwapping">' +
                        '    <span id="IdentitySwappingBannerText"></span>' +
                        '    <input type="button" id="impersonationClearButton" onclick="IdentitySwappingClassInstance.RemoveIdentitySwapping()" value="Clear Identity Swapping" />' +
                        '</div>';

    // private functions
    /*
    * This particular method was copied from:
    * http://stackoverflow.com/questions/4003823/javascript-getcookie-functions/4004010#4004010
    */
    ReadCookie = function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    /*
    * Creates an individual IdentitySwapping cookie
    *
    */
    WriteCookie = function (cookieName, cookieValue, expirationInMinutes) {
        // Setup expiration time for the new cookies
        var date = new Date();
        date.setTime(date.getTime() + (expirationInMinutes * 60 * 1000));
        var cookieExpiration = '; expires=' + date.toGMTString();

        // Create Cookie
        var cookie = cookieName + '=' + cookieValueString + '=' + cookieValue + cookieExpiration + '; path=/';

        // Write it into the document
        document.cookie = cookie;
    }

    /*
    * Removes IdentitySwapping Cookies
    *
    */
    RemoveIdentitySwappingCookies = function () {
        WriteCookie(IdentitySwappingUserCookieName, '', -1 * 24 * 60); // to expire a cookie you make it's date 1 day earlier
        WriteCookie(IdentitySwappingRolesCookieName, '', -1 * 24 * 60); // to expire a cookie you make it's date 1 day earlier
    }

    /*
    * Writes out all cookies used by IdentitySwapping
    *
    */
    CreateIdentitySwappingCookies = function (userId, roles) {
        // first remove existing cookies, if any
        RemoveIdentitySwappingCookies();

        // Create new cookies
        WriteCookie(IdentitySwappingUserCookieName, userId, numberOfMinutesCookieIsGoodFor);
        WriteCookie(IdentitySwappingRolesCookieName, roles, numberOfMinutesCookieIsGoodFor);
    }

    /*
    * Writes out the IdentitySwapping banner, based on the username and roles & displays it
    *
    */
    WriteOutBannerForIdentitySwapping = function (impersonatedUser, impersonatedRoles) {
        var innerText = 'You are impersonating user: ' + impersonatedUser;

        if (impersonatedRoles) {
            innerText += ', with the following roles: ' + impersonatedRoles;
        }

        document.getElementById('IdentitySwappingBannerText').innerText = innerText;
        document.getElementById('IdentitySwappingBannerDiv').className = '';
        document.getElementById('IdentitySwappingSettingDiv').className = 'hiddenIdentitySwapping';
    }

    /*
    * Clears out the IdentitySwapping banner & hides it
    *
    */
    ClearBannerForIdentitySwapping = function () {
        document.getElementById('IdentitySwappingBannerText').innerHTML = '';
        document.getElementById('IdentitySwappingBannerDiv').className = 'hiddenIdentitySwapping';
        document.getElementById('IdentitySwappingSettingDiv').className = '';
    }

    /*
    * Validates UserName
    * 
    */
    ValidateUserName = function (userName) {
        // this regex only allows letters, numbers, dash, backslash and underscore
        var re = new RegExp("[a-zA-Z0-9-\\\\_]{0,}");

        return userName.match(re) == userName;
    }

    /*
    * Validates Roles
    * 
    */
    ValidateRoles = function (roles) {
        // this regex only allows letters, numbers, dash, comma, period, space, backslash and underscore
        var re = new RegExp("[a-zA-Z0-9-,\. \\\\_]{0,}");

        return roles.match(re) == roles;
    }
    // public functions
    return {
        /*
        * Need to make sure the constructor stays public.
        */
        constructor: IdentitySwappingClass,

        /*
        * Takes inputs from the textboxes and sets up the IdentitySwapping;
        *
        */
        Impersonate: function () {
            var impersonatedUser = document.getElementById('impersonationUserName').value;
            var impersonatedRole = document.getElementById('impersonationRoles').value;

            if (!ValidateUserName(impersonatedUser)) {
                alert('Username input is invalid. It can contain only letters, numbers, dash (-), backslash (\\) and underscore (_).');
            } else if (!ValidateRoles(impersonatedRole)) {
                alert('Roles input is invalid. It can only contain letters, numbers, dash (-), comma (,), space ( ), period (.), backslash (\\) and underscore (_).');
            } else {
                CreateIdentitySwappingCookies(impersonatedUser, impersonatedRole);
                WriteOutBannerForIdentitySwapping(impersonatedUser, impersonatedRole);

                location.reload();
            }
        },

        /*
        * Writes IdentitySwapping HTML into the page
        *
        */
        RemoveIdentitySwapping: function () {
            RemoveIdentitySwappingCookies();
            ClearBannerForIdentitySwapping();

            location.reload();
        },

        /*
        * Writes IdentitySwapping HTML into the page;
        * Part that deals w/ setting styles into the head element was based on: http://www.phpied.com/dynamic-script-and-style-elements-in-ie/
        *
        */
        CreateIdentitySwappingHTML: function () {
            var outerDiv = document.getElementById('IdentitySwappingDiv');

            if (outerDiv) {
                // Write out the style
                var styleElement = document.createElement("style");
                styleElement.type = "text/css";

                if (styleElement.styleSheet) {   // IE
                    styleElement.styleSheet.cssText = styleForPage;
                } else {                // the world
                    styleElement.appendChild(document.createTextNode(styleForPage));
                }
                document.getElementsByTagName('head')[0].appendChild(styleElement);

                // Write out the HTML
                outerDiv.innerHTML = htmlForPage;

                // if the cookies exist already, then fill in the banner
                var userCookie = ReadCookie(IdentitySwappingUserCookieName);
                var roleCookie = ReadCookie(IdentitySwappingRolesCookieName);

                if (userCookie) {
                    var userName = userCookie.replace('value=', '');

                    var roles = null;
                    if (roleCookie) {
                        roles = roleCookie.replace('value=', '');
                    }

                    WriteOutBannerForIdentitySwapping(userName, roles);
                }
            }
        }
    }
})();