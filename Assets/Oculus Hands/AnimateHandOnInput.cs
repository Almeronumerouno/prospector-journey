using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{

    public InputActionProperty triggerValue;
    public InputActionProperty gripValue;

    public Animator handAnimator;

    private void Awake()
    {
        if (handAnimator == null)
        {
            handAnimator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        if (triggerValue.action != null && !triggerValue.action.enabled)
        {
            triggerValue.action.Enable();
        }
        if (gripValue.action != null && !gripValue.action.enabled)
        {
            gripValue.action.Enable();
        }
    }

    private void Update()
    {
        if (handAnimator == null) return;

        float trigger = 0f;
        if (triggerValue.action != null)
        {
            trigger = triggerValue.action.ReadValue<float>();
        }

        float grip = 0f;
        if (gripValue.action != null)
        {
            grip = gripValue.action.ReadValue<float>();
        }

        handAnimator.SetFloat("Trigger", trigger);
        handAnimator.SetFloat("Grip", grip);
    }
}
