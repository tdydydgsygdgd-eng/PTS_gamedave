using System;
using UnityEngine;

public class SimpleSpriteAnimator : MonoBehaviour
{
    public enum AnimState
    {
        Idle,
        Walk,
        Attack,
        Death
    }

    [Header("Animation Frames")]
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] attackFrames;
    public Sprite[] deathFrames;

    [Header("Settings")]
    public float framesPerSecond = 10f;
    public bool autoFlip = true;

    private SpriteRenderer spriteRenderer;
    private AnimState currentState = AnimState.Idle;
    private float timer = 0f;
    private int currentFrame = 0;
    private Action onAnimationComplete;
    private bool isPlayingOnce = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Update()
    {
        Sprite[] activeFrames = GetCurrentFrames();
        if (activeFrames == null || activeFrames.Length == 0) return;

        timer += Time.deltaTime;
        float frameInterval = 1f / Mathf.Max(1f, framesPerSecond);

        if (timer >= frameInterval)
        {
            timer -= frameInterval;
            currentFrame++;

            if (currentFrame >= activeFrames.Length)
            {
                if (isPlayingOnce)
                {
                    currentFrame = activeFrames.Length - 1;
                    isPlayingOnce = false;
                    Action callback = onAnimationComplete;
                    onAnimationComplete = null;
                    callback?.Invoke();
                    return;
                }
                else
                {
                    currentFrame = 0;
                }
            }

            if (spriteRenderer != null && currentFrame < activeFrames.Length)
            {
                spriteRenderer.sprite = activeFrames[currentFrame];
            }
        }
    }

    private Sprite[] GetCurrentFrames()
    {
        switch (currentState)
        {
            case AnimState.Walk:
                return (walkFrames != null && walkFrames.Length > 0) ? walkFrames : idleFrames;
            case AnimState.Attack:
                return (attackFrames != null && attackFrames.Length > 0) ? attackFrames : idleFrames;
            case AnimState.Death:
                return (deathFrames != null && deathFrames.Length > 0) ? deathFrames : idleFrames;
            case AnimState.Idle:
            default:
                return idleFrames;
        }
    }

    public void PlayIdle()
    {
        if (isPlayingOnce) return;
        if (currentState != AnimState.Idle)
        {
            currentState = AnimState.Idle;
            currentFrame = 0;
            timer = 0f;
            UpdateSpriteImmediately();
        }
    }

    public void PlayWalk()
    {
        if (isPlayingOnce) return;
        if (currentState != AnimState.Walk)
        {
            currentState = AnimState.Walk;
            currentFrame = 0;
            timer = 0f;
            UpdateSpriteImmediately();
        }
    }

    public void PlayAttack(Action onComplete = null)
    {
        currentState = AnimState.Attack;
        currentFrame = 0;
        timer = 0f;
        isPlayingOnce = true;
        onAnimationComplete = onComplete;
        UpdateSpriteImmediately();
    }

    public void PlayDeath(Action onComplete = null)
    {
        currentState = AnimState.Death;
        currentFrame = 0;
        timer = 0f;
        isPlayingOnce = true;
        onAnimationComplete = onComplete;
        UpdateSpriteImmediately();
    }

    public void SetFacing(float horizontalDirection)
    {
        if (!autoFlip || spriteRenderer == null) return;
        if (horizontalDirection < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
        else if (horizontalDirection > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void UpdateSpriteImmediately()
    {
        Sprite[] active = GetCurrentFrames();
        if (active != null && active.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = active[currentFrame % active.Length];
        }
    }
}
