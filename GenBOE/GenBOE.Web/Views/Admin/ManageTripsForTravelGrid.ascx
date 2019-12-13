<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ManageTripsForTravelGridModelView>" %>

<script type="text/javascript">
    var ManageTripsForTravelGridWidget = new Widget('ManageTripsForTravelGrid', '<%= ViewData["READONLY"] %>'.isTrue());

    ManageTripsForTravelGridWidget.Initialize = function () {
        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('[name=PageControls]');
        pagingData.action = function(data) {
            delete data.TripsForTravelCollection;
            $(document).trigger('PAGE_TRIP_RESULTS', data);
        };

        pagingData.type = "Arrow";

        ManageTripsForTravelGridWidget.AddPaging(pagingData);

        ManageTripsForTravelGridWidget.sortField = '<%: ViewData["SortField"] %>';
        ManageTripsForTravelGridWidget.sortDirection = '<%: ViewData["SortDirection"] %>';

    };

    $('#ManageTripsForTravelGrid th a').click(function () {
        var sortField = $(this).parent().data('sortby');

        var data = {};
        data.inSortField = sortField;
        data.inSortDirection = 'Asc';

        if (ManageTripsForTravelGridWidget.sortField == sortField || ManageTripsForTravelGridWidget.sortField == '') {
            if (ManageTripsForTravelGridWidget.sortDirection == 'Asc' || ManageTripsForTravelGridWidget.sortDirection == '') {
                data.inSortDirection = 'Desc';
            }
        }
        ManageTripsForTravelGridWidget.sortField = data.inSortField;
        ManageTripsForTravelGridWidget.sortDirection = data.inSortDirection;

        $(document).trigger('RELOAD_GRID',
                            data);
    });


    ManageTripsForTravelGridWidget.BindEvents = function () {
        // Bind Select All Checkbox change event
        $('#SelectAllCheckbox').change(function () {
            if ($(this).is(':checked')) {
                $('#ManageTripsForTravelGrid tbody input.checkbox').prop('checked', true);
            }
            else {
                $('#ManageTripsForTravelGrid tbody input.checkbox').prop('checked', false);
            }

            ManageTripsForTravelGridWidget.CheckForDeleteButtonEnabled();
        });

        // Bind individual row Checkbox change events
        $('#ManageTripsForTravelGrid tbody input.checkbox').change(function () {

            ManageTripsForTravelGridWidget.CheckForDeleteButtonEnabled();
        });

        $('#ManageTripsForTravelGrid tbody tr a').click(function () {
            $(document).trigger('EDIT_TRIP', $(this).parents('tr'));
        });

        ManageTripsForTravelGridWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageTripsForTravelGridWidget.cleanDirty(); });

        ManageTripsForTravelGridWidget.registerForEvent('GET_DELETED_TRIPS', function () {
            var checkedTripIDs = [];

            $('#ManageTripsForTravelGrid tbody input.checkbox:checked').each(function () {
                checkedTripIDs.push($(this).parents('tr').data('pkid'));
            });

            $(document).trigger('DELETE_TRIPS', checkedTripIDs);
        });
    };

    ManageTripsForTravelGridWidget.CheckForDeleteButtonEnabled = function() {
        $(document).trigger('ENABLE_TRIPS_DELETE_BUTTON', $('#ManageTripsForTravelGrid tbody input.checkbox:checked').length > 0);
    };

    $(function () {
        ManageTripsForTravelGridWidget.afterDOMLoad();
        ManageTripsForTravelGridWidget.BindEvents();
        ManageTripsForTravelGridWidget.Initialize();

        ManageTripsForTravelGridWidget.CheckForDeleteButtonEnabled();
    });
</script>

<div class="manage-trips-for-travel-grid" style="overflow: auto; width: 1150px;">
    <table id="ManageTripsForTravelGrid" class="grid readonly" style="width: 1150px">
        <thead>
            <tr>
                <th class="select-checkbox" style="width: 36px"><input type="checkbox" class="checkbox" id="SelectAllCheckbox" /></th>
                <th class="trip-id" data-sortby="tripid" style="width: 70px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-TripID") %>
                        <div id="Search-TripID" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Trip ID</a>
                </th>
                <th class="departure-location" data-sortby="departure" style="width: 100px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-Departure") %>
                        <div id="Search-Departure" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Departure Loc</a>
                </th>
                <th class="per-diem-dest" data-sortby="destination" style="width: 100px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-Destination") %>
                        <div id="Search-Destination" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Destination Loc</a>
                </th>
                <th data-sortby="perdiemdest" style="width: 100px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-PerDiemDest") %>
                        <div id="Search-PerDiemDest" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Per Diem Dest</a>
                </th>
                <th class="mode" data-sortby="mode" style="width: 100px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-Mode") %>
                        <div id="Search-Mode" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Mode</a>
                </th>
                <th class="qualification" data-sortby="qualification" style="width: 100px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-Qualification") %>
                        <div id="Search-Qualification" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Qualification</a>
                </th>
                <th class="fare" data-sortby="fare" style="width: auto"><a>Fare</a></th>
                <th class="hotel" data-sortby="hotel" style="width: auto"><a>Hotel</a></th>
                <th class="mie-rate" data-sortby="mie" style="width: auto"><a>MIE Rate</a></th>
                <th class="rental-car" data-sortby="car" style="width: auto"><a>Rental Car</a></th>
                <th class="misc" data-sortby="misc" style="width:auto"><a>Misc</a></th>
                <th class="departure-code" data-sortby="departurecode" style="width: 60px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-DepartureCode") %>
                        <div id="Search-DepartureCode" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Depart Cd</a>
                </th>
                <th class="destination-code last-child" data-sortby="destinationcode" style="width: 60px">
                    <div class="search-box">
                        <%: Html.TextBox("SearchText-DestinationCode") %>
                        <div id="Search-DestinationCode" class="button search-magnify-glass-button"></div>
                    </div>
                    <br />
                    <a>Dest Cd</a>
                </th>
            </tr>
        </thead>
        <tbody>
            <% if (Model.TripsForTravelCollection.Count > 0)
               {
                   foreach (TripForTravelModelView item in Model.TripsForTravelCollection)
                   { %>
                    <tr data-pkid="<%: item.TripID %>">

                        <td style="text-overflow: clip">
                            <% if (item.inUse)
                               { %>
                                    <span id="InUse" style="display: inline-block">In Use</span>
                            <% }
                               else
                               { %>
                                    <input type="checkbox" class="checkbox" name="DeleteTrip" style="display: inline-block" />
                            <% } %>
                        
                            <input type="hidden" value="<%: item.MiscTravelRateID %>" class="MiscTravelRateID" />
                            <input type="hidden" value="<%: item.DepartureLocationID %>" class="DepartureLocationID" />
                            <input type="hidden" value="<%: item.DepartureLocationName %>" class="DepartureLocationName" />
                            <input type="hidden" value="<%: item.DepartureLocationCode %>" class="DepartureLocationCode" />
                            <input type="hidden" value="<%: item.DestinationLocationID %>" class="DestinationLocationID" />
                            <input type="hidden" value="<%: item.DestinationLocationName %>" class="DestinationLocationName" />
                            <input type="hidden" value="<%: item.DestinationLocationCode %>" class="DestinationLocationCode" />
                            <input type="hidden" value="<%: item.PerDiemID %>" class="PerDiemID" />
                            <input type="hidden" value="<%: item.PerDiemDestination %>" class="PerDiemDestination" />
                            <input type="hidden" value="<%: item.Qualification %>" class="Qualification" />
                            <input type="hidden" value="<%: item.Fare %>" class="Fare" />
                            <input type="hidden" value="<%: item.HotelRate %>" class="HotelRate" />
                            <input type="hidden" value="<%: item.MIERate %>" class="MIERate" />
                            <input type="hidden" value="<%: item.PerDiemNotes %>" class="PerDiemNotes" />
                            <input type="hidden" value="<%: item.RentalCar %>" class="RentalCar" />
                            <input type="hidden" value="<%: item.RTMIles %>" class="RTMIles" />
                            <input type="hidden" value="<%: item.FareLastUpdated %>" class="FareLastUpdated" />
                            <input type="hidden" value="<%: item.PerDiemLastUpdated %>" class="PerDiemLastUpdated" />
                            <input type="hidden" value="<%: item.TripLastUsed %>" class="TripLastUsed" />
                            <input type="hidden" value="<%: item.TripCount %>" class="TripCount" />
                            <input type="hidden" value="<%: item.inUse %>" class="inUse" />
                            <input type="hidden" value="<%: item.UpdateDateLong %>" class="UpdateDateLong" />
                            <input type="hidden" value="<%: item.PerDiemUpdateDateLong %>" class="PerDiemUpdateDateLong" />
                        </td>

                        <td><a><%: item.TripID %></a></td>
                        <td class="word-wrap" title="<%: item.DepartureLocationName%>">
                            <a><%: item.DepartureLocationName%></a>
                        </td>
                        <td class="word-wrap" title="<%: item.DestinationLocationName%>">
                            <a><%: item.DestinationLocationName%></a>
                        </td>
                        <td class="word-wrap" title="<%: item.PerDiemDestination %>">
                            <a><%: item.PerDiemDestination %></a>
                        </td>
                        <td class="word-wrap" title="<%: item.Mode%>">
                            <a><%: item.Mode%></a>
                        </td>
                        <td class="word-wrap" title="<%: item.Qualification%>">
                            <a><%: item.Qualification%></a>
                        </td>
                        <td class="text-right" title="<%: item.Fare.ToString("#.00") %>">
                            <%: item.Fare.ToString("#.00") %>
                        </td>
                        <td class="text-right" title="<%: item.HotelRate.ToString("#.00") %>">
                            <%: item.HotelRate.ToString("#.00") %>
                        </td>
                        <td class="text-right" title="<%: item.MIERate.ToString("#.00") %>">
                            <%: item.MIERate.ToString("#.00") %>
                        </td>
                        <td class="text-right" title="<%: item.RentalCar.ToString("#.00") %>">
                            <%: item.RentalCar.ToString("#.00") %>
                        </td>
                        <td class="text-right" title="<%: item.MiscRate.ToString("#.00") %>">
                            <%: item.MiscRate.ToString("#.00") %>
                        </td>
                        <td><a><%: item.DepartureLocationCode %></a></td>
                        <td><a><%: item.DestinationLocationCode %></a></td>                        
                    </tr>
                <% }
                }
                else
                { %>
                    <tr>
                        <td>
                        </td>
                        <td colspan="9">
                            There are no trips.
                        </td>
                    </tr>
            <%  } %>
        </tbody>
    </table>
</div>
