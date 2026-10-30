
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    private bool waitForKeyRelease = false;

    [TextArea(3, 6)]
    public string[] dialogueLines = {
        "Thank you, little cat! That was some good fighting.",
        "I noticed you were fighting those pesky dog knights earlier.",
        "They've been causing trouble in the village for a while now.",
        "Come on lets go see how the village is doing."
    };

    private PlayerController player;
    private bool playerNearby = false;

    private bool AllEnemiesDefeated()
    {
        LevelManager level = FindObjectOfType<LevelManager>();

        if (level == null)
            return false;

        return level.EnemiesRemaining <= 0;
    }
   

    private void Update()
    {
        if (!playerNearby || player == null)
            return;

        if (DialogueUI.Instance == null ||
            Keyboard.current == null)
            return;

        if (waitForKeyRelease)
        {
            if (!Keyboard.current.fKey.isPressed)
                waitForKeyRelease = false;

            return;
        }

        if (!AllEnemiesDefeated())
        {
            DialogueUI.Instance.ShowPrompt(false);
            return;
        }

        if (DialogueUI.Instance.IsTalking)
            return;

        DialogueUI.Instance.ShowPrompt(true);

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            waitForKeyRelease = true;
            DialogueUI.Instance.StartDialogue(dialogueLines, player);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerController foundPlayer =
            other.GetComponentInParent<PlayerController>();

        if (foundPlayer == null)
            return;

        player = foundPlayer;
        playerNearby = true;

        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowPrompt(AllEnemiesDefeated());
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController foundPlayer =
            other.GetComponentInParent<PlayerController>();

        if (foundPlayer == null || foundPlayer != player)
            return;

        playerNearby = false;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowPrompt(false);

            if (DialogueUI.Instance.IsTalking)
                DialogueUI.Instance.CloseDialogue();
        }

        player = null;
    }

    private void OnDisable()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowPrompt(false);

            if (DialogueUI.Instance.IsTalking)
                DialogueUI.Instance.CloseDialogue();
        }

        playerNearby = false;
        player = null;
    }
}
