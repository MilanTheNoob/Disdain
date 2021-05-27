using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Control Settings")]
public class ControlSettings : ScriptableObject
{
    public PCControlsClass PCControls;
    public MobileControlsClass MobileControls;
    public ConsoleControlsClass ConsoleControls;

    [System.Serializable]
    public class PCControlsClass
    {
        public float Sensitivity;

        [Space]

        public KeyCode InteractKey = KeyCode.E;
        public KeyCode BuildKey = KeyCode.LeftShift;
    }

    [System.Serializable]
    public class MobileControlsClass
    {
        public float Sensitivity;
        public float DeadZone;
    }

    [System.Serializable]
    public class ConsoleControlsClass
    {
        public float Sensitivity;
    }
}
