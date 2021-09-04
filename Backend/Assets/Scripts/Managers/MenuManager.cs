using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region Singleton
    public static MenuManager instance;
    void Awake() { instance = this; }
    #endregion

    public GameObject[] Tabs;
    public RectTransform FadingPanel;

    [Space]

    public TMP_InputField Password;
    public TMP_InputField IP;
    public TMP_InputField Port;

    [Space]

    public Button Submit;
    public TextMeshProUGUI Error;

    private void Start()
    {
        for (int i = 0; i < Tabs.Length; i++)
        {
            if (i != 0)
            {
                Tabs[i].SetActive(false);
            }
            else
            {
                Tabs[i].SetActive(true);
            }
        }

        Submit.onClick.AddListener(Login);
        Error.text = "";

        FadingPanel.gameObject.SetActive(true);
        LeanTween.alpha(FadingPanel, 0f, 0.2f).setDelay(0.5f);
        StartCoroutine(IStart());
    }

    IEnumerator IStart()
    {
        yield return new WaitForSeconds(0.7f);
        FadingPanel.gameObject.SetActive(false);
    }

    public void SwitchTab(int index)
    {
        FadingPanel.gameObject.SetActive(true);
        LeanTween.alpha(FadingPanel, 1f, 0.2f);
        StartCoroutine(Ist(index));
    }

    IEnumerator Ist(int index)
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < Tabs.Length; i++)
        {
            if (i != index)
            {
                Tabs[i].SetActive(false);
            }
            else
            {
                Tabs[i].SetActive(true);
            }
        }

        LeanTween.alpha(FadingPanel, 0f, 0.2f);

        yield return new WaitForSeconds(0.2f);
        FadingPanel.gameObject.SetActive(false);
    }

    void Login() { DataManager.Connect(IP.text, 26951); }
}
