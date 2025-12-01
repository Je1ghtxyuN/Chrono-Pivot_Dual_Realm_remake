using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using UnityEngine.InputSystem;

namespace RootMotion.Demos
{
    public class VRIKCalibrationController : MonoBehaviour
    {
        [Tooltip("Reference to the VRIK component on the avatar.")] public VRIK ik;
        [Tooltip("The settings for VRIK calibration.")] public VRIKCalibrator.Settings settings;
        [Tooltip("The HMD.")] public Transform headTracker;
        [Tooltip("(Optional) A tracker placed anywhere on the body of the player, preferrably close to the pelvis, on the belt area.")] public Transform bodyTracker;
        [Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's left hand.")] public Transform leftHandTracker;
        [Tooltip("(Optional) A tracker or hand controller device placed anywhere on or in the player's right hand.")] public Transform rightHandTracker;
        [Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's left leg.")] public Transform leftFootTracker;
        [Tooltip("(Optional) A tracker placed anywhere on the ankle or toes of the player's right leg.")] public Transform rightFootTracker;

        [Header("Data stored by Calibration")]
        public VRIKCalibrator.CalibrationData data = new VRIKCalibrator.CalibrationData();

        // 添加输入动作引用
        public InputActionReference calibrateAction;
        public InputActionReference calibrateWithDataAction;
        public InputActionReference recalibrateScaleAction;

        [Header("Keyboard Fallback (For Testing)")]
        [Tooltip("Keyboard key to trigger calibration (fallback if InputAction not set)")]
        public Key calibrateKey = Key.Space; // 改为 Input System 的 Key 类型
        [Tooltip("Keyboard key to trigger calibration with stored data")]
        public Key calibrateWithDataKey = Key.F; // 改为 Input System 的 Key 类型
        [Tooltip("Keyboard key to recalibrate scale only")]
        public Key recalibrateScaleKey = Key.R; // 改为 Input System 的 Key 类型

        void OnEnable()
        {
            // 启用输入动作
            if (calibrateAction != null) calibrateAction.action.Enable();
            if (calibrateWithDataAction != null) calibrateWithDataAction.action.Enable();
            if (recalibrateScaleAction != null) recalibrateScaleAction.action.Enable();
        }

        void OnDisable()
        {
            // 禁用输入动作
            if (calibrateAction != null) calibrateAction.action.Disable();
            if (calibrateWithDataAction != null) calibrateWithDataAction.action.Disable();
            if (recalibrateScaleAction != null) recalibrateScaleAction.action.Disable();
        }

        void LateUpdate()
        {
            // 检测校准输入 (新Input System + 键盘备选)
            bool calibratePressed = (calibrateAction != null && calibrateAction.action.triggered) ||
                                   (Keyboard.current != null && Keyboard.current[calibrateKey].wasPressedThisFrame);

            // 检测使用数据校准输入
            bool calibrateWithDataPressed = (calibrateWithDataAction != null && calibrateWithDataAction.action.triggered) ||
                                           (Keyboard.current != null && Keyboard.current[calibrateWithDataKey].wasPressedThisFrame);

            // 检测重新校准比例输入
            bool recalibrateScalePressed = (recalibrateScaleAction != null && recalibrateScaleAction.action.triggered) ||
                                          (Keyboard.current != null && Keyboard.current[recalibrateScaleKey].wasPressedThisFrame);

            // 使用新的 Input System 检测按键
            if (calibratePressed)
            {
                // Calibrate the character, store data of the calibration
                data = VRIKCalibrator.Calibrate(ik, settings, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
                Debug.Log("Calibration triggered with key: " + calibrateKey);
            }

            /*
             * calling Calibrate with settings will return a VRIKCalibrator.CalibrationData, which can be used to calibrate that same character again exactly the same in another scene (just pass data instead of settings), 
             * without being dependent on the pose of the player at calibration time.
             * Calibration data still depends on bone orientations though, so the data is valid only for the character that it was calibrated to or characters with identical bone structures.
             * If you wish to use more than one character, it would be best to calibrate them all at once and store the CalibrationData for each one.
             * */
            if (calibrateWithDataPressed)
            {
                if (data.scale == 0f)
                {
                    Debug.LogError("No Calibration Data to calibrate to, please calibrate with settings first.");
                }
                else
                {
                    // Use data from a previous calibration to calibrate that same character again.
                    VRIKCalibrator.Calibrate(ik, data, headTracker, bodyTracker, leftHandTracker, rightHandTracker, leftFootTracker, rightFootTracker);
                    Debug.Log("Calibration with stored data triggered with key: " + calibrateWithDataKey);
                }
            }

            // Recalibrates avatar scale only. Can be called only if the avatar has been calibrated already.
            if (recalibrateScalePressed)
            {
                if (data.scale == 0f)
                {
                    Debug.LogError("Avatar needs to be calibrated before RecalibrateScale is called.");
                }
                else
                {
                    VRIKCalibrator.RecalibrateScale(ik, data, settings);
                    Debug.Log("Recalibrate scale triggered with key: " + recalibrateScaleKey);
                }
            }
        }
    }
}