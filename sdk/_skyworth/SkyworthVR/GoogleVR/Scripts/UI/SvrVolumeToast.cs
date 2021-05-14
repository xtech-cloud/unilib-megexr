using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SvrVolumeToast : MonoBehaviour
{
    [SerializeField]
    private Image m_volumeicon;
    [SerializeField]
    private Image m_volume_close_icon;
    [SerializeField]
    private Image m_volume_progress_icon;

    private GameObject root;

    private static Queue<Action> ExecuteQueue = new Queue<Action>();
    private float showtime = 0;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.GetChild(0).gameObject;
        root.SetActive(false);
        if (Svr.SvrSetting.IsVR9Device)
        {
            SVR.AtwAPI.OnVolumeChanged = (int currentvolume) =>
            {
                ExecuteQueue.Enqueue(() =>
                {
                    UpdateVolume(currentvolume);
                });
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Svr.SvrSetting.IsVR9Device && (SVR.AtwAPI.VolumeDownKeyDown || SVR.AtwAPI.VolumeUpKeyDown))
        {
            UpdateVolume(SVR.AtwAPI.GetCurrentVolume());
        }
        try
        {
            while (ExecuteQueue.Count > 0)
            {
                ExecuteQueue.Dequeue().Invoke();
            }
        }
        catch (Exception error)
        {
            Debug.LogError("Main Thread Queue Error Info=" + error);
            ExecuteQueue.Clear();
        }

        if (root.activeSelf && Time.time - showtime >= 1f)
        {
            root.SetActive(false);
        }
    }

    private void UpdateVolume(int currentVolume)
    {
        showtime = Time.time;
        root.SetActive(true);
        m_volume_progress_icon.fillAmount = currentVolume / (float)SVR.AtwAPI.MaxVolume;
        m_volume_close_icon.gameObject.SetActive(currentVolume == 0);
        m_volumeicon.gameObject.SetActive(currentVolume != 0);
    }
}
