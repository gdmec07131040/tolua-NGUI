public interface IPool {
    UnityEngine.Object GetObject ();
    void Recycle (UnityEngine.Object obj);
    void Clear ();
    void Dispose ();
    void Flush(float delta);
    int RefCount { get; set; }
    bool IsDisposeable { get; }
    AssetID id { get; }
    UnityEngine.Object source { set; get; }
    PoolStrategy Strategy {set;get;}
}