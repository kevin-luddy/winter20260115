<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.MOQEquationModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.Dtos" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.BOE" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView" %>
<%@ Import namespace="System.Web.Optimization" %>

<% 
    IEnumerable<WorkspaceVariableModelView> workspaceVariables = (IEnumerable<WorkspaceVariableModelView>)ViewData["WorkspaceVariables"];
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    bool IsSubContractor = (bool)ViewData["IsSubContractor"];
    int rteFieldSize = ViewBag.RteFieldSize;
    int taskElementId = Model.TaskElementId > 0 ? Model.TaskElementId : -1;
    bool showMoqQuestions = Model.MoqTemplateAnswers.Any();;
    int numberMoqQuestions = showMoqQuestions ? Model.MoqTemplateAnswers.Count : 1;
%>
<script type="text/javascript">
    var MOQEquationFieldModel = {
        BaseUrl: '<%=this.ResolveClientUrl("~/")%>',
        MoqEquationLabel: '<%=Model.MOQEquationLabel%>',
        MoqEquationType: '<%=Model.TypeOfMoqEquation%>',
        MOQType: '<%=Model.MOQType%>',
        TaskElementId: <%=Model.TaskElementId > 0 ? Model.TaskElementId : -1%>,
        ShowSearchMetricsLink: <%=Model.ShowSearchMetricsLink ? "true" : "false"%>,
        UsingTemplateBOE: <%=Model.UsingTemplateBOE ? "true" : "false"%>,
        IsReadOnly: <%=ViewData["READONLY"]%>,
        WorkspaceVariables: <%=serializer.Serialize(workspaceVariables)%>,
        MOQTypes: <%=serializer.Serialize(Model.MOQTypes.Select(x => new { id = x.Value, text = x.Text }))%>
        };
</script>
<script type="text/javascript">
    // Get the read-only attribute passed in from the controller
    var MOQEquationFieldWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
        
    //create base js object;
    var MOQEquationFieldWidget = new Widget("MOQEquationField", MOQEquationFieldWidget_ReadOnly);
    
    MOQEquationFieldWidget.initialLoad = false;
    MOQEquationFieldWidget.MarkedEquation = "";
    MOQEquationFieldWidget.WorkspaceVariables = <%= serializer.Serialize(workspaceVariables) %>;
    MOQEquationFieldWidget.OrdinaryVariables = <%= serializer.Serialize(Model.TaskOrdinaryVariables) %>;
    
    MOQEquationFieldWidget.MOQCalculating = false;
    MOQEquationFieldWidget.MOQValidating = false;
    MOQEquationFieldWidget.MOQValid = false;
    MOQEquationFieldWidget.SavePending = false;

    MOQEquationFieldWidget.NewOrdinaryVariableID = -1;
    <% if (Model.TypeOfMoqEquation == MOQEquationType.Cost) { %>
    // This will avoid collisions with IDs when placed on the same page as an Hours MOQ Equation.  250 was chosen because that's the char limit in the text box
    // so you are guaranteed there will never be 250 variables.
    MOQEquationFieldWidget.NewOrdinaryVariableID = -250;
    <% } %>
    
    MOQEquationFieldWidget.HarvestWorkspaceVariablesForSave = function() {
    
        var toReturn = [];

        var workspaceVariableFields = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

        for (var ndx = 0; ndx < MOQEquationFieldWidget.WorkspaceVariables.length; ndx++)
        {
            var variableID = MOQEquationFieldWidget.WorkspaceVariables[ndx].WorkspaceVariableID;
            var variableField = workspaceVariableFields.filter('[pkid=' + variableID + ']');
            
            if (variableField.length)
            {
                toReturn.push(variableID);
            }
        }

        return toReturn;
    };

    MOQEquationFieldWidget.HarvestOrdinaryVariablesForSave = function() {
        var toReturn = [];
        var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        for (var ndx = 0; ndx < MOQEquationFieldWidget.OrdinaryVariables.length; ndx++)
        {
            var currentTaskOrdinaryVariable = MOQEquationFieldWidget.OrdinaryVariables[ndx];
            var variableID = currentTaskOrdinaryVariable.OrdinaryVariableID;
            var variableField = currentVariableInputs.filter('[pkid=' + variableID + ']');
            var variableValue;
            
            if (variableField.length && (variableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField)).length > 0)
            {
                currentTaskOrdinaryVariable.Deleted = false;
                currentTaskOrdinaryVariable.OrdinaryVariableValue = variableValue;
                currentTaskOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(variableField);

                if (variableField.attr('BOEToSum') != undefined)
                {
                    currentTaskOrdinaryVariable.BOEToSum = JSON.parse(variableField.attr('BOEToSum'));
                }

                if (variableField.attr('WBSToSum') != undefined)
                {
                    currentTaskOrdinaryVariable.WBSToSum = JSON.parse(variableField.attr('WBSToSum'));
                }

                if (variableField.attr('CLINToSum') != undefined)
                {
                    currentTaskOrdinaryVariable.CLINToSum = JSON.parse(variableField.attr('CLINToSum'));
                }

                if (variableField.attr('ResourceTypes') != undefined)
                {
                    currentTaskOrdinaryVariable.ResourceTypes = JSON.parse(variableField.attr('ResourceTypes'));
                }

                if (variableField.attr('SortBOEBy') != undefined)
                {
                    currentTaskOrdinaryVariable.SortBOEBy = variableField.attr('SortBOEBy');
                }

                if (currentTaskOrdinaryVariable.BOEToSum.length || currentTaskOrdinaryVariable.WBSToSum.length || currentTaskOrdinaryVariable.CLINToSum.length) {
                    currentTaskOrdinaryVariable.OrdinaryVariableValueType = '<%: (int)VarValueType.SumOfBOEs %>';
                }
                else {
                    currentTaskOrdinaryVariable.OrdinaryVariableValueType = '<%: (int)VarValueType.Discrete %>';
                }
            }
            else
            {
                currentTaskOrdinaryVariable.Deleted = true;
                currentTaskOrdinaryVariable.OrdinaryVariableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField);
                currentTaskOrdinaryVariable.OrdinaryVariableValueType = currentTaskOrdinaryVariable.OrdinaryVariableValueType.toString();
            }
            
            delete currentTaskOrdinaryVariable.UpdateDate;
            currentTaskOrdinaryVariable.UpdateDateLong = currentTaskOrdinaryVariable.UpdateDateLong.toString();
            toReturn.push(currentTaskOrdinaryVariable);
        }
        
        if (currentVariableInputs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++)
            {
                var variableAlreadyInReturnSet = false;
                var currentVariableInput = $(currentVariableInputs[inputNdx]);
                var currentVariableInputID = currentVariableInput.attr('pkid');
                var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInput);

                if (currentVariableInputValue.length > 0)
                {
                    for (var returnNdx = 0; returnNdx < toReturn.length; returnNdx++)
                    {
                        if (currentVariableInputID == toReturn[returnNdx].OrdinaryVariableID)
                        {
                            variableAlreadyInReturnSet =  true;
                            break;
                        }
                    }

                    if (!variableAlreadyInReturnSet)
                    {
                        var newOrdinaryVariable = {};
                        newOrdinaryVariable.OrdinaryVariableID = currentVariableInputID;
                        newOrdinaryVariable.OrdinaryVariableName = currentVariableInput.attr('name');;
                        newOrdinaryVariable.OrdinaryVariableValue = currentVariableInputValue;
                        newOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(currentVariableInput);
                        newOrdinaryVariable.DefaultSize = currentVariableInput.closest('tr').data('variable-default-size');

                        newOrdinaryVariable.BOEToSum = JSON.parse(currentVariableInput.attr('BOEToSum'));
                        newOrdinaryVariable.WBSToSum = JSON.parse(currentVariableInput.attr('WBSToSum'));
                        newOrdinaryVariable.CLINToSum = JSON.parse(currentVariableInput.attr('CLINToSum'));
                        newOrdinaryVariable.ResourceTypes = JSON.parse(currentVariableInput.attr('ResourceTypes'));
                        newOrdinaryVariable.SortBOEBy = currentVariableInput.attr('SortBOEBy');

                        if (newOrdinaryVariable.BOEToSum.length || newOrdinaryVariable.WBSToSum.length || newOrdinaryVariable.CLINToSum.length) {
                            newOrdinaryVariable.OrdinaryVariableValueType = '<%: (int)VarValueType.SumOfBOEs %>';
                        }
                        else {
                            newOrdinaryVariable.OrdinaryVariableValueType = '<%: (int)VarValueType.Discrete %>';
                        }

                        toReturn.push(newOrdinaryVariable);
                    }
                }
            }
        }

        return toReturn;
    };

    MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen = function () {
        var scope = angular.element(document.getElementById('MOQHoursMOQEquationField')).scope();
        return scope.model.insertWorkspaceModalOpen;
    };
    
    MOQEquationFieldWidget.InsertMOQElementDialogClosing = function () {
        if (!MOQEquationFieldWidget.MOQValid && !MOQEquationFieldWidget.MOQValidating)
        {
            MOQEquationFieldWidget.ShowMOQValidationError();
        }
    };

    MOQEquationFieldWidget.GetAllVariableFields = function() {
        return $('#MOQEquationField #MOQEquation-Variables [wsVar], #MOQEquationField #MOQEquation-Variables [ordVar]');
    };

    MOQEquationFieldWidget.RefreshStyles = function() {
        MOQEquationFieldWidget.applyReadOnly();

        if (MOQEquationFieldWidget.isReadOnly())
        {
            $("#MOQEquationField #InsertWorkspaceVariableLink").parent().prev().remove();
            $("#MOQEquationField #InsertWorkspaceVariableLink").parent().remove();
            $("#MOQEquationField #SearchEstimatingCatalogLink").parent().prev().remove();
            $("#MOQEquationField #SearchEstimatingCatalogLink").parent().remove();

            /*
                MOQ-Type (drop-down) and MOQ-Text EFFECTIVELY inherit THEIR editability from the Task Details widget.

                This partial view contains the MOQ Equation, MOQ Type and MOQ Text fields.  MOQ Equation is NOT editable in
                a Locked state, but the other two are.  The Widget logic automatically locks access to all three, so this
                logic is added to override that for the Type and Text fields.

                The REAL issue here (and in a broader sense, throughout the UI) is that the Widgets apply their
                read-only flag to EVERY child element as a whole -- But that CANNOT work for cases (like this one) where
                different elements can have different accessibilities.  The ideal approach is to have a SET of access flags
                for different (individual) fields and/or groups of fields.  This is what the security matrix is SUPPOSED to
                be used for, but our code does not always consult it (or even consult it correctly or completely in every case).
                In addition, we often have access-determination logic in the views themselves that should really be executed
                in the view models (or at least in the controllers) and then forwarded to the views as booleans.
            */
            if (!TaskElementDetailsWidget.isReadOnly()) {
                var moqSection = $('#MOQEquationField');

                var moqTypeElement = $("#MOQType", moqSection);
                $('.replacedWidgetText', moqTypeElement.parent()).addClass('display-none');
                moqTypeElement.removeClass('display-none');

                RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
            }
        }

        refreshModule($('.task-element-details.module'));
    };

    MOQEquationFieldWidget.ValidateMOQEquation = function () {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;

        GenSession.ShowLoadingBox();
        MOQEquationFieldWidget.MOQValidating = true;

        MOQEquationFieldWidget.HideError();

        var moqEquation = $.trim($("#MOQEquationField #MOQEquation").val());

        var dataToSend = { "moqEquation" : moqEquation }
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
                '<%: WebConstants.ACTION_MOQ_VALIDATE%>', 'boe/' + '<%: ViewData["BOEID"] %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function(response, textStatus) {
                MOQEquationFieldWidget.MOQValidated(response);

                if(MOQEquationFieldWidget.isReadOnly() && '<%: ViewData["ShouldMoqReadOnlyBeReversed"] %>' == 'True')
                {
                    $('.magnifier-button').addClass('display-none');
                    $('#MoqType').children('.replacedWidgetText').remove();
                    $('.moqTypes').removeClass('display-none');

                    var moqSection = $('#MOQEquationField');
                    RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
                }
            },
            error: function(response, textStatus) {
                MOQEquationFieldWidget.MOQValidationError(response);
            }
        });
    };
    
    MOQEquationFieldWidget.MOQValidationError = function (exceptionstring) {
        MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", "A general validation error occurred. Please check the MOQ equation for validity.");
        MOQEquationFieldWidget.ShowMOQValidationError();
        MOQEquationFieldWidget.MOQValidating = false;
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
        GenSession.HideLoadingBox();
    };

    MOQEquationFieldWidget.SetMOQError = function (errorSubject, errorMessage) {
        if (errorSubject != undefined && errorMessage != undefined)
        {
            $("#MOQEquationField #MOQEquation-ErrorTitle").html(errorSubject);
            $("#MOQEquationField #MOQEquation-ErrorText").html(errorMessage);
        }
    };

    MOQEquationFieldWidget.ShowMOQValidationError = function () {
        $("#MOQEquationField #MOQEquation-Variables").hide();
        MOQEquationFieldWidget.ShowMOQError();
    };

    MOQEquationFieldWidget.ShowMOQError = function() {
        $("#MOQEquationField #MOQEquation-Error").show();
        MOQEquationFieldWidget.SetMOQResult("");
        MOQEquationFieldWidget.RefreshStyles();
    };

    MOQEquationFieldWidget.HideError = function () {
        $("#MOQEquationField #MOQEquation-Error").hide();
        MOQEquationFieldWidget.RefreshStyles();
    };
    
    MOQEquationFieldWidget.FocusError = function () {
        $("#MOQEquationField #MOQEquation-ErrorTitle").focus();
    }

    MOQEquationFieldWidget.MOQEquationPreparedForSubmit = function() {
    
        if (MOQEquationFieldWidget.MOQValidating || MOQEquationFieldWidget.MOQCalculating)
        {
            MOQEquationFieldWidget.SavePending = true;
        }

        return (!$("#MOQEquationField #MOQEquation-Error").is(":visible") &&
                !MOQEquationFieldWidget.MOQValidating &&
                !MOQEquationFieldWidget.MOQCalculating &&
                MOQEquationFieldWidget.ReadyForCalculate());
    };

    MOQEquationFieldWidget.MOQValidated = function (results) {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;
        GenSession.ShowLoadingBox();
        // Set Widget valid flag
        MOQEquationFieldWidget.MOQValid = results.Status;

        if (results.Status)
        {
            var variables = results.Variables;

            MOQEquationFieldWidget.RemoveInvalidVariables(variables);

            for (var ndx = 0; ndx < variables.length; ndx++)
            {
                if (ndx == 0)
                {
                    MOQEquationFieldWidget.MarkedEquation = variables[ndx];
                }
                else
                {
                    var defaultSize = MOQEquationFieldWidget.GetVariableDefaultSize(variables[ndx]);
                    MOQEquationFieldWidget.AddVariableField(variables[ndx], defaultSize);
                }
            }
            // Reinitialize imported historical metric data.
            $('#MOQDefaultSizes').val("");
            $('#HistoricalMetricEquation').val("");
    
            var currentVariables = MOQEquationFieldWidget.GetAllVariableFields();

            if (currentVariables.length > 0)
            {
                $("#MOQEquationField #MOQEquation-Variables").show();
                $("#MOQEquationField .moqVariableNote").removeClass('display-none');
            }
            else
            {
                $("#MOQEquationField #MOQEquation-Variables").hide();
            }
             
            MOQEquationFieldWidget.RefreshStyles();

            MOQEquationFieldWidget.CalculateMOQResult();
        }
        else
        {
            // Set the validation error text
            MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", results.Message);

            // If the user isn't inserting a variable, we'll show the error
            if (!MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen() &&
                (!TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogMST').isOpen() ||
                !TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogCommon').isOpen()))
            {
                MOQEquationFieldWidget.ShowMOQValidationError();
            }

            MOQEquationFieldWidget.SavePending = false;
        }

        MOQEquationFieldWidget.MOQValidating = false;
        GenSession.HideLoadingBox();
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
    };

    /**
    * Gets a default size value for a variable that was created as a result of importing a historical
    * metric.  Returns "" for all other variable types.
    * @param {String} currentVariable Variable name
    * @returns {String}
    */
    MOQEquationFieldWidget.GetVariableDefaultSize = function(currentVariable) {
        var metricEquation = $('#HistoricalMetricEquation').val().toUpperCase();
        var defaultSize = "";
        if (metricEquation.length) {
            var newMetricDefaultSizes = $('#MOQDefaultSizes').val();
            if (newMetricDefaultSizes != "" && metricEquation.indexOf(currentVariable.toUpperCase()) != -1) {
                // Parse out the 1st default sizes for assignment to the current variable.
                var moqVariableDefaultSizes = [];
                moqVariableDefaultSizes = JSON.parse(newMetricDefaultSizes);
                if (moqVariableDefaultSizes.length) {
                    defaultSize = moqVariableDefaultSizes[0];
                    moqVariableDefaultSizes.splice(0, 1);
                    $('#MOQDefaultSizes').val(JSON.stringify(moqVariableDefaultSizes));
                }
            }
        }
        return defaultSize;
     };

    MOQEquationFieldWidget.RemoveInvalidVariables = function(currentVariables)
    {
         var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields();

         if (currentVariableInputs.length > 0)
         {
             for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++)
             {
                var currentVariableInput = $(currentVariableInputs[inputNdx]);
                var variableStillValid = false;
                
                for (var variableNdx = 0; variableNdx < currentVariables.length; variableNdx++)
                {
                    if (currentVariableInput.attr('name') == currentVariables[variableNdx])
                    {
                        variableStillValid =  true;
                        break;
                    }
                }

                if (!variableStillValid)
                {
                    currentVariableInput.parents('tr').remove();
                }
             }   
         }  
    };

    MOQEquationFieldWidget.AddVariableField = function(variableName, defaultSize)
    {
        var workspaceVariable = undefined;
        var ordinaryVariable = undefined;
        var existingVariableField = MOQEquationFieldWidget.GetAllVariableFields().filter('[name="' + variableName + '"]');
        if (!existingVariableField.length)
        {
            var moqEquationVariablesElement = $('#MOQEquationField #MOQEquation-Variables');

            // Workspace Variable Field
            if ((workspaceVariable = MOQEquationFieldWidget.GetWorkspaceVariable(variableName)) != undefined)
            {
                // Summed BOE Workspace Variable
                if (workspaceVariable.WorkspaceVariableValueType == 2) {
                    moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><a wsVar="true" readOnlyVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</a></td></tr>');
                }
                // Discrete Workspace Variable
                else {
                    moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><div wsVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</div></td></tr>');
                }
            }
            // Existing (Saved) Task Ordinary Variable Field
            else if ((ordinaryVariable = MOQEquationFieldWidget.GetOrdinaryVariable(variableName)) != undefined)
            {
                var defaultSizeLabel = '';
                if (ordinaryVariable.DefaultSize != undefined && ordinaryVariable.DefaultSize != '') {
                    defaultSizeLabel = '(' + ordinaryVariable.DefaultSize + ')';
                }
                if (ordinaryVariable.OrdinaryVariableID < 0) {
                    // Actually a new variable that came from a refreshed patial view.
                    MOQEquationFieldWidget.NewOrdinaryVariableID--;
                }
                if (MOQEquationFieldWidget.isReadOnly()) {
                    // READ ONLY Summed BOE Existing Task Ordinary Variable code
                    if (ordinaryVariable.OrdinaryVariableValueType == '<%: (int)VarValueType.SumOfBOEs %>') {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName + ':</td><td><a ordVar="true" readOnlyVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '">' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '</a></td></tr>');
                    }
                    // READ ONLY Discrete Existing Task Ordinary Variable code
                    else {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName + ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/></td></tr>');
                    }
                }
                else {
                    // Summed BOE Existing Task Ordinary Variable code
                    if (ordinaryVariable.OrdinaryVariableValueType == '<%: (int)VarValueType.SumOfBOEs %>') {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName + ':</td><td><input disabled="disabled" ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="' + JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="' + JSON.stringify(ordinaryVariable.ResourceTypes) + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>   <% if(IsSubContractor==false) {%>      <a class="sumVariable" var="' + ordinaryVariable.OrdinaryVariableID + '">Select BOEs to sum</a> <% }%>   <span class="clearVariable" var="' + ordinaryVariable.OrdinaryVariableID + '"> | <a>Clear</a></span></td></tr>');
                    }
                    // Discrete Existing Task Ordinary Variable code
                    else {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.DefaultSize + '"><td class="variableLabel">' + defaultSizeLabel + ordinaryVariable.OrdinaryVariableName + ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="' + JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="' + JSON.stringify(ordinaryVariable.ResourceTypes) + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>  <% if(IsSubContractor==false) { %><a class="sumVariable" var="' + ordinaryVariable.OrdinaryVariableID + '">Select BOEs to sum</a><% } %></td></tr>');
                    }
                }
            }
            // New Task Ordinary Variable Field
            else
            {
                var defaultSizeLabel = '';
                if (defaultSize != undefined && defaultSize != '') {
                    defaultSizeLabel = '(' + defaultSize + ')';
                }
                MOQEquationFieldWidget.NewOrdinaryVariableID--;
                moqEquationVariablesElement.append('<tr var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" data-variable-default-size="' + defaultSize + '"><td class="variableLabel">' + defaultSizeLabel + variableName + ':</td><td><input ordVar="true" pkid="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" name="' + variableName + '" class="variableInput" value="' + defaultSize + '" BOEToSum="[]" WBSToSum="[]" CLINToSum="[]" ResourceTypes="[]" SortBOEBy="<%:(int)VarSortBOEBy.WBS%>"/>  <% if(IsSubContractor==false) { %>  <a class="sumVariable" var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '">Select BOEs to sum</a>  <% } %>    </td></tr>');
            }
        }
    };

    MOQEquationFieldWidget.GetWorkspaceVariable =  function(variableName)
    {
        for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.WorkspaceVariables.length; variableNdx++)
        {
            if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.WorkspaceVariables[variableNdx].WorkspaceVariableName.toUpperCase()))
            {
                return MOQEquationFieldWidget.WorkspaceVariables[variableNdx];
            }
        }

        return undefined;
    };

    MOQEquationFieldWidget.GetOrdinaryVariable =  function(variableName)
    {
        for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.OrdinaryVariables.length; variableNdx++)
        {
            if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.OrdinaryVariables[variableNdx].OrdinaryVariableName.toUpperCase()))
            {
                return MOQEquationFieldWidget.OrdinaryVariables[variableNdx];
            }
        }

        return undefined;
    };

    MOQEquationFieldWidget.CalculateMOQResult = function () {
        if (MOQEquationFieldWidget.ReadyForCalculate())
        {
            GenSession.ShowLoadingBox();
            var moqEquation = MOQEquationFieldWidget.GetInputEquation();
        
            MOQEquationFieldWidget.MOQCalculating = true;

            var dataToSend = { "moqEquation" : moqEquation }
            dataToSend = JSON.stringify(dataToSend);

            $.ajax({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_BOE_LABOR%>',
                    '<%: WebConstants.ACTION_MOQ_CALCULATE%>', 'boe/' + '<%: ViewData["BOEID"] %>'),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: MOQEquationFieldWidget.MOQCalculationSuccess,
                error: MOQEquationFieldWidget.MOQCalculationError
            });
        }
        else
        {
            MOQEquationFieldWidget.SetMOQResult("");

            MOQEquationFieldWidget.SavePending = false;
        }
    };

    MOQEquationFieldWidget.ReadyForCalculate = function() {
        MOQEquationFieldWidget.HideError();
               
        var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (currentVariableInputs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++)
            {
                var currentValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInputs[inputNdx]);

                if (currentValue.length > 0)
                {
                    if (!currentValue.match(/^[\+\-]*[\d,]*\.?[\d,]+%?$/))
                    {
                        MOQEquationFieldWidget.SetMOQError("Invalid MOQ Variables", "All MOQ equation variables must be valid numerical values. Please check each variable field below.");
                        MOQEquationFieldWidget.ShowMOQError();
                        return false;
                    }
                    else
                    {
                        $(currentVariableInputs[inputNdx]).val(Helper.addCommas(Helper.removeCommas(currentValue)));
                    }
                }
                else
                {
                    MOQEquationFieldWidget.SetMOQError("Incomplete MOQ Variables", "All MOQ equation variable fields must contain values. Please fill in all of the variable fields below.");
                    MOQEquationFieldWidget.ShowMOQError();
                    return false;
                }
            }
        }

        return true;
    };
    
    MOQEquationFieldWidget.GetInputEquation = function() {
        var markedEquation = $.trim(MOQEquationFieldWidget.MarkedEquation);
        var ordinaryVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (ordinaryVariableInputs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < ordinaryVariableInputs.length; inputNdx++)
            {
                var currentVariableInput = $(ordinaryVariableInputs[inputNdx]);
                var currentVariableInputName = currentVariableInput.attr('name');
                var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(ordinaryVariableInputs[inputNdx]);
                var result = currentVariableInputName.replace(/\(\d+.*\) /g, "");
                var currentVariableRegEx = new RegExp("<" + result + ">", "gi");
                markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
            }
        }
        
        workspaceVariableDivs = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

        if (workspaceVariableDivs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < workspaceVariableDivs.length; inputNdx++)
            {
                var currentVariableInput = $(workspaceVariableDivs[inputNdx]);
                var currentVariableInputName = currentVariableInput.attr('name');
                var currentVariableInputValue = currentVariableInput.text();
                
                var currentVariableRegEx = new RegExp("<" + currentVariableInputName + ">", 'gi');
                markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
            }
        }

        return markedEquation;
    };

    MOQEquationFieldWidget.GetOrdinaryVariableFieldValue = function(ordinaryVariableField) {
        ordinaryVariableField = $(ordinaryVariableField);

        var fieldValue = '';

        if (ordinaryVariableField.is('[readOnlyVar]')) {
            fieldValue = ordinaryVariableField.text();
        }
        else {
            fieldValue = ordinaryVariableField.val();
        }

        return fieldValue;
    };

    MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage = function(ordinaryVariableField) {
        ordinaryVariableField = $(ordinaryVariableField);

        var fieldValue = '';

        if (ordinaryVariableField.is('[readOnlyVar]')) {
            fieldValue = ordinaryVariableField.text();
        }
        else {
            fieldValue = ordinaryVariableField.val();
        }

        return fieldValue.match(/%/) != null;
    };

    MOQEquationFieldWidget.MOQCalculationSuccess = function (results) {
        GenSession.ShowLoadingBox();

        if (results.Status)
        {
            MOQEquationFieldWidget.SetMOQResult(results.Result);
           
            if (MOQEquationFieldWidget.SavePending)
            {
                MOQEquationFieldWidget.SavePending = false;
                MOQEquationFieldWidget.MOQCalculating = false;
                MOQEquationFieldWidget.CallSaveManually();
            }

            // MOQ equation has been changed, the change has been validated, and the new hours total (<result>) has been determined - OK to apply recalculations
            var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
            scope.moqUpdated(MOQEquationFieldWidget.initialLoad);
            scope.$apply();
        }
        else {
            MOQEquationFieldWidget.MOQCalculationError(results.Message);

            MOQEquationFieldWidget.SavePending = false;
        }

        // if the moqEquation is empty, resource types/spreads will be calculated as if the equation  = 0, but we don't want to show the result as 0 to the user 
        var moqEquation = MOQEquationFieldWidget.GetInputEquation();
        if (moqEquation.length == 0)
            {
              MOQEquationFieldWidget.SetMOQResult("");
            }
        MOQEquationFieldWidget.MOQCalculating = false;
        MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    }; 
    
    MOQEquationFieldWidget.MOQCalculationError = function (exceptionstring) {
        MOQEquationFieldWidget.SetMOQError("MOQ Calculation Error", exceptionstring);
        MOQEquationFieldWidget.ShowMOQError();
        
        MOQEquationFieldWidget.SavePending = false;
        MOQEquationFieldWidget.MOQCalculating = false;
        MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    };

    MOQEquationFieldWidget.SetMOQResult = function (result) {
        $("#MOQEquationField #equals").text(Helper.addCommas(result));
        $(document).trigger('MOQEquationReady');
    };
    
    MOQEquationFieldWidget.CallSaveManually = function() {
        $(document).trigger('SaveBOEUpdatesAndClose');
    }; 

    MOQEquationFieldWidget.FilterFieldToIntegers =  function(field) {
        field.value = field.value.replace(/[^+\-\,\.0-9]/g, '');
    };
    
    MOQEquationFieldWidget.OpenSumOfBOEsByWBS = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS %>',
                'boe/<%: (int)ViewData["BOEID"] %>'),
            success: function (response) {
                HideLoadingBox();
                $('#VariableSumOfBOEsByWBSDialogContainer').html(response);
                $(document).trigger(event, eventData);
               
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };

    MOQEquationFieldWidget.OpenSumOfBOEsByCLIN = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN %>',
                'boe/<%: (int)ViewData["BOEID"] %>'),
            success: function (response) {
                HideLoadingBox();
                $('#VariableSumOfBOEsByCLINDialogContainer').html(response);
                $(document).trigger(event, eventData);
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };

    MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent = function (event, eventData) {
        var VariableBOESumByWBSDialog = $('#VariableBOESumByWBS');

        if (VariableBOESumByWBSDialog == undefined || VariableBOESumByWBSDialog.length == 0) {
            // if the WBS dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByWBS function)
            MOQEquationFieldWidget.OpenSumOfBOEsByWBS(event, eventData);
        }
    };

    MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent = function (event, eventData) {
        var VariableBOESumByCLINDialog = $('#VariableBOESumByCLIN');

        if (VariableBOESumByCLINDialog == undefined || VariableBOESumByCLINDialog.length == 0)
        {
            // if the CLIN dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByCLIN function)
            MOQEquationFieldWidget.OpenSumOfBOEsByCLIN(event, eventData);
        }
    };
    
    //bind any events that the objects need to observe to and member functions.
    $(function () {
        $("#MOQEquationField #MOQEquation").change(function() {
            $(document).trigger('ValidateMOQEquation');
        });

        $("#MOQEquationField #MOQEquation").change(MOQEquationFieldWidget.setDirty);

        // use change instead of focusout to catch all changes having to deal with the moq equation or variables. focusout was not catching changes on boe to sum variable in IE
        MOQEquationFieldWidget.registerForLiveEvent('change', "#MOQEquationField #MOQEquation-Variables input.variableInput", function() {
            MOQEquationFieldWidget.CalculateMOQResult();
        });
        
        MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a[readOnlyVar]', function() {
            var eventData = {};
            var variable = undefined;

            if ($(this).is('[wsVar]')) {
                variable = MOQEquationFieldWidget.GetWorkspaceVariable($(this).attr('name'));

                if (variable == undefined) { return; }

                eventData.dialogTitle = 'Summed BOEs for ' + variable.WorkspaceVariableName;
            }
            else if ($(this).is('[ordVar]')) {
                variable = MOQEquationFieldWidget.GetOrdinaryVariable($(this).attr('name'));

                if (variable == undefined) { return; }

                eventData.dialogTitle = 'Summed BOEs for ' + variable.OrdinaryVariableName;
            }
            
            eventData.BOEToSum = variable.BOEToSum;
            eventData.WBSToSum = variable.WBSToSum;
            eventData.CLINToSum = variable.CLINToSum;
            eventData.ResourceTypes = variable.ResourceTypes;
            eventData.SortBOEBy = variable.SortBOEBy;
            eventData.ReadOnly = true;

            if (eventData.SortBOEBy == '<%:(int)VarSortBOEBy.CLIN%>') {
                $(document).trigger('VariableBOESumByCLIN_OpenReadOnly', eventData);
            }
            else {
                $(document).trigger('VariableBOESumByWBS_OpenReadOnly', eventData);
            }
        });
        
        MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a.sumVariable', function() {
            var pkid = $(this).attr('var');

            var selectedInput = $(this).siblings('input[ordVar][pkid=' + pkid + ']');
            
            var eventData = {};

            eventData.pkid = pkid;
            eventData.BOEToSum = JSON.parse(selectedInput.attr('BOEToSum'));
            eventData.WBSToSum = JSON.parse(selectedInput.attr('WBSToSum'));
            eventData.CLINToSum = JSON.parse(selectedInput.attr('CLINToSum'));
            eventData.ResourceTypes = JSON.parse(selectedInput.attr('ResourceTypes'));
            eventData.SortBOEBy = selectedInput.attr('SortBOEBy');
            eventData.ReadOnly = false;
            
            if (eventData.BOEToSum.length || eventData.WBSToSum.length || eventData.CLINToSum.length) {
                if (eventData.SortBOEBy == '<%:(int)VarSortBOEBy.CLIN%>') {
                    $(document).trigger('VariableBOESumByCLIN_Open', eventData);
                } else {
                    $(document).trigger('VariableBOESumByWBS_Open', eventData);
                }
            }
            else {
                if (eventData.SortBOEBy == '<%:(int)VarSortBOEBy.CLIN%>') {
                    $(document).trigger('VariableBOESumByCLIN_OpenNew', eventData);            
                } else {
                    $(document).trigger('VariableBOESumByWBS_OpenNew', eventData);            
                }
            }
        });
        
        MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables span.clearVariable a', function() {
            var pkid = $(this).parent().attr('var');

            $(this).parents('#MOQEquationField #MOQEquation-Variables')
                .find('input[pkid=' + pkid + ']')
                .attr('BOEToSum', '[]')
                .attr('WBSToSum', '[]')
                .attr('CLINToSum', '[]')
                .attr('ResourceTypes', '[]')
                .attr('SortBOEBy', '<%:(int)VarSortBOEBy.WBS%>')
                .prop('disabled', false)
                .val('')
                .change()
                .siblings('span.clearVariable[var=' + pkid + ']')
                .remove();
        });
        
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Create', function (event, eventData) {
            MOQEquationFieldWidget.setDirty();

            var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

            variableInput
                .val(Helper.addCommas(eventData.Sum))
                .attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
                .attr('WBSToSum', '[]')
                .attr('CLINToSum', JSON.stringify(eventData.CLINToSum))
                .attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
                .attr('SortBOEBy', eventData.SortBOEBy)
                .prop('disabled', true)
                .change();

            if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length)
            {
                variableInput
                    .parent()
                    .append(' <span class="clearVariable" var="' + eventData.pkid + '">|<a>Clear</a></span>');
            }

            $(document).trigger('ValidateMOQEquation');
        });
        
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Create', function (event, eventData) {
            MOQEquationFieldWidget.setDirty();

            var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');
            
            variableInput
                .val(Helper.addCommas(eventData.Sum))
                .attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
                .attr('WBSToSum', JSON.stringify(eventData.WBSToSum))
                .attr('CLINToSum', '[]')
                .attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
                .attr('SortBOEBy', eventData.SortBOEBy)
                .prop('disabled', true)
                .change();

            if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length)
            {
                variableInput
                    .parent()
                    .append(' <span class="clearVariable" var="' + eventData.pkid + '">| <a>Clear</a></span>');
            }

            $(document).trigger('ValidateMOQEquation');
        });
        
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
        MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);

        MOQEquationFieldWidget.registerForEvent('ValidateMOQEquation', function(event, options){
            if(options != undefined && options.initialLoad != undefined && options.initialLoad != null) {
                MOQEquationFieldWidget.initialLoad = options.initialLoad;
            } else {
                MOQEquationFieldWidget.initialLoad = false;
            }

            MOQEquationFieldWidget.ValidateMOQEquation();
        });

        MOQEquationFieldWidget.registerForEvent('InsertMOQElementDialogClosing', MOQEquationFieldWidget.InsertMOQElementDialogClosing);
        
        MOQEquationFieldWidget.RefreshStyles();

        TaskElementDetailsWidget.ChildWidgets.push(MOQEquationFieldWidget);

        if (MOQEquationFieldWidget_ReadOnly) {
            // Disable dropdown.
            $('.magnifier-button').addClass('display-none');
        }
            
         // Register events for menu show/hide menu
        TaskElementDetailsWidget.registerForDelegateEvent('click', '.menu-icon', function (event) {
            if($('#menu-options-box').hasClass("display-none")){
                $('#menu-options-box').removeClass('display-none');
            }
            else{
                $('#menu-options-box').addClass('display-none');
            }
        });

        // Hide menu on option selection
        TaskElementDetailsWidget.registerForDelegateEvent('click', '#menu-options-box', function (event) {
            $('#menu-options-box').addClass('display-none');
            event.stopPropagation();
        });

        TaskElementDetailsWidget.CheckToShowMetrics();
        TaskElementDetailsWidget.MOQText = CreateRteTemplate('<%:showMoqQuestions%>'.isTrue(), <%:numberMoqQuestions%>);

        if(!MOQEquationFieldWidget.isReadOnly() || '<%: ViewData["ShouldMoqReadOnlyBeReversed"] %>' == 'True' || !TaskElementDetailsWidget.isReadOnly())
        {
            InitializeRteTemplate(TaskElementDetailsWidget.MOQText, 'MOQText', <%:rteFieldSize%>);
        }
        else
        {
            HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText');
        }

        $(document).trigger('MOQWidgetLoaded', "MOQEquationField");
        $(document).trigger('WidgetLoaded', "MOQEquationField");
    });
</script>

<div id="MOQEquationField" class="bootstrap" data-ng-controller="MoqEquationController" data-ng-init="init()">
    <div class="form-row">
        <div class="form-label">{{model.MoqEquationLabel}} **</div>
        <div class="form-element">
            <div uib-dropdown class="btn-group">
                <div id="dropdown-magnifier" uib-dropdown-toggle class="magnifier-button"></div>
                <ul class="uib-dropdown-menu dropdown-menu" role="menu" aria-labelledby="dropdown-magnifier">
                    <li><a id="InsertWorkspaceVariableLink" data-ng-click="InsertWorkspaceVariableClicked()">Insert Workspace Variable</a></li>
                    <li data-ng-if="model.ShowSearchMetricsLink"><a id="SearchEstimatingCatalogLink" data-ng-click="SearchEstimatingCatalogClicked()">Search Estimating Catalog</a></li>
                    <li><a id="CopyMoqFromBoeLink" data-ng-click="CopyMoqFromBoeClicked()">Copy MOQ from BOE</a></li>
                </ul>
            </div>
            <input type="text" id="MOQEquation" name="MOQEquation" value="<%: Model.MOQEquation %>" class="moq-equation-input" maxlength="250" />
            <div>
                <input id="MOQDefaultSizes" value="" type="hidden" />
                <input id="HistoricalMetricEquation" value="" type="hidden" />
            </div>
        </div>
        <span class="moq-equation-equals">=</span>
        <span data-ng-if="model.IsCostEquation">$</span>
        <div id="equals" class="form-element moq-equation-result"></div>
        <div class="clear"></div>
        <div class="form-label"></div>
        <div class="form-element LockedWorkspaceState" style="display: inline-block; margin-top: 2px;"></div>
    </div>
    <div id="MOQEquation-Error" class="form-row" style="display: none;">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="validation-box" style="width: 520px; display: block">
                <div>
                    <div id="MOQEquation-ErrorTitle"><b>Invalid MOQ Equation.</b></div>
                    <div id="MOQEquation-ErrorText"></div>
                </div>
                <div class="clear"></div>
            </div>
        </div>
    </div>
    <table id="MOQEquation-Variables" class="moqVariables"></table>
    <div class="moqVariableNote clear display-none"><b>Note:</b> The value of Variables that are the sum of select BOEs will change if the <i>Totals</i> of the select BOEs change.</div>



    <div data-ng-if="model.UsingTemplateBOE" class="form-row">
        <div class="form-label">
            <span>MOQ Type(s)</span>
        </div>
    </div>

    
    <div data-ng-if="model.UsingTemplateBOE" data-ng-repeat="item in model.SelectedMoqTypes" class="form-row">
        MOQ ITEM FOR.. Id: {{item.id}}, Type: {{item.text}}
        <input type="button" data-ng-click="RemoveMoqType(item)" value="DELETE" />
    </div>


    <div data-ng-if="model.UsingTemplateBOE" class="form-row">
        <div class="form-label">
            <span>Add New MOQ Type *</span>
        </div>
        <div class="form-element">
            <select data-ng-model="model.MOQType" data-ng-options="moqType.text for moqType in model.MOQTypes" class="moqTypes">
            </select>
            <input type="button" data-ng-click="AddMoqType()" value="ADD" />
        </div>
    </div>



    <div data-ng-if="!model.UsingTemplateBOE" class="form-row">
        <div class="form-label">
            <span>MOQ Type **</span>
            <div class="help-icon" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text"><%: Html.Raw(Model.HelpText) %></div>
            </div>
        </div>
        <div class="form-element" id="MoqType">
            <select id="MOQType" name="MOQType" class="moqTypes">
                <%=ViewData["MOQTypes"]%>
            </select>
        </div>
    </div>
    <div data-ng-if="!model.UsingTemplateBOE" class="form-row">
        <div class="form-label"><%: Model.MOQTextLabel %> **</div>
        <div class="form-element moq-text-area"><% Html.RenderPartial(WebConstants.VIEW_RTE_TEMPLATE, new GenBOE.Web.ModelView.RteTemplateModelView(Model.MoqTemplateAnswers, "MOQText", Model.MOQText));  %></div>
    </div>
    <div id="UsedHistoricalMetrics" class="form-row display-none">
        <div class="form-label">
            <%if (Model.Company == CompanyConfiguration.MST) 
              {%>
            Historical Measures<br />Used
            <div class="help-icon" style="margin-top:1px;" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text">The historical measures used to estimate the labor for this task.  If the historical measure is no longer used, it should be deleted.</div>
            </div>
            <%}
              else
              { %>
            <span>Historical Metrics
                <br />
                Used</span>
            <div class="help-icon" style="margin-top:1px;" onclick="MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text">The historical metrics used to estimate the labor for this task.  If the historical metric is no longer used, it should be deleted.</div>
            </div>
            <%} %>
        </div>
        <div id="addMetricToList" class="form-element">
            <button id="HMUDeleteButton" class="ies" name="delete-button" type="button" onclick="TaskElementDetailsWidget.deleteAllSelected()">Delete</button>
            <table id="HistoricalMetricsUsedGrid" class="manage-historical-metrics-used-grid grid readonly">
                <thead>
                    <% if (Model.Company == CompanyConfiguration.MST)
                        { %>
                    <tr>
                        <th class="delete-checkbox"><input type="checkbox" id="Checkbox1" onclick="TaskElementDetailsWidget.deleteAllToggle(this)" /></th>    
                        <th class="metric-title"><b>Measure Name</b></th>
                        <th class="status"><b>Program Name</b></th>
                        <th class="moq"><b>Date Applied to BOE</b></th>
                    </tr>
                    <% } %>
                    <% else
                        { %>
                    <tr>
                        <th class="delete-checkbox"><input type="checkbox" id="Checkbox1" onclick="TaskElementDetailsWidget.deleteAllToggle(this)" /></th>    
                        <th class="metric-title"><b>Metric Title</b></th>
                        <th class="status"><b>Status</b></th>
                        <th class="moq"><b>MOQ Equation</b></th>
                        <th class="moq-type"><b>MOQ Type</b></th>
                    </tr>
                    <% } %>
                </thead>
                <tbody id="HistoricalMetricsBody">
                    <%if (Model.Company == CompanyConfiguration.MST)
                        { %>
                        <% foreach (MSTMetricDetailsDTO historicalMetric in Model.PMMetricsUsed)
                           { %>
                            <tr name="historicalMetric" id="HMURow" historicalMetricID="<%:historicalMetric.Id%>"><td class="delete-checkbox" id="DeleteCheckboxId"><input type="checkbox" name="DeleteResource" id="DeleteThisResource" onclick="TaskElementDetailsWidget.deleteToggled(this)"/>
                                <td class="metric-id display-none"> <a name="MetricID" class="edit-resource-link"></a> </td>               
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.MeasureName%>"><%:historicalMetric.MeasureName%></a> </td>
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.ProgramName %>"><%:historicalMetric.ProgramName %></a> </td>
                                <td> <a class="edit-resource-link" onclick="TaskElementDetailsWidget.DisplayMetricSelected(null, {metricId :<%:historicalMetric.Id%>, getFromSource : false}, true)" style="white-space:normal; width:100px;" title="<%:historicalMetric.DateAddedToTaskElement != null ? historicalMetric.DateAddedToTaskElement.Value.ToString("MM/dd/yyyy") : "N/A"%>"><%:historicalMetric.DateAddedToTaskElement != null ? historicalMetric.DateAddedToTaskElement.Value.ToString("MM/dd/yyyy") : "N/A"%></a> </td>
                                <br id="HistoricalEndRow"/>
                            </tr>
                        <%}%>
                    <% }%>
                </tbody>
            </table>
        </div>
    </div>
</div>
<div id="VariableSumOfBOEsByWBSDialogContainer"></div>
<div id="VariableSumOfBOEsByCLINDialogContainer"></div>