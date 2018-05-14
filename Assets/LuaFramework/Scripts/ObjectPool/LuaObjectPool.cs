using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaObjectPool {
    class PoolNode {
        public int index;
        public GameObject obj; //只针对GameObject
        public int parent = 0; //默认为head(空)
        public Transform mTransform;
        public Dictionary<int, Component> mComponents = new Dictionary<int, Component> (); //存储组件
        public List<int> mChilds = new List<int>();
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
            for (int i = 0; i != mComponents.Count; i++) {
                e.MoveNext ();
                KeyValuePair<int, Component> pair = e.Current;
                if (pair.Value == com) {
                    return pair.Key;
                }
            }
            return 0;
        }
        public void AddChild(int handle){
            if (mChilds.Contains(handle)){
                return;
            }
            mChilds.Add(handle);
        }
        public void RemoveChild(int handle){
            if (!mChilds.Contains(handle)){
                return;
            }
            mChilds.Remove(handle);
        }
    }
    private List<PoolNode> list;
    //同lua_ref策略，0作为一个回收链表头，不使用这个位置
    private PoolNode head = null;
    private int count = 0;
    private int iComStart = 10000;
    public LuaObjectPool () {
        list = new List<PoolNode> ();
        head = new PoolNode (0, null);
        list.Add (head);
        list.Add (new PoolNode (1, null));
        count = list.Count;
    }
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
    }
    public int Add (GameObject obj) {
        int pos = -1;
        if (head.index != 0) {
            pos = head.index;
            list[pos].obj = obj;
            head.index = list[pos].index;
        } else {
            pos = list.Count;
            list.Add (new PoolNode (pos, obj));
            count = pos + 1;
        }
        return pos;
    }
    public GameObject Get(int pos){
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            if(o != null){
                return o;
            }
        }
        return null;
    }
    public GameObject Remove (int pos) {
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            list[pos].obj = null;
            list[pos].index = head.index; //头部必定是空
            list[pos].mComponents.Clear();
            list[pos].mChilds.Clear();
            head.index = pos;//下一个Add使用此位置
            return o;
        }
        return null;
    }
    public GameObject Destroy (int pos) {
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            list[pos].obj = null;
            return o;
        }
        return null;
    }
    public int comIndexToObjIndex (int comIndex) {
        return comIndex / iComStart;
    }
    public int AddComponent (int pos, Component com) {
        if (pos > 0 && pos < count) {
            GameObject o = list[pos].obj;
            int comCount = list[pos].mComponents.Count;
            int index = (comCount++) + iComStart * pos; //可反向找到o
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
            int index = list[pos].GetComponent (com);
            if (index == 0) {
                //存在但是没有存储
                return AddComponent (pos, com);
            }
        }
        return 0;
    }
    public Component GetComponent (int comIndex) {
        int pos = comIndexToObjIndex (comIndex);
        if (pos > 0 && pos < count) {
            Component mCom = list[pos].GetComponent (comIndex);
            if (mCom != null) {
                return mCom;
            }
        }
        return null;
    }
}