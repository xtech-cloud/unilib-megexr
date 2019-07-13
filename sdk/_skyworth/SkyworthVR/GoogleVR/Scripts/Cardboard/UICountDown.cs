using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICountDown : MonoBehaviour
{

    [SerializeField]
    private float m_count = 1;
    public float Count { get { return m_count; } }

    [SerializeField]
    private bool m_Continue = false;
    public bool Continue { get { return m_Continue; } }
}
