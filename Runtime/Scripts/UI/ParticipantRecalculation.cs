using TMPro;
using UnityEngine;

namespace DistractorTask.UI
{
    public class ParticipantRecalculation : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown participantDropdown;
        [SerializeField]
        private TMP_Dropdown study1Dropdown;

        private void Awake()
        {
            participantDropdown.onValueChanged.AddListener(UpdateStudy1Dropdown);
            Debug.Log($"Option Count: {study1Dropdown.options.Count}");
        }

        private void UpdateStudy1Dropdown(int arg0)
        {
            if (arg0 < study1Dropdown.options.Count)
            {
                study1Dropdown.value = arg0;
            }
        }
    }
}