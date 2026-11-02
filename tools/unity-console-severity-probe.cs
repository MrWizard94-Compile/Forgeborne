if(Application.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new System.InvalidOperationException("Stopped imported Editor required.");
var assembly=typeof(EditorApplication).Assembly;
var entries=assembly.GetType("UnityEditor.LogEntries");var entry=assembly.GetType("UnityEditor.LogEntry");
var statics=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var instances=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
var result=new System.Collections.Generic.List<object>();
var counts=new object[]{0,0,0};
entries.GetMethod("GetCountsByType",statics).Invoke(null,counts);
int count=(int)entries.GetMethod("StartGettingEntries",statics).Invoke(null,null);
try
{
    var item=System.Activator.CreateInstance(entry);
    for(int i=0;i<count;i++)
    {
        entries.GetMethod("GetEntryInternal",statics).Invoke(null,new object[]{i,item});
        string message=(string)entry.GetField("message",instances).GetValue(item);
        if(message.IndexOf("URP assets included in build",System.StringComparison.Ordinal)>=0)
            result.Add(new{index=i,mode=(int)entry.GetField("mode",instances).GetValue(item),message=message,
                file=(string)entry.GetField("file",instances).GetValue(item),line=(int)entry.GetField("line",instances).GetValue(item)});
    }
}
finally{entries.GetMethod("EndGettingEntries",statics).Invoke(null,null);}
return new{unityVersion=Application.unityVersion,countsByType=new{errors=counts[0],warnings=counts[1],logs=counts[2]},matchingEntries=result,
    note="Read-only actual Unity counts and raw entry modes; no log suppression or installed-package changes."};
