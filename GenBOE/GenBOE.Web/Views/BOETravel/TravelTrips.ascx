<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>"%>

<script type="text/javascript">

    //the form TravelTripsGridContainer is referenced in a validation message, if you change it be sure to do a search for it.
    TravelTripsContainer = new Widget("TravelTripsContainerDiv");

    TravelTripsContainer.ReloadGrid = function () {
        var data = {};

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_TRAVEL %>',
                '<%:WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS_GRID %>',
                'boe/' + '<%= ViewData["BOEID"] %>' + '<%= (ViewData["TRAVELID"] != null) ? "/travelelement/" + ViewData["TRAVELID"] : string.Empty %>'),                                                      
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            success: function (response) {
                $('#TravelTripGridContent').html(response);
                TravelTripsContainer.cleanDirty();
                refreshModule(TravelTripsContainer.Module);
            }
        });
    };

    $(function () {
        var module = $('#TravelTripsContainerDiv');
        createModule(module);
        refreshModule(module);
        CollapsibleModule(module);
        TravelTripsContainer.ReloadGrid();
    });

</script>

   <div class="travel-trips module expanded" id="TravelTripsContainerDiv">
    <div class="module-header-data">
        Trips</div>
    <div class="module-content-data expanded-content">
        <form id="TravelTripsGridContainer">
            <ul class="validation-box">
            </ul>
        </form>
        <div id="TravelTripGridContent" class="clear">
        </div>
    </div>
</div>