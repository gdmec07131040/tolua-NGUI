using UnityEngine;
[DisallowMultipleComponent]
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _instance = default (T);
    private static readonly object locker = new object ();

    public static T Instance {
        get {
            if (_instance == null) {
                lock (locker) {
                    if (_instance == null) {
                        GameObject go = new GameObject (typeof (T).Name, typeof (T));
                        GameObject.DontDestroyOnLoad (go);
                        _instance = go.GetComponent<T> ();
                    }
                }
            }
            return _instance;
        }
    }
    protected virtual void OnDestroy () {
        if (_instance == this) {
            _instance = default (T);
        }
    }
}