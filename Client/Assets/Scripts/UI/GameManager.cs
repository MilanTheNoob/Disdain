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

    public static GameManager instance;

    public static float MouseX;
    public static float MouseY;
    public static float Horizontal;
    public static float Vertical;
    public static bool Jump;

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

            //ActivePlayer.transform.position = DataManager.SaveFile.PlayerLocation;
            ActivePlayer.transform.position = new Vector3(0, 500, 0);
        }

        StartCoroutine(IEntry());
        Application.targetFrameRate = 60;

        if (Mobile.UI != null) Mobile.UI.SetActive(false);
        if (Mobile.ErrorUI != null) Mobile.ErrorUI.SetActive(false);

        if (Console.UI != null) Console.UI.SetActive(false);
        if (Console.ErrorUI != null) Console.ErrorUI.SetActive(false);

        if (PC.UI != null) PC.UI.SetActive(false);
        if (PC.ErrorUI != null) PC.ErrorUI.SetActive(false);

        if (WorldSettings.BackgroundPanel != null) { WorldSettings.BackgroundPanel.SetActive(false); }
        //if (DataManager.GameState == GameStateEnum.Lobby) { ClientSend.JoinLobby(); }
    }

    void Start() 
    {
        if (DataManager.ControlState == ControlStateEnum.Mobile)
        {
            ActiveUI = Mobile;
            if (Mobile.JumpB != null) Mobile.JumpB.onClick.AddListener(FJump);
        }
        else if (DataManager.ControlState == ControlStateEnum.Console)
        {
            ActiveUI = Console;
        }
        else
        {
            Debug.Log("please");
            ActiveUI = PC;
        }

        InstantToggleUISection(0);

        if (DataManager.GameState != GameStateEnum.Login) 
        {
            if (DataManager.ControlState == ControlStateEnum.PC)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        //if (DataManager.SaveData.completedTutorial) { MovePlayer(DataManager.SaveFile.playerPos); }
    }

    void FixedUpdate() 
    {
        if (DataManager.ControlState == ControlStateEnum.Mobile && Mobile.MoveJ != null)
        {
            Horizontal = Mobile.MoveJ.Horizontal;
            Vertical = Mobile.MoveJ.Vertical;

            MouseX = Mobile.ViewJ.Horizontal * Input.MobileControls.Sensitivity;
            MouseY = Mobile.ViewJ.Vertical * Input.MobileControls.Sensitivity;
        }
        else if (DataManager.ControlState == ControlStateEnum.PC && currentUISection == 0)
        {
            Horizontal = UnityEngine.Input.GetAxis("Horizontal");
            Vertical = UnityEngine.Input.GetAxis("Vertical");

            MouseX = UnityEngine.Input.GetAxis("Mouse X") * Input.PCControls.Sensitivity;
            MouseY = UnityEngine.Input.GetAxis("Mouse Y") * Input.PCControls.Sensitivity;

            Jump = UnityEngine.Input.GetKeyDown(KeyCode.Space);

            if (UnityEngine.Input.GetKeyDown(KeyCode.E) && !InteractionManager.CanInteract)
            {
                if (DataManager.GameState != GameStateEnum.Login) { ToggleUISection(1); }
            }
        }
        else if (DataManager.ControlState == ControlStateEnum.Console && currentUISection == 0)
        {
            Horizontal = UnityEngine.Input.GetAxis("Horizontal");
            Vertical = UnityEngine.Input.GetAxis("Vertical");

            MouseX = UnityEngine.Input.GetAxis("Mouse X") * Input.ConsoleControls.Sensitivity;
            MouseY = UnityEngine.Input.GetAxis("Mouse Y") * Input.ConsoleControls.Sensitivity;

            Jump = UnityEngine.Input.GetKeyDown("joystick button 0");

            if (UnityEngine.Input.GetKeyDown("joystick button 2"))
            {
                if (DataManager.GameState != GameStateEnum.Login) 
                {
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

        //if (DataManager.GameState == GameStateEnum.Lobby) ClientSend.MovementL(Horizontal, Vertical, Jump);
    }

    #region Enumerators

    IEnumerator IEntry()
    {
        yield return new WaitForSeconds(3f);
        ActiveUI.UI.SetActive(true);
        TweeningLibrary.FadeIn(ActiveUI.UI, 0.3f);
        if (DataManager.ControlState == ControlStateEnum.Console) StartCoroutine(ISetSelectedButton(0));
    }

    IEnumerator ISetSelectedButton(int i)
    {
        yield return 0;
        Console.EventSystem.SetSelectedGameObject(null);
        yield return null;
        Console.EventSystem.SetSelectedGameObject(Console.HighlightedUISections[i]);
    }

    void FJump() { StartCoroutine(IJump()); }
    IEnumerator IJump() { Jump = true; yield return 0; Jump = false; }

    #endregion
    #region Basic References

    void ResetInput()
    {
        Horizontal = 0;
        Vertical = 0;

        MouseX = 0;
        MouseY = 0;
    }

    public void SavePos() { /*DataManager.SaveFile.playerPos = DataManager.player.transform.position;*/ }

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
        if (DataManager.ControlState == ControlStateEnum.Console) instance.StartCoroutine(instance.ISetSelectedButton(0));

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

        if (DataManager.ControlState == ControlStateEnum.Console) instance.StartCoroutine(instance.ISetSelectedButton(UIIndex));

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

        if (DataManager.ControlState == ControlStateEnum.PC && DataManager.GameState != GameStateEnum.Login)
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

        if (DataManager.ControlState == ControlStateEnum.Console) instance.StartCoroutine(instance.ISetSelectedButton(UIIndex));

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

        if (DataManager.ControlState == ControlStateEnum.PC && DataManager.GameState != GameStateEnum.Login)
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

    #endregion
    #region Misc

    public static void QuitGame() { Application.Quit(); }

    public static void SetSlotVar(BaseInventorySlot slot) { instance.StartCoroutine(ISetSlotVar(slot)); }
    static IEnumerator ISetSlotVar(BaseInventorySlot slot) { yield return 0; slot.Clicked = false; }

    #endregion

    #region Local Classes

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

    #endregion
}
