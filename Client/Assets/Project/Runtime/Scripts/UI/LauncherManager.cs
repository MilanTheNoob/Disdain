using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherManager : MonoBehaviour
{
    /// <summary>
    /// TO BE IMPLEMENTED
    /// </summary>
    /// <param name=""></param>
    public static void SetLauncherData(LauncherDataClass launcherData)
    {

    }
}

public class LauncherDataClass
{
    public bool UpdateNeeded;
    public string UpdateVersion;

    public string CurrentVersion;

    public UpdatePatchNotesClass[] UpdatePatchNotes; 
    public class UpdatePatchNotesClass
    {
        public string Title;
        public string[] Paragraphs;
    }
}