using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace DistractorTask.FrameCapture
{
    public class FrameCaptureComponent : MonoBehaviour
    {
        private PhotoCapture _photoCapture;


        
        //todo implement for magic leap 
        private void Start()
        {
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        }

        private void OnPhotoCaptureCreated(PhotoCapture captureobject)
        {
            _photoCapture = captureobject;
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

            CameraParameters c = new CameraParameters();
            c.hologramOpacity = 0.0f;
            c.cameraResolutionWidth = cameraResolution.width;
            c.cameraResolutionHeight = cameraResolution.height;
            c.pixelFormat = CapturePixelFormat.BGRA32;

            
            _photoCapture.StartPhotoModeAsync(c, OnPhotoModeStarted);
        }

        void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            _photoCapture.Dispose();
            _photoCapture = null;
        }
        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                string filename = $"CapturedImage{Time.time}_n.jpg";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

                _photoCapture.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            }
            else
            {
                Debug.LogError("Unable to start photo mode!");
            }
        }
        
        void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                Debug.Log("Saved Photo to disk!");
                _photoCapture.StopPhotoModeAsync(OnStoppedPhotoMode);
            }
            else
            {
                Debug.Log("Failed to save Photo to disk");
            }
        }
    }
}