<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOECopyConflictsModelView>" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>


<script type="text/javascript">

    var BOECopyConflictsWidget = new Widget("BOECopyConflicts", <%: ViewData["READONLY"] %>);

     BOECopyConflictsWidget.Initialize = function() {
            BOECopyConflictsWidget.Module = $('#BOECopyConflicts');

            $(document).trigger('CHANGE_SEARCH_HEADER');
     }


     BOECopyConflictsWidget.CloseCopyConflicts = function() {
        $('#SearchResults').dialog('close');
     }

      $('#ContinueWithCopy-BOECopyConflicts').click(function() {
        var data = {};       
       
        $('#ContinueWithCopy-BOECopyConflicts').addClass('display-none');
        $('#Loader-BOECopyConflicts').removeClass('display-none');

        data.copyBOEID = '<%:Model.CopyBoeId %>'; 
        data.taskElementsToCopy = [<%:Model.TaskElementsToCopy != null && Model.TaskElementsToCopy.Any() ? String.Join(",", Model.TaskElementsToCopy) : ""%>];
        var dataToSend = JSON.stringify(data);

		  BOECopyConflictsWidget.ajaxRequest({
			  type: 'POST',
			  url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE%>',
                '<%: WebConstants.ACTION_SAVE_COPY_OF_BOE%>',
				  'boe/<%:Model.BoeId%>'),
			  contentType: 'application/json; charset=utf-8',
			  dataType: 'json',
			  data: dataToSend,
			  success: function (response) {
				  BOECopyConflictsWidget.CloseCopyConflicts();
				  location.reload();
			  },
			  error: function () {
				  $('#Loader-BOECopyConflicts').addClass('display-none');
			  }
          });
    });

     $(function () {
        BOECopyConflictsWidget.afterDOMLoad();
        BOECopyConflictsWidget.Initialize();

        $('#BOESearchResults').addClass('display-none');
        $('#BOECopyConflicts').removeClass('display-none');
        $('#BOECopyConflicts').addClass('boe-search-results');

        $('#Cancel-BOECopyConflicts').click(BOECopyConflictsWidget.CloseCopyConflicts);

     });

</script>

<div id="BOECopyConflicts" class="boe-copy-conflicts">
    <ul class="validation-box"></ul>
    <div class="boe-copy-conflicts-scrollspace">
        <div> The following conflicts exist between the BOE to be copied and your BOE. Click Continue with copy to copy the BOE. 
        Click Cancel to not copy the BOE. </div><br />

    <% if (Model.BOECopyConflictModelViews.Any()) {

           if (Model.BOECopyConflictModelViews.Where(m => m.taskElementTypeConflict.Any()).Any())
            { %>
                <div id="BOEHeader">
                    <div class="form-header ui-widget-header">BOE Header</div>
                    <% if (Model.BOECopyConflictModelViews.Where(m => m.taskElementTypeConflict.Count > 0).Count() > 0)
                           
                       {%>
                           <ul>
                            <%  foreach (BOECopyConflictModelView item in Model.BOECopyConflictModelViews.Where(m => m.taskElementTypeConflict.Any()))
                                {
                                    foreach (TaskElementDetailModelView task in item.taskElementTypeConflict)
                                    { %>
                                        <li><%:task.TaskID%> <%:task.Title%></li>
                
                                 <% } %>
                            <%  } %>
                           </ul>
                       <%} %>                                          
                </div><br />
        <% }
       
          
            if (Model.BOECopyConflictModelViews.Where(m => m.taskTitle.Length > 0).Any())
            { %>
                <div class="form-header ui-widget-header">LM Labor</div>
                <div id="LMLabor">
                <%  foreach (BOECopyConflictModelView item in Model.BOECopyConflictModelViews.Where(m => m.taskTitle.Length > 0))
                    { %>
                    <div><b>Task: <%: item.taskTitle%></b></div>
                    <%  if (item.workspaceVariableCircularReferences.Count > 0)
                        { %>
                            <div><b>This task will not be copied</b> because it contains the following Workspace Variables. These Workspace Variables include the sum of this BOE and will cause a circular reference.</div>
                            <ul>
                            <%  foreach (var workspaceVariable in item.workspaceVariableCircularReferences)
                                { %>
                                    <li><%:workspaceVariable.WorkspaceVariableName%> </li>
                            <% } %>
                            </ul><br />
                    <%      continue;
                        }
                         
                        if (item.workspaceVariableConflicts.Count > 0)
                        { %>
                            <div>
                                The following Variables from the copied BOE are defined as Workspace Variables in
                                your Workspace. The value of the Workspace Variable in your Workspace will be used
                                instead. The % Spread will be maintained for each Labor Type. The <%: ViewData["HoursLabel"] %> Spread will
                                adjust accordingly.
                            </div>
                                <table id="WorkspaceVarGrid" class="grid readonly">
                                <thead>
                                    <tr>
                                        <th class="wbs-number">Variable Name</th>
                                        <th class="wbs-number">Variable in Copied BOE</th>
                                        <th class="wbs-number">Workspace Variable in Your Workspace</th>
                                    </tr>
                                </thead>
                                <tbody>
               
                                <% foreach (var workspaceVariablePair in item.workspaceVariableConflicts)
                                    { %> 
                                    <tr>
                                        <td> <%: workspaceVariablePair.Key.WorkspaceVariableName%> </td>
                                        <td> <%: workspaceVariablePair.Key.WorkspaceVariableValue%> </td>
                                        <td> <%: workspaceVariablePair.Value.ToString("#,##0.##########")%> </td>
                                    </tr>
                                    <% } %>
                                </tbody><br />
                    
                            </table>
                    <%  }
                        
                        if (item.ordinaryVariableCircularReferences.Count > 0)
                        { %>
                            <div>The values for following Task Variables cannot be copied because they include the sum of this BOE and will cause a circular reference. A new value will need to be entered for the Task Variables.</div>
                            <ul>
                            <%  foreach (var ordinaryVariable in item.ordinaryVariableCircularReferences)
                                { %>
                                    <li><%:ordinaryVariable.OrdinaryVariableName%> </li>
                            <% } %>
                            </ul><br />
                    <%  }                       

                        if (item.resourceConflicts.Count > 0)
                        { %>
                            <div>The following Resources cannot be copied from the BOE because they do not exist in your Workspace.</div>
                            <ul>
                            <%  foreach (ResourceModelView resource in item.resourceConflicts)
                                { %>
                                    <li><%:resource.ResourceName%> <%:resource.ResourceDesc%> </li>
                            <% } %>
                            </ul><br />
                    <%  }

                        if (item.performingOrgConflicts.Count > 0)
                        { %>
                            <div>The following Performing Organizations cannot be copied from the BOE because they do not exist in your Workspace.
                                    You will need to select a valid Performing Organization for each.</div>
                            <ul>
                            <%  foreach (PerformingOrgModelView performingOrg in item.performingOrgConflicts)
                                { %>
                                    <li><%:performingOrg.PerformingOrgName%> <%:performingOrg.PerformingOrgDesc%> </li>
                            <% } %>
                            </ul><br />
                    <%  }                                                

                        if (item.taskIDConflict.Length > 0)
                        { %>
                            <div>Task ID <%:item.taskIDConflict%> cannot be copied into the BOE because it must be unique </div>
                           
                           <br />
                    <%  } %>
                <% } %>
            </div>
        <% } %>
      <% } 
         else
         { %>
            <div class="text-noresults">No BOE Conflicts found.</div>
            <div class="divider"></div>
      <% } %>

    </div>

        <div class="buttons boe-conflict-buttons">
            <button id="ContinueWithCopy-BOECopyConflicts" class="ies" type="button">Continue with copy</button>
            <div id="Loader-BOECopyConflicts" class="loader display-none"></div>
            <button id="Cancel-BOECopyConflicts" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
</div>
