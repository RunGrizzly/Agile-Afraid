using UnityEditor;
using UnityEngine;

public class BuildBundles
{
    [MenuItem("Build/Build AssetBundles")]
    static void BuildAllBundles()
    {
        string outputPath = "D:\\UnityProjects\\RunGrizzly\\petwords-content\\contents";
        
        if (!System.IO.Directory.Exists(outputPath))
        {
            System.IO.Directory.CreateDirectory(outputPath);   
        }
        
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.WebGL);
        Debug.Log("AssetBundles built to " + outputPath);
    }
}