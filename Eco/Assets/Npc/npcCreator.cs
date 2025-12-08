using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class npc : ScriptableObject
{
    public string npcName;
    public Sprite[] npcSprite;
    public float frameRate = 4f;
    public string[] dialogueLines;
    public bool isInteractable;
}
