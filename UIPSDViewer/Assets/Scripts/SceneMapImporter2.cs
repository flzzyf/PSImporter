using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

[ScriptedImporter(1, "psb")]
public class SceneMapImporter2 : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        //name = GetFileName(ctx.assetPath);
        Debug.Log("qe"); 

        //var t = AssetDatabase.LoadAssetAtPath(ctx.assetPath, typeof(Texture2D));
        //Debug.Log(t.name);

        //ctx.AddObjectToAsset("main obj", cube);
        //ctx.SetMainObject(cube);

        //var material = new Material(Shader.Find("Standard"));
        //material.name = "mat";
        //ctx.AddObjectToAsset("mat", material);
    }
}
