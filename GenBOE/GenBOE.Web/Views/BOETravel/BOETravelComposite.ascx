<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var TravelElementsComposite_ReadOnly = <%= ViewData["READONLY"] %>;
    var containsOCI = <%= ViewData["ContainsOCI"] %>;
    var boeSummaryGridReloadEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';
    var saveTravelDetailsCompositeUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_BOE_TRAVEL%>',
                        '<%:WebConstants.ACTION_SAVE_EDIT_TRAVEL_DETAILS_COMPOSITE%>', 'boe/<%: ViewData["BOEID"] %>');  

    var TravelElementsComposite = InitializeTravelElementsCompositeWidget(TravelElementsComposite_ReadOnly, containsOCI, boeSummaryGridReloadEvent,
        saveTravelDetailsCompositeUrl);        

    $(function () {
		TravelElementsComposite.BannerTextWithOCI = '<%: SiteMasterUtilities.GetBannerText() %>';
		TravelElementsComposite.BannerTextWithoutOCI = '<%: SiteMasterUtilities.GetBannerText(true) %>';
        AfterDomLoadTravelElementsCompositeWidget(TravelElementsComposite); 
    });
</script>
    
<div id="TravelElementsComposite" class="travel-elements-composite composite">
    <%
        if (ViewData["TRAVELID"] != null)
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"], travelElementID = ViewData["TRAVELID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS, new { boeID = ViewData["BOEID"], travelElementID = ViewData["TRAVELID"] });
        }
        else
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_TRAVEL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_TRAVEL_TRIPS, new { boeID = ViewData["BOEID"] });
        }
    %>
    <div class="buttons-left"></div>
    <div class="buttons">
         <div class="required-note">
            <div>* required for saving as draft.</div>
            <div>** required for validating and submitting for approval</div>
        </div>
        <div class="oci-note"><b>Note: </b><span id="Travel-OCINote"></span></div>  
        <button id="Save-TravelUpdates" class="ies-action disabled" name="save-button" type="button">Save</button>
        <div id="Loader-TravelUpdates" class="loader display-none"></div>
        <button id="Cancel-TravelUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <div class="buttons-right"></div>
</div>
