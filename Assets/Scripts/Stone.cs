using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public bool isDrog;

    private void OnEnable()
    {
        isDrog = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float leftWall = -1.5f + transform.localPosition.x / 2f;
        float rightWall = 1.5f + transform.localPosition.x / 2f;

        if (mousePos.x < leftWall)
        {
            mousePos.x = leftWall;
        }
        else if (mousePos.x < rightWall)
        {
            mousePos.x = rightWall;
        }

        mousePos.y = 4;
        mousePos.z = 0;
        transform.position = Vector3.Lerp(transform.position, mousePos, 0.1f);
    }

    public void Drog()
    {
        isDrog = true;
    }

    public void Drop()
    {
        isDrog = false;

    }
}
