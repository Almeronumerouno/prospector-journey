using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProspectorJourney.UI
{
    [RequireComponent(typeof(Selectable))]
    public class VRButtonAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        private MainMenuVRController controller;
        private RectTransform rectTransform;
        private Vector3 originalScale;

        private void Awake()
        {
            controller = GetComponentInParent<MainMenuVRController>();
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
            if (controller != null && controller.IsMenuMoving)
            {
                return;
            }

            // Debounce agar tidak ada suara hover beruntun (spam) dalam interval singkat
            if (Time.unscaledTime - s_LastHoverTime < MinHoverInterval)
            {
                return;
            }

            s_LastHoverTime = Time.unscaledTime;

            if (controller != null)
            {
                controller.PlayHoverSound();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller != null)
            {
                controller.PlayClickSound();
            }
        }
    }
}
