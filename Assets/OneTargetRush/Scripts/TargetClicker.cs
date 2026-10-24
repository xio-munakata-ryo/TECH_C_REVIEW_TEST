// TargetClicker.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetClicker : MonoBehaviour
{
    private Camera mainCamera;

    private InputAction inputAction;

    private PlayerInput input;

    private void Start()
    {
        mainCamera = Camera.main;
        inputAction = input.currentActionMap["Attack"];
    }

    private void Update()
    {
        if (inputAction.WasPressedThisFrame())
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent<TargetObject>(out TargetObject target))
                {
                    target.OnHit();
                }
            }
        }
    }
}