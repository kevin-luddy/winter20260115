<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var ODC_ContainsOCI = <%= ViewData["ContainsOCI"] %>;
    var summaryGridReloadEvent = '<%:WebConstants.EVENT_BOESUMMARYGRID_RELOAD %>';

    var ODCElementsComposite = InitializeODCElementsCompositeWidget(summaryGridReloadEvent);

    $(function () {
        AfterDomLoadODCElementsCompositeWidget(ODCElementsComposite, ODC_ContainsOCI);
    });
</script>
    
<div id="ODCElementsComposite" class="odc-elements-composite composite">
    <%
        if (ViewData["ODCID"] != null)
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"], odcElementID = ViewData["ODCID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_TYPES, new { boeID = ViewData["BOEID"], odcElementID = ViewData["ODCID"] });
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_SPREAD, new { boeID = ViewData["BOEID"], odcElementID = ViewData["ODCID"] });
        }
        else
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"]});
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_TYPES, new { boeID = ViewData["BOEID"]});
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_SPREAD, new { boeID = ViewData["BOEID"]});

        }
    %>
    <div class="buttons-left"></div>
    <div class="buttons">
         <div class="required-note">
            <div>* required for saving as draft.</div>
            <div>** required for validating and submitting for approval</div>
        </div>
        <div class="oci-note"><b>Note: </b><span id="ODC-OCINote"></span></div>  
        <div id="Loader-ODCUpdates" class="loader display-none"></div>
        <button id="Cancel-ODCUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <div class="buttons-right"></div>
</div>
