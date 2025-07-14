using System;
using DistractorTask.UserStudy;
using DistractorTask.UserStudy.Core;
using DistractorTask.UserStudy.DataDrivenSetup;
using DistractorTask.UserStudy.DistractorSelectionStage.DistractorComponents;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace DistractorTask.VideoPlayer
{
    public class VideoPlayerDebugger : MonoBehaviour
    {

        [SerializeField]
        private VideoPlayerController videoPlayerController;
        
        private Keyboard _keyboard;
        
        
        private void Start()
        {
            _keyboard = InputSystem.GetDevice<Keyboard>();

            if (_keyboard == null || !videoPlayerController)
            {
                this.enabled = false;
            }
            
        }

        private void Update()
        {
            if (_keyboard.digit0Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.None, 1, 1, false, false)
                });
            }
            if (_keyboard.digit1Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.Low, 1, 1, false, false)
                });
            }
            if (_keyboard.digit2Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.High, 1, 1, false, false)
                });
            }
            if (_keyboard.digit3Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.Audio, 1, 1, false, false)
                });
            }
            if (_keyboard.digit4Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.Visual, 1, 1, false, false)
                });
            }
            if (_keyboard.digit5Key.wasPressedThisFrame)
            {
                videoPlayerController.DebugSwitchVideoClip(new StudyConditionVideoInfoData
                {
                    studyCondition = new StudyCondition(LoadLevel.Low, NoiseLevel.Max, 1, 1, false, false)
                });
            }
        }
    }
}