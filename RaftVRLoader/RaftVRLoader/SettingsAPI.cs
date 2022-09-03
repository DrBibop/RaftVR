using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

public class SettingsAPI
{
    public bool ExtraSettingsAPI_Loaded = false;

    private Component initializerInstance;

    private MethodInfo onLoadMethod;
    private MethodInfo snapTurnSetter;
    private MethodInfo snapTurnAngleSetter;
    private MethodInfo smoothTurnSpeedSetter;
    private MethodInfo seatedModeSetter;
    private MethodInfo interactionRaySetter;
    private MethodInfo playspaceCenterSetter;
    private MethodInfo armScaleSetter;
    private MethodInfo showCalibrateCanvasMethod;
    private MethodInfo runtimeGetter;
    private MethodInfo writeRuntimeMethod;
    private MethodInfo setDirectionOriginMethod;
    private MethodInfo setRadialHotbarModeMethod;
    private MethodInfo waterDistortionSetter;

    public SettingsAPI(Component initializerInstance, Type initializerType, Type configsType)
    {
        this.initializerInstance = initializerInstance;

        onLoadMethod = initializerType.GetMethod("OnSettingsAPILoaded", (BindingFlags)(-1));
        snapTurnSetter = configsType.GetProperty("SnapTurn", (BindingFlags)(-1)).GetSetMethod();
        snapTurnAngleSetter = configsType.GetProperty("SnapTurnAngle", (BindingFlags)(-1)).GetSetMethod();
        smoothTurnSpeedSetter = configsType.GetProperty("SmoothTurnSpeed", (BindingFlags)(-1)).GetSetMethod();
        seatedModeSetter = configsType.GetProperty("SeatedMode", (BindingFlags)(-1)).GetSetMethod();
        interactionRaySetter = configsType.GetProperty("ShowInteractionRay", (BindingFlags)(-1)).GetSetMethod();
        playspaceCenterSetter = configsType.GetMethod("SetShowPlayspaceCenter", (BindingFlags)(-1));
        armScaleSetter = configsType.GetProperty("ArmScale", (BindingFlags)(-1)).GetSetMethod();
        showCalibrateCanvasMethod = configsType.GetMethod("ShowCalibrateCanvas", (BindingFlags)(-1));
        runtimeGetter = configsType.GetProperty("Runtime", (BindingFlags)(-1)).GetGetMethod();
        writeRuntimeMethod = configsType.GetMethod("WriteRuntimeToFile", (BindingFlags)(-1));
        setDirectionOriginMethod = configsType.GetMethod("SetMoveDirectionOrigin", (BindingFlags)(-1));
        setRadialHotbarModeMethod = configsType.GetMethod("SetRadialHotbarMode", (BindingFlags)(-1));
        waterDistortionSetter = configsType.GetProperty("UnderwaterDistortion", (BindingFlags)(-1)).GetSetMethod();
    }

    public void ExtraSettingsAPI_Load()
    {
        onLoadMethod.Invoke(initializerInstance, null);
        RefreshSettings(false);
        string armScaleString = ExtraSettingsAPI_GetDataValue("hiddenSettings", "armScale");

        if (float.TryParse(armScaleString, NumberStyles.Any, CultureInfo.InvariantCulture, out float armScale))
            armScaleSetter.Invoke(null, new object[] { armScale });

        int runtimeIndex = (int)runtimeGetter.Invoke(null, null);

        if (runtimeIndex != 0)
            ExtraSettingsAPI_SetComboboxSelectedIndex("runtime", runtimeIndex - 1);
    }

    public void ExtraSettingsAPI_SettingsClose()
    {
        RefreshSettings();
    }

    private void RefreshSettings(bool writeRuntime = true)
    {
        snapTurnSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("snapTurn") });
        snapTurnAngleSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetSliderValue("snapTurnAngle") });
        smoothTurnSpeedSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetSliderValue("smoothTurnSpeed") });
        seatedModeSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("seatedMode") });
        interactionRaySetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("interactionRay") });
        playspaceCenterSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetComboboxSelectedIndex("playspaceCenter") });
        setDirectionOriginMethod.Invoke(null, new object[] { ExtraSettingsAPI_GetComboboxSelectedIndex("directionOrigin") });
        setRadialHotbarModeMethod.Invoke(null, new object[] { ExtraSettingsAPI_GetComboboxSelectedIndex("useRadialHotbar") });
        waterDistortionSetter.Invoke(null, new object[] { ExtraSettingsAPI_GetCheckboxState("underwaterDistortion") });

        if (writeRuntime)
            writeRuntimeMethod.Invoke(null, new object[] { ExtraSettingsAPI_GetComboboxSelectedIndex("runtime") + 1 });
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

    public static void ExtraSettingsAPI_SetComboboxSelectedIndex(string SettingName, int value) { }

    public static string ExtraSettingsAPI_GetDataValue(string SettingName, string subname) => "";

    public static void ExtraSettingsAPI_SetDataValue(string SettingName, string subname, string value) { }
}