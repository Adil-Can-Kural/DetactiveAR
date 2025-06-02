using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Yeni Giriþ Sistemi için

public class IntroDialogueManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public GameObject dialogueBubbleObject;
    public TextMeshProUGUI dialogueText;

    [Header("Karakter Referanslarý")]
    public Animator characterAnimator;

    [Header("Diyalog Ayarlarý")]
    public List<DialogueLine> dialogueLines;
    public string mainMenuSceneName = "MainMenu"; // Ana Menü sahnenizin adý

    [Header("Giriþ Ayarlarý (Input System)")]
    [Tooltip("Adým 1'de kontrol ettiðiniz/oluþturduðunuz genel týklama Action'ýný (örn: OyunKontrolleri -> AnaMenuGirdileri -> BirincilDokunma VEYA DefaultInputActions -> UI -> Click) buraya atayýn.")]
    public InputActionReference advanceDialogueAction; // Inspector'dan atanacak

    private int currentLineIndex = -1;
    private bool dialogueActive = false;
    private Coroutine advanceCooldownCoroutine;

    [System.Serializable]
    public struct DialogueLine
    {
        [TextArea(3, 5)]
        public string text;
        public bool triggerGreeting;
    }

    void Awake()
    {
        if (advanceDialogueAction == null || advanceDialogueAction.action == null)
        {
            Debug.LogError("IntroDialogueManager: 'Advance Dialogue Action' atanmamýþ veya Action null! Lütfen Inspector'dan atayýn. Script çalýþmayacak.");
            enabled = false;
            return;
        }
        advanceDialogueAction.action.Enable();
        advanceDialogueAction.action.performed += OnAdvanceDialogueInput;
        Debug.Log("IntroDialogueManager: Advance Dialogue Action etkinleþtirildi ve 'performed' olayýna abone olundu.");
    }

    void Start()
    {
        if (dialogueBubbleObject != null)
            dialogueBubbleObject.SetActive(false);
        StartDialogue();
    }

    void OnDestroy()
    {
        if (advanceDialogueAction != null && advanceDialogueAction.action != null)
        {
            advanceDialogueAction.action.performed -= OnAdvanceDialogueInput;
            // Disable etmeye gerek olmayabilir, sahne deðiþince zaten gider
            // advanceDialogueAction.action.Disable();
            Debug.Log("IntroDialogueManager: Advance Dialogue Action aboneliði kaldýrýldý.");
        }
    }

    private void OnAdvanceDialogueInput(InputAction.CallbackContext context)
    {
        Debug.Log("IntroDialogueManager: OnAdvanceDialogueInput çaðrýldý (Dokunma/Týklama Algýlandý!)"); // <<<--- CÝHAZ LOGLARI ÝÇÝN ÇOK ÖNEMLÝ
        if (dialogueActive && advanceCooldownCoroutine == null)
        {
            ShowNextLine();
        }
        else if (!dialogueActive)
        {
            Debug.Log("IntroDialogueManager: Dokunma algýlandý ama diyalog aktif deðil.");
        }
        else if (advanceCooldownCoroutine != null)
        {
            Debug.Log("IntroDialogueManager: Dokunma algýlandý ama cooldown aktif.");
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning("IntroDialogueManager: Diyalog satýrý yok. Direkt ana menüye geçiliyor.");
            EndDialogue();
            return;
        }

        dialogueActive = true;
        currentLineIndex = -1;
        if (dialogueBubbleObject != null) dialogueBubbleObject.SetActive(true);

        if (characterAnimator != null && dialogueLines.Count > 0)
        {
            if (dialogueLines[0].triggerGreeting) characterAnimator.SetTrigger("greet");
        }
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (advanceCooldownCoroutine != null) return;

        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Count)
        {
            advanceCooldownCoroutine = StartCoroutine(AdvanceCooldown());
            DialogueLine line = dialogueLines[currentLineIndex];
            if (dialogueText != null) dialogueText.text = line.text;

            if (characterAnimator != null)
            {
                if (!line.triggerGreeting) characterAnimator.SetTrigger("StartTalking");
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        if (dialogueBubbleObject != null) dialogueBubbleObject.SetActive(false);
        Debug.Log("IntroDialogueManager: Diyalog bitti. Ana Menü yükleniyor: " + mainMenuSceneName);
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else Debug.LogError("IntroDialogueManager: Ana Menü sahne adý belirtilmemiþ!");
    }

    private IEnumerator AdvanceCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        advanceCooldownCoroutine = null;
    }
}