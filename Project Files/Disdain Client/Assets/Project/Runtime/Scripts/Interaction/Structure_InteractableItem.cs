public class Structure_InteractableItem : InteractableItem
{
    void Awake() { interactTxt = "Demolish"; toolType = PlayerManager.ToolType.Hammer; }

    public override void OnInteract()
    {
        for (int i = 0; i < DataManager.SaveFile.Chunks[DataManager.SaveFile.FindChunk(transform.position)].PropTypes[5].Props.Count; i++) 
        { 
            if (DataManager.SaveFile.Chunks[DataManager.SaveFile.FindChunk(transform.position)].PropTypes[5].Props[i].Pos == transform.position) 
            { 
                DataManager.SaveFile.Chunks[DataManager.SaveFile.FindChunk(transform.position)].PropTypes[5].Props.RemoveAt(i); 
            } 
        }
        Destroy(gameObject);
    }
}
