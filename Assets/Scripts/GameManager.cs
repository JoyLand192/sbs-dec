using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ cour ]")]
    public bool isOver;
    public int score;
    public int maxLevel;
    
    [Header("--------------[ object Poling ]")]
    public GameObject DonglePrefab;
    public Transform DongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1,30)]
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

    [Header("--------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;


    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < PoolSize; index++)
        {
            MakeDongle();
        }

        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        // 오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // 사운드 플레이
        bgmPlayer.Play();
        SfxPlay(Sfx.Batton);

        // 게임 시작 (동글생성)
        Invoke("nextDongle", 1.5f);
    }

    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instanteffectOdj = Instantiate(effectPrefab, effectGroup);
        instanteffectOdj.name = "Effect" + effectPool.Count;
        ParticleSystem instanteffect = instanteffectOdj.GetComponent<ParticleSystem>();

        // 동글 생성
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
        for(int index = 0; index < donglePool.Count; index++)
        {
            PoolCursor= (PoolCursor + 1) % donglePool.Count;
            if(!donglePool[PoolCursor].gameObject.activeSelf)
            {
                return donglePool[PoolCursor];
            }
        }

        return MakeDongle();
    }

    void nextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("waitNext");
    }

    IEnumerator waitNext()
    {
        while(lastDongle != null)
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        nextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drog();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
            return;

        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if(isOver)
        {
            return;
        }

        isOver = true;

        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        // 1.장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // 2.지우기 전에 모든 동글의 물리효과 비활성화
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }
        // 3.1번의 목록을 하나씩 접근해서 지우기
        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        // 게임오버 UI 표시
        sudScoreText.text = "점수: " + scoreText.text;
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
        switch(type)
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
        if(Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
