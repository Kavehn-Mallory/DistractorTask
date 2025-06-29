using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace DistractorTask.Core
{
    public class PermissionRequestComponent : MonoBehaviour
    {
        void Start()
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