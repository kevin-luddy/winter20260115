<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.CustomFields.RestoreOptionData>" %>
<%@ Import Namespace="GenBOE.ActionLogic.CustomFields" %>

<script type="text/javascript">
    $(function () {
        $('#Close-PerformingOrgRestoreResults').click(function () { $(document).trigger('CLOSE_PERFORMING_ORG_RESTORE_RESULTS'); });
    });
</script>

<div class="restored-performing-organizations">
    <div>The restore of <%: ViewData["SYSTEM_LIST_NAME"] %> was successful.</div>

    <div class="title"><%: Model.OptionAdded.Count %> options added:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionAdded) { %>
            <li><%: item.Name %>, <%: item.Desc %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionChanged.Count %> options changed:</div>
    <ul>
        <% foreach (RestoreOptionChanged item in Model.OptionChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %> changed to <%: item.ChangedToName %>, <%: item.ChangedToDesc %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionDeleted.Count %> options deleted:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionDeleted) { %>
            <li><%: item.Name%>, <%: item.Desc %></li>
        <% } %>
    </ul>

    <div class="title"><%: Model.OptionNotChanged.Count %> options could not be changed or deleted because they are in use by at least one BOE:</div>
    <ul>
        <% foreach (RestoreOption item in Model.OptionNotChanged) { %>
            <li><%: item.Name%>, <%: item.Desc %></li>
        <% } %>
    </ul>
</div>

<div class="buttons">
    <button id="Close-PerformingOrgRestoreResults" class="ies" name="close-button" type="button">Close</button>
</div>