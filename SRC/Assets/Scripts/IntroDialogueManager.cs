using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Yeni Giri� Sistemi i�in

public class IntroDialogueManager : MonoBehaviour
{
    [Header("UI Referanslar�")]
    public GameObject dialogueBubbleObject;
    public TextMeshProUGUI dialogueText;

    [Header("Karakter Referanslar�")]
    public Animator characterAnimator;

    [Header("Diyalog Ayarlar�")]
    public List<DialogueLine> dialogueLines;
    public string mainMenuSceneName = "MainMenu"; // Ana Men� sahnenizin ad�

    [Header("Giri� Ayarlar� (Input System)")]
    [Tooltip("Ad�m 1'de kontrol etti�iniz/olu�turdu�unuz genel t�klama Action'�n� (�rn: OyunKontrolleri -> AnaMenuGirdileri -> BirincilDokunma VEYA DefaultInputActions -> UI -> Click) buraya atay�n.")]
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
            Debug.LogError("IntroDialogueManager: 'Advance Dialogue Action' atanmam�� veya Action null! L�tfen Inspector'dan atay�n. Script �al��mayacak.");
            enabled = false;
            return;
        }
        advanceDialogueAction.action.Enable();
        advanceDialogueAction.action.performed += OnAdvanceDialogueInput;
        Debug.Log("IntroDialogueManager: Advance Dialogue Action etkinle�tirildi ve 'performed' olay�na abone olundu.");
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
            // Disable etmeye gerek olmayabilir, sahne de�i�ince zaten gider
            // advanceDialogueAction.action.Disable();
            Debug.Log("IntroDialogueManager: Advance Dialogue Action aboneli�i kald�r�ld�.");
        }
    }

    private void OnAdvanceDialogueInput(InputAction.CallbackContext context)
    {
        Debug.Log("IntroDialogueManager: OnAdvanceDialogueInput �a�r�ld� (Dokunma/T�klama Alg�land�!)"); // <<<--- C�HAZ LOGLARI ���N �OK �NEML�
        if (dialogueActive && advanceCooldownCoroutine == null)
        {
            ShowNextLine();
        }
        else if (!dialogueActive)
        {
            Debug.Log("IntroDialogueManager: Dokunma alg�land� ama diyalog aktif de�il.");
        }
        else if (advanceCooldownCoroutine != null)
        {
            Debug.Log("IntroDialogueManager: Dokunma alg�land� ama cooldown aktif.");
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning("IntroDialogueManager: Diyalog sat�r� yok. Direkt ana men�ye ge�iliyor.");
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
        Debug.Log("IntroDialogueManager: Diyalog bitti. Ana Men� y�kleniyor: " + mainMenuSceneName);
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else Debug.LogError("IntroDialogueManager: Ana Men� sahne ad� belirtilmemi�!");
    }

    private IEnumerator AdvanceCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        advanceCooldownCoroutine = null;
    }
}