<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.BOETravelTripsGridModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    IEnumerable<SegmentTypeModelView> TravelTripsegments = (IEnumerable<SegmentTypeModelView>)ViewData["TravelTripSegments"];
    IEnumerable<MiscRateModelView> Modes = (IEnumerable<MiscRateModelView>)ViewData["TravelTripModes"];
    IEnumerable<SelectListItem> Destinations = (IEnumerable<SelectListItem>)ViewData["Destinations"];
    IEnumerable<SelectListItem> Departures = (IEnumerable<SelectListItem>)ViewData["Departures"];

    ICollection<BOECustomFieldModelView> customfields = (ICollection<BOECustomFieldModelView>)ViewData["CustomFields"];
%>
<script type="text/javascript">

    var TravelTripsWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    //Create new Grid Widget
    TravelTripsWidget = new GridWidget("TravelTripsWidgetContainer", "TravelTripID", TravelTripsWidget_ReadOnly);

    //change to an array if we wait on more than one thing.
    TravelTripsWidget.waitingBeforeSubmit = false;
    //negative number iterator for new items.
    TravelTripsWidget.newItemCount= -1;
    TravelTripsWidget.ValidationCreated = false;
    TravelTripsWidget.savedMetaData = <%= serializer.Serialize(Model) %>;
    TravelTripsWidget.segments = <%= serializer.Serialize(TravelTripsegments) %>;
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var travelcontroller = '<%: WebConstants.CONTROLLER_BOE_TRAVEL%>';
    var summaryGridTravelHoursUpdatedEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_TRAVEL_HOURS_UPDATED %>';
    var travelElementCostType = <%: (int)ElementOfCostType.Travel %>;
    var calculateTravelTripsUrl = CreatePostURL(workspace, travelcontroller,'<%: WebConstants.ACTION_CALCULATE_TRAVEL_TRIPS %>','boe/' +  '<%: ViewData["BOEID"] %>')
    var summaryGridHoursUpdatedEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_HOURS_UPDATED %>';
    var getFilteredTripDataUrl = CreatePostURL(workspace,travelcontroller,'<%: WebConstants.ACTION_GET_FILTERED_TRIP_DATA %>','');

    InitializeTravelTripsWidget(TravelTripsWidget, summaryGridTravelHoursUpdatedEvent, travelElementCostType, calculateTravelTripsUrl, summaryGridHoursUpdatedEvent,
        getFilteredTripDataUrl);
    
    $(function () {
        var getTripIdUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE_TRAVEL %>',
                '<%: WebConstants.ACTION_GET_TRIP_ID %>',
                '');

        AfterDomLoadTravelTripsWidget(TravelTripsWidget, workspace, getTripIdUrl);
        
        <%
        foreach (var item in Model)
        {%>
            var trip = <%= serializer.Serialize(item)%>;
            trip.TripDateToString= "<%: item.TripDate.ToString("MM/yyyy")%>";
            TravelTripsWidget.LoadExistingTrips(trip);
        <% } %>
        
    });


</script>

<input type="hidden" id="StartDate" value="<%:ViewData["BOEStartDate"]%>" />
<input type="hidden" id="EndDate" value="<%:ViewData["BOEEndDate"]%>" />

<div class="TravelTrips" id="TravelTripsWidgetContainer">
    <div class="travelbuttons">
        <button class="ies disabled" name="delete-button" type="button">Delete</button>
        <button name="add-button" class="ies" type="button">+ Add</button>
    </div>
    <table class="sortable grid readonly" id="TravelTrips">
        <thead>
            <tr>
                <th class="delete">
                    <input type="checkbox" /></th>
                <th width="30" class="sort ID" sorttype="number">ID</th>
                <th class="sort segment">Segment</th>
                <th class="sort perfOrg">Perf Org.</th>
                <th class="sort mode">Mode</th>
                <th class="sort dynamic">Departure Loc</th>
                <th class="sort dynamic">Destination</th>
                <th class="sort purpose">Purpose</th>
                <th class="sort tripDate">Date</th>
                <th class="sort numOF" sorttype="number"># Trips</th>
                <th class="sort numOF" sorttype="number"># People</th>
                <th class="sort numOF" sorttype="number"># Days</th>
                <th class="sort tripCost" sorttype="cost">Cost</th>

            </tr>
        </thead>
        <tbody>
            <%  
                ulong count = 0;
                foreach (var item in Model)
                {
                    count++;
            %>
            <tr pkid="<%:item.TravelTripID%>" updatedatelong="<%:item.UpdateDateLong %>">
                <td class="delete">
                    <input type="checkbox" /></td>
                <td class="GroupID" groupid="<%:item.GroupID%>">
                    <a><%: item.GroupID %></a>
                    <%foreach (var cf in item.CustomFieldValues)
                      {%>
                    <input type="hidden" name="customField" value="<%:cf.CustomFieldValueID%>" updatedatelong="<%:cf.UpdateDateLong %>" selectionid="<%:cf.SelectionID %> " />
                    <%}; %>
                </td>

                <td class="Segment" segment="<%: (int)item.Segment %>">
                    <a><%: item.Segment %></a>
                </td>
                <td class="PerfOrg" perforgname="<%: item.PerformingOrgName %>" perforgid="<%:item.PerformingOrgID %>">
                    <a><%: item.PerformingOrgName %></a>
                </td>
                <td class="Mode" modename="<%: item.Mode %>" modeid="<%: item.ModeID %>">
                    <a><%: item.Mode %></a>
                </td>

                <td class="DepartureName" departurename="<%: item.DepartureName %>" departureid="<%: item.DepartureID %>">
                    <a><%: item.DepartureName %></a>
                </td>
                <td class="DestinationName" destinationname="<%: item.DestinationName%>" destinationid="<%:item.DestinationID %>" perdiemid="<%:item.PerDiemID %>" systemtripid="<%:item.SystemTripID %>">
                    <a><%: item.DestinationName%></a>
                </td>
                <td class="Purpose" purpose="<%: item.Purpose %>">
                    <%: item.Purpose %>
                </td>
                <td class="TripDate" tripdate="<%: item.TripDate.ToString("MM/yyyy")%>">
                    <%: item.TripDate.ToString("MM/yyyy")%>
                </td>
                <td class="numOfTrips" numoftrips="<%: item.numOfTrips %>">
                    <%: item.numOfTrips %>
                </td>
                <td class="numOfPeople" numofpeople="<%: item.numOfPeople %>">
                    <%: item.numOfPeople %>
                </td>
                <td class="numOfDays" numofdays="<%: item.numOfDays %>">
                    <%: item.numOfDays %>
                </td>
                <td class="costOfTrip" cost="<%: item.Cost %>">
                    <%: (((decimal)item.Cost)/100).ToString("N2") %>
                </td>

            </tr>
            <% } %>
        </tbody>
    </table>


    <div id="addEditTripDialog" style="display: none" class="add-trip-dialog">

        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "addEditTripDialogForm" }))
           { %>
        <ul class="validation-box"></ul>
        <div class="form-row">
            <div class="form-label">
                ID   
            <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="TravelTripID-HelpDialog" class="help-dialog" style="width: 200px; margin-top: 40px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        This field is used for grouping related trips and sorting.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <input type="hidden" name="TravelTripID" />
                <input type="text" name="GroupID" class="quarter" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Segment *
                <% if (((Boolean)ViewData["ShowSegmentHelpLink"]))
                   {%>
                <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="Segment-HelpDialog" class="help-dialog" style="width: 200px; margin-top: 110px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        The options are:
                                   <ul>
                                       <li>DS – Development Segment</li>
                                       <li>ES – Engineering Services Segment</li>
                                       <li>LS – Lockheed Martin Services, Inc.</li>
                                       <li>TS –Transportation and Security Solutions (Heritage TSS)</li>
                                   </ul>
                    </div>
                </div>
                <% } %>
            </div>
            <div class="form-element">
                <select id="Segment-select" name="Segment" class="quarter">
                    <option value=""></option>
                    <% foreach (SegmentTypeModelView type in TravelTripsegments)
                       {%>
                    <option value="<%:type.SegmentTypeID %>"><%:type.SegmentTypeName%></option>
                    <%} %>
                </select>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Performing Org *</div>
            <div class="form-element">
                <div id="perforgSelection" class="perforgSelection">
                    <input type="text" name="PerformingOrgName" class="select-perforg-autocomplete quarter" />
                    <input type="hidden" name="PerformingOrgID" class="performingorg-id" perforg="" />
                    <div id="perform-select" class="inline-block attached-down-arrow-button button popup-div-button for-perf-orgs"></div>
                </div>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Departure Loc*
            <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="DepartureID-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        If you cannot find the Trip that you need, please contact your Workspace Administrator.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <select id="DepartureID-select" name="DepartureID" class="three-quarter">
                    <option value=""></option>
                    <% foreach (SelectListItem departure in Departures)
                       {%>
                    <option value="<%:departure.Value%>" text="<%:departure.Text%>"><%:departure.Text%></option>
                    <%} %>
                </select>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                Destination *
            <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="Destination-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        If you cannot find the Trip that you need, please contact your Workspace Administrator.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <select id="DestinationLoc-select" name="DestinationLoc" class="three-quarter">
                    <option value=""></option>
                    <% foreach (SelectListItem destination in Destinations)
                       {%>
                    <option value="<%:destination.Value%>" text="<%:destination.Text%>"><%:destination.Text%></option>
                    <%} %>
                </select>
                <input type="hidden" name="DestinationID" />
                <input type="hidden" name="PerDiemID" />
                <input type="hidden" name="SystemTripID" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Mode *</div>
            <div class="form-element">
                <select id="Mode-select" name="Mode" class="three-quarter">
                    <option value=""></option>
                    <% foreach (MiscRateModelView mode in Modes)
                       {%>
                    <option value="<%:mode.MiscTravelRateID%>" text="<%:mode.MiscTravelRateMode %>"><%:mode.MiscTravelRateMode%></option>
                    <%} %>
                </select>
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Purpose *</div>
            <div class="form-element">
                <input type="text" name="Purpose" maxlength="35" class="full" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">Date *</div>
            <div class="form-element">
                <div class="default-text">mm/yyyy</div>
                <input type="text" name="TripDate" maxlength="100" class="quarter" />
                <input type="hidden" name="TripDateWithDaysAdded" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                # Trips *
            <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="NumTrips-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Number of trips per month. Enter a value from 1 to 999.  Negative values can be entered for credit proposals.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <input type="text" name="numOfTrips" class="quarter" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                # People *
             <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="NumPeople-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Number of people traveling during each trip. Enter a value from 1 to 999.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <input type="text" name="numOfPeople" class="quarter" />
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                # Days *
             <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="NumDays-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        Number of days for each trip. Enter a value from 1 to 999.
                    </div>
                </div>
            </div>
            <div class="form-element">
                <input type="text" name="numOfDays" class="quarter" />
            </div>
        </div>
        <!-- BEGIN CUSTOM FIELDS -->
        <%  if (customfields != null && customfields.Any())
            { %>
        <%      foreach (BOECustomFieldModelView customField in customfields)
                { %>
        <div class="form-row">
            <div class="form-label">
                <%:customField.CustomFieldMetaData.FieldName%><%if (customField.CustomFieldMetaData.isRequired)
                                                                {%>*<%} %>
            </div>
            <div class="form-element" id="TaskElementCustomFieldID">
                <% // PKID of the Xref in the DB
                                                                int selectedID = -1;
                                                                //option ID for the selected value
                                                                int selectedOptionID = -1;
                                                                string updateDateLong = "0";
                %>
                <select customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" updatedatelong="<%:updateDateLong %>" class="customField TaskElementCustomField" selectionid="<%:selectedID%>">
                    <option value=""></option>
                    <% foreach (BOECustomFieldOptionModelView option in customField.CustomFieldOptions)
                       { %>
                    <option value="<%:option.CustomFieldOptionID %>" <%if (selectedOptionID == option.CustomFieldOptionID)
                                                                       {%>selected="selected"
                        <% } %>><%:option.ID%>-<%:option.Description%></option>
                    <% } %>
                </select>
            </div>
        </div>
        <%      }  // end foreach %>
        <%  }  // end if %>
        <!-- END CUSTOM FIELDS -->
        <div class="form-row">
            <div class="form-label">
                <input type="checkbox" name="makeOccurences" class="make-occurrences" />Create multiple occurrences of a trip over time
                <div class="help-icon" onclick="TravelTripsWidget.ToggleHelp(this)"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="MakeOccurrences-HelpDialog" class="help-dialog" style="width: 200px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">
                        When selected, you can create more than one occurrence of the trip definition at periodic intervals, where you input:
                        <ul>
                            <li># Occurrences as the number of trips to be created.</li>
                            <li>Interval as the number of months between the trips.</li>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="form-element">
            </div>
        </div>
        <div class="form-row occurenceRows">
            <div class="form-label">
                # Occurrences *
            </div>
            <div class="form-element">
                <input type="text" name="numOfOccurrences" class="quarter" />
            </div>
        </div>
        <div class="form-row occurenceRows">
            <div class="form-label">
                Interval *
            </div>
            <div class="form-element">
                <input type="text" name="Interval" class="quarter" />
                months
            </div>
        </div>
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div class="buttons dialogbuttons" style="width: 450px">
                    <button id="Add-AddTrip" class="ies-action disabled" name="save-button" type="button">Save</button>
                    <div id="Loader-AddTrip" class="loader display-none"></div>
                    <button id="AddAnother-AddTrip" class="ies disabled" name="save-add-another-button" type="button">Save & add another</button>
                </div>
            </div>
        </div>
        <div class="popup-div select-perforg" style="display: none;">
        </div>
        <%} %>
    </div>

</div>
