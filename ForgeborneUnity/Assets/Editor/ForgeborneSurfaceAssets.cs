using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Import contract for original matched pigment and packed surface maps.</summary>
public static class ForgeborneSurfaceAssets
{
    const string Root="Assets/Resources/Art/Surfaces/";
    static readonly Dictionary<string,Vector2> Recipes=new Dictionary<string,Vector2>
    {
        {"Graphite",new Vector2(.65f,.00032f)},{"Steel",new Vector2(.70f,.00013f)},
        {"Copper",new Vector2(.75f,.00036f)},{"Ceramic",new Vector2(1.2f,.00030f)},
        {"Rubber",new Vector2(.25f,.00022f)},{"Basalt",new Vector2(2f,.0022f)},
        {"Paint",new Vector2(2f,.0011f)},{"Timber",new Vector2(1f,.0012f)},
        {"CastFloor",new Vector2(1.5f,.00012f)},{"GloveFabric",new Vector2(.12f,.00018f)},
        {"SleeveFabric",new Vector2(.16f,.00013f)}
    };
    public static void Configure(Material material,string role,Color linearColour,Color linearEmission)
    {
        if(!Recipes.TryGetValue(role,out var recipe))
        {
            material.SetFloat("_TextureStrength",0);
            material.SetColor("_BaseColor",linearColour.gamma);
        }
        else
        {
            string textureRole=role=="CastFloor" ? "Basalt" : role;
            var color=Import(textureRole+"_Color",true);var surface=Import(textureRole+"_HROM",false);
            material.SetTexture("_BaseMap",color);material.SetTexture("_SurfaceMap",surface);
            material.SetFloat("_TextureStrength",1);material.SetFloat("_SurfaceTiling",1/recipe.x);
            material.SetFloat("_ReliefMetres",recipe.y);material.SetColor("_BaseColor",Color.white);
        }
        material.SetColor("_EmissionColor",linearEmission.gamma);
        material.SetFloat("_RoughnessBias",role=="CastFloor" ? -.16f : 0);
    }
    static Texture2D Import(string name,bool color)
    {
        string path=Root+name+".png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null)throw new InvalidOperationException("Bake the original surface map before preparation: "+path);
        bool changed=importer.textureType!=TextureImporterType.Default || importer.sRGBTexture!=color
            || importer.alphaIsTransparency || importer.wrapMode!=TextureWrapMode.Repeat || !importer.mipmapEnabled
            || importer.filterMode!=FilterMode.Trilinear || importer.anisoLevel!=8 || importer.isReadable
            || importer.textureCompression!=TextureImporterCompression.CompressedHQ;
        var platform=importer.GetPlatformTextureSettings("Standalone");
        changed|=!platform.overridden || platform.format!=TextureImporterFormat.BC7 || platform.maxTextureSize!=1024;
        if(changed)
        {
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=color;importer.alphaIsTransparency=false;
            importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;
            importer.anisoLevel=8;importer.isReadable=false;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            platform.name="Standalone";platform.overridden=true;platform.format=TextureImporterFormat.BC7;platform.maxTextureSize=1024;
            importer.SetPlatformTextureSettings(platform);importer.SaveAndReimport();
        }
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if(texture==null || texture.width!=1024 || texture.height!=1024)throw new InvalidOperationException("Original surface capacity/import failed: "+path);
        return texture;
    }
}
