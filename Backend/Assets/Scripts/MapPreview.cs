using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPreview : MonoBehaviour
{
    public static GenerateData GenerateData = null;

    public Button PreviewButton;
    public Button ExitButton;

    [Space]

    public GameObject GameObject;

    [Space]

    public ChooseUIStruct ChooseUI;
    public BiomesUIStruct BiomesUI;
    public HeightmapsUIStruct HeightmapsUI;
    public HeightmapUIStruct HeightmapUI;
    public PropGroupsUIStruct PropGroupsUI;
    public PropUIStruct PropUI;

    [Space]

    public Sprite RoundedSquare;
    public Color32 OptionColor;
    public TMP_FontAsset OptionFont;

    [Space]

    public int MapSize;
    public Material Material;

    List<TerrainChunk> Chunks = new List<TerrainChunk>();
    GameObject chunkParent;

    private void OnEnable()
    {
        if (GenerateData == null)
            Debug.LogErrorFormat("MapPreview has not received GenerateData before being activated");

        #region Button Listeners

        #region Choose UI

        ChooseUI.BiomeButton.onClick.RemoveAllListeners();
        ChooseUI.BiomeButton.onClick.AddListener(CUI_BiomeButton);

        ChooseUI.HeightmapButton.onClick.RemoveAllListeners();
        ChooseUI.HeightmapButton.onClick.AddListener(CUI_BaseHeightmaps);

        ChooseUI.BiomeHeightmapButton.onClick.RemoveAllListeners();
        ChooseUI.BiomeHeightmapButton.onClick.AddListener(CUI_BiomeHeightmap);

        #endregion
        #region Biomes UI

        BiomesUI.BackButton.onClick.RemoveAllListeners();
        BiomesUI.BackButton.onClick.AddListener(BUI_Back);

        #endregion
        #region Heightmap

        #endregion
    }

    #region Listeners

    void CUI_BiomeButton() { Fade(ChooseUI.Parent.gameObject, BiomesUI.Parent.gameObject); }
    void CUI_BaseHeightmaps() 
    { 
        Fade(ChooseUI.Parent.gameObject, HeightmapsUI.Parent.gameObject); 

        for (int i = 0; i < HeightmapsUI.ContentObject.transform.childCount; i++)
            Destroy(HeightmapsUI.ContentObject.transform.GetChild(0).gameObject);

        for (int i = 0; i < GenerateData.BaseHeightmaps.Count; i++)
        {
            GameObject button = new GameObject("Heightmap UI Button #" + (i + 1));
            Image buttonI = button.AddComponent<Image>();
            RectTransform buttonRT = button.GetComponent<RectTransform>();

            buttonRT.parent = HeightmapsUI.ContentObject.transform;
            buttonRT.sizeDelta = new Vector2(buttonRT.sizeDelta.x, 30f);
            buttonRT.localScale = Vector3.one;

            buttonI.sprite = RoundedSquare;
            buttonI.color = OptionColor;
            buttonI.type = Image.Type.Sliced;
            buttonI.pixelsPerUnitMultiplier = 8;


            GameObject buttonText = new GameObject("Text");
            TextMeshProUGUI buttonTextTMP = buttonText.AddComponent<TextMeshProUGUI>();
            RectTransform buttonTextRT = buttonText.GetComponent<RectTransform>();

            buttonTextRT.parent = buttonRT;
            buttonTextRT.position = Vector3.zero;
            buttonTextRT.localScale = Vector3.one;
            buttonTextRT.sizeDelta = new Vector2(300f, 15f);

            buttonTextTMP.text = "Heightmap #" + i;
            buttonTextTMP.font = OptionFont;
            buttonTextTMP.enableAutoSizing = true;
            buttonTextTMP.fontSizeMin = 0;
            buttonTextTMP.fontSizeMax = 72;
            buttonTextTMP.color = new Color(1, 1, 1);
        }
    }
    void CUI_BiomeHeightmap() { Fade(ChooseUI.Parent.gameObject, HeightmapUI.Parent.gameObject); }

    void BUI_Back() { Fade(BiomesUI.Parent.gameObject, ChooseUI.Parent.gameObject); }

    #endregion
    #region UI Code

    public void Fade(GameObject from, GameObject to)
    {
        to.SetActive(true);
        from.SetActive(false);
    }

    #endregion

    #region Generation

    public void StopPreviewing()
    {
        for (int i = 0; i < Chunks.Count; i++) { DestroyImmediate(Chunks[i].ChunkObject); }
        Chunks.Clear();
    }
    /*
    void OnValuesUpdated()
    {
        for (int i = 0; i < Chunks.Count; i++) { DestroyImmediate(Chunks[i].ChunkObject); }
        Chunks.Clear();

        for (int x = 0; x < MapSize; x++)
        {
            for (int y = 0; y < MapSize; y++)
            {
                Noise noise = new Noise(Random.Range(0, 9999999));

                SaveFile.ChunkClass chunkData = new SaveFile.ChunkClass
                {
                    Coord = Vector3.zero,
                    HeightMap = Generation.GenerateHeightMap(GS, Vector3.zero, Vector3.zero, null, noise)
                };

                Chunks.Add(new TerrainChunk(chunkData, GS, chunkParent.transform, Material));
            }
        }
    }
    */
    #endregion
    #region UI Structs

    [System.Serializable]
    public struct ChooseUIStruct
    {
        public RectTransform Parent;

        [Space]

        public Button BiomeButton;
        public Button HeightmapButton;

        [Space]

        public Button BiomeHeightmapButton;
    }

    [System.Serializable]
    public struct BiomesUIStruct
    {
        public RectTransform Parent;

        [Space]

        public Button HeightmapButton;
        public Button PropGroupButton;

        [Space]

        public Button BackButton;
    }

    [System.Serializable]
    public struct HeightmapsUIStruct
    {
        public RectTransform Parent;

        [Space]

        public Button AddHeightmapButton;
        public Button BackButton;

        [Space]

        public GameObject ContentObject;
    }

    [System.Serializable]
    public struct HeightmapUIStruct
    {
        public RectTransform Parent;

        [Space]

        public TextArea ScaleArea;
        public TextArea HeightArea;
        public TextArea OctavesArea;

        [Space]

        public TextArea PersistenceArea;
        public TextArea LacunarityArea;

        [Space]

        public Button BackButton;
    }

    [System.Serializable]
    public struct PropGroupsUIStruct
    {
        public RectTransform Parent;

        [Space]

        public Button AddGroupButton;
        public Button BackButton;

        [Space]

        public GameObject ContentObject;
    }

    [System.Serializable]
    public struct PropUIStruct
    {
        public RectTransform Parent;
        public Button BackButton;

        [Space]

        public TextArea PropGroupIDArea;
        public TextArea PropIDArea;

        [Space]

        public TextArea XRotationArea;
        public TextArea YRotationArea;
        public TextArea ZRotationArea;

        [Space]

        public TextArea MaxScaleArea;
        public TextArea MinScaleArea;

        [Space]

        public TextArea GenerateTypeArea;

        [Space]

        public TextArea ChunkMinArea;
        public TextArea ChunkMaxArea;

        [Space]

        public TextArea ChanceArea;

        [Space]

        public TextArea SimplexValue;
        public Button SimplexMap;

        [Space]

        public Toggle UseBoundsToggle;
        public TextArea BoundsXArea;
        public TextArea BoundsYArea;
        public TextArea BoundsZArea;
    }

    #endregion
}
