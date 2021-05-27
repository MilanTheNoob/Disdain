using UnityEngine;

public class BuildPreview : MonoBehaviour
{
    public StructureType structureType;

    bool isSnapped;
    GameObject snappedTo;

    public enum StructureType
    {
        Foundation,
        Wall,
        Furniture,
        Storage
    }

    void Start()
    {
        isSnapped = true;
        if (gameObject.GetComponent<Collider>()) { gameObject.AddComponent<BoxCollider>(); }

        HighlightManager.Highlight(gameObject);

        Transform[] children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) { children[i].gameObject.layer = 10; }

        transform.eulerAngles = Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (structureType == StructureType.Foundation && other.CompareTag("Foundation_SP") || structureType == StructureType.Wall && other.CompareTag("Wall_SP")) 
        {
            Snapped(other.transform.position);
            snappedTo = other.gameObject;
        }
    }
    void OnTriggerExit(Collider other) { if (other.name == snappedTo.name) { snappedTo = null; } }

    void Snapped(Vector3 pos)
    {
        BuildingManager.PauseBuild(true);

        transform.position = pos;
        isSnapped = true;
    }

    public bool Place()
    {
        if (structureType == StructureType.Storage) { gameObject.AddComponent<Storage_InteractableItem>(); } else { /*gameObject.AddComponent<Structure_InteractableItem>();*/ }
        TerrainGenerator.AddToNearestChunk(gameObject, TerrainGenerator.ChildType.Structure);

        HighlightManager.Remove(gameObject);

        Destroy(this);
        Destroy(snappedTo);
        return true;
    }

    public bool GetSnapped() { return isSnapped; }
}
