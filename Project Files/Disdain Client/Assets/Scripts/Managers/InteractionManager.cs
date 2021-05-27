using UnityEngine.UI;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    #region Singleton

    // The singleton var
    public static InteractionManager instance;

    // Awake is called before Start
    void Awake()
    {
        // Set the singleton to us
        instance = this;
    }

    #endregion

    public static bool CanInteract;

    public float raycastDistance;
    public float raycastSphereRadius;
    public LayerMask interactableLayer;

    public enum InteractableType
    {
        Object,
        Tree,
        Door,
        Trader
    }

    [HideInInspector]
    public Camera interactCamera;

    InteractableItem interactableI;

    void Start()
    {
        GameManager.instance.Mobile.InteractButton.gameObject.SetActive(false);
        GameManager.instance.Mobile.InteractButton.onClick.AddListener(Interact);
        interactCamera = FindObjectOfType<Camera>();

        CanInteract = false;
    }

    void Interact()
    {
        if (interactableI != null)
        {
            interactableI.OnInteract();
            HighlightManager.Restore(interactableI.gameObject);
        }
    }

    void Update()
    {
        if (interactCamera == null)
        {
            interactCamera = FindObjectOfType<Camera>();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E) && CanInteract && interactableI != null)
            {
                interactableI.OnInteract();
                interactableI = null;

                try { HighlightManager.Restore(interactableI.gameObject); } catch { }
                TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.1f);
            }

            if (interactCamera != null)
            {
                Ray ray = new Ray(interactCamera.transform.position, interactCamera.transform.forward);
                RaycastHit hitInfo;

                bool hitInteractableObject = Physics.SphereCast(ray, raycastSphereRadius, out hitInfo, raycastDistance, interactableLayer);
                if (hitInteractableObject && interactableI == null)
                {
                    interactableI = hitInfo.transform.GetComponent<InteractableItem>();
                    if (interactableI.toolType == GameManager.ActivePlayerManager.currentToolType && interactableI.isInteractable || interactableI.toolType == PlayerManager.ToolType.None && interactableI.isInteractable)
                    {
                        CanInteract = true;
                        TweeningLibrary.FadeIn(GameManager.instance.Mobile.InteractButton.gameObject, 0.1f);
                        HighlightManager.Highlight(interactableI.gameObject);
                    }
                }
                else if (!hitInteractableObject && interactableI != null)
                {
                    CanInteract = false;
                    HighlightManager.Restore(interactableI.gameObject);
                    interactableI = null;

                    TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.1f);
                }
            }
        }
    }
}
