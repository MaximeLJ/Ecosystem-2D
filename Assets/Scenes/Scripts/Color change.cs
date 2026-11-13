using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colorchange : MonoBehaviour
{
    public Color newColor;
    private SpriteRenderer rend;
    // Start is called before the first frame update
    void Start()
    {
       rend = GetComponent<SpriteRenderer>();
       rend.color = newColor; 
    }
}
