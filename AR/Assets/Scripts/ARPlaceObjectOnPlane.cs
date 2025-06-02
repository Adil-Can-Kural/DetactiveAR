using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation; // ARPlane, ARRaycastManager için
using UnityEngine.XR.ARSubsystems; // TrackableType, ARSessionState için
using UnityEngine.InputSystem;   // InputActionReference için

public class ARPlaceObjectOnPlane : MonoBehaviour
{
    [Header("Temel Ayarlar")]
    public GameObject objectToPlacePrefab;    // Inspector'dan atanacak model prefabý
    public ARRaycastManager raycastManager;   // Sahnedeki ARRaycastManager

    [Header("Giriþ Ayarlarý (Input System)")]
    [Tooltip("Genel týklama/dokunma Action'ýný (örn: DefaultInputActions -> UI -> Click VEYA kendi oluþturduðunuz BirincilDokunma) buraya atayýn.")]
    public InputActionReference placeObjectAction; // Týklama/Dokunma eylemi için referans

    [Header("Opsiyonel Ayarlar")]
    public ARPlaneManager planeManager;         // Düzlem görsellerini gizlemek için
    public bool hidePlanesAfterPlacement = true; // Model yerleþtirildikten sonra düzlemleri gizle

    private GameObject spawnedObject;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Pozisyon için ayrý bir Action (En iyi pratik)
    // Bunu da Inspector'dan atamanýz ve Input Actions Asset'inizde <Pointer>/position binding'i olan
    // bir Vector2 Value Action oluþturmanýz gerekir.
    [Header("Giriþ Pozisyonu (Önerilen)")]
    [Tooltip("<Pointer>/position binding'i olan bir Vector2 Value Action'ýný buraya atayýn.")]
    public InputActionReference pointerPositionAction;


    void Awake()
    {
        Debug.Log("ARPlaceObjectOnPlane Awake çaðrýldý.");

        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            if (raycastManager == null) Debug.LogError("ARRaycastManager sahnede bulunamadý! Bu script düzgün çalýþmayacak.");
        }
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            // PlaneManager opsiyonel olduðu için hata logu þart deðil.
        }

        // --- INPUT ACTION KURULUMU ---
        if (placeObjectAction == null || placeObjectAction.action == null)
        {
            Debug.LogError("ARPlaceObjectOnPlane: 'Place Object Action' atanmamýþ veya Action null! Lütfen Inspector'dan atayýn. Týklama/dokunma çalýþmayacak.");
            enabled = false; // Scripti devre dýþý býrakabiliriz
            return;
        }
        // Ýsteðe baðlý: Eðer pozisyon için ayrý bir Action kullanmýyorsanýz, aþaðýdaki logu ve enabled = false satýrýný yorum satýrý yapýn.
        if (pointerPositionAction == null || pointerPositionAction.action == null)
        {
            Debug.LogWarning("ARPlaceObjectOnPlane: 'Pointer Position Action' atanmamýþ. Pozisyon almak için doðrudan cihaz sorgusu kullanýlacak (daha az güvenilir olabilir).");
            // enabled = false; // Eðer bu Action zorunluysa scripti devre dýþý býrak
        }

        // Action'larý etkinleþtir
        placeObjectAction.action.Enable();
        if (pointerPositionAction != null && pointerPositionAction.action != null)
        {
            pointerPositionAction.action.Enable();
        }
        Debug.Log("ARPlaceObjectOnPlane: Input Action(lar) etkinleþtirildi.");
    }

    void OnEnable()
    {
        // 'performed' olayýna abone ol
        if (placeObjectAction != null && placeObjectAction.action != null)
        {
            placeObjectAction.action.performed += OnPlaceObjectInputTriggered;
            Debug.Log("ARPlaceObjectOnPlane: 'placeObjectAction.performed' olayýna abone olundu.");
        }
    }

    void OnDisable()
    {
        // Abonelikten çýk
        if (placeObjectAction != null && placeObjectAction.action != null)
        {
            placeObjectAction.action.performed -= OnPlaceObjectInputTriggered;
            Debug.Log("ARPlaceObjectOnPlane: 'placeObjectAction.performed' olayýndan abonelik kaldýrýldý.");
            // Action'larý burada disable etmeye genellikle gerek yoktur,
            // özellikle de baþka scriptler ayný Action'ý kullanýyorsa.
            // placeObjectAction.action.Disable();
            // if (pointerPositionAction != null && pointerPositionAction.action != null) pointerPositionAction.action.Disable();
        }
    }

    private void OnPlaceObjectInputTriggered(InputAction.CallbackContext context)
    {
        if (!this.enabled || objectToPlacePrefab == null) // Script aktif deðilse veya prefab yoksa çýk
        {
            if (objectToPlacePrefab == null) Debug.LogWarning("ARPlaceObjectOnPlane: Yerleþtirilecek Prefab atanmamýþ!");
            return;
        }

        if (spawnedObject != null)
        {
            Debug.Log("ARPlaceObjectOnPlane: Obje zaten yerleþtirilmiþ. Yeni týklama yok sayýlýyor.");
            return;
        }

        Vector2 screenPositionToRaycast;

        // Tercih edilen yöntem: Ayrý bir Pointer Position Action kullanmak
        if (pointerPositionAction != null && pointerPositionAction.action != null && pointerPositionAction.action.enabled)
        {
            screenPositionToRaycast = pointerPositionAction.action.ReadValue<Vector2>();
            Debug.Log($"ARPlaceObjectOnPlane: Pointer Position Action'dan pozisyon okundu: {screenPositionToRaycast}");
        }
        // Fallback: Eðer Pointer Position Action atanmamýþsa, doðrudan cihazdan okumayý dene (daha az güvenilir)
        else if (Pointer.current != null && Pointer.current.deviceId != InputDevice.InvalidDeviceId)
        {
            screenPositionToRaycast = Pointer.current.position.ReadValue();
            Debug.LogWarning($"ARPlaceObjectOnPlane: Pointer Position Action atanmamýþ. Pointer.current kullanýlýyor. Pozisyon: {screenPositionToRaycast}");
        }
        else if (Touchscreen.current != null && Touchscreen.current.deviceId != InputDevice.InvalidDeviceId && Touchscreen.current.primaryTouch.isInProgress)
        {
            screenPositionToRaycast = Touchscreen.current.primaryTouch.position.ReadValue();
            Debug.LogWarning($"ARPlaceObjectOnPlane: Pointer Position Action atanmamýþ. Touchscreen.current kullanýlýyor. Pozisyon: {screenPositionToRaycast}");
        }
        else
        {
            Debug.LogError("ARPlaceObjectOnPlane: Geçerli bir ekran pozisyonu alýnamadý!");
            return;
        }

        Debug.Log($"ARPlaceObjectOnPlane: Raycast için kullanýlacak ekran pozisyonu: {screenPositionToRaycast}");

        if (raycastManager.Raycast(screenPositionToRaycast, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            spawnedObject = Instantiate(objectToPlacePrefab, hitPose.position, hitPose.rotation);
            Debug.Log("Obje yerleþtirildi: " + spawnedObject.name + " at " + hitPose.position);

            // Modelin yönünü ayarla (kameranýn Y açýsýna göre)
            spawnedObject.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

            if (hidePlanesAfterPlacement && planeManager != null)
            {
                SetAllPlanesActive(false);
            }
        }
        else
        {
            Debug.Log("ARPlaceObjectOnPlane: Raycast herhangi bir AR düzlemiyle kesiþmedi.");
        }
    }

    void SetAllPlanesActive(bool value)
    {
        // planeManager null kontrolü OnPlaceObjectInputTriggered içinde yapýldýðý için burada tekrar gerek yok, ama güvenlik için kalabilir.
        if (planeManager == null) return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
        // Yeni düzlem algýlamayý da durdurmak veya baþlatmak isterseniz:
        // planeManager.enabled = value;
        // veya
        // planeManager.requestedDetectionMode = value ? PlaneDetectionMode.Horizontal : PlaneDetectionMode.None;
    }

    // Dýþarýdan çaðrýlarak modeli sýfýrlamak ve tekrar yerleþtirmeye izin vermek için
    public void ResetAndAllowPlacement()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
        if (planeManager != null) // Eðer planeManager atanmýþsa
        {
            SetAllPlanesActive(true);
        }
        Debug.Log("ARPlaceObjectOnPlane: Obje yerleþimi sýfýrlandý, tekrar yerleþtirme için hazýr.");
    }
}