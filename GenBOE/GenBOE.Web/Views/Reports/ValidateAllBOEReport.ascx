<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Dtos.ValidationAllBOEModelView>" %>

<script type="text/javascript">
    // Base URL used for all the links we'll need to create on this page
    var baseUrl = CreatePostURL('<%:SiteMasterUtilities.GetCurrentWorkspace()%>', '<%: WebConstants.CONTROLLER_BOE %>', '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>', 'boe/');

    function setUrlForBoe(boeId)
    {
        $('.boe' + boeId).attr('href', baseUrl + boeId);
    }

    function setUrlForTaskElement(boeId, taskId)
    {
        $('.task' + taskId).attr('href', baseUrl + boeId + '#LMLabor/task/' + taskId);
    }

    function setUrlForODCElement(boeId, taskId)
    {
        $('.odc' + taskId).attr('href', baseUrl + boeId + '#ODC/odc/' + taskId);
    }
    function setUrlForTravelElement(boeId, taskId)
    {
        $('.travel' + taskId).attr('href', baseUrl + boeId + '#Travel/travel/' + taskId);
    }
    function setUrlForMaterialElement(boeId, taskId)
    {
        $('.material' + taskId).attr('href', baseUrl + boeId + '#Material/material/' + taskId);
    }
    $(function () {
        if (<%: Model.AllBOEs.Where(x=>x.isValid==false).Count() %> == 0) {
            $('#AllValidationReportTable tbody').html('<tr><td colspan="5"><div class="empty-grid-text">No validation errors were found.</div></td></tr>');
        }

        createModule($('#ValidateAllBOEReport'));
        refreshModule($('#ValidateAllBOEReport'));
        // Setup URLs for all the items in the table
        <% foreach (GenBOE.Dtos.ValidationBOEModelView item in Model.AllBOEs)
           { %>
        setUrlForBoe('<%: item.BOEID %>');
        <%foreach (var tasks in item.Tasks)
          {%>
        setUrlForTaskElement('<%: item.BOEID %>','<%: tasks.TaskId %>');
        <%}%>

         <%foreach (var odcs in item.Costs)
           {%>
        setUrlForODCElement('<%: item.BOEID %>','<%: odcs.TaskId %>');
        <%}%>
         <%foreach (var travel in item.Travels)
           {%>
        setUrlForTravelElement('<%: item.BOEID %>','<%: travel.TaskId %>');
        <%}%>
         <%foreach (var material in item.Materials)
           {%>
        setUrlForMaterialElement('<%: item.BOEID %>','<%: material.TaskId %>');
        <%}%>

        <% } %>

        // Handle the collapse/expand
        $('.collapsibleCell').click(function() {
            if($(this).hasClass('collapsed')) {
                $(this).removeClass('collapsed').addClass('expanded');
                $(this).parent('td').parent('tr').nextAll('tr').each( function() { 
                    if ($(this).is('.collapsibleHeaderRow')) { 
                        return false; 
                    } 
                
                    $(this).show();
                }); 
            } else {
                $(this).removeClass('expanded').addClass('collapsed');
                $(this).parent('td').parent('tr').nextAll('tr').each( function() { 
                    if ($(this).is('.collapsibleHeaderRow')) { 
                        return false; 
                    } 
                
                    $(this).hide();
                });
            }
        });
    });

</script>

<style type="text/css">
    .collapsibleHeaderRow {
        font-weight: bold;
    }
</style>

<div id="ValidateAllBOEOutter">
    <div id="ValidateAllBOEReport" class="module">
        <div class="module-header-data">
            Validate All BOEs
        </div>
        <div class="module-content-data">


            <table id="AllValidationReportTable" class="grid readonly full">
                <thead>
                    <tr>
                        <th width="5%"></th>
                        <th width="10%">BOE</th>
                        <th width="25%">Level</th>
                        <th width="20%">Section</th>
                        <th width="40%">Message</th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (GenBOE.Dtos.ValidationBOEModelView boeItem in Model.AllBOEs)
                       {
                           if ((boeItem.Tasks.Any() || boeItem.BOEHeaderMsgs.Any() || boeItem.BOECustomFieldValidationMessages.Any() || boeItem.BOECommentandApprovals.Any()
                               || boeItem.Costs.Any() || boeItem.Travels.Any() || boeItem.Materials.Any()))
                           {
                    %>
                    <tr name="boerow" class="collapsibleHeaderRow">
                        <td>
                            <div class="TaskElementsToCopy expanded collapsibleCell"></div>
                        </td>
                        <td colspan="4"><a href class="boe<%:boeItem.BOEID%>" target="_blank"><%=boeItem.BOEName%></a></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%if (boeItem.BOEHeaderMsgs.Any())
                      {
                          %>
                       <tr class="subRow">
                        <td></td>
                        <td></td>
                        <td>BOE Header</td>
                        <td></td>
                        <td></td>

                    </tr>
                    <%
                          foreach (string boemessage in boeItem.BOEHeaderMsgs)
                          {
                    %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=boemessage%></td>

                    </tr>
                    <%}
                      }


                      if (boeItem.BOECustomFieldValidationMessages.Any())
                      {%>
                    <tr class="subRow">
                        <td></td>
                        <td></td>
                        <td>BOE Custom Field Validation</td>
                        <td></td>
                        <td></td>

                    </tr>
                    <%       
                          foreach (string custMess in boeItem.BOECustomFieldValidationMessages)
                          {
                    %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=custMess%></td>

                    </tr>
                    <%}
                      }
                   
                    %>


                    <% if (boeItem.BOECommentandApprovals.Any())
                       {%>
                    <tr class="subRow">
                        <td></td>
                        <td></td>
                        <td>BOE Comments</td>
                        <td></td>
                        <td></td>

                    </tr>
                    <%       
                           foreach (string commentMessage in boeItem.BOECommentandApprovals)
                           {
                    %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=commentMessage%></td>

                    </tr>
                    <%}
                       }
                   
                    %>

                    <%if (boeItem.Tasks.Any())
                      {%>
                    <tr class="subRow">
                        <td></td>
                        <td></td>
                        <td>LM / IWTA / Sub Labor</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%
                          foreach (var boeTasks in boeItem.Tasks)
                          {
                    %>
                    <tr class="reportTitle">
                        <td></td>
                        <td></td>
                        <%if(boeTasks.TaskId !=-1){ %>
                        <td><a href class="task<%:boeTasks.TaskId%>" target="_blank"><%=boeTasks.TaskMessage%></a></td>
                        <%}else{%>
                          <td><%=boeTasks.TaskMessage%></td> 
                        <% }%>
                        <td></td>
                        <td></td>
                    </tr>
                    <%if (boeTasks.TaskElementDetails.TaskElementDetailsHeader != "")
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>

                        <td class="reportTitle"><%= boeTasks.TaskElementDetails.TaskElementDetailsHeader%></td>
                        <td></td>
                    </tr>
                    <% }
                      foreach (var boeMess in boeTasks.TaskElementDetails.TaskElementDetailValidationMessages)
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td ><%=boeMess%></td>


                    </tr>
                    <%}
                      foreach (var LaborType in boeTasks.LaborTypes)
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td title="<%=LaborType.LaborTypeHeader%>" class="reportTitle"><%=LaborType.LaborTypeHeader%></td>
                        <td></td>

                    </tr>
                    <% foreach (var laborMessage in LaborType.LaborTypeValidationMsgs)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=laborMessage%></td>

                    </tr>

                    <%}//end of loop for labormessages 
                      }//end of loop for labor types 

                          }//if statement
                      }//for each item in labor.     
                    %>

                    <%if (boeItem.Materials.Any())
                      {%>
                    <tr class="subRow">

                        <td></td>
                        <td></td>
                        <td>Materials</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%
                          foreach (var boeTasks in boeItem.Materials)
                          {
                    %>
                    <tr class="reportTitle">
                        <td></td>
                        <td></td>
                        <td><a href class="material<%:boeTasks.TaskId%>" target="_blank"><%=boeTasks.TaskMessage%></a></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%if (boeTasks.TaskElementDetails.TaskElementDetailValidationMessages.Any())
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>

                        <td  class="reportTitle"><%= boeTasks.TaskElementDetails.TaskElementDetailsHeader%></td>
                        <td></td>
                    </tr>
                    <% foreach (var boeMess in boeTasks.TaskElementDetails.TaskElementDetailValidationMessages)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=boeMess%></td>


                    </tr>
                    <%}
                      }
                      foreach (var LaborType in boeTasks.LaborTypes)
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td title="<%=LaborType.LaborTypeHeader%>" class="reportTitle"><%=LaborType.LaborTypeHeader%></td>
                        <td></td>

                    </tr>
                    <% foreach (var laborMessage in LaborType.LaborTypeValidationMsgs)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=laborMessage%></td>

                    </tr>

                    <%}//end of loop for labormessages 
                      }//end of loop for labor types 

                          }//if statement
                      }//for each item in labor.     
                    %>

                    <%if (boeItem.Costs.Any())
                      {%>
                    <tr class="subRow">

                        <td></td>
                        <td></td>
                        <td>ODC</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%
                          foreach (var boeTasks in boeItem.Costs)
                          {
                    %>
                    <tr class="reportTitle">
                        <td></td>
                        <td></td>
                        <td><a href class="odc<%:boeTasks.TaskId%>" target="_blank"><%=boeTasks.TaskMessage%></a></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%if (boeTasks.TaskElementDetails.TaskElementDetailValidationMessages.Any())
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>

                        <td class="reportTitle"><%= boeTasks.TaskElementDetails.TaskElementDetailsHeader%></td>
                        <td></td>
                    </tr>
                    <% foreach (var boeMess in boeTasks.TaskElementDetails.TaskElementDetailValidationMessages)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=boeMess%></td>


                    </tr>
                    <%}
                      }
                      foreach (var LaborType in boeTasks.LaborTypes)
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td title="<%=LaborType.LaborTypeHeader%>" class="reportTitle"><%=LaborType.LaborTypeHeader%></td>
                        <td></td>

                    </tr>
                    <% foreach (var laborMessage in LaborType.LaborTypeValidationMsgs)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=laborMessage%></td>

                    </tr>

                    <%}//end of loop for labormessages %>

                    <%
                      }//end of loop for labor types 

                          }//if statement
                      }//for each item in odc.
                    %>

                    <%if (boeItem.Travels.Any())
                      {%>
                    <tr class="subRow">

                        <td></td>
                        <td></td>
                        <td>Travel</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%
                          foreach (var boeTasks in boeItem.Travels)
                          {
                    %>
                    <tr class="reportTitle">
                        <td></td>
                        <td></td>
                        <td><a href class="travel<%:boeTasks.TaskId%>" target="_blank"><%=boeTasks.TaskMessage%></a></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <%if (boeTasks.TaskElementDetails.TaskElementDetailValidationMessages.Any())
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>

                        <td class="reportTitle"><%= boeTasks.TaskElementDetails.TaskElementDetailsHeader%></td>
                        <td></td>
                    </tr>
                    <% foreach (var boeMess in boeTasks.TaskElementDetails.TaskElementDetailValidationMessages)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=boeMess%></td>
                    </tr>
                    <%}
                      }
                      foreach (var LaborType in boeTasks.LaborTypes)
                      { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td title="<%=LaborType.LaborTypeHeader %>" class="reportTitle"><%=LaborType.LaborTypeHeader%></td>
                        <td></td>

                    </tr>
                    <% foreach (var laborMessage in LaborType.LaborTypeValidationMsgs)
                       { %>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td><%=laborMessage%></td>

                    </tr>

                    <%}//end of loop for labormessages %>

                    <%
                      }//end of loop for labor types 

                          }//if statement
                      }//for each item in odc.
                    %>

                    <%
                           }//end of validation messages
                       }//end of if
                    %>
                </tbody>
            </table>
        </div>
    </div>
</div>
