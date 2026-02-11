using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float rotationSpeed = 450;
    public float walkSpeed = 5;
    public float runSpeed = 10;

    private Quaternion targetRotation;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction runAction;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        var playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
    }

    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 input = new Vector3(moveInput.x, 0, moveInput.y);

        if (input.magnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(
                transform.eulerAngles.y,
                targetRotation.eulerAngles.y,
                rotationSpeed * Time.deltaTime
            );
        }

        bool isRunning = runAction.ReadValue<float>() > 0.5f;
        Vector3 motion = input.normalized;
        motion *= isRunning ? runSpeed : walkSpeed;
        motion += Vector3.up * -8;

        controller.Move(motion * Time.deltaTime);
    }
}