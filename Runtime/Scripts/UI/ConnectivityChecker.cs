﻿using System;
using DistractorTask.Transport;
using MixedReality.Toolkit.UX;
using TMPro;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using UnityEngine;

namespace DistractorTask.UI
{
    public class ConnectivityChecker : MonoBehaviour
    {
        public FontIconSelector selector;
        public TextMeshProUGUI text;


        public string connectedIconName = "Icon 114";
        public string disconnectedIconName = "Icon 126";
        public string attemptingConnectionIconName = "Icon 3";
        public string connectionFailedIconName = "Icon 140";

        private const string Connected = "Connected";
        private const string Disconnected = "Disconnected";
        private const string AttemptingConnection = "Attempting to connect";
        private const string ConnectionFailed = "Connection failed";

        private void Start()
        {
            selector.CurrentIconName = disconnectedIconName;
            text.text = Disconnected;
            NetworkConnectionManager.Instance.OnConnectionRequested += OnConnectionRequested;
            NetworkConnectionManager.Instance.OnConnectionEstablished += OnConnectionEstablished;
            NetworkConnectionManager.Instance.OnConnectionDisconnected += OnConnectionDisconnected;
        }

        private void OnConnectionDisconnected(NetworkEndpoint networkEndpoint, DisconnectReason disconnectReason)
        {
            if (NetworkConnectionManager.DidConnectionThrowError(disconnectReason))
            {
                OnConnectionFailed();
                return;
            }
            selector.CurrentIconName = disconnectedIconName;
            text.text = Disconnected;
        }

        private void OnConnectionEstablished(bool connectionSuccessful)
        {
            if (connectionSuccessful)
            {
                OnConnectionEstablished();
                return;
            }
            OnConnectionFailed();
        }

        private void OnConnectionEstablished()
        {
            selector.CurrentIconName = connectedIconName;
            text.text = Connected;
        }

        private void OnConnectionFailed()
        {
            selector.CurrentIconName = connectionFailedIconName;
            text.text = ConnectionFailed;
        }

        private void OnConnectionRequested()
        {
            selector.CurrentIconName = attemptingConnectionIconName;
            text.text = AttemptingConnection;
        }
    }
}