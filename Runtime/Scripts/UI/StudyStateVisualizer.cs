using DistractorTask.UserStudy.DataDrivenSetup;
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
            controlPanel.OnIterationCompleted += SetIterationText;
            controlPanel.OnStudyPhaseEnd += SetStudyStageEndText;
        }

        private void SetStudyStageEndText(string studyStageName)
        {
            Debug.Log("Received study stage ended");
            studyStageText.text = $"{studyStageName} is completed";
        }


        private void SetStudyStageText(string studyStageName, int studyIndex)
        {
            //Todo I think I broke this. Maybe I should replace this call for the logging system with a direct call to the logging system
            studyStageText.text = studyStageName;
        }

        private void SetIterationText(string _, int iteration, int iterationCount)
        {
            iterationCountText.text = $"Completed {iteration + 1} / {iterationCount}";
        }
    }
}