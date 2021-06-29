using UnityEngine;

public class ItemBuildPreview : MonoBehaviour
{
    public bool Place(bool ignoreGravity)
    {
        if (!ignoreGravity) { gameObject.AddComponent<Rigidbody>(); }
        TerrainGenerator.AddToNearestChunk(gameObject, ChildTypeEnum.Misc);

        Destroy(this);
        return true;
    }
}
