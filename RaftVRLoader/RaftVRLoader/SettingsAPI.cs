using RaftVR;
using RaftVR.Configs;
using System.Globalization;

public class SettingsAPI
{
    public bool ExtraSettingsAPI_Loaded = false;

    private ModInitializer initializerInstance;

    public SettingsAPI(ModInitializer initializerInstance)
    {
        this.initializerInstance = initializerInstance;

        VRConfigs.OnCalibrateSettingsUpdated += UpdateCalibrationSettings;
        VRConfigs.OnFirstSetupDone += UpdateSetupSettings;
    }

    public void ExtraSettingsAPI_Load()
    {
        initializerInstance.OnSettingsAPILoaded();
        RefreshSettings(false);

        string armScaleString = ExtraSettingsAPI_GetDataValue("hiddenSettings", "armScale");
        if (float.TryParse(armScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float armScale))
            VRConfigs.ArmScale = armScale;
        else
            VRConfigs.ArmScale = 0f;

        string legScaleString = ExtraSettingsAPI_GetDataValue("hiddenSettings", "legScale");
        if (float.TryParse(legScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float legScale))
            VRConfigs.LegScale = legScale;
        else
            VRConfigs.LegScale = 0f;

        if (VRConfigs.Runtime != VRConfigs.VRRuntime.None)
            ExtraSettingsAPI_SetComboboxSelectedIndex("runtime", (int)VRConfigs.Runtime - 1);
    }

    public void ExtraSettingsAPI_SettingsClose()
    {
        RefreshSettings();
    }

    private void RefreshSettings(bool writeRuntime = true)
    {
        VRConfigs.SnapTurn = ExtraSettingsAPI_GetCheckboxState("snapTurn");
        VRConfigs.SnapTurnAngle = ExtraSettingsAPI_GetSliderValue("snapTurnAngle");
        VRConfigs.SnapRepeatDelay = ExtraSettingsAPI_GetSliderValue("snapRepeatDelay");
        VRConfigs.SmoothTurnSpeed = ExtraSettingsAPI_GetSliderValue("smoothTurnSpeed");
        VRConfigs.MoveDirectionOrigin = (VRConfigs.DirectionOriginType)ExtraSettingsAPI_GetComboboxSelectedIndex("directionOrigin");
        VRConfigs.SeatedMode = ExtraSettingsAPI_GetCheckboxState("seatedMode");
        VRConfigs.ShowInteractionRay = ExtraSettingsAPI_GetCheckboxState("interactionRay");
        VRConfigs.ShowPlayspaceCenter = (VRConfigs.PlayspaceCenterDisplay)ExtraSettingsAPI_GetComboboxSelectedIndex("playspaceCenter");
        VRConfigs.UseRadialHotbar = (VRConfigs.RadialHotbarMode)ExtraSettingsAPI_GetComboboxSelectedIndex("useRadialHotbar");
        VRConfigs.UnderwaterDistortion = ExtraSettingsAPI_GetCheckboxState("underwaterDistortion");
        VRConfigs.ImmersiveThrowing = ExtraSettingsAPI_GetCheckboxState("immersiveThrow");
        VRConfigs.ImmersiveBow = ExtraSettingsAPI_GetCheckboxState("immersiveBow");
        VRConfigs.ThrowForceMultiplier = ExtraSettingsAPI_GetSliderValue("throwForceMultiplier");
        VRConfigs.UIScale = ExtraSettingsAPI_GetSliderValue("uiScale");
        VRConfigs.VisibleBody = ExtraSettingsAPI_GetCheckboxState("visibleBody");

        RaftVR.UI.UIHelper.UpdateUIScale();

        if (writeRuntime)
            VRConfigs.WriteRuntimeToFile((VRConfigs.VRRuntime)(ExtraSettingsAPI_GetComboboxSelectedIndex("runtime") + 1));
    }

    internal void UpdateCalibrationSettings()
    {
        ExtraSettingsAPI_SetDataValue("hiddenSettings", "armScale", VRConfigs.ArmScale.ToString(CultureInfo.InvariantCulture));
        ExtraSettingsAPI_SetDataValue("hiddenSettings", "legScale", VRConfigs.ArmScale.ToString(CultureInfo.InvariantCulture));
    }

    private void UpdateSetupSettings()
    {
        ExtraSettingsAPI_SetCheckboxState("snapTurn", VRConfigs.SnapTurn);
        ExtraSettingsAPI_SetCheckboxState("seatedMode", VRConfigs.SeatedMode);
        ExtraSettingsAPI_SetComboboxSelectedIndex("directionOrigin", (int)VRConfigs.MoveDirectionOrigin);

        ExtraSettingsAPI_SaveSettings();
    }

    public void ExtraSettingsAPI_ButtonPress(string name)
    {
        if (name == "calibrate") VRConfigs.ShowCalibrateCanvas();
    }

    public bool ExtraSettingsAPI_GetCheckboxState(string SettingName) => new bool();

    public static void ExtraSettingsAPI_SetCheckboxState(string SettingName, bool value) { }

    public float ExtraSettingsAPI_GetSliderValue(string SettingName) => 0;

    public int ExtraSettingsAPI_GetComboboxSelectedIndex(string SettingName) => 0;

    public static void ExtraSettingsAPI_SetComboboxSelectedIndex(string SettingName, int value) { }

    public static string ExtraSettingsAPI_GetDataValue(string SettingName, string subname) => "";

    public static void ExtraSettingsAPI_SetDataValue(string SettingName, string subname, string value) { }

    public static void ExtraSettingsAPI_SaveSettings() { }
}
