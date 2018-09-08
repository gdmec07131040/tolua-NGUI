public class Singleton<T> where T : class, new () {
    private static T _instance = null;
    private static readonly object locker = new object ();
    private Singleton () { }
    public static T Instance {
        get {
            if (_instance == null) {
                lock (locker) {
                    if (_instance == null) {
                        _instance = new T ();
                    }
                }
            }
            return _instance;
        }
    }
}