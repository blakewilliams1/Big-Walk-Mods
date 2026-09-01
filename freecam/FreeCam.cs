using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;

namespace FreeCamModLibrary {
    [BepInPlugin("com.blake.bigwalk.FreeCamMod", "Free Cam Mod", "1.0.0")]
    public class FreeCamModPlugin : BasePlugin {
        public static ManualLogSource ModLogger;

        // Loads all and applies the parts of this mod from below.
        public override void Load() {
            ModLogger = Log;
            ModLogger.LogInfo($"Plugin FreeCam mod is loaded!");

            // 1. Register the custom MonoBehaviour type with the IL2CPP runtime
            ClassInjector.RegisterTypeInIl2Cpp<FreeCamController>();
            AddComponent<FreeCamController>();


            Harmony harmony = new Harmony("com.blake.bigwalk.FreeCamMod");
            harmony.PatchAll();
        }
    }

public class FreeCamController : MonoBehaviour {
        public float moveSpeed = 10f;
        public float fastMoveMultiplier = 2.5f;
        public float mouseSensitivity = 3f;
        private bool isFreeCamActive = false;
        private Transform mainCamTransform;
        private Transform originalParent;
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private float yaw = 0f;
        private float pitch = 0f;

        public FreeCamController(System.IntPtr ptr) : base(ptr) { }

        private void Update() {
            // Toggle Free Cam when 'C' is pressed
            if (Input.GetKeyDown(KeyCode.C)) {
                ToggleFreeCam();
            }

            if (isFreeCamActive) {
                HandleFreeCamMovement();
            }
        }

        private void ToggleFreeCam() {
            if (mainCamTransform == null) {
                if (Camera.main != null) {
                    mainCamTransform = Camera.main.transform;
                } else {
                    FreeCamModPlugin.ModLogger.LogError("Main camera could not be found when toggling freecam mode");
                    return;
                }
            }

            isFreeCamActive = !isFreeCamActive;

            if (isFreeCamActive) {
                // Save original camera transform state relative to player
                originalParent = mainCamTransform.parent;
                originalLocalPosition = mainCamTransform.localPosition;
                originalLocalRotation = mainCamTransform.localRotation;

                // Initialize freecam angles to current rotation
                Vector3 currentAngles = mainCamTransform.eulerAngles;
                yaw = currentAngles.y;
                pitch = currentAngles.x;

                // Detach camera from player parent so it moves independently
                mainCamTransform.SetParent(null);
            } else {
                // Snap camera back to player character original relative position & rotation
                mainCamTransform.SetParent(originalParent);
                mainCamTransform.localPosition = originalLocalPosition;
                mainCamTransform.localRotation = originalLocalRotation;
            }
        }

        private void HandleFreeCamMovement() {
            // Stop camera movement and mouse look if any menu (Esc menu, chat, etc.) has unlocked or shown the cursor
            if (Cursor.lockState == CursorLockMode.None) {
                return;
            }

            // --- 1. Mouse Look Controls ---
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            mainCamTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) {
                currentSpeed *= fastMoveMultiplier;
            }
            Vector3 moveDirection = Vector3.zero;

            // Checking standard keys for movement. Not integrated with saved key bindings atm.
            if (Input.GetKey(KeyCode.W)) moveDirection += mainCamTransform.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection -= mainCamTransform.forward;
            if (Input.GetKey(KeyCode.A)) moveDirection -= mainCamTransform.right;
            if (Input.GetKey(KeyCode.D)) moveDirection += mainCamTransform.right;
            if (Input.GetKey(KeyCode.Space)) moveDirection += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) moveDirection += Vector3.down;

            // Apply translation over frame time
            mainCamTransform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }
}
