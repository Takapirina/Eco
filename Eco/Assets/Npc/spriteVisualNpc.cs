using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spriteVisualNpc : MonoBehaviour
{
    private npcController npcController;
    private SpriteRenderer spriteRenderer;

    private int currentFrameIndex = 0;
    private float frameTimer = 0f;

    private void Awake()
    {
        npcController = GetComponentInParent<npcController>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (npcController != null && npcController.npcData != null)
        {
            spriteRenderer.sprite = npcController.npcData.npcSprite[0];
        }
    }

        private void Update()
    {
        if (npcController.npcData.npcSprite.Length == 0) return;

        float frameDuration = 1f / npcController.npcData.frameRate;
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            currentFrameIndex++;
            if (currentFrameIndex >= npcController.npcData.npcSprite.Length)
                currentFrameIndex = 0;

            spriteRenderer.sprite = npcController.npcData.npcSprite[currentFrameIndex];
        }
    }
}
