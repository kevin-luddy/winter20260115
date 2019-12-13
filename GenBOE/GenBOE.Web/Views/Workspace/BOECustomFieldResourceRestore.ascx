<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.CustomFields.RestoreOptionData>" %>
<%@ Import Namespace="GenBOE.ActionLogic.CustomFields" %>

<script type="text/javascript">
    $(function () {
        $('#Close-RestoreResults').click(function () { $(document).trigger('CLOSE_RESTORE_RESULTS'); });
    });
</script>

<div class="restored-resources">
    <div>The restore of <%: ViewData["SYSTEM_LIST_NAME"] %> was successful.</div>

    <div class="title"><%: Model.OptionAdded.Count %> options added:</div>

    <div class="title"><%: Model.OptionDeleted.Count %> options deleted:</div>

    <div class="title"><%: Model.OptionChanged.Count %> options changed:</div>
    <ul>
        <% foreach (RestoreOptionChanged item in Model.OptionChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.ElementOfCostName %> changed to <%: item.ChangedToID %>, <%: item.ChangedToDesc %>, <%: item.ChangedToSegRegion %>, <%: item.ChangedToLaborType %>, <%: item.ChangedToElementOfCostName %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionNotChanged.Count %> options could not be changed or deleted because they are in use by at least one BOE:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionNotChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %>, <%: item.SegRegion %>, <%: item.LaborType %>, <%: item.ElementOfCostName %></li>
        <% } %>
    </ul>
</div>

<div class="buttons">
    <button id="Close-RestoreResults" class="ies" name="close-button" type="button">Close</button>
</div>