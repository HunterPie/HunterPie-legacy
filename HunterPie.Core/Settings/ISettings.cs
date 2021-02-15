namespace HunterPie.Settings
{
    /// <summary>
    /// Mediator to operate single custom settings tab.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Should return 'true' if settings were changed after initial creation.
        /// </summary>
        bool IsSettingsChanged { get; }

        /// <summary>
        /// Load settings from storage to view.
        /// </summary>
        void LoadSettings();

        /// <summary>
        /// Save settings from view to storage. Should only be called after <see cref="ValidateSettings"/> if there are no errors.
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Validate current settings state. Should return string if there are any validation errors.
        /// Otherwise, should return null.
        /// </summary>
        string ValidateSettings();
    }
}
