using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using SubjectNerd.PsdImporter.PsdParser;

[ScriptedImporter(1, "psb")]
public class PsbImporter : ScriptedImporter
{
    public float pixelsPerUnit = 100;
    public Vector2 pivot = new Vector2(.5f, 0);
    public FilterMode filterMode;

    //根据Sprite的锚点计算初始位置
    Vector2 originPos { get { return pivot * psd.Width / pixelsPerUnit; } }

    PsdDocument psd;

    GameObject go;

    //当前图层顺序
    int orderInLayer;

    AssetImportContext ctx;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string name = GetFileName(ctx.assetPath);
        go = new GameObject(name);

        ctx.AddObjectToAsset(name, go);
        ctx.SetMainObject(go);

        this.ctx = ctx;

        LoadPSD(ctx.assetPath);
        CreateTextures();

        UnloadPSD();
    }

    //加载PSD
    void LoadPSD(string path)
    {
        //判断是ps文件
        if (path.EndsWith(".psb"))
        {
            psd = PsdDocument.Create(path);
        }
    }
    //卸载PSD文件
    void UnloadPSD()
    {
        psd.Dispose();

        psd = null;
    }

    void CreateTextures()
    {
        CreateLayers(psd.Childs, go.transform);
    }

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
                    sprite = Sprite.Create(tex, new Rect(0, 0, layer.Width, layer.Height), new Vector2(.5f, .5f), pixelsPerUnit);
                    sprite.name = layer.Name;

                    ctx.AddObjectToAsset("tex_" + tex.name, tex);
                    ctx.AddObjectToAsset("sprite_" + sprite.name, sprite);
                }

                //设置贴图
                SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;

                //设置图层顺序
                spriteRenderer.sortingOrder = orderInLayer;
                orderInLayer++;

                //sprites.Add(sprite);

                int bottom = psd.Height - layer.Bottom;

                Vector2 pos = new Vector2(layer.Left + layer.Width / 2f, bottom + layer.Height / 2f) / pixelsPerUnit;
                pos -= originPos;

                go.transform.position = pos;

                ctx.AddObjectToAsset(go.name, go);
            }

            //如果有子物体，递归创建下去
            if (layer.Childs.Length > 0)
            {
                CreateLayers(layer.Childs, go.transform);
            }
        }
    }

    //从路径获取文件名
    private string GetFileName(string assetPath)
    {
        string[] parts = assetPath.Split('/');
        string filename = parts[parts.Length - 1];

        return filename.Substring(0, filename.LastIndexOf('.'));
    }

    //生成PS图层的贴图
    Texture2D GetTexture2D(IPsdLayer layer)
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

        tex.filterMode = filterMode;

        return tex;
    }

}
