<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>"%>
   <div class="odc-types module expanded" id="ManageODCTypes">
    <div class="module-header-data">
        ODC Types</div>
    <div class="module-content-data expanded-content">
        <div class="import-buttons">
            <button class="ies" id="ManageODCType-Export" type="button">Export</button>
        </div>
        
        <div id="ODCTypeGridContent" class="clear">
         <%
                Html.RenderAction(WebConstants.ACTION_DISPLAY_BOE_ODC_TYPES_GRID,
                    WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST,
                    new { boeID = ViewData["BOEID"], odcElementID = ViewData["ODCID"] });
            %>  
        </div>
    </div>
</div>