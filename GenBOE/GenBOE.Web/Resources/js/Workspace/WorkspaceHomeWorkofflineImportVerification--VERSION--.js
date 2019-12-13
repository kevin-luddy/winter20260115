$(function() {
    //alert('here');
    WorkspaceHomeWorkofflineImportVerificationWidget = {};
    WorkspaceHomeWorkofflineImportVerificationWidget.data = {};

    WorkspaceHomeWorkofflineImportVerificationWidget.CompleteButton = $('#CompleteImportButton-WorkofflineImportVerification');
    WorkspaceHomeWorkofflineImportVerificationWidget.Loader = $('#CompleteImportLoader-WorkofflineImportVerification');

    WorkspaceHomeWorkofflineImportVerificationWidget.SetCompleteImport = function () {
        if (WorkspaceHomeWorkofflineImportVerificationWidget.invalidData == true) {
            WorkspaceHomeWorkofflineImportVerificationWidget.CompleteButton.addClass('display-none');
        }
        else {
            WorkspaceHomeWorkofflineImportVerificationWidget.CompleteButton.removeClass('display-none');
        }
    };

    //if (WorkspaceHomeWorkofflineImportVerificationWidget.data.importResults.length < 1) {
    //    $('#CompleteImportButton-WorkofflineImportVerification').addClass('display-none');
    //}

    WorkspaceHomeWorkofflineImportVerificationWidget.CompleteButton.click(function () {
        WorkspaceHomeWorkofflineImportVerificationWidget.CompleteButton.addClass('display-none');
        WorkspaceHomeWorkofflineImportVerificationWidget.Loader.removeClass('display-none');

        $.ajax({
            type: 'POST',
            url: window.parent.CreatePostURL(WorkspaceHomeWorkofflineImportVerificationWidget.CurrentWorkspace,
                WorkspaceHomeWorkofflineImportVerificationWidget.Controller,
                WorkspaceHomeWorkofflineImportVerificationWidget.Action, ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(WorkspaceHomeWorkofflineImportVerificationWidget.data.importResults),
            success: function (response) {
                window.parent.WorkspaceHomeWidget.hideImportExportDialog();
                window.parent.WorkspaceHomeWidget.ImportSucceeded();
                window.parent.WorkspaceHomeWidget.LoadBOEs();
                window.parent.WorkspaceHomeWidget.RedisplayImport();
            },
            error: function () {
                window.parent.WorkspaceHomeWidget.hideImportExportDialog();
                window.parent.WorkspaceHomeWidget.ImportFailed();
                window.parent.WorkspaceHomeWidget.LoadBOEs();
                window.parent.WorkspaceHomeWidget.RedisplayImport();
            },
            complete: function () {
                window.parent.WorkspaceHomeWidget.ClearFileSelection();
            }
        });
    });

    $('#Back-WorkofflineImportVerification').click(function () {
        window.parent.WorkspaceHomeWidget.ClearFileSelection();
        window.parent.WorkspaceHomeWidget.RedisplayImport();
    });

    window.parent.WorkspaceHomeWidget.ShowExpectedImportResults($('#WorkofflineImportVerification'));
});
