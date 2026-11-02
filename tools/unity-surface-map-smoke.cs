if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required for isolated GPU map readback.");
string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../art/source/FoundrySurfaces.json"));
var document=System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(path));
var target=new RenderTexture(1024,1024,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
var pixels=new Texture2D(1024,1024,TextureFormat.RGBAFloat,false,true);
var active=RenderTexture.active;bool srgb=GL.sRGBWrite;int samples=0;float maximumError=0;long textureBytes=0;
try
{
    foreach(var role in document.RootElement.GetProperty("materials").EnumerateObject())
    {
        var material=Resources.Load<Material>("Art/Materials/"+role.Name);
        if(material==null || material.GetFloat("_TextureStrength")!=1)throw new System.InvalidOperationException("Material surface program is inactive: "+role.Name);
        foreach(bool color in new[]{true,false})
        {
            var texture=(Texture2D)material.GetTexture(color ? "_BaseMap" : "_SurfaceMap");
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
            if(texture==null || importer.sRGBTexture!=color || importer.alphaIsTransparency || texture.isReadable
                || importer.wrapMode!=TextureWrapMode.Repeat || !importer.mipmapEnabled || importer.anisoLevel!=8
                || importer.filterMode!=FilterMode.Trilinear || importer.textureCompression!=TextureImporterCompression.CompressedHQ
                || texture.width!=1024 || texture.height!=1024 || texture.format!=TextureFormat.BC7)
                throw new System.InvalidOperationException("Incorrect actual color/data/alpha/mip import: "+role.Name);
            var platform=importer.GetPlatformTextureSettings("Standalone");
            if(!platform.overridden || platform.maxTextureSize!=1024 || platform.format!=TextureImporterFormat.BC7)
                throw new System.InvalidOperationException("Incorrect declared Standalone surface capacity/compression: "+role.Name);
            textureBytes+=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            GL.sRGBWrite=false;Graphics.Blit(texture,target);RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1024,1024),0,0);pixels.Apply();
            foreach(var probe in role.Value.GetProperty("gpuProbeSamples").EnumerateArray())
            {
                var coordinate=probe.GetProperty("pixel");int x=coordinate[0].GetInt32(),y=coordinate[1].GetInt32();
                var expected=probe.GetProperty(color ? "colorLinear" : "surface");var actual=pixels.GetPixel(x,y);
                for(int channel=0;channel<(color ? 3 : 4);channel++)
                {
                    float error=Mathf.Abs(actual[channel]-expected[channel].GetSingle());maximumError=Mathf.Max(maximumError,error);
                    if(error>.035f)throw new System.InvalidOperationException("GPU map value/color-space/alpha mismatch: "+role.Name+" "+(color ? "color" : "data")+" channel "+channel+" at "+x+","+y+" error "+error);
                }
                samples++;
            }
        }
    }
    return new{actualGpuSamples=samples,maximumLinearChannelError=maximumError,importedTextureRuntimeBytes=textureBytes,
        pairedColorAndDataConsumers=true,packedMetalAlphaPreservesRgb=true,mipsAndRepeat=true,cpuCopiesDisabled=true,actualBc7AndTrilinear=true,
        scope="Compressed texture value/import proof; not a rendered-game performance or AAA+ acceptance claim."};
}
finally
{
    document.Dispose();GL.sRGBWrite=srgb;RenderTexture.active=active;target.Release();
    UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);
}
