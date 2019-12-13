<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ImportTripsResultsModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>

<% JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
   string serializedPerDiems = serializer.Serialize(from x in Model.ImportedPerDiems
                                                    select new
                                                    {
                                                        PerDiemID = x.PerDiemID,
                                                        PerDiemDestination = x.PerDiemDestination,
                                                        Qualification = x.Qualification,
                                                        HotelRate = x.HotelRate.ToString(),
                                                        MIERate = x.MIERate.ToString(),
                                                        PerDiemNotes = x.PerDiemNotes
                                                    });
   string serializedLocations = serializer.Serialize(Model.ImportedLocations); %>

<% bool errorsOccurred = ViewData.ContainsKey("ERRORS_OCCURRED") ? (bool)ViewData["ERRORS_OCCURRED"] : false; %>

<%: Scripts.Render("~/bundles/verification") %>

<script type="text/javascript">
    $(function () {
        ManageTripsImportVerificationWidget = {};
        ManageTripsImportVerificationWidget.data = {};

        ManageTripsImportVerificationWidget.data.ImportedTrips = <%= ViewData["SERIALIZED_DATA"] %>;
        ManageTripsImportVerificationWidget.data.ImportedPerDiems = <%= serializedPerDiems %>;
        ManageTripsImportVerificationWidget.data.ImportedLocations = <%= serializedLocations %>
        ManageTripsImportVerificationWidget.CompleteButton = $('#CompleteImportButton-ManageTrips');
        ManageTripsImportVerificationWidget.Loader = $('#CompleteImportLoader-ManageTrips');

        if (ManageTripsImportVerificationWidget.data.ImportedTrips.length < 1) {
            $('#CompleteImportButton-ManageTrips').addClass('display-none');
        }

        ManageTripsImportVerificationWidget.CompleteButton.click(function () {
            ManageTripsImportVerificationWidget.CompleteButton.addClass('display-none');   
            ManageTripsImportVerificationWidget.Loader.removeClass('display-none');
                        
            $.ajax({
                type: 'POST',
                url: window.parent.CreateSystemAdminPostURL(
                    '<%:WebConstants.CONTROLLER_ADMIN %>',
                    '<%:WebConstants.ACTION_COMPLETE_TRIPS_IMPORT %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify(ManageTripsImportVerificationWidget.data),
                success: function() {
                    window.parent.RaiseNotification("Import Successful");
                    window.parent.ManageTripsForTravelWidget.CloseDialog(window.parent.ManageTripsForTravelWidget.ImportTripsDialog);
                    window.parent.ManageTripsForTravelWidget.ReloadGridData();                    
                },
                error: function() {
                    window.parent.RaiseNotification("An error occurred during Import");
                    ManageTripsImportVerificationWidget.CompleteButton.removeClass('display-none');   
                    ManageTripsImportVerificationWidget.Loader.addClass('display-none');
                    window.parent.ManageTripsForTravelWidget.CloseDialog(window.parent.ManageTripsForTravelWidget.ImportTripsDialog);
                }
            });
        });

        $('#Back-ManageTrips').click(function() {
            window.parent.ManageTripsForTravelWidget.RedisplayImport();
        });

        if (<%:errorsOccurred ? "true" : "false"%>) {
            window.parent.ManageTripsForTravelWidget.DisplayImportError();
        } else {
            window.parent.ManageTripsForTravelWidget.ShowExpectedImportResults($('#ImportVerification'));
        }
    });
</script>

<div id="ImportVerification" class="import-verification">
    <div>The import file will make the following updates. To continue with the import, click <i>Complete Import</i>, otherwise click <i>Back</i> to import a different file or close this dialog window to not import a file.</div>
    <% if (Model.ImportedTrips.Count() > 0) { %>
    <div class="import-results">
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.AddNewTrip).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.AddNewTrip).Count()%> Trips will be added:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.AddNewTrip))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.UpdateExistingTrip).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.UpdateExistingTrip).Count()%> Trips will be updated:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.UpdateExistingTrip))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldsCannotChangeForExistingTrip).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldsCannotChangeForExistingTrip).Count()%> Trips will not be updated because the Mode, Departure Loc, Departure Loc Code, Destination, Per Diem Dest, Destination Code, or Qualification cannot change for an existing Trip:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldsCannotChangeForExistingTrip))
                   { %>
                <li>Trip ID: <%: result.TripID %>; Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MismatchedPerDiemData).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MismatchedPerDiemData).Count()%> Trips will not be added/updated because the Per Diem Dest and Qualification are the same but the Per Diem Data (Hotel, MIE Rate, Per Diem Notes) are not the same:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MismatchedPerDiemData))
                   { %>
                <li>Trip ID: <%: result.TripID > 0 ? result.TripID.ToString() : string.Empty %>; Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesExistingTrip).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesExistingTrip).Count()%> Trips will not be added because they match a trip already existing in genBOE:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesExistingTrip))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesNewTrip).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesNewTrip).Count()%> Trips will not be added because the Mode, Departure Loc, Destination and Qualification are the same:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MatchesNewTrip))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MissingRequiredField).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MissingRequiredField).Count()%> Trips will not be added or modified because a field is missing a required value:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.MissingRequiredField))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %>; Fields missing required value: <% Response.Write(string.Join(", ", result.MissingFields)); %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldContainsInvalidValue).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldContainsInvalidValue).Count()%> Trips will not be added or modified because a field contains an invalid value:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.FieldContainsInvalidValue))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %>; Field "<%: result.InvalidField %>" contains "<%: result.InvalidValue %>"</li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.TripIDDoesNotExist).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.TripIDDoesNotExist).Count()%> Trips will not be added because they have a Trip ID that does not exist in genBOE:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.TripIDDoesNotExist))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
        <div<% if (Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.InvalidMode).Count() == 0) { %> class="display-none"<% } %>>
            <div class="title"><%: Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.InvalidMode).Count()%> Trips will not be added because the mode is invalid:</div>
            <ul>
                <% foreach (ImportedTripModelView result in Model.ImportedTrips.Where(m => m.ImportType == (int)TripImportResult.InvalidMode))
                   { %>
                <li>Mode: <%: result.Mode %>; Departure Loc: <%: result.DepartureLocation %>; Per Diem Dest: <%: result.PerDiemDestination %>; Qualification: <%: result.Qualification %></li>
                <% } %>
            </ul>
        </div>
    </div>
    <% } else  { %>
    <div class="import-results"><div class="title">There are no changes to import.</div></div>
    <% } %>
    <div class="buttons">
        <button id="Back-ManageTrips" class="ies" name="back-button" type="button">Back</button>
        <button id="CompleteImportButton-ManageTrips" class="ies-action" name="complete-import-button" type="button">Complete import</button>
        <div id="CompleteImportLoader-ManageTrips" class="loader display-none"></div>
    </div>
</div>
