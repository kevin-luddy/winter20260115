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
%>

<script type="text/javascript">
    var MOQEquationFieldModel = {
        BaseUrl: '<%=this.ResolveClientUrl("~/")%>',
        IsReadOnly: <%=ViewData["READONLY"]%>,
        MoqEquationName: '<%=Model.MoqEquationName%>',
        MoqEquationLabel: '<%=Model.MOQEquationLabel%>',
        MoqEquationType: '<%=Model.TypeOfMoqEquation%>',
        WorkspaceVariables: <%=serializer.Serialize(workspaceVariables)%>,
        ShowSearchMetricsLink: <%=Model.ShowSearchMetricsLink ? "true" : "false"%>,
        TaskElementId: <%=Model.TaskElementId > 0 ? Model.TaskElementId : -1%>
        };
</script>

<script type="text/javascript">

    // Get the read-only attribute passed in from the controller
    var <%: Model.MoqEquationName %>MOQEquationFieldWidget_ReadOnly = '<%= ViewData["READONLY"] %>'.isTrue();
        
    //create base js object;
    var <%: Model.MoqEquationName %>MOQEquationFieldWidget = new Widget("<%: Model.MoqEquationName %>MOQEquationField", <%: Model.MoqEquationName %>MOQEquationFieldWidget_ReadOnly);
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.initialLoad = false;
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MarkedEquation = "";
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables = <%= serializer.Serialize(workspaceVariables) %>;
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables = <%= serializer.Serialize(Model.TaskOrdinaryVariables) %>;
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating = false;
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating = false;
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValid = false;
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID = -1;
    <% if (Model.TypeOfMoqEquation == MOQEquationType.Cost) { %>
    // This will avoid collisions with IDs when placed on the same page as an Hours MOQ Equation.  250 was chosen because that's the char limit in the text box
    // so you are guaranteed there will never be 250 variables.
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID = -250;
    <% } %>
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.HarvestWorkspaceVariablesForSave = function() {
    
        var toReturn = [];

        var workspaceVariableFields = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

        for (var ndx = 0; ndx < <%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables.length; ndx++)
        {
            var variableID = <%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables[ndx].WorkspaceVariableID;
            var variableField = workspaceVariableFields.filter('[pkid=' + variableID + ']');
            
            if (variableField.length)
            {
                toReturn.push(variableID);
            }
        }

        return toReturn;
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.HarvestOrdinaryVariablesForSave = function() {
        var toReturn = [];
        var currentVariableInputs = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        for (var ndx = 0; ndx < <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables.length; ndx++)
        {
            var currentTaskOrdinaryVariable = <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables[ndx];
            var variableID = currentTaskOrdinaryVariable.OrdinaryVariableID;
            var variableField = currentVariableInputs.filter('[pkid=' + variableID + ']');
            var variableValue;
            
            if (variableField.length && (variableValue = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField)).length > 0)
            {
                currentTaskOrdinaryVariable.Deleted = false;
                currentTaskOrdinaryVariable.OrdinaryVariableValue = variableValue;
                currentTaskOrdinaryVariable.IsPercentage = <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(variableField);

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
                currentTaskOrdinaryVariable.OrdinaryVariableValue = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField);
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
                var currentVariableInputValue = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInput);

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
                        newOrdinaryVariable.IsPercentage = <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(currentVariableInput);
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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen = function () {
        var scope = angular.element(document.getElementById('MOQHoursMOQEquationField')).scope();
        return scope.model.insertWorkspaceModalOpen;
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.InsertMOQElementDialogClosing = function () {
        if (!<%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValid && !<%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating)
        {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQValidationError();
        }
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields = function() {
        return $('#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables [wsVar], #<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables [ordVar]');
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.RefreshStyles = function() {
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.applyReadOnly();

        if (<%: Model.MoqEquationName %>MOQEquationFieldWidget.isReadOnly())
        {
            $("#<%: Model.MoqEquationName %>MOQEquationField #InsertWorkspaceVariableLink").parent().prev().remove();
            $("#<%: Model.MoqEquationName %>MOQEquationField #InsertWorkspaceVariableLink").parent().remove();
            $("#<%: Model.MoqEquationName %>MOQEquationField #SearchEstimatingCatalogLink").parent().prev().remove();
            $("#<%: Model.MoqEquationName %>MOQEquationField #SearchEstimatingCatalogLink").parent().remove();

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
                var moqSection = $('#<%:Model.MoqEquationName%>MOQEquationField');

                var moqTypeElement = $("#MOQType", moqSection);
                $('.replacedWidgetText', moqTypeElement.parent()).addClass('display-none');
                moqTypeElement.removeClass('display-none');

                var moqTextElement = $("#MOQText", moqSection);
                $('.replacedWidgetText', moqTextElement.parent()).addClass('display-none');
                moqTextElement.removeClass('display-none');
            }
        }

        refreshModule($('.task-element-details.module'));
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.ValidateMOQEquation = function () {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;

        GenSession.ShowLoadingBox();
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating = true;

        <%: Model.MoqEquationName %>MOQEquationFieldWidget.HideError();

        var moqEquation = $.trim($("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation").val());

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
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidated(response);

                if(<%: Model.MoqEquationName %>MOQEquationFieldWidget.isReadOnly() && '<%: ViewData["ShouldMoqReadOnlyBeReversed"] %>' == 'True')
                {
                    $('.magnifier-button').addClass('display-none');
                    $('#MoqType').children('.replacedWidgetText').remove();
                    $('.moqTypes').removeClass('display-none');

                    $('#MOQText').siblings('.replacedWidgetText').remove();
                }
            },
            error: function(response, textStatus) {
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidationError(response);
            }
        });
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidationError = function (exceptionstring) {
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", "A general validation error occurred. Please check the MOQ equation for validity.");
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQValidationError();
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating = false;
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
        GenSession.HideLoadingBox();
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError = function (errorSubject, errorMessage) {
        if (errorSubject != undefined && errorMessage != undefined)
        {
            $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-ErrorTitle").html(errorSubject);
            $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-ErrorText").html(errorMessage);
        }
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQValidationError = function () {
        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables").hide();
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQError();
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQError = function() {
        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Error").show();
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQResult("");
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.RefreshStyles();
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.HideError = function () {
        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Error").hide();
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.RefreshStyles();
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.FocusError = function () {
        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-ErrorTitle").focus();
    }

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQEquationPreparedForSubmit = function() {
    
        if (<%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating || <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating)
        {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = true;
        }

        return (!$("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Error").is(":visible") &&
                !<%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating &&
                !<%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating &&
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.ReadyForCalculate());
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidated = function (results) {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;
        GenSession.ShowLoadingBox();
        // Set Widget valid flag
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValid = results.Status;

        if (results.Status)
        {
            var variables = results.Variables;

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.RemoveInvalidVariables(variables);

            for (var ndx = 0; ndx < variables.length; ndx++)
            {
                if (ndx == 0)
                {
                    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MarkedEquation = variables[ndx];
                }
                else
                {
                    var defaultSize = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetVariableDefaultSize(variables[ndx]);
                    <%: Model.MoqEquationName %>MOQEquationFieldWidget.AddVariableField(variables[ndx], defaultSize);
                }
            }
            // Reinitialize imported historical metric data.
            $('#MOQDefaultSizes').val("");
            $('#HistoricalMetricEquation').val("");
    
            var currentVariables = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields();

            if (currentVariables.length > 0)
            {
                $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables").show();
                $("#<%: Model.MoqEquationName %>MOQEquationField .moqVariableNote").removeClass('display-none');
            }
            else
            {
                $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables").hide();
            }
             
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.RefreshStyles();

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.CalculateMOQResult();
        }
        else
        {
            // Set the validation error text
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", results.Message);

            // If the user isn't inserting a variable, we'll show the error
            if (!<%: Model.MoqEquationName %>MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen() &&
                (!TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogMST').isOpen() ||
                !TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogCommon').isOpen()))
            {
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQValidationError();
            }

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;
        }

        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQValidating = false;
        GenSession.HideLoadingBox();
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
    };

    /**
    * Gets a default size value for a variable that was created as a result of importing a historical
    * metric.  Returns "" for all other variable types.
    * @param {String} currentVariable Variable name
    * @returns {String}
    */
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetVariableDefaultSize = function(currentVariable) {
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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.RemoveInvalidVariables = function(currentVariables)
    {
         var currentVariableInputs = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields();

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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.AddVariableField = function(variableName, defaultSize)
    {
        var workspaceVariable = undefined;
        var ordinaryVariable = undefined;
        var existingVariableField = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[name="' + variableName + '"]');
        if (!existingVariableField.length)
        {
            var moqEquationVariablesElement = $('#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables');

            // Workspace Variable Field
            if ((workspaceVariable = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetWorkspaceVariable(variableName)) != undefined)
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
            else if ((ordinaryVariable = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariable(variableName)) != undefined)
            {
                var defaultSizeLabel = '';
                if (ordinaryVariable.DefaultSize != undefined && ordinaryVariable.DefaultSize != '') {
                    defaultSizeLabel = '(' + ordinaryVariable.DefaultSize + ')';
                }
                if (ordinaryVariable.OrdinaryVariableID < 0) {
                    // Actually a new variable that came from a refreshed patial view.
                    <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID--;
                }
                if (<%: Model.MoqEquationName %>MOQEquationFieldWidget.isReadOnly()) {
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
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID--;
                moqEquationVariablesElement.append('<tr var="' + <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID + '" data-variable-default-size="' + defaultSize + '"><td class="variableLabel">' + defaultSizeLabel + variableName + ':</td><td><input ordVar="true" pkid="' + <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID + '" name="' + variableName + '" class="variableInput" value="' + defaultSize + '" BOEToSum="[]" WBSToSum="[]" CLINToSum="[]" ResourceTypes="[]" SortBOEBy="<%:(int)VarSortBOEBy.WBS%>"/>  <% if(IsSubContractor==false) { %>  <a class="sumVariable" var="' + <%: Model.MoqEquationName %>MOQEquationFieldWidget.NewOrdinaryVariableID + '">Select BOEs to sum</a>  <% } %>    </td></tr>');
            }
        }
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetWorkspaceVariable =  function(variableName)
    {
        for (var variableNdx = 0; variableNdx < <%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables.length; variableNdx++)
        {
            if ($.trim(variableName.toUpperCase()) == $.trim(<%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables[variableNdx].WorkspaceVariableName.toUpperCase()))
            {
                return <%: Model.MoqEquationName %>MOQEquationFieldWidget.WorkspaceVariables[variableNdx];
            }
        }

        return undefined;
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariable =  function(variableName)
    {
        for (var variableNdx = 0; variableNdx < <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables.length; variableNdx++)
        {
            if ($.trim(variableName.toUpperCase()) == $.trim(<%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables[variableNdx].OrdinaryVariableName.toUpperCase()))
            {
                return <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariables[variableNdx];
            }
        }

        return undefined;
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.CalculateMOQResult = function () {
        if (<%: Model.MoqEquationName %>MOQEquationFieldWidget.ReadyForCalculate())
        {
            GenSession.ShowLoadingBox();
            var moqEquation = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetInputEquation();
        
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating = true;

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
                success: <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculationSuccess,
                error: <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculationError
            });
        }
        else
        {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQResult("");

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;
        }
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.ReadyForCalculate = function() {
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.HideError();
               
        var currentVariableInputs = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (currentVariableInputs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++)
            {
                var currentValue = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInputs[inputNdx]);

                if (currentValue.length > 0)
                {
                    if (!currentValue.match(/^[\+\-]*[\d,]*\.?[\d,]+%?$/))
                    {
                        <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError("Invalid MOQ Variables", "All MOQ equation variables must be valid numerical values. Please check each variable field below.");
                        <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQError();
                        return false;
                    }
                    else
                    {
                        $(currentVariableInputs[inputNdx]).val(Helper.addCommas(Helper.removeCommas(currentValue)));
                    }
                }
                else
                {
                    <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError("Incomplete MOQ Variables", "All MOQ equation variable fields must contain values. Please fill in all of the variable fields below.");
                    <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQError();
                    return false;
                }
            }
        }

        return true;
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetInputEquation = function() {
        var markedEquation = $.trim(<%: Model.MoqEquationName %>MOQEquationFieldWidget.MarkedEquation);
        var ordinaryVariableInputs = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (ordinaryVariableInputs.length > 0)
        {
            for (var inputNdx = 0; inputNdx < ordinaryVariableInputs.length; inputNdx++)
            {
                var currentVariableInput = $(ordinaryVariableInputs[inputNdx]);
                var currentVariableInputName = currentVariableInput.attr('name');
                var currentVariableInputValue = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(ordinaryVariableInputs[inputNdx]);
                var result = currentVariableInputName.replace(/\(\d+.*\) /g, "");
                var currentVariableRegEx = new RegExp("<" + result + ">", "gi");
                markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
            }
        }
        
        workspaceVariableDivs = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariableFieldValue = function(ordinaryVariableField) {
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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage = function(ordinaryVariableField) {
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

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculationSuccess = function (results) {
        GenSession.ShowLoadingBox();

        if (results.Status)
        {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQResult(results.Result);
           
            if (<%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending)
            {
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating = false;
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.CallSaveManually();
            }

            // MOQ equation has been changed, the change has been validated, and the new hours total (<result>) has been determined - OK to apply recalculations
            var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
            scope.moqUpdated(<%:Model.MoqEquationName%>MOQEquationFieldWidget.initialLoad);
            scope.$apply();
        }
        else {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculationError(results.Message);

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;
        }

        // if the moqEquation is empty, resource types/spreads will be calculated as if the equation  = 0, but we don't want to show the result as 0 to the user 
        var moqEquation = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetInputEquation();
        if (moqEquation.length == 0)
            {
              <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQResult("");
            }
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating = false;
        <%:Model.MoqEquationName%>MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    }; 
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculationError = function (exceptionstring) {
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQError("MOQ Calculation Error", exceptionstring);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.ShowMOQError();
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.SavePending = false;
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.MOQCalculating = false;
        <%:Model.MoqEquationName%>MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.SetMOQResult = function (result) {
        $("#<%: Model.MoqEquationName %>MOQEquationField #equals").text(Helper.addCommas(result));
        $(document).trigger('MOQEquationReady');
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.CallSaveManually = function() {
        $(document).trigger('SaveBOEUpdatesAndClose');
    }; 

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.FilterFieldToIntegers =  function(field) {
        field.value = field.value.replace(/[^+\-\,\.0-9]/g, '');
    };
    
    <%: Model.MoqEquationName %>MOQEquationFieldWidget.OpenSumOfBOEsByWBS = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_WBS %>',
                'boe/<%: (int)ViewData["BOEID"] %>'),
            success: function (response) {
                HideLoadingBox();
                $('#<%: Model.MoqEquationName %>VariableSumOfBOEsByWBSDialogContainer').html(response);
                $(document).trigger(event, eventData);
               
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.OpenSumOfBOEsByCLIN = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%:WebConstants.CONTROLLER_BOE_LABOR %>',
                '<%:WebConstants.ACTION_DISPLAY_VARIABLE_BOE_SUM_BY_CLIN %>',
                'boe/<%: (int)ViewData["BOEID"] %>'),
            success: function (response) {
                HideLoadingBox();
                $('#<%: Model.MoqEquationName %>VariableSumOfBOEsByCLINDialogContainer').html(response);
                $(document).trigger(event, eventData);
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent = function (event, eventData) {
        var VariableBOESumByWBSDialog = $('#VariableBOESumByWBS');

        if (VariableBOESumByWBSDialog == undefined || VariableBOESumByWBSDialog.length == 0)
        {
            if (eventData.MoqEquationName == '<%: Model.MoqEquationName %>') {
                // if the WBS dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByWBS function)
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.OpenSumOfBOEsByWBS(event, eventData);
            }
        }
    };

    <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent = function (event, eventData) {
        var VariableBOESumByCLINDialog = $('#VariableBOESumByCLIN');

        if (VariableBOESumByCLINDialog == undefined || VariableBOESumByCLINDialog.length == 0)
        {
            // if the CLIN dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByCLIN function)
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.OpenSumOfBOEsByCLIN(event, eventData);
        }
    };
    
    //bind any events that the objects need to observe to and member functions.
    $(function () {
        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation").change(function() {
            $(document).trigger('ValidateMOQEquation');
        });

        $("#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation").change(<%: Model.MoqEquationName %>MOQEquationFieldWidget.setDirty);

        // use change instead of focusout to catch all changes having to deal with the moq equation or variables. focusout was not catching changes on boe to sum variable in IE
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForLiveEvent('change', "#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables input.variableInput", function() {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.CalculateMOQResult();
        });
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForLiveEvent('click', '#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables a[readOnlyVar]', function() {
            var eventData = {};
            var variable = undefined;

            if ($(this).is('[wsVar]')) {
                variable = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetWorkspaceVariable($(this).attr('name'));

                if (variable == undefined) { return; }

                eventData.dialogTitle = 'Summed BOEs for ' + variable.WorkspaceVariableName;
            }
            else if ($(this).is('[ordVar]')) {
                variable = <%: Model.MoqEquationName %>MOQEquationFieldWidget.GetOrdinaryVariable($(this).attr('name'));

                if (variable == undefined) { return; }

                eventData.dialogTitle = 'Summed BOEs for ' + variable.OrdinaryVariableName;
            }
            
            eventData.MoqEquationName = '<%: Model.MoqEquationName %>';
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
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForLiveEvent('click', '#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables a.sumVariable', function() {
            var pkid = $(this).attr('var');

            var selectedInput = $(this).siblings('input[ordVar][pkid=' + pkid + ']');
            
            var eventData = {};

            eventData.pkid = pkid;
            eventData.BOEToSum = JSON.parse(selectedInput.attr('BOEToSum'));
            eventData.WBSToSum = JSON.parse(selectedInput.attr('WBSToSum'));
            eventData.CLINToSum = JSON.parse(selectedInput.attr('CLINToSum'));
            eventData.ResourceTypes = JSON.parse(selectedInput.attr('ResourceTypes'));
            eventData.SortBOEBy = selectedInput.attr('SortBOEBy');
            eventData.MoqEquationName = '<%: Model.MoqEquationName %>';
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
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForLiveEvent('click', '#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables span.clearVariable a', function() {
            var pkid = $(this).parent().attr('var');

            $(this).parents('#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables')
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
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Create', function (event, eventData) {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.setDirty();

            var variableInput = $('#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

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
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Create', function (event, eventData) {
            <%: Model.MoqEquationName %>MOQEquationFieldWidget.setDirty();

            var variableInput = $('#<%: Model.MoqEquationName %>MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');
            
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
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Open', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenNew', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenReadOnly', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Open', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenNew', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenReadOnly', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);

        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('ValidateMOQEquation', function(event, options){
            if(options != undefined && options.initialLoad != undefined && options.initialLoad != null) {
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.initialLoad = options.initialLoad;
            } else {
                <%: Model.MoqEquationName %>MOQEquationFieldWidget.initialLoad = false;
            }

            <%: Model.MoqEquationName %>MOQEquationFieldWidget.ValidateMOQEquation();
        });

        <%: Model.MoqEquationName %>MOQEquationFieldWidget.registerForEvent('InsertMOQElementDialogClosing', <%: Model.MoqEquationName %>MOQEquationFieldWidget.InsertMOQElementDialogClosing);
        
        <%: Model.MoqEquationName %>MOQEquationFieldWidget.RefreshStyles();

        TaskElementDetailsWidget.ChildWidgets.push(<%: Model.MoqEquationName %>MOQEquationFieldWidget);

        if (<%: Model.MoqEquationName %>MOQEquationFieldWidget_ReadOnly) {
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

        if(!<%: Model.MoqEquationName %>MOQEquationFieldWidget.isReadOnly() || '<%: ViewData["ShouldMoqReadOnlyBeReversed"] %>' == 'True' || !TaskElementDetailsWidget.isReadOnly())
        {
            InitializeRTE('MOQText', { maxlen: <%:rteFieldSize%>, enableCharCounting: true }, TaskElementDetailsWidget);
        }
        else
        {
            HandleRTEDataForReadOnly("#MOQText", ".replacedWidgetText");
        }

        $(document).trigger('MOQWidgetLoaded', "MOQEquationField");
        $(document).trigger('WidgetLoaded', "MOQEquationField");
    });

</script>

<div id="<%: Model.MoqEquationName %>MOQEquationField" class="bootstrap" data-ng-controller="MoqEquationController" data-ng-init="init()">
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
            <input type="text" id="MOQEquation" name="<%: Model.MoqEquationName %>MOQEquation" value="<%: Model.MOQEquation %>" class="moq-equation-input" maxlength="250" />
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
                    <b><div id="MOQEquation-ErrorTitle">Invalid MOQ Equation.</div></b>
                    <div id="MOQEquation-ErrorText"></div>
                </div>
                <div class="clear"></div>
            </div>
        </div>
    </div>
    <table id="MOQEquation-Variables" class="moqVariables"></table>
    <div class="moqVariableNote clear display-none"><b>Note:</b> The value of Variables that are the sum of select BOEs will change if the <i>Totals</i> of the select BOEs change.</div>
    <div class="form-row">
        <div class="form-label">
            <span helptext="<%: Model.HelpText %>">MOQ Type **</span>
            <div class="help-icon" onclick="<%: Model.MoqEquationName %>MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text"><%: Html.Raw(Model.HelpText) %></div>
            </div>
        </div>
        <div class="form-element" id="MoqType">
            <select id="MOQType" name="MOQType" class="moqTypes">
                <%= ViewData["MOQTypes"] %>
            </select>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"><%: Model.MOQTextLabel %> **</div>
        <div class="form-element moq-text-area"><%: Html.TextAreaFor(model => model.MOQText, new { onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH + ")" })%></div>
    </div>
    <div id="UsedHistoricalMetrics" class="form-row display-none">
        <div class="form-label">
            <%if (Model.Company == CompanyConfiguration.MST) 
              {%>
            Historical Measures<br />Used
            <div class="help-icon" style="margin-top:1px;" onclick="<%: Model.MoqEquationName %>MOQEquationFieldWidget.ToggleHelp(this);"></div>
            <div class="help-dialog" style="max-width: 275px;">
                <div class="help-dialog-text">The historical measures used to estimate the labor for this task.  If the historical measure is no longer used, it should be deleted.</div>
            </div>
            <%}
              else
              { %>
            <span>Historical Metrics
                <br />
                Used</span>
            <div class="help-icon" style="margin-top:1px;" onclick="<%: Model.MoqEquationName %>MOQEquationFieldWidget.ToggleHelp(this);"></div>
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
<div id="<%: Model.MoqEquationName %>VariableSumOfBOEsByWBSDialogContainer"></div>
<div id="<%: Model.MoqEquationName %>VariableSumOfBOEsByCLINDialogContainer"></div>