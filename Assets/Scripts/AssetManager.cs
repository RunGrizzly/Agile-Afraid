using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

[System.Serializable]
public class AssetManifest
{
    public string version; 
    public AssetBundleInfo[] bundles;
}

[System.Serializable]
public class AssetBundleInfo
{
    public string name;
    public string url; 
    public uint crc;
}

public class AssetManager : MonoBehaviour
{
    public RunSettings TestRunSettings = null;
    public LevelSet DeliveredLevelSet = null;

    private void Start()
    {
        //Immediately grab manifest levels
        TryFetchManifest();
    }

    public static string SanitizeJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        // Remove BOM if present
        if (json[0] == '\uFEFF')
            json = json.Substring(1);

        // Trim leading/trailing whitespace and line breaks
        json = json.Trim();

        return json;
    }

    [Button]
    public void TryInspectBundle()
    {
        StartCoroutine(InspectBundle("https://petwords-content-delivery.netlify.app/asset_bundle_manifest.json"));
    }
    
    public IEnumerator InspectBundle(string bundleUrl)
    {
        // Step 1: Download bundle
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download bundle: {www.error}");
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle from downloaded data.");
                yield break;
            }

            Debug.Log($"Loaded AssetBundle: {bundle.name}");

            // Step 2: List all assets
            string[] assetNames = bundle.GetAllAssetNames();
            Debug.Log($"Assets in bundle ({assetNames.Length}):");
            foreach (var name in assetNames)
            {
                Debug.Log(" - " + name);
            }

            // Step 3: Inspect ScriptableObjects for broken references
            foreach (var name in assetNames)
            {
                var request = bundle.LoadAssetAsync<ScriptableObject>(name);
                yield return request;

                ScriptableObject so = request.asset as ScriptableObject;
                if (so != null)
                {
                    Debug.Log($"Inspecting SO: {so.name}");

#if UNITY_EDITOR
                    // Use SerializedObject to inspect fields safely in editor
                    var serialized = new UnityEditor.SerializedObject(so);
                    var prop = serialized.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                        {
                            Debug.LogFormat($"Inspecting reference in SO '{so.name}', field '{prop.name}'");
                            
                            if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                            {
                                Debug.LogWarning($"Broken reference in SO '{so.name}', field '{prop.name}'");
                            }
                        }
                    }
#endif
                }
            }

            // Step 4: Unload the bundle but keep loaded assets if needed
            bundle.Unload(false);
            Debug.Log("Finished inspecting AssetBundle.");
        }
    }


    [Button]
    public void TryFetchManifest()
    {
        StartCoroutine(FetchManifest("https://rungrizzly.github.io/petwords-content/asset_bundle_manifest.json"));
    }
    
    private IEnumerator FetchManifest(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch manifest: {www.error}");
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log($"Raw JSON:\n{json}");

            AssetManifest manifest = null;
            try
            {
                manifest = JsonUtility.FromJson<AssetManifest>(SanitizeJson(json));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse JSON: {ex.Message}");
            }

            if (manifest != null)
            {
                Debug.Log($"Manifest version: {manifest.version}");
                if (manifest.bundles != null)
                {
                    Debug.Log($"Found {manifest.bundles.Length} bundle(s):");
                    foreach (var bundle in manifest.bundles)
                    {
                        Debug.Log($" - {bundle.name} @ {bundle.url} (crc: {bundle.crc})");
                        TryLoadBundle(bundle);
                    }
                }
            }
        }
    }
    
    // Call this with your parsed bundle info
    public void TryLoadBundle(AssetBundleInfo bundleInfo)
    {
        StartCoroutine(LoadBundle(bundleInfo));
    }

    private IEnumerator LoadBundle(AssetBundleInfo bundleInfo)
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleInfo.url, bundleInfo.crc, 0))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download bundle '{bundleInfo.name}': {uwr.error}");
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            
            if (bundle == null)
            {
                Debug.LogError($"Bundle '{bundleInfo.name}' is empty or invalid");
                yield break;
            }

            Debug.Log($"Successfully loaded bundle '{bundleInfo.name}'");

            //Object[] allAssets = bundle.LoadAllAssets();
            
            string[] assetNames = bundle.GetAllAssetNames();

            foreach (var name in assetNames)
            {
                Debug.Log($"Asset: {name}");
            }

            var v = bundle.LoadAllAssetsAsync();

            while (!v.isDone)
            {
                Debug.LogFormat($"Loading bundle assets {v.progress}");
                yield return null;
            }
            
            Object[] allAssets = v.allAssets;
            
            foreach (var a in allAssets)
            {
                Debug.Log($"{a.name} ({a.GetType().Name})");

                if (a is LevelSet so)
                {
                    Debug.LogFormat($"Bundle has a scriptable object with {so.Levels.Count} levels");
                    DeliveredLevelSet = so;
                }
            }
            
            
            // Example: Load a specific asset from the bundle
            
            //Lets not do this yet
            // string assetName = "MyPrefab"; // replace with actual asset name
            // GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
            // if (prefab != null)
            // {
            //     Instantiate(prefab);
            // }

            // Optionally unload the bundle from memory when done
            // bundle.Unload(false); // keep loaded assets
        }
    }
}
