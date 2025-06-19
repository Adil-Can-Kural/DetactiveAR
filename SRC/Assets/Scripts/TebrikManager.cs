using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class TebrikManager : MonoBehaviour
{
    [Header("UI Referanslar�")]
    public GameObject dialogueBubbleObject;
    public TextMeshProUGUI dialogueText;

    [Header("Karakter Referanslar�")]
    public Animator characterAnimator;

    [Header("Tebrik Diyaloglar�")]
    [Tooltip("Her zaman g�sterilecek ilk tebrik c�mlesi.")]
    [TextArea(3, 5)]
    public string baslangicTebrikMesaji = "Harikas�n ka�if! G�revi ba�ar�yla tamamlad�n ve kay�p par�ay� buldun!";

    [Tooltip("Her oynan��ta bunlardan rastgele biri se�ilecek.")]
    public List<string> eglenceliBilgilerListesi; // Fun Facts - Inspector'dan doldurulacak

    [Tooltip("Her zaman g�sterilecek son kapan�� c�mlesi.")]
    [TextArea(3, 5)]
    public string kapanisMesaji = "Yeni maceralarda ve yeni gizemlerde tekrar g�r��mek �zere! Ho��a kal!";

    [Header("Sahne ve Animasyon Ayarlar�")]
    [Tooltip("Tebrik bittikten sonra gidilecek sahne (Genellikle Ana Men�)")]
    public string targetSceneName = "MainMenu";
    [Tooltip("Ba�lang�� alk�� animasyonunun yakla��k s�resi (saniye)")]
    public float initialClappingDuration = 2.5f;

    [Header("Giri� Ayarlar� (Input System)")]
    public InputActionReference advanceTebrikAction;

    // O anki oyun i�in olu�turulan ve g�sterilecek olan tam diyalog listesi
    private List<string> gosterilecekDiyaloglar = new List<string>();
    private int currentTebrikLineIndex = -1;
    private bool tebrikDialogueActive = false;
    private Coroutine advanceTebrikCooldownCoroutine;

    void Awake()
    {
        // Gerekli referanslar� ve Input Action'� ayarla
        if (advanceTebrikAction == null || advanceTebrikAction.action == null)
        {
            Debug.LogError("TebrikManager: 'Advance Tebrik Action' atanmam��! L�tfen Inspector'dan atay�n. Script �al��mayacak.");
            enabled = false;
            return;
        }
        advanceTebrikAction.action.Enable();
        advanceTebrikAction.action.performed += OnAdvanceTebrikInput;
        Debug.Log("TebrikManager: Input Action etkinle�tirildi ve 'performed' olay�na abone olundu.");
    }

    void Start()
    {
        // Ba�lang��ta konu�ma balonunu gizle
        if (dialogueBubbleObject != null)
        {
            dialogueBubbleObject.SetActive(false);
        }
        // Tebrik sekans�n� otomatik ba�lat
        StartCoroutine(FullTebrikSequenceRoutine());
    }

    void OnDestroy()
    {
        // Sahne de�i�irken veya obje yok edilirken abonelikten ��k
        if (advanceTebrikAction != null && advanceTebrikAction.action != null)
        {
            advanceTebrikAction.action.performed -= OnAdvanceTebrikInput;
        }
    }

    // Input Action tetiklendi�inde bu metot �a�r�l�r
    private void OnAdvanceTebrikInput(InputAction.CallbackContext context)
    {
        if (tebrikDialogueActive && advanceTebrikCooldownCoroutine == null)
        {
            ShowNextTebrikLine();
        }
    }

    // Alk��, diyalog haz�rl��� ve diyalog ba�lang�c�n� y�neten ana Coroutine
    private IEnumerator FullTebrikSequenceRoutine()
    {
        // 1. Alk�� Animasyonu
        if (characterAnimator != null)
        {
            Debug.Log("TebrikManager: Ba�lang�� alk�� animasyonu tetikleniyor...");
            characterAnimator.SetTrigger("startClapping"); // Animator'da bu Trigger olmal�
            yield return new WaitForSeconds(initialClappingDuration);
        }
        else
        {
            Debug.LogWarning("TebrikManager: Character Animator atanmam��.");
        }

        // 2. Oynanacak Diyalog Listesini Haz�rla
        PrepareDialogueSequence();

        if (gosterilecekDiyaloglar.Count == 0)
        {
            Debug.LogWarning("TebrikManager: G�sterilecek diyalog yok. Hedef sahneye ge�iliyor.");
            EndTebrikDialogue();
            yield break;
        }

        // 3. Diyaloglar� Ba�lat
        tebrikDialogueActive = true;
        currentTebrikLineIndex = -1;

        // �NCEK� SORUNUN ��Z�M�: Konu�ma balonunu alk�� bittikten sonra aktif et
        if (dialogueBubbleObject != null)
        {
            dialogueBubbleObject.SetActive(true);
        }

        ShowNextTebrikLine(); // �lk diyalog sat�r�n� g�ster
    }

    // G�sterilecek diyalog listesini dinamik olarak olu�turan fonksiyon
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
            Debug.LogWarning("TebrikManager: E�lenceli bilgiler listesi bo� veya atanmam��.");
        }

        if (!string.IsNullOrWhiteSpace(kapanisMesaji))
        {
            gosterilecekDiyaloglar.Add(kapanisMesaji);
        }
    }

    // Bir sonraki diyalog sat�r�n� g�sterir
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
                // Her yeni sat�rda 'StartTalking' trigger'�n� ate�le.
                characterAnimator.SetTrigger("StartTalking");
            }
        }
        else
        {
            EndTebrikDialogue(); // Diyaloglar bitti
        }
    }

    // Diyalogu bitirir ve sahne ge�i�ini ba�lat�r
    private void EndTebrikDialogue()
    {
        tebrikDialogueActive = false;

        // E�er baloncu�un sahne ge�i�ine kadar kalmas�n� istemiyorsan burada gizleyebilirsin
        // if (dialogueBubbleObject != null)
        // {
        //     dialogueBubbleObject.SetActive(false);
        // }

        Debug.Log("TebrikManager: Tebrik diyalo�u bitti. Hedef sahne y�kleniyor: " + targetSceneName);
        LoadTargetScene();
    }

    // Hedef sahneyi y�kler
    private void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("TebrikManager: Gidilecek hedef sahne ad� (targetSceneName) belirtilmemi�!");
        }
    }

    // T�klamalar aras� k�sa bir bekleme s�resi sa�lar
    private IEnumerator AdvanceTebrikCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        advanceTebrikCooldownCoroutine = null;
    }
}