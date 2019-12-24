using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SubjectNerd.PsdImporter.PsdParser;

[ExecuteInEditMode]
public class SceneMapImporter : MonoBehaviour
{
    //文件
    public Sprite spriteFile;

    PsdDocument psd;

    [SerializeField, HideInInspector]
    List<Sprite> sprites;

    //根据Sprite的锚点计算初始位置
    Vector2 originPos { get { return spriteFile.pivot / spriteFile.pixelsPerUnit; } }

    //当前图层顺序
    int orderInLayer;

    #region 拖动文件自动生成

    //[UnityEditor.InitializeOnLoadMethod]
    //static void AutoCreateMethod()
    //{
    //    EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;

    //    EditorApplication.hierarchyChanged += OnHierarchyChanged;
    //}

    ////当添加物体到Hierarchy
    //static void HierarchyWindowItemCallback(int pID, Rect pRect)
    //{
    //    if (!pRect.Contains(Event.current.mousePosition))
    //        return;

    //    Debug.Log("qwe");

    //    //当前位于鼠标下的物体
    //    GameObject targetGo = UnityEditor.EditorUtility.InstanceIDToObject(pID) as GameObject;

    //    Debug.Log(targetGo.name);

    //    //不能是拖动到Canvas中
    //    if (targetGo == null || targetGo.GetComponentInParent<Canvas>() != null)
    //        return;

    //    if (Event.current.type == EventType.DragUpdated)
    //    {
    //        Debug.Log("DragUpdated");

    //        return;

    //        foreach (string path in UnityEditor.DragAndDrop.paths)
    //        {
    //            if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
    //            {
    //                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Link;
    //                UnityEditor.DragAndDrop.AcceptDrag();
    //                Event.current.Use();
    //            }
    //        }
    //    }
    //    else if (Event.current.type == EventType.DragPerform)
    //    {
    //        Debug.Log("DragPerform");

    //        return;

    //        foreach (string path in UnityEditor.DragAndDrop.paths)
    //        {
    //            if (!string.IsNullOrEmpty(path) && path.EndsWith(".psd"))
    //            {
    //                Object asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
    //                GameObject go = new GameObject(asset.name);
    //                go.transform.SetParent(targetGo.transform, false);
    //                go.AddComponent<UIPSDViewer>().asset = asset;
    //                Event.current.Use();
    //            }
    //        }
    //    }
    //}

    //static void OnHierarchyChanged()
    //{
    //    Debug.Log("lemonjuice");


    //}

    #endregion

#if UNITY_EDITOR

    //void OnValidate()
    //{
    //    if (asset != null)
    //    {
    //        string path = AssetDatabase.GetAssetPath(asset);

    //        //判断格式是psb
    //        if (path.EndsWith(".psd"))
    //        {
    //            Debug.Log("psb");
    //            PsdDocument psd = PsdDocument.Create(path);

    //            //创建贴图子物体
    //            CreateLayers(psd.Childs, transform);
    //        }
    //    }
    //}


    //根据设置的文件生成图层
    public void GenerateLayers()
    {
        if (spriteFile != null)
        {
            LoadPSD();

            //ClearTextures();
            InitTextures();

            //创建贴图子物体
            //CreateLayers(psd.Childs, transform);

            //AssetDatabase.Refresh();

            //UnloadPSD();

            //Material mat = new Material(Shader.Find("Specular"));
            //AssetDatabase.CreateAsset(mat, "Assets/QWE.mat");

            AnimationClip anim = new AnimationClip();
            anim.name = "anim2";
            AssetDatabase.AddObjectToAsset(anim, obj);

            Texture2D tex = new Texture2D(3, 4);
            tex.name = "tex3";
            AssetDatabase.AddObjectToAsset(tex, obj);


            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(anim));
        }
    }

    void LoadPSD()
    {
        string path = AssetDatabase.GetAssetPath(spriteFile);

        //判断是ps文件
        if (path.EndsWith(".psd"))
        {
            psd = PsdDocument.Create(path);
        }
    }

    void UnloadPSD()
    {
        psd.Dispose();

        psd = null;
    }

    void InitTextures()
    {
        sprites = new List<Sprite>();


    }

    void ClearTextures()
    {
        if (sprites == null)
            return;

        while(sprites.Count > 0)
        {
            DestroyImmediate(sprites[0].texture, true);
            DestroyImmediate(sprites[0], true);

            sprites.RemoveAt(0);
        }

        sprites = null;

        orderInLayer = 0;

        //清除子物体
        while(transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public Object obj;

    //输入PSD图层来创建图层
    void CreateLayers(IPsdLayer[] layers, Transform parent)
    {
        foreach (var layer in layers)
        {
            GameObject go = new GameObject(layer.Name);
            go.transform.SetParent(parent);
            go.transform.SetAsFirstSibling();

            //如果是空图层，不设置贴图
            if (layer.Width > 0 && layer.Height > 0)
            {
                Sprite sprite = null;
                //寻找同名贴图
                //string[] findedSprites = UnityEditor.AssetDatabase.FindAssets(layer.Name + " t:sprite");
                //if (findedSprites.Length > 0)
                //{
                //    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(findedSprites[0]));
                //}

                //如果没有同名贴图
                if (sprite == null)
                {
                    Texture2D tex = GetTexture2D(layer);
                    tex.name = layer.Name;
                    sprite = Sprite.Create(tex, new Rect(0, 0, layer.Width, layer.Height), new Vector2(.5f, .5f), spriteFile.pixelsPerUnit);
                    AssetDatabase.AddObjectToAsset(tex, AssetDatabase.GetAssetPath(obj));
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj));
                }

                //设置贴图
                SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;

                //设置图层顺序
                spriteRenderer.sortingOrder = orderInLayer;
                orderInLayer++;

                sprites.Add(sprite);

                int bottom = psd.Height - layer.Bottom;

                Vector2 pos = new Vector2(layer.Left + layer.Width / 2f, bottom + layer.Height / 2f) / spriteFile.pixelsPerUnit;
                pos -= originPos;

                go.transform.position = (Vector2)transform.position + pos;
            }

            //如果有子物体，递归创建下去
            if(layer.Childs.Length > 0)
            {
                CreateLayers(layer.Childs, go.transform);
            }
        }
    }

    [MenuItem("GameObject/Create Mat")]
    static void CreateMaterial()
    {
        Material mat = new Material(Shader.Find("Specular"));
        AssetDatabase.CreateAsset(mat, "Assets/QWE.mat");

        AnimationClip anim = new AnimationClip();
        anim.name = "anim";
        AssetDatabase.AddObjectToAsset(anim, mat);

        Texture2D tex = new Texture2D(3, 4);
        tex.name = "tex";
        AssetDatabase.AddObjectToAsset(tex, mat);


        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(anim));
    }

    //生成PS图层的贴图
    public Texture2D GetTexture2D(IPsdLayer layer)
    {
        byte[] data = layer.MergeChannels();
        var channelCount = layer.Channels.Length;
        var pitch = layer.Width * layer.Channels.Length;
        var w = layer.Width;
        var h = layer.Height;

        var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
        var tex = new Texture2D(w, h, format, false);
        var colors = new Color32[data.Length / channelCount];

        var k = 0;
        for (var y = h - 1; y >= 0; --y)
        {
            for (var x = 0; x < pitch; x += channelCount)
            {
                var n = x + y * pitch;
                var c = new Color32();
                if (channelCount == 5)
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    n++;
                    c.a = (byte)Mathf.RoundToInt((float)(data[n++]) * layer.Opacity);
                }
                else if (channelCount == 4)
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    c.a = (byte)Mathf.RoundToInt((float)data[n++] * layer.Opacity);
                }
                else
                {
                    c.b = data[n++];
                    c.g = data[n++];
                    c.r = data[n++];
                    c.a = (byte)Mathf.RoundToInt(layer.Opacity * 255f);
                }
                colors[k++] = c;
            }
        }
        tex.SetPixels32(colors);
        tex.Apply(false, true);

        tex.filterMode = spriteFile.texture.filterMode;

        return tex;
    }


#endif
}
