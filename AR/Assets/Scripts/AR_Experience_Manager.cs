using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
// using TMPro; // Do�rudan kullanm�yorsa

public class AR_Experience_Manager : MonoBehaviour
{
    [Header("AR Sistem Referanslar�")]
    public ARSession arSession;
    public GameObject xrOrigin;
    public ARPlaceObjectOnPlane arPlacementScript; // Dokunmayla yerle�tirme scriptiniz

    [Header("3D G�r�nt�leyici Modu Referanslar�")]
    public Camera viewerModeCamera;
    public GameObject modelViewerAnchorObject; // PARLIAMENT modelini i�eren parent (ve �zerinde ModelViewController olan)
    public ModelViewController modelViewController; // modelViewerAnchorObject �zerindeki script

    [Header("UI Kontrol Referanslar�")]
    public AR_UIManager uiManager; // AR_UI_Canvas �zerindeki script
    public GameObject arModeUIElements;  // Opsiyonel: Sadece AR Modunda aktif olacak UI
    public GameObject viewerModeUIElements; // Opsiyonel: Sadece 3D Viewer moduna �zel UI

    // private GameObject spawnedModelInstance; // Art�k dinamik spawn etmiyoruz
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

        ApplyCurrentModeState(); // Ba�lang�� modunu uygula
        // Start sonunda uiManager'�n buton metnini g�ncellemesini sa�la
        if (uiManager != null) uiManager.UpdateToggleModeButtonText();
    }

    bool ValidateReferences()
    {
        bool allValid = true;
        if (arSession == null) { Debug.LogError("AR_Experience_Manager: ARSession atanmam��!"); allValid = false; }
        if (xrOrigin == null) { Debug.LogError("AR_Experience_Manager: XR Origin atanmam��!"); allValid = false; }
        if (arPlacementScript == null) { Debug.LogError("AR_Experience_Manager: ARPlaceObjectOnPlane (arPlacementScript) atanmam��!"); allValid = false; }
        if (viewerModeCamera == null) { Debug.LogError("AR_Experience_Manager: ViewerModeCamera atanmam��!"); allValid = false; }
        if (modelViewerAnchorObject == null) { Debug.LogError("AR_Experience_Manager: ModelViewerAnchorObject atanmam��!"); allValid = false; }
        if (modelViewController == null) { Debug.LogError("AR_Experience_Manager: ModelViewController atanmam��!"); allValid = false; }
        if (uiManager == null) { Debug.LogWarning("AR_Experience_Manager: AR_UIManager atanmam��."); allValid = false; } // Art�k �nemli
        return allValid;
    }

    public void SwitchMode()
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogWarning("Cihaz AR desteklemedi�i i�in mod de�i�tirilemiyor. 3D modunda kal�n�yor.");
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
            // --- AR MODUNU AKT�F ET ---
            Debug.Log("AR Moduna ge�iliyor.");
            if (viewerModeCamera != null) viewerModeCamera.gameObject.SetActive(false);
            if (modelViewerAnchorObject != null) modelViewerAnchorObject.SetActive(false); // Anchor'� ve alt�ndaki modeli gizle
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
                { // Sadece e�er XR Origin aktifse Reset �a��r
                    arPlacementScript.ResetAndAllowPlacement();
                }
            }
            if (arModeUIElements != null) arModeUIElements.SetActive(true);
        }
        else // 3D G�R�NT�LEY�C� MODU
        {
            Debug.Log("3D G�r�nt�leyici Moduna ge�iliyor.");
            // AR Elemanlar�n� Kapat
            if (arPlacementScript != null)
            {
                if (arPlacementScript.gameObject.activeInHierarchy)
                {
                    arPlacementScript.ResetAndAllowPlacement(); // AR'dan ��karken AR modelini temizle
                }
                arPlacementScript.enabled = false;
            }
            if (arModeUIElements != null) arModeUIElements.SetActive(false);
            if (xrOrigin != null) xrOrigin.SetActive(false); // Bu, i�indeki ARPlaneManager vs.'yi de deaktif eder

            // 3D G�r�nt�leyici Elemanlar�n� A�
            if (viewerModeCamera != null)
            {
                viewerModeCamera.gameObject.SetActive(true);
                Debug.Log("ApplyCurrentModeState (3D Modu): viewerModeCamera AKT�F ED�LD�.");
            }
            else Debug.LogError("viewerModeCamera referans� null!");

            if (modelViewerAnchorObject != null)
            {
                modelViewerAnchorObject.SetActive(true); // <<<--- ANAHTAR SATIR
                Debug.Log("ApplyCurrentModeState (3D Modu): modelViewerAnchorObject AKT�F ED�LD�. Mevcut durumu: " + modelViewerAnchorObject.activeSelf);
            }
            else Debug.LogError("modelViewerAnchorObject referans� null!");


            if (modelViewController != null)
            {
                modelViewController.enabled = true;
                Debug.Log("ApplyCurrentModeState (3D Modu): modelViewController ENABLED YAPILDI.");
                // targetModel'in ModelViewController.Awake() veya Start() i�inde
                // kendi child'� olarak ayarland���n� varsay�yoruz.
                // viewerCamera'y� da ModelViewController i�inde Awake/Start'ta veya buradan atamam�z laz�m.
                if (viewerModeCamera != null)
                {
                    modelViewController.viewerCamera = this.viewerModeCamera; // Emin olmak i�in burada atayal�m
                }
                modelViewController.ResetView(); // G�r�n�m� ilk haline getir
            }
            else Debug.LogError("modelViewController referans� null!");

            if (viewerModeUIElements != null) viewerModeUIElements.SetActive(true);
        }

        if (uiManager != null)
        {
            uiManager.UpdateToggleModeButtonText();
        }
        Debug.Log("--- ApplyCurrentModeState TAMAMLANDI --- Mevcut Mod AR m�? " + isARModeCurrentlyActive);
    }

    public bool IsARModeActiveCurrently()
    {
        return isARModeCurrentlyActive;
    }

    public void PrepareToLeaveScene()
    {
        Debug.Log("Sahneden ��k��a haz�rlan�l�yor...");
        // Moddan ba��ms�z olarak her �eyi kapatmaya �al��al�m
        if (xrOrigin != null) xrOrigin.SetActive(false);
        if (arPlacementScript != null) arPlacementScript.enabled = false;

        if (modelViewerAnchorObject != null) modelViewerAnchorObject.SetActive(false);
        if (viewerModeCamera != null) viewerModeCamera.gameObject.SetActive(false);
        if (modelViewController != null) modelViewController.enabled = false;
        // isARModeCurrentlyActive = false; // Veya bir sonraki a��l�� i�in varsay�lan� ayarla
    }
}