using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.XR.OpenXR;
using MagicLeap.OpenXR.Features.EyeTracker;
using MagicLeap.OpenXR.Features.FacialExpressions;
using MagicLeap.Android;
using UnityEngine.InputSystem;
using UnityEngine.XR.MagicLeap;
using System.Collections;

public class CSVWriter : MonoBehaviour
{
    public static CSVWriter instance;
    public string CurrentSubjectID => currentSubjectID;
    private string currentSubjectID = null;
    private bool recordingStarted = false;
    private string fileName;
    private string filePath;
    private StringBuilder sb = new StringBuilder();
    private StreamWriter streamWriter;
    private Camera mainCamera;
    private MagicLeapEyeTrackerFeature eyeTrackerFeature;
    private bool permissionsGranted;
    private bool eyeTrackerInitialized = false;
    private int initializationAttempts = 0;
    private const int MaxInitializationAttempts = 3;
    private MagicLeapFacialExpressionFeature facialExpressionFeature;
    private BlendShapeProperties[] blendShapeProperties;
    private bool facialPermissionGranted;
    private bool facialInitialized;
    private bool lightSensorAvailable = false;
    private float lastLuxValue = 0f;
    private Accelerometer accelerometer;
    private UnityEngine.InputSystem.Gyroscope gyroscope;
    private AttitudeSensor attitudeSensor;
    private LinearAccelerationSensor linearAccelerationSensor;
    private bool accelAvailable = false;
    private bool gyroAvailable = false;
    private bool attitudeAvailable = false;
    private bool linearAccelAvailable = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CSVWriter: Main Camera not found! Camera position/rotation will not be logged.");
        }
    }

    void Start()
    {
        RequestPermissions();
        RequestFacialPermissions();
        InitializeLightSensor();
        InitializeIMUSensors();

        StartCoroutine(VerifyPermissionsAfterDelay(1.0f));
    }

    void Update()
    {
        if (lightSensorAvailable && Time.frameCount % 60 == 0)
        {
            LogLuxValue();
        }

        Debug.Log($"Update - permissionsGranted: {permissionsGranted}, eyeTrackerFeature null: {eyeTrackerFeature == null}, Platform: {Application.platform}");

        if (!permissionsGranted || eyeTrackerFeature == null) return;

        if (eyeTrackerFeature == null)
        {
            InitializeEyeTracker();
            if (eyeTrackerFeature == null) return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Collecting eye tracking data...");
            CollectEyeTrackingData();
        }
        else
        {
            Debug.Log($"Not collecting eye tracking data - not on Android platform. Current platform: {Application.platform}");
        }
    }

    private void OnDestroy()
    {
        eyeTrackerFeature?.DestroyEyeTracker();
        facialExpressionFeature?.DestroyClient();

        if (lightSensorAvailable && LightSensor.current != null)
        {
            InputSystem.DisableDevice(LightSensor.current);
            Debug.Log("Light sensor disabled.");
        }

        if (streamWriter != null)
        {
            streamWriter.Flush();
            streamWriter.Close();
        }

        if (accelAvailable && accelerometer != null) InputSystem.DisableDevice(accelerometer);
        if (gyroAvailable && gyroscope != null) InputSystem.DisableDevice(gyroscope);
        if (linearAccelAvailable && linearAccelerationSensor != null) InputSystem.DisableDevice(linearAccelerationSensor);
        if (attitudeAvailable && attitudeSensor != null) InputSystem.DisableDevice(attitudeSensor);
        Debug.Log("IMU sensors disabled.");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application quitting - Final CSV save");
        WriteToFile();
        if (streamWriter != null)
        {
            streamWriter.Flush();
            streamWriter.Close();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("Application pausing - Saving CSV");
            WriteToFile();
        }
    }

    public void StartRecordingWithSubjectID(string subjectID)
    {
        if (recordingStarted && streamWriter != null)
        {
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter = null;
            recordingStarted = false;
        }

        currentSubjectID = !string.IsNullOrEmpty(subjectID) ? subjectID : GenerateSubjectID();

        InitializeRecording();
    }

    public void WriteData(string eventName, string value)
    {
        if (!recordingStarted)
        {
            if (currentSubjectID == null)
            {
                currentSubjectID = GenerateSubjectID();
            }
            InitializeRecording();
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string taskload = "Low";
        string pathType = "Default";
        string studySelection = "Default";

        float luxValue = GetLuxValue();
        string luxString = luxValue >= 0 ? luxValue.ToString("F2") : "N/A";

        string line;
        int numFacialExpressions = Enum.GetNames(typeof(FacialBlendShape)).Length;
        int numEyeGazeColumns = 15;
        int numIMUColumns = 12;
        int numCameraColumns = 6;

        if (eventName == "LUX_READING")
        {
            string emptyEyeGazeColumns = string.Join(",", Enumerable.Repeat("N/A", numEyeGazeColumns));
            string emptyIMUColumns = string.Join(",", Enumerable.Repeat("N/A", numIMUColumns));
            string emptyCameraColumns = string.Join(",", Enumerable.Repeat("N/A", numCameraColumns));
            string emptyFacialColumns = string.Join(",", Enumerable.Repeat("N/A", numFacialExpressions));

            line = $"{timestamp},{studySelection},{taskload},{pathType},{eventName},," +
                   $"{emptyEyeGazeColumns}," +
                   $"{emptyIMUColumns}," +
                   $"{emptyCameraColumns}," +
                   $"{value}," +
                   $"{emptyFacialColumns}";
        }
        else
        {
            string emptyEyeGazeIMUCameraLuxColumns = string.Join(",", Enumerable.Repeat("N/A", numEyeGazeColumns + numIMUColumns + numCameraColumns + 1));
            string emptyFacialColumns = string.Join(",", Enumerable.Repeat("N/A", numFacialExpressions));

            line = $"{timestamp},{studySelection},{taskload},{pathType},{eventName},{value}," +
                   $"{emptyEyeGazeIMUCameraLuxColumns}," +
                   $"{emptyFacialColumns}";
        }

        sb.AppendLine(line);
        WriteToFile();
    }

    public void LogLuxValue()
    {
        float currentLux = GetLuxValue();
        if (currentLux >= 0)
        {
            lastLuxValue = currentLux;
            WriteData("LUX_READING", currentLux.ToString("F2"));
        }
    }

    private void InitializeRecording()
    {
        fileName = $"Data_{currentSubjectID}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        string directoryPath;
        if (Application.platform == RuntimePlatform.Android)
        {
            directoryPath = Application.persistentDataPath;
            Debug.Log($"Base Android path: {directoryPath}");
            directoryPath = Path.Combine(directoryPath, "WordClouds");
            Debug.Log($"Full Android path: {directoryPath}");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Created directory: {directoryPath}");
            }
        }
        else
        {
            directoryPath = Path.Combine(Application.dataPath, "Recordings");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        filePath = Path.Combine(directoryPath, fileName);
        Debug.Log($"Final file path: {filePath}");

        if (streamWriter != null)
        {
            try
            {
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error closing previous stream: {e.Message}");
            }
            streamWriter = null;
        }

        try
        {
            streamWriter = new StreamWriter(filePath, false);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open file: {e.Message}");
            return;
        }

        string header = "Timestamp,StudySelection,TaskLoad,PathType,Event,Value," +
                        "Eye,Openness,EyeInSkullX,EyeInSkullY,PupilDiameter," +
                        "GazeBehaviorType,GazeAmplitude,GazeDirection," +
                        "GazeVelocity,GazeOnsetTime,GazeDuration,EyeWidthMax,EyeHeightMax," +
                        "VergenceX,VergenceY,VergenceZ,FixationConfidence,LeftBlink,RightBlink," +
                        "LeftCenterConfidence,RightCenterConfidence," +
                        "AccelX,AccelY,AccelZ," +
                        "GyroX,GyroY,GyroZ," +
                        "LinearAccelX,LinearAccelY,LinearAccelZ," +
                        "Pitch,Yaw,Roll," +
                        "CamPosX,CamPosY,CamPosZ," +
                        "CamPitch,CamYaw,CamRoll," +
                        "CamForwardX,CamForwardY,CamForwardZ," +
                        "LuxValue," +
                        "GazePosX,GazePosY,GazePosZ," +
                        "GazeRotX,GazeRotY,GazeRotZ,GazeRotW," +
                        string.Join(",", GetFacialExpressionHeaders());
        sb.AppendLine(header);
        WriteToFile();

        recordingStarted = true;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string taskload = "Low";
        string pathType = "Default";
        string studySelection = "Default";

        string line = $"{timestamp},{studySelection},{taskload},{pathType},SUBJECT_ID,{currentSubjectID}";
        sb.AppendLine(line);
        WriteToFile();

        Debug.Log($"Started recording with subject ID: {currentSubjectID}");
    }

    private void InitializeLightSensor()
    {
        if (LightSensor.current != null)
        {
            InputSystem.EnableDevice(LightSensor.current);
            LightSensor.current.samplingFrequency = 60;
            lightSensorAvailable = true;
            Debug.Log("Light sensor enabled for CSV logging.");
        }
        else
        {
            Debug.LogWarning("Light sensor not available for CSV logging.");
            lightSensorAvailable = false;
        }
    }

    private void InitializeIMUSensors()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            accelerometer = Accelerometer.current;
            if (accelerometer != null)
            {
                InputSystem.EnableDevice(accelerometer);
                accelAvailable = true;
                Debug.Log("Accelerometer enabled for CSV logging.");
            }
            else { Debug.LogWarning("Accelerometer not available for CSV logging."); }

            gyroscope = UnityEngine.InputSystem.Gyroscope.current;
            if (gyroscope != null)
            {
                InputSystem.EnableDevice(gyroscope);
                gyroAvailable = true;
                Debug.Log("Gyroscope enabled for CSV logging.");
            }
            else { Debug.LogWarning("Gyroscope not available for CSV logging."); }

            linearAccelerationSensor = LinearAccelerationSensor.current;
            if (linearAccelerationSensor != null)
            {
                InputSystem.EnableDevice(linearAccelerationSensor);
                linearAccelAvailable = true;
                Debug.Log("Linear Acceleration Sensor enabled for CSV logging.");
            }
            else { Debug.LogWarning("Linear Acceleration Sensor not available for CSV logging."); }

            attitudeSensor = AttitudeSensor.current;
            if (attitudeSensor != null)
            {
                InputSystem.EnableDevice(attitudeSensor);
                attitudeAvailable = true;
                Debug.Log("Attitude Sensor enabled for CSV logging.");
            }
            else { Debug.LogWarning("Attitude Sensor not available for CSV logging."); }
        }
        else
        {
            Debug.Log("IMU sensors only initialized on Android platform.");
        }
    }

    private void InitializeFacialExpressions()
    {
        var allRequestedFacialBlendShapes = Enum.GetValues(typeof(FacialBlendShape)) as FacialBlendShape[];

        facialExpressionFeature = OpenXRSettings.Instance
            .GetFeature<MagicLeapFacialExpressionFeature>();

        facialExpressionFeature.CreateClient(allRequestedFacialBlendShapes);

        blendShapeProperties = new BlendShapeProperties[allRequestedFacialBlendShapes.Length];

        for (int i = 0; i < blendShapeProperties.Length; i++)
        {
            blendShapeProperties[i].FacialBlendShape = allRequestedFacialBlendShapes[i];
            blendShapeProperties[i].Weight = 0;
            blendShapeProperties[i].Flags = BlendShapePropertiesFlags.None;
        }

        facialInitialized = true;
    }

    private void InitializeEyeTracker()
    {
        try
        {
            Debug.Log("Initializing eye tracker...");
            eyeTrackerFeature = OpenXRSettings.Instance.GetFeature<MagicLeapEyeTrackerFeature>();
            if (eyeTrackerFeature != null && eyeTrackerFeature.enabled)
            {
                eyeTrackerFeature.CreateEyeTracker();
                Debug.Log("Eye tracker initialization called.");

                StartCoroutine(VerifyEyeTrackerInitialization(0.5f));
            }
            else
            {
                Debug.LogWarning("MagicLeapEyeTrackerFeature is not available or enabled. Eye tracking data will not be available.");
                initializationAttempts++;

                if (initializationAttempts < MaxInitializationAttempts)
                {
                    Debug.Log($"Retrying eye tracker initialization (attempt {initializationAttempts}/{MaxInitializationAttempts})...");
                    StartCoroutine(RetryInitialization(1.0f));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing eye tracker: {ex.Message}");
            initializationAttempts++;

            if (initializationAttempts < MaxInitializationAttempts)
            {
                Debug.Log($"Retrying eye tracker initialization (attempt {initializationAttempts}/{MaxInitializationAttempts})...");
                StartCoroutine(RetryInitialization(1.0f));
            }
        }
    }

    private void RequestPermissions()
    {
        try
        {
            Debug.Log("Requesting eye tracking permissions...");
            Permissions.RequestPermissions(
                new string[] {
                    Permissions.EyeTracking,
                    Permissions.PupilSize
                },
                OnPermissionGranted,
                OnPermissionDenied
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error requesting permissions: {ex.Message}");
            permissionsGranted = false;
        }
    }

    private void RequestFacialPermissions()
    {
        try
        {
            Debug.Log("Requesting facial expression permissions...");
            Permissions.RequestPermission(
                Permissions.FacialExpression,
                OnFacialPermissionGranted,
                OnFacialPermissionDenied
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error requesting facial permissions: {ex.Message}");
            facialPermissionGranted = false;
        }
    }

    private void OnPermissionGranted(string permission)
    {
        permissionsGranted = true;
        Debug.Log($"Permission granted: {permission}");

        if (Permissions.CheckPermission(Permissions.EyeTracking) &&
            Permissions.CheckPermission(Permissions.PupilSize))
        {
            InitializeEyeTracker();
        }
    }

    private void OnPermissionDenied(string permission)
    {
        permissionsGranted = false;
        Debug.LogError($"Eye tracking permission denied: {permission}");
    }

    private void OnFacialPermissionGranted(string permission)
    {
        facialPermissionGranted = true;
        InitializeFacialExpressions();
    }

    private void OnFacialPermissionDenied(string permission)
    {
        facialPermissionGranted = false;
        Debug.LogError($"Facial expression permission denied: {permission}");
    }

    private IEnumerator VerifyPermissionsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        VerifyAllPermissions();
    }

    private void VerifyAllPermissions()
    {
        bool eyeTrackingPermission = Permissions.CheckPermission(Permissions.EyeTracking);
        bool pupilSizePermission = Permissions.CheckPermission(Permissions.PupilSize);
        bool facialPermission = Permissions.CheckPermission(Permissions.FacialExpression);

        Debug.Log($"Permission Status - Eye Tracking: {eyeTrackingPermission}, " +
                 $"Pupil Size: {pupilSizePermission}, " +
                 $"Facial: {facialPermission}");

        if (!eyeTrackingPermission || !pupilSizePermission)
        {
            Debug.LogWarning("Some tracking permissions are not granted. Requesting again...");
            RequestPermissions();
        }

        if (!facialPermission)
        {
            Debug.LogWarning("Facial expression permission is not granted. Requesting again...");
            RequestFacialPermissions();
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
            if (eyeTrackerFeature != null)
            {
                EyeTrackerData testData = eyeTrackerFeature.GetEyeTrackerData();
                if (!testData.Equals(default(EyeTrackerData)) && testData.PupilData != null)
                {
                    Debug.Log("Eye tracker verified as working!");
                    eyeTrackerInitialized = true;
                }
                else
                {
                    Debug.LogWarning("Eye tracker initialization check failed - no valid data received.");
                    initializationAttempts++;

                    if (initializationAttempts < MaxInitializationAttempts)
                    {
                        Debug.Log($"Retrying eye tracker initialization (attempt {initializationAttempts}/{MaxInitializationAttempts})...");
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

    private void CollectEyeTrackingData()
    {
        EyeTrackerData data = eyeTrackerFeature.GetEyeTrackerData();
        Debug.Log($"Got eye tracker data - Valid: {!data.Equals(default(EyeTrackerData))}, PupilData null: {data.PupilData == null}");

        bool staticDataValid = false;
        StaticData staticData = default;
        try
        {
            staticData = data.StaticData;
            staticDataValid = !staticData.Equals(default(StaticData));
            if (!staticDataValid)
            {
                Debug.LogWarning("Static eye data is not available. Using fallback values for eye measurements.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error getting static eye data: {ex.Message}. Using fallback values for eye measurements.");
        }

        for (int i = 0; i < data.PupilData.Length && i < data.GeometricData.Length; i++)
        {
            var pupilData = data.PupilData[i];
            if (!pupilData.Valid) continue;

            if (i >= data.GeometricData.Length) continue;
            GeometricData geometricData = data.GeometricData[i];
            if (!geometricData.Valid) continue;

            GazeBehavior gazeBehavior = data.GazeBehaviorData;
            if (gazeBehavior.Equals(default(GazeBehavior)) || !gazeBehavior.MetaData.Valid) continue;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string taskload = "Low";
            string pathType = "Default";
            string studySelection = "Default";

            Vector3 accel = accelAvailable ? accelerometer.acceleration.ReadValue() : Vector3.zero;
            Vector3 gyro = gyroAvailable ? gyroscope.angularVelocity.ReadValue() : Vector3.zero;
            Vector3 linearAccel = linearAccelAvailable ? linearAccelerationSensor.acceleration.ReadValue() : Vector3.zero;
            Quaternion attitude = attitudeAvailable ? attitudeSensor.attitude.ReadValue() : Quaternion.identity;
            Vector3 eulerAngles = attitude.eulerAngles;

            string accelStr = accelAvailable ? $"{accel.x:F4},{accel.y:F4},{accel.z:F4}" : "N/A,N/A,N/A";
            string gyroStr = gyroAvailable ? $"{gyro.x:F4},{gyro.y:F4},{gyro.z:F4}" : "N/A,N/A,N/A";
            string linearAccelStr = linearAccelAvailable ? $"{linearAccel.x:F4},{linearAccel.y:F4},{linearAccel.z:F4}" : "N/A,N/A,N/A";
            string attitudeStr = attitudeAvailable ? $"{eulerAngles.x:F4},{eulerAngles.y:F4},{eulerAngles.z:F4}" : "N/A,N/A,N/A";

            Vector3 camPos = mainCamera?.transform.position ?? Vector3.zero;
            Vector3 camRot = mainCamera?.transform.rotation.eulerAngles ?? Vector3.zero;
            Vector3 camForward = mainCamera?.transform.forward ?? Vector3.zero;

            string camPosStr = mainCamera != null ?
                $"{camPos.x:0.0000},{camPos.y:0.0000},{camPos.z:0.0000}" : "N/A,N/A,N/A";
            string camRotStr = mainCamera != null ?
                $"{camRot.x:0.0000},{camRot.y:0.0000},{camRot.z:0.0000}" : "N/A,N/A,N/A";
            string camForwardStr = mainCamera != null ?
                $"{camForward.x:0.0000},{camForward.y:0.0000},{camForward.z:0.0000}" : "N/A,N/A,N/A";

            float luxValue = GetLuxValue();
            string luxString = luxValue >= 0 ? luxValue.ToString("F2") : "N/A";

            string gazePosStr = "N/A,N/A,N/A";
            string gazeRotStr = "N/A,N/A,N/A,N/A";

            string facialData = "";
            if (facialPermissionGranted && facialInitialized)
            {
                facialExpressionFeature.GetBlendShapesInfo(ref blendShapeProperties);
                facialData = string.Join(",", blendShapeProperties.Select(p =>
                    p.Flags.HasFlag(BlendShapePropertiesFlags.ValidBit) &&
                    p.Flags.HasFlag(BlendShapePropertiesFlags.TrackedBit)
                        ? p.Weight.ToString("F4")
                        : "0"));
            }
            else
            {
                int numFacialExpressions = Enum.GetNames(typeof(FacialBlendShape)).Length;
                facialData = string.Join(",", Enumerable.Repeat("0", numFacialExpressions));
            }

            string vergenceStr = "N/A,N/A,N/A";

            string eyeWidthHeightStr = staticDataValid ?
                $"{staticData.EyeWidthMax:F4},{staticData.EyeHeightMax:F4}" : "N/A,N/A";

            string gazeBehaviorTypeStr = gazeBehavior.MetaData.Valid ? gazeBehavior.GazeBehaviorType.ToString() : "Invalid";
            string gazeAmplitudeStr = gazeBehavior.MetaData.Valid ? gazeBehavior.MetaData.Amplitude.ToString("F4") : "N/A";
            string gazeDirectionStr = gazeBehavior.MetaData.Valid ? gazeBehavior.MetaData.Direction.ToString("F4") : "N/A";
            string gazeVelocityStr = gazeBehavior.MetaData.Valid ? gazeBehavior.MetaData.Velocity.ToString("F4") : "N/A";
            string gazeOnsetTimeStr = gazeBehavior.MetaData.Valid ? gazeBehavior.OnsetTime.ToString() : "N/A";
            string gazeDurationStr = gazeBehavior.MetaData.Valid ? gazeBehavior.Duration.ToString("F4") : "N/A";

            string line = $"{timestamp},{studySelection},{taskload},{pathType},PUPIL_DATA,," +
                          $"{pupilData.Eye},{geometricData.EyeOpenness:F4}," +
                          $"{geometricData.EyeInSkullPosition.x:F4},{geometricData.EyeInSkullPosition.y:F4}," +
                          $"{pupilData.PupilDiameter:F4}," +
                          $"{gazeBehaviorTypeStr}," +
                          $"{gazeAmplitudeStr}," +
                          $"{gazeDirectionStr}," +
                          $"{gazeVelocityStr}," +
                          $"{gazeOnsetTimeStr},{gazeDurationStr}," +
                          $"{eyeWidthHeightStr}," +
                          $"{vergenceStr}," +
                          "N/A," +
                          "N/A,N/A," +
                          "N/A,N/A," +
                          $"{accelStr}," +
                          $"{gyroStr}," +
                          $"{linearAccelStr}," +
                          $"{attitudeStr}," +
                          $"{camPosStr}," +
                          $"{camRotStr}," +
                          $"{camForwardStr}," +
                          $"{luxString}," +
                          $"{gazePosStr}," +
                          $"{gazeRotStr}," +
                          facialData;
            sb.AppendLine(line);
        }
        WriteToFile();
    }

    private void WriteToFile()
    {
        if (streamWriter != null && sb.Length > 0)
        {
            try
            {
                if (!streamWriter.BaseStream.CanWrite)
                {
                    Debug.LogWarning("Attempted to write to closed StreamWriter - skipping write operation");
                    return;
                }
                streamWriter.Write(sb.ToString());
                streamWriter.Flush();
                sb.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error writing to CSV file: {e.Message}");
            }
        }
    }

    private float GetLuxValue()
    {
        if (lightSensorAvailable && LightSensor.current != null && LightSensor.current.enabled)
        {
            return LightSensor.current.lightLevel.ReadValue();
        }
        return -1f;
    }

    private string GenerateSubjectID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder id = new StringBuilder(6);
        System.Random random = new System.Random();

        for (int i = 0; i < 6; i++)
        {
            id.Append(chars[random.Next(chars.Length)]);
        }

        string generatedID = id.ToString();
        Debug.Log($"Generated random subject ID: {generatedID}");
        return generatedID;
    }

    private string[] GetFacialExpressionHeaders()
    {
        return Enum.GetNames(typeof(FacialBlendShape));
    }
    
}
