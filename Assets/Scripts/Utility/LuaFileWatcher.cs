using System.Collections.Generic;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using XLua;

[CSharpCallLua]
public delegate void ReloadDelegate(string path);

public  class LuaFileWatcher
{
    private static ReloadDelegate ReloadFunction;
    
    private static HashSet<string> _changedFiles = new HashSet<string>();
    public static void CreateLuaFileWatcher()
    {
        var scriptPath = Path.Combine(Application.dataPath, XLuaManager.luaScriptsFolder);
        var directoryWatcher =
            new DirectoryWatcher(scriptPath, new FileSystemEventHandler(LuaFileOnChanged));
        ReloadFunction = XLuaManager.Instance.luaEnv.Global.Get<ReloadDelegate>("hotfix");
        EditorApplication.update -= Reload;
        EditorApplication.update += Reload;
    }

    private static void LuaFileOnChanged(object obj, FileSystemEventArgs args)
    {
        var fullPath = args.FullPath;
        if (fullPath.EndsWith(".lua") == false)
        {
            return;
        }

        var luaFolderName = "LuaScripts";
        var requirePath = fullPath.Replace(".lua", "");

        Debug.Log("lua file changed: " + fullPath);
        
        var luaScriptIndex = requirePath.IndexOf(luaFolderName) + luaFolderName.Length + 1;
        requirePath = requirePath.Substring(luaScriptIndex);
        requirePath = requirePath.Replace('\\','.');
        _changedFiles.Add(requirePath);
    }

    private static void Reload()
    {
        if (EditorApplication.isPlaying == false)
        {
            return;
        }
        if (_changedFiles.Count == 0)
        {
            return;
        }

        foreach (var file in _changedFiles)
        {
            ReloadFunction(file);
            Debug.Log("Reload:" + file);
        }
        _changedFiles.Clear();
    }
}