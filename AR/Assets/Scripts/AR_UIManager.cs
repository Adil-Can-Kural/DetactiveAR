using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation; // ARSession.state i�in
using UnityEngine.XR.ARSubsystems; // ARSession.state i�in
using TMPro;

public class AR_UIManager : MonoBehaviour
{
    [Header("Panel Referanslar�")]
    public GameObject settingsPanel;
    public GameObject modalBlockerPanel;

    [Header("Buton Metinleri (Dinamik De�i�im ��in)")]
    public TextMeshProUGUI toggleModeButtonTextInSettings;

    private AR_Experience_Manager arExperienceManager;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        else Debug.LogError("AR_UIManager: Settings Panel referans� atanmam��!");

        if (modalBlockerPanel != null)
        {
            modalBlockerPanel.SetActive(false);
            Button blockerButton = modalBlockerPanel.GetComponent<Button>();
            if (blockerButton != null) blockerButton.onClick.AddListener(CloseSettingsPanelDueToBlockerClick);
            else Debug.LogWarning("ModalBlockerPanel �zerinde Button bile�eni yok.");
        }
        else Debug.LogWarning("AR_UIManager: Modal Blocker Panel referans� atanmam��.");

        arExperienceManager = FindObjectOfType<AR_Experience_Manager>();
        if (arExperienceManager == null) Debug.LogWarning("AR_UIManager: Sahnede AR_Experience_Manager bulunamad�.");

        UpdateToggleModeButtonText();
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        bool newActiveState = !settingsPanel.activeSelf;
        settingsPanel.SetActive(newActiveState);
        if (modalBlockerPanel != null) modalBlockerPanel.SetActive(newActiveState);
        if (newActiveState) UpdateToggleModeButtonText(); // Panel a��ld���nda metni g�ncelle
    }

    public void GoToMainMenu()
    {
        if (arExperienceManager != null) arExperienceManager.PrepareToLeaveScene();
        SceneManager.LoadScene("MainMenu");
    }

    public void RequestPlaceOrResetModel() // Butonun ad� "Modeli S�f�rla" olabilir
    {
        if (arExperienceManager != null && !arExperienceManager.IsARModeActiveCurrently())
        {
            Debug.Log("3D G�r�nt�leyici modunda 'Modeli S�f�rla'n�n �zel bir i�levi yok. Sadece AR modu i�in.");
            CloseSettingsPanelDueToBlockerClick();
            return;
        }
        // AR modundaysak
        ARPlaceObjectOnPlane placementScript = FindObjectOfType<ARPlaceObjectOnPlane>(); // Do�rudan bul
        if (placementScript != null && placementScript.enabled)
        {
            Debug.Log("Model yerle�imini s�f�rlama iste�i (ARPlaceObjectOnPlane)...");
            placementScript.ResetAndAllowPlacement(); // ARPlaceObjectOnPlane'deki metodu �a��r
        }
        else
        {
            Debug.LogWarning("'Modeli S�f�rla' butonu t�kland� ama uygun/aktif bir ARPlaceObjectOnPlane scripti bulunamad�.");
        }
        CloseSettingsPanelDueToBlockerClick();
    }

    public void TriggerModeSwitch()
    {
        if (arExperienceManager != null)
        {
            arExperienceManager.SwitchMode();
            UpdateToggleModeButtonText(); // Hemen g�ncelle
        }
        else Debug.LogError("AR_Experience_Manager bulunamad�, mod de�i�tirilemiyor!");
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

        // ARSession.state AR Foundation paketinden gelir, using UnityEngine.XR.ARSubsystems ekli olmal�
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
                toggleModeButtonTextInSettings.text = arExperienceManager.IsARModeActiveCurrently() ? "3D �ncele" : "AR'ye Ge�";
            }
            else // Fallback
            {
                // Bu durum pek olmamal� ama AR_Experience_Manager bulunamazsa varsay�lan metin
                toggleModeButtonTextInSettings.text = "Mod De�i�tir";
            }
        }
    }
}