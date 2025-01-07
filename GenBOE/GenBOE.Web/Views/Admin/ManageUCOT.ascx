<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import namespace="Newtonsoft.Json" %>

<script type="text/javascript">
    var ManageUCOTWidget = new Widget('ManageUCOT', false);

    $(function () {
        ManageUCOTWidget.registerForEvent('MANAGE_UCOT_LOADED', function () {
            return true;
        });
    })
</script>
<div class="section">
    <div class="title">Manage UCOT</div>
    <div class="data">
        TBE
    </div>
</div>
