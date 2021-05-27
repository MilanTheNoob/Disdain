using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Skins")]
    public List<GameObject> skins;

    [Header("Tools")]
    public List<ToolsStruct> tools;
    public enum ToolType
    {
        None,
        Axe,
        Pickaxe,
        Shovel,
        Hammer,
        Pitchfork,
        Knife,
        Sword
    }

    [HideInInspector]
    public ToolType currentToolType;
    [HideInInspector]
    public ToolsStruct currentToolStruct;

    [HideInInspector]
    public int id;
    [HideInInspector]
    public string username;

    Vector3 oldPos;
    Animator anim;
    [HideInInspector]
    public CharacterController controller;

    int animationTick;
    Vector3 velocity;

    public static float speed = 12;
    public static float jump = 3;
    public bool player;

    void Start()
    {
        oldPos = transform.position;
        anim = gameObject.GetComponent<Animator>();
        controller = gameObject.GetComponent<CharacterController>();

        anim.SetFloat("MovementValue", 0f);

        if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            currentToolType = ToolType.None;
            currentToolStruct = null;

            for (int i = 0; i < tools.Count; i++) { tools[i].toolsObject.SetActive(false); tools[i].beingUsed = false; }
            Inventory.instance.onItemChangedCallback += UpdateTools;
        }
        if (player || SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            for (int i = 0; i < skins.Count; i++)
            {
                if (i != SavingManager.skin) { skins[i].SetActive(false); } else { skins[i].SetActive(true); }
            }
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Vertical == 0 || GameManager.Horizontal == 0) { GameManager.moving = false; } else { GameManager.moving = true; }

        if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            if (controller.isGrounded && velocity.y < 0)
                velocity.y = -2f;

            if (GameManager.Vertical == 0) { anim.SetFloat("MovementValue", 0f); } else { anim.SetFloat("MovementValue", 1f); }

            Vector3 move = transform.right * GameManager.Horizontal + transform.forward * GameManager.Vertical;
            controller.Move(move * speed * Time.deltaTime);

            velocity.y += GameManager.instance.WorldSettings.Gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            if (transform.position.y < -15f)
            {
                controller.enabled = false;
                transform.position = new Vector3(transform.position.x, 25f, transform.position.z);
                controller.enabled = true;
            }
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer)
        {
            if (controller.enabled)
                controller.enabled = false;

            if (transform.position == oldPos) { animationTick += 1; } else { animationTick = 0; }
            try { if (animationTick >= 5) { anim.SetFloat("MovementValue", 0f); } else { anim.SetFloat("MovementValue", 1f); } } catch { }
            oldPos = transform.position;

            ClientSend.SendPlayerMovement(GameManager.Horizontal, GameManager.Vertical, false);
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Lobby)
        {
            if (controller.enabled)
                controller.enabled = false;

            if (transform.position == oldPos) { animationTick += 1; } else { animationTick = 0; }
            try { if (animationTick >= 5) { anim.SetFloat("MovementValue", 0f); } else { anim.SetFloat("MovementValue", 1f); } } catch { }
            oldPos = transform.position;

            if (player) LobbySend.SendPlayerMovement(GameManager.Horizontal, GameManager.Vertical, false);
        }
    }

    public void SetSkin(int skin)
    {
        for (int i = 0; i < skins.Count; i++)
        {
            if (i != skin) { skins[i].SetActive(false); } else { skins[i].SetActive(true); }
        }
    }

    public void Jump()
    {
        if (!controller.isGrounded)
            return;

        velocity.y = Mathf.Sqrt(jump * -2f * GameManager.instance.WorldSettings.Gravity);
        controller.Move(velocity * Time.deltaTime);
    }

    public void UpdateTools() { if (currentToolStruct != null && !Inventory.instance.items.Contains(currentToolStruct.scriptableObject)) { UnEquipTool(); } }

    public void EquipTool(ToolItemSettings requestedToolSettings)
    {
        for (int i = 0; i < tools.Count; i++)
        {
            if (tools[i].scriptableObject == requestedToolSettings)
            {
                tools[i].toolsObject.SetActive(true);

                currentToolType = tools[i].toolType;
                currentToolStruct = tools[i];
            }
            else { tools[i].toolsObject.SetActive(false); }
        }
    }

    public void UnEquipTool()
    {
        for (int i = 0; i < tools.Count; i++) { tools[i].toolsObject.SetActive(false); }

        currentToolType = ToolType.None;
        currentToolStruct = null;
    }

    [System.Serializable]
    public class ToolsStruct
    {
        public GameObject toolsObject;
        public ToolItemSettings scriptableObject;

        public ToolType toolType;

        [HideInInspector]
        public bool beingUsed;
    }
}
