<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%: Scripts.Render("~/bundles/identification") %>
<script type="text/javascript">
    var widgetConfig = {};
    widgetConfig.ContextID = "ManageUCOT";
    widgetConfig.isReadOnly = false;
    widgetConfig.IsModule = true;
    
    ManageUCOTWidget = new GenWidget(widgetConfig);
   $(function () {
        refreshModule($('.manage-ucot.module'));
    });
</script>
<div id="ManageUCOT" class="manage-ucot module">
    <div class="module-header-data">
        Manage UCOT
    </div>
    <div class="module-content-data">
        TBE
    </div>
</div>
