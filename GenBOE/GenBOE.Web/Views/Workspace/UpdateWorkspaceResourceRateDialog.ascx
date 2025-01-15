<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.UpdateWorkspaceResourceRateModelView>" %>


<script type="text/javascript">
    // The timeout was added to deal w/ Workspace admin and others, where the dialog loaded before the rest of the page, and thus got hidden.
    window.setTimeout(function() {
        /*** This utility is for a browser alert for updating a locked workspace resource rate
        a cookie ensures single prompt per session */
        var showDialogZone = "<%:Model.ShowZoneTravelRatesDialog %>";
        var showDialogOffload = "<%:Model.ShowOffloadRatesDialog%>";
        var showDialogUCOTFactor = "<%:Model.ShowUCOTFactorDialog%>";

        if (showDialogZone == "True") {
            DisplayZoneTravelDialog(showDialogOffload);
            
        }
        
        if (showDialogOffload == "True" && showDialogZone != "True") {
            DisplayOffloadRatesDialog();
        }

		if (showDialogUCOTFactor == "True" && showDialogUCOTFactor != "True") {
			DisplayUCOTFactorDialog();
		}

    }, 300);

    function DisplayZoneTravelDialog(showDialogOffload) {
        Session.confirmDialog('Update Zone Travel Rates', 'Zone Travel rates have changed on <%:Model.LastUpdatedTimeZoneTravel %> ' + getTZString() + '.  Would you like to update the Zone Travel Rates with the current rates?  This may impact Escalation Rate, Travel Agency Rates as well as Misc/Other Daily Rate.  If Yes, you will not be able to revert to the old rates.  To update rates at a later time go to Workspace Administration > Update Zone Travel Rates.', function () {
            $.ajax({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_UPDATE_ZONE_TRAVEL_RATES %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: null,
                    success: function (response) {
                        RaiseNotification('The Zone Travel Rates were successfully updated');
                        $('a[name=UpdateZoneTravelRatesLink]').parent().addClass('display-none');

                        if (showDialogOffload == "True") {
                            DisplayOffloadRatesDialog();
                        }
                    },
                    error: function (response) {
                        RaiseNotification('The update to the Zone Travel Rates failed');
                    }
            });
        }, function () {
            // If 'No' is selected, we still need to check for the Offload Rates dialog
            if (showDialogOffload == "True") {
                // Need time out in order for new dialog to be able to show
                window.setTimeout(function () { DisplayOffloadRatesDialog(); }, 300);
            }
        });
    };

    function DisplayOffloadRatesDialog() {
        Session.confirmDialog('Update Offload Rates', 'Offload Rates have changed on <%:Model.LastUpdatedTimeOffloadRates%> ' + getTZString() + '.  Would you like to update the Offload Rates with the current rates?  If Yes, you will not be able to revert to the old rates.  To update rates at a later time go to Workspace Administration > Update Offload Rates.', function () {
            $.ajax({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_UPDATE_OFFLOAD_RATES %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: null,
                success: function (response) {
                    RaiseNotification('The Offload Rates were successfully updated');
                    $('a[name=UpdateOffloadRatesLink]').parent().addClass('display-none');
                },
                error: function (response) {
                    RaiseNotification('The update to the Offload Rates failed');
                }
            });
        });
    };

    function DisplayUCOTFactorDialog() {
		Session.confirmDialog('Update UCOT Factor', 'UCOT Factor has changed.  Would you like to update the Workspace UCOT Factor with the current System value?  If Yes, you will not be able to revert to the old UCOT Factor.  To update the UCOT Factor at a later time go to Workspace Administration > Update UCOT Factor.', function () {
			$.ajax({
				type: 'POST',
				url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
             '<%: WebConstants.CONTROLLER_WORKSPACE %>',
			 '<%: WebConstants.ACTION_UPDATE_UCOT_FACTOR %>', ''),
		 contentType: 'application/json; charset=utf-8',
		 dataType: 'json',
		 data: null,
		 success: function (response) {
			 RaiseNotification('The UCOT Factor was successfully updated');
			 $('a[name=UpdateUCOTFactorLink]').parent().addClass('display-none');
		 },
		 error: function (response) {
			 RaiseNotification('The update to the UCOT Factor failed');
		 }
	 });
		 });
    };

    //Get the string to append to the date string for either eastern
    //standard time or daylight savings time.
    function getTZString() {
        var tz = 'EST';
        var now = new Date();
        if (isDST(now)) {
            tz = 'EDT';
        }

        return tz;
    }

    //Use the standard time zone off set and the current time zone offset
    //to determine if we are in daylight savings time.
    function isDST(date) {
        return date.getTimezoneOffset() < stdTimezoneOffset(date);
    }

    //A function found on the internet to determine standard time zone offset
    function stdTimezoneOffset(date) {
        var jan = new Date(date.getFullYear(), 0, 1);
        var jul = new Date(date.getFullYear(), 6, 1);
        return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
    }
</script>