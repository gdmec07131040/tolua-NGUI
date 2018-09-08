using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LuaObjectPool {
    private static LuaObjectPool _instance;
    public static readonly int NULL = 0;
    public static LuaObjectPool Instance {
        get {
            if (_instance == null) {
                _instance = new LuaObjectPool ();
            }
            return _instance;
        }
    }
    class PoolNode {
        public int index;
        public GameObject obj; //只针对GameObject
        public int parent = 0; //默认为head(空)
        public Transform mTransform;
        public Dictionary<int, Component> mComponents = new Dictionary<int, Component> (); //存储组件
        public List<int> mChilds = new List<int> ();
        public Transform transform {
            get {
                if (mTransform == null) {
                    if (obj != null) {
                        mTransform = obj.transform;
                    }
                }
                return mTransform;
            }
        }
        public PoolNode (int index, GameObject obj) {
            this.index = index;
            this.obj = obj;

        }
        public Component GetComponent (int index) {
            if (mComponents.ContainsKey (index)) {
                return mComponents[index];
            } else {
                return null;
            }
        }
        public int GetComponent (Component com) {
            Dictionary<int, Component>.Enumerator e = mComponents.GetEnumerator ();
            /* for (int i = 0; i != mComponents.Count; i++) {
                e.MoveNext ();
                KeyValuePair<int, Component> pair = e.Current;
                if (pair.Value == com) {
                    return pair.Key;
                }
            } */
            while (e.MoveNext ()) {
                if (com == e.Current.Value)
                    return e.Current.Key;
            }
            return 0;
        }
        public void AddChild (int handle) {
            if (mChilds.Contains (handle)) {
                return;
            }
            mChilds.Add (handle);
        }
        public void RemoveChild (int handle) {
            if (!mChilds.Contains (handle)) {
                return;
            }
            mChilds.Remove (handle);
        }
    }
    private List<PoolNode> list;
    //同lua_ref策略，0作为一个回收链表头，不使用这个位置
    private PoolNode head = null;
    private int count = 0;
    private int comStart = 10000;
    public LuaObjectPool () {
        list = new List<PoolNode> ();
        head = new PoolNode (0, null);
        list.Add (head);
        //list.Add (new PoolNode (1, null));
        count = list.Count;
    }
    /// <summary>
    /// 用法:LuaObjectPool.Instance[pos]
    /// </summary>
    /// <returns></returns>
    public GameObject this [int i] {
        get {
            if (i > 0 && i < count) {
                return list[i].obj;
            }
            return null;
        }
    }
    public void Clear () {
        list.Clear ();
        head = null;
        count = 0;
        _instance = null;
    }
    //需要检查避免重复
    public int New (GameObject obj, int parent) {
#if UNITY_EDITOR
        for (int i = 0; i < list.Count; i++) {
            PoolNode node = list[i];
            if (node != null && node.obj == obj) {
                Debug.Log ("GameObject Get Twice Obj Name = " + obj.name);
            }
        }
#endif
        int pos = Add (obj, parent);
        return pos;

    }
    public int Add (GameObject obj, int parent) {
        int pos = -1;
        if (head.index != 0) {
            pos = head.index;
            PoolNode node = list[pos];
            node.obj = obj;
            head.index = node.index;
            if (parent > 0 && parent < count) {
                node.parent = parent;
                list[parent].AddChild (node.index);
            }
        } else {
            pos = list.Count;
            list.Add (new PoolNode (pos, obj));
            list[pos].parent = parent;
            count = pos + 1;
        }
        return pos;
    }
    public GameObject Get (int pos) {
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            if (o != null) {
                return o;
            }
        }
        return null;
    }
    public Transform GetTransform (int pos) {
        if (pos > 0 && pos < count) {
            PoolNode node = list[pos];
            return node.transform;
        }
        return null;
    }
    //关于Destory的一些问题
    //调用前删除(对象)
    //是否child需要从List中移除
    //是否父对象需要删除子对象
    //删除成为head
    public GameObject Destroy (int pos, bool detach = true) {
        if (pos > 0 && pos < count) {
            PoolNode node = list[pos];
            GameObject o = node.obj;
            node.obj = null;
            node.index = head.index;
            node.mComponents.Clear ();
            for (int i = 0; i != node.mChilds.Count; i++) {
                Destroy (node.mChilds[i], false); //子物体删除
            }
            node.mChilds.Clear ();
            head.index = pos; //下一个Add使用此位置
            if (detach && node.parent != 0) {
                PoolNode parent = list[node.parent];
                parent.RemoveChild (pos);
            }
            return o;
        }
        return null;
    }

    public int comIndexToObjIndex (int comIndex) {
        return comIndex / comStart;
    }
    public int AddComponent (int pos, Component com) {
        if (pos > 0 && pos < count) {
            //GameObject o = list[pos].obj;
            int comCount = list[pos].mComponents.Count;
            int index = (comCount++) + comStart * pos; //可反向找到o
            list[pos].mComponents.Add (index, com);
            return index;
        }
        return 0;
    }
    public int GetComponent (int pos, Type type) {
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            Component com = o.GetComponent (type);
            if (com == null) {
                return 0;
            }
#if UNITY_EDITOR
            int index = list[pos].GetComponent (com);
            if (index == 0)
#else
                int index = 0;
#endif
            {
                index = AddComponent (pos, com);
            };
            return index;
        }
        return 0;

    }
    public Component GetComponent (int comIndex) {
        int pos = comIndexToObjIndex (comIndex);
        if (pos > 0 && pos < count) {
            Component com = list[pos].GetComponent (comIndex);
            if (com != null) {
                return com;
            }
        }
        return null;
    }
}