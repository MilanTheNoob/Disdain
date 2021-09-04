using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Components")]
    public AudioMixer AudioMixer;

    [Space]

    public Slider MainAudioSlider;
    public Slider SFAudioSlider;
    public Slider MusicAudioSlider;

    [Space]

    public Slider RenderDistance;
    public Text RenderDistanceT;

    [Space]

    public Slider FramerateSlider;
    public Text FramerateTxt;

    [Space]

    public Toggle UseAliasing;
    public Toggle UseHDR;

    [Space]

    public Toggle UseTonemapping;
    public Toggle UseMotionBlur;
    public Toggle UseVignette;
    public Toggle UseBloom;

    [Space]

    public Slider Sensitivity;

    [Header("Profiles & Scriptable Objects")]
    public PostProcessVolume Volume;

    Camera cam;

    Vignette vignette;
    Bloom bloom;
    ColorGrading colorGrading;
    MotionBlur motionBlur;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        cam = GameManager.ActivePlayer.GetComponentInChildren<Camera>();

        Volume.profile.TryGetSettings(out vignette);
        Volume.profile.TryGetSettings(out bloom);
        Volume.profile.TryGetSettings(out colorGrading);
        Volume.profile.TryGetSettings(out motionBlur);

        #region Audio Setup

        MainAudioSlider.value = DataManager.SaveData.MainAudioLevel;
        SFAudioSlider.value = DataManager.SaveData.SFAudioLevel;
        MusicAudioSlider.value = DataManager.SaveData.MusicAudioLevel;

        AudioMixer.SetFloat("MainAudioLevel", DataManager.SaveData.MainAudioLevel);
        AudioMixer.SetFloat("SFLevel", DataManager.SaveData.SFAudioLevel);
        AudioMixer.SetFloat("MusicLevel", DataManager.SaveData.MusicAudioLevel);

        #endregion

        #region Graphics

        int fps = DataManager.SaveData.FPS;
        Application.targetFrameRate = fps;
        FramerateSlider.value = fps;
        FramerateTxt.text = fps.ToString();

        //UseAliasing.isOn = DataManager.SaveData.SettingsData.AA;
        //cam.allowMSAA = DataManager.SaveData.SettingsData.AA;

        UseHDR.isOn = DataManager.SaveData.HDR;
        cam.allowHDR = DataManager.SaveData.HDR;

        UseVignette.isOn = DataManager.SaveData.Vignette;
        vignette.active = DataManager.SaveData.Vignette;

        UseBloom.isOn = DataManager.SaveData.Bloom;
        bloom.active = DataManager.SaveData.Bloom;

        UseTonemapping.isOn = DataManager.SaveData.Tonemapping;
        colorGrading.active = DataManager.SaveData.Tonemapping;

        UseMotionBlur.isOn = DataManager.SaveData.MotionBlur;
        motionBlur.active = DataManager.SaveData.MotionBlur;

        #endregion

        //GameManager.ActiveUI.Sensitivity = DataManager.SaveData.SettingsData.Sensitivity;
        Sensitivity.value = DataManager.SaveData.Sensitivity;

        RenderDistance.value = DataManager.SaveData.RenderDistance;
        RenderDistanceT.text = DataManager.SaveData.RenderDistance.ToString();
    }

    #region BasicChangeFuncs

    public void SetMainVolume(float volume) { AudioMixer.SetFloat("MainAudioLevel", volume); DataManager.SaveData.MainAudioLevel = volume; }
    public void SetSFVolume(float volume) { AudioMixer.SetFloat("SFLevel", volume); DataManager.SaveData.SFAudioLevel = volume; }
    public void SetMusicVolume(float volume) { AudioMixer.SetFloat("MusicLevel", volume); DataManager.SaveData.MusicAudioLevel = volume; }

    public void SetFramerate(float value) { Application.targetFrameRate = (int)value; DataManager.SaveData.FPS = (int)value; FramerateTxt.text = value.ToString(); }
    public void SetAA(bool aa) { /*cam.allowMSAA = aa; DataManager.SaveData.SettingsData.AA = aa;*/ }
    public void SetHDR(bool hdr) { cam.allowHDR = hdr; DataManager.SaveData.HDR = hdr; }

    public void SetVignette(bool b) { UseVignette.isOn = b; vignette.active = b; }
    public void SetBloom(bool b) { UseBloom.isOn = b; bloom.active = b; }
    public void SetTonemapping(bool b) { UseTonemapping.isOn = b; colorGrading.active = b; }
    public void SetMotionBlur(bool b) { UseMotionBlur.isOn = b; motionBlur.active = b; DataManager.SaveData.MotionBlur = b; }

    //public void SetSensitivity(float s) { GameManager.Act.Sensitivity = s; DataManager.SaveData.SettingsData.Sensitivity = s; }
    /*
    public void SetRenderDistance(float d) 
    { 
        TerrainGenerator.instance.ViewDst = (int)d; 
        TerrainGenerator.instance.UpdateViewDist();

        DataManager.SaveData.RenderDistance = (int)d;
        RenderDistanceT.text = d.ToString();
    }
    */
    #endregion
}