<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var MaterialElementsComposite_ReadOnly = <%= ViewData["READONLY"] %>;
      var MaterialElementsComposite_ContainsOCI = <%= ViewData["ContainsOCI"] %>;


    var MaterialElementsComposite = new Widget("MaterialElementsComposite", MaterialElementsComposite_ReadOnly);

    MaterialElementsComposite.UpdateFields = function () {
        $("#Save-BOEMaterialUpdates").removeClass("display-none");
        $("#Loader-BOEMaterialUpdates").addClass("display-none");
        window.location.hash = 'Material';

    //Cleanup
    BOEMaterialWidget.cleanDirty();
    MaterialElementDetails.cleanDirty();
    for (widgetIndex in MaterialElementDetails.ChildWidgets) {
       MaterialElementDetails.ChildWidgets[widgetIndex].cleanDirty();
    }

    };

    
    MaterialElementsComposite.SaveBOEMaterialUpdates = function () {

        if (MaterialElementDetails.preparedForSubmit()) {
            $("#Save-BOEMaterialUpdates").addClass("display-none");
            $("#Loader-BOEMaterialUpdates").removeClass("display-none");
                
            MaterialElementsComposite.data.inDetailsWV = MaterialElementDetails.data;
      
            var dataToSend = JSON.stringify(MaterialElementsComposite.data);

            MaterialElementsComposite.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_BOE_MATERIAL%>',
                        '<%:WebConstants.ACTION_SAVE_EDIT_MATERIAL_DETAILS_COMPOSITE%>', 'boe/<%: ViewData["BOEID"] %>'),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: MaterialElementsComposite.UpdateFields,
                error: function() {
                    $("#Save-BOEMaterialUpdates").removeClass("display-none");
                    $("#Loader-BOEMaterialUpdates").addClass("display-none");
                }
            });
        }
    };



    $(function () {
        MaterialElementsComposite.afterDOMLoad();
        MaterialElementsComposite.widgetsLoaded=0;

        MaterialElementsComposite.registerForEvent("SaveBOEMaterialUpdates", MaterialElementsComposite.SaveBOEMaterialUpdates);
    
        MaterialElementsComposite.registerForEvent("MaterialWidgetLoaded", function(e){
            MaterialElementsComposite.widgetsLoaded++;
            if(MaterialElementsComposite.widgetsLoaded == 2)
            {
                $(document).trigger("AllMaterialWidgetLoaded");
            }
        });
            $("#Cancel-BOEMaterialUpdates").click(function () {

                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function() {

                        window.location.hash = 'Material';
                    },
                    null);
        });

        //Change the OCI note based off the workspace
        if (MaterialElementsComposite_ContainsOCI == true){
           $('#OCINote').html('Must not contain any classified, export controlled or third party proprietary information.');
        }
        else {
           $('#OCINote').html('Must not contain any OCI, classified, export controlled or third party proprietary information.');
        }

        MaterialElementsComposite.registerForLiveEvent('click',  "#Save-BOEMaterialUpdates:not(.disabled)", MaterialElementsComposite.SaveBOEMaterialUpdates);

         if (MaterialElementsComposite.isReadOnly()) {
            $("#Save-BOEMaterialUpdates").hide();
        }

        refreshModule($('.material-details .module'));
    });

    
</script>
    
<div id="MaterialElementsComposite" class="material-elements-composite composite">
    <%
       
        if (ViewData["MaterialID"] != null)
        {
            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_MATERIAL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"], materialID = ViewData["MaterialID"] });
        }
        else
        {

            Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_MATERIAL_ELEMENT_DETAILS, new { boeID = ViewData["BOEID"] });
           

        }
    %>
    <div class="buttons-left"></div>
    <div class="buttons">
         <div class="required-note">
            <div>* required for saving as draft.</div>
            <div>** required for validating and submitting for approval</div>
        </div>
        <div class="oci-note"><b>Note: </b><span id="OCINote"></span></div>  
        <button id="Save-BOEMaterialUpdates" class="ies-action disabled" name="save-button" type="button">Save</button>
        <div id="Loader-BOEMaterialUpdates" class="loader display-none"></div>
        <button id="Cancel-BOEMaterialUpdates" class="ies" name="cancel-button" type="button">Cancel</button>
    </div>
    <div class="buttons-right"></div>
</div>