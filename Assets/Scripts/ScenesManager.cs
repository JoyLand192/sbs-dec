using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenesManager : MonoBehaviour
{
    [SerializeField]
    private Button StartBtn = null;

    [SerializeField]
    private Button ScoreBtn = null;

    [SerializeField]
    private Button ExplainBtn = null;

    private void StartBtnClick()
    {
        Debug.Log("버튼이 클릭되었습니다!");
    }

    // Start is called before the first frame update
    void Start()
    {
        StartBtn.onClick.AddListener(StartBtnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
