<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Admin.ManageMSTZoneTravelDestinationsGridModelView>" %>

<script type="text/javascript">
    var ManageZoneTravelDestinationsGridWidget = new Widget('ManageZoneTravelDestinationsGrid', '<%= ViewData["READONLY"] %>'.isTrue());
    
    ManageZoneTravelDestinationsGridWidget.Initialize = function () {
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
            delete data.MSTZoneTravelDestinationsCollection;
            $(document).trigger('PAGE_ORIGIN_RESULTS', data);
        };

        pagingData.type = "Arrow";

        ManageZoneTravelDestinationsGridWidget.AddPaging(pagingData);
    };

    ManageZoneTravelDestinationsGridWidget.BindEvents = function () {
        $('#ManageZoneTravelDestinationsGrid tbody tr a').click(function () {
            $(document).trigger('EDIT_DESTINATION', $(this).parents('tr'));
        });
    }

    $(function() {
        ManageZoneTravelDestinationsGridWidget.afterDOMLoad();
        ManageZoneTravelDestinationsGridWidget.BindEvents();
        ManageZoneTravelDestinationsGridWidget.Initialize();
    });
</script>

<div class="manage-zone-travel-destinations-grid" style="overflow: auto; width: 950px">
    <table id="ManageZoneTravelDestinationsGrid" class="grid readonly" style="width: 950px">
        <thead>
            <tr>
                <th style="width: 1px"></th>
                <th class="destination-id" data-sortby="destinationid" style="width: 128px">
                    <div>Destination ID</div>
                </th>
                <th class="destination-location" data-sortby="destination" style="width: 490px">
                    <div>Destination</div>
                </th>
                <th class="abbreviation" data-sortby="abbreviation" style="width: 140px">
                    <div>Abbreviation</div>
                </th>
                <th class="zone" data-sortby="zone" style="width: 135px">
                    <div>Zone</div>
                </th>
            </tr>
        </thead>
        <tbody>
            <%if (Model.MSTZoneTravelDestinationsCollection.Any())
            {
                foreach (GenBOE.ActionLogic.ModelView.Admin.MSTZoneTravelDestinationModelView item in Model.MSTZoneTravelDestinationsCollection)
                { %>
                    <tr data-pkid="<%: item.DestinationID %>">
                        <td style="text-overflow: clip">
                            <input type="hidden" value="<%: item.DestinationID %>" class="DestinationID" />
                            <input type="hidden" value="<%: item.Destination %>" class="Destination" />
                            <input type="hidden" value="<%: item.Abbreviation %>" class="Abbreviation" />
                            <input type="hidden" value="<%: item.Zone %>" class="Zone" />
                        </td>
                        <td><a><%: item.DestinationID %></a></td>
                        <td class="word-wrap" title="<%: item.Destination %>"><a><%: item.Destination %></a></td>
                        <td class="word-wrap" title="<%: item.Abbreviation %>"><a><%: item.Abbreviation %></a></td>
                        <td class="word-wrap" title="<%: item.Zone %>"><a><%: item.Zone %></a></td>
                    </tr>
                <%}
            }
            else
            {%>
                <tr>
                    <td></td>
                    <td colspan="4">
                        There are no destinations.
                    </td>
                </tr>
            <%} %>
        </tbody>
    </table>
</div>