using System;
using System.Collections.Generic;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using DistractorTask.UserStudy.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace DistractorTask.UserStudy.DistractorSelectionStage
{
    public class DistractorTaskManager : SendingStudyStageComponent<DistractorSelectionStageEvent>
    {
        [FormerlySerializedAs("trials")] public Study[] studies = Array.Empty<Study>();

        [SerializeField] private int studyToStart = 0;
        
        [SerializeField, Tooltip("Determines the type of participant / order of conditions for the given study")]
        private int startingCondition = 0;
        
        
        private int _tempVideoClipData = 0;
        private StudyConditionsEnumerator _studyConditionsEnumerator;


        public override void StartStudy(INetworkManager manager)
        {
            _studyConditionsEnumerator = new StudyConditionsEnumerator(studies[studyToStart], startingCondition);
            Manager.RegisterCallback<ConfirmationData>(OnStudyBegin, NetworkExtensions.DefaultPort);
            base.StartStudy(manager);
        }

        private void OnStudyBegin(ConfirmationData data, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            Manager.UnregisterCallback<ConfirmationData>(OnStudyBegin, NetworkExtensions.DefaultPort);
            Manager.RegisterCallback<TrialCompletedData>(OnTrialCompleted, NetworkExtensions.DefaultPort);
            if (_studyConditionsEnumerator.MoveNext())
            {
                StartTrial();
            }
            
        }

        private async void PerformAwaitableStudy()
        {
            _studyConditionsEnumerator = new StudyConditionsEnumerator(studies[studyToStart], startingCondition);
            
        }

        private async void IterateAwaitableConditions()
        {
            //todo maybe implement it this way?
            while (_studyConditionsEnumerator.MoveNext())
            {
                //await NetworkManagerExtensions.MulticastMessageAndAwaitResponse<>()
            }
        }

        private void StartTrial()
        {
            var trial = _studyConditionsEnumerator.Current;
            var positions = new int[]
            {
                0, 1, 2, 3, 4, 5
            };
            Manager.BroadcastMessage(new VideoClipChangeData
            {
                videoClipIndex = _tempVideoClipData
            }, GetInstanceID());
            _tempVideoClipData = (_tempVideoClipData + 1) % 2;
            
            Manager.BroadcastMessage(new DistractorSelectionTrialData
            {
                loadLevel = (byte)trial.LoadLevel,
                noiseLevel = (byte)trial.NoiseLevel,
                trialCount = trial.TrialCount,
                repetitionsPerTrial = trial.RepetitionsPerTrial,
                markers = positions
            }, GetInstanceID());
        }

        private void OnTrialCompleted(TrialCompletedData obj, int callerId)
        {
            if (callerId == GetInstanceID())
            {
                return;
            }
            if (_studyConditionsEnumerator.MoveNext())
            {
                StartTrial();
            }
            EndStudy(Manager);
            //todo start next study. Currently it will just end the entire process. Might require a small redesign of study setup / study should not end here but only if all studies are performed / operator ends study
        }
        
        public override void EndStudy(INetworkManager manager)
        {
            Manager.UnregisterCallback<TrialCompletedData>(OnTrialCompleted, NetworkExtensions.DefaultPort);
            base.EndStudy(manager);
        }
        
    }


    [Serializable]
    public struct Study
    {
        [Tooltip("Trial consists of these many selections. ")]
        public int selectionsPerTrial;
        public int trialsPerCondition;
        public Condition conditions;

    }
}