using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DistractorTask.Logging
{
    public class TogglePauseLoggingComponent : MonoBehaviour
    {
        public Button button;

        public TMP_Text buttonText;

        public GameObject controls;

        private void Start()
        {
            button.onClick.AddListener(TogglePause);
        }

        private void TogglePause()
        {
            LoggingComponent.Instance.TogglePauseLogging();
            controls.SetActive(!LoggingComponent.Instance.IsPaused);
            if (LoggingComponent.Instance.IsPaused)
            {
                buttonText.text = "Continue Logging";
            }
            else
            {
                buttonText.text = "Pause Logging";
            }
        }
    }
}