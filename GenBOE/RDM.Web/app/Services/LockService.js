angular.module('RDM').factory('lockService', ['$http', '$q', '$timeout', 'utilityService', function ($http, $q, $timeout, utilityService) {
    var id; // This value set in the calling controllers
    var warningTimer;
    var expirationTimer;

    var lockService = {

        /* Private method for this class - do not call externally. */
        performPost: function(url) {
            var deferred = $q.defer();
            $http.post(url + this.id, {} ).then(
                function (response) {
                    // Convert the InUse Date from the JSON format to the proper string format.
                    response.data.LockInfo.InUse = utilityService.getDateStringFromJsonDate(response.data.LockInfo.InUse);
                    deferred.resolve(response);
                },
                function () { deferred.reject(); });
            return deferred.promise;
        },

        lock: function () {
            $(document).trigger("SHOW_LOADING_BOX");
            return lockService.performPost('/Home/Lock/');
        },

        unlock: function () {
            return lockService.performPost('/Home/Unlock/');
        },

        refreshLock: function () {
            return lockService.performPost('/Home/RefreshLock/');
        },

        synchronousUnlock: function () {
            $.ajax({
                type: 'POST',
                async: false,
                url: '/Home/Unlock/' + this.id,
                contentType: 'application/json; charset=utf-8'
            });
        },

        /* Clear previous timers (if any are running) */
        clearLockTimers: function () {
            if (warningTimer) { $timeout.cancel(warningTimer); warningTimer = null; }
            if (expirationTimer) { $timeout.cancel(expirationTimer); expirationTimer = null; }
        },

        /* Start edit lock timers: 1) to show warning if lock is about to expire, 2) to invoke expirationCallback when the lock expires.  */
        startLockTimers: function (warningMinutes, refreshLockCallback, expirationMinutes, expirationCallback) {
            lockService.clearLockTimers();
            
            warningTimer = $timeout(function () {
                lockService.showLockExpirationWarning(refreshLockCallback, expirationMinutes, expirationMinutes - warningMinutes);
            }, warningMinutes * 60 * 1000);

            expirationTimer = $timeout(function () {
                expirationCallback();
            }, expirationMinutes * 60 * 1000);
        },

        showLockExpirationWarning: function (refreshLockCallback, expirationMinutes, remainingMinutes) {
            var expirationTime = new Date();
            expirationTime.setMinutes(expirationTime.getMinutes() + remainingMinutes);
            var confirmationMessage = 'Your edit lock is about to expire.  Do you want to extend your edit session? <br/><br/>Select \'Yes\' to extend your session by ' + expirationMinutes + ' minutes. <br/>Select \'No\' to allow your session to expire in ' + remainingMinutes + ' minutes at ' + expirationTime.toLocaleTimeString() + '.';
            ConfirmDialog("Confirm Lock Extension", confirmationMessage, function () {
                // Yes
                refreshLockCallback();
            });
        },

        /* Handles the processing of a response from a POST request for locking or refreshing a lock. */
        processResponse: function (response, warningMinutes, refreshLockCallback, expirationMinutes, expirationCallback, clearScopeLockTimersCallback, postSuccessCallback, isRefresh) {
            if (response.data.Status) {
                clearScopeLockTimersCallback();
                lockService.startLockTimers(warningMinutes, refreshLockCallback, expirationMinutes, expirationCallback);
                if (postSuccessCallback != null) {
                    postSuccessCallback();
                }
            } else {
                $(document).trigger("HIDE_LOADING_BOX");
                response.data.Title = isRefresh ? 'Failed to Refresh Lock. ' : 'Enable Editing Failed. ';
                DisplaySimpleExceptionDialog(response.data);
            }
        },

        /* Handles the processing of a response from a POST request for releasing a lock. */
        processResponseRelease: function (response, clearScopeTimersCallback) {
            if (clearScopeTimersCallback != null) {
                clearScopeTimersCallback();
            }

            if (!response.data.Status) {
                response.data.Title = 'Failed to Release Lock';
                DisplaySimpleExceptionDialog(response.data);
            }
        },

        /* Handles the error that occurs when a POST request was rejected, e.g. something went wrong during the request. */
        requestRejected: function() {
            $(document).trigger("HIDE_LOADING_BOX");
            var error = { Title: 'System Error', Message: 'An unknown error occurred with the locking/unlocking process.<br />Please try again later.' };
            DisplaySimpleExceptionDialog(error);
        }
    };

    return lockService;

}]);
