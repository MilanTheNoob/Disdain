using System.Collections;
using UnityEngine;


/// <summary>
/// Deals with all the Audio in the game
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    public AudioSource[] clicks;

    [Space]

    public AudioSource[] moving;

    [Space]

    public AudioSource[] music;
    public AudioSource[] ambience;

    [Space]

    public AudioSource build;
    public AudioSource equip;
    public AudioSource hurt;
    public AudioSource chop;

    bool oldMoving;

    void Start()
    {
        StartCoroutine(PlayMusicI());
        for (int i = 0; i < ambience.Length; i++) { ambience[i].loop = true; ambience[i].Play(); }
    }

    void Update()
    {
        /*
        if (GameManager.moving != oldMoving)
        {
            if (GameManager.moving == true)
            {
                for (int i = 0; i < moving.Length; i++) { moving[i].Play(); }
            }
            else
            {
                for (int i = 0; i < moving.Length; i++) { moving[i].Stop(); }
            }
        }

        oldMoving = GameManager.moving;
        */
    }

    IEnumerator PlayMusicI()
    {
        music[Random.Range(0, music.Length)].Play();
        yield return new WaitForSeconds(Random.Range(180, 240));
        StartCoroutine(PlayMusicI());
    }

    /// <summary>
    /// Plays a click noise
    /// </summary>
    public static void PlayClick() { instance.clicks[Random.Range(0, instance.clicks.Length)].Play(); }
    /// <summary>
    /// Plays a build noise when called
    /// </summary>
    public static void PlayBuild() { instance.build.Play(); }
    /// <summary>
    /// Plays an equip noise
    /// </summary>
    public static void PlayEquip() { instance.equip.Play(); }
    /// <summary>
    /// A hurt sound is made when called
    /// </summary>
    public static void PlayHurt() { instance.hurt.Play(); }
    /// <summary>
    /// Makes a chop noise when called
    /// </summary>
    public static void PlayChop() { instance.chop.Play(); }
}
