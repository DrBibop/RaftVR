using HarmonyLib;
using System.Reflection;
using UltimateWater;

namespace RaftVR.Utils
{
    static class ReflectionInfos
    {
        // Fields
        internal static FieldInfo chargeMeterCurrentChargeField = AccessTools.Field(typeof(ChargeMeter), "currentCharge");
        internal static FieldInfo chargeMeterMinChargeField = AccessTools.Field(typeof(ChargeMeter), "minCharge");
        internal static FieldInfo chargeMeterMaxChargeField = AccessTools.Field(typeof(ChargeMeter), "maxCharge");

        internal static FieldInfo displayTextPriorityField = AccessTools.Field(typeof(DisplayText), "currentPriority");
        internal static FieldInfo displayTextButtonTextField = AccessTools.Field(typeof(DisplayText), "buttonText");

        internal static FieldInfo throwableRotationField = AccessTools.Field(typeof(Throwable), "throwableStartRotation");

        internal static FieldInfo animationConnectionsField = AccessTools.Field(typeof(AnimationEventCaller), "connections");

        internal static FieldInfo toolOnPressUseEventField = AccessTools.Field(typeof(UsableTool), "OnPressUseButton");
        internal static FieldInfo toolOnReleaseUseEventField = AccessTools.Field(typeof(UsableTool), "OnReleaseUseButton");
        internal static FieldInfo toolThisItemField = AccessTools.Field(typeof(UsableTool), "thisItem");
        internal static FieldInfo toolSetAnimationField = AccessTools.Field(typeof(UsableTool), "setItemHitAnimation");

        internal static FieldInfo weaponDamageField = AccessTools.Field(typeof(MeleeWeapon), "damage");
        internal static FieldInfo weaponGoThroughInvurnabilityField = AccessTools.Field(typeof(MeleeWeapon), "goThroughInvurnability");

        internal static FieldInfo usableUseAnimationField = AccessTools.Field(typeof(ItemInstance_Usable), "animationOnUse");

        internal static FieldInfo netPickupTargetField = AccessTools.Field(typeof(SweepNet), "currentPickupTarget");
        internal static FieldInfo netSwingEventField = AccessTools.Field(typeof(SweepNet), "eventRef_netSwing");

        internal static FieldInfo itemCanChannelField = AccessTools.Field(typeof(UseableItem), "canChannel");

        internal static FieldInfo shovelCurrentTargetField = AccessTools.Field(typeof(Shovel), "currentTarget");

        internal static FieldInfo hookGatherTimerField = AccessTools.Field(typeof(Hook), "gatherTimer");
        internal static FieldInfo hookGatherEmitterMethod = AccessTools.Field(typeof(Hook), "eventEmitter_gather");

        internal static FieldInfo optionsMenuSettingsField = AccessTools.Field(typeof(OptionsMenuBox), "settings");

        internal static FieldInfo personControllerNetworkPlayerField = AccessTools.Field(typeof(PersonController), "playerNetwork");
        internal static FieldInfo personControllerCamTransformField = AccessTools.Field(typeof(PersonController), "camTransform");

        internal static FieldInfo macheteQuestTagField = AccessTools.Field(typeof(Machete), "macheteInteractTagName");

        internal static FieldInfo storageInventoryRefField = AccessTools.Field(typeof(Storage_Small), "inventoryReference");

        internal static FieldInfo characterModelPlayerNetworkField = AccessTools.Field(typeof(CharacterModelModifications), "playerNetwork");

        internal static FieldInfo throwableCanThrowField = AccessTools.Field(typeof(ThrowableComponent), "canThrow");

        internal static FieldInfo characterModelNetworkPlayerField = AccessTools.Field(typeof(CharacterModelModifications), "playerNetwork");

        internal static FieldInfo equipmentModelNetworkPlayerField = AccessTools.Field(typeof(Equipment_Model), "playerNetwork");

        // Methods
        internal static MethodInfo usableItemUse = AccessTools.Method(typeof(UseItemController), "Use");

        internal static MethodInfo netAttemptCaptureMethod = AccessTools.Method(typeof(SweepNet), "AttemptCaptureWithNet");
        internal static MethodInfo netPlayCatureSoundMethod = AccessTools.Method(typeof(SweepNet), "PlaySuccessfullCaptureSound");

        internal static MethodInfo shovelResetMethod = AccessTools.Method(typeof(Shovel), "ResetItemChannel");

        internal static MethodInfo hookStartCollectingMethod = AccessTools.Method(typeof(Hook), "StartCollecting");
        internal static MethodInfo hookStopCollectingMethod = AccessTools.Method(typeof(Hook), "StopCollecting");
        internal static MethodInfo hookFinishGatheringMethod = AccessTools.Method(typeof(Hook), "FinishGathering");

        internal static MethodInfo macheteQuestInteract = AccessTools.Method(typeof(Machete), "MacheteInteractWithQuest");

        internal static MethodInfo waterDistortionField = AccessTools.PropertySetter(typeof(WaterMaterials), "UnderwaterDistortionsIntensity");

        internal static MethodInfo throwableReleaseHandMethod = AccessTools.Method(typeof(ThrowableComponent), "ReleaseHand");

        internal static MethodInfo ikSolverAnimatorSetMethod = AccessTools.PropertySetter(typeof(RootMotion.FinalIK.IKSolverVR), "animator");
    }
}
