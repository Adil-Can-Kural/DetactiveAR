using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static UnityEngine.UIElements.UxmlAttributeDescription;
// using TMPro; // E�er TextMeshPro elemanlar�na do�rudan eri�im varsa

public class AR_UIManager : MonoBehaviour
{
    [Header("Panel Referanslar�")]
    public GameObject settingsPanel;      // Ayarlar panelin
    public GameObject modalBlockerPanel;  // Ayarlar paneli a��kken arkadaki t�klamalar� engelleyen panel

    private AR_Experience_Manager arExperienceManager; // Sahneden ��k��a haz�rl�k i�in

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("AR_UIManager: Settings Panel referans� atanmam��!");
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
            Debug.LogWarning("AR_UIManager: Sahnede AR_Experience_Manager bulunamad�. Sahneden ��k�� d�zg�n �al��mayabilir.");
        }
    }

    // Ayarlar ikonuna (Pati kafas�) t�kland���nda �a�r�l�r
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        bool newActiveState = !settingsPanel.activeSelf;
        settingsPanel.SetActive(newActiveState);
        if (modalBlockerPanel != null) modalBlockerPanel.SetActive(newActiveState);
    }

    // Ayarlar panelindeki "Ana Men�" butonuna t�kland���nda �a�r�l�r
    public void GoToMainMenu()
    {
        if (arExperienceManager != null)
        {
            arExperienceManager.PrepareToLeaveScene(); // AR'� kapat
        }
        else
        {
            // Fallback: Manager yoksa direkt sahne y�kle, ama AR kapanmayabilir
            Debug.LogWarning("AR_Experience_Manager bulunamad��� i�in PrepareToLeaveScene �a�r�lamad�.");
        }
        SceneManager.LoadScene("MainMenu");
    }

    // Modal Blocker panele t�kland���nda �a�r�l�r
    private void CloseSettingsPanelDueToBlockerClick()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            if (modalBlockerPanel != null) modalBlockerPanel.SetActive(false);
        }
    }

    // Art�k di�er buton fonksiyonlar�na ve UpdateToggleModeButtonText'e gerek yok
 
}