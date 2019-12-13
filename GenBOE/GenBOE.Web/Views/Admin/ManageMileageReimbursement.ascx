<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.ManageMileageReimbursementModelView>" %>

<script type="text/javascript">
    var ManageMileageReimbursementWidget = new Widget('MiscRateOptionForm', <%: ViewData["READONLY"] %>);

        ManageMileageReimbursementWidget.SaveMileageRate = function() { 
            $('#SaveButton-ManageMileage').addClass('disabled');
            $('#SaveLoader-ManageMileage').removeClass('display-none');
            $('#SaveButton-ManageMileage').addClass('display-none');

            var dataToSend = {};
            var mileage = {};
            mileage.MileageReimbursementRate = $('#MileageReimbursementRate').val();
            dataToSend = JSON.stringify(mileage);

            ManageMileageReimbursementWidget.ajaxRequest({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_ADMIN %>',
                    '<%: WebConstants.ACTION_SAVE_MILEAGE_REIMBURSEMENT %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function(response) {
                    window.location.hash = '#';
                    RaiseNotification("Changes saved");
                },
                error: function(response) {
                    $('#SaveButton-ManageMileage').removeClass('display-none');
                    $('#SaveLoader-ManageMileage').addClass('display-none');
                }
            }, $('#SaveButton-ManageMileage') );
        }
        
    ManageMileageReimbursementWidget.CancelClicked = function() {
    
    window.location.hash = '#';

    }

    ManageMileageReimbursementWidget.FilterRate =  function() {
        $('#SaveButton-ManageMileage').removeClass('disabled');
        var value = $('#MileageReimbursementRate').val();

        $('#MileageReimbursementRate').val($('#MileageReimbursementRate').val().replace(/[^\,\.0-9]/g, ''));
        var dotIndex = -1;
        dotIndex = value.indexOf('.');

        if(dotIndex >0 && value.substring(dotIndex, value.length).length > 6)
        {
            $('#MileageReimbursementRate').val(value.substring(0, dotIndex + 6));
        }
    }

    $(function() {        
        ManageMileageReimbursementWidget.registerForEvent('CLEAN_SYSTEM_ADMIN_DIRTY', function () { ManageMileageReimbursementWidget.cleanDirty(); });

        $('#CancelButton-ManageMileage').click(ManageMileageReimbursementWidget.CancelClicked);

        $('#SaveButton-ManageMileage').click(ManageMileageReimbursementWidget.SaveMileageRate);

        $('#ManageMileageForm').submit( function(e) {
            ManageMileageReimbursementWidget.SaveMileageRate();
            e.preventDefault();
        });
    });
</script>

<div id="ManageMileage" class="manage-default-resources section">
    <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ManageMileageForm" })) { %>
    <ul class="validation-box"> </ul>
    <div class="title">Manage Mileage Reimbursement Rate</div>
    <div class="data">
    <br />

    <div class="form-row">
        <div class="form-label" style="width:240px">Mileage Reimbursement Rate $/mile</div>
        <div class="form-element" style="width:200px">
            <%: Html.TextBoxFor(model => model.MileageReimbursementRate, new { id = "MileageReimbursementRate", onkeyup = "ManageMileageReimbursementWidget.FilterRate()" })%>
        </div>
    </div>
        <div class="form-row">
            <div class="manage-default-resources-buttons form-element">
                <div class="buttons inline">
                    <button id="SaveButton-ManageMileage" class="ies-action disabled" name="save-button" type="button">Save</button>
                    <div id="SaveLoader-ManageMileage" class="loader display-none"></div>
                    <button id="CancelButton-ManageMileage" class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
            </div>
        </div>
  <% } %>
    </div>
</div>