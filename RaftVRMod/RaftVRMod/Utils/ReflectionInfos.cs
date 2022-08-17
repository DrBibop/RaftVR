using System;
using System.Reflection;

namespace RaftVR.Utils
{
    static class ReflectionInfos
    {
        internal static FieldInfo chargeMeterCurrentChargeField = GetFieldInfo(typeof(ChargeMeter), "currentCharge");
        internal static FieldInfo chargeMeterMinChargeField = GetFieldInfo(typeof(ChargeMeter), "minCharge");
        internal static FieldInfo chargeMeterMaxChargeField = GetFieldInfo(typeof(ChargeMeter), "maxCharge");

        internal static FieldInfo displayTextPriorityField = GetFieldInfo(typeof(DisplayText), "currentPriority");
        internal static FieldInfo displayTextButtonTextField = GetFieldInfo(typeof(DisplayText), "buttonText");

        internal static FieldInfo throwableRotationField = GetFieldInfo(typeof(Throwable), "throwableStartRotation");

        internal static FieldInfo animationConnectionsField = GetFieldInfo(typeof(AnimationEventCaller), "connections");

        internal static FieldInfo toolOnPressUseEventField = GetFieldInfo(typeof(UsableTool), "OnPressUseButton");
        internal static FieldInfo toolOnReleaseUseEventField = GetFieldInfo(typeof(UsableTool), "OnReleaseUseButton");
        internal static FieldInfo toolThisItemField = GetFieldInfo(typeof(UsableTool), "thisItem");
        internal static FieldInfo toolSetAnimationField = GetFieldInfo(typeof(UsableTool), "setItemHitAnimation");

        internal static FieldInfo weaponDamageField = GetFieldInfo(typeof(MeleeWeapon), "damage");
        internal static FieldInfo weaponGoThroughInvurnabilityField = GetFieldInfo(typeof(MeleeWeapon), "goThroughInvurnability");

        internal static FieldInfo usableUseAnimationField = GetFieldInfo(typeof(ItemInstance_Usable), "animationOnUse");

        internal static FieldInfo netPickupTargetField = GetFieldInfo(typeof(SweepNet), "currentPickupTarget");
        internal static FieldInfo netSwingEventField = GetFieldInfo(typeof(SweepNet), "eventRef_netSwing");

        internal static FieldInfo itemCanChannelField = GetFieldInfo(typeof(UseableItem), "canChannel");

        internal static FieldInfo shovelCurrentTargetField = GetFieldInfo(typeof(Shovel), "currentTarget");

        internal static FieldInfo hookGatherTimerField = GetFieldInfo(typeof(Hook), "gatherTimer");
        internal static FieldInfo hookGatherEmitterMethod = GetFieldInfo(typeof(Hook), "eventEmitter_gather");

        internal static FieldInfo optionsMenuSettingsField = GetFieldInfo(typeof(OptionsMenuBox), "settings");

        internal static FieldInfo personControllerNetworkPlayerField = GetFieldInfo(typeof(PersonController), "playerNetwork");

        internal static MethodInfo usableItemUse = GetMethodInfo(typeof(UseItemController), "Use");

        internal static MethodInfo netAttemptCaptureMethod = GetMethodInfo(typeof(SweepNet), "AttemptCaptureWithNet");
        internal static MethodInfo netPlayCatureSoundMethod = GetMethodInfo(typeof(SweepNet), "PlaySuccessfullCaptureSound");

        internal static MethodInfo shovelResetMethod = GetMethodInfo(typeof(Shovel), "ResetItemChannel");

        internal static MethodInfo hookStartCollectingMethod = GetMethodInfo(typeof(Hook), "StartCollecting");
        internal static MethodInfo hookStopCollectingMethod = GetMethodInfo(typeof(Hook), "StopCollecting");
        internal static MethodInfo hookFinishGatheringMethod = GetMethodInfo(typeof(Hook), "FinishGathering");

        private static FieldInfo GetFieldInfo(Type classType, string fieldName)
        {
            return classType.GetField(fieldName, (BindingFlags)(-1));
        }

        private static MethodInfo GetMethodInfo(Type classType, string methodName, params Type[] parameterTypes)
        {
            return classType.GetMethod(methodName, (BindingFlags)(-1));
        }
    }
}
