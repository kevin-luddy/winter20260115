<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<GenBOE.Web.ModelView.ODCTypesGridModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<% 
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    
    IEnumerable<ResourceDataForGridsModelView> odcResources = (IEnumerable<ResourceDataForGridsModelView>)ViewData["ODCResources"];
    IEnumerable<OtherDirectCostSpreadCurveModelView> spreadCurves = (IEnumerable<OtherDirectCostSpreadCurveModelView>)ViewData["ODCSpreadCurve"];
    IEnumerable<ResourceDataForGridsDescriptionModelView> odcResourceDescriptions = (IEnumerable<ResourceDataForGridsDescriptionModelView>)ViewData["ODCResourceDescriptions"];

%>
<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var ODCTypesWidget_ReadOnly = <%= ViewData["READONLY"] %>;
    
    //Create new Grid Widget
    ODCTypesWidget = new GridWidget("ODCTypesWidgetContainer", "ODCTypeID", ODCTypesWidget_ReadOnly);

    //Get set total.
    ODCTypesWidget.total = null;
    //chagne to an array if we wait on more than one thing.
    ODCTypesWidget.waitingBeforeSubmit = false;
    //negative number iterator for new items.
    ODCTypesWidget.newItemCount= 0;
    ODCTypesWidget.ValidationCreated = false;
    ODCTypesWidget.savedMetaData = <%= serializer.Serialize(Model) %>;
    ODCTypesWidget.descriptions = <%= serializer.Serialize(odcResourceDescriptions) %>;
    ODCTypesWidget.resources = <%= serializer.Serialize(odcResources) %>;
    
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var summaryGridHoursUpdatedEvent = '<%: WebConstants.EVENT_BOESUMMARYGRID_HOURS_UPDATED %>';
    var spreadCurvesLoad = "<%=SpreadCurves.Load.ToString()%>";
    var elementOfCostODC = <%: (int)ElementOfCostType.ODC %>;
    var spreadCurvesDiscreteCost = "<%=SpreadCurves.DiscreteCost.ToString()%>";
    var ocdElementId = '<%= ViewData["ODCID"] %>';
    var boeIdUrlPart = 'boe/' + '<%= ViewData["BOEID"] %>';
    var getPerformingOrgByNameUrl = CreatePostURL(workspace,
            '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
            '<%: WebConstants.ACTION_GET_PERFORMING_ORG_ID_BYNAME%>',
        boeIdUrlPart);
    var odcController = '<%: WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>';
    var exportODCTypeUrl = CreatePostURL(workspace,'<%: WebConstants.CONTROLLER_BOE_OTHER_DIRECT_COST %>',
        '<%: WebConstants.ACTION_EXPORT_ODC_TYPE %>',
        boeIdUrlPart + '/odcElement/');
    
    InitializeODCTypesWidget(ODCTypesWidget, spreadCurvesLoad, summaryGridHoursUpdatedEvent, elementOfCostODC, spreadCurvesDiscreteCost, getPerformingOrgByNameUrl);
   
    $(function () {
        AfterDomLoadODCTypesWidget(ODCTypesWidget, exportODCTypeUrl, ocdElementId);
    });
</script>
        <div class="addedableODCTypes" id="ODCTypesWidgetContainer">                   
                <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ODCTypesForm" }))
                   { %>
                    <ul class="validation-box"></ul>
            <div class="ODCTypeGrid">
            <table class="sortable odc-types-grid addable grid editable" id="ODCTypes">
            <thead>
                <tr>
                    <th class="sort resourceDescription" width="200px">Resource Description*</th>
                    <th class="sort performingOrgName" width="120px">Performing Org*</th>
                    <th class="sort StartDate" sortType="date" width="60px">Start Date*</th>
                    <th class="sort months" sortType="number" width="70px">#Months</th>
                    <th class="sort endDate" sortType="date" width="50px">End Date*</th>
                    <th class="sort spreadCurve" width="85px">Spread Curve*
                        <div id="SpreadCurve-Help" class="help-icon" onclick="ODCTypesWidget.ToggleHelp(this)"></div>
                        <!-- This comment is needed for the jquery animation to work in IE8... -->
                        <div id="SpreadCurve-HelpDialog" class="help-dialog" style="width: 325px;">
                            <div class="help-dialog-close"></div>
                            <div class="help-dialog-text" style="width:300px; white-space:normal; color:Black;">
                                The following options are the options for the spread curve:<br /><br /><br />
                                Discrete:  When this option is selected, the user enters monthly costs in the ODC Spread table below.  The cost on this row is disabled for input but will show the total amount of what is entered in the ODC Spread table. <br /><br />
                                Level:  When this option is selected, the amount entered in the Cost column is spread evenly across each month of the period. <br /><br />
                                Load:  When this option is selected, the amount entered in the Cost column is loaded into each month of the period of the spread. <br /><br />

                            </div>
                        </div>
                    </th>
                    <th class="sort cost" sortType="number" width="80px">$ Cost</th>
                </tr>
            </thead>
            <tbody>
                <%  
                       ulong count = 0;
                       foreach (var item in Model)
                       {
                           count++;
                %>
                <tr pkid="<%:item.ODCTypeID%>">
                    <td>
                        <select class="resource-description" orgval="<%: item.ResourceDescription%>" name="ResourceDescription" ltid="<%:item.BOESummaryText %>"  >
                        </select>
                        
                    </td>
                    <td class="performing-org">
                        <div class="perforg-selection">
                            <input type="text" name="PerformingOrg" value="<%:item.PerformingOrgName%>" class="select-perforg-autocomplete" />
                            <input type="hidden" name="PerformingOrgID" class="performingorg-id" value="<%:item.PerformingOrgID%>" perforg="<%:item.PerformingOrgName%>" />
                            <div class="inline-block attached-down-arrow-button button popup-div-button for-perf-orgs"></div>
                        </div>
                    </td>
                    <td class="text start-date">
                        <input type="text" class="StartDate" value="<%: item.StartDate %>" name="StartDate"
                             />
                    </td>
                    <td class="text numberofMonths">
                        <input type="text" class="numberofMonths" numOfMonths="<%: item.NumberOfMonths %>" value="<%: item.NumberOfMonths %>" name="NumberOfMonths"  />
                    </td>
                    <td class="text end-date">
                        <input type="text" class="EndDate" value="<%: item.EndDate %>" name="EndDate"  />
                    </td>
                    <td>
                        <select class="ODCSpreadCurveID" name="ODCSpreadCurveID" spread="<%:item.ODCSpreadCurveID%>" >
                            <option value="" />
                            <% foreach (var lt in spreadCurves)
                               { %>
                            <option value="<%: lt.SpreadCurveID %>" <%if(lt.SpreadCurveID== item.ODCSpreadCurveID){%>selected<%} %>>
                                <%: lt.SpreadCurveName%>
                            </option>
                            <%
                               } %>
                        </select>
                    </td>
                    <td class="Cost">
                        <input type="text" name="Cost" value="<%:item.Cost%>" cost="<%:item.Cost%>" class="Cost" />
                    </td>
                </tr>
                <% } %>
                <tr class="blank">
                    <td>
                        <select class="resource-description" name="ResourceDescription" ltid=""  ></select>
                    </td>
                    <td class="performing-org">
                        <div class="perforg-selection">
                            <input type="text" value="" class="select-perforg-autocomplete" name="PerformingOrg" placeholder="Perf. Org" />
                            <input type="hidden" name="PerformingOrgID" class="performingorg-id" value="" perforg="" />
                            <div class="inline-block attached-down-arrow-button button popup-div-button for-perf-orgs"></div>
                        </div>
                    </td>
                    <td class="text start-date">
                        <div class="default-text">mm/yyyy</div>
                        <input class="StartDate" maxlength="7" size="10" name="StartDate"  />
                    </td>
                    <td class="text number-of-months">
                        <div class="default-text">1</div>
                        <input class="numberofMonths" name="NumberOfMonths"  />
                    </td>
                    <td class="text end-date">
                        <div class="default-text">mm/yyyy</div>
                        <input class="EndDate" maxlength="7" size="10" name="EndDate"  />
                    </td>
                    <td class="spread-curve">
                        <div class="default-text">Add a Spread Curve</div>
                        <select class="ODCSpreadCurveID" name="ODCSpreadCurveID" >
                            <option value="" />
                            <% foreach (var lt in spreadCurves)
                               { %>
                            <option value="<%: lt.SpreadCurveID %>">
                                <%: lt.SpreadCurveName%>
                            </option>
                            <%
                               } %>
                        </select>
                        <input type="hidden" value="<%: ViewData["odcElementId"] %>" name="BOEODCElementID" />
                    </td>
                    <td class="text cost">
                        <div class="default-text">nnnnn.nn</div>
                        <input name="Cost" class="Cost" cost="0" />
                    </td>
                </tr>
            </tbody>
        </table>
                <div class="popup-div select-perforg">
                </div>
            </div>
        <% } %>
        </div>
