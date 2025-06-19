using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Yap�m A�amas�nda Paneli/Baloncu�u")]
    public GameObject yapimAsamasiPanel;
    public TextMeshProUGUI mesajText;
    public float uyariGosterimSuresi = 7f;
    public Color uyariMetniRengi = new Color(0.627f, 0.322f, 0.176f);
    public float alphaGecisSuresi = 0.3f;

    [Header("Giri� Ayarlar�")]
    public InputActionReference primaryTouchAction;
    public GameObject inputBlockerPanel;

    private readonly string[] aktifMekanlar = { "bigben" };
    private Coroutine hidePanelCoroutine;
    private Coroutine alphaTransitionCoroutine;
    private CanvasGroup yapimAsamasiCanvasGroup;

    IEnumerator Start()
    {
        if (yapimAsamasiPanel != null)
        {
            yapimAsamasiCanvasGroup = yapimAsamasiPanel.GetComponent<CanvasGroup>();
            if (yapimAsamasiCanvasGroup == null)
            {
                Debug.LogError("yapimAsamasiPanel objesinde CanvasGroup bile�eni bulunamad�!");
                yapimAsamasiPanel.SetActive(false);
            }
            else
            {
                yapimAsamasiCanvasGroup.alpha = 0f;
                yapimAsamasiCanvasGroup.interactable = false;
                yapimAsamasiCanvasGroup.blocksRaycasts = false;
                yapimAsamasiPanel.SetActive(true);
            }
        }

        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(true);
        yield return null;
        if (inputBlockerPanel != null) inputBlockerPanel.SetActive(false);
    }

    void Awake()
    {
        if (primaryTouchAction != null && primaryTouchAction.action != null)
        {
            primaryTouchAction.action.Enable();
            primaryTouchAction.action.performed += OnPrimaryTouchPerformed;
        }
        else Debug.LogWarning("Primary Touch Action referans� atanmam�� veya Action null.");
    }

    void OnDestroy()
    {
        if (primaryTouchAction != null && primaryTouchAction.action != null)
        {
            primaryTouchAction.action.performed -= OnPrimaryTouchPerformed;
            primaryTouchAction.action.Disable();
        }
    }

    private void OnPrimaryTouchPerformed(InputAction.CallbackContext context)
    {
        if (inputBlockerPanel != null && inputBlockerPanel.activeSelf) return;
        if (yapimAsamasiCanvasGroup != null && yapimAsamasiCanvasGroup.alpha > 0.01f) // E�er g�r�n�rse
        {
            KapatYapimAsamasiPanelini();
        }
    }

    public void LoadLandmarkScene(string landmarkName)
    {
        if (inputBlockerPanel != null && inputBlockerPanel.activeSelf) return;
        string normalizedLandmarkName = landmarkName.ToLower().Trim();
        bool mekanAktifMi = false;
        foreach (string aktifMekan in aktifMekanlar)
        {
            if (aktifMekan == normalizedLandmarkName)
            {
                mekanAktifMi = true;
                break;
            }
        }

        if (mekanAktifMi)
        {
            PlayerPrefs.SetString("SelectedLandmark", landmarkName);
            PlayerPrefs.Save();
            SceneManager.LoadScene("RealARScene");
        }
        else
        {
            GosterYapimAsamasiUyarisi(landmarkName);
        }
    }

    void GosterYapimAsamasiUyarisi(string mekanAdi)
    {
        if (yapimAsamasiPanel == null || mesajText == null || yapimAsamasiCanvasGroup == null)
        {
            Debug.LogError("Uyar� g�sterilemiyor. Referanslar eksik.");
            if (yapimAsamasiPanel != null && yapimAsamasiCanvasGroup == null) yapimAsamasiPanel.SetActive(true);
            return;
        }

        // Metni her zaman g�ncelle
        string gorunenMekanAdi = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mekanAdi.ToLower());
        string[] mesajlar = {
             $"Merhaba! Ben Dedektif Pati. {gorunenMekanAdi} i�in hen�z ipu�lar�m eksik. �ok yak�nda haz�r olacak!",
             $"Maceraperest! {gorunenMekanAdi} gizemini ��zmek �zereyim. Biraz daha sab�r, yak�nda ke�ifteyiz!",
             $"{gorunenMekanAdi} �ok yak�nda! Dedektif Pati bu g�rev �zerinde �al���yor. Harika olacak!"
        };
        mesajText.text = mesajlar[Random.Range(0, mesajlar.Length)];
        mesajText.color = uyariMetniRengi;

        // Otomatik gizleme zamanlay�c�s�n� her zaman durdur ve yeniden ba�lat
        if (hidePanelCoroutine != null) StopCoroutine(hidePanelCoroutine);
        hidePanelCoroutine = StartCoroutine(AutoHidePanelRoutine());


        // --- FLICKER �NLEME MANTI�I ---
        // E�er panel zaten tamamen g�r�n�rse (veya g�r�nmeye �ok yak�nsa) veya bir fade-in i�lemi zaten s�r�yorsa,
        // yeni bir fade-in ba�latma. Sadece metin g�ncellenmi� oldu, zamanlay�c� da s�f�rland�.
        bool isAlreadyVisibleOrFadingIn = yapimAsamasiCanvasGroup.alpha > 0.01f ||
                                         (alphaTransitionCoroutine != null && yapimAsamasiCanvasGroup.alpha < 1f); // Basit bir fade-in kontrol�

        if (isAlreadyVisibleOrFadingIn)
        {
            // E�er bir fade-out s�r�yorsa, onu iptal et ve tekrar fade-in yap (veya alpha'y� direkt 1 yap)
            if (alphaTransitionCoroutine != null)
            {
                // E�er coroutine bir fade-out ise ve biz fade-in istiyorsak,
                // onu durdurup yeni bir fade-in ba�latmak yerine, alpha'y� direkt 1'e �ekebiliriz
                // veya mevcut alpha'dan 1'e yeni bir fade-in ba�latabiliriz.
                // �imdilik basitlik ad�na, e�er aktif bir coroutine varsa ve g�r�n�r de�ilsek,
                // muhtemelen bir fade-out'tur. Onu durdurup yeni fade-in ba�lataca��z.
                StopCoroutine(alphaTransitionCoroutine);
                alphaTransitionCoroutine = null; // Referans� temizle
            }
            // E�er zaten g�r�n�rse (alpha == 1) veya zaten bir fade-in yap�yorsa, alpha'y� elle 1'e set edip b�rak.
            // Ya da e�er bir fade-out yeni durdurulduysa, mevcut alpha'dan 1'e yeni bir fade ba�lat.
            if (yapimAsamasiCanvasGroup.alpha < 0.99f)
            { // Tam 1 de�ilse, yeni fade-in ba�lat
                alphaTransitionCoroutine = StartCoroutine(FadeCanvasGroup(yapimAsamasiCanvasGroup, yapimAsamasiCanvasGroup.alpha, 1f, alphaGecisSuresi));
            }
            else
            {
                // Zaten g�r�n�r (veya �ok yak�n), alpha ile oynama
                yapimAsamasiCanvasGroup.alpha = 1f; // Emin olal�m
                yapimAsamasiCanvasGroup.interactable = true;
                yapimAsamasiCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            // Panel tamamen gizli, normal fade-in ba�lat
            if (alphaTransitionCoroutine != null) StopCoroutine(alphaTransitionCoroutine); // �nceki (belki fade-out) coroutine'i durdur
            alphaTransitionCoroutine = StartCoroutine(FadeCanvasGroup(yapimAsamasiCanvasGroup, 0f, 1f, alphaGecisSuresi));
        }
    }

    void StartFadeOutAndHide()
    {
        if (yapimAsamasiCanvasGroup == null)
        {
            if (yapimAsamasiPanel != null) yapimAsamasiPanel.SetActive(false);
            return;
        }

        if (alphaTransitionCoroutine != null) StopCoroutine(alphaTransitionCoroutine);
        if (hidePanelCoroutine != null) StopCoroutine(hidePanelCoroutine);

        // Sadece e�er panel �u an g�r�n�rse (veya g�r�nmeye �al���yorsa) fade-out ba�lat
        if (yapimAsamasiCanvasGroup.alpha > 0.01f)
        {
            alphaTransitionCoroutine = StartCoroutine(FadeCanvasGroup(yapimAsamasiCanvasGroup, yapimAsamasiCanvasGroup.alpha, 0f, alphaGecisSuresi));
        }
        else // Zaten gizli veya gizleniyorsa, alpha'y� 0'a ayarla ve b�rak.
        {
            yapimAsamasiCanvasGroup.alpha = 0f;
            yapimAsamasiCanvasGroup.interactable = false;
            yapimAsamasiCanvasGroup.blocksRaycasts = false;
        }
    }

    public void KapatYapimAsamasiPanelini()
    {
        StartFadeOutAndHide();
    }

    IEnumerator AutoHidePanelRoutine()
    {
        // �nce panelin tam olarak g�r�n�r olmas�n� bekle (fade-in'in bitmesini)
        // Bu, alphaTransitionCoroutine'in tamamlanmas�n� beklemek anlam�na gelir.
        // Ancak bu, coroutine'ler aras�nda karma��k bir ba��ml�l�k yaratabilir.
        // Basit bir ��z�m: alphaGecisSuresi kadar bekleyip, �zerine uyariGosterimSuresi eklemek.
        yield return new WaitForSeconds(alphaGecisSuresi); // Fade-in s�resi kadar bekle
        yield return new WaitForSeconds(uyariGosterimSuresi); // Sonra as�l bekleme s�resi

        if (yapimAsamasiCanvasGroup != null && yapimAsamasiCanvasGroup.alpha > 0.01f)
        {
            StartFadeOutAndHide();
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        if (endAlpha > startAlpha) // Fade in
        {
            // E�er alpha zaten hedefe yak�nsa veya hedefi ge�mi�se, i�lemi yapma
            if (cg.alpha >= endAlpha - 0.01f && duration > 0) yield break; // �oktan orada veya ge�iyorsa ��k

            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else if (endAlpha < startAlpha)
        { // Fade out
            if (cg.alpha <= endAlpha + 0.01f && duration > 0) yield break; // �oktan orada veya ge�iyorsa ��k
        }


        // E�er duration 0 ise, an�nda ayarla
        if (duration <= 0f)
        {
            cg.alpha = endAlpha;
            if (endAlpha < 0.5f)
            {
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            else
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            onComplete?.Invoke();
            yield break; // Coroutine'i sonland�r
        }


        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = endAlpha;

        if (endAlpha < 0.5f)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        onComplete?.Invoke();
        // Coroutine bitti�inde alphaTransitionCoroutine referans�n� temizleyelim
        if (ReferenceEquals(alphaTransitionCoroutine, GetCurrentCoroutine())) // Bu biraz hacky olabilir
        {
            //alphaTransitionCoroutine = null; 
            // Bu sat�r� do�rudan kullanmak yerine, �a��ran yerde null yapmak daha g�venli olabilir.
            // Ya da coroutine bitti�inde bir event tetikleyip oradan null'lamak.
            // �imdilik bu k�sm� kald�r�yorum, ��nk� coroutine referans y�netimi karma��kla�abilir.
        }
    }

    // Helper method to get current coroutine (not directly possible, this is a conceptual placeholder)
    private Coroutine GetCurrentCoroutine() { /* This is not a real Unity API */ return null; }


    // ... (OpenSettingsMenu ve QuitGame metodlar� ayn�) ...
    public void OpenSettingsMenu() { /*...*/ }
    public void QuitGame() { /*...*/ }
}