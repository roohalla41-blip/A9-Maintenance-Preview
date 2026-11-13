using UnityEngine;

/// <summary>
/// Mobile-first strategy camera: one-finger orbit, two-finger pan/pinch,
/// desktop keyboard navigation and mouse orbit/zoom.
/// </summary>
[RequireComponent(typeof(Camera))]
public sealed class FreeCityCameraController : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 12f;
    [Min(1f)] public float fastMoveMultiplier = 2f;
    [Min(0f)] public float rotateSpeed = 0.12f;
    [Min(0f)] public float panSpeed = 0.018f;
    [Min(0f)] public float zoomSpeed = 0.025f;

    [Header("Camera Limits")]
    [Min(1f)] public float minDistance = 7f;
    [Min(1f)] public float maxDistance = 45f;
    [Range(5f, 85f)] public float minPitch = 18f;
    [Range(10f, 89f)] public float maxPitch = 78f;
    public Vector2 worldBounds = new Vector2(60f, 60f);

    [Header("Smoothing")]
    [Range(0f, 30f)] public float positionSmoothing = 18f;
    [Range(0f, 30f)] public float rotationSmoothing = 18f;

    private Vector3 target;
    private float distance = 23f;
    private float yaw;
    private float pitch = 48f;
    private Vector3 desiredPosition;
    private Quaternion desiredRotation;
    private Vector2 previousTouch;
    private Vector2 previousPinchCenter;
    private float previousPinchDistance;
    private bool initialized;
    private bool singleTouchStartedOverHud;

    public void SetInitialView(Vector3 lookAt, float startDistance, float startPitch, float startYaw)
    {
        target = lookAt;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        pitch = Mathf.Clamp(startPitch, minPitch, maxPitch);
        yaw = startYaw;
        UpdateDesiredPose();
        transform.SetPositionAndRotation(desiredPosition, desiredRotation);
        initialized = true;
    }

    private void Start()
    {
        if (!initialized)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = Mathf.Clamp(angles.x, minPitch, maxPitch);
            if (transform.position.sqrMagnitude > 0.01f)
                target = transform.position + transform.forward * distance;
            UpdateDesiredPose();
        }
    }

    private void LateUpdate()
    {
        HandleKeyboard();
        HandleTouch();
        HandleMouse();
        ClampTarget();
        UpdateDesiredPose();

        float positionFactor = 1f - Mathf.Exp(-positionSmoothing * Time.deltaTime);
        float rotationFactor = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationFactor);
    }

    private void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                previousTouch = touch.position;
                singleTouchStartedOverHud = MobileUiLayout.IsHudPosition(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.position - previousTouch;
                if (!singleTouchStartedOverHud &&
                    !MobileUiLayout.IsHudPosition(touch.position))
                {
                    yaw += delta.x * rotateSpeed;
                    pitch = Mathf.Clamp(pitch - delta.y * rotateSpeed * 0.7f, minPitch, maxPitch);
                }
                previousTouch = touch.position;
            }
            previousPinchDistance = 0f;
            return;
        }

        if (Input.touchCount >= 2)
        {
            Touch a = Input.GetTouch(0);
            Touch b = Input.GetTouch(1);
            if (MobileUiLayout.IsHudPosition(a.position) ||
                MobileUiLayout.IsHudPosition(b.position))
            {
                previousPinchDistance = 0f;
                return;
            }
            Vector2 center = (a.position + b.position) * 0.5f;
            float pinch = Vector2.Distance(a.position, b.position);

            if (previousPinchDistance > 0f)
            {
                float pinchDelta = pinch - previousPinchDistance;
                distance = Mathf.Clamp(distance - pinchDelta * zoomSpeed, minDistance, maxDistance);

                Vector2 centerDelta = center - previousPinchCenter;
                Pan(centerDelta);
            }

            previousPinchDistance = pinch;
            previousPinchCenter = center;
            return;
        }

        previousPinchDistance = 0f;
    }

    private void Pan(Vector2 screenDelta)
    {
        Quaternion heading = Quaternion.Euler(0f, yaw, 0f);
        Vector3 right = heading * Vector3.right;
        Vector3 forward = heading * Vector3.forward;
        float scale = panSpeed * Mathf.Max(1f, distance);
        target += (-right * screenDelta.x - forward * screenDelta.y) * scale;
    }

    private void HandleKeyboard()
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
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed * 8f;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * rotateSpeed * 6f, minPitch, maxPitch);
        }
        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed * 40f,
            minDistance, maxDistance);
    }

    private void ClampTarget()
    {
        target.x = Mathf.Clamp(target.x, -worldBounds.x, worldBounds.x);
        target.z = Mathf.Clamp(target.z, -worldBounds.y, worldBounds.y);
    }

    private void UpdateDesiredPose()
    {
        desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        desiredPosition = target + desiredRotation * new Vector3(0f, 0f, -distance);
    }
}
