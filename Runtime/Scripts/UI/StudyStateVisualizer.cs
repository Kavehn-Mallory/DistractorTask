﻿using DistractorTask.UserStudy.DataDrivenSetup;
using TMPro;
using UnityEngine;

namespace DistractorTask.UI
{
    public class StudyStateVisualizer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI studyStageText;
        
        [SerializeField]
        private TextMeshProUGUI iterationCountText;

        [SerializeField]
        private ControlPanel controlPanel;

        private void Start()
        {
            controlPanel.OnStudyPhaseStart += SetStudyStageText;
            controlPanel.OnNextIteration += SetIterationText;
            controlPanel.OnStudyPhaseEnd += SetStudyStageEndText;
        }

        private void SetStudyStageEndText(string studyStageName)
        {
            studyStageText.text = $"{studyStageName} is completed";
        }


        private void SetStudyStageText(string studyStageName)
        {
            studyStageText.text = studyStageName;
        }

        private void SetIterationText(string _, int iteration, int iterationCount)
        {
            iterationCountText.text = $"{iteration + 1} / {iterationCount}";
        }
    }
}