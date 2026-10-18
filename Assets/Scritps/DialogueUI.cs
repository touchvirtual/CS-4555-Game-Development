
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text talkPrompt;

    private bool isTalking = false;
    private PlayerController currentPlayerController;

    public bool IsTalking => isTalking;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CloseDialogue();
    }

    public void ShowPrompt(bool show)
    {
        if (talkPrompt != null)
            talkPrompt.gameObject.SetActive(show && !isTalking);
    }

    public void StartDialogue(
        string message,
        PlayerController playerController)
    {
        isTalking = true;
        currentPlayerController = playerController;

        if (currentPlayerController != null)
            currentPlayerController.enabled = false;

        if (talkPrompt != null)
            talkPrompt.gameObject.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = message;
    }

    public void CloseDialogue()
    {
        isTalking = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (talkPrompt != null)
            talkPrompt.gameObject.SetActive(false);

        if (currentPlayerController != null)
        {
            currentPlayerController.enabled = true;
            currentPlayerController = null;
        }
    }
}
