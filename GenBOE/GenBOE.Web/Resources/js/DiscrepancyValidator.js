
function InitializeDiscrepancyValidator(validateDiscrepancyUrl) {
    var DiscrepancyValidator = new Widget();
    var serviceUrl = validateDiscrepancyUrl;

    /**
     * Validates the workspace for discrepancies.  Proceeds with report if none are found.  If discrepancies are found, prompts the user to determine whether to continue.
     * 
     * @param proceedWithExportFunction
     *          Function to call if there are no discrepancies or if the user elects to proceed with the export.
     */
    DiscrepancyValidator.ValidateDiscrepancies = function (proceedWithExportFunction) {
        // Show the loading box as this may take a while.
        ShowLoadingBox();

        // Check for discrepancies.
        $.ajax({
            type: "POST",
            url: serviceUrl,
            dataType: 'json',
            success: function (discrepancyData) {
                // Hide the loading image.
                HideLoadingBox();

                // If discrepancies were found, display them to the user before proceeding with the export.
                if (discrepancyData != null && discrepancyData.length > 0)
                {                                
                    var messageBody = DiscrepancyValidator.GetBoeDiscrepancyMessage(discrepancyData);
                    GenSession.proceedDialog("Discrepancy Errors", messageBody,
                    function () {
                        proceedWithExportFunction();
                    });
                }
                else
                {
                    // Otherwise, just proceed with the export.
                    proceedWithExportFunction();
                }
            }
        });
    }

    /**
     * Builds the message to display to the user with all of the discrepancy data.
     * 
     * @param discrepancyData
     *          JSON object containing all of the discrepancy data that was found.  Assumed not to be null as this is called from ValidateDiscrepancies() which checks for this condition.
     */
    DiscrepancyValidator.GetBoeDiscrepancyMessage = function (discrepancyData) {
        var messageBody = '';
        var i = 0;
        
        // Message Header and beginning of BOE list.
        messageBody += "<div class=\"text-left\">The following BOE discrepancy errors have been found.  To continue running the export with these discrepancy errors, click the OK button.</div><br\>";
        messageBody += "<ul class=\"text-left\">";

        while (i < discrepancyData.length) {
            // Add BOE information items.
            messageBody += "<li>";
            messageBody += "<span class=\"bold\">WBS: </span>" + discrepancyData[i].Wbs + "<br\>"
            messageBody += "<span class=\"bold\">BOE Title: </span>" + discrepancyData[i].BoeTitle + "<br\>"
            messageBody += "<span class=\"bold\">CLIN: </span>" + discrepancyData[i].Clin + "<br\>";
            messageBody += "<span class=\"bold\">BOE Approver(s): </span>" + discrepancyData[i].BoeAuthors + "<br\>";

            // Begin Discrepancy list.
            if (discrepancyData[i].ElementsWithIssues != null) {
                var j = 0;
                messageBody += "<ul>";

                // Add Discrepancy information items.
                while (j < discrepancyData[i].ElementsWithIssues.length) {
                    messageBody += "<li>";
                    var taskId = discrepancyData[i].ElementsWithIssues[j].DisplayedTaskId;
                    messageBody +=  taskId != null && taskId != '' ? "<span class=\"bold\">Task ID: </span>" + taskId + "<br\>" : '';
                    messageBody += "<span class=\"bold\">Task Title: </span>" + discrepancyData[i].ElementsWithIssues[j].TaskTitle + "<br\>";
                    messageBody += "<span class=\"bold\">Discrepancy: </span>" + discrepancyData[i].ElementsWithIssues[j].InconsistencyText + "<br\>";
                    messageBody += "</li>";
                    j++;
                }

                // End Discrepancy list.
                messageBody += "</ul>";
            }

            // End BOE information item.
            messageBody += "</li>";
            i++;
        }

        // End Message.
        messageBody += "</ul>";
        return messageBody;
    }

    return DiscrepancyValidator;
}