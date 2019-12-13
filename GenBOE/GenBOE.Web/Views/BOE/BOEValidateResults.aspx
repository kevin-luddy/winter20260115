<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1">
    <!--  *** DO NOT PLACE ANYTHING ABOVE THIS META STATEMENT, OR YOU WILL BREAK US AGAIN. THIS HAS TO BE THE FIRST THING IN THE HEAD. IF THAT'S AN ISSUE, 
                TALK TO DUSAN OR MATT. *** 
        Force Internet Explorer 8 if its available.
        -->
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />

    <title>Validation Errors</title>

    <script type="text/jscript">
        // Global variable that stores the production URL. This is used to decide whether we are in production or not.
        // It is created/assigned in every master/layout page, so that way our JS libraries (in this solution and in common) don't have to have the URL hardcoded,
        // but they can use the web.config values instead :)
        var prodGenBoeUrl_Global = "<%: SiteMasterUtilities.ProductionUrl() %>";

        // This script is used to prevent cross-frame attacks
        if (top.frames.length != 0) {
            top.location = self.document.location;
        }
    </script>

    <%: Scripts.Render("~/bundles/results") %>
    <%: Styles.Render("~/Content/homeCss") %>
    
    <script type="text/javascript">
        var prefix = ''; 
    </script> 

    <script type="text/javascript">
    var BOEValidateResults = new Widget("BOEValidateResults");

    $(function () {        
        BOEValidateResults.GetValidationErrors();
    });

    BOEValidateResults.GetValidationErrors = function()
    {        
        var dataToSend = { "id": <%=ViewData["BOEID"]%> };
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%: WebConstants.CONTROLLER_BOE%>',
            '<%: WebConstants.ACTION_VALIDATE_BOE%>',
            'boe/<%: ViewData["BOEID"] %>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: BOEValidateResults.ValidateSucess,
            error: BOEValidateResults.ValidateFailure
        });
    };

    BOEValidateResults.ValidateSucess = function(results) {

        $("#BOEValidateResults-ValidationResults").children().remove();

        if (results != null &&
            ((results.BOEHeaderMsgs != null && results.BOEHeaderMsgs.length > 0) ||
            (results.Tasks != null && results.Tasks.length > 0) || (results.BOECommentandApprovals != null && results.BOECommentandApprovals.length > 0) 
            || (results.Materials != null && results.Materials.length > 0) ||(results.Costs != null && results.Costs.length > 0)
            || (results.Travels != null && results.Travels.length > 0)
            || (results.BOECustomFieldValidationMessages != null && results.BOECustomFieldValidationMessages.length > 0)))
        {
            if (results.BOEHeaderMsgs != null && results.BOEHeaderMsgs.length > 0)
            {
                $("#BOEValidateResults-ValidationResults").append('<h1>BOE Header</h1>');
                for(headers in results.BOEHeaderMsgs)
                {
                    var H = results.BOEHeaderMsgs[headers];
                    $("#BOEValidateResults-ValidationResults").append('<div>' + H + '</div>');
                    $("#BOEValidateResults-ValidationResults").append('<br/>');
                }
            }

            if (results.BOECustomFieldValidationMessages != null && results.BOECustomFieldValidationMessages.length > 0)
            {
               
                // if there was no BOE Header Msgs, but we have custom field validation msgs, add the BOE Header
                if (results.BOEHeaderMsgs.length == 0)
                {
                    $("#BOEValidateResults-ValidationResults").append('<h1>BOE Header</h1>');
                }
                for(customFields in results.BOECustomFieldValidationMessages)
                {
                   var CF = results.BOECustomFieldValidationMessages[customFields];
                  
                    $("#BOEValidateResults-ValidationResults").append('<div>' + CF  + '</div>');
                     $("#BOEValidateResults-ValidationResults").append('<br/>');
                }
            }            

            if (results.Tasks != null && results.Tasks.length > 0)
            {
                $("#BOEValidateResults-ValidationResults").append('<h1>LM / IWTA / Sub Labor</h1>');
                for (tasksNdx in results.Tasks)
                {
                    var task = results.Tasks[tasksNdx];
                    $("#BOEValidateResults-ValidationResults").append('<h2>' + task.TaskMessage + '</h2>');

                    if(task.TaskElementDetails != null)
                    {
                        var taskDetails = task.TaskElementDetails;
                        $("#BOEValidateResults-ValidationResults").append('<h3>' + taskDetails.TaskElementDetailsHeader + '</h3>');
                        
                        if (taskDetails.TaskElementDetailValidationMessages != null && taskDetails.TaskElementDetailValidationMessages.length > 0)
                        {
                            for (detailMessageNdx in taskDetails.TaskElementDetailValidationMessages)
                            {
                                var detailMessage = taskDetails.TaskElementDetailValidationMessages[detailMessageNdx];
                                $("#BOEValidateResults-ValidationResults").append('<div>' + detailMessage + '</div>');
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                
                    if(task.LaborTypes != null && task.LaborTypes.length > 0)
                    {
                        for (laborTypeNdx in task.LaborTypes)
                        {
                            var laborType = task.LaborTypes[laborTypeNdx];
                            $("#BOEValidateResults-ValidationResults").append('<h3>' + laborType.LaborTypeHeader + '</h3>');
                        
                            if (laborType.LaborTypeValidationMsgs != null && laborType.LaborTypeValidationMsgs.length > 0)
                            {
                                for (laborMessageNdx in laborType.LaborTypeValidationMsgs)
                                {
                                    var laborMessage = laborType.LaborTypeValidationMsgs[laborMessageNdx];
                                    $("#BOEValidateResults-ValidationResults").append('<div>' + laborMessage + '</div>');
                                }
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                }                                
            }

            if (results.Travels != null && results.Travels.length > 0)
            {
                $("#BOEValidateResults-ValidationResults").append('<h1>Travel</h1>');
                for (tasksNdx in results.Travels)
                {
                    var task = results.Travels[tasksNdx];
                    $("#BOEValidateResults-ValidationResults").append('<h2>' + task.TaskMessage + '</h2>');

                    if(task.TaskElementDetails != null)
                    {
                        var taskDetails = task.TaskElementDetails;
                        $("#BOEValidateResults-ValidationResults").append('<h3>' + taskDetails.TaskElementDetailsHeader + '</h3>');
                        
                        if (taskDetails.TaskElementDetailValidationMessages != null && taskDetails.TaskElementDetailValidationMessages.length > 0)
                        {
                            for (detailMessageNdx in taskDetails.TaskElementDetailValidationMessages)
                            {
                                var detailMessage = taskDetails.TaskElementDetailValidationMessages[detailMessageNdx];
                                $("#BOEValidateResults-ValidationResults").append('<div>' + detailMessage + '</div>');
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                
                    if(task.LaborTypes != null && task.LaborTypes.length > 0)
                    {
                        for (laborTypeNdx in task.LaborTypes)
                        {
                            var laborType = task.LaborTypes[laborTypeNdx];
                            $("#BOEValidateResults-ValidationResults").append('<h3>' + laborType.LaborTypeHeader + '</h3>');
                        
                            if (laborType.LaborTypeValidationMsgs != null && laborType.LaborTypeValidationMsgs.length > 0)
                            {
                                for (laborMessageNdx in laborType.LaborTypeValidationMsgs)
                                {
                                    var laborMessage = laborType.LaborTypeValidationMsgs[laborMessageNdx];
                                    $("#BOEValidateResults-ValidationResults").append('<div>' + laborMessage + '</div>');
                                }
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                }
            }

            if (results.Materials != null && results.Materials.length > 0)
            {
                $("#BOEValidateResults-ValidationResults").append('<h1>Materials</h1>');
                for (tasksNdx in results.Materials)
                {
                    var task = results.Materials[tasksNdx];
                    $("#BOEValidateResults-ValidationResults").append('<h2>' + task.TaskMessage + '</h2>');

                    if(task.TaskElementDetails != null)
                    {
                        var taskDetails = task.TaskElementDetails;
                        $("#BOEValidateResults-ValidationResults").append('<h3>' + taskDetails.TaskElementDetailsHeader + '</h3>');
                        
                        if (taskDetails.TaskElementDetailValidationMessages != null && taskDetails.TaskElementDetailValidationMessages.length > 0)
                        {
                            for (detailMessageNdx in taskDetails.TaskElementDetailValidationMessages)
                            {
                                var detailMessage = taskDetails.TaskElementDetailValidationMessages[detailMessageNdx];
                                $("#BOEValidateResults-ValidationResults").append('<div>' + detailMessage + '</div>');
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                
                    if(task.LaborTypes != null && task.LaborTypes.length > 0)
                    {
                        for (laborTypeNdx in task.LaborTypes)
                        {
                            var laborType = task.LaborTypes[laborTypeNdx];
                            $("#BOEValidateResults-ValidationResults").append('<h3>' + laborType.LaborTypeHeader + '</h3>');
                        
                            if (laborType.LaborTypeValidationMsgs != null && laborType.LaborTypeValidationMsgs.length > 0)
                            {
                                for (laborMessageNdx in laborType.LaborTypeValidationMsgs)
                                {
                                    var laborMessage = laborType.LaborTypeValidationMsgs[laborMessageNdx];
                                    $("#BOEValidateResults-ValidationResults").append('<div>' + laborMessage + '</div>');
                                }
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                }
            }

            if (results.Costs != null && results.Costs.length > 0)
            {

                $("#BOEValidateResults-ValidationResults").append('<h1>ODC</h1>');
                for (tasksNdx in results.Costs)
                {
                    var task = results.Costs[tasksNdx];
                    $("#BOEValidateResults-ValidationResults").append('<h2>' + task.TaskMessage + '</h2>');

                    if(task.TaskElementDetails != null)
                    {
                        var taskDetails = task.TaskElementDetails;
                        $("#BOEValidateResults-ValidationResults").append('<h3>' + taskDetails.TaskElementDetailsHeader + '</h3>');
                        
                        if (taskDetails.TaskElementDetailValidationMessages != null && taskDetails.TaskElementDetailValidationMessages.length > 0)
                        {
                            for (detailMessageNdx in taskDetails.TaskElementDetailValidationMessages)
                            {
                                var detailMessage = taskDetails.TaskElementDetailValidationMessages[detailMessageNdx];
                                $("#BOEValidateResults-ValidationResults").append('<div>' + detailMessage + '</div>');
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                
                    if(task.LaborTypes != null && task.LaborTypes.length > 0)
                    {
                        for (laborTypeNdx in task.LaborTypes)
                        {
                            var laborType = task.LaborTypes[laborTypeNdx];
                            $("#BOEValidateResults-ValidationResults").append('<h3>' + laborType.LaborTypeHeader + '</h3>');
                        
                            if (laborType.LaborTypeValidationMsgs != null && laborType.LaborTypeValidationMsgs.length > 0)
                            {
                                for (laborMessageNdx in laborType.LaborTypeValidationMsgs)
                                {
                                    var laborMessage = laborType.LaborTypeValidationMsgs[laborMessageNdx];
                                    $("#BOEValidateResults-ValidationResults").append('<div>' + laborMessage + '</div>');
                                }
                            }
                            $("#BOEValidateResults-ValidationResults").append('<br/>');
                        }
                    }
                }
                                
            }
            if (results.BOECommentandApprovals != null && results.BOECommentandApprovals.length > 0)
            {
                $("#BOEValidateResults-ValidationResults").append('<h1>Comments & Approvals</h1>');
                for(boeCommentandApproval in results.BOECommentandApprovals)
                {
                    var BCA = results.BOECommentandApprovals[boeCommentandApproval];
                    $("#BOEValidateResults-ValidationResults").append('<div>' + BCA + '</div>');
                    $("#BOEValidateResults-ValidationResults").append('<br/>');
                }
            }

            $("#BOEValidateResults-ValidationResults").removeClass("display-none");
        }
        else
        {
            $("#BOEValidateResults-DataValidDialog").removeClass("display-none");
        }
        
    };

    BOEValidateResults.ValidateFailure = function(exceptionString) {
        Session.alertDialog('BOE Validation Error', exceptionString);
    };

    // If the Submit For Approval had invalid BOEs, need to provide the list to the user
     $(document).bind("ACTION_DISPLAY_INVALID_SUBMIT_FOR_APPROVAL", function (e,submitResults) {
        BOEValidateResults.ValidateSucess(submitResults);
     });
    </script>
</head>
<body>
    <div id="BOEValidateResults-ValidationResults" class="boevalidate-validation-results display-none">
    </div>
    <div id="BOEValidateResults-DataValidDialog" style="text-align: center;" class="display-none">
        <div class="dialog-text">
            <br />
            No validation errors were found.
            <br />
            <br />
        </div>
    </div>
</body>
</html>
