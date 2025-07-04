using System;
using System.Collections;
using DistractorTask.Core;
using MagicLeap.OpenXR.Features.EyeTracker;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.OpenXR;

namespace DistractorTask.Logging
{
    public class EyeTrackingLogger : MonoBehaviour
    {
        
        private MagicLeapEyeTrackerFeature _eyeTrackerFeature;
        private bool _eyeTrackerInitialized;
        private int _initializationAttempts;
        private const int MaxInitializationAttempts = 3;
        
        private Camera _mainCamera;
        
        private bool _lightSensorAvailable = false;
        private float _lastLuxValue = 0f;
        private Accelerometer _accelerometer;
        private UnityEngine.InputSystem.Gyroscope _gyroscope;
        private AttitudeSensor _attitudeSensor;
        private LinearAccelerationSensor _linearAccelerationSensor;
        
        private bool _accelAvailable = false;
        
        private bool _gyroAvailable = false;
        private bool _attitudeAvailable = false;
        private bool _linearAccelAvailable = false;

        [SerializeField]
        private TMP_Text debugText;
        
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError("CSVWriter: Main Camera not found! Camera position/rotation will not be logged.");
            }
            PermissionRequestComponent.Instance.OnEyeTrackingAndPupilSizePermissionGranted += InitializeEyeTracker;
        }


        private void Start()
        {
            InitializeLightSensor();
            InitializeIMUSensors();
        }

        private void Update()
        {
            if (_eyeTrackerInitialized)
            {
                CollectEyeTrackingData();
            }
            else if(_initializationAttempts == 0)
            {
                StartCoroutine(RetryInitialization(0));
            }
            GatherGyroValues();
            GatherLuxValue();
        }

        private void OnDestroy()
        {
            if (_eyeTrackerFeature != null && _eyeTrackerFeature.enabled)
            {
                _eyeTrackerFeature.DestroyEyeTracker();
                Debug.Log("Eye Tracker destroyed.");
            }
        }


        private void InitializeLightSensor()
        {
            if (LightSensor.current != null)
            {
                InputSystem.EnableDevice(LightSensor.current);
                LightSensor.current.samplingFrequency = 60;
                _lightSensorAvailable = true;
                Debug.Log("Light sensor enabled for CSV logging.");
            }
            else
            {
                Debug.LogWarning("Light sensor not available for CSV logging.");
                _lightSensorAvailable = false;
            }
        }
        
        private void InitializeIMUSensors()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                _accelerometer = Accelerometer.current;
                if (_accelerometer != null)
                {
                    InputSystem.EnableDevice(_accelerometer);
                    _accelAvailable = true;
                    Debug.Log("Accelerometer enabled for CSV logging.");
                }
                else { Debug.LogWarning("Accelerometer not available for CSV logging."); }

                _gyroscope = UnityEngine.InputSystem.Gyroscope.current;
                if (_gyroscope != null)
                {
                    InputSystem.EnableDevice(_gyroscope);
                    _gyroAvailable = true;
                    Debug.Log("Gyroscope enabled for CSV logging.");
                }
                else { Debug.LogWarning("Gyroscope not available for CSV logging."); }

                _linearAccelerationSensor = LinearAccelerationSensor.current;
                if (_linearAccelerationSensor != null)
                {
                    InputSystem.EnableDevice(_linearAccelerationSensor);
                    _linearAccelAvailable = true;
                    Debug.Log("Linear Acceleration Sensor enabled for CSV logging.");
                }
                else { Debug.LogWarning("Linear Acceleration Sensor not available for CSV logging."); }

                _attitudeSensor = AttitudeSensor.current;
                if (_attitudeSensor != null)
                {
                    InputSystem.EnableDevice(_attitudeSensor);
                    _attitudeAvailable = true;
                    Debug.Log("Attitude Sensor enabled for CSV logging.");
                }
                else { Debug.LogWarning("Attitude Sensor not available for CSV logging."); }
            }
            else
            {
                Debug.Log("IMU sensors only initialized on Android platform.");
            }
        }
        
        
        private void InitializeEyeTracker()
        {
            PermissionRequestComponent.Instance.OnEyeTrackingAndPupilSizePermissionGranted -= InitializeEyeTracker;
            try
            {
                Debug.Log("Initializing eye tracker...");
                _eyeTrackerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapEyeTrackerFeature>();
                if (_eyeTrackerFeature != null && _eyeTrackerFeature.enabled)
                {
                    _eyeTrackerFeature.CreateEyeTracker();
                    Debug.Log("Eye tracker initialization called.");

                    StartCoroutine(VerifyEyeTrackerInitialization(0.5f));
                }
                else
                {
                    Debug.LogWarning("MagicLeapEyeTrackerFeature is not available or enabled. Eye tracking data will not be available.");
                    _initializationAttempts++;

                    if (_initializationAttempts < MaxInitializationAttempts)
                    {
                        Debug.Log($"Retrying eye tracker initialization (attempt {_initializationAttempts}/{MaxInitializationAttempts})...");
                        StartCoroutine(RetryInitialization(1.0f));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing eye tracker: {ex.Message}");
                _initializationAttempts++;

                if (_initializationAttempts < MaxInitializationAttempts)
                {
                    Debug.Log($"Retrying eye tracker initialization (attempt {_initializationAttempts}/{MaxInitializationAttempts})...");
                    StartCoroutine(RetryInitialization(1.0f));
                }
            }
        }
        
        private IEnumerator RetryInitialization(float delay)
        {
            yield return new WaitForSeconds(delay);
            InitializeEyeTracker();
        }
        
        private IEnumerator VerifyEyeTrackerInitialization(float delay)
        {
            yield return new WaitForSeconds(delay);

            try
            {
                if (_eyeTrackerFeature != null)
                {
                    EyeTrackerData testData = _eyeTrackerFeature.GetEyeTrackerData();
                    if (!testData.Equals(default(EyeTrackerData)) && testData.PupilData != null)
                    {
                        Debug.Log("Eye tracker verified as working!");
                        _eyeTrackerInitialized = true;
                    }
                    else
                    {
                        Debug.LogWarning("Eye tracker initialization check failed - no valid data received.");
                        _initializationAttempts++;

                        if (_initializationAttempts < MaxInitializationAttempts)
                        {
                            Debug.Log($"Retrying eye tracker initialization (attempt {_initializationAttempts}/{MaxInitializationAttempts})...");
                            StartCoroutine(RetryInitialization(1.0f));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error verifying eye tracker: {ex.Message}");
            }
        }
        

        private void GatherGyroValues()
        {
            Vector3 accel = _accelAvailable ? _accelerometer.acceleration.ReadValue() : Vector3.zero;
            Vector3 gyro = _gyroAvailable ? _gyroscope.angularVelocity.ReadValue() : Vector3.zero;
            Vector3 linearAccel = _linearAccelAvailable
                ? _linearAccelerationSensor.acceleration.ReadValue()
                : Vector3.zero;
            Quaternion attitude = _attitudeAvailable ? _attitudeSensor.attitude.ReadValue() : Quaternion.identity;
            
            LoggingComponent.Log(LogData.CreateGyroValuesLogData(accel, gyro, linearAccel, attitude));
        }

        private void CollectEyeTrackingData()
        {
            EyeTrackerData data = _eyeTrackerFeature.GetEyeTrackerData();
            Debug.Log(
                $"Got eye tracker data - Valid: {!data.Equals(default(EyeTrackerData))}, PupilData null: {data.PupilData == null}");
            
            Vector3 leftEyePosition = new Vector3(-1, -1, -1);
            Vector3 rightEyePosition = new Vector3(-1, -1, -1);
            Vector2 pupilDiameter = new Vector2(-1, -1);

            if (data.GeometricData != null)
            {
                foreach (var geometricData in data.GeometricData)
                {
                    if (geometricData.Valid)
                    {
                        var eyePosition = new Vector3(geometricData.EyeInSkullPosition.x,
                            geometricData.EyeInSkullPosition.y, geometricData.EyeOpenness);
                        if (geometricData.Eye == Eye.Left)
                        {
                            leftEyePosition = eyePosition;
                        }
                        else
                        {
                            rightEyePosition = eyePosition;
                        }
                    }
                }
            }
            


            if (data.PupilData != null)
                foreach (var pupilData in data.PupilData)
                {
                    if (!pupilData.Valid) continue;

                    if (pupilData.Eye == Eye.Left)
                    {
                        pupilDiameter.x = pupilData.PupilDiameter;
                    }
                    else
                    {
                        pupilDiameter.y = pupilData.PupilDiameter;
                    }
                }

            StaticData staticData = data.StaticData;
            
            
            GazeBehavior gazeBehavior = data.GazeBehaviorData;

            var gazeBehaviourType = "";
            long gazeStartTimeStamp = -1;
            ulong gazeDuration = 0;

            long currentTimeStamp = -1;
            
            if (gazeBehavior is { Valid: true, MetaData: { Valid: true } })
            {
                gazeBehaviourType = gazeBehavior.GazeBehaviorType.ToString();
                gazeStartTimeStamp = gazeBehavior.OnsetTime;
                
                gazeDuration = gazeBehavior.Duration;
                currentTimeStamp = gazeBehavior.Time;
            }


            debugText.text = "We got here and we are tracking the eyes";
            LoggingComponent.Log(LogData.CreateEyeTrackingLogData(_mainCamera.transform.position, _mainCamera.transform.rotation, leftEyePosition, rightEyePosition, new Vector2(staticData.EyeWidthMax, staticData.EyeHeightMax), pupilDiameter, currentTimeStamp, gazeBehaviourType, gazeStartTimeStamp, gazeDuration));
        }
        
        
        private void GatherLuxValue()
        {
            var luxValue = -1f;
            if (_lightSensorAvailable && LightSensor.current != null && LightSensor.current.enabled)
            {
                luxValue = LightSensor.current.lightLevel.ReadValue();
            }
            LoggingComponent.Log(LogData.CreateLuxLogData(luxValue));
        }
    }
}