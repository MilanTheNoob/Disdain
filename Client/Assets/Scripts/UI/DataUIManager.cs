using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataUIManager : MonoBehaviour
{
    #region Singleton

    public static DataUIManager instance;
    void Awake() { instance = this; }

    #endregion

    public GameObject UI;
    public RectTransform FadePanel;

    [Space]

    public RectTransform LoadingUI;
    public GameObject BGPanel;

    [Space]

    public RectTransform ErrorParent;
    public GameObject ContentHolder;
    public Button ErrorButton;

    [Space]

    public TMP_FontAsset TitleFont;
    public TMP_FontAsset DescFont;

    static ErrorData[] Errors =
    {
        new ErrorData
        {
            Title = "Endianness Mismatch",
            Paragraphs = new string[] { "test", "test" }
        },
        new ErrorData
        {
            Title = "No Connection",
            Paragraphs = new string[] { "U simply have no connection bro" }
        },
        new ErrorData
        {
            Title = "Server Down",
            Paragraphs = new string[] { "down bro" }
        }
    };

    void Start()
    {
        instance.UI.SetActive(true);
        FadePanel.gameObject.SetActive(true);
        //LoadingUI.gameObject.SetActive(false);
        ErrorParent.gameObject.SetActive(false);

        //PopUpErrorButton.onClick.AddListener(ErrorOnClick);
        //ErrorButton.onClick.AddListener(ErrorOnClick);
    }

    #region Scene Transitioning

    public static void FadeOut() { LeanTween.alpha(instance.FadePanel, 0f, 0.5f); }
    public static void FadeIn() { LeanTween.alpha(instance.FadePanel, 1f, 0.5f); }

    #endregion
    #region Error Funcs

    public static void Error(string error, ErrorCausesEnum[] errorCauses)
    {
        instance.UI.SetActive(true);
        //instance.LoadingUI.gameObject.SetActive(false);
        instance.ErrorParent.gameObject.SetActive(true);
        instance.BGPanel.SetActive(false);

        LeanTween.alpha(instance.BGPanel, 1f, 1f);
        LeanTween.scale(instance.ErrorParent.gameObject, Vector3.one, 0.5f).setDelay(0.5f).setEaseInExpo();

        for (int i = 0; i < instance.ContentHolder.transform.childCount; i++)
            LeanTween.alpha(instance.FadePanel, 0f, 1f);

        for (int i = 0; i < errorCauses.Length; i++)
        {
            GameObject title = new GameObject("Cause " + (i + 1) + " - Title");
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();

            title.transform.parent = instance.ContentHolder.transform;
            title.transform.localScale = Vector3.one;

            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 15;
            titleText.fontSizeMax = 20;
            titleText.font = instance.TitleFont;
            titleText.text = "Possible Cause " + (i + 1) + " - " + Errors[i].Title;

            for (int j = 0; j < Errors[i].Paragraphs.Length; j++)
            {
                GameObject para = new GameObject("Cause " + (i + 1) + " - Paragraph " + (j + 1));
                TextMeshProUGUI paraText = para.AddComponent<TextMeshProUGUI>();

                para.transform.parent = instance.ContentHolder.transform;
                para.transform.localScale = Vector3.one;

                RectTransform pt = title.GetComponent<RectTransform>();
                pt.sizeDelta = new Vector3(pt.sizeDelta.x, 100f);   

                paraText.enableAutoSizing = true;
                paraText.fontSizeMin = 10;
                paraText.fontSizeMax = 15;
                paraText.font = instance.DescFont;
                paraText.text = Errors[i].Paragraphs[j];
            }

            title.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 20);
        }
    }

    public void QuitOnClick() { Application.Quit(); }

    #endregion
}

public enum ErrorCausesEnum
{
    Endianness = 0,
    NoConnection,
    ServerDown,
}

class ErrorData
{
    public string Title;
    public string[] Paragraphs;
}