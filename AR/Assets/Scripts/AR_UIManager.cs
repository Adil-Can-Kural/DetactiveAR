using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation; // ARSession.state için
using UnityEngine.XR.ARSubsystems; // ARSession.state için
using TMPro;

public class AR_UIManager : MonoBehaviour
{
    [Header("Panel Referanslarý")]
    public GameObject settingsPanel;
    public GameObject modalBlockerPanel;

    [Header("Buton Metinleri (Dinamik Deðiþim Ýçin)")]
    public TextMeshProUGUI toggleModeButtonTextInSettings;

    private AR_Experience_Manager arExperienceManager;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        else Debug.LogError("AR_UIManager: Settings Panel referansý atanmamýþ!");

        if (modalBlockerPanel != null)
        {
            modalBlockerPanel.SetActive(false);
            Button blockerButton = modalBlockerPanel.GetComponent<Button>();
            if (blockerButton != null) blockerButton.onClick.AddListener(CloseSettingsPanelDueToBlockerClick);
            else Debug.LogWarning("ModalBlockerPanel üzerinde Button bileþeni yok.");
        }
        else Debug.LogWarning("AR_UIManager: Modal Blocker Panel referansý atanmamýþ.");

        arExperienceManager = FindObjectOfType<AR_Experience_Manager>();
        if (arExperienceManager == null) Debug.LogWarning("AR_UIManager: Sahnede AR_Experience_Manager bulunamadý.");

        UpdateToggleModeButtonText();
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        bool newActiveState = !settingsPanel.activeSelf;
        settingsPanel.SetActive(newActiveState);
        if (modalBlockerPanel != null) modalBlockerPanel.SetActive(newActiveState);
        if (newActiveState) UpdateToggleModeButtonText(); // Panel açýldýðýnda metni güncelle
    }

    public void GoToMainMenu()
    {
        if (arExperienceManager != null) arExperienceManager.PrepareToLeaveScene();
        SceneManager.LoadScene("MainMenu");
    }

    public void RequestPlaceOrResetModel() // Butonun adý "Modeli Sýfýrla" olabilir
    {
        if (arExperienceManager != null && !arExperienceManager.IsARModeActiveCurrently())
        {
            Debug.Log("3D Görüntüleyici modunda 'Modeli Sýfýrla'nýn özel bir iþlevi yok. Sadece AR modu için.");
            CloseSettingsPanelDueToBlockerClick();
            return;
        }
        // AR modundaysak
        ARPlaceObjectOnPlane placementScript = FindObjectOfType<ARPlaceObjectOnPlane>(); // Doðrudan bul
        if (placementScript != null && placementScript.enabled)
        {
            Debug.Log("Model yerleþimini sýfýrlama isteði (ARPlaceObjectOnPlane)...");
            placementScript.ResetAndAllowPlacement(); // ARPlaceObjectOnPlane'deki metodu çaðýr
        }
        else
        {
            Debug.LogWarning("'Modeli Sýfýrla' butonu týklandý ama uygun/aktif bir ARPlaceObjectOnPlane scripti bulunamadý.");
        }
        CloseSettingsPanelDueToBlockerClick();
    }

    public void TriggerModeSwitch()
    {
        if (arExperienceManager != null)
        {
            arExperienceManager.SwitchMode();
            UpdateToggleModeButtonText(); // Hemen güncelle
        }
        else Debug.LogError("AR_Experience_Manager bulunamadý, mod deðiþtirilemiyor!");
        CloseSettingsPanelDueToBlockerClick();
    }

    private void CloseSettingsPanelDueToBlockerClick()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            if (modalBlockerPanel != null) modalBlockerPanel.SetActive(false);
        }
    }

    public void UpdateToggleModeButtonText()
    {
        if (toggleModeButtonTextInSettings == null) return;

        Button arButton = toggleModeButtonTextInSettings.GetComponentInParent<Button>();

        // ARSession.state AR Foundation paketinden gelir, using UnityEngine.XR.ARSubsystems ekli olmalý
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.Unsupported)
        {
            toggleModeButtonTextInSettings.text = "AR Desteklenmiyor";
            if (arButton != null) arButton.interactable = false;
        }
        else
        {
            if (arButton != null) arButton.interactable = true;
            if (arExperienceManager != null)
            {
                toggleModeButtonTextInSettings.text = arExperienceManager.IsARModeActiveCurrently() ? "3D Ýncele" : "AR'ye Geç";
            }
            else // Fallback
            {
                // Bu durum pek olmamalý ama AR_Experience_Manager bulunamazsa varsayýlan metin
                toggleModeButtonTextInSettings.text = "Mod Deðiþtir";
            }
        }
    }
}