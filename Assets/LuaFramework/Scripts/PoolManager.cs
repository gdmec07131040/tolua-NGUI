using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaFramework;
using UnityEngine;
using Object = UnityEngine.Object;
public class PoolManager : MonoSingleton<PoolManager> {
    private static Dictionary<AssetID, IPool> mPools = new Dictionary<AssetID, IPool> ();
    private static Dictionary<Object, ActiveInfo> mActivePools = new Dictionary<Object, ActiveInfo> ();
    private static Queue<LoadItem> allLoad = new Queue<LoadItem> (); //队列 先进先出 加载队列
    //堆栈 后进先出
    private PoolStrategy defaultStrategy = null;
    private List<AssetID> disposeCache;
    private float checkTimeCache = 0f;
    private static GameObject mPoolContainer;
    internal static Transform PoolContainer {
        get {
            if (mPoolContainer == null) {
                mPoolContainer = new GameObject ("PoolContainer");
                DontDestroyOnLoad (mPoolContainer);
            }
            return mPoolContainer.transform;
        }
    }
    private void Awake () {
        defaultStrategy = new PoolStrategy ();
        disposeCache = new List<AssetID> ();
    }
    private void StartCheckGC () {

    }
    private void StopCheckGC () {

    }

    IEnumerator CheckGC () {
        while (true) {
            yield return new WaitForSeconds (60f);

            break;
        }
    }
    private void LateUpdate () {
        //检查并回收垃圾
        this.checkTimeCache += Time.deltaTime;
        if (this.checkTimeCache > 1f) {
            foreach (var item in mPools) {
                //执行内部update
                //如果可以删除就遵循删除策略删除
                item.Value.Flush (this.checkTimeCache);
                if (item.Value.IsDisposeable) {
                    this.disposeCache.Add (item.Key);
                }
            }
            this.checkTimeCache = 0f;
            for (int i = 0; i < this.disposeCache.Count; i++) {
                AssetID temp = this.disposeCache[i];
                if (mPools.ContainsKey (temp)) {
                    IPool pool = mPools[temp];
                    mPools.Remove (temp);
                    pool.Dispose ();
                }
            }
            this.disposeCache.Clear ();
        }

        //检查加载队列并回调
        while (allLoad.Count > 0) {
            CheckLoadQueue (allLoad.Dequeue ());
        }
    }
    public void CheckLoadQueue (LoadItem loadItem) {
        IPool pool;
        if (mPools.TryGetValue (loadItem.id, out pool)) {
            Object target = pool.GetObject ();
            pool.RefCount--;
            if (target == null) {
                Debug.LogError ("获取对象为空");
                return;
            }
            AddActivePoolInfo (target, pool.id);
            loadItem.callback (target);
        } else {
            Debug.LogError ("没找到池子");
        }
    }
    /// <summary>
    /// 先从总池子里获取 获取不到就开始执行加载 因为加载问题无法立即返回 所以只能用ACTION
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetName"></param>
    /// <param name="callback"></param>
    public void Get (string path, string assetName, Action<Object> callback) {
        //获取池子 如果存在那就拿出来 
        AssetID id = Util.GetAssetID (path);
        Object target;
        IPool pool = GetPool (id);
        if (pool == null) {
            //创建并进行加载
            Debug.LogError ("池子不存在");
            CreatePool (id, out pool);
            //因为创建不一定能直接拿到资源
            //pool = new ResourcePool(id);

            //target = pool.GetObject ();

            // callback(target);
            AddLoadQueue (pool, callback); //加载完调用
        } else {
            //直接回调 这里也可以改成加到加载队列
            if (callback != null) {
                Debug.LogError ("存在池子");
                //考虑在池子拿出去的都放在一个空obj下
                target = pool.GetObject ();
                Debug.LogError ("获取到对象");
                AddActivePoolInfo (target, id);
                callback (target);
            }
        }
        //Instance.StartCheckGC ();
    }
    public void CreatePool (AssetID id, out IPool pool) {
        //IPool pool = null;
        pool = new ResourcePool (id); //赋值策略
        //TODO 通过资源管理器加载后赋值 并加入到总池 这里不回调
        //Debug.Log(id.AssetName);
        Debug.LogError ("新建池子");
        Object res = Resources.Load (id.AssetName) as Object;
        pool.Strategy = defaultStrategy;
        AddPool (pool, res);
        //return pool;
    }
    public IPool GetPool (AssetID id) {
        IPool pool = null;
        mPools.TryGetValue (id, out pool);
        return pool;
    }
    public void AddLoadQueue (IPool pool, Action<Object> callback) {
        pool.RefCount++;
        allLoad.Enqueue (new LoadItem (pool.id, callback));
    }
    public void AddPool (IPool pool, Object source) {
        pool.source = source;
        if (mPools.ContainsKey (pool.id)) {
            //已经存在池子
            Debug.LogError ("存在池子？");
            if (pool.source == null) {
                pool.source = source;
            } else {
                //回收重复加载的资源 正常不会发生
            }
        } else {
            mPools.Add (pool.id, pool);
        }
    }

    public void Recycle (Object target) {
        //回收
        ActiveInfo info;
        if (mActivePools.TryGetValue (target, out info)) {
            DecActivePoolInfo (target);
            if (mPools.ContainsKey (info.id)) {
                IPool pool = mPools[info.id];
                pool.Recycle (target);
            } else {
                Object.Destroy (target);
            }
        } else {
            Object.Destroy (target);
        }
    }
    public void RemovePool (IPool pool) {
        pool.Dispose ();
        mPools.Remove (pool.id);
    }
    private bool TryRemovePool (IPool pool) {
        if (pool.IsDisposeable) {

        }
        return false;
    }

    private bool TryClearPool (IPool pool) {
        return false;
    }
    private void ClearPool () {

    }

    /// <summary>
    /// 在最后回调的时候调用
    /// </summary>
    /// <param name="target"></param>
    /// <param name="id"></param>
    private void AddActivePoolInfo (Object target, AssetID id) {
        if (target == null) return;
        ActiveInfo info;
        if (!mActivePools.TryGetValue (target, out info)) {
            info = new ActiveInfo (target, id);
            mActivePools.Add (target, info);
        }
        info.AddCount ();
    }
    /// <summary>
    /// 回收时对激活池进行减处理
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private ActiveInfo DecActivePoolInfo (Object target) {
        ActiveInfo info = null;
        if (mActivePools.TryGetValue (target, out info)) {
            info.DecCount ();
            if (info.IsEmpty) {
                mActivePools.Remove (target);
                info.Dispose ();
            }
        }
        return info;
    }
    protected override void OnDestroy () {
        defaultStrategy = null;
        base.OnDestroy ();
    }
}
class ActiveInfo {
    public Object target { set; get; }
    public AssetID id { set; get; }
    public int count { set; get; }

    public ActiveInfo (Object targetValue, AssetID idValue) {
        target = targetValue;
        id = idValue;
    }
    public void AddCount () {
        count++;
    }
    public void DecCount () {
        count = count > 0 ? --count : 0;
    }
    public bool IsEmpty {
        get { return count <= 0; }
    }
    public void Dispose () {
        target = null;
    }
}
public struct AssetID {
    private string assetName;
    private string bundleName;

    public AssetID (string assetName, string bundleName) {
        this.assetName = assetName;
        this.bundleName = bundleName;
    }
    public string AssetName {
        get {
            return this.assetName;
        }
    }
    public string BundleName {
        get {
            return this.bundleName;
        }
    }
}

/// <summary>
/// 池子策略
/// </summary>
public class PoolStrategy {
    //回收后多久删除（回收后检查超出最小的部分直接删除）
    private float recycleTime = 12f;
    //池子的最小数量 （能够在池子里的最小数量）
    private int minSize = 4;
    //什么时候销毁池子 （如果池子没有对象 超过改时间直接删除池子）
    private float destroyTime = 30f;

    internal float RecycleTime { get { return this.recycleTime; } }
    internal int MinSize { get { return this.minSize; } }
    internal float DestroyTime { get { return this.destroyTime; } }
    public PoolStrategy () {

    }
}
/// <summary>
/// 加载队列
/// </summary>
public class LoadItem {
    public readonly AssetID id;
    public readonly Action<Object> callback;

    public LoadItem (AssetID idValue, Action<Object> callbackValue) {
        this.id = idValue;
        this.callback = callbackValue;
    }
}