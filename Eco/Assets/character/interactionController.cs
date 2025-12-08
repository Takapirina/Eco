using TMPro;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] GameObject dialogPanel;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dialogText;

    private npcController currentNPC;
    private int dialogIndex = 0;

    private void OnTriggerEnter(Collider other)
    {
        npcController npc = other.GetComponentInParent<npcController>();
        if (npc != null && npc.npcData.isInteractable)
        {
            currentNPC = npc;
            npc.transform.Find("Icona").gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        npcController npc = other.GetComponentInParent<npcController>();
        if (npc != null && npc == currentNPC && npc.npcData.isInteractable)
        {
            npc.transform.Find("Icona").gameObject.SetActive(false);
            currentNPC = null;

            dialogPanel.SetActive(false);
            dialogIndex = 0;
        }
    }

    private void Update()
    {
        if (currentNPC == null || !currentNPC.npcData.isInteractable) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!dialogPanel.activeSelf)
            {
                StartDialogue();
            }
            else
            {
                NextLine();
            }
        }
    }

    void StartDialogue()
    {
        dialogIndex = 0;

        dialogPanel.SetActive(true);
        nameText.text = currentNPC.npcData.npcName;
        dialogText.text = currentNPC.npcData.dialogueLines[dialogIndex];
        GetComponentInParent<PlayerMovement>().freeze = true;
        transform.parent.GetComponentInChildren<SpriteVisual>().setSpriteAnimState(SpriteAnimState.Idle);
        currentNPC.transform.Find("Icona").gameObject.SetActive(false);
    }

    void NextLine()
    {
        dialogIndex++;
        if (dialogIndex < currentNPC.npcData.dialogueLines.Length)
        {
            dialogText.text = currentNPC.npcData.dialogueLines[dialogIndex];
        }
        else
        {
            dialogPanel.SetActive(false);
            GetComponentInParent<PlayerMovement>().freeze = false;
            currentNPC.transform.Find("Icona").gameObject.SetActive(true);
            dialogIndex = 0;
        }
    }
}