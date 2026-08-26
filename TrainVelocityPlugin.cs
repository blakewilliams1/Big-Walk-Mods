using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System;

// Some important learnings:
// 1: acceleration property is different than deceleration. Default acceleration is 0.2f and default deceleration is 0.2f
// 2: fullSpeed variable is the maximum speed the train should go to. Default value is 3.0f
// 3: targetSpeed seems to be the current speed and setting it to a fixed value prevents the train from braking/stopping.
// 4: The chairlift is treated as a train because in practice it's the same thing: A vehicle to hold players and be led along a pre-determined route.
namespace TrainVelocityLibrary {
    [BepInPlugin("com.blake.bigwalk.trainvelocity", "Train Velocity Mod", "1.0.0")]
    public class TrainVelocityPlugin : BasePlugin {
        public static ManualLogSource ModLogger;

        public override void Load() {
            ModLogger = Log;

            // On game load, use BepinEx to patch in the NetworkedTrainFixedUpdatePatch as defined below.
            try {
                Harmony harmony = new Harmony("com.blake.bigwalk.trainvelocity");
                var fixedUpdateMethod = AccessTools.Method(typeof(NetworkedTrain), nameof(NetworkedTrain.FixedUpdate));
                if (fixedUpdateMethod != null) {
                    harmony.Patch(fixedUpdateMethod, prefix: new HarmonyMethod(typeof(NetworkedTrainFixedUpdatePatch), nameof(NetworkedTrainFixedUpdatePatch.Prefix)));
                    ModLogger.LogInfo("[TrainVelocityPlugin] Successfully patched FixedUpdate!");
                } else {
                    ModLogger.LogError("Couldn't find FixedUpdate() method for NetworkedTrain class");
                }
            } catch (Exception ex) {
                ModLogger.LogError($"[TrainVelocityPlugin] Patching FixedUpdate() method for NetworkedTrain failed: {ex}");
            }
        }
    }

    public static class NetworkedTrainFixedUpdatePatch {
        [HarmonyPrefix]
        public static void Prefix(NetworkedTrain __instance) {
            TrainVelocityPlugin.ModLogger
                    .LogInfo($"[TrainMod] get_fullSpeed original: {__instance.fullSpeed}, accel original: {__instance.acceleration}, decel original: {__instance.deceleration}, has cable: {__instance.hasCable}");

            // If the 'train' has a cable, it's actually the chairlift system.
            if (__instance.hasCable) {
                return;
            }

            // Although it would be preferable to modify returned values of NetworkedTrain.getAcceleration() or
            // NetworkedTrain.getFullSpeed(), Big Walk is made with Unity Il2CPP instead of Mono and that seems to interfere
            // with things somehow.
            __instance.acceleration = 0.4f;
            __instance.deceleration = 0.7f;
            __instance.fullSpeed = 6.0f;
        }
    }
}