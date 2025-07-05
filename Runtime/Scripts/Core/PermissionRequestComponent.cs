using System;
using System.Collections.Generic;
using MagicLeap.Android;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace DistractorTask.Core
{
    public class PermissionRequestComponent : Singleton<PermissionRequestComponent>
    {
        

        private Dictionary<string, PermissionCallbackHandler> _permissionCallbacks = new Dictionary<string, PermissionCallbackHandler>();
        
        private void Start()
        {
            // The permissions to request
            string[] permissions = new string[]
                { UnityEngine.Android.Permission.Camera, Permissions.EyeTracking, Permissions.PupilSize, Permissions.SpatialMapping, Permissions.DepthCamera, Permissions.EyeCamera, MagicLeap.Android.Permissions.WebView, MagicLeap.Android.Permissions.SpatialAnchors };

            MagicLeap.Android.Permissions.RequestPermissions(
                permissions,
                OnPermissionGranted, OnPermissionDenied, OnPermissionDeniedDontAskAgain);
            
            
        }
        private void OnPermissionGranted(string permission)
        {
            Debug.Log($"{permission} was granted.");

            if (_permissionCallbacks.TryGetValue(permission, out var handler))
            {
                handler.PermissionGranted = true;
                handler.OnPermissionGranted.Invoke();
                return;
            }
            _permissionCallbacks.Add(permission, new PermissionCallbackHandler
            {
                PermissionGranted = true,
                OnPermissionGranted = delegate { }
            });

        }
        
        
        private void OnPermissionDenied(string permission)
        {
            Debug.Log($"{permission} was denied.");
        }

        private void OnPermissionDeniedDontAskAgain(string permission)
        {
            Debug.Log($"{permission} was denied and cannot be request again.");
        }


        public void RegisterOnPermissionGranted(string permission, Action onPermissionGranted)
        {
            if (_permissionCallbacks.TryGetValue(permission, out var handler))
            {
                if (handler.PermissionGranted)
                {
                    onPermissionGranted.Invoke();
                }

                handler.OnPermissionGranted += onPermissionGranted;
                return;
            }
            _permissionCallbacks.Add(permission, new PermissionCallbackHandler
            {
                PermissionGranted = false,
                OnPermissionGranted = onPermissionGranted
            });
            
        }

        public void UnregisterOnPermissionGranted(string permission, Action onPermissionGranted)
        {
            if (_permissionCallbacks.TryGetValue(permission, out var handler))
            {
                handler.OnPermissionGranted -= onPermissionGranted;
            }
        }

        internal class PermissionCallbackHandler
        {
            public bool PermissionGranted;
            public Action OnPermissionGranted = delegate { };
        }
        
       
    }
}