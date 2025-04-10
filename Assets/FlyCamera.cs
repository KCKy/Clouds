using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float boostMultiplier = 3f;
    public float lookSpeed = 0.4f;

    float _yaw;
    float _pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.mousePositionDelta.x;
            float mouseY = Input.mousePositionDelta.y;
            _yaw += mouseX * lookSpeed;
            _pitch -= mouseY * lookSpeed;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);

            transform.eulerAngles = new(_pitch, _yaw, 0f);
        }

        if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Cursor.lockState == CursorLockMode.Locked && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;
        if (Input.GetKey(KeyCode.E)) direction += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) direction += Vector3.down;

        transform.Translate(speed * Time.deltaTime * direction.normalized, Space.Self);
    }
}