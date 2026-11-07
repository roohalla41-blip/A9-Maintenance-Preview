using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCityCameraController : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float fastMoveMultiplier = 2f;
    public float rotateSpeed = 3f;
    public float zoomSpeed = 4f;
    public float minDistance = 7f;
    public float maxDistance = 35f;
    public float minPitch = 18f;
    public float maxPitch = 78f;

    private Vector3 target;
    private float distance = 23f;
    private float yaw;
    private float pitch = 48f;

    public void SetInitialView(Vector3 lookAt, float startDistance, float startPitch, float startYaw)
    {
        target = lookAt;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        pitch = Mathf.Clamp(startPitch, minPitch, maxPitch);
        yaw = startYaw;
        UpdateCameraPose();
    }

    private void Start()
    {
        if (transform.position.sqrMagnitude > 0.01f)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = Mathf.Clamp(angles.x, minPitch, maxPitch);
        }
        UpdateCameraPose();
    }

    private void LateUpdate()
    {
        MoveTarget();
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed * 35f;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed * 25f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed, minDistance, maxDistance);
        UpdateCameraPose();
    }

    private void MoveTarget()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMoveMultiplier : 1f);
        Quaternion heading = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = heading * Vector3.forward;
        Vector3 right = heading * Vector3.right;
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement += forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement -= forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement += right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement -= right;
        target += movement.normalized * speed * Time.deltaTime;
        target.x = Mathf.Clamp(target.x, -22f, 22f);
        target.z = Mathf.Clamp(target.z, -22f, 22f);
    }

    private void UpdateCameraPose()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target + rotation * new Vector3(0f, 0f, -distance);
        transform.rotation = rotation;
    }
}