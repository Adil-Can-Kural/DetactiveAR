using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
// using TMPro; // Doðrudan kullanmýyorsa

public class AR_Experience_Manager : MonoBehaviour
{
    [Header("AR Sistem Referanslarý")]
    public ARSession arSession;
    public GameObject xrOrigin;
    public ARPlaceObjectOnPlane arPlacementScript; // Dokunmayla yerleþtirme scriptiniz

    [Header("3D Görüntüleyici Modu Referanslarý")]
    public Camera viewerModeCamera;
    public GameObject modelViewerAnchorObject; // PARLIAMENT modelini içeren parent (ve üzerinde ModelViewController olan)
    public ModelViewController modelViewController; // modelViewerAnchorObject üzerindeki script

    [Header("UI Kontrol Referanslarý")]
    public AR_UIManager uiManager; // AR_UI_Canvas üzerindeki script
    public GameObject arModeUIElements;  // Opsiyonel: Sadece AR Modunda aktif olacak UI
    public GameObject viewerModeUIElements; // Opsiyonel: Sadece 3D Viewer moduna özel UI

    // private GameObject spawnedModelInstance; // Artýk dinamik spawn etmiyoruz
    private bool isARModeCurrentlyActive = true;

    void Start()
    {
        if (!ValidateReferences()) { enabled = false; return; }

        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.Unsupported)
        {
            isARModeCurrentlyActive = false;
        }
        else
        {
            isARModeCurrentlyActive = true;
        }

        ApplyCurrentModeState(); // Baþlangýç modunu uygula
        // Start sonunda uiManager'ýn buton metnini güncellemesini saðla
        if (uiManager != null) uiManager.UpdateToggleModeButtonText();
    }

    bool ValidateReferences()
    {
        bool allValid = true;
        if (arSession == null) { Debug.LogError("AR_Experience_Manager: ARSession atanmamýþ!"); allValid = false; }
        if (xrOrigin == null) { Debug.LogError("AR_Experience_Manager: XR Origin atanmamýþ!"); allValid = false; }
        if (arPlacementScript == null) { Debug.LogError("AR_Experience_Manager: ARPlaceObjectOnPlane (arPlacementScript) atanmamýþ!"); allValid = false; }
        if (viewerModeCamera == null) { Debug.LogError("AR_Experience_Manager: ViewerModeCamera atanmamýþ!"); allValid = false; }
        if (modelViewerAnchorObject == null) { Debug.LogError("AR_Experience_Manager: ModelViewerAnchorObject atanmamýþ!"); allValid = false; }
        if (modelViewController == null) { Debug.LogError("AR_Experience_Manager: ModelViewController atanmamýþ!"); allValid = false; }
        if (uiManager == null) { Debug.LogWarning("AR_Experience_Manager: AR_UIManager atanmamýþ."); allValid = false; } // Artýk önemli
        return allValid;
    }

    public void SwitchMode()
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogWarning("Cihaz AR desteklemediði için mod deðiþtirilemiyor. 3D modunda kalýnýyor.");
            isARModeCurrentlyActive = false;
        }
        else
        {
            isARModeCurrentlyActive = !isARModeCurrentlyActive;
        }
        ApplyCurrentModeState();
    }

    void ApplyCurrentModeState()
    {
        if (isARModeCurrentlyActive)
        {
            // --- AR MODUNU AKTÝF ET ---
            Debug.Log("AR Moduna geçiliyor.");
            if (viewerModeCamera != null) viewerModeCamera.gameObject.SetActive(false);
            if (modelViewerAnchorObject != null) modelViewerAnchorObject.SetActive(false); // Anchor'ý ve altýndaki modeli gizle
            if (modelViewController != null) modelViewController.enabled = false;
            if (viewerModeUIElements != null) viewerModeUIElements.SetActive(false);

            if (xrOrigin != null) xrOrigin.SetActive(true);
            if (arSession != null)
            {
                if (!arSession.gameObject.activeInHierarchy) arSession.gameObject.SetActive(true);
                if (ARSession.state != ARSessionState.None && ARSession.state != ARSessionState.Unsupported &&
                    (arSession.subsystem == null || !arSession.subsystem.running))
                {
                    arSession.Reset();
                }
            }
            if (arPlacementScript != null)
            {
                arPlacementScript.enabled = true;
                if (arPlacementScript.gameObject.activeInHierarchy)
                { // Sadece eðer XR Origin aktifse Reset çaðýr
                    arPlacementScript.ResetAndAllowPlacement();
                }
            }
            if (arModeUIElements != null) arModeUIElements.SetActive(true);
        }
        else // 3D GÖRÜNTÜLEYÝCÝ MODU
        {
            Debug.Log("3D Görüntüleyici Moduna geçiliyor.");
            // AR Elemanlarýný Kapat
            if (arPlacementScript != null)
            {
                if (arPlacementScript.gameObject.activeInHierarchy)
                {
                    arPlacementScript.ResetAndAllowPlacement(); // AR'dan çýkarken AR modelini temizle
                }
                arPlacementScript.enabled = false;
            }
            if (arModeUIElements != null) arModeUIElements.SetActive(false);
            if (xrOrigin != null) xrOrigin.SetActive(false); // Bu, içindeki ARPlaneManager vs.'yi de deaktif eder

            // 3D Görüntüleyici Elemanlarýný Aç
            if (viewerModeCamera != null)
            {
                viewerModeCamera.gameObject.SetActive(true);
                Debug.Log("ApplyCurrentModeState (3D Modu): viewerModeCamera AKTÝF EDÝLDÝ.");
            }
            else Debug.LogError("viewerModeCamera referansý null!");

            if (modelViewerAnchorObject != null)
            {
                modelViewerAnchorObject.SetActive(true); // <<<--- ANAHTAR SATIR
                Debug.Log("ApplyCurrentModeState (3D Modu): modelViewerAnchorObject AKTÝF EDÝLDÝ. Mevcut durumu: " + modelViewerAnchorObject.activeSelf);
            }
            else Debug.LogError("modelViewerAnchorObject referansý null!");


            if (modelViewController != null)
            {
                modelViewController.enabled = true;
                Debug.Log("ApplyCurrentModeState (3D Modu): modelViewController ENABLED YAPILDI.");
                // targetModel'in ModelViewController.Awake() veya Start() içinde
                // kendi child'ý olarak ayarlandýðýný varsayýyoruz.
                // viewerCamera'yý da ModelViewController içinde Awake/Start'ta veya buradan atamamýz lazým.
                if (viewerModeCamera != null)
                {
                    modelViewController.viewerCamera = this.viewerModeCamera; // Emin olmak için burada atayalým
                }
                modelViewController.ResetView(); // Görünümü ilk haline getir
            }
            else Debug.LogError("modelViewController referansý null!");

            if (viewerModeUIElements != null) viewerModeUIElements.SetActive(true);
        }

        if (uiManager != null)
        {
            uiManager.UpdateToggleModeButtonText();
        }
        Debug.Log("--- ApplyCurrentModeState TAMAMLANDI --- Mevcut Mod AR mý? " + isARModeCurrentlyActive);
    }

    public bool IsARModeActiveCurrently()
    {
        return isARModeCurrentlyActive;
    }

    public void PrepareToLeaveScene()
    {
        Debug.Log("Sahneden çýkýþa hazýrlanýlýyor...");
        // Moddan baðýmsýz olarak her þeyi kapatmaya çalýþalým
        if (xrOrigin != null) xrOrigin.SetActive(false);
        if (arPlacementScript != null) arPlacementScript.enabled = false;

        if (modelViewerAnchorObject != null) modelViewerAnchorObject.SetActive(false);
        if (viewerModeCamera != null) viewerModeCamera.gameObject.SetActive(false);
        if (modelViewController != null) modelViewController.enabled = false;
        // isARModeCurrentlyActive = false; // Veya bir sonraki açýlýþ için varsayýlaný ayarla
    }
}