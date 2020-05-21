using UnityEngine;
using System.Collections;

public class CheckConnectState : MonoBehaviour {

    public UnityEngine.UI.Text mConnect;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        mConnect.text = "IsConnected = " + IVR.IVRInputHandler.IsConnected() + "\n" 
            + "UseHandler = " + IVRInput.UseHandler;

    }
}
