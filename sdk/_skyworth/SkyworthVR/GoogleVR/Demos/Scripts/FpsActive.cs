using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsActive : MonoBehaviour
{
    private void Awake()
    {
         
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(Svr.SvrSetting.ShowFPS);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
