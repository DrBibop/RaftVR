using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

// This had to be moved in this assembly due to some issues
public class SettingsAPI
{
    public bool ExtraSettingsAPI_Loaded = false;

    private Component initializerInstance;

    private MethodInfo onLoadMethod;
    private MethodInfo snapTurnSetter;
    private MethodInfo snapTurnAngleSetter;
    private MethodInfo smoothTurnSpeedSetter;
    private MethodInfo seatedModeSetter;
    private MethodInfo playspaceCenterSetter;
    private MethodInfo armScaleSetter;
    private MethodInfo showCalibrateCanvasMethod;

    public SettingsAPI(Component initializerInstance, Type initializerType, Type configsType)
    {
        this.initializerInstance = initializerInstance;

        onLoadMethod = initializerType.GetMethod("OnSettingsAPILoaded", (BindingFlags)(-1));
        snapTurnSetter = configsType.GetProperty("SnapTurn", (BindingFlags)(-1)).GetSetMethod(true);
        snapTurnAngleSetter = configsType.GetProperty("SnapTurnAngle", (BindingFlags)(-1)).GetSetMethod(true);
        smoothTurnSpeedSetter = configsType.GetProperty("SmoothTurnSpeed", (BindingFlags)(-1)).GetSetMethod(true);
        seatedModeSetter = configsType.GetProperty("SeatedMode", (BindingFlags)(-1)).GetSetMethod(true);
        playspaceCenterSetter = configsType.GetMethod("SetShowPlayspaceCenter", (BindingFlags)(-1));
        armScaleSetter = configsType.GetProperty("ArmScale", (BindingFlags)(-1)).GetSetMethod(true);
        showCalibrateCanvasMethod = configsType.GetMethod("ShowCalibrateCanvas", (BindingFlags)(-1));
    }

    public void ExtraSettingsAPI_Load()
    {
        onLoadMethod.Invoke(initializerInstance, null);
        RefreshSettings();
        string armScaleString = ExtraSettingsAPI_GetDataValue("hiddenSettings", "armScale");

        if (float.TryParse(armScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float armScale))
        {
            armScaleSetter.Invoke(null, new object[] { armScale });
        }
    }

    public void ExtraSettingsAPI_SettingsClose()
    {
        RefreshSettings();
    }

    private void RefreshSettings()
    {
        snapTurnSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("snapTurn") });
        snapTurnAngleSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetSliderValue("snapTurnAngle") });
        smoothTurnSpeedSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetSliderValue("smoothTurnSpeed") });
        seatedModeSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("seatedMode") });
        playspaceCenterSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetComboboxSelectedIndex("playspaceCenter") });
    }

    public void ExtraSettingsAPI_ButtonPress(string name)
    {
        if (name == "calibrate") showCalibrateCanvasMethod.Invoke(null, null);
    }

    internal void RefreshHiddenSettings(float armScale)
    {
        ExtraSettingsAPI_SetDataValue("hiddenSettings", "armScale", armScale.ToString(CultureInfo.InvariantCulture));
    }

    public bool ExtraSettingsAPI_GetCheckboxState(string SettingName) => new bool();

    public float ExtraSettingsAPI_GetSliderValue(string SettingName) => 0;

    public int ExtraSettingsAPI_GetComboboxSelectedIndex(string SettingName) => 0;

    public static string ExtraSettingsAPI_GetDataValue(string SettingName, string subname) => "";

    public static void ExtraSettingsAPI_SetDataValue(string SettingName, string subname, string value) { }
}
