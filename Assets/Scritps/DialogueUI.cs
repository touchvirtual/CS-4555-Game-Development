
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject interactionPrompt;

    public bool IsTalking { get; private set; }
    public System.Action OnDialogueCompleted;

    private string[] dialogueLines;
    private int currentLine;
    private PlayerController currentPlayer;
    private bool canAdvance = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!IsTalking || Keyboard.current == null)
            return;

        if (!Keyboard.current.qKey.isPressed)
        {
            canAdvance = true;
        }

        if (canAdvance &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            canAdvance = false;
            AdvanceDialogue();
        }
    }

    public void StartDialogue(
        string[] lines, PlayerController player)
    {
        if (lines == null || lines.Length == 0)
            return;

        dialogueLines = lines;
        currentLine = 0;
        currentPlayer = player;
        IsTalking = true;
        canAdvance = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = dialogueLines[currentLine];

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (currentPlayer != null)
            currentPlayer.enabled = false;
    }

    public void StartDialogue(
        string message, PlayerController player)
    {
        StartDialogue(new string[] { message }, player);
    }

    private void AdvanceDialogue()
    {
        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            CloseDialogue();
            OnDialogueCompleted?.Invoke();
            return;
        }

        if (dialogueText != null)
            dialogueText.text = dialogueLines[currentLine];
    }



    public void CloseDialogue()
    {
        IsTalking = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (currentPlayer != null)
            currentPlayer.enabled = true;

        currentPlayer = null;
        dialogueLines = null;
        currentLine = 0;
        canAdvance = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }



    public void ShowPrompt(bool show)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(show && !IsTalking);
    }
}
