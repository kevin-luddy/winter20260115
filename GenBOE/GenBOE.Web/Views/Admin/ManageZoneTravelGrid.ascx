<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.ManageMSTZoneTravelOriginsGridModelView>" %>

<script type="text/javascript">
    var ManageZoneTravelOriginsGridWidget = new Widget('ManageZoneTravelOriginsGrid', '<%= ViewData["READONLY"] %>'.isTrue());

    ManageZoneTravelOriginsGridWidget.Initialize = function () {
        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%: Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('[name=PageControls]');
        pagingData.action = function(data) {
            delete data.MSTZoneTravelOriginsCollection;
            $(document).trigger('PAGE_ORIGIN_RESULTS', data);
        };

        pagingData.type = "Arrow";

        ManageZoneTravelOriginsGridWidget.AddPaging(pagingData);
    };

    ManageZoneTravelOriginsGridWidget.BindEvents = function () {
        // Bind Select All Checkbox change event
        $('#SelectAllCheckbox').change(function () {
            if ($(this).is(':checked')) {
                $('#ManageZoneTravelGrid tbody input.checkbox').prop('checked', true);
            }
            else {
                $('#ManageZoneTravelGrid tbody input.checkbox').prop('checked', false);
            }

            ManageZoneTravelOriginsGridWidget.CheckForDeleteButtonEnabled();
        });

        // Bind individual row Checkbox change events
        $('#ManageZoneTravelGrid tbody input.checkbox').change(function () {
            ManageZoneTravelOriginsGridWidget.CheckForDeleteButtonEnabled();
        });

        $('#ManageZoneTravelGrid tbody tr a').click(function () {
            $(document).trigger('EDIT_ORIGIN', $(this).parents('tr'));
        });

        ManageZoneTravelOriginsGridWidget.registerForEvent('GET_DELETED_ORIGINS', function() {
            var checkedOriginIDs = [];

            $('#ManageZoneTravelGrid tbody input.checkbox:checked').each(function () {
                checkedOriginIDs.push($(this).parents('tr').data('pkid'));
            });

            $(document).trigger('DELETE_ORIGINS', checkedOriginIDs);
        })
    };

    ManageZoneTravelOriginsGridWidget.CheckForDeleteButtonEnabled = function() {
        $(document).trigger('ENABLE_ORIGINS_DELETE_BUTTON', $('#ManageZoneTravelGrid tbody input.checkbox:checked').length > 0);
    };

    $(function() {
        ManageZoneTravelOriginsGridWidget.afterDOMLoad();
        ManageZoneTravelOriginsGridWidget.BindEvents();
        ManageZoneTravelOriginsGridWidget.Initialize();
    });
</script>

<div class="manage-zone-travel-grid" style="overflow: auto; width: 950px;">
    <table id="ManageZoneTravelGrid" class="grid readonly" style="width: 950px">
        <thead>
            <tr>
                <th class="select-checkbox" style="width: 25px"><input type="checkbox" class="checkbox" id="SelectAllCheckbox" /></th>
                <th class="origin-id" data-sortby="originid" style="width: 190px">
                    <div>Origin ID</div>
                </th>
                <th class="origin-location" data-sortby="origin" style="width: 500px">
                    <div>Origin</div>
                </th>
                <th class="Site" data-sortby="site" style="width: 190px">
                    <div>Site</div>
                </th>
            </tr>
        </thead>
        <tbody>
            <%if (Model.MSTZoneTravelOriginsCollection.Any())
                {
                    foreach (GenBOE.Dtos.MSTZoneTravelOriginModelView item in Model.MSTZoneTravelOriginsCollection)
                    {%>
                    <tr data-pkid="<%: item.OriginID %>">
                        <td style ="text-overflow: clip">
                            <input type="checkbox" class="checkbox" name="DeleteOrigin" style="display: inline-block" />
                            <input type="hidden" value="<%: item.OriginID %>" class="OriginID" />
                            <input type="hidden" value="<%: item.Origin %>" class="Origin" />
                            <input type="hidden" value="<%: item.Site %>" class="Site" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ1 %>" class="ResourceIDPRZ1" />
                            <input type ="hidden" value="<%: item.ResourcePRZ1 %>" class="ResourcePRZ1" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ2 %>" class="ResourceIDPRZ2" />
                            <input type ="hidden" value="<%: item.ResourcePRZ2 %>" class="ResourcePRZ2" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ3 %>" class="ResourceIDPRZ3" />
                            <input type ="hidden" value="<%: item.ResourcePRZ3 %>" class="ResourcePRZ3" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ4 %>" class="ResourceIDPRZ4" />
                            <input type ="hidden" value="<%: item.ResourcePRZ4 %>" class="ResourcePRZ4" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ5 %>" class="ResourceIDPRZ5" />
                            <input type ="hidden" value="<%: item.ResourcePRZ5 %>" class="ResourcePRZ5" />
                            <input type ="hidden" value="<%: item.ResourceIDPRZ6 %>" class="ResourceIDPRZ6" />
                            <input type ="hidden" value="<%: item.ResourcePRZ6 %>" class="ResourcePRZ6" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ1 %>" class="ResourceIDTRZ1" />
                            <input type ="hidden" value="<%: item.ResourceTRZ1 %>" class="ResourceTRZ1" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ2 %>" class="ResourceIDTRZ2" />
                            <input type ="hidden" value="<%: item.ResourceTRZ2 %>" class="ResourceTRZ2" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ3 %>" class="ResourceIDTRZ3" />
                            <input type ="hidden" value="<%: item.ResourceTRZ3 %>" class="ResourceTRZ3" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ4 %>" class="ResourceIDTRZ4" />
                            <input type ="hidden" value="<%: item.ResourceTRZ4 %>" class="ResourceTRZ4" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ5 %>" class="ResourceIDTRZ5" />
                            <input type ="hidden" value="<%: item.ResourceTRZ5 %>" class="ResourceTRZ5" />
                            <input type ="hidden" value="<%: item.ResourceIDTRZ6 %>" class="ResourceIDTRZ6" />
                            <input type ="hidden" value="<%: item.ResourceTRZ6 %>" class="ResourceTRZ6" />
                            </td>

                        <td><a><%: item.OriginID %></a></td>
                        <td class="word-wrap" title="<%: item.Origin %>"><a><%: item.Origin %></a></td>
                        <td class ="word-wrap" title="<%: item.Site %>"><a><%: item.Site %></a></td>
                    </tr>
                    <%}
                }
                else
                {%>
                    <tr>
                        <td>
                        </td>
                        <td colspan ="3">
                            There are no origins.
                        </td>
                    </tr>
            <% } %>
        </tbody>
    </table>
</div>