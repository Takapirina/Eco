using UnityEngine;

public enum NPCRole { npc, Nurse }

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class npc : ScriptableObject
{
    public string npcName;
    public NPCRole NPCRole;

    [Header("Sprites by direction")]
    public Sprite[] down;
    public Sprite[] up;
    public Sprite[] left;
    public Sprite[] right;

    public float frameRate = 4f;

    [Header("Dialogue")]
    public string[] dialogueLines;
    public bool isInteractable;
}