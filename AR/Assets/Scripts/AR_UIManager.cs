using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.UIElements.UxmlAttributeDescription;
// using TMPro; // Eðer TextMeshPro elemanlarýna doðrudan eriþim varsa

public class AR_UIManager : MonoBehaviour
{
    [Header("Panel Referanslarý")]
    public GameObject settingsPanel;      // Ayarlar panelin
    public GameObject modalBlockerPanel;  // Ayarlar paneli açýkken arkadaki týklamalarý engelleyen panel

    private AR_Experience_Manager arExperienceManager; // Sahneden çýkýþa hazýrlýk için

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("AR_UIManager: Settings Panel referansý atanmamýþ!");
        }

        if (modalBlockerPanel != null)
        {
            modalBlockerPanel.SetActive(false);
            Button blockerButton = modalBlockerPanel.GetComponent<Button>();
            if (blockerButton != null)
            {
                blockerButton.onClick.AddListener(CloseSettingsPanelDueToBlockerClick);
            }
        }

        arExperienceManager = FindObjectOfType<AR_Experience_Manager>();
        if (arExperienceManager == null)
        {
            Debug.LogWarning("AR_UIManager: Sahnede AR_Experience_Manager bulunamadý. Sahneden çýkýþ düzgün çalýþmayabilir.");
        }
    }

    // Ayarlar ikonuna (Pati kafasý) týklandýðýnda çaðrýlýr
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        bool newActiveState = !settingsPanel.activeSelf;
        settingsPanel.SetActive(newActiveState);
        if (modalBlockerPanel != null) modalBlockerPanel.SetActive(newActiveState);
    }

    // Ayarlar panelindeki "Ana Menü" butonuna týklandýðýnda çaðrýlýr
    public void GoToMainMenu()
    {
        if (arExperienceManager != null)
        {
            arExperienceManager.PrepareToLeaveScene(); // AR'ý kapat
        }
        else
        {
            // Fallback: Manager yoksa direkt sahne yükle, ama AR kapanmayabilir
            Debug.LogWarning("AR_Experience_Manager bulunamadýðý için PrepareToLeaveScene çaðrýlamadý.");
        }
        SceneManager.LoadScene("MainMenu");
    }

    // Modal Blocker panele týklandýðýnda çaðrýlýr
    private void CloseSettingsPanelDueToBlockerClick()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            if (modalBlockerPanel != null) modalBlockerPanel.SetActive(false);
        }
    }

    // Artýk diðer buton fonksiyonlarýna ve UpdateToggleModeButtonText'e gerek yok
 
}