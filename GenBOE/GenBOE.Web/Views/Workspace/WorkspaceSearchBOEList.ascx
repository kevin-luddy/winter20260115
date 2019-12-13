<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.WorkspaceToCopyBOEListModelView>>" %>
<%@ Import Namespace="IES.Common.OfficeUtilities" %>

<script type="text/javascript">

    $(function () {
        $('#BOEToCopy-SelectAllCheckbox').click(function () {
            $(this).parents('table').find('tbody :input.checkbox').prop('checked', $(this).prop('checked'));
        });
    });

</script>
<div style="margin: 15px 0px;">
    <%if (Model.Count() > 0)
      { %>
        <div>BOEs to Copy (All WBS elements and CLINs will also be copied).</div>
        <table id="BOEToCopyGrid" class="boes-to-copy-grid readonly grid" style="width: 100%;">
            <colgroup>
                <col width="3%"/>
                <col width="10%"/>
                <col width="17%"/>
                <col width="20%"/>
                <col width="10%"/>
                <col width="20%"/>
                <col/>
            </colgroup>
            <thead>
                <tr>
                    <th class="select-checkbox">
                        <input type="checkbox" class="checkbox ignoreValidation" id="BOEToCopy-SelectAllCheckbox" />
                    </th>
                    <th class="wbsnum">
                        WBS #
                    </th>
                    <th class="wbs">
                        WBS
                    </th>
                    <th class="boetitle">
                        BOE Title
                    </th>
                    <th class="clinnum">
                        CLIN #
                    </th>
                    <th class="clin">
                        CLIN
                    </th>
                    <th class="last-child">
                        Description
                    </th>
                </tr>
            </thead>
            <tbody>
                <% foreach (WorkspaceToCopyBOEListModelView item in Model)
                   { %>
                    <tr pkid="<%: item.BOEID %>">
                        <td>
                            <input type="checkbox" class="checkbox ignoreValidation" />
                        </td>
                        <td title="<%: item.WBS_NUM %>">
                            <%: item.WBS_NUM %>
                        </td>
                        <td title="<%: item.WBS %>">
                            <%: item.WBS %>
                        </td>
                        <td title="<%: item.BOETitle %>">
                            <%: item.BOETitle %>
                        </td>
                        <td title="<%: item.CLIN_NUM %>">
                            <%: item.CLIN_NUM %>
                        </td>
                        <td title="<%: item.CLIN %>">
                            <%: item.CLIN %>
                        </td>
                        <td title="<%: RTEUtilities.TurnHTMLIntoPlainText(item.Description) %>">
                            <%: RTEUtilities.TurnHTMLIntoPlainText(item.Description) %>
                        </td>
                    </tr>
                <% } %>
            </tbody>
        </table>
    <% }
      else
      { %>
        <div><b>Selected Workspace contains no BOEs</b></div>
    <% } %>
</div>
