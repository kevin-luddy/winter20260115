<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.WorkspaceAllowSearchModelView>" %>

<script type="text/javascript">

    WorkspaceAllowSearchWidget = new Widget("WorkspaceAllowSearch", <%: ViewData["READONLY"] %>);

    WorkspaceAllowSearchWidget.preparedForSubmit = function () {
        WorkspaceAllowSearchWidget.data.AllowSearch = $("input[name='AllowSearch']:checked").val();
        WorkspaceAllowSearchWidget.data.ContainsTemplate = $("input[name='ContainsTemplate']:checked").val();
        WorkspaceAllowSearchWidget.data.UpdateDateLong = '<%: Model.UpdateDateLong %>';
        return true;
    }

    WorkspaceAllowSearchWidget.applyReadOnly = function () {
        $('#WorkspaceAllowSearch input[name=AllowSearch]').prop('disabled', true);
        $('#WorkspaceAllowSearch input[name=ContainsTemplate]').prop('disabled', true);
        $('#WorkspaceAllowSearch .buttons').addClass('display-none');
            
        $('#Back-WorkspaceAllowSearch').removeClass('display-none');
    };

    WorkspaceAllowSearchWidget.CheckContainsTemplate = function()
    { 
        if ($("input[name='ContainsTemplate']:checked").val() == "true")
        {
            $("input[name='AllowSearch']")[0].checked = true;
            $('#WorkspaceAllowSearch input[name=AllowSearch]').prop('disabled',true);
        }
        else if (!WorkspaceAllowSearchWidget.isReadOnly())
        {
            $('#WorkspaceAllowSearch input[name=AllowSearch]').prop('disabled',false);
        }
        WorkspaceAllowSearchWidget.setDirty();
    };

    WorkspaceAllowSearchWidget.BackToJumpPage = function() {
        WorkspaceAllowSearchWidget.cleanDirty();
        window.location.hash = '#';
    };

    $(function () {
        WorkspaceAllowSearchWidget.afterDOMLoad();
        createModule($('#WorkspaceAllowSearch'));

        WorkspaceAllowSearchWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { WorkspaceAllowSearchWidget.cleanDirty(); });

        $('#Cancel-WorkspaceAllowSearch, #Back-WorkspaceAllowSearch').click(function () {
            if (WorkspaceAllowSearchWidget.isDirty()) {
                Session.confirmDialog(
                    "Cancel",
                    "Are you sure you want to cancel all changes?",
                    function() {
                        WorkspaceAllowSearchWidget.BackToJumpPage();
                    },
                    null);
            }
            else {
                WorkspaceAllowSearchWidget.BackToJumpPage();
            }
        });


        WorkspaceAllowSearchWidget.registerForLiveEvent('click', '#Save-WorkspaceAllowSearch:not(.disabled)', function () {

            if (WorkspaceAllowSearchWidget.preparedForSubmit()) {

                $('#Save-WorkspaceAllowSearch').addClass('display-none');
                $('#Loader-WorkspaceAllowSearch').removeClass('display-none');

                var dataToSend = JSON.stringify(WorkspaceAllowSearchWidget.data);

                $.ajax({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%: WebConstants.ACTION_SAVE_WORKSPACE_ALLOW_SEARCH %>', ''),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function () {
                        WorkspaceAllowSearchWidget.BackToJumpPage();
                    },
                    error: function () {
                        $('#Save-WorkspaceAllowSearch').removeClass('display-none');
                        $('#Loader-WorkspaceAllowSearch').addClass('display-none');
                    }
                });
            }
        });

        $("#WorkspaceAllowSearch :input[name=ContainsTemplate]").click(WorkspaceAllowSearchWidget.CheckContainsTemplate);

        if (WorkspaceAllowSearchWidget.isReadOnly()) {
           
            WorkspaceAllowSearchWidget.applyReadOnly();
        }

        refreshModule($('#WorkspaceAllowSearch'));

        // if the AllowSearch radiobutton has been clicked, set dirty to enable save
        $(":input[name=AllowSearch]").click(function () {
            var toState = $("input[name='AllowSearch']:checked").val();
            var fromState = "<%:Model.AllowSearch %>";
            // only enable the save if the selection really changed
            if (toState != fromState)
            {
                WorkspaceAllowSearchWidget.setDirty();
            }
        });

        // Make sure radios are disabled on load if Contains Template
        WorkspaceAllowSearchWidget.CheckContainsTemplate();
    });


</script>

<div id="WorkspaceAllowSearch" class="workspace-allow-search module">
    <div class="module-header-data">Share and Allow Search</div>
    <div class="module-content-data">
        <div class="form-row">
            Specify if all information within the   Workspace can be shared with all genBOE users once the Workspace status is Complete. 
            This will allow all genBOE users to search and reuse Workspace and BOE data. Sharing information encouraged to promote efficiency. 
            If the Workspace contains Organizational Conflict of Interest (OCI) information, it cannot be shared and searched.
        </div>
        <label><b>Workspace Template</b></label>
        <div class="form-row">
            <div class="form-element">
                <%: Html.CheckBox("ContainsTemplate",Model.ContainsTemplate) %>&nbsp;<label for="ContainsTemplate">Workspace Contains BOE Templates.</label>
            </div>
        </div>
        <label><b>Share & Allow Search*</b></label>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("AllowSearch", true, Model.AllowSearch == true)%>
                Yes, share and allow users to search all Workspace Information when the Workspace’s status is Complete.
            </div>
        </div>
        <div class="form-row">
            <div class="form-element radio">
                <%: Html.RadioButton("AllowSearch", false, Model.AllowSearch == false)%>
                No, do not share and allow users to search this Workspace.
            </div>
        </div>
         <div class="buttons">
            <button id="Save-WorkspaceAllowSearch" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="Loader-WorkspaceAllowSearch" class="loader display-none"></div>
            <button id="Cancel-WorkspaceAllowSearch" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
        <button id="Back-WorkspaceAllowSearch" class="ies back-to-workspace-settings-button display-none" type="button">Back to Workspace Settings</button>
    </div>
</div>
