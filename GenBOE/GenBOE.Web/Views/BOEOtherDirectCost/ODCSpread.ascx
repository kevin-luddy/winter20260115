<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="GenBOE.ActionLogic.IO.Import" %>

<script type="text/javascript">
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var odcController = '<%: WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>';
    var odcElementId = '<%= ViewData["ODCID"] %>';
    
    var boeIdUrlPart = 'boe/' + '<%= ViewData["BOEID"] %>';
    var displayOdcSpreadGridUrl = CreatePostURL(workspace, odcController,
                '<%:WebConstants.ACTION_DISPLAY_BOE_ODC_SPREAD_GRID %>',
                 boeIdUrlPart + '<%= (ViewData["ODCID"] != null) ? "/odcelement/" + ViewData["ODCID"] : string.Empty %>');
    var exportODCSpreadUrl = CreatePostURL(workspace,odcController,
                    '<%: WebConstants.ACTION_EXPORT_ODC_SPREAD %>',
                    boeIdUrlPart + '/odcElement/' + odcElementId);

    var ManageODCSpread = InitializeManageODCSpreadWidget(odcElementId, displayOdcSpreadGridUrl, exportODCSpreadUrl);

    $(function () {
        AfterDomLoadManageODCSpreadWidget(ManageODCSpread);
    });
</script>

<div class="labor-spread module expanded" id="ManageODCSpread">
    <div class="module-header-data">
        ODC Spread</div>
    <div class="module-content-data expanded-content">
        <div class="import-buttons">
            <button class="ies" id="ManageODCSpread-Export" type="button">Export</button>
        </div>
        <div id="ODCSpreadGridContent" class="clear">
        </div>
    </div>
</div>