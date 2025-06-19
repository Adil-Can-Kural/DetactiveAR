using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // PlaneAlignment, TrackableId için
using System.Collections.Generic; // Listeler için

public class ARPlaceObjectOnPlane : MonoBehaviour // Otomatik Yerleştirme için
{
    [Header("Model ve AR Ayarları")]
    public GameObject objectToPlacePrefab;    // Yerleştirilecek model prefabı
    public ARPlaneManager planeManager;         // Sahnedeki ARPlaneManager

    [Tooltip("Modelin yerleştirileceği düzlemin en az bu kadar genişlik VE derinlikte olması gerekir (metre).")]
    public float minPlaneDimension = 0.4f;

    [Tooltip("Model yerleştirildikten sonra düzlem görselleri gizlensin mi?")]
    public bool hidePlanesAfterPlacement = true;

    [Tooltip("Model yerleştirildikten sonra yeni düzlem algılama dursun mu?")]
    public bool disablePlaneDetectionAfterPlacement = true;


    private GameObject spawnedObject;
    private bool objectHasBeenPlaced = false;

    void Awake()
    {
        Debug.Log("ARPlaceObjectOnPlane (Auto): Awake çağrıldı.");
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            if (planeManager == null)
            {
                Debug.LogError("ARPlaneManager sahnede bulunamadı! Otomatik yerleştirme çalışmayacak.");
                enabled = false;
                return;
            }
        }
        if (objectToPlacePrefab == null)
        {
            Debug.LogError("Object To Place Prefab atanmamış! Otomatik yerleştirme çalışmayacak.");
            enabled = false;
            return;
        }
    }
    // void Update()
    // {
    //  if (spawnedObject != null)
    //  {
    // float dist = Vector3.Distance(Camera.main.transform.position, spawnedObject.transform.position);
    //Debug.Log($"📏 Kamera ↔ Model uzaklık: {dist:F2} m");

    //Vector3 direction = (spawnedObject.transform.position - Camera.main.transform.position).normalized;
    // float dot = Vector3.Dot(Camera.main.transform.forward, direction);
    //Debug.Log($"👁️ Görüş yönüne göre açı (dot): {dot:F2}");
    //}
    // }

    void OnEnable() // Script veya XR Origin aktif olduğunda
    {
        Debug.Log("ARPlaceObjectOnPlane (Auto): Etkinleştirildi, planesChanged olayına abone olunuyor.");
        planeManager.planesChanged += OnPlanesChanged;
        // Script aktif olduğunda hemen mevcut düzlemleri de kontrol et
        if (gameObject.activeInHierarchy && enabled) // Sadece obje gerçekten aktifse
        {
            CheckExistingPlanesAndPlace();
        }
    }

    void OnDisable() // Script veya XR Origin deaktif olduğunda
    {
        Debug.Log("ARPlaceObjectOnPlane (Auto): Devre dışı bırakıldı, planesChanged olayından abonelik kaldırılıyor.");
        if (planeManager != null) // planeManager null değilse abonelikten çık
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }

        // Opsiyonel: Eğer bu script deaktif olduğunda (örn: 3D moda geçiş)
        // sahnede kalan modeli de yok etmek isterseniz.
        // if (spawnedObject != null)
        // {
        //     Destroy(spawnedObject);
        //     spawnedObject = null;
        //     objectHasBeenPlaced = false;
        // }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (objectHasBeenPlaced || !this.enabled || !gameObject.activeInHierarchy) return;

        foreach (ARPlane plane in args.added)
        {
            if (IsValidPlaneForPlacement(plane))
            {
                PlaceObject(plane);
                return;
            }
        }
        foreach (ARPlane plane in args.updated)
        {
            if (IsValidPlaneForPlacement(plane))
            {
                PlaceObject(plane);
                return;
            }
        }
    }

    void CheckExistingPlanesAndPlace()
    {
        if (objectHasBeenPlaced || !this.enabled || !gameObject.activeInHierarchy) return;

        // planeManager.trackables canlı bir koleksiyon olduğu için doğrudan dönebiliriz
        // Yeni bir liste oluşturmaya gerek yok.
        Debug.Log($"Mevcut {planeManager.trackables.count} düzlem kontrol ediliyor...");
        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.gameObject.activeSelf && IsValidPlaneForPlacement(plane)) // Sadece aktif düzlemleri kontrol et
            {
                PlaceObject(plane);
                return;
            }
        }
        Debug.Log("CheckExistingPlanes: Uygun mevcut düzlem bulunamadı.");
    }

    bool IsValidPlaneForPlacement(ARPlane plane)
    {
        Debug.Log($"Plane {plane.trackableId} | Align: {plane.alignment} | Size: {plane.size}");

        if (plane.alignment != PlaneAlignment.HorizontalUp) return false;
        if (plane.size.x >= minPlaneDimension && plane.size.y >= minPlaneDimension)
        {
            return true;
        }
        return false;
    }


    void PlaceObject(ARPlane onPlane)
    {
        if (spawnedObject != null)
            Destroy(spawnedObject);

        Vector3 worldPos = onPlane.transform.position;
        Quaternion rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        spawnedObject = Instantiate(objectToPlacePrefab, worldPos, rotation);
        spawnedObject.transform.SetParent(null);
        spawnedObject.transform.localScale = Vector3.one * 0.01f;

        objectHasBeenPlaced = true;

        Debug.Log($"✅ Model yerleştirildi: {worldPos}");

        // Plane'leri kapat
        if (hidePlanesAfterPlacement)
            SetAllPlanesActive(false);
        if (disablePlaneDetectionAfterPlacement)
            planeManager.enabled = false;

        // SaatSpawner’a bildir (❗ burası doğru yer!)
        FindObjectOfType<SaatSpawner>()?.SpawnSaati(spawnedObject);
    }








    void SetAllPlanesActive(bool value)
    {
        if (planeManager == null) return;
        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(value);
        }
    }

    public void ResetAndAllowPlacement()
    {   
        Debug.Log("ARPlaceObjectOnPlane (Auto): ResetAndAllowPlacement çağrıldı.");
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
        objectHasBeenPlaced = false;

        if (planeManager != null)
        {
            if (!planeManager.enabled) planeManager.enabled = true;
            SetAllPlanesActive(true);
            // Hemen mevcut düzlemleri kontrol et ki yeni bir yerleştirme denensin
            CheckExistingPlanesAndPlace();
        }
        Debug.Log("Model yerleşimi sıfırlandı, tekrar otomatik yerleştirme bekleniyor.");
    }
    
}
    