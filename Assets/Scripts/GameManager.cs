using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ Game Guide ]")]
    [SerializeField] GuideMenu gameGuideMenu;

    [Header("--------------[ Core ]")]
    public bool isOver;
    public int score;
    public int maxLevel;

    [Header("--------------[ Object Pooling ]")]
    public GameObject DonglePrefab;
    public Transform DongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int PoolSize;
    public int PoolCursor;
    public Dongle lastDongle;

    [Header("--------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Batton, over };
    int sfxCursor;

    [Header("--------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text sudScoreText;
    public UIManager uiManager;

    [Header("--------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    [Header("--------------[ Hold System ]")]
    public int holdLevel = -1;
    public bool isHoldUsed = false;

    [Header("--------------[ Spawn Settings ]")]
    public float autoSpawnDelay = 1.0f;

    private int nextDongleLevel;

    private void Awake()
    {
        Application.targetFrameRate = 120;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for (int index = 0; index < PoolSize; index++)
        {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        holdLevel = -1;
        isHoldUsed = false;
        uiManager.SetHoldItemDisplay(holdLevel);

        bgmPlayer.Play();
        SfxPlay(Sfx.Batton);

        nextDongleLevel = Random.Range(0, Mathf.Clamp(maxLevel, 2, 5));
        uiManager.SetNextItemDisplay(nextDongleLevel);

        Invoke("SpawnFirstDongle", 1.5f);
    }

    void SpawnFirstDongle()
    {
        nextDongle();
        if (lastDongle != null)
        {
            lastDongle.Drag();
        }
    }
    public void OpenGameGuide() => gameGuideMenu.gameObject.SetActive(true);

    Dongle MakeDongle()
    {
        GameObject instanteffectOdj = Instantiate(effectPrefab, effectGroup);
        instanteffectOdj.name = "Effect" + effectPool.Count;
        ParticleSystem instanteffect = instanteffectOdj.GetComponent<ParticleSystem>();

        GameObject instantDongleOdj = Instantiate(DonglePrefab, DongleGroup);
        instantDongleOdj.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instantDongleOdj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instanteffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for (int index = 0; index < donglePool.Count; index++)
        {
            PoolCursor = (PoolCursor + 1) % donglePool.Count;
            if (!donglePool[PoolCursor].gameObject.activeSelf)
            {
                return donglePool[PoolCursor];
            }
        }

        return MakeDongle();
    }

    void nextDongle()
    {
        if (isOver) return;

        isHoldUsed = false;

        lastDongle = GetDongle();
        lastDongle.level = nextDongleLevel;
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("waitNext");

        int rangeMax = Mathf.Clamp(maxLevel, 2, 5);
        nextDongleLevel = Random.Range(0, rangeMax);
        uiManager.SetNextItemDisplay(nextDongleLevel);
    }

    IEnumerator waitNext()
    {
        while (lastDongle != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(autoSpawnDelay);

        nextDongle();
        if (lastDongle != null)
        {
            lastDongle.Drag();
        }
    }

    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void HoldCurrentSwap()
    {
        isHoldUsed = true;
        int currentLevel = lastDongle.level;
        lastDongle.gameObject.SetActive(false);

        if (holdLevel == -1)
        {
            holdLevel = currentLevel;
            lastDongle = GetDongle();
            lastDongle.level = nextDongleLevel;
            lastDongle.gameObject.SetActive(true);

            int rangeMax = Mathf.Clamp(maxLevel, 2, 5);
            nextDongleLevel = Random.Range(0, rangeMax);
            uiManager.SetNextItemDisplay(nextDongleLevel);
        }
        else
        {
            int temp = holdLevel;
            holdLevel = currentLevel;
            lastDongle = GetDongle();
            lastDongle.level = temp;
            lastDongle.gameObject.SetActive(true);
        }

        uiManager.SetHoldItemDisplay(holdLevel);
        lastDongle.Drag();
        SfxPlay(Sfx.Batton);
    }

    public void HoldNext()
    {
        isHoldUsed = true;

        if (holdLevel == -1)
        {
            holdLevel = nextDongleLevel;
            int rangeMax = Mathf.Clamp(maxLevel, 2, 5);
            nextDongleLevel = Random.Range(0, rangeMax);
        }
        else
        {
            int temp = holdLevel;
            holdLevel = nextDongleLevel;
            nextDongleLevel = temp;
        }

        uiManager.SetNextItemDisplay(nextDongleLevel);
        uiManager.SetHoldItemDisplay(holdLevel);

        SfxPlay(Sfx.Batton);
    }

    public void GameOver()
    {
        if (isOver)
        {
            return;
        }

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        sudScoreText.text = "Score: " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Batton);
        StartCoroutine("ResetRoutine");
    }

    IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
    }

    public void SfxPlay(Sfx type)
    {
        switch (type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Batton:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) Application.Quit();

        if (lastDongle != null && lastDongle.isDrag && !isHoldUsed)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                HoldCurrentSwap();
            }
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}