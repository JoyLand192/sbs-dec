using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuideMenu : MonoBehaviour
{
    [SerializeField] GameObject[] pages;
    [SerializeField] Button nextPageButton;
    [SerializeField] Button prevPageButton;
    [SerializeField] int currentPage;
    public void NextPage()
    {
        if (pages.Length < currentPage + 2) return;

        pages[currentPage++].SetActive(false);
        pages[currentPage].SetActive(true);

        if (currentPage == pages.Length - 1) nextPageButton.interactable = false;

        prevPageButton.interactable = true;
    }
    public void PrevPage()
    {
        if (currentPage == 0) return;

        pages[currentPage--].SetActive(false);
        pages[currentPage].SetActive(true);

        if (currentPage == 0) prevPageButton.interactable = false;

        nextPageButton.interactable = true;
    }
    public void CloseGuide()
    {
        gameObject.SetActive(false);
    }
}
