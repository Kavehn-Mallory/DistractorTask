using System;
using System.Collections;
using System.Globalization;
using System.IO;
using DistractorTask.Core;
using DistractorTask.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace DistractorTask.FrameCapture
{
    public class FrameCaptureComponent : MonoBehaviour
    {
        //todo implement for magic leap 
        // Indicates if the camera is connected
        private bool isCameraConnected;
        // Reference to the MLCamera object that will access the device's camera
        private MLCamera colorCamera;
        // Indicates if the camera device is available
        private bool cameraDeviceAvailable;

        // Indicates if an image is currently being captured
        private bool isCapturingImage;
        
        private string KeyFrameLogPath
        {
            get
            {
#if UNITY_EDITOR
                return Application.streamingAssetsPath + $"/{DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-')}_Framecapture.jpg";
#else
                return Application.persistentDataPath + $"/{DateTime.Now.ToString("u", CultureInfo.InvariantCulture).Replace(':', '-')}_Framecapture.jpg";
#endif

            }

        }


        private void Awake()
        {
            PermissionRequestComponent.Instance.RegisterOnPermissionGranted(UnityEngine.Android.Permission.Camera, InitializeCamera);
            
        }

        private void Start()
        {
            InputHandler.InputHandler.Instance.OnSelectionButtonPressed += CaptureImage;
        }
        

        private void OnDisable()
        {
            DisableMLCamera();
        }

        private void InitializeCamera()
        {
            StartCoroutine(EnableMLCamera());
        }
        
        // Define a coroutine that will enable the camera by checking its availability,
        // creating and connecting it, and preparing it for capture
        private IEnumerator EnableMLCamera()
        {
            // Loop until the camera device is available
            while (!cameraDeviceAvailable)
            {
                MLResult result = MLCamera.GetDeviceAvailabilityStatus(MLCamera.Identifier.Main, out cameraDeviceAvailable);
                if (!(result.IsOk && cameraDeviceAvailable))
                {
                    // Wait until camera device is available
                    yield return new WaitForSeconds(1.0f);
                }
            }

            Debug.Log("Camera device available.");

            // Create and connect the camera with a context that enables video stabilization and camera only capture
            ConnectCamera();

            // Wait until the camera is connected since this script uses the async "CreateAndConnectAsync" Method to connect to the camera.
            while (!isCameraConnected)
            {
                yield return null;
            }

            Debug.Log("Camera device connected.");

            // Prepare the camera for capture with a configuration that specifies JPEG output format, frame rate, and resolution
            ConfigureAndPrepareCapture();
        }
        
        // Define an async method that will create and connect the camera with a context that enables video stabilization and Video only capture
        private async void ConnectCamera()
        {
            MLCamera.ConnectContext context = MLCamera.ConnectContext.Create();
            context.EnableVideoStabilization = false;
            context.Flags = MLCameraBase.ConnectFlag.CamOnly;

            // Use the CreateAndConnectAsync method to create and connect the camera asynchronously
            colorCamera = await MLCamera.CreateAndConnectAsync(context);

            if (colorCamera != null)
            {
                // Register a callback for when a raw image is available after capture
                colorCamera.OnRawImageAvailable += OnCaptureRawImageComplete;
                isCameraConnected = true;
            }
        }
        
        // Define an async method that will prepare the camera for capture with a configuration that specifies
        // JPEG output format, frame rate, and resolution
        private async void ConfigureAndPrepareCapture()
        {
            MLCamera.CaptureStreamConfig[] imageConfig = new MLCamera.CaptureStreamConfig[1]
            {
                new MLCamera.CaptureStreamConfig()
                {
                    OutputFormat = MLCamera.OutputFormat.JPEG,
                    CaptureType = MLCamera.CaptureType.Image,
                    Width = 1920,
                    Height = 1080
                }
            };

            MLCamera.CaptureConfig captureConfig = new MLCamera.CaptureConfig()
            {
                StreamConfigs = imageConfig,
                CaptureFrameRate = MLCamera.CaptureFrameRate._30FPS
            };

            // Use the PrepareCapture method to set the capture configuration and get the metadata handle
            MLResult prepareCaptureResult = colorCamera.PrepareCapture(captureConfig, out MLCamera.Metadata _);

            if (!prepareCaptureResult.IsOk)
            {
                return;
            }

        }
        
        /// <summary>
        /// Handles the event of a new image getting captured and visualizes it with the Image Visualizer
        /// </summary>
        /// <param name="capturedImage">Captured frame.</param>
        /// <param name="resultExtras">Results Extras.</param>
        private void OnCaptureRawImageComplete(MLCamera.CameraOutput capturedImage, MLCamera.ResultExtras resultExtras, MLCamera.Metadata metadataHandle)
        {
            MLResult aeStateResult = metadataHandle.GetControlAEStateResultMetadata(out MLCameraBase.Metadata.ControlAEState controlAEState);
            MLResult awbStateResult = metadataHandle.GetControlAWBStateResultMetadata(out MLCameraBase.Metadata.ControlAWBState controlAWBState);

            if (aeStateResult.IsOk && awbStateResult.IsOk)
            {
                bool autoExposureComplete = controlAEState == MLCameraBase.Metadata.ControlAEState.Converged || controlAEState == MLCameraBase.Metadata.ControlAEState.Locked;
                bool autoWhiteBalanceComplete = controlAWBState == MLCameraBase.Metadata.ControlAWBState.Converged || controlAWBState == MLCameraBase.Metadata.ControlAWBState.Locked;

                if (autoExposureComplete && autoWhiteBalanceComplete)
                {
                    // This example is configured to render JPEG images only.
                    if(capturedImage.Format == MLCameraBase.OutputFormat.JPEG)
                    {
                        byte[] bytes = capturedImage.Planes[0].Data;
                        string filePath = KeyFrameLogPath;
                        File.WriteAllBytes(filePath, bytes);
                        LoggingComponent.Log(LogData.CreateFrameCaptureLogData(LogData.GetCurrentTimestamp(), Camera.main.transform.position, Camera.main.transform.rotation, filePath));
                    }
                }
            }

        }
        
        private async void CaptureImage()
        {
            // Set the flag to indicate that an image capture is in progress
            isCapturingImage = true;

            var aeawbResult = await colorCamera.PreCaptureAEAWBAsync();
            if (!aeawbResult.IsOk)
            {
                Debug.LogError("Image capture failed!");
            }
            else
            {
                var result = await colorCamera.CaptureImageAsync(1);
                if (!result.IsOk)
                {
                    Debug.LogError("Image capture failed!");
                }
            }

            // Reset the flag to indicate that image capture is complete
            isCapturingImage = false;
        }
        
        /// <summary>
        /// Disconnects the MLCamera if it was ever created or connected.
        /// </summary>
        private void DisableMLCamera()
        {
            if (colorCamera != null)
            {
                colorCamera.Disconnect();

                // Explicitly set to false here as the disconnect was attempted.
                isCameraConnected = false;
            }
        }
    }
}