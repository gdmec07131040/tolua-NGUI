using System;
using System.Collections;
using UnityEngine;
public class BaseManager : MonoBehaviour {
    static GameObject m_GameManager;
    GameObject AppGameManager {
        get {
            if (m_GameManager == null) {
                m_GameManager = GameObject.Find ("GameManager");
            }
            return m_GameManager;
        }
    }
}