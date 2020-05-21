using UnityEngine;
using System.Collections;

public class AnchorDemo : MonoBehaviour
{

    private Vector3 rot = new Vector3(0, 3, 0);

    void Start()
    {
        AnchorWidget aw = VREventListener.Get(gameObject);

        aw.onHover = (GameObject go, bool isHover) => {
            if (isHover)
            {
                StartCoroutine("Rotation");
            }
            else
            {
                StopCoroutine("Rotation");
            }
        };

        aw.OnClickEvent = (GameObject go) =>
        {
            rot = new Vector3(rot.z, rot.x, rot.y);
        };

    }

    IEnumerator Rotation()
    {
        while (true)
        {
            transform.Rotate(rot);
            yield return null;
        }
    }

}
