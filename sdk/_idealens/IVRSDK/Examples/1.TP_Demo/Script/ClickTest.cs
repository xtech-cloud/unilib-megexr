using UnityEngine;
using System.Collections;

public class ClickTest : MonoBehaviour {

    public GameObject Node;

    public void OnClick()
    {
        Debug.Log("click me");
        Node.SetActive(!Node.activeInHierarchy);
    }

}
