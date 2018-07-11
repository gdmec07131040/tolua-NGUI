using System.Collections;
using LuaInterface;
using UnityEngine;
using System.IO;
namespace LuaFramework {
    /// <summary>
    /// 框架主入口
    /// </summary>
    public class Main : MonoBehaviour {
        void Start () {
            AppFacade.Instance.StartUp (); //启动游戏
        }
        void LateUpdate() {
            
        }
    }
} 