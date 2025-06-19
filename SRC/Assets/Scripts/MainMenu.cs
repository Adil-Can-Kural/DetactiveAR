using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Yapým Aþamasýnda Paneli/Baloncuðu")]
    public GameObject yapimAsamasiPanel;
    public TextMeshProUGUI mesajText;
    public float uyariGosterimSuresi = 7f;
    public Color uyariMetniRengi = new Color(0.627f, 0.322f, 0.176f);
    public float alphaGecisSuresi = 0.3f;

    [Header("Giriþ Ayarlarý")]
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
                Debug.LogError("yapimAsamasiPanel objesinde CanvasGroup bileþeni bulunamadý!");
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
        else Debug.LogWarning("Primary Touch Action referansý atanmamýþ veya Action null.");
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
        if (yapimAsamasiCanvasGroup != null && yapimAsamasiCanvasGroup.alpha > 0.01f) // Eðer görünürse
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
            Debug.LogError("Uyarý gösterilemiyor. Referanslar eksik.");
            if (yapimAsamasiPanel != null && yapimAsamasiCanvasGroup == null) yapimAsamasiPanel.SetActive(true);
            return;
        }

        // Metni her zaman güncelle
        string gorunenMekanAdi = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mekanAdi.ToLower());
        string[] mesajlar = {
             $"Merhaba! Ben Dedektif Pati. {gorunenMekanAdi} için henüz ipuçlarým eksik. Çok yakýnda hazýr olacak!",
             $"Maceraperest! {gorunenMekanAdi} gizemini çözmek üzereyim. Biraz daha sabýr, yakýnda keþifteyiz!",
             $"{gorunenMekanAdi} çok yakýnda! Dedektif Pati bu görev üzerinde çalýþýyor. Harika olacak!"
        };
        mesajText.text = mesajlar[Random.Range(0, mesajlar.Length)];
        mesajText.color = uyariMetniRengi;

        // Otomatik gizleme zamanlayýcýsýný her zaman durdur ve yeniden baþlat
        if (hidePanelCoroutine != null) StopCoroutine(hidePanelCoroutine);
        hidePanelCoroutine = StartCoroutine(AutoHidePanelRoutine());


        // --- FLICKER ÖNLEME MANTIÐI ---
        // Eðer panel zaten tamamen görünürse (veya görünmeye çok yakýnsa) veya bir fade-in iþlemi zaten sürüyorsa,
        // yeni bir fade-in baþlatma. Sadece metin güncellenmiþ oldu, zamanlayýcý da sýfýrlandý.
        bool isAlreadyVisibleOrFadingIn = yapimAsamasiCanvasGroup.alpha > 0.01f ||
                                         (alphaTransitionCoroutine != null && yapimAsamasiCanvasGroup.alpha < 1f); // Basit bir fade-in kontrolü

        if (isAlreadyVisibleOrFadingIn)
        {
            // Eðer bir fade-out sürüyorsa, onu iptal et ve tekrar fade-in yap (veya alpha'yý direkt 1 yap)
            if (alphaTransitionCoroutine != null)
            {
                // Eðer coroutine bir fade-out ise ve biz fade-in istiyorsak,
                // onu durdurup yeni bir fade-in baþlatmak yerine, alpha'yý direkt 1'e çekebiliriz
                // veya mevcut alpha'dan 1'e yeni bir fade-in baþlatabiliriz.
                // Þimdilik basitlik adýna, eðer aktif bir coroutine varsa ve görünür deðilsek,
                // muhtemelen bir fade-out'tur. Onu durdurup yeni fade-in baþlatacaðýz.
                StopCoroutine(alphaTransitionCoroutine);
                alphaTransitionCoroutine = null; // Referansý temizle
            }
            // Eðer zaten görünürse (alpha == 1) veya zaten bir fade-in yapýyorsa, alpha'yý elle 1'e set edip býrak.
            // Ya da eðer bir fade-out yeni durdurulduysa, mevcut alpha'dan 1'e yeni bir fade baþlat.
            if (yapimAsamasiCanvasGroup.alpha < 0.99f)
            { // Tam 1 deðilse, yeni fade-in baþlat
                alphaTransitionCoroutine = StartCoroutine(FadeCanvasGroup(yapimAsamasiCanvasGroup, yapimAsamasiCanvasGroup.alpha, 1f, alphaGecisSuresi));
            }
            else
            {
                // Zaten görünür (veya çok yakýn), alpha ile oynama
                yapimAsamasiCanvasGroup.alpha = 1f; // Emin olalým
                yapimAsamasiCanvasGroup.interactable = true;
                yapimAsamasiCanvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            // Panel tamamen gizli, normal fade-in baþlat
            if (alphaTransitionCoroutine != null) StopCoroutine(alphaTransitionCoroutine); // Önceki (belki fade-out) coroutine'i durdur
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

        // Sadece eðer panel þu an görünürse (veya görünmeye çalýþýyorsa) fade-out baþlat
        if (yapimAsamasiCanvasGroup.alpha > 0.01f)
        {
            alphaTransitionCoroutine = StartCoroutine(FadeCanvasGroup(yapimAsamasiCanvasGroup, yapimAsamasiCanvasGroup.alpha, 0f, alphaGecisSuresi));
        }
        else // Zaten gizli veya gizleniyorsa, alpha'yý 0'a ayarla ve býrak.
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
        // Önce panelin tam olarak görünür olmasýný bekle (fade-in'in bitmesini)
        // Bu, alphaTransitionCoroutine'in tamamlanmasýný beklemek anlamýna gelir.
        // Ancak bu, coroutine'ler arasýnda karmaþýk bir baðýmlýlýk yaratabilir.
        // Basit bir çözüm: alphaGecisSuresi kadar bekleyip, üzerine uyariGosterimSuresi eklemek.
        yield return new WaitForSeconds(alphaGecisSuresi); // Fade-in süresi kadar bekle
        yield return new WaitForSeconds(uyariGosterimSuresi); // Sonra asýl bekleme süresi

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
            // Eðer alpha zaten hedefe yakýnsa veya hedefi geçmiþse, iþlemi yapma
            if (cg.alpha >= endAlpha - 0.01f && duration > 0) yield break; // Çoktan orada veya geçiyorsa çýk

            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else if (endAlpha < startAlpha)
        { // Fade out
            if (cg.alpha <= endAlpha + 0.01f && duration > 0) yield break; // Çoktan orada veya geçiyorsa çýk
        }


        // Eðer duration 0 ise, anýnda ayarla
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
            yield break; // Coroutine'i sonlandýr
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
        // Coroutine bittiðinde alphaTransitionCoroutine referansýný temizleyelim
        if (ReferenceEquals(alphaTransitionCoroutine, GetCurrentCoroutine())) // Bu biraz hacky olabilir
        {
            //alphaTransitionCoroutine = null; 
            // Bu satýrý doðrudan kullanmak yerine, çaðýran yerde null yapmak daha güvenli olabilir.
            // Ya da coroutine bittiðinde bir event tetikleyip oradan null'lamak.
            // Þimdilik bu kýsmý kaldýrýyorum, çünkü coroutine referans yönetimi karmaþýklaþabilir.
        }
    }

    // Helper method to get current coroutine (not directly possible, this is a conceptual placeholder)
    private Coroutine GetCurrentCoroutine() { /* This is not a real Unity API */ return null; }


    // ... (OpenSettingsMenu ve QuitGame metodlarý ayný) ...
    public void OpenSettingsMenu() { /*...*/ }
    public void QuitGame() { /*...*/ }
}