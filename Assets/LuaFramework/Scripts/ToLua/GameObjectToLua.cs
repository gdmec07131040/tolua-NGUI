using System;
using UnityEngine;
public class GameObjectToLua {
    public static int New () {
        GameObject obj = new GameObject ();
        int pos = LuaObjectPool.Instance.New (obj, LuaObjectPool.NULL);
        return pos;
    }
    public static int NewWithNameParent (string name, int parent) {
        GameObject obj = new GameObject (name);
        int pos = LuaObjectPool.Instance.New (obj, parent);
        if(parent != LuaObjectPool.NULL){
            Transform parentTran = LuaObjectPool.Instance.GetTransform(parent);
            Transform tran = obj.transform;
            tran.parent = parentTran;
            tran.localPosition = Vector3.zero;
            tran.localRotation = Quaternion.identity;
            tran.localScale = Vector3.one;
        }
        return pos;
    }
    public static GameObject Get (int pos) {
        GameObject obj = LuaObjectPool.Instance.Get (pos);
        return obj;
    }
    public static GameObject Destroy (int pos) {
        GameObject obj = LuaObjectPool.Instance.Destroy (pos);
        GameObject.Destroy (obj);
        return obj;
    }
    public static void SetParent (int pos, int parent) {
        GameObject obj = LuaObjectPool.Instance.Get (pos);
        Transform parentTran = null;
        if (parent != LuaObjectPool.NULL) {
            parentTran = LuaObjectPool.Instance.GetTransform (parent);
        }
        if (obj != null) {
            Transform tran = obj.transform;
            tran.parent = parentTran;
        }
    }
}