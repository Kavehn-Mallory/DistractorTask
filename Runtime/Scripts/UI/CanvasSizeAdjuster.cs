using System;
using UnityEngine;

namespace DistractorTask.UI
{
    public class CanvasSizeAdjuster : MonoBehaviour
    {
        public Canvas canvas;
        public Camera mainCamera;


        private void Awake()
        {
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(mainCamera.pixelWidth, mainCamera.pixelHeight);
        }
    }
}