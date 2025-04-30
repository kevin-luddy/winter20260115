using IES.Common;

namespace GenBOE.DataBridge.DTO.SkillMix
{
    /// <summary>
    /// Class that holds skill mix table help urls
    /// </summary>
    public class SkillMixTableHelpUrls
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public SkillMixTableHelpUrls()
        {
            this.RMSCurrentSkillMixTableHelpUrl = ConfigurationUtilities.GetAppSetting("RMSCurrentSkillMixTableHelpUrl");
            this.RMSLMEnterpriseSkillMixTableHelpUrl = ConfigurationUtilities.GetAppSetting("RMSLMEnterpriseSkillMixTableHelpUrl");
            this.SpaceLegacySkillMixTableHelpUrl = ConfigurationUtilities.GetAppSetting("SpaceLegacySkillMixTableHelpUrl");
            this.SpaceLMEnterpriseSkillMixTableHelpUrl = ConfigurationUtilities.GetAppSetting("SpaceLMEnterpriseSkillMixTableHelpUrl");
        }

        /// <summary>
        /// Url for RMS Current Skill Mix table compliance document
        /// </summary>
        public string RMSCurrentSkillMixTableHelpUrl { get; set; }

        /// <summary>
        /// Url for RMS LM Enterprise Skill Mix table compliance document
        /// </summary>
        public string RMSLMEnterpriseSkillMixTableHelpUrl { get; set; }

        /// <summary>
        /// Url for Space Legacy Skill Mix table compliance document
        /// </summary>
        public string SpaceLegacySkillMixTableHelpUrl { get; set; }

        /// <summary>
        /// Url for Space LM Enterprise Skill Mix table compliance document
        /// </summary>
        public string SpaceLMEnterpriseSkillMixTableHelpUrl { get; set; }
    }
}
