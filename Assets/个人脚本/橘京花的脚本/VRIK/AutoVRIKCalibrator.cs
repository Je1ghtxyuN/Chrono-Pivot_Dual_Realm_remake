using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos
{
    public class AutoVRIKCalibrator : MonoBehaviour
    {
        [Header("核心组件")]
        [Tooltip("VRIK 组件引用")]
        public VRIK ik;
        [Tooltip("校准设置")]
        public VRIKCalibrator.Settings settings;

        [Header("追踪器 (Trackers)")]
        [Tooltip("头显位置")]
        public Transform headTracker;
        [Tooltip("(可选) 身体追踪器")]
        public Transform bodyTracker;
        [Tooltip("(可选) 左手手柄")]
        public Transform leftHandTracker;
        [Tooltip("(可选) 右手手柄")]
        public Transform rightHandTracker;
        [Tooltip("(可选) 左脚追踪器")]
        public Transform leftFootTracker;
        [Tooltip("(可选) 右脚追踪器")]
        public Transform rightFootTracker;

        [Header("自动校准设置")]
        [Tooltip("是否在游戏开始时自动校准")]
        public bool calibrateOnStart = true;

        [Tooltip("延迟多少秒后进行校准？(建议设置 0.5 到 1.0 秒，等待 VR 设备追踪稳定)")]
        public float startDelay = 0.5f;

        private void Start()
        {
            if (calibrateOnStart)
            {
                StartCoroutine(CalibrateWithDelay());
            }
        }

        private IEnumerator CalibrateWithDelay()
        {
            // 等待指定的秒数，确保 VR 头显和手柄已正确追踪
            yield return new WaitForSeconds(startDelay);

            PerformCalibration();
        }

        /// <summary>
        /// 执行校准逻辑
        /// </summary>
        public void PerformCalibration()
        {
            if (ik == null)
            {
                Debug.LogError("AutoVRIKCalibrator: 未赋值 VRIK 组件，无法校准！");
                return;
            }

            if (headTracker == null)
            {
                Debug.LogError("AutoVRIKCalibrator: 必须赋值 Head Tracker (头显)！");
                return;
            }

            // 调用 FinalIK 提供的静态校准方法
            // 这与 VRIKCalibrationController 中按下空格键调用的逻辑完全一致
            VRIKCalibrator.Calibrate(
                ik,
                settings,
                headTracker,
                bodyTracker,
                leftHandTracker,
                rightHandTracker,
                leftFootTracker,
                rightFootTracker
            );

            Debug.Log("VRIK 自动校准完成！");
        }
    }
}