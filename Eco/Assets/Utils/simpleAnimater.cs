using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleAnimater : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 4f;
    private SpriteRenderer spriteRenderer;
    private int currentFrameIndex = 0;
    private float frameTimer = 0f;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frames[0];
    }
    private void Update()
    {
        if (frames.Length == 0) return;

        float frameDuration = 1f / frameRate;
        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            currentFrameIndex++;
            if (currentFrameIndex >= frames.Length)
                currentFrameIndex = 0;

            spriteRenderer.sprite = frames[currentFrameIndex];
        }
    }
    
}
