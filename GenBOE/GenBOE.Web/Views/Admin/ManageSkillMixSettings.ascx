<script type="text/javascript">
	var ManageSkillMixSettingsWidget = new Widget('ManageSkillMixSettings', false);
	$(function () {
		ManageSkillMixSettingsWidget.registerForEvent('MANAGE_SKILL_MIX_SETTINGS_LOADED', function () {
			return true;
		});
	})
</script>

<div id="ManageSkillMixSettings" class="manage-skill-mix-settings section">
    <div class="title">Manage Skill Mix Settings</div>
    <div class="data">
        <div data-ng-app="genboe" data-ng-controller="manageSkillMixSettingsController">
    <form name="skillMixSettingsForm" id="skillMixSettingsForm">
        <div data-ng-cloak>
            <div class="form-row css3pie-position-fix">
                <gen-validation data-errors="$root.errors"></gen-validation>                    
            </div>
            <div class="form-row">
                <div ui-grid="gridOptions" ui-grid-edit ui-grid-cellNav ui-grid-exporter class="system-setting-grid-ui" />
            </div>
            <div class="form-row css3pie-position-fix last-form-row">
                <div class="inline css3pie-position-fix">
                    <%--<button data-ng-click="saveSystemSettings()" data-ng-disabled="isDataLoading || !PageIsDirty" type="button" class="ies-action">Save</button>--%>
                    <button data-ng-click="confirmCancel()" data-ng-disabled="isDataLoading" type="button" class="ies">Cancel</button>
                    &nbsp; <!-- need line-height and nbsp to ensure div still occupies some real estate when button is hidden -->
                </div>
            </div>
        </div>
    </form>
</div> 
    </div>
 </div>