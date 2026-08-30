using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Il2CppInterop.Runtime;

// Some important learnings:
// 1: acceleration property is different than deceleration. Default acceleration is 0.2f and default deceleration is 0.2f
// 2: fullSpeed variable is the maximum speed the train should go to. Default value is 3.0f
// 3: targetSpeed seems to be the current speed and setting it to a fixed value prevents the train from braking/stopping.
// 4: The chairlift is treated as a train because in practice it's the same thing: A vehicle to hold players and be led along a pre-determined route.
namespace NoSleepWhenTalkingLibrary {
    [BepInPlugin("com.blake.bigwalk.nosleepwhentalking", "No Sleep When Talking Mod", "1.0.0")]
    public class NoSleepWhenTalkingPlugin : BasePlugin {
        public static ManualLogSource ModLogger;

        public override void Load() {
            ModLogger = Log;

            // On game load, use BepinEx to patch in the PlayerLipsFixedUpdatePatch as defined below.
            Harmony harmony = new Harmony("com.blake.bigwalk.NoSleepWhenTalking");
            try {
                var fixedUpdateMethod = AccessTools.Method(typeof(PlayerLips), nameof(PlayerLips.Update));
                if (fixedUpdateMethod != null) {
                    harmony.Patch(fixedUpdateMethod, prefix: new HarmonyMethod(typeof(PlayerLipsFixedUpdatePatch), nameof(PlayerLipsFixedUpdatePatch.Prefix)));
                    ModLogger.LogInfo("[NoSleepWhenTalkingPlugin] Successfully patched FixedUpdate!");
                } else {
                    ModLogger.LogError("Couldn't find FixedUpdate() method for PlayerLips class");
                }
            } catch (Exception ex) {
                ModLogger.LogError($"[NoSleepWhenTalkingPlugin] Patching FixedUpdate() method for PlayerLips failed: {ex}");
            }
        }
    }

    public static class PlayerLipsFixedUpdatePatch {
        // Stores the UTC timestamp of the last logged execution
        private static DateTime lastLogTime = DateTime.MinValue;
        private static readonly TimeSpan checkIntervalMs = TimeSpan.FromMilliseconds(200.0);

        [HarmonyPrefix]
        public static void Prefix(PlayerLips __instance) {
            DateTime now = DateTime.UtcNow;

            if ((now - lastLogTime) < checkIntervalMs) {
                return;
            }

            // Update the timestamp to current UTC time
            lastLogTime = now;
            bool isTalking = __instance.amplitude > 0.95f;
            NoSleepWhenTalkingPlugin.ModLogger.LogInfo($"[NoSleepTalkingMod] isTalking: {isTalking}");

            /*GameObject asdf = __instance.transform.root;
            if (!isTalking) {
                return;
            }

            PlayerSleeper sleeper = __instance.transform.root;//.gameObject.GetComponent<PlayerSleeper>();
            if (sleeper == null) {
                NoSleepWhenTalkingPlugin.ModLogger.LogInfo("[NoSleepTalkingMod] Couldn't find PlayerSleeper on __instance");
                return;
            }

            NoSleepWhenTalkingPlugin.ModLogger.LogInfo("[NoSleepTalkingMod] timeTillSleep: ${sleeper.timeTilSleep}");*/
        }
    }
}
