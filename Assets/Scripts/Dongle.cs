using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    [SerializeField] float currentAxisValue;
    public GameManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;

    [Header("Movement Settings")]
    public float moveSpeed = 20.0f;
    public float dropCooldown = 1.0f;
    public float moveSmoothness = 0.2f;
    public float boostMultiplier = 2.0f;
    public float boostThreshold = 0.5f;
    public float boostSmoothness = 0.05f;

    public Rigidbody2D rigid;
    CircleCollider2D Circle;
    Animator anim;
    SpriteRenderer SpriteRenderer;

    float deadTime;
    float dropCoolTime = 0f;
    private float currentVelocity = 0f;
    private float keyHoldTime = 0f;
    private float lastInputDirection = 0f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        Circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector3.zero;
        Circle.enabled = true;

        dropCoolTime = 0f;
        currentVelocity = 0f;
        keyHoldTime = 0f;
        lastInputDirection = 0f;
    }

    void Update()
    {
        if (isDrag)
        {
            dropCoolTime += Time.deltaTime;

            float h = Input.GetAxisRaw("Horizontal");

            if (h != 0)
            {
                if (Mathf.Sign(h) == Mathf.Sign(lastInputDirection))
                {
                    keyHoldTime += Time.deltaTime;
                }
                else
                {
                    keyHoldTime = 0f;
                }
                lastInputDirection = h;
            }
            else
            {
                keyHoldTime = 0f;
                lastInputDirection = 0f;
            }

            float currentSpeed = moveSpeed;
            float currentSmoothness = moveSmoothness;
            if (keyHoldTime > boostThreshold)
            {
                currentSpeed *= boostMultiplier;
                currentSmoothness = boostSmoothness;
            }

            float targetX = transform.position.x + h * currentSpeed * Time.deltaTime;

            float leftBorder = -4.3f + transform.localScale.x / 2f;
            float rightBorder = 4.3f - transform.localScale.x / 2f;

            if (targetX < leftBorder) targetX = leftBorder;
            if (targetX > rightBorder) targetX = rightBorder;

            float smoothX = Mathf.SmoothDamp(transform.position.x, targetX, ref currentVelocity, currentSmoothness);

            Vector3 nextPos = transform.position;
            nextPos.x = smoothX;
            nextPos.y = 8;
            nextPos.z = 0;
            transform.position = nextPos;

            if (dropCoolTime > dropCooldown)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Drop();
                }
            }
            currentAxisValue = h * currentSpeed;
        }
    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
        manager.lastDongle = null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("AttachRoutine");
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            if (level == other.level && !isMerge && !other.isMerge && level < 9)
            {
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                if (meY < otherY || (meY == otherY && meX > otherX))
                {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        rigid.simulated = false;
        Circle.enabled = false;

        if (targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int framCount = 0;

        while (framCount < 20)
        {
            framCount++;
            if (targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if (targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }

            yield return null;
        }

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        int[] scoreTable = { 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 };

        manager.score += scoreTable[level];

        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);

        level++;
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if (deadTime > 0.5f)
            {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            deadTime = 0;
            SpriteRenderer.color = Color.white;
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localPosition = transform.localPosition;
        effect.Play();
    }
}