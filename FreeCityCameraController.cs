using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCityCameraController : MonoBehaviour
{
    [Header("حرکت")]
    public float moveSpeed = 12f;
    public float fastMoveMultiplier = 2f;

    [Header("چرخش و زوم")]
    public float rotateSpeed = 3.0f;
    public float zoomSpeed = 4f;
    public float minDistance = 7f;
    public float maxDistance = 35f;
    public float minPitch = 18f;
    public float maxPitch = 78f;

    private Vector3 target = Vector3.zero;
    private float distance = 23f;
    private float yaw = 0f;
    private float pitch = 48f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        distance = Vector3.Distance(transform.position, target);
        UpdateCamera();
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

        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed,
            minDistance, maxDistance);

        UpdateCamera();
    }

    private void MoveTarget()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMoveMultiplier : 1f);
        Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, yaw, 0) * Vector3.right;
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement += forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement -= forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement += right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement -= right;

        target += movement.normalized * speed * Time.deltaTime;
        target.x = Mathf.Clamp(target.x, -22f, 22f);
        target.z = Mathf.Clamp(target.z, -22f, 22f);
    }

    private void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target + offset;
        transform.rotation = rotation;
    }
}
