using System;
using MagicLeap.Android;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace DistractorTask.Core
{
    public class PermissionRequestComponent : Singleton<PermissionRequestComponent>
    {
        
        public Action OnEyeTrackingAndPupilSizePermissionGranted = delegate { };
        
        private void Start()
        {
            // The permissions to request
            string[] permissions = new string[]
                { MLPermission.Camera, MLPermission.EyeTracking, MLPermission.PupilSize, MLPermission.SpatialMapping, MLPermission.DepthCamera, MLPermission.EyeCamera, MagicLeap.Android.Permissions.WebView };

            MagicLeap.Android.Permissions.RequestPermissions(
                permissions,
                OnPermissionGranted, OnPermissionDenied, OnPermissionDeniedDontAskAgain);
            
            
        }
        private void OnPermissionGranted(string permission)
        {
            Debug.Log($"{permission} was granted.");
            if (Permissions.CheckPermission(Permissions.EyeTracking) &&
                Permissions.CheckPermission(Permissions.PupilSize))
            {
                OnEyeTrackingAndPupilSizePermissionGranted.Invoke();
            }
        }
        
        
        private void OnPermissionDenied(string permission)
        {
            Debug.Log($"{permission} was denied.");
        }

        private void OnPermissionDeniedDontAskAgain(string permission)
        {
            Debug.Log($"{permission} was denied and cannot be request again.");
        }
        
       
    }
}