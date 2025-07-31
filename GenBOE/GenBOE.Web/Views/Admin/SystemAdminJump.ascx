<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import namespace="IES.Common.classes" %>
<script type="text/javascript">
    var SystemAdminJumpWidget = new Widget('SystemAdminJump');
</script>

<div id="SystemAdminJump" class="system-admin-jump section">
    <div class="title">System Administration</div>
    <div class="data">
        <div class="left-column">
            <div>
                <a id="SystemAdminPermissions" href="#SystemAdminPermissions">System Administration Permissions</a>
                <div>
                    Add new or delete System Administrators. System Administrators will be able to manage
                    system settings and access all Workspaces.</div>
            </div>
            <div>
                <a id="CreateWorkspacePermissions" href="#CreateWorkspacePermissions">Create Workspace Permissions</a>
                <div>
                    Manage who can create workspaces.</div>
            </div>
            <div>
                <a id="ManageDefaultResources" href="#ManageDefaultResources">Manage Default Resources</a>
                <div>
                    Manage the default list of resources that can be used by Workspaces. Resources
                    may be a combination of Segment/Region and Labor Type.
                </div>
            </div>
            <div>
                <a id="ManageDefaultPerfOrgs" href="#ManageDefaultPerfOrgs">Manage Default Performing Organizations</a>
                <div>
                    Manage the default list of Performing Organizations that can be used by Workspaces.</div>
            </div>
            <div>
                <a id="ManageOutputFormatTemplates" href="#ManageOutputFormatTemplates">Manage Output Format Templates</a>
                <div>
                    Manage output format templates access in association with Workspaces.
                </div>
            </div>
            <% // This is only for RMS. SSC does not use this feature.
            if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
            {  %>
            <div>
                <a id="ManageSystemSettings" href="#ManageSystemSettings">Manage System Settings</a>
                <div>
                    Manage system settings.
                </div>
            </div>
            <% } %>
        </div>
        <div class="right-column">
            <% // This feature is no longer used. But we are not allowed to delete, because maybe, just maybe, in the future, maybe, they will want it back.
                if (false)
                { %> 
                <div>
                    <a id="ManageTripsForTravel" href="#ManageTripsForTravel">Manage Trips for Travel</a>
                    <div>
                        Manage the fare, hotel, per diem, and rental car rates for trips.
                    </div>
                </div>
                <div>
                    <a id="ManageMiscRates" href="#ManageMiscRates">Manage Miscellaneous Rates for Travel Mode</a>
                    <div>
                        Manage the miscellaneous rates for transportation modes.
                    </div>
                </div>
            <% } %>
            <% // This is only for RMS. SSC does not use this feature.
                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
                {  %>
                 <div>
                     <a id="ManageZoneTravel" href="#ManageZoneTravel">Manage Zone Travel Origins</a>
                     <div>
                         Manage Origins and their Sites and Resource IDs for Zone Travel.
                     </div>
                 </div>
                <div>
                     <a id="ManageZoneTravelDestinations" href="#ManageZoneTravelDestinations">Manage Zone Travel Destinations</a>
                     <div>
                         Manage Zones for Zone Travel Destinations.
                     </div>
                 </div>
                <div>
                    <a id="ManageNonzoneFeesAndCosts" href="#ManageNonzoneFeesAndCosts">Manage Fees and Costs for Nonzone Travel</a>
                    <div>
                        Manage the Travel Agency Fee and Miscellaneous/Other Costs for Nonzone Domestic and International Travel Trips.
                    </div>
                </div>
                <div>
                    <a id="ManageEscalationRates" href="#ManageEscalationRates">Manage Escalation Rates for Travel</a>
                    <div>
                        Manage Escalation Rates for Travel.
                    </div>
                </div>
                <%if (SiteMasterUtilities.IsProjectMapEnabled) { %>
                <div>
                    <a id="ManageOffloadRates" href="#ManageOffloadRates">Manage Offloading Rates</a>
                    <div>
                        Manage the rates for Offloading.
                    </div>
                </div>
                <div>
                    <a id="ManageLegacyResources" href="#ManageLegacyResources">View Legacy Resources</a>
                    <div>
                        View the Legacy Resources.
                    </div>
                </div>
                <%} %>
            <% } %>

            <% // This feature is no longer used. But we are not allowed to delete, because maybe, just maybe, in the future, maybe, they will want it back.
                if (false)
                { %> 
                <div>
                    <a id="ManageMileageReimbursement" href="#ManageMileageReimbursement">Manage Mileage Reimbursement Rate</a>
                    <div>
                        Set the reimbursement rate for mileage driven in a personal car.
                    </div>
                </div>
            <% } %>
            <div>
                <a id="ManageEmailPreferences" href="#ManageEmailPreferences">Manage Email Preferences</a>
                <div>
                    Manage Default and Forced Email Preferences for Workspaces.
                </div>
            </div>
            <div>
                <a id="ManageOverdueTraining" href="#ManageOverdueTraining">Manage Overdue Training</a>
                <div>
                    Determine which Users have Overdue Training.
                </div>
            </div>
            <div>
                <a id="ManageSystemProPricerExports" href="#ManageSystemProPricerExports">Manage System ProPricer Export Templates</a>
                <div>
                    Manage the System-level ProPricer Export Templates.
                </div>
            </div>
            <% // Manage UCOT is Space-only
                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems && Utilities.IsUCOTEnabledForSystem)
                { %>
            <div>
                <a id="ManageUCOT" href="#ManageUCOT">Manage Uncompensated Overtime</a>
                <div>
                    Manage and edit the UCOT (Uncompensated Overtime) Factor.
                </div>
            </div>
            <% } %>
            <% // Manage Skill Mix Settings is Space-only
                if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems)
                {
                %>
            <div>
                <a id="ManageSkillMixSettings" href="#ManageSkillMixSettings">Manage Skill Mix Settings</a>
                <div>
                    Manage Skill Mix settings.
                </div>
            </div>
            <% } %>
        </div>
        <div class="clear"></div>
    </div>
</div>
