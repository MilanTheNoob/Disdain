using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    #region Singleton

    public static TransitionManager instance;
    void Awake() { instance = this; }

    #endregion

    public Animator anim;
    public Text LoadingText;

    [HideInInspector]
    public GameObject UI;

    static bool loading;

    void Start()
    {
        transform.parent = null;
        DontDestroyOnLoad(this);

        UI = GameObject.Find("UI");
        UI.SetActive(false);
        StartCoroutine(ResetI());
    }

    IEnumerator ResetI()
    {
        yield return new WaitForSeconds(6f);
        UI.SetActive(true);
        TweeningLibrary.FadeIn(UI, 0.3f);
    }

    public static void ToScene(int scene) 
    { 
        if (loading) { return; }
        loading = true;

        if (GameObject.Find("UI") != null) { TweeningLibrary.FadeOut(GameObject.Find("UI"), 1f); }
        instance.anim.SetTrigger("DefaultTransition"); 
        instance.StartCoroutine(ToMenuI(scene)); 
    }
    static IEnumerator ToMenuI(int scene) 
    { 
        yield return new WaitForSeconds(3f); 
        SceneManager.LoadScene(scene, LoadSceneMode.Single);

        loading = false;
    }
}
