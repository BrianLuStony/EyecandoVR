using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Stack<GameObject> pageStack = new Stack<GameObject>();
    public GameObject mainPage;

    // Start with the main page active
    void Start()
    {
        mainPage.SetActive(true);
        pageStack.Push(mainPage);
    }

    // Method to open a new page and push it onto the stack
    public void OpenPage(GameObject newPage)
    {
        if (pageStack.Count > 0)
        {
            GameObject currentPage = pageStack.Peek();
            currentPage.SetActive(false);
        }
        newPage.SetActive(true);
        pageStack.Push(newPage);
    }

    // Method to return to the previous page
    public void ReturnToPreviousPage()
    {
        if (pageStack.Count > 1)
        {
            GameObject currentPage = pageStack.Pop();
            currentPage.SetActive(false);
            GameObject previousPage = pageStack.Peek();
            previousPage.SetActive(true);
        }
    }
}
