using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
/// <summary>
/// 资源对象池
/// TODO 删除IPool接口 不需要
/// </summary>
public class ResourcePool : IPool {

    public string assetName { get; private set; }
    public string bundleName { get; private set; }
    private Object source; //存当前使用
    //private Type mType;
    private AssetID mId;
    private List<Object> freelist = new List<Object> (); //已回收列表 需要删除超出部分 而使用List
    private const float DefaultLifeTime = 1800;
    private int refCount = 0; //引用次数 在获取时加处理 回收和回调成功后减处理（当你在使用池子的时候不允许有销毁池子的行为）
    //先理解为使用次数 获取的时候++ 回收的时候--
    private PoolStrategy strategy = null;
    private float recycleTimeCache = 0f;
    private float destroyTimeCache = 0f;
    public ResourcePool (AssetID idValue) {
        this.mId = idValue;
        this.assetName = idValue.AssetName;
        this.bundleName = idValue.BundleName;
    }
    public bool IsDisposeable {
        get {
            return this.refCount <= 0 && this.destroyTimeCache > strategy.DestroyTime;
        }
    }
    public AssetID id {
        get {
            return (mId = new AssetID (assetName, bundleName));
        }
    }
    public void Flush (float delta) {
        this.recycleTimeCache += delta;
        if (this.recycleTimeCache > strategy.RecycleTime && freelist.Count > strategy.MinSize) {
            this.recycleTimeCache = 0f;
            int num = freelist.Count / 2;
            if (num <= strategy.MinSize) { num = strategy.MinSize; }
            for (int i = freelist.Count - 1; i >= num; i--) {
                Object obj = freelist[i];
                freelist.RemoveAt (i);
                Object.Destroy (obj);
            }
            Debug.LogError ("删除超出部分");
            this.destroyTimeCache = 0f;
        }
        if (RefCount < 1) {
            //准备倒计时删除池子
            this.destroyTimeCache += delta;
        }
    }
    /// <summary>
    /// 清空池子
    /// </summary>
    public void Clear () {
        throw new System.NotImplementedException ();
    }
    /// <summary>
    /// 删除池子
    /// </summary>
    public void Dispose () {
        for (int i = 0; i < freelist.Count; i++) {
            Object obj = freelist[i];
            Object.Destroy (obj);
        }
        freelist.Clear ();
        //TODO资源缓存移除
        this.source = null;
        Debug.LogError ("删除池子");
    }
    /// <summary>
    /// 获取对象 获取对象时分情况
    /// </summary>
    /// <returns></returns>
    public Object GetObject () {
        Object target = null;

        if (freelist.Count > 0) //已存在的情况
        {
            target = freelist[0];
            freelist.RemoveAt (0);
            Debug.LogError ("存在回收过的资源");
        } else {
            if (source == null) {
                Debug.LogError ("资源获取为空" + this.mId.AssetName);
                return null;
            }
            target = Object.Instantiate (this.source);
            target.name = this.source.name;
            Debug.LogError ("创建对象！！！");
        }
        this.RefCount++;
        Debug.LogError ("this.RefCount ++ " + this.RefCount);
        return target;
    }
    public void SetObject (Object obj) {
        if (this.source == obj) {
            Debug.LogError ("资源重复设置");
        }
        this.source = obj;
    }
    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj"></param>
    public void Recycle (Object obj) {
        Debug.LogError ("回收++++++++++++++++");
        if (obj == null) { Debug.LogError ("null"); return; }
        if (freelist.Contains (obj)) {
            Debug.LogError ("资源重复回收" + obj.name);
            return;
        }
        //放回池子根节点 TODO UI对象和场景对象区分
        if (strategy != null && freelist.Count > strategy.MinSize) { //strategy.MinSize) {
            Object.Destroy (obj);
            Debug.LogError ("删除！！！！");
        } else {
            GameObject temp = (obj as GameObject);
            temp.SetActive (false);
            temp.transform.SetParent (PoolManager.PoolContainer);
            freelist.Add (obj);
            temp = null;
        }
        this.RefCount--;
        Debug.LogError ("this.RefCount -- " + this.RefCount);
    }

    Object IPool.source {
        get { return source; }
        set { source = value; }
    }

    public int RefCount {
        get {
            return refCount;
        }
        set {
            refCount = value;
        }
    }

    PoolStrategy IPool.Strategy {
        get {
            return this.strategy;
        }

        set {
            this.strategy = value;
        }
    }
}