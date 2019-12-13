<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">
    //the form ZoneTravelTripsGridContainer is referenced in a validation message, if you change it be sure to do a search for it.
    ZoneTravelTripsContainer = new Widget("ZoneTravelTripsContainerDiv");

    ZoneTravelTripsContainer.ReloadGrid = function () {
        var data = {};

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_ZONE_TRAVEL %>',
                '<%:WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_TRIPS_GRID %>',
                'boe/' + '<%= ViewData["BOEID"] %>' + '<%= (ViewData["ZONETRAVELID"] != null) ? "/travelelement/" + ViewData["ZONETRAVELID"] : string.Empty %>'),                                                      
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#ZoneTravelTripGridContent').html(response);
                ZoneTravelTripsContainer.cleanDirty();
                refreshModule(ZoneTravelTripsContainer.Module);
            }
        });
    };

    $(function () {
        var module = $('#ZoneTravelTripsContainerDiv');
        createModule(module);
        refreshModule(module);
        CollapsibleModule(module);
        ZoneTravelTripsContainer.ReloadGrid();
    });
</script>

<div class="zone-travel-trips module expanded" id="ZoneTravelTripsContainerDiv">
    <div class="module-header-data">
        Trips
    </div>
    <div class="module-content-data expanded-content">
        <form id="ZoneTravelTripsGridContainer">
            <ul class="validation-box">
            </ul>
        </form>
        <div id="ZoneTravelTripGridContent" class="clear">
        </div>
    </div>
</div>