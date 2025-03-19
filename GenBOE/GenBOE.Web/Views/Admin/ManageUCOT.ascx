<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import namespace="Newtonsoft.Json" %>

<script type="text/javascript">
    var ManageUCOTWidget = new Widget('ManageUCOT', false);

    ManageUCOTWidget.BindEvents = function () {
        ManageUCOTWidget.registerForLiveEvent('click', "#Save-ManageUCOT", function () {
            ManageUCOTWidget.SaveButtonClick();
        });
    };

    ManageUCOTWidget.EnableSave = function () {
        $('#Save-ManageUCOT').removeClass('disabled');
    };

    ManageUCOTWidget.SaveButtonClick = function () {
        console.log("dataToSend", "got here");
        $('#Save-ManageUCOT').addClass('display-none');
        $('#Loader-ManageUCOT').removeClass('display-none');
        const dataToSend = JSON.stringify({ ucot: $('#ucot').val() });

        ManageUCOTWidget.ajaxRequest({
            type: 'POST',
            url: CreateSystemAdminPostURL(
                '<%:WebConstants.CONTROLLER_ADMIN %>',
                '<%:WebConstants.ACTION_SAVE_UCOT %>', ''
            ),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                $('#Loader-ManageUCOT').addClass('display-none');
                $('#Save-ManageUCOT').removeClass('display-none');
                Session.alertDialog("Success", "Your changes have been saved.");
            },
            error: function () {
                $('#Loader-ManageUCOT').addClass('display-none');
                $('#Save-ManageUCOT').removeClass('display-none');
            }
        }, $('#Save-ManageUCOT'));
    };

    $(function () {
        ManageUCOTWidget.registerForEvent('MANAGE_UCOT_LOADED', function () {
            return true;
        });
        ManageUCOTWidget.BindEvents();
    });
</script>
<div class="section">
    <div class="title">Manage UCOT</div>
    <div class="data">
         <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ManageUCOTForm", name="ManageUCOTForm" })) { %>
             <ul class="validation-box"></ul>
             <div class="form-row">
                 <div class="form-label">
                     <span>UCOT Factor %</span>
                 </div>
                 <div class="form-element">
                     <input value="<%:Model %>"  id="ucot" name="ucot" type = "number", step = "any" onchange = "ManageUCOTWidget.EnableSave()" />&nbsp;&nbsp;
                     <div class ="buttons inline">
                         <button id="Save-ManageUCOT" class="ies-action disabled" name="save-button" type="button">Save</button>
                         <div id="Loader-ManageUCOT" class="loader display-none"></div>
                     </div>
                 </div>
             </div>
         <% } %>
    </div>
</div>
