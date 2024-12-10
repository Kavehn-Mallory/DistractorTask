using System;
using System.Collections;
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

            //todo remove this and replace with button or something like that 
#if UNITY_EDITOR
            /*if (Manager is Client client)
            {
                client.Connect();
            }
            StartStudy();*/
#endif
        }

        public virtual void OnStudyBeginRequest(RequestStudyBeginData obj)
        {
            Manager.UnregisterCallback<RequestStudyBeginData>(OnStudyBeginRequest);
            StartStudy();
        }


        [ContextMenu("Start Study")]
        public void StartStudy()
        {
            if (studyStages[0] is ReceivingStudyStageComponent)
            {
                _studyIndex++;
                Manager.TransmitNetworkMessage(new UserStudyBeginData
                {
                    studyIndex = _studyIndex
                });
            }
            else
            {
                Debug.Log("Sending study start request");
                Manager.TransmitNetworkMessage(new RequestStudyBeginData());
            }
        }


        private void OnStudyBegin(UserStudyBeginData obj)
        {
            if (obj.studyIndex <= _studyIndex)
            {
                throw new ArgumentException($"The given study index {obj.studyIndex} is smaller or equal to the current index {_studyIndex}. This study was started already");
            }

            _studyIndex = obj.studyIndex;
            if (studyStages[_studyIndex] is not SendingStudyStageComponent sender)
            {
                throw new Exception("The selected study is a receiver not a sender, something went wrong");
            }
            Debug.Log($"Starting next sending study {_studyIndex}");
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
                Debug.Log($"Starting next receiver study {_studyIndex}");
                Manager.TransmitNetworkMessage(new UserStudyBeginData
                {
                    studyIndex = _studyIndex
                });
            }
        }

    }
}