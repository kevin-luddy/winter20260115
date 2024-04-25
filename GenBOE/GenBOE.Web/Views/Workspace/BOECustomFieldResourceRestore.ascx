<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.CustomFields.RestoreOptionData>" %>
<%@ Import Namespace="GenBOE.ActionLogic.CustomFields" %>

<script type="text/javascript">
    $(function () {
        $('#Close-RestoreResults').click(function () { $(document).trigger('CLOSE_RESTORE_RESULTS'); });
    });
</script>

<div class="restored-resources">
    <div>The restore of <%: ViewData["SYSTEM_LIST_NAME"] %> was successful.</div>

    <div class="title"><%: Model.OptionAdded.Count %> Resources added:</div>
	<ul>
        <% foreach (RestoreOption item in Model.OptionAdded) { %>
            <li><%: item.Name %>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.RateType.GetDescription() %>, <%: item.ElementofCost.GetDescription() %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionDeleted.Count %> Resources deleted:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionDeleted) { %>
            <li><%: item.Name%>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.RateType.GetDescription() %>, <%: item.ElementofCost.GetDescription() %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionChanged.Count %> Resources changed:</div>
    <ul>
        <% foreach (RestoreOptionChanged item in Model.OptionChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.RateType.GetDescription() %>, <%: item.ElementOfCost.GetDescription() %> changed to <%: item.ChangedToName %>, <%: item.ChangedToDesc %>, <%: item.ChangedToSegRegion %>, <%: item.ChangedToLaborType %>, <%: item.ChangedToRateType.GetDescription() %>, <%: item.ChangedToElementOfCost.GetDescription() %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionNotChanged.Count %> Resources could not be changed or deleted because they are in use by at least one BOE:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionNotChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.RateType.GetDescription() %>, <%: item.ElementofCost.GetDescription() %></li>
        <% } %>
    </ul>
</div>

<div class="buttons">
    <button id="Close-RestoreResults" class="ies" name="close-button" type="button">Close</button>
</div>