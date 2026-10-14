using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProspectorJourney.UI
{
    [RequireComponent(typeof(Selectable))]
    public class VRButtonAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        private MainMenuVRController mainMenuController;
        private ProspectorJourney.Room.ProspectorInventory inventoryController;
        private PauseMenuController pauseMenuController;
        private RectTransform rectTransform;
        private Vector3 originalScale;

        private void Awake()
        {
            mainMenuController = GetComponentInParent<MainMenuVRController>();
            inventoryController = GetComponentInParent<ProspectorJourney.Room.ProspectorInventory>();
            pauseMenuController = GetComponentInParent<PauseMenuController>();
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalScale = rectTransform.localScale;
            }
        }

        private static float s_LastHoverTime;
        private const float MinHoverInterval = 0.12f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Jangan bunyikan hover sound kalau canvas menu lagi melayang/nengok (catching-up)
            if ((mainMenuController != null && mainMenuController.IsMenuMoving) ||
                (inventoryController != null && inventoryController.IsMenuMoving) ||
                (pauseMenuController != null && pauseMenuController.IsMenuMoving))
            {
                return;
            }

            // Debounce agar tidak ada suara hover beruntun (spam) dalam interval singkat
            if (Time.unscaledTime - s_LastHoverTime < MinHoverInterval)
            {
                return;
            }

            s_LastHoverTime = Time.unscaledTime;

            if (mainMenuController != null)
            {
                mainMenuController.PlayHoverSound();
            }
            else if (pauseMenuController != null)
            {
                pauseMenuController.PlayHoverSound();
            }
            else if (inventoryController != null)
            {
                inventoryController.PlayHoverSound();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (mainMenuController != null)
            {
                mainMenuController.PlayClickSound();
            }
            else if (pauseMenuController != null)
            {
                pauseMenuController.PlayClickSound();
            }
            else if (inventoryController != null)
            {
                inventoryController.PlayClickSound();
            }
        }
    }
}
