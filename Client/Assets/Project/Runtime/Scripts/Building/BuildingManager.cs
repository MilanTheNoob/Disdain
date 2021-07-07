using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    #region Singleton

    public static BuildingManager instance;
    void Awake() { instance = this; }

    #endregion

    public GameObject buildButton;
    public GameObject cancelButton;
    public GameObject rotateButton;

    [Space]

    public float tolerance = 0f;

    [Space]

    public LayerMask layer;

    List<Vector3> placedStructures = new List<Vector3>();

    public Camera cam;

    bool isBuilding;
    bool pauseBuilding;
    bool oldSnapped;
    bool oldBuilding;

    GameObject previewG;
    BuildPreview previewS;
    ItemSettings previewI;

    GameObject dropG;
    ItemSettings dropI;

    #region Unity Funcs

    void Start()
    {
        buildButton.SetActive(false);
        cancelButton.SetActive(false);
        rotateButton.SetActive(false);

        if (DataManager.GameState == GameStateEnum.Singleplayer) { LoadData(); }
    }

    void FixedUpdate()
    {
        if (cam == null) { cam = FindObjectOfType<Camera>(); }

        if (isBuilding)
        {
            if (dropG == null)
            {
                if (isBuilding != oldBuilding)
                {
                    TweeningLibrary.FadeIn(cancelButton, 0.1f);
                    TweeningLibrary.FadeIn(rotateButton, 0.1f);
                    oldBuilding = isBuilding;

                    if (previewS.GetSnapped()) { TweeningLibrary.FadeIn(buildButton, 0.1f); }
                }

                if (previewS.GetSnapped() != oldSnapped)
                {
                    if (previewS.GetSnapped() == true) { TweeningLibrary.FadeIn(buildButton, 0.1f); } else { TweeningLibrary.FadeOut(buildButton, 0.1f); }
                    oldSnapped = previewS.GetSnapped();
                }

                if (pauseBuilding)
                {
                    if (Mathf.Abs(GameManager.MouseX) >= tolerance || Mathf.Abs(GameManager.MouseY) >= tolerance) { pauseBuilding = false; }
                }
                else { BuildRay(); }
            }

            if (dropG != null) { BuildRay(); }

            if (dropG != null) { dropG.layer = 10; }
        }

        if (isBuilding && Input.GetKeyDown(KeyCode.Q)) FinishBuild();
    }

    #endregion

    #region Basic Building Funcs

    /// <summary>
    /// Creates a new build based off an ItemSettings
    /// </summary>
    /// <param name="i">ItemSettings to instantiate a structure</param>
    /// <returns></returns>
    public bool NewBuild(ItemSettings i)
    {
        if (isBuilding) { return false; }

        previewG = Instantiate(i.gameObject, Vector3.zero, i.gameObject.transform.rotation);
        previewG.name = i.name;
        previewS = previewG.GetComponent<BuildPreview>();
        previewI = i;
        previewG.layer = 10;

        isBuilding = true;
        AudioManager.PlayBuild();

        return true;
    }

    /// <summary>
    /// Rotataes the current structure by 90 degrees
    /// </summary>
    public void RotateBuild()
    {
        if (!isBuilding) { return; }

        if (previewG != null)
        {
            previewG.transform.eulerAngles = new Vector3(previewG.transform.eulerAngles.x, previewG.transform.eulerAngles.y + 90f, previewG.transform.eulerAngles.z);
        }
        else
        {
            dropG.transform.eulerAngles = new Vector3(dropG.transform.eulerAngles.x, dropG.transform.eulerAngles.y + 90f, dropG.transform.eulerAngles.z);
        }
    }

    /// <summary>
    /// Cancels the current structure
    /// </summary>
    public void CancelBuild()
    {
        if (previewG != null)
        {
            Destroy(previewG);

            previewG = null;
            previewS = null;
            previewI = null;
        }
        else
        {
            Destroy(dropG);

            dropG = null;
            dropI = null;
        }

        isBuilding = false;
        oldBuilding = false;

        AudioManager.PlayBuild();

        TweeningLibrary.FadeOut(buildButton, 0.1f);
        TweeningLibrary.FadeOut(cancelButton, 0.1f);
        TweeningLibrary.FadeOut(rotateButton, 0.1f);
    }

    #endregion

    /// <summary>
    /// Finishes the current structure
    /// </summary>
    public void FinishBuild()
    {
        /*
        if (previewG != null)
        {
            if (!previewS.GetSnapped()) { return; }

            if (DataManager.GameState == GameStateEnum.Singleplayer)
            {
                SaveFileClass.PropClass sd = new SaveFileClass.PropClass
                {
                    ID = previewI.name,
                    Pos = previewG.transform.position,
                    Euler = previewG.transform.eulerAngles
                };
                placedStructures.Add(previewG.transform.position);
                DataManager.SaveFile.Chunks[DataManager.SaveFile.FindChunk(sd.Pos)].Structures.Add(sd);

                Inventory.instance.Destroy(previewI);
            }
            else if (DataManager.GameState == GameStateEnum.Multiplayer)
            {
                //ClientSend.AddStructure(previewG);
                Destroy(previewG);
            }

            previewG = null;
            previewS = null;
            previewI = null;
        }
        else
        {
            if (DataManager.GameState == GameStateEnum.Singleplayer)
            {
                try
                {
                    SaveFileClass.PropClass propData = new SaveFileClass.PropClass
                    {
                        ID = dropI.gameObject.name,
                        Pos = dropG.transform.position,
                        Euler = dropG.transform.eulerAngles,
                        Scale = dropG.transform.localScale
                    };
                    DataManager.SaveFile.Chunks[DataManager.SaveFile.FindChunk(propData.Pos)].Misc.Add(propData);
                } catch { }

                Inventory.instance.Destroy(dropI);
                dropG.layer = 9;

                //if (!dropI.ignoreGravity) { dropG.AddComponent<Rigidbody>(); }
                TerrainGenerator.AddToNearestChunk(dropG, ChildTypeEnum.Structures);
            }
            else if (DataManager.GameState == GameStateEnum.Multiplayer)
            {
                // TODO: Add multiplayer support
            }

            dropG = null;
            dropI = null;
        }

        isBuilding = false;
        oldBuilding = false;

        AudioManager.PlayBuild();

        TweeningLibrary.FadeOut(buildButton, 0.1f);
        TweeningLibrary.FadeOut(cancelButton, 0.1f);
        TweeningLibrary.FadeOut(rotateButton, 0.1f);
        */
    }

    /// <summary>
    /// Called to drop an item from the inventory
    /// </summary>
    /// <param name="i">The item in question</param>
    /// <returns></returns>
    public bool StartDropItem(ItemSettings i)
    {
        if (isBuilding) { return false; }

        GameManager.ToggleUISection(0);
        TweeningLibrary.FadeIn(buildButton, 0.1f);
        TweeningLibrary.FadeIn(cancelButton, 0.1f);
        TweeningLibrary.FadeIn(rotateButton, 0.1f);

        dropG = Instantiate(i.gameObject);
        dropG.layer = 10;
        dropG.transform.eulerAngles = Vector3.zero;
        dropG.name = i.gameObject.name;
        dropI = i;

        isBuilding = true;

        return true;
    }

    public static void PauseBuild(bool v) { instance.pauseBuilding = v; }

    void BuildRay()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 20f, ~layer))
        {
            if (previewG != null)
            {
                previewG.transform.position = hit.point;
            }
            else
            {
                dropG.transform.position = hit.point;
            }
        }
    }

    #region Loading

    public void LoadData()
    {
        /*
        for (int i = 0; i < DataManager.SaveFile.structures.Count; i++)
        {
            if (!placedStructures.Contains(DataManager.SaveFile.structures[i].pos) && TerrainGenerator.CanAddToNearestChunk(DataManager.SaveFile.structures[i].pos))
            {
                StructureItemSettings si = Resources.Load<StructureItemSettings>("Prefabs/Interactable Items/" + DataManager.SaveFile.structures[i].name);
                GameObject g = Instantiate(si.gameObject, DataManager.SaveFile.structures[i].pos, DataManager.SaveFile.structures[i].rot);
                g.GetComponent<BuildPreview>().Place();

                placedStructures.Add(DataManager.SaveFile.structures[i].pos);
            }

        }
        */
    }

    #endregion
}
