using System;
using System.Collections;
using System.Collections.Generic;
using LuaFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 资源管理待拓展 
/// 实现功能 对象缓存 bundle缓存 定时销毁
/// Manifest加载
/// bundle下载地址
/// </summary>
public class AssetManager : MonoSingleton<AssetManager> {

    private readonly Dictionary<UnityEngine.Object, string> assetCache = new Dictionary<UnityEngine.Object, string> ();
    private AssetBundleManifest manifest = null;
    /// <summary>
    /// Manifest加载分两种一种是 在运行时加载本地的  之后对比版本号开始更新是要远程加载
    /// </summary>
    public void LoadManifest () {
    }
    public UnityEngine.Object LoadObject (string path) {
        AssetID id = Util.GetAssetID (path);
        return LoadObject (id.AssetName, id.BundleName);
    }
    public UnityEngine.Object LoadObject (AssetID id) {

        return LoadObject (id.AssetName, id.BundleName);
    }
    /// <summary>
    /// 在最终的加载是以bundleName加载到bundle再从bundle里loadasset
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadObject (string assetName, string bundleName) {
        var result = AssetBundle.LoadFromFile (bundleName);
        UnityEngine.Object target = result.LoadAsset (assetName);
        return target;
    }

    public void LoadObjectASync (string path, Action<UnityEngine.Object> callback) {
        AssetID id = Util.GetAssetID (path);
        LoadObjectASync (id.AssetName, id.BundleName, callback);
    }
    public void LoadObjectASync (AssetID id, Action<UnityEngine.Object> callback) {
        LoadObjectASync (id.AssetName, id.BundleName, callback);
    }
    public void LoadObjectASync (string assetName, string bundleName, Action<UnityEngine.Object> callback) { 
        //一个携程
        StartCoroutine(LoadAssetBundle(bundleName));
    }
    IEnumerator LoadAssetBundle(string bundleName){

        yield return null;
        yield return AssetBundle.LoadFromFileAsync(bundleName);
    }
    public AssetBundle LoadBundleSync(string bundleName){
        return AssetBundle.LoadFromFile (bundleName);
    }
    public void LoadScene () {
        string sceneName = "";
        //这种方式要在buildsetting中加入场景配置
        SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }
    public void LoadSceneASync () { 
        string sceneName = "";
        SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Single);
    }
}