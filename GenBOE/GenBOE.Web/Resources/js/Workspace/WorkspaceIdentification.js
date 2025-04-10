/*
* This javascript file is called from Workspace Identification ascx files
*/

function InitializeWorkspaceIdentificationWidget(widgetConfig, jumpUrl) {
    WorkspaceIdentificationWidget = new GenWidget(widgetConfig);

    WorkspaceIdentificationWidget.AdjustDates = {};

    WorkspaceIdentificationWidget.Cancel = function () {
        if (WorkspaceIdentificationWidget.isDirty()) {
            Session.confirmDialog(
                "Cancel",
                "Are you sure you want to cancel all changes?",
                function () {
                    WorkspaceIdentificationWidget.cleanDirty("WorkspaceIdentificationForm");
                    WorkspaceIdentificationWidget.BackToJumpPage();
                },
                null);
        }
        else {
            WorkspaceIdentificationWidget.BackToJumpPage();
        }
	};

    WorkspaceIdentificationWidget.ContainsOCIRadioClick = function () {
		var form = $('#WorkspaceIdentificationForm');
		if (form.find('input[name=ContainsOCI]:checked').val() === 'True') {
            form.find('.oci-note').html('<b>Note:</b> ' + widgetConfig.FormConfigs[0].BannerTextWithoutOCI);
        }
        else {
            GenSession.confirmDialog("Contains OCI Information", "Is the Workspace clean of all OCI data?",
				function () {
					form.find('.oci-note').html('<b>Note:</b> ' + widgetConfig.FormConfigs[0].BannerTextWithOCI);
                },
                function () {
					$('#ContainsOCI-Yes').prop("checked", true);
                },
                function () {
					$('#ContainsOCI-Yes').prop("checked", true);
                });
        }
    };

    WorkspaceIdentificationWidget.BackToJumpPage = function (message, newJumpUrl) {
        if (message && message.length > 0) {
            sessionStorage.errorMessage = message;
        }

        // Cannot do a simple hash change because OCI might have changed
        if (newJumpUrl !== undefined) {
            window.location = newJumpUrl;
        } else {
            window.location = jumpUrl;
        }
    };

    WorkspaceIdentificationWidget.WarnPrecisionChange = function () {
        GenSession.confirmDialog("Resource Decimal Precision", "Changing Resource Decimal Precision will cause all task elements in the workspace to be recalculated. This could be a long running process. Would you like to continue with this change?",
             // Do nothing if they give postive confirmation.
             null,
             function () {
                 // Negative confirmation. Restore old Resource Decimal Precision value.
                 $("#ResourceDecimalPrecision").val($("#OriginalDecimalPrecision").val());
             });
    }

    WorkspaceIdentificationWidget.WarnCostPrecisionChange = function () {
        if ($("#CostDecimalPrecision").val() != $("#OriginalCostDecimalPrecision").val()) {
            GenSession.confirmDialog("Cost Decimal Precision", "Changing Cost Decimal Precision will cause all task elements in the workspace to be recalculated. This could be a long running process. Would you like to continue with this change?",
                 // Do nothing if they give postive confirmation.
                 null,
                 function () {
                     // Negative confirmation. Restore old Cost Decimal Precision value.
                     $("#CostDecimalPrecision").val($("#OriginalCostDecimalPrecision").val());
                 });
        }
    }

    return WorkspaceIdentificationWidget;
}