using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPreviewUI : MonoBehaviour
{
    /*
     * The following code is incredibly long in length (compared to my norm), please refer
     * to these abbreviations for any shortened variable names
     * 
     * GD - Generate Data
     * CB - Current Biome
     * CP - Current Prop
     * 
     * CUI - Choose UI
     * 
     * BSUI - Biomes UI
     * BUI - Biome UI
     * 
     * HSUI - Heightmaps UI
     * HUI - Heightmap UI
     * 
     * PSUI - Props UI
     * PUI - Prop UI
     */

    public TMP_InputField MapSizeInput;
    public Button PreviewBtn;

    [Space]

    public Button ExitBtn;
    public Button SaveExitBtn;

    [Space]

    public Button BackButton;

    [Space]

    public CUIStruct CUI;
    public BSUIStruct BSUI;
    public BUIStruct BUI;
    public HSUIStruct HSUI;
    public HUIStruct HUI;
    public PSUIStruct PSUI;
    public PUIStruct PUI;
    public RectTransform PLUI;

    [Space]

    public Sprite RoundedSquare;
    public Color32 OptionColor;
    public TMP_FontAsset OptionFont;

    public static GenerateData GD;

    int CB = -1;
    int CP = -1;

    void Start()
    {
        CUI.Parent.gameObject.SetActive(true);
        PLUI.gameObject.SetActive(false);

        BSUI.Parent.gameObject.SetActive(false);
        BUI.Parent.gameObject.SetActive(false);

        HSUI.Parent.gameObject.SetActive(false);
        HUI.Parent.gameObject.SetActive(false);

        PSUI.Parent.gameObject.SetActive(false);
        PUI.Parent.gameObject.SetActive(false);

        for (int i = 0; i < MapPreview.instance.PropsData.Length; i++)
        {
            GameObject propG = new GameObject("Prop UI #" + i);
            TextMeshProUGUI propT = propG.AddComponent<TextMeshProUGUI>();
            RectTransform propRT = propG.GetComponent<RectTransform>();

            propRT.parent = PLUI;
            propRT.position = Vector3.zero;
            propRT.localScale = Vector3.one;
            propRT.sizeDelta = new Vector2(100f, 10f);

            propT.text = MapPreview.instance.PropsData[i].PropName + ", Group - " + MapPreview.instance.PropsData[i].Group + 
                ", Id - " + MapPreview.instance.PropsData[i].ID;
            propT.font = OptionFont;
            propT.enableAutoSizing = true;
            propT.fontSizeMin = 0;
            propT.fontSizeMax = 72;
            propT.color = new Color(1, 1, 1);
        }

        PreviewBtn.onClick.RemoveAllListeners();
        PreviewBtn.onClick.AddListener(() =>
        {
            bool c = int.TryParse(MapSizeInput.text, out int mapPreview);
            if (c) Send.BackendPreview(GD, mapPreview);
        });

        ExitBtn.onClick.RemoveAllListeners();
        ExitBtn.onClick.AddListener(() =>
        {
            Send.BackendSave(GD);
        });

        CUI.Biomes.onClick.RemoveAllListeners();
        CUI.Biomes.onClick.AddListener(() => {
            Fade(CUI.Parent, BSUI.Parent);
            ListBiomes();

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(BSUI.Parent, CUI.Parent); });
        });
        CUI.BiomeMap.onClick.RemoveAllListeners();
        CUI.BiomeMap.onClick.AddListener(() => {
            Fade(CUI.Parent, HUI.Parent);
            ShowHeightmap(GD.BiomeHeightmap);

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(HUI.Parent, CUI.Parent); });
        });

        BSUI.AddBiome.onClick.RemoveAllListeners();
        BSUI.AddBiome.onClick.AddListener(() => {
            Fade(BSUI.Parent, BUI.Parent);

            CB = GD.Biomes.Count;
            GD.Biomes.Add(new GenerateData.BiomeData());

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(BUI.Parent, BSUI.Parent); ListBiomes(); CB = -1; });
        });

        BUI.Heightmap.onClick.RemoveAllListeners();
        BUI.Heightmap.onClick.AddListener(() => { 
            Fade(BUI.Parent, HSUI.Parent); ListBiomeHeightmaps();

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(HSUI.Parent, BUI.Parent); });
        });
        BUI.Props.onClick.RemoveAllListeners();
        BUI.Props.onClick.AddListener(() =>  { 
            Fade(BUI.Parent, PSUI.Parent); ListBiomeProps();

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(PSUI.Parent, BUI.Parent); });
        });

        HSUI.AddHeightmap.onClick.RemoveAllListeners();
        HSUI.AddHeightmap.onClick.AddListener(() => {
            Fade(HSUI.Parent, HUI.Parent);

            int pos = GD.Biomes[CB].Heightmaps.Count;
            GD.Biomes[CB].Heightmaps.Add(new GenerateData.HeightmapData());

            HUI.Scale.InitializeArea(10);
            HUI.Scale.ValueChanged = (float value) => { GD.Biomes[CB].Heightmaps[pos].Scale = (int)value; };
            HUI.Height.InitializeArea(10);
            HUI.Height.ValueChanged = (float value) => { GD.Biomes[CB].Heightmaps[pos].HeightMultiplier = (int)value; };
            HUI.Octaves.InitializeArea(7);
            HUI.Octaves.ValueChanged = (float value) => { GD.Biomes[CB].Heightmaps[pos].Octaves = (int)value; };

            HUI.Persistence.InitializeArea(0.6f);
            HUI.Persistence.ValueChanged = (float value) => { GD.Biomes[CB].Heightmaps[pos].Persistence = value; };
            HUI.Lacunarity.InitializeArea(1.44f);
            HUI.Lacunarity.ValueChanged = (float value) => { GD.Biomes[CB].Heightmaps[pos].Lacunarity = value; };

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(HUI.Parent, HSUI.Parent); ListBiomeHeightmaps(); });
        });

        PSUI.AddPropButton.onClick.RemoveAllListeners();
        PSUI.AddPropButton.onClick.AddListener(() => {
            Fade(PSUI.Parent, PUI.Parent);

            CP = GD.Biomes[CB].Props.Count;
            GD.Biomes[CB].Props.Add(new GenerateData.PropData());

            ShowProp(CP);

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(PUI.Parent, PSUI.Parent); ListBiomeProps(); });
        });
    }

    #region Props Listing

    void ListBiomeProps()
    {
        for (int i = 0; i < PSUI.Content.transform.childCount; i++)
            Destroy(PSUI.Content.transform.GetChild(i).gameObject);

        for (int i = 0; i < GD.Biomes[CB].Props.Count; i++)
        {
            GameObject button = new GameObject("Prop Group UI Button #" + (i + 1));
            Image buttonI = button.AddComponent<Image>();
            RectTransform buttonRT = button.GetComponent<RectTransform>();
            Button buttonB = button.AddComponent<Button>();

            buttonRT.SetParent(PSUI.Content.transform);
            buttonRT.sizeDelta = new Vector2(buttonRT.sizeDelta.x, 18f);
            buttonRT.localScale = Vector3.one;

            buttonI.sprite = RoundedSquare;
            buttonI.color = new Color(1, 1, 1);
            buttonI.type = Image.Type.Sliced;
            buttonI.pixelsPerUnitMultiplier = 8;

            buttonB.colors = new ColorBlock
            {
                normalColor = new Color32(30, 30, 30, 255),
                highlightedColor = new Color32(32, 32, 32, 255),
                pressedColor = new Color32(35, 35, 35, 255),
                selectedColor = new Color32(30, 30, 30, 255),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            buttonB.onClick.AddListener(() => { ShowProp(buttonB.transform.GetSiblingIndex()); });

            GameObject buttonText = new GameObject("Text");
            TextMeshProUGUI buttonTextTMP = buttonText.AddComponent<TextMeshProUGUI>();
            RectTransform buttonTextRT = buttonText.GetComponent<RectTransform>();

            buttonTextRT.parent = buttonRT;
            buttonTextRT.position = Vector3.zero;
            buttonTextRT.localScale = Vector3.one;
            buttonTextRT.sizeDelta = new Vector2(100f, 10f);

            buttonTextTMP.text = "Prop #" + (i + 1) + ", " + FindPropName(i);
            buttonTextTMP.font = OptionFont;
            buttonTextTMP.enableAutoSizing = true;
            buttonTextTMP.fontSizeMin = 0;
            buttonTextTMP.fontSizeMax = 72;
            buttonTextTMP.color = new Color(1, 1, 1);
        }
    }

    #endregion
    #region Biomes Listing

    void ListBiomes()
    {
        for (int i = 0; i < BSUI.Content.transform.childCount; i++)
            Destroy(BSUI.Content.transform.GetChild(i).gameObject);

        for (int i = 0; i < GD.Biomes.Count; i++)
        {
            GameObject button = new GameObject("Biome UI Button #" + (i + 1));
            Image buttonI = button.AddComponent<Image>();
            RectTransform buttonRT = button.GetComponent<RectTransform>();
            Button buttonB = button.AddComponent<Button>();

            buttonRT.SetParent(BSUI.Content.transform);
            buttonRT.sizeDelta = new Vector2(buttonRT.sizeDelta.x, 18f);
            buttonRT.localScale = Vector3.one;

            buttonI.sprite = RoundedSquare;
            buttonI.color = new Color(1, 1, 1);
            buttonI.type = Image.Type.Sliced;
            buttonI.pixelsPerUnitMultiplier = 8;

            buttonB.colors = new ColorBlock
            {
                normalColor = new Color32(30, 30, 30, 255),
                highlightedColor = new Color32(32, 32, 32, 255),
                pressedColor = new Color32(35, 35, 35, 255),
                selectedColor = new Color32(30, 30, 30, 255),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            buttonB.onClick.AddListener(() =>
            {
                Fade(BSUI.Parent, BUI.Parent);
                CB = buttonB.transform.GetSiblingIndex();

                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(() => { Fade(BUI.Parent, BSUI.Parent); ListBiomes(); CB = -1; });

                BUI.Delete.onClick.RemoveAllListeners();
                BUI.Delete.onClick.AddListener(() => {
                    Fade(BUI.Parent, BSUI.Parent);
                    GD.Biomes.RemoveAt(CB);
                    ListBiomes();
                });
            });

            GameObject buttonText = new GameObject("Text");
            TextMeshProUGUI buttonTextTMP = buttonText.AddComponent<TextMeshProUGUI>();
            RectTransform buttonTextRT = buttonText.GetComponent<RectTransform>();

            buttonTextRT.parent = buttonRT;
            buttonTextRT.position = Vector3.zero;
            buttonTextRT.localScale = Vector3.one;
            buttonTextRT.sizeDelta = new Vector2(100f, 10f);

            buttonTextTMP.text = "Biome #" + i;
            buttonTextTMP.font = OptionFont;
            buttonTextTMP.enableAutoSizing = true;
            buttonTextTMP.fontSizeMin = 0;
            buttonTextTMP.fontSizeMax = 72;
            buttonTextTMP.color = new Color(1, 1, 1);
        }
    }

    #endregion
    #region Heightmaps Listing

    void ListBiomeHeightmaps()
    {
        if (CB == -1) return;

        for (int i = 0; i < HSUI.Content.transform.childCount; i++)
            Destroy(HSUI.Content.transform.GetChild(i).gameObject);

        for (int i = 0; i < GD.Biomes[CB].Heightmaps.Count; i++)
        {
            GameObject button = new GameObject("Heightmap UI Button #" + (i + 1));
            Image buttonI = button.AddComponent<Image>();
            RectTransform buttonRT = button.GetComponent<RectTransform>();
            Button buttonB = button.AddComponent<Button>();

            buttonRT.SetParent(HSUI.Content.transform);
            buttonRT.sizeDelta = new Vector2(buttonRT.sizeDelta.x, 18f);
            buttonRT.localScale = Vector3.one;

            buttonI.sprite = RoundedSquare;
            buttonI.color = new Color(1, 1, 1);
            buttonI.type = Image.Type.Sliced;
            buttonI.pixelsPerUnitMultiplier = 8;

            buttonB.colors = new ColorBlock
            {
                normalColor = new Color32(30, 30, 30, 255),
                highlightedColor = new Color32(32, 32, 32, 255),
                pressedColor = new Color32(35, 35, 35, 255),
                selectedColor = new Color32(30, 30, 30, 255),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            buttonB.onClick.AddListener(() => {
                Fade(HSUI.Parent, HUI.Parent);
                ShowHeightmap(GD.Biomes[CB].Heightmaps[buttonB.transform.GetSiblingIndex()]);

                HUI.Delete.onClick.RemoveAllListeners();
                HUI.Delete.onClick.AddListener(() => { 
                    Fade(HUI.Parent, HSUI.Parent); 
                    GD.Biomes[CB].Heightmaps.RemoveAt(buttonB.transform.GetSiblingIndex()); 
                    ListBiomeHeightmaps(); 
                });
            });

            GameObject buttonText = new GameObject("Text");
            TextMeshProUGUI buttonTextTMP = buttonText.AddComponent<TextMeshProUGUI>();
            RectTransform buttonTextRT = buttonText.GetComponent<RectTransform>();

            buttonTextRT.parent = buttonRT;
            buttonTextRT.position = Vector3.zero;
            buttonTextRT.localScale = Vector3.one;
            buttonTextRT.sizeDelta = new Vector2(100f, 10f);

            buttonTextTMP.text = "Heightmap #" + i;
            buttonTextTMP.font = OptionFont;
            buttonTextTMP.enableAutoSizing = true;
            buttonTextTMP.fontSizeMin = 0;
            buttonTextTMP.fontSizeMax = 72;
            buttonTextTMP.color = new Color(1, 1, 1);
        }
    }

    void ShowHeightmap(GenerateData.HeightmapData heightmapData)
    {
        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(() => { 
            Fade(HUI.Parent, HSUI.Parent);

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(HSUI.Parent, BUI.Parent); });
        });

        HUI.Scale.InitializeArea(heightmapData.Scale);
        HUI.Scale.ValueChanged = (float value) => { heightmapData.Scale = (int)value; };
        HUI.Height.InitializeArea(heightmapData.HeightMultiplier);
        HUI.Height.ValueChanged = (float value) => { heightmapData.HeightMultiplier = (int)value; };
        HUI.Octaves.InitializeArea(heightmapData.Octaves);
        HUI.Octaves.ValueChanged = (float value) => { heightmapData.Octaves = (int)value; };
        HUI.Persistence.InitializeArea(heightmapData.Persistence);
        HUI.Persistence.ValueChanged = (float value) => { heightmapData.Persistence = value; };
        HUI.Lacunarity.InitializeArea(heightmapData.Lacunarity);
        HUI.Lacunarity.ValueChanged = (float value) => { heightmapData.Lacunarity = value; };
    }

    #endregion

    #region Misc

    void ShowProp(int i)
    {
        Fade(PSUI.Parent, PUI.Parent);

        BackButton.onClick.RemoveAllListeners();
        BackButton.onClick.AddListener(() => { Fade(PUI.Parent, PSUI.Parent); });

        PUI.ChunkMin.transform.parent.gameObject.SetActive(true);
        PUI.ChunkMax.transform.parent.gameObject.SetActive(true); 
        PUI.ChanceUse.transform.parent.gameObject.SetActive(false);
        PUI.PerlinMap.transform.parent.gameObject.SetActive(false);
        PUI.PerlinNum.transform.parent.gameObject.SetActive(false);

        PUI.PropGroup.InitializeArea(GD.Biomes[CB].Props[i].GroupId);
        PUI.PropGroup.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].GroupId = (byte)value; };
        PUI.PropID.InitializeArea(GD.Biomes[CB].Props[i].PropId);
        PUI.PropID.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PropId = (byte)value; };

        PUI.xRot.InitializeArea(GD.Biomes[CB].Props[i].Rotation.x);
        PUI.xRot.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Rotation.x = value; };
        PUI.yRot.InitializeArea(GD.Biomes[CB].Props[i].Rotation.y);
        PUI.yRot.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Rotation.y = value; };
        PUI.zRot.InitializeArea(GD.Biomes[CB].Props[i].Rotation.z);
        PUI.zRot.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Rotation.z = value; };

        PUI.MaxScale.InitializeArea(GD.Biomes[CB].Props[i].MaxScale);
        PUI.MaxScale.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].MaxScale = value; };
        PUI.MinScale.InitializeArea(GD.Biomes[CB].Props[i].MinScale);
        PUI.MinScale.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].MinScale = value; };
        PUI.gType.InitializeArea(GD.Biomes[CB].Props[i].GenerateType);
        PUI.gType.ValueChanged = (float value) => {
            GD.Biomes[CB].Props[i].GenerateType = (byte)value;

            if (value == 0)
            {
                PUI.ChunkMin.transform.parent.gameObject.SetActive(true);
                PUI.ChunkMax.transform.parent.gameObject.SetActive(true);
                PUI.ChanceUse.transform.parent.gameObject.SetActive(false);
                PUI.PerlinNum.transform.parent.gameObject.SetActive(false);
                PUI.PerlinMap.transform.parent.gameObject.SetActive(false);
            }
            else if (value == 1)
            {
                PUI.ChunkMin.transform.parent.gameObject.SetActive(false);
                PUI.ChunkMax.transform.parent.gameObject.SetActive(false);
                PUI.ChanceUse.transform.parent.gameObject.SetActive(true);
                PUI.PerlinNum.transform.parent.gameObject.SetActive(false);
                PUI.PerlinMap.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                PUI.ChunkMin.transform.parent.gameObject.SetActive(false);
                PUI.ChunkMax.transform.parent.gameObject.SetActive(false);
                PUI.ChanceUse.transform.parent.gameObject.SetActive(false);
                PUI.PerlinNum.transform.parent.gameObject.SetActive(true);
                PUI.PerlinMap.transform.parent.gameObject.SetActive(true);

                if (GD.Biomes[CB].Props[i].PerlinMap == null)
                {
                    GD.Biomes[CB].Props[i].PerlinMap = new GenerateData.HeightmapData
                    {
                        Scale = 100,
                        HeightMultiplier = 1,
                        Octaves = 2,

                        Persistence = 1,
                        Lacunarity = 1
                    };
                }
            }
        };

        PUI.ChunkMin.InitializeArea(GD.Biomes[CB].Props[i].PerChunkMin);
        PUI.ChunkMin.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerChunkMin = (byte)value; };
        PUI.ChunkMax.InitializeArea(GD.Biomes[CB].Props[i].PerChunkMax);
        PUI.ChunkMax.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerChunkMax = (byte)value; };
        PUI.ChanceUse.InitializeArea(GD.Biomes[CB].Props[i].Chance);
        PUI.ChanceUse.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Chance = (byte)value; };
        PUI.PerlinNum.InitializeArea(GD.Biomes[CB].Props[i].Perlin);
        PUI.PerlinNum.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Perlin = value; };

        PUI.PerlinMap.onClick.RemoveAllListeners();
        PUI.PerlinMap.onClick.AddListener(() =>
        {
            Fade(PUI.Parent, HUI.Parent);

            HUI.Scale.InitializeArea(GD.Biomes[CB].Props[i].PerlinMap.Scale);
            HUI.Scale.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerlinMap.Scale = (int)value; };
            HUI.Height.InitializeArea(GD.Biomes[CB].Props[i].PerlinMap.HeightMultiplier);
            HUI.Height.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerlinMap.HeightMultiplier = (int)value; };
            HUI.Octaves.InitializeArea(GD.Biomes[CB].Props[i].PerlinMap.Octaves);
            HUI.Octaves.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerlinMap.Octaves = (int)value; };
            HUI.Persistence.InitializeArea(GD.Biomes[CB].Props[i].PerlinMap.Persistence);
            HUI.Persistence.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerlinMap.Persistence = value; };
            HUI.Lacunarity.InitializeArea(GD.Biomes[CB].Props[i].PerlinMap.Lacunarity);
            HUI.Lacunarity.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].PerlinMap.Lacunarity = value; };

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() => { Fade(HUI.Parent, PUI.Parent); });
        });

        PUI.xBounds.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Bounds = new Vector2(GD.Biomes[CB].Props[i].Bounds.x, value); };
        PUI.yBounds.ValueChanged = (float value) => { GD.Biomes[CB].Props[i].Bounds = new Vector2(value, GD.Biomes[CB].Props[i].Bounds.y); };

        PUI.Delete.onClick.RemoveAllListeners();
        PUI.Delete.onClick.AddListener(() => { Fade(PUI.Parent, PSUI.Parent); GD.Biomes[CB].Props.RemoveAt(i); ListBiomeProps(); });

        PUI.ViewProps.onClick.RemoveAllListeners();
        PUI.ViewProps.onClick.AddListener(() =>
        {
            Fade(PUI.Parent, PLUI);

            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(() =>
            {
                Fade(PLUI, PUI.Parent);

                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(() => { Fade(PUI.Parent, PSUI.Parent); });
            });
        });
    }

    public void Fade(RectTransform from, RectTransform to) { to.gameObject.SetActive(true); from.gameObject.SetActive(false); }

    string FindPropName(int prop)
    {
        if (CB == -1) return "";

        for (int i = 0; i < MapPreview.instance.PropsData.Length; i++)
        {
            if (MapPreview.instance.PropsData[i].Group == GD.Biomes[CB].Props[prop].GroupId &&
                MapPreview.instance.PropsData[i].ID == GD.Biomes[CB].Props[prop].PropId)
            {
                return MapPreview.instance.PropsData[i].PropName;
            }
        }

        return "";
    }

    #endregion
    #region UI Structs

    [System.Serializable]
    public struct CUIStruct
    {
        public RectTransform Parent;
        public Button Biomes;
        public Button BiomeMap;
    }

    [System.Serializable]
    public struct BSUIStruct
    {
        public RectTransform Parent;
        public GameObject Content;
        public Button AddBiome;
    }

    [System.Serializable]
    public struct BUIStruct
    {
        public RectTransform Parent;
        public Button Delete;

        [Space]

        public Button Heightmap;
        public Button Props;
    }

    [System.Serializable]
    public struct HSUIStruct
    {
        public RectTransform Parent;
        public GameObject Content;
        public Button AddHeightmap;
    }

    [System.Serializable]
    public struct HUIStruct
    {
        public RectTransform Parent;
        public Button Delete;

        [Space]

        public TextArea Scale;
        public TextArea Height;
        public TextArea Octaves;

        [Space]

        public TextArea Persistence;
        public TextArea Lacunarity;
    }

    [System.Serializable]
    public struct PSUIStruct
    {
        public RectTransform Parent;
        public GameObject Content;
        public Button AddPropButton;
    }

    [System.Serializable]
    public struct PUIStruct
    {
        public RectTransform Parent;
        public Button Delete;

        [Space]

        public TextArea PropGroup;
        public TextArea PropID;
        public Button ViewProps;

        [Space]

        public TextArea xRot;
        public TextArea yRot;
        public TextArea zRot;

        [Space]

        public TextArea MaxScale;
        public TextArea MinScale;

        [Space]

        public TextArea gType;

        [Space]

        public TextArea ChunkMin;
        public TextArea ChunkMax;
        public TextArea ChanceUse;
        public TextArea PerlinNum;
        public Button PerlinMap;

        [Space]

        public TextArea xBounds;
        public TextArea yBounds;
    }

    #endregion
}
