using System;
using DistractorTask.Transport;
using DistractorTask.Transport.DataContainer;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class TestLog : MonoBehaviour
    {

        private float _lastTimeStamp;
        private const float Interval = 2f;
        private int _counter;


        private void Awake()
        {
            _lastTimeStamp = Time.time + Interval;
        }

        private void Update()
        {
            if (_lastTimeStamp + Interval > Time.time)
            {
                _lastTimeStamp = Time.time;
                _counter++;
                LogSystem.LogKeyframe(LogCategory.Network, $"Test Message {_counter}");
            }
        }
    }
}