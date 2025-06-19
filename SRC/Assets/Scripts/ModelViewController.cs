using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ModelViewController : MonoBehaviour
{
    [Header("Kontrol Ayarlarý")]
    public float rotationSpeed = 30f;
    public float zoomSpeed = 0.75f;
    public float minZoomDistance = 0.5f;
    public float maxZoomDistance = 10f;
    public Vector3 initialModelRotation = Vector3.zero; // Modelin baþlangýç lokal rotasyonu
    public Vector3 initialCameraOffsetFromModel = new Vector3(0, 0.75f, -2.5f); // Kameranýn modele göre baþlangýç ofseti

    [HideInInspector] public Camera viewerCamera; // AR_Experience_Manager tarafýndan atanacak
    private Transform targetModel; // Bu scriptin altýndaki ilk çocuk olacak

    private Vector2 lastPointerPosition;
    private bool isRotating = false;
    private float currentTargetZoomDistance;

    void Awake()
    {
        // targetModel'i bu GameObject'in ilk çocuðu olarak bul
        if (transform.childCount > 0)
        {
            targetModel = transform.GetChild(0);
            Debug.Log("ModelViewController: Target Model otomatik olarak ilk çocuk (" + targetModel.name + ") olarak ayarlandý.");
        }
        else
        {
            Debug.LogError("ModelViewController: Kontrol edilecek model (targetModel) ModelViewerAnchor'ýn çocuðu olarak bulunamadý! Lütfen modeli ModelViewerAnchor altýna yerleþtirin.");
            enabled = false; // Model yoksa scripti devre dýþý býrak
            return;
        }
    }

    // AR_Experience_Manager modu deðiþtirdiðinde ve bu scripti enable ettiðinde çaðrýlabilir
    // veya doðrudan AR_Experience_Manager tarafýndan çaðrýlýr.
    void OnEnable()
    {
        // viewerCamera ve targetModel atanmýþsa ve kamera aktifse görünümü sýfýrla
        if (targetModel != null && viewerCamera != null && viewerCamera.gameObject.activeInHierarchy)
        {
            ResetView();
        }
    }

    public void ResetView()
    {
        if (targetModel == null || viewerCamera == null)
        {
            Debug.LogWarning("ModelViewController.ResetView: targetModel veya viewerCamera atanmamýþ/aktif deðil.");
            return;
        }

        // Kamerayý modelin baþlangýç pozisyonuna ve açýsýna ayarla
        // initialCameraOffsetFromModel'i targetModel'in dünya rotasyonuna göre deðil,
        // targetModel'in parent'ýnýn (ModelViewerAnchor) rotasyonuna göre veya dünya koordinatlarýna göre uygula.
        // Eðer ModelViewerAnchor hep (0,0,0) rotasyonunda ise, direkt toplamak iþe yarar.
        viewerCamera.transform.position = targetModel.position + initialCameraOffsetFromModel; // Daha basit ofset
        viewerCamera.transform.LookAt(targetModel.position);
        currentTargetZoomDistance = Vector3.Distance(viewerCamera.transform.position, targetModel.position);

        targetModel.localEulerAngles = initialModelRotation;
        isRotating = false;
        Debug.Log("ModelViewController: View Resetted. Camera Pos: " + viewerCamera.transform.position + " Target: " + targetModel.name);
    }

    void Update()
    {
        if (targetModel == null || viewerCamera == null || !this.enabled || !viewerCamera.gameObject.activeInHierarchy) return;

        HandleCameraRotation();
        HandleCameraZoom();
    }

    void HandleCameraRotation()
    {
        bool wantsToRotate = false;
        Vector2 currentInputPosition = Vector2.zero;
        Vector2 dragDelta = Vector2.zero;

        if (Mouse.current != null && Mouse.current.deviceId != InputDevice.InvalidDeviceId)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                isRotating = true;
                lastPointerPosition = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isRotating = false;
            }
            if (isRotating && Mouse.current.leftButton.isPressed)
            {
                wantsToRotate = true;
                currentInputPosition = Mouse.current.position.ReadValue();
                dragDelta = currentInputPosition - lastPointerPosition;
                lastPointerPosition = currentInputPosition;
            }
        }
        else if (Touchscreen.current != null && Touchscreen.current.deviceId != InputDevice.InvalidDeviceId && Touchscreen.current.primaryTouch.isInProgress)
        {
            TouchControl primaryTouch = Touchscreen.current.primaryTouch;
            UnityEngine.InputSystem.TouchPhase touchPhase = primaryTouch.phase.ReadValue();
            if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                isRotating = true;
                lastPointerPosition = primaryTouch.position.ReadValue();
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Ended || touchPhase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                isRotating = false;
            }
            else if (isRotating && touchPhase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                wantsToRotate = true;
                dragDelta = primaryTouch.delta.ReadValue(); // Dokunmatik için delta daha direkt olabilir
            }
        }

        if (wantsToRotate && dragDelta.sqrMagnitude > 0.01f)
        {
            viewerCamera.transform.RotateAround(targetModel.position, viewerCamera.transform.up, -dragDelta.x * rotationSpeed * Time.deltaTime);
            viewerCamera.transform.RotateAround(targetModel.position, viewerCamera.transform.right, dragDelta.y * rotationSpeed * Time.deltaTime);
            viewerCamera.transform.LookAt(targetModel.position);
        }
    }

    void HandleCameraZoom()
    {
        float scrollInput = 0f;
        if (Mouse.current != null && Mouse.current.deviceId != InputDevice.InvalidDeviceId)
        {
            scrollInput = Mouse.current.scroll.ReadValue().y * 0.1f;
        }
        if (Touchscreen.current != null && Touchscreen.current.deviceId != InputDevice.InvalidDeviceId && Touchscreen.current.touches.Count >= 2)
        {
            TouchControl touchZero = Touchscreen.current.touches[0];
            TouchControl touchOne = Touchscreen.current.touches[1];
            if ((touchZero.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || touchZero.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began) &&
                (touchOne.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || touchOne.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
            {
                Vector2 touchZeroPrevPos = touchZero.position.ReadValue() - touchZero.delta.ReadValue();
                Vector2 touchOnePrevPos = touchOne.position.ReadValue() - touchOne.delta.ReadValue();
                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position.ReadValue() - touchOne.position.ReadValue()).magnitude;
                float difference = currentMagnitude - prevMagnitude;
                scrollInput += difference * 0.005f;
            }
        }

        if (Mathf.Abs(scrollInput) > 0.0001f)
        {
            currentTargetZoomDistance = Mathf.Clamp(currentTargetZoomDistance - scrollInput * zoomSpeed, minZoomDistance, maxZoomDistance);
            Vector3 directionFromTargetToCamera = (viewerCamera.transform.position - targetModel.position).normalized;
            if (directionFromTargetToCamera == Vector3.zero) directionFromTargetToCamera = -viewerCamera.transform.forward;
            if (directionFromTargetToCamera == Vector3.zero) directionFromTargetToCamera = -Vector3.forward;
            viewerCamera.transform.position = targetModel.position + directionFromTargetToCamera * currentTargetZoomDistance;
        }
    }
}