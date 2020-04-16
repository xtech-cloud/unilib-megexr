using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using XTC.MegeXR.Core;
using XTC.MegeXR.Decorator;
using XTC.MegeXR.SDK;

public class Sample : MonoBehaviour 
{
    public enum VRMode
    {
        OFF,
        DUMMY,
        PICO,
        SKYWORTH,
        STEAM
    }
    public VRMode modeVR = VRMode.OFF;
    public Transform eventSystem;
    public Transform reticle;
    public Transform canvas3D;

    public GameObject cubePoint;
    public GameObject imgPoint;

    void Awake()
    {
        if(modeVR == VRMode.OFF)
            return;

        IXR xr = new DummyXR();
        if(modeVR == VRMode.PICO)
            xr = new PicoVR();
        else if(modeVR == VRMode.SKYWORTH)
            xr = new SkyworthVR();
        else if(modeVR == VRMode.STEAM)
            xr = new SteamVR();
        Engine.InjectXR(xr);
        Engine.InjectReticle(reticle);
        Engine.InjectCanvas3D(canvas3D);
        Engine.Preload();
        Engine.Initialize();

        eventSystem.gameObject.AddComponent<PointerInputDecorator>();
		
		ReticlePointerDecorator reticlePointer = reticle.gameObject.AddComponent<ReticlePointerDecorator>();

		xr.camera.gameObject.AddComponent<XPointerPhysicsRaycaster>();

		canvas3D.gameObject.AddComponent<XPointerGraphicRaycaster>();

        Debug.Log(XPointerInputModule.Pointer);

        XPointerInputModule.Pointer.overridePointerCamera = xr.camera.GetComponent<Camera>();

        XKeyHandler.onKeyDown.AddListener((_key)=>{
            Debug.Log("down key: " + _key);
        });
        XKeyHandler.onKeyUp.AddListener((_key)=>{
            Debug.Log("up key: " + _key);
        });
        XKeyHandler.onKeyHold.AddListener((_key)=>{
            Debug.Log("hold key: " + _key);
        });
        
    }
	

    void OnEnable()
    {
        if(modeVR == VRMode.OFF)
            return;
        Engine.Run();
    }

    void Start()
    {
        if(modeVR == VRMode.OFF)
            return;

        cubePoint.GetComponent<PointerHandleDecorator>().handler.AddClickEvent((_event)=>{
            Debug.Log("click " + cubePoint.name);
        });
        imgPoint.GetComponent<PointerHandleDecorator>().handler.AddClickEvent((_event)=>{
            imgPoint.transform.Find("icon").GetComponent<Image>().color = new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f),Random.Range(0, 1.0f));
        });
    }

    
	// Update is called once per frame
	void Update () {
        if(modeVR == VRMode.OFF)
            return;
        Engine.Update();
	}

    void OnDisable()
    {
        if(modeVR == VRMode.OFF)
            return;
        Engine.Stop();
    }

    void OnDestroy()
    {
        if(modeVR == VRMode.OFF)
            return;
        Engine.Release();
    }
}
