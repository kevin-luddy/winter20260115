<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CustomReportSelectorModelView>" %>

<div id="CustomReportSelectorDialogMainDialog">
<%using (Html.BeginForm(string.Empty, string.Empty, FormMethod.Post, new { id = "CustomReportSelectorForm" })) {%>
    <input type="hidden" name="workspace" value="<%:Model.WorkspaceName%>" />
    <input type="hidden" name="summarizeByCustomField" />
    <ul class="validation-box"> </ul>
    <div>
        Select BOEs and Report Components to be included in the All BOEs report.  Multiple item selection is supported via <i>Shift</i> key for grouped items and <i>Ctrl</i> key for separated items.  *Note:  If the template selected by the workspace administrator is a legacy template, the custom export will override this selection and generate the report using the Master template. 
        <div class="form-row">
            <div class="form-label">BOEs</div>
            <div class="form-element"></div>
        </div>
        <div class="form-row">
            <div class="form-label">Primary sort by:&nbsp;&nbsp;<%: Html.DropDownList("BoeSortBy", Model.BoeSortByList, new { @style = "width: 233px;" })%></div>
            <div class="form-label"><%: Html.DropDownList("BoeSortByValue", Model.BoeSortValues, new { @style = "width: 233px;" })%></div>
        </div>
        <div class="form-row">
            <div class="form-label">Secondary sort by:&nbsp;&nbsp;<%: Html.DropDownList("BoeSecondarySortBy", Model.BoeSecondarySortByList, new { @style = "width: 233px;" })%></div>
            <div class="form-label"><%: Html.DropDownList("BoeSecondarySortByValue", Model.BoeSecondarySortValues, new { @style = "width: 233px;" })%></div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <div style="float: left; width: 280px; height: 180px;">
                    Exclude from Report<br />
                    <%: Html.ListBox("BoesUnselected", Model.BoesUnselected, new { @class = "select-list-with-ellipses", @style = "width: 280px; height: 180px;" })%>
                </div>
                <div style="float: left; width: 100px; height: 180px;" class="buttons inline-block centered">
                    <button id="BoesMoveToSelected" class="ies custom-report-move-button-top" type="button">></button><br />
                    <button id="BoesMoveAllToSelected" class="ies custom-report-move-button" type="button">>></button><br />
                    <button id="BoesMoveToUnselected" class="ies custom-report-move-button" type="button"><</button><br />
                    <button id="BoesMoveAllToUnselected" class="ies custom-report-move-button" type="button"><<</button>
                </div>
                <div style="float: left; width: 280px; height: 180px;">
                    Include in Report<br />
                    <%: Html.ListBox("BoesSelected", Model.BoesSelected, new { @class = "select-list-with-ellipses", @style = "width: 280px; height: 180px;" })%>
                </div>
                <div style="float: left; width: 150px; height: 180px;" class="buttons inline-block centered">
                    <button id="BoesMoveItemsUp" style="margin-top: 50px;" class="ies move-button" type="button">Move Up</button><br /><br/>
                    <button id="BoesMoveItemsDown" style="margin-top: 40px;" class="ies move-button" type="button">Move Down</button>
                </div>
            </div>
            <div class="form-element"></div>
        </div>
        <div class="form-row">
            <div class="form-label">Report Components</div>
            <div class="form-element"></div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <div style="float: left; width: 280px; height: 180px;">
                    Exclude from Report<br />
                    <%: Html.ListBox("ComponentsUnselected", Model.ComponentsUnselected, new { @class = "select-list-with-ellipses", @style = "width: 280px; height: 180px;" })%>
                </div>
                <div style="float: left; width: 100px; height: 180px;" class="buttons inline-block centered">
                    <button id="ComponentsMoveToSelected" class="ies custom-report-move-button-top" type="button">></button><br />
                    <button id="ComponentsMoveAllToSelected" class="ies custom-report-move-button" type="button">>></button><br />
                    <button id="ComponentsMoveToUnselected" class="ies custom-report-move-button" type="button"><</button><br />
                    <button id="ComponentsMoveAllToUnselected" class="ies custom-report-move-button" type="button"><<</button><br />
                </div>
                <div style="float: left; width: 280px; height: 180px;">
                    Include in Report<br />
                    <%: Html.ListBox("ComponentsSelected", Model.ComponentsSelected, new { @class = "select-list-with-ellipses", @style = "width: 280px; height: 180px;" })%>
                </div>
            </div>
            <div class="form-element"></div>
        </div>
    </div>
<%}%>
</div>
<div id="CustomReportSelectorDialogIsBusy" class="loader display-none" style="margin: 275px 100px 275px 400px;">
</div>