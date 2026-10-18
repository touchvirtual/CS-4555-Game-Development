
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [TextArea(3, 6)]
    public string dialogue =
        "Hello, little cat! That was some good fighting.";

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

        if (!AllEnemiesDefeated())
        {
            DialogueUI.Instance.ShowPrompt(false);
            return;
        }

        DialogueUI.Instance.ShowPrompt(true);

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (DialogueUI.Instance.IsTalking)
                DialogueUI.Instance.CloseDialogue();
            else
                DialogueUI.Instance.StartDialogue(dialogue, player);
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
