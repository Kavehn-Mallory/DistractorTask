using System;
using System.Collections;
using DistractorTask.Logging;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.UserStudy.Core
{
    public abstract class UserStudyManager : MonoBehaviour
    {
        public StudyStageComponent[] studyStages = Array.Empty<StudyStageComponent>();

        private int _studyIndex = -1;

        protected float SecondsToWait = 5f;

        public abstract INetworkManager Manager { get; }
        
        protected virtual IEnumerator Start()
        {
            
            //yield return new WaitForSeconds(SecondsToWait);
            Manager.RegisterCallback<RequestStudyBeginData>(OnStudyBeginRequest);
            Manager.RegisterCallback<UserStudyBeginData>(OnStudyBegin);
            foreach (var studyStage in studyStages)
            {
                studyStage.Manager = Manager;
                if (studyStage is ReceivingStudyStageComponent receiver)
                {
                    receiver.RegisterStudyComponent(Manager);
                }

                studyStage.OnStudyEnd += OnStudyStageEnds;
            }

            if (studyStages.Length <= 0)
            {
                yield return null;
            }
            
        }

        public virtual void OnStudyBeginRequest(RequestStudyBeginData obj)
        {
            Debug.Log("Received on Study Begin Data", this);
            Manager.UnregisterCallback<RequestStudyBeginData>(OnStudyBeginRequest);
            StartStudy();
        }


        [ContextMenu("Start Study")]
        public void StartStudy()
        {
            if (studyStages[0] is ReceivingStudyStageComponent)
            {
                _studyIndex++;
                Debug.Log($"{this.GetType()} is transmitting UserStudyBeginData for first study stage",this);
                Manager.BroadcastMessage(new UserStudyBeginData
                {
                    studyIndex = _studyIndex
                });
            }
            else
            {
                Debug.Log($"{this.GetType()} is sending study start request", this);
                Manager.BroadcastMessage(new RequestStudyBeginData());
            }
        }


        private void OnStudyBegin(UserStudyBeginData obj)
        {
            if (obj.studyIndex <= _studyIndex)
            {
                Debug.Log($"Index: {obj.studyIndex} vs {_studyIndex}", this);
                throw new ArgumentException($"The given study index {obj.studyIndex} is smaller or equal to the current index {_studyIndex}. This study was started already");
            }

            _studyIndex = obj.studyIndex;
            if (studyStages[_studyIndex] is not SendingStudyStageComponent sender)
            {
                throw new Exception("The selected study is a receiver not a sender, something went wrong");
            }
            Debug.Log($"Starting next sending study {_studyIndex}", this);
            sender.StartStudy(Manager);
        }
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void OnStudyStageEnds()
        {
            var study = studyStages[_studyIndex];
            if (study is ReceivingStudyStageComponent receiver)
            {
                receiver.UnregisterStudyComponent(Manager);
            }
            if (_studyIndex + 1 < studyStages.Length && studyStages[_studyIndex + 1] is ReceivingStudyStageComponent)
            {
                _studyIndex++;
                Debug.Log($"Starting next receiver study {_studyIndex}", this);
                Manager.BroadcastMessage(new UserStudyBeginData
                {
                    studyIndex = _studyIndex
                });
            }
        }

    }
}