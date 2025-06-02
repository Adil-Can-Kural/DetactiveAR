using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation; // ARPlane, ARRaycastManager i�in
using UnityEngine.XR.ARSubsystems; // TrackableType, ARSessionState i�in
using UnityEngine.InputSystem;   // InputActionReference i�in

public class ARPlaceObjectOnPlane : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public GameObject objectToPlacePrefab;    // Inspector'dan atanacak model prefab�
    public ARRaycastManager raycastManager;   // Sahnedeki ARRaycastManager

    [Header("Giri� Ayarlar� (Input System)")]
    [Tooltip("Genel t�klama/dokunma Action'�n� (�rn: DefaultInputActions -> UI -> Click VEYA kendi olu�turdu�unuz BirincilDokunma) buraya atay�n.")]
    public InputActionReference placeObjectAction; // T�klama/Dokunma eylemi i�in referans

    [Header("Opsiyonel Ayarlar")]
    public ARPlaneManager planeManager;         // D�zlem g�rsellerini gizlemek i�in
    public bool hidePlanesAfterPlacement = true; // Model yerle�tirildikten sonra d�zlemleri gizle

    private GameObject spawnedObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Pozisyon i�in ayr� bir Action (En iyi pratik)
    // Bunu da Inspector'dan ataman�z ve Input Actions Asset'inizde <Pointer>/position binding'i olan
    // bir Vector2 Value Action olu�turman�z gerekir.
    [Header("Giri� Pozisyonu (�nerilen)")]
    [Tooltip("<Pointer>/position binding'i olan bir Vector2 Value Action'�n� buraya atay�n.")]
    public InputActionReference pointerPositionAction;


    void Awake()
    {
        Debug.Log("ARPlaceObjectOnPlane Awake �a�r�ld�.");

        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            if (raycastManager == null) Debug.LogError("ARRaycastManager sahnede bulunamad�! Bu script d�zg�n �al��mayacak.");
        }
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            // PlaneManager opsiyonel oldu�u i�in hata logu �art de�il.
        }

        // --- INPUT ACTION KURULUMU ---
        if (placeObjectAction == null || placeObjectAction.action == null)
        {
            Debug.LogError("ARPlaceObjectOnPlane: 'Place Object Action' atanmam�� veya Action null! L�tfen Inspector'dan atay�n. T�klama/dokunma �al��mayacak.");
            enabled = false; // Scripti devre d��� b�rakabiliriz
            return;
        }
        // �ste�e ba�l�: E�er pozisyon i�in ayr� bir Action kullanm�yorsan�z, a�a��daki logu ve enabled = false sat�r�n� yorum sat�r� yap�n.
        if (pointerPositionAction == null || pointerPositionAction.action == null)
        {
            Debug.LogWarning("ARPlaceObjectOnPlane: 'Pointer Position Action' atanmam��. Pozisyon almak i�in do�rudan cihaz sorgusu kullan�lacak (daha az g�venilir olabilir).");
            // enabled = false; // E�er bu Action zorunluysa scripti devre d��� b�rak
        }

        // Action'lar� etkinle�tir
        placeObjectAction.action.Enable();
        if (pointerPositionAction != null && pointerPositionAction.action != null)
        {
            pointerPositionAction.action.Enable();
        }
        Debug.Log("ARPlaceObjectOnPlane: Input Action(lar) etkinle�tirildi.");
    }

    void OnEnable()
    {
        // 'performed' olay�na abone ol
        if (placeObjectAction != null && placeObjectAction.action != null)
        {
            placeObjectAction.action.performed += OnPlaceObjectInputTriggered;
            Debug.Log("ARPlaceObjectOnPlane: 'placeObjectAction.performed' olay�na abone olundu.");
        }
    }

    void OnDisable()
    {
        // Abonelikten ��k
        if (placeObjectAction != null && placeObjectAction.action != null)
        {
            placeObjectAction.action.performed -= OnPlaceObjectInputTriggered;
            Debug.Log("ARPlaceObjectOnPlane: 'placeObjectAction.performed' olay�ndan abonelik kald�r�ld�.");
            // Action'lar� burada disable etmeye genellikle gerek yoktur,
            // �zellikle de ba�ka scriptler ayn� Action'� kullan�yorsa.
            // placeObjectAction.action.Disable();
            // if (pointerPositionAction != null && pointerPositionAction.action != null) pointerPositionAction.action.Disable();
        }
    }

    private void OnPlaceObjectInputTriggered(InputAction.CallbackContext context)
    {
        if (!this.enabled || objectToPlacePrefab == null) // Script aktif de�ilse veya prefab yoksa ��k
        {
            if (objectToPlacePrefab == null) Debug.LogWarning("ARPlaceObjectOnPlane: Yerle�tirilecek Prefab atanmam��!");
            return;
        }

        if (spawnedObject != null)
        {
            Debug.Log("ARPlaceObjectOnPlane: Obje zaten yerle�tirilmi�. Yeni t�klama yok say�l�yor.");
            return;
        }

        Vector2 screenPositionToRaycast;

        // Tercih edilen y�ntem: Ayr� bir Pointer Position Action kullanmak
        if (pointerPositionAction != null && pointerPositionAction.action != null && pointerPositionAction.action.enabled)
        {
            screenPositionToRaycast = pointerPositionAction.action.ReadValue<Vector2>();
            Debug.Log($"ARPlaceObjectOnPlane: Pointer Position Action'dan pozisyon okundu: {screenPositionToRaycast}");
        }
        // Fallback: E�er Pointer Position Action atanmam��sa, do�rudan cihazdan okumay� dene (daha az g�venilir)
        else if (Pointer.current != null && Pointer.current.deviceId != InputDevice.InvalidDeviceId)
        {
            screenPositionToRaycast = Pointer.current.position.ReadValue();
            Debug.LogWarning($"ARPlaceObjectOnPlane: Pointer Position Action atanmam��. Pointer.current kullan�l�yor. Pozisyon: {screenPositionToRaycast}");
        }
        else if (Touchscreen.current != null && Touchscreen.current.deviceId != InputDevice.InvalidDeviceId && Touchscreen.current.primaryTouch.isInProgress)
        {
            screenPositionToRaycast = Touchscreen.current.primaryTouch.position.ReadValue();
            Debug.LogWarning($"ARPlaceObjectOnPlane: Pointer Position Action atanmam��. Touchscreen.current kullan�l�yor. Pozisyon: {screenPositionToRaycast}");
        }
        else
        {
            Debug.LogError("ARPlaceObjectOnPlane: Ge�erli bir ekran pozisyonu al�namad�!");
            return;
        }

        Debug.Log($"ARPlaceObjectOnPlane: Raycast i�in kullan�lacak ekran pozisyonu: {screenPositionToRaycast}");

        if (raycastManager.Raycast(screenPositionToRaycast, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            spawnedObject = Instantiate(objectToPlacePrefab, hitPose.position, hitPose.rotation);
            Debug.Log("Obje yerle�tirildi: " + spawnedObject.name + " at " + hitPose.position);

            // Modelin y�n�n� ayarla (kameran�n Y a��s�na g�re)
            spawnedObject.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

            if (hidePlanesAfterPlacement && planeManager != null)
            {
                SetAllPlanesActive(false);
            }
        }
        else
        {
            Debug.Log("ARPlaceObjectOnPlane: Raycast herhangi bir AR d�zlemiyle kesi�medi.");
        }
    }

    void SetAllPlanesActive(bool value)
    {
        // planeManager null kontrol� OnPlaceObjectInputTriggered i�inde yap�ld��� i�in burada tekrar gerek yok, ama g�venlik i�in kalabilir.
        if (planeManager == null) return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
        // Yeni d�zlem alg�lamay� da durdurmak veya ba�latmak isterseniz:
        // planeManager.enabled = value;
        // veya
        // planeManager.requestedDetectionMode = value ? PlaneDetectionMode.Horizontal : PlaneDetectionMode.None;
    }

    // D��ar�dan �a�r�larak modeli s�f�rlamak ve tekrar yerle�tirmeye izin vermek i�in
    public void ResetAndAllowPlacement()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
        if (planeManager != null) // E�er planeManager atanm��sa
        {
            SetAllPlanesActive(true);
        }
        Debug.Log("ARPlaceObjectOnPlane: Obje yerle�imi s�f�rland�, tekrar yerle�tirme i�in haz�r.");
    }
}