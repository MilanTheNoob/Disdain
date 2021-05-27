using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("UI & Input")]
    public MobileUIClass Mobile;
    public UIBaseClass PC;
    public ConsoleUIClass Console;

    [Space]

    public ControlSettings Input;

    [Header("Misc")]
    public WorldSettingsClass WorldSettings;

    public static ControlStateEnum ControlState;
    public enum ControlStateEnum
    {
        Mobile,
        PC,
        Console
    }

    public static GameManager instance;

    public static float MouseX;
    public static float MouseY;
    public static float Horizontal;
    public static float Vertical;

    public static bool moving;
    public static bool loading;

    public static string ip;
    public static int port;

    public static GameObject ActivePlayer;
    public static Camera ActiveCamera;
    public static PlayerManager ActivePlayerManager;

    public static UIBaseClass ActiveUI;

    float xRotation = 0f;
    int currentUISection;

    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        #endregion

        if (WorldSettings.ActivePlayer != null)
        {
            ActivePlayer = WorldSettings.ActivePlayer;
            ActiveCamera = ActivePlayer.GetComponentInChildren<Camera>();
            ActivePlayerManager = ActivePlayer.GetComponent<PlayerManager>();
        }

        StartCoroutine(IEntry());
        Application.targetFrameRate = 60;

        Mobile.UI.SetActive(false);
        Mobile.ErrorUI.SetActive(false);
        Console.UI.SetActive(false);
        Console.ErrorUI.SetActive(false);
        PC.UI.SetActive(false);
        PC.ErrorUI.SetActive(false);

        if (WorldSettings.BackgroundPanel != null) { WorldSettings.BackgroundPanel.SetActive(false); }

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ControlState = ControlStateEnum.Mobile;
            ActiveUI = Mobile;

            Mobile.JumpB.onClick.AddListener(ActivePlayerManager.Jump);
        }
        else if (Application.platform == RuntimePlatform.XboxOne || Application.platform == RuntimePlatform.PS4 ||
            Application.platform == RuntimePlatform.PS5 || Application.platform == RuntimePlatform.GameCoreXboxOne)
        {
            ControlState = ControlStateEnum.Console;
            ActiveUI = Console;
        }
        else 
        { 
            ControlState = ControlStateEnum.PC;
            ActiveUI = PC;
        }

        ControlState = ControlStateEnum.Console;
        ActiveUI = Console;
    }

    IEnumerator IEntry()
    {
        yield return new WaitForSeconds(3f);
        ActiveUI.UI.SetActive(true);
        TweeningLibrary.FadeIn(ActiveUI.UI, 0.3f);
        StartCoroutine(ISetSelectedButton(0));
    }

    IEnumerator ISetSelectedButton(int i)
    {
        yield return 0;
        Console.EventSystem.SetSelectedGameObject(null);
        yield return null;
        Console.EventSystem.SetSelectedGameObject(Console.HighlightedUISections[i]);
    }

    void Start() 
    {
        InstantToggleUISection(0);

        if (SavingManager.GameState != SavingManager.GameStateEnum.Login) 
        {
            if (ControlState == ControlStateEnum.PC)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        //if (SavingManager.SaveData.completedTutorial) { MovePlayer(SavingManager.SaveFile.playerPos); }
    }

    void FixedUpdate() 
    {
        if (ControlState == ControlStateEnum.Mobile && Mobile.MoveJ != null)
        {
            Horizontal = Mobile.MoveJ.Horizontal;
            Vertical = Mobile.MoveJ.Vertical;

            MouseX = Mobile.ViewJ.Horizontal * Input.MobileControls.Sensitivity;
            MouseY = Mobile.ViewJ.Vertical * Input.MobileControls.Sensitivity;
        }
        else if (ControlState == ControlStateEnum.PC && currentUISection == 0)
        {
            Horizontal = UnityEngine.Input.GetAxis("Horizontal");
            Vertical = UnityEngine.Input.GetAxis("Vertical");

            MouseX = UnityEngine.Input.GetAxis("Mouse X") * Input.PCControls.Sensitivity;
            MouseY = UnityEngine.Input.GetAxis("Mouse Y") * Input.PCControls.Sensitivity;

            if (UnityEngine.Input.GetKeyDown(KeyCode.E) && !InteractionManager.CanInteract)
            {
                if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer ||
                    SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer) { ToggleUISection(1); }
            }
        }
        else if (ControlState == ControlStateEnum.Console && currentUISection == 0)
        {
            Horizontal = UnityEngine.Input.GetAxis("Horizontal");
            Vertical = UnityEngine.Input.GetAxis("Vertical");

            MouseX = UnityEngine.Input.GetAxis("Mouse X") * Input.ConsoleControls.Sensitivity;
            MouseY = UnityEngine.Input.GetAxis("Mouse Y") * Input.ConsoleControls.Sensitivity;

            if (UnityEngine.Input.GetKeyDown("joystick button 2"))
            {
                if (SavingManager.GameState != SavingManager.GameStateEnum.Login) 
                {
                    print(currentUISection);
                    if (currentUISection == 0)
                    {
                        ToggleUISection(1);
                    }
                    else
                    {
                        ToggleUISection(0);
                    }
                }
            }
        }
        if (ActivePlayer != null)
        {
            xRotation -= MouseY * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            ActiveCamera.transform.localEulerAngles = new Vector3(xRotation, 0f, 0f);
            ActivePlayer.transform.Rotate(Vector3.up * MouseX * Time.deltaTime);
        }
    }

    void ResetInput()
    {
        Horizontal = 0;
        Vertical = 0;

        MouseX = 0;
        MouseY = 0;
    }

    #region Basic References

    public void SavePos() { /*SavingManager.SaveFile.playerPos = SavingManager.player.transform.position;*/ }

    public static void ShowError(string error) 
    { 
        TweeningLibrary.FadeIn(ActiveUI.ErrorUI, 0.2f); 
        ActiveUI.ErrorText.text = error; 
    }
    public void HideError() { TweeningLibrary.FadeOut(ActiveUI.ErrorUI, 0.2f); }

    public static void MovePlayer(Vector3 pos)
    {
        ActivePlayer.GetComponent<CharacterController>().enabled = false;
        ActivePlayer.transform.position = pos;
        ActivePlayer.GetComponent<CharacterController>().enabled = true;
    }

    #endregion

    #region UI Code

    public static void DisableUISections()
    {
        instance.currentUISection = 0;
        instance.StartCoroutine(instance.ISetSelectedButton(0));

        for (int i = 0; i < ActiveUI.UISections.Length; i++)
        {
            if (ActiveUI.UISections[i] != null)
            {
                if (ActiveUI.UISections[i].activeSelf) { TweeningLibrary.FadeOut(ActiveUI.UISections[i], 0.3f); }
            }
        }

        TweeningLibrary.FadeOut(instance.WorldSettings.BackgroundPanel, 0.3f);
    }

    public static void ToggleUISection(int UIIndex)
    {
        instance.currentUISection = UIIndex;
        instance.ResetInput();

        instance.StartCoroutine(instance.ISetSelectedButton(UIIndex));

        for (int i = 0; i < ActiveUI.UISections.Length; i++)
        {
            if (ActiveUI.UISections[i] != null)
            {
                if (i == UIIndex)
                {
                    TweeningLibrary.FadeIn(ActiveUI.UISections[i], 0.3f);
                }
                else if (ActiveUI.UISections[i].activeSelf)
                {
                    TweeningLibrary.FadeOut(ActiveUI.UISections[i], 0.3f);
                }
            }
        }

        if (ActiveUI.UseBackgroundPanel)
        {
            if (UIIndex == 0)
            {
                TweeningLibrary.FadeOut(instance.WorldSettings.BackgroundPanel, 0.3f);
            }
            else
            {
                TweeningLibrary.FadeIn(instance.WorldSettings.BackgroundPanel, 0.3f);
            }
        }

        if (ControlState == ControlStateEnum.PC && SavingManager.GameState != SavingManager.GameStateEnum.Login)
        {
            if (UIIndex == 0)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public static void InstantToggleUISection(int UIIndex)
    {
        instance.currentUISection = UIIndex;
        instance.ResetInput();

        instance.StartCoroutine(instance.ISetSelectedButton(UIIndex));

        for (int i = 0; i < ActiveUI.UISections.Length; i++)
        {
            if (ActiveUI.UISections[i] != null)
            {
                if (i == UIIndex)
                {
                    ActiveUI.UISections[i].SetActive(true);
                }
                else if (ActiveUI.UISections[i].activeSelf)
                {
                    ActiveUI.UISections[i].SetActive(false);
                }
            }
        }

        if (ActiveUI.UseBackgroundPanel)
        {
            if (UIIndex == 0)
            {
                instance.WorldSettings.BackgroundPanel.SetActive(false);
            }
            else
            {
                instance.WorldSettings.BackgroundPanel.SetActive(true);
            }
        }

        if (ControlState == ControlStateEnum.PC && SavingManager.GameState != SavingManager.GameStateEnum.Login)
        {
            if (UIIndex == 0)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void ToSingleplayer()
    {
        SavingManager.ToSave();
    }

    #endregion

    public static void QuitGame() { Application.Quit(); }

    [System.Serializable]
    public class UIBaseClass
    {
        public GameObject UI;
        public GameObject[] UISections;

        [Space]

        public GameObject ErrorUI;
        public Text ErrorText;

        [Space]

        public BaseInventorySlot[] Inventory;

        [Space]

        public BaseInventorySlot[] TradingPlayer;
        public BaseInventorySlot[] TradingNPC;

        [Space]

        public BaseInventorySlot[] StoragePlayer;
        public BaseInventorySlot[] StorageContainer;

        [Space]

        public bool UseBackgroundPanel;
        public GameObject RecipesHolder;
    }

    [System.Serializable]
    public class MobileUIClass : UIBaseClass
    {
        [Space]

        public Joystick ViewJ;
        public Joystick MoveJ;
        public Button JumpB;

        [Space]

        public Button InteractButton;
    }

    /*
    [System.Serializable]
    public class PCUIClass : UIBaseClass
    {
        
    }
    */
    [System.Serializable]
    public class ConsoleUIClass : UIBaseClass
    {
        [Space]

        public GameObject[] HighlightedUISections;
        public EventSystem EventSystem;
    }

    [System.Serializable]
    public class WorldSettingsClass
    {
        public float Gravity = -9.81f;
        public float GroundDistance = 0.4f;

        [Space]

        public GameObject PlayerPrefab;
        public GameObject ActivePlayer;

        [Space]

        public GameObject BackgroundPanel;
    }
}
