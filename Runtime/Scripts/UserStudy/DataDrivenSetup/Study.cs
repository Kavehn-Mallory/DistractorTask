using System;
using DistractorTask.UserStudy.Core;
using UnityEngine;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    [Serializable]
    public struct Study
    {
        public string studyName;
        [Tooltip("Trial consists of these many selections. ")]
        public int selectionsPerTrial;
        public int trialsPerCondition;
        public Condition conditions;

    }
}