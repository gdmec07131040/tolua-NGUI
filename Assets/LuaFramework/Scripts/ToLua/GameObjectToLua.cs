using System;
using UnityEngine;
public class GameObjectToLua
{
    public static int New()
    {
        GameObject obj = new GameObject();
        int pos = LuaObjectPool.Instance.New(obj,0);
        return pos;
    }
    public static GameObject Get(int pos){
        GameObject obj = LuaObjectPool.Instance.Get(pos);
        return obj;
    }
    public static GameObject Destroy(int pos){
        GameObject obj = LuaObjectPool.Instance.Destroy(pos);
        return obj;
    }
}