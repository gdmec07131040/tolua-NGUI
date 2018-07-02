using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance = default (T);
    public static T Instance {
        get {
            return _instance;
        }
    }
    private GameObject mGameManager;
    private GameObject GameManager {
        get {
            if (mGameManager == null) {
                mGameManager = GameObject.Find ("GameManager");
                DontDestroyOnLoad (mGameManager);
            }
            return mGameManager;
        }
    }
    protected virtual void Awake () {
        if (_instance == null) {
            _instance = GameManager.GetComponent<T> ();
        }
    }

    protected virtual void OnDestroy () {
        if (_instance == this) {
            _instance = default (T);
        }
    }
}