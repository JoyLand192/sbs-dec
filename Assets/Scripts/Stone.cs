using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        mousePos.y = 4;
        mousePos.z = 0;
        transform.position = Vector3.Lerp(transform.position, mousePos, 1f);
    }
}
