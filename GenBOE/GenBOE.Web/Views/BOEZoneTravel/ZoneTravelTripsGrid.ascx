<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.ActionLogic.ModelView.BOEZoneTravelTripsGridModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    ICollection<MSTZoneTravelDestinationDTO> DestinationStates = (ICollection<MSTZoneTravelDestinationDTO>)ViewData["DestinationStates"];
    ICollection<MSTZoneTravelOriginDTO> Origins = (ICollection<MSTZoneTravelOriginDTO>)ViewData["Origins"];
    IEnumerable<MSTTravelMode> Modes = (IEnumerable<MSTTravelMode>)ViewData["MSTTravelModes"];

    ICollection<BOECustomFieldModelView> customfields = (ICollection<BOECustomFieldModelView>)ViewData["CustomFields"];
%>

<script type="text/javascript">
    var ZoneTravelTripsWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    //Create new Grid Widget
    ZoneTravelTripsWidget = new GridWidget("ZoneTravelTripsWidgetContainer", "ZoneTravelTripID", ZoneTravelTripsWidget_ReadOnly);

    ZoneTravelTripsWidget.newItemCount = -1;    
    ZoneTravelTripsWidget.NotApplicableString = '<%= Constants.NOT_APPLICABLE %>';
    ZoneTravelTripsWidget.IsMulti = '<%: (ViewBag.IsMulti ?? false).ToString() %>';

    ZoneTravelTripsWidget.ZoneNoAirfare = <%:(int)MSTTravelMode.ZoneNoAirfare%>;
    ZoneTravelTripsWidget.ZoneAirfare = <%:(int)MSTTravelMode.ZoneAirfare%>;
    ZoneTravelTripsWidget.NonzoneDomestic = <%:(int)MSTTravelMode.NonZoneDomestic%>;
    ZoneTravelTripsWidget.NonzoneInternational = <%:(int)MSTTravelMode.NonZoneInternational%>;

    InitializeZoneTravelTripsWidget(ZoneTravelTripsWidget);

    $(function () {
        if(ZoneTravelTripsWidget_ReadOnly) {
            $(".ZoneTravelTrips .zonetravelbuttons button").addClass("disabled").hide();
        }

        var taskStartDate = $('#ZoneTravelStartDate').val().replace("/", "-");
        var taskEndDate = $('#ZoneTravelEndDate').val().replace("/", "-");
        
        var verifyInputsUrl = CreatePostURL(workspace, boeZoneTravelController, '<%:WebConstants.ACTION_VERIFY_CALCULATE_ZONE_TRAVEL_TRIP%>', 'boe/' + boeId);// + '?taskStartDate=' + taskStartDate + '&taskEndDate=' + taskEndDate);

        AfterDomLoadZoneTravelTripsWidget(ZoneTravelTripsWidget, verifyInputsUrl, taskStartDate, taskEndDate);
        
        $('#Mode-select').on('change', function () {
            if(this.value == '<%: (int)MSTTravelMode.ZoneNoAirfare %>' || this.value == '<%: (int)MSTTravelMode.ZoneAirfare %>') {
                $('.zone-inputs').removeClass('display-none');
                $('.nonzone-inputs').addClass('display-none');
                $('.shared-inputs').removeClass('display-none');
            }
            else if (this.value == '<%: (int)MSTTravelMode.NonZoneDomestic %>' || this.value == '<%: (int)MSTTravelMode.NonZoneInternational %>') {
                $('.zone-inputs').addClass('display-none');
                $('.nonzone-inputs').removeClass('display-none');
                $('.shared-inputs').removeClass('display-none');
                if (this.value == '<%: (int)MSTTravelMode.NonZoneDomestic %>') {
                    $('.nonzone-domestic-only').removeClass('display-none');
                    $('.nonzone-international-only').addClass('display-none');
                }
                else {
                    $('.nonzone-international-only').removeClass('display-none');
                    $('.nonzone-domestic-only').addClass('display-none');
                }
            }
            else {
                $('.zone-inputs').addClass('display-none');
                $('.nonzone-inputs').addClass('display-none');
                $('.shared-inputs').addClass('display-none');
            }
        });
    });
</script>

<input type="hidden" id="StartDate" value="<%:ViewData["BOEStartDate"]%>" />
<input type="hidden" id="EndDate" value="<%:ViewData["BOEEndDate"]%>" />

<div class="ZoneTravelTrips" id="ZoneTravelTripsWidgetContainer">
    <div class="zonetravelbuttons">
        <button class="ies disabled" name="delete-button" type="button">Delete</button>
        <button name="add-button" class="ies" type="button">+ Add</button>
    </div>
    
    <table class="sortable grid readonly" id="ZoneTravelTrips">
        <thead>
            <tr>
                <th class="delete">
                    <input type="checkbox" /></th>
                <th width="30" class="sort ID" sorttype="number">ID</th>
                <th class="sort perfOrg">Perf Org.</th>

                <%if (ViewBag.IsMulti)
                    { %>
                        <th class="sort dynamic">Clin</th>
                        <th class="sort dynamic">Wbs</th>
                <%  } %>

                <th class="sort nonZoneResource">Resource</th>
                <th class="sort modeName">Mode</th>
                <th class="sort dynamic">Departure Loc</th>
                <th class="sort dynamic">Destination</th>
                <th width="45" class="sort zone">Zone</th>
                <th class="sort purpose">Purpose</th>
                <th class="sort dateOfEst">Date of Estimate</th>
                <th class="sort tripDate">Trip Date</th>
                <th class="sort numOF" sorttype="number"># People</th>
                <th class="sort numOF" sorttype="number"># Days</th>
                <th class="sort numOF" sorttype="number"># Cars</th>
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
                        {
                            if (cf.IsOpenEnded)
                            { %>
                                <input type="hidden" name="customField" value="<%: cf.OpenEndedValue %>" customfieldid="<%: cf.CustomFieldID %>" customfieldvalueid="<%: cf.CustomFieldValueID %>" updatedatelong="<%: cf.UpdateDateLong %>" selectionid="<%: cf.SelectionID %>" openended="true" />
                            <% }
                            else
                            { %>
                                <input type="hidden" name="customField" value="<%: cf.CustomFieldValueID %>" updatedatelong="<%: cf.UpdateDateLong %>" selectionid="<%: cf.SelectionID %> " />
                            <% } %>
                    <%}; %>
                </td>
                <td class="PerfOrg" perforgname="<%: item.PerformingOrgName %>" perforgid="<%:item.PerformingOrgID %>">
                    <a><%: item.PerformingOrgName %></a>
                </td>
                
                <%if (ViewBag.IsMulti)
                    { %>
                        <td class="Clin" clintext="<%: item.ClinText %>" clinid="<%: item.ClinId %>"><a><%: item.ClinText %></a></td>
                        <td class="Wbs" wbstext="<%: item.WbsText %>" wbsid="<%: item.WbsId %>"><a><%: item.WbsText %></a></td>
                <%  } %>

                <td class="NonZoneResource" nonzoneresourcename="<%: item.NonZoneResourceName %>" nonzoneresourceid="<%:item.NonZoneResourceID %>">
                    <a><%: item.ResourceIdForExport %> 
                        <%if (!string.IsNullOrEmpty(item.SecondaryResourceIdForDisplay))
                            {%>
                        , 
                        <%:item.SecondaryResourceIdForDisplay %>
                        <%} %>
                    </a>
                </td>
                <td class="Mode" modename="<%: item.ModeID.GetDescription() %>" modeid="<%: (int)item.ModeID %>">
                    <a><%: item.ModeID.GetDescription() %></a>
                </td>
                <td class="DepartureName" departurename="<%: item.DepartureName %>" originname="<%: item.OriginName %>" originid="<%: item.OriginID %>" fromlocation="<%: item.FromLocation %>">
                    <a><%: item.DepartureName %></a>
                </td>
                <td class="DestinationName" destinationname="<%: item.DestinationName%>" destinationcity="<%: item.DestinationCity %>" destinationstatename="<%: item.DestinationStateName %>" destinationstateid="<%:item.DestinationStateID %>" tolocation="<%: item.ToLocation %>">
                    <a><%: item.DestinationName%></a>
                </td>
                <td class="Zone" zone="<%: item.Zone %>">
                    <% if (item.ModeID == MSTTravelMode.ZoneNoAirfare || item.ModeID == MSTTravelMode.ZoneAirfare)
                        { %> <%: item.Zone %> <% }
                    else
                    { %><%: Constants.NOT_APPLICABLE %><% } %>
                </td>
                <td class="Purpose" purpose="<%: item.Purpose %>">
                    <%: item.Purpose %>
                </td>
                <td class="DateOfEst" dateofest="<%: item.DateOfEstimate.ToString("MM/yyyy") %>">
                    <%: item.DateOfEstimate.ToString("MM/yyyy") %>
                </td>
                <td class="TripDate" tripdate="<%: item.EstTripDate.ToString("MM/yyyy")%>">
                    <%: item.EstTripDate.ToString("MM/yyyy")%>
                </td>
                <td class="numOfPeople" numofpeople="<%: item.NumOfPeopleStr %>">
                    <%: item.NumOfPeopleStr %>
                </td>
                <td class="numOfDays" numofdays="<%: item.NumOfDaysStr %>">
                    <%: item.NumOfDaysStr %>
                </td>
                <td class="numOfCars" numofcars="<%: item.NumOfCarsStr %>">
                    <%:item.NumOfCarsStr %>
                </td>
                <td class="costOfTrip" cost="<%: item.Cost %>" airfareestimate="<%: Math.Round(item.AirfareEst ?? 0m, 2) %>" perdiemdaily="<%: Math.Round(item.PerDiemDaily ?? 0m, 2) %>" carrentaltrans="<%: Math.Round(item.CarRentalTrans ?? 0m, 2) %>">
                    <% if (item.ModeID == MSTTravelMode.NonZoneDomestic || item.ModeID == MSTTravelMode.NonZoneInternational)
                    { %> <%:item.Cost.ToString("C2") %> <% }
                    else
                    { %><%:Constants.NOT_APPLICABLE%><% } %>
                </td>

            </tr>
            <%  } %>
        </tbody>
    </table>

    <div id="addEditTripDialog" style="display: none" class="add-trip-dialog">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "addEditTripDialogForm" }))
            { %>        
        <div id="addEditTripValidation">
            <ul class="validation-box"></ul>
        </div>
        <div class="form-row">
            <div class="form-label">
                Mode*
            </div>
            <div class="form-element">
                <select id="Mode-select" name="mode" class="three-quarter">
                    <% foreach (MSTTravelMode mode in Modes)
                        { %>
                        <option value="<%:(int)mode%>" text="<%:mode.GetDescription()%>"><%:mode.GetDescription()%></option>
                    <% } %>
                </select>
            </div>
        </div>
        <div class="shared-inputs display-none">
            <div class="form-row">
                <div class="form-label">
                    ID   
                <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="TravelTripID-HelpDialog" class="help-dialog" style="width: 200px; margin-top: 40px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            This field is used for identifying related trips. Enter a value from 1 to 9999.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <input type="hidden" name="TravelTripID" />
                    <input type="text" name="GroupID" class="quarter" maxlength="4"/>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Performing Org*</div>
                <div class="form-element">
                    <div id="perforgSelection" class="perforgSelection">
                        <input type="text" name="PerformingOrgName" class="select-perforg-autocomplete quarter" value="" />
                        <input type="hidden" name="PerformingOrgID" class="performingorg-id" perforg="" value="" />
                        <div id="perform-select" class="inline-block attached-down-arrow-button button popup-div-button for-perf-orgs"></div>
                    </div>
                </div>
            </div>
        <%if (ViewBag.IsMulti)
            { %>
            <div class="form-row">
                <div class="form-label">Clin</div>
                <div class="form-element">
                    <div id="clinSelection" class="clinSelection">
                        <input type="text" name="ClinName" class="select-clin-autocomplete quarter" />
                        <input type="hidden" name="ClinId" class="clin-id" clin="" />
                        <div id="clin-select" class="inline-block attached-down-arrow-button button popup-div-button for-clins"></div>
                    </div>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Wbs</div>
                <div class="form-element">
                    <div id="wbsSelection" class="wbsSelection">
                        <input type="text" name="WbsName" class="select-wbs-autocomplete quarter" />
                        <input type="hidden" name="WbsId" class="wbs-id" wbs="" />
                        <div id="wbs-select" class="inline-block attached-down-arrow-button button popup-div-button for-wbss"></div>
                    </div>
                </div>
            </div>
        <%  } %>
        </div>
        <div class="zone-inputs display-none">
            <div class="form-row">
                <div class="form-label">
                    Origin Point*
                <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="Origin-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            If you cannot find the Origin that you need, please contact your Workspace Administrator.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <select id="Origin-select" name="OriginID" class="three-quarter">
                        <option value=""></option>
                        <% foreach (MSTZoneTravelOriginDTO origin in Origins)
                            { %>
                            <option value="<%:origin.OriginID%>" text="<%:origin.Origin%>"><%:origin.Origin%></option>
                        <% } %>
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Destination City*</div>
                <div class="form-element">
                    <input type="text" name="DestinationCity" maxlength="35" class="three-quarter" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    Destination State*
                </div>
                <div class="form-element">
                    <select id="Destination-select" name="DestinationID" class="three-quarter">
                        <option value=""></option>
                        <% foreach (MSTZoneTravelDestinationDTO destination in DestinationStates)
                            { %>
                            <option value="<%:destination.DestinationID%>" text="<%:destination.Destination%>" zone="<%:destination.Zone%>"><%:destination.Destination%></option>
                        <% } %>
                    </select>
                </div>
            </div>
        </div>

        <div class="nonzone-inputs display-none">

            <div class="form-row">
                <div class="form-label">Resource*</div>
                <div class="form-element">
                    <div id="nonzoneresourceSelection" class="nonzoneresourceSelection">
                        <input type="text" name="NonZoneResourceName" class="select-nonzoneresource-autocomplete quarter" value="" />
                        <input type="hidden" name="NonZoneResourceID" class="nonzoneresource-id" nonzoneresource="" value="" />
                        <div id="nonzoneresource-select" class="inline-block attached-down-arrow-button button popup-div-button for-nonzone-resources"></div>
                    </div>
                </div>
            </div>
            
            <div class="form-row">
                <div class="form-label">From*</div>
                <div class="form-element">
                    <input type="text" name="FromLocation" maxlength="150" class="three-quarter" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">To*</div>
                <div class="form-element">
                    <input type="text" name="ToLocation" maxlength="150" class="three-quarter" />
                </div>
            </div>
        </div>

        <div class="shared-inputs display-none">
            <div class="form-row">
                <div class="form-label">Purpose*</div>
                <div class="form-element">
                    <input type="text" name="Purpose" maxlength="35" class="full" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    Date of Est.
                    <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="DateOfEst-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            If this is left blank, it will be set to today's date.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <div class="default-text">mm/yyyy</div>
                    <input name="DateOfEst" id="DateOfEst" maxlength="7" class="quarter" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Est. Trip Date*</div>
                <div class="form-element">
                    <div class="default-text">mm/yyyy</div>
                    <input name="TripDate" id="TripDate" maxlength="7" class="quarter" />
                    <input type="hidden" name="TripDateWithDaysAdded" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    # People*
                 <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="NumPeople-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Number of people traveling during each trip. Enter a value from -999 to -1 or 1 to 999.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <input type="text" name="NumOfPeople" class="quarter" maxlength="10" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">
                    # Days*
                 <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="NumDays-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Number of days for each trip. Enter a value from -999 to -1 or 1 to 999.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <input type="text" name="NumOfDays" class="quarter" maxlength="10" />
                </div>
            </div>
        </div>

        <div class="nonzone-inputs display-none">
            <div class="form-row">
                <div class="form-label">
                    # Cars*
                    <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="NumCars-HelpDialog" class="help-dialog" style="width: 200px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Number of rental cars needed for each trip. Enter a value from -999 to -1 or 1 to 999.
                        </div>
                    </div>
                </div>
                <div class="form-element">
                    <input type="text" name="NumOfCars" class="quarter" maxlength="10" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Airfare Estimate*</div>
                <div class="form-element">
                    <input type="text" name="AirfareEstimate" maxlength="50" class="three-quarter" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Per Diem Daily*</div>
                <div class="form-element">
                    <input type="text" name="PerDiemDaily" maxlength="50" class="three-quarter" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Car Rental/Trans*</div>
                <div class="form-element">
                    <input type="text" name="CarRentalTrans" maxlength="50" class="three-quarter" />
                </div>
            </div>
            <div class="nonzone-domestic-only">
                <div class="form-row">
                    <div class="form-label">Travel Agency Fee</div>
                    <div class="form-element">
                        <div id="DomesticTravelAgencyFee" data-cost="<%:ViewData["DomesticTravelAgencyFee"]%>"><%:((decimal)ViewData["DomesticTravelAgencyFee"]).ToString("C2")%></div>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">Misc/Other Costs</div>
                    <div class="form-element">
                        <div id="MiscOtherCosts" data-cost="<%:ViewData["MiscOtherCosts"]%>"><%:((decimal)ViewData["MiscOtherCosts"]).ToString("C2")%></div>
                    </div>
                </div>
            </div>
            <div class="nonzone-international-only">
                <div class="form-row">
                    <div class="form-label">Travel Agency Fee</div>
                    <div class="form-element">
                        <div id="InternationalTravelAgencyFee" data-cost="<%:ViewData["InternationalTravelAgencyFee"]%>"><%:((decimal)ViewData["InternationalTravelAgencyFee"]).ToString("C2")%></div>
                    </div>
                </div>
            </div>
        </div>

        <div class="shared-inputs display-none">
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
                    int openEndedId = -1;
                    string openEndedValue = string.Empty;
                    if (customField.CustomFieldMetaData.isOpenEnded)
                    {%>
                        <input type="text" customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" customfieldvalueid="<%: openEndedId %>" selectionid="<%:selectedID%>" updatedatelong="<%:updateDateLong %>" openended="true" class="customField TaskElementCustomField" maxlength="250" value="<%: openEndedValue %>" />  
                    <%}
                    else
                    {%>
                        <select customfieldid="<%:customField.CustomFieldMetaData.CustomFieldID %>" name="TaskElement-CF<%:customField.CustomFieldMetaData.CustomFieldID%>" updatedatelong="<%:updateDateLong %>" openended="false" class="customField TaskElementCustomField" selectionid="<%:selectedID%>">
                            <option value=""></option>
                            <% foreach (BOECustomFieldOptionModelView option in customField.CustomFieldOptions)
                               { %>
                            <option value="<%:option.CustomFieldOptionID %>" <%if (selectedOptionID == option.CustomFieldOptionID)
                                                                               {%>selected="selected"
                                <% } %>><%:option.ID%>-<%:option.Description%></option>
                            <% } %>
                        </select>
                    <%}%>
                </div>
            </div>
            <%      }  // end foreach %>
            <%  }  // end if %>

            <div class="form-row" id="MakeOccurrencesRow">
                <div class="form-label">
                    <input type="checkbox" name="makeOccurrences" class="make-occurrences" />Create multiple occurrences of a trip over time
                    <div class="help-icon" onclick="ZoneTravelTripsWidget.ToggleHelp(this)"></div>
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
        </div>
        <div class="popup-div select-perforg" style="display: none;"></div>
        <div class="popup-div select-nonzoneresource" style="display: none;"></div>
        <div class="popup-div select-clin" style="display: none;"></div>
        <div class="popup-div select-wbs" style="display: none;"></div>
        <% } %>
    </div>
</div>