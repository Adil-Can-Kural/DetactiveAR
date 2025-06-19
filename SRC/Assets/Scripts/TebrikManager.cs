using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class TebrikManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public GameObject dialogueBubbleObject;
    public TextMeshProUGUI dialogueText;

    [Header("Karakter Referanslarý")]
    public Animator characterAnimator;

    [Header("Tebrik Diyaloglarý")]
    [Tooltip("Her zaman gösterilecek ilk tebrik cümlesi.")]
    [TextArea(3, 5)]
    public string baslangicTebrikMesaji = "Harikasýn kaþif! Görevi baþarýyla tamamladýn ve kayýp parçayý buldun!";

    [Tooltip("Her oynanýþta bunlardan rastgele biri seçilecek.")]
    public List<string> eglenceliBilgilerListesi; // Fun Facts - Inspector'dan doldurulacak

    [Tooltip("Her zaman gösterilecek son kapanýþ cümlesi.")]
    [TextArea(3, 5)]
    public string kapanisMesaji = "Yeni maceralarda ve yeni gizemlerde tekrar görüþmek üzere! Hoþça kal!";

    [Header("Sahne ve Animasyon Ayarlarý")]
    [Tooltip("Tebrik bittikten sonra gidilecek sahne (Genellikle Ana Menü)")]
    public string targetSceneName = "MainMenu";
    [Tooltip("Baþlangýç alkýþ animasyonunun yaklaþýk süresi (saniye)")]
    public float initialClappingDuration = 2.5f;

    [Header("Giriþ Ayarlarý (Input System)")]
    public InputActionReference advanceTebrikAction;

    // O anki oyun için oluþturulan ve gösterilecek olan tam diyalog listesi
    private List<string> gosterilecekDiyaloglar = new List<string>();
    private int currentTebrikLineIndex = -1;
    private bool tebrikDialogueActive = false;
    private Coroutine advanceTebrikCooldownCoroutine;

    void Awake()
    {
        // Gerekli referanslarý ve Input Action'ý ayarla
        if (advanceTebrikAction == null || advanceTebrikAction.action == null)
        {
            Debug.LogError("TebrikManager: 'Advance Tebrik Action' atanmamýþ! Lütfen Inspector'dan atayýn. Script çalýþmayacak.");
            enabled = false;
            return;
        }
        advanceTebrikAction.action.Enable();
        advanceTebrikAction.action.performed += OnAdvanceTebrikInput;
        Debug.Log("TebrikManager: Input Action etkinleþtirildi ve 'performed' olayýna abone olundu.");
    }

    void Start()
    {
        // Baþlangýçta konuþma balonunu gizle
        if (dialogueBubbleObject != null)
        {
            dialogueBubbleObject.SetActive(false);
        }
        // Tebrik sekansýný otomatik baþlat
        StartCoroutine(FullTebrikSequenceRoutine());
    }

    void OnDestroy()
    {
        // Sahne deðiþirken veya obje yok edilirken abonelikten çýk
        if (advanceTebrikAction != null && advanceTebrikAction.action != null)
        {
            advanceTebrikAction.action.performed -= OnAdvanceTebrikInput;
        }
    }

    // Input Action tetiklendiðinde bu metot çaðrýlýr
    private void OnAdvanceTebrikInput(InputAction.CallbackContext context)
    {
        if (tebrikDialogueActive && advanceTebrikCooldownCoroutine == null)
        {
            ShowNextTebrikLine();
        }
    }

    // Alkýþ, diyalog hazýrlýðý ve diyalog baþlangýcýný yöneten ana Coroutine
    private IEnumerator FullTebrikSequenceRoutine()
    {
        // 1. Alkýþ Animasyonu
        if (characterAnimator != null)
        {
            Debug.Log("TebrikManager: Baþlangýç alkýþ animasyonu tetikleniyor...");
            characterAnimator.SetTrigger("startClapping"); // Animator'da bu Trigger olmalý
            yield return new WaitForSeconds(initialClappingDuration);
        }
        else
        {
            Debug.LogWarning("TebrikManager: Character Animator atanmamýþ.");
        }

        // 2. Oynanacak Diyalog Listesini Hazýrla
        PrepareDialogueSequence();

        if (gosterilecekDiyaloglar.Count == 0)
        {
            Debug.LogWarning("TebrikManager: Gösterilecek diyalog yok. Hedef sahneye geçiliyor.");
            EndTebrikDialogue();
            yield break;
        }

        // 3. Diyaloglarý Baþlat
        tebrikDialogueActive = true;
        currentTebrikLineIndex = -1;

        // ÖNCEKÝ SORUNUN ÇÖZÜMÜ: Konuþma balonunu alkýþ bittikten sonra aktif et
        if (dialogueBubbleObject != null)
        {
            dialogueBubbleObject.SetActive(true);
        }

        ShowNextTebrikLine(); // Ýlk diyalog satýrýný göster
    }

    // Gösterilecek diyalog listesini dinamik olarak oluþturan fonksiyon
    private void PrepareDialogueSequence()
    {
        gosterilecekDiyaloglar.Clear();

        if (!string.IsNullOrWhiteSpace(baslangicTebrikMesaji))
        {
            gosterilecekDiyaloglar.Add(baslangicTebrikMesaji);
        }

        if (eglenceliBilgilerListesi != null && eglenceliBilgilerListesi.Count > 0)
        {
            int randomIndex = Random.Range(0, eglenceliBilgilerListesi.Count);
            gosterilecekDiyaloglar.Add(eglenceliBilgilerListesi[randomIndex]);
        }
        else
        {
            Debug.LogWarning("TebrikManager: Eðlenceli bilgiler listesi boþ veya atanmamýþ.");
        }

        if (!string.IsNullOrWhiteSpace(kapanisMesaji))
        {
            gosterilecekDiyaloglar.Add(kapanisMesaji);
        }
    }

    // Bir sonraki diyalog satýrýný gösterir
    private void ShowNextTebrikLine()
    {
        if (advanceTebrikCooldownCoroutine != null) return;

        currentTebrikLineIndex++;
        if (currentTebrikLineIndex < gosterilecekDiyaloglar.Count)
        {
            advanceTebrikCooldownCoroutine = StartCoroutine(AdvanceTebrikCooldown());
            string lineText = gosterilecekDiyaloglar[currentTebrikLineIndex];
            if (dialogueText != null) dialogueText.text = lineText;

            if (characterAnimator != null)
            {
                // Her yeni satýrda 'StartTalking' trigger'ýný ateþle.
                characterAnimator.SetTrigger("StartTalking");
            }
        }
        else
        {
            EndTebrikDialogue(); // Diyaloglar bitti
        }
    }

    // Diyalogu bitirir ve sahne geçiþini baþlatýr
    private void EndTebrikDialogue()
    {
        tebrikDialogueActive = false;

        // Eðer baloncuðun sahne geçiþine kadar kalmasýný istemiyorsan burada gizleyebilirsin
        // if (dialogueBubbleObject != null)
        // {
        //     dialogueBubbleObject.SetActive(false);
        // }

        Debug.Log("TebrikManager: Tebrik diyaloðu bitti. Hedef sahne yükleniyor: " + targetSceneName);
        LoadTargetScene();
    }

    // Hedef sahneyi yükler
    private void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("TebrikManager: Gidilecek hedef sahne adý (targetSceneName) belirtilmemiþ!");
        }
    }

    // Týklamalar arasý kýsa bir bekleme süresi saðlar
    private IEnumerator AdvanceTebrikCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        advanceTebrikCooldownCoroutine = null;
    }
}