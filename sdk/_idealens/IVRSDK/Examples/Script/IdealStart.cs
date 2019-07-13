using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IdealStart : MonoBehaviour {

    private Image mImage;
    void Awake()
    {
        mImage = GetComponent<Image>();
    }
	// Use this for initialization
	IEnumerator Start () {

        yield return new WaitForSeconds(3);
        Color cc = mImage.color;
        cc.a = 1;
        mImage.color = cc;
        yield return Application.LoadLevelAsync(1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
