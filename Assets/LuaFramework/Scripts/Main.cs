﻿using System;
using System.Collections;
using System.IO;
using LuaInterface;
using UnityEngine;

namespace LuaFramework {
    /// <summary>
    /// 框架主入口
    /// </summary>
    public class Main : MonoBehaviour {
        void Start () {
            AppFacade.Instance.StartUp (); //启动游戏
        }
    }
}