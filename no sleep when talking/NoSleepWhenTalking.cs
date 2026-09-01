using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace NoSleepWhenTalkingLibrary {
    [BepInPlugin("com.blake.bigwalk.nosleepwhentalking", "No Sleep When Talking Mod", "1.0.0")]
    public class NoSleepWhenTalkingPlugin : BasePlugin {
        public static ManualLogSource ModLogger;

        // Loads all and applies the parts of this mod from below.
        public override void Load() {
            ModLogger = Log;
            ModLogger.LogInfo($"Plugin NoSleepWhenTalking mod is loaded!");
            Harmony harmony = new Harmony("com.blake.bigwalk.NoSleepWhenTalking");
            harmony.PatchAll();
        }
    }

    // This is a helper class to deduplicate code dealing with throttling how often these talking checkers are actually ran.
    public static class Throttle {
        public static bool Ready(ref DateTime lastExecution, double intervalMs = 200) {
            DateTime now = DateTime.UtcNow;
            if ((now - lastExecution).TotalMilliseconds < intervalMs) return false;
            lastExecution = now;
            return true;
        }
    }
    
    // Container class to be a value of PlayerRegistry map.
    public class PlayerData {
        public PlayerCharacter character { get; set; }
        public PlayerLips lips { get; set; }
        public PlayerSleeper sleeper { get; set; }
    }

    // A static structure to globally hold a mapping of PlayerCharacter to associated PlayerLips and PlayerSleeper instances.
    public static class PlayerRegistry {
        // Map keyed by PlayerCharacter Instance ID to really ensure no issues with object equality comparison operations.
        private static readonly Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

        public static PlayerData GetOrCreate(PlayerCharacter _character) {
            int key = _character.GetInstanceID();
            if (!players.TryGetValue(key, out var data)) {
                data = new PlayerData { character = _character };
                players[key] = data;
            }
            return data;
        }

        public static IEnumerable<PlayerData> GetAllPlayers() {
            return players.Values;
        }
    }

    [HarmonyPatch(typeof(PlayerSleeper), nameof(PlayerSleeper.Update))]
    public static class PlayerSleeperUpdatePatch {
        private static DateTime lastRunTime = DateTime.MinValue;

        [HarmonyPostfix]
        public static void Postfix(PlayerSleeper __instance) {
            if (!Throttle.Ready(ref lastRunTime)) {
                return;
            }

            if (__instance.playerCharacter == null) {
                return;
            }

            PlayerData data = PlayerRegistry.GetOrCreate(__instance.playerCharacter);
            if (data.sleeper == __instance) {
                return;
            }

            data.sleeper = __instance;
            NoSleepWhenTalkingPlugin.ModLogger.LogDebug(
                $"[NoSleepMod] Dynamically registered PlayerSleeper to '{__instance.playerCharacter.name}'");
        }
    }

    // This is where the important stuff happens. Assuming the map of player lips and sleepers to characters are all in place,
    // this is where the sleep timeout resets when it detects you talking.
    [HarmonyPatch(typeof(PlayerLips), nameof(PlayerLips.Update))]
    public static class PlayerLipsUpdatePatch {
        private static DateTime lastRunTime = DateTime.MinValue;

        [HarmonyPostfix]
        public static void Postfix(PlayerLips __instance) {
            if (!Throttle.Ready(ref lastRunTime)) {
                return;
            }

            if (__instance == null || __instance.playerCharacter == null) {
                return;
            }

            PlayerData data = PlayerRegistry.GetOrCreate(__instance.playerCharacter);
            if (data.lips != __instance) {
                data.lips = __instance;
                NoSleepWhenTalkingPlugin.ModLogger.LogDebug(
                    $"[NoSleepMod] Dynamically registered PlayerLips to '{__instance.playerCharacter.name}'");
            }

            // Check if this player's voice volume exceeds the talking threshold.
            if (__instance.amplitude < 0.95f) {
                return;
            }

            // The player is talking; find the corresponding PlayerSleeper and let it know a wakeful action is happening.
            data.sleeper.RecordAction();
        }
    }
}
