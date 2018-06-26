using UnityEngine;
using System.Collections;
using LuaInterface;
namespace LuaFramework {
    /// <summary>
    /// 框架主入口
    /// </summary>
    public class Main : MonoBehaviour {

        void Start() {
            AppFacade.Instance.StartUp();   //启动游戏
            int a = GameObjectToLua.New();
            int b = GameObjectToLua.NewWithNameParent("a的子物体", a);
            Debug.LogError(a);
            Debug.LogError(b);
            // GameObjectToLua.Destroy(b);
            int c = GameObjectToLua.NewWithNameParent("b的子物体",LuaObjectPool.NULL);
            
            Debug.LogError(c);
            GameObject d = GameObjectToLua.Get(b);
            GameObjectToLua.SetParent(c,b);
            Debug.LogError(d.name);
            LuaInterface.LuaObjectPool pool = new LuaInterface.LuaObjectPool();
            int f = pool.Add(d);
            Debugger.LogError(f);
        }
    }
}