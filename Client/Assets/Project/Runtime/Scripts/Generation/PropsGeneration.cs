using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropsGeneration : MonoBehaviour
{
    public static List<PoolTypeClass> Pools = new List<PoolTypeClass>();
    public static PropSettingsStruct PropSettings;

    #region Init

    public static void Init(GameObject holder, PropSettingsStruct propSettings)
    {
        PropSettings = propSettings;

        if (DataManager.GameState == GameStateEnum.Singleplayer ||
            DataManager.GameState == GameStateEnum.Multiplayer)
        {
            for (int c = 0; c < PropSettings.PropTypes.Length; c++)
            {
                PoolTypeClass Pool = new PoolTypeClass
                {
                    ID = c,
                    Holder = new GameObject(PropSettings.PropTypes[c].ID + " Pools"),
                    PropPools = new PoolClass[PropSettings.PropTypes[c].Props.Length]
                };

                Pool.Holder.transform.parent = holder.transform;

                for (int i = 0; i < PropSettings.PropTypes[c].Props.Length; i++)
                {
                    Pool.PropPools[i] = new PoolClass
                    {
                        ID = PropSettings.PropTypes[c].Props[i].ID,
                        PrefabHolders = new GameObject[PropSettings.PropTypes[c].Props[i].Prefabs.Length],

                        Holder = new GameObject(i.ToString())
                    };
                    Pool.PropPools[i].Holder.transform.parent = Pool.Holder.transform;

                    for (int j = 0; j < PropSettings.PropTypes[c].Props[i].Prefabs.Length; j++)
                    {
                        GameObject prefabParent = new GameObject(i + "," + j);
                        prefabParent.transform.parent = Pool.PropPools[i].Holder.transform;
                        Pool.PropPools[i].PrefabHolders[j] = prefabParent;

                        for (int b = 0; b < PropSettings.PoolSizes; b++)
                        {
                            GameObject prefab = Instantiate(PropSettings.PropTypes[c].Props[i].Prefabs[j],
                                Vector3.zero, Quaternion.identity, Pool.PropPools[i].PrefabHolders[j].transform);
                            prefab.transform.localScale = Vector3.one;
                            prefab.transform.name = i + "," + j;
                            prefab.SetActive(false);
                        }
                    }
                }

                Pools.Add(Pool);
            }
        }
    }

    #endregion
    #region Generation

    public static void Generate(TerrainChunk chunk)
    {
        chunk.PropParents = new GameObject[chunk.ChunkData.PropTypes.Count];

        for (int i = 0; i < chunk.ChunkData.PropTypes.Count; i++)
        {
            chunk.PropParents[i] = new GameObject("Category - " + PropSettings.PropTypes[i].ID);

            chunk.PropParents[i].transform.parent = chunk.ChunkObject.transform;
            chunk.PropParents[i].transform.localPosition = Vector3.zero;

            for (int j = 0; j < chunk.ChunkData.PropTypes[i].Props.Count; j++)
            {
                GameObject prop = Pools[i].PropPools[chunk.ChunkData.PropTypes[i].Props[j].ID].PrefabHolders
                    [Random.Range(0, Pools[i].PropPools[chunk.ChunkData.PropTypes[i].Props[j].ID].PrefabHolders.Length)].
                    transform.GetChild(0).gameObject;
                prop.transform.parent = chunk.PropParents[i].transform;

                prop.transform.localPosition = chunk.ChunkData.PropTypes[i].Props[j].Pos;
                prop.transform.eulerAngles = chunk.ChunkData.PropTypes[i].Props[j].Euler;
                prop.transform.localScale = chunk.ChunkData.PropTypes[i].Props[j].Scale;
                prop.SetActive(true);
            }
        }

        // TODO: Storage
    }

    public static void Clear(TerrainChunk chunk)
    {
        for (int i = 0; i < chunk.PropParents.Length; i++)
        {
            for (int j = 0; j < chunk.PropParents[i].transform.childCount; j++)
            {
                GameObject prop = chunk.PropParents[i].transform.GetChild(j).gameObject;
                string[] nValues = prop.name.Split(',');

                prop.transform.parent = Pools[i].PropPools[int.Parse(nValues[0])].PrefabHolders[int.Parse(nValues[1])].transform;
                prop.transform.localPosition = Vector3.zero;
                prop.transform.eulerAngles = Vector3.zero;
                prop.SetActive(false);
            }
        }
        
        // TODO: Storage
    }

    #endregion
    #region Misc

    #endregion

    #region Local Classes

    public class PoolTypeClass
    {
        public int ID;
        public GameObject Holder;
        public PoolClass[] PropPools;
    }

    public class PoolClass
    {
        public int ID;
        public GameObject Holder;
        public GameObject[] PrefabHolders;
    }

    #endregion
}