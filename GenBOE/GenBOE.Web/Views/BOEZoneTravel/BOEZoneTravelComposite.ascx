<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">
    // Get the read-only attribute passed in from the controller
    var ZoneTravelElementsComposite_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
    var containsOCI = <%= ViewData["ContainsOCI"] %>;
    var boeSummaryGridReloadEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';

    var saveZoneTravelDetailsCompositeUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_BOE_ZONE_TRAVEL%>',
                        '<%:WebConstants.ACTION_SAVE_EDIT_ZONE_TRAVEL_DETAILS_COMPOSITE%>', 'boe/<%: ViewData["BOEID"] %>');  

    var ZoneTravelElementsComposite = InitializeZoneTravelElementsCompositeWidget(ZoneTravelElementsComposite_ReadOnly, containsOCI, boeSummaryGridReloadEvent,
        saveZoneTravelDetailsCompositeUrl); 
    
	$(function () {
		ZoneTravelElementsComposite.BannerTextWithoutOCI = '<%: SiteMasterUtilities.GetBannerText(true) %>';
		ZoneTravelElementsComposite.BannerTextWithOCI = '<%: SiteMasterUtilities.GetBannerText() %>';
        AfterDomLoadZoneTravelElementsCompositeWidget(ZoneTravelElementsComposite);
    });
</script>

<div id="ZoneTravelElementsComposite" class="zone-travel-elements-composite composite">
    <%
        if (ViewData["ZONETRAVELID"] != null)
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"], travelElementID = ViewData["ZONETRAVELID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_TRIPS, new { boeID = ViewData["BOEID"], travelElementID = ViewData["ZONETRAVELID"] });
        }
        else
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ZONE_TRAVEL_TRIPS, new { boeID = ViewData["BOEID"] });
        }
    %>
    <div class="buttons-left"></div>
    <div class="buttons">
        <div class="required-note">
            <div>* required for saving as draft.</div>
            <div>** required for validating and submitting for approval</div>
        </div>
        <div class="oci-note"><b>Note: </b><span id="ZoneTravel-OCINote"></span></div>  
        <button id="Save-ZoneTravelUpdates" class="ies-action disabled" name="save-button" type="button">Save</button>
        <div id="Loader-ZoneTravelUpdates" class="loader display-none"></div>
        <button id="Cancel-ZoneTravelUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <div class="buttons-right"></div>
</div>