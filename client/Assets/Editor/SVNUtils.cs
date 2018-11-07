using System.IO;
using UnityEditor;
using UnityEngine;

public class SVNUtils
{
    #region 成员
    private enum Command
    {
        Log,
        CheckOut,
        Update,
        Commit,
        Add,
        Revert,
        CleanUp,
        Resolve,//解决
        Remove,
        Rename,
        Diff,
        Ignore,//忽略
        Lock,
        UnLock,
    }

    private static readonly string[] drives = { "c:", "d:", "e:", "f:", "g:", "h:" };
    private static readonly string[] svnPath = {
        @"\Program Files (x86)\TortoiseSVN\bin\",
        @"\Program Files\TortoiseSVN\bin\",
        @"\Unity\SVN\bin\",
        @"\TortoiseSVN\bin\",
        @"\svn\bin\"};
    private static string svnProc = @"TortoiseProc.exe";
    private static string svnProcPath = "";
    #endregion

    #region 菜单选项
    [MenuItem("Tools/SVN/更新Unity %&e")]
    public static void UpdateFromSVN()
    {
        ExecuteSvnCommand(Command.Update, GetProjectPath(), 0);
    }

    [MenuItem("Tools/SVN/提交Unity %&r")]
    public static void CommitToSVN()
    {
        ExecuteSvnCommand(Command.Commit, GetProjectPath());
    }

    [MenuItem("Tools/SVN/清理Unity")]
    public static void CleanUpFromSVN()
    {
        ExecuteSvnCommand(Command.CleanUp, GetProjectPath());
    }

    [MenuItem("Tools/SVN/更新工程 %&a")]
    public static void UpdateFromProgram()
    {
        ExecuteSvnCommand(Command.Update, GetProgramPath(), 0);
    }

    [MenuItem("Tools/SVN/提交工程 %&d")]
    public static void CommitToProgram()
    {
        ExecuteSvnCommand(Command.Commit, GetProgramPath());
    }

    [MenuItem("Tools/SVN/清理工程")]
    public static void CleanUpFromProgram()
    {
        ExecuteSvnCommand(Command.CleanUp, GetProgramPath());
    }

    /// <summary>
    /// 执行SVN命令
    /// </summary>
    /// <param name="cmd">命令</param>
    /// <param name="path">操作路径</param>
    /// <param name="closeonend">0:不自动关闭,1:如果没发生错误则自动关闭对话框,
    /// 2:如果没发生错误和冲突则自动关闭对话框,3:如果没有错误、冲突和合并，会自动关闭</param>
    private static void ExecuteSvnCommand(Command cmd, string path, int closeonend = -1)
    {
        if (string.IsNullOrEmpty(svnProcPath))
            svnProcPath = GetSvnProcPath();

        string cmdString = string.Format("/command:{0} /path:\"{1}\"", cmd.ToString().ToLower(), path);
        if (closeonend >= 0 && closeonend <= 3)
            cmdString += string.Format(" /closeonend:{0}", closeonend);
        System.Diagnostics.Process.Start(svnProcPath, cmdString);
    }

    private static string GetProjectPath()
    {
        var dir = new DirectoryInfo(Application.dataPath + "../");
        return dir.Parent.FullName.Replace('/', '\\');
    }

    private static string GetProgramPath()
    {
        var dir = new DirectoryInfo(Application.dataPath + "../../");
        return dir.Parent.FullName.Replace('/', '\\');
    }

    private static string GetSvnProcPath()
    {
        for (int i = 0; i < svnPath.Length; ++i)
        {
            foreach (var item in drives)
            {
                var path = string.Concat(item, svnPath[i], svnProc);
                if (File.Exists(path))
                    return path;
            }
        }

        return EditorUtility.OpenFilePanel("Select TortoiseProc.exe", "c:\\", "exe");
    }

    //[MenuItem("Tools/Apply Prefab Changes %&a")]
    static public void ApplyPrefabChanges()
    {
        var obj = Selection.activeGameObject;
        if (obj != null)
        {
            var prefab_root = PrefabUtility.FindPrefabRoot(obj);
            var prefab_src = PrefabUtility.GetPrefabParent(prefab_root);
            if (prefab_src != null)
            {
                PrefabUtility.ReplacePrefab(prefab_root, prefab_src, ReplacePrefabOptions.ConnectToPrefab);
                Debug.Log("Updating prefab : " + AssetDatabase.GetAssetPath(prefab_src));
            }
            else
            {
                Debug.Log("Selected has no prefab");
            }
        }
        else
        {
            Debug.Log("Nothing selected");
        }
    }
    #endregion

    #region 右键选项
    private static string GetPrefabPath(GameObject go)
    {
        if (go == null)
            return string.Empty;

        Object prefab;
        PrefabType type = PrefabUtility.GetPrefabType(go);
        if (type == PrefabType.PrefabInstance)
            prefab = PrefabUtility.GetPrefabParent(go);
        else if (type == PrefabType.Prefab)
            prefab = go;
        else
            return string.Empty;

        return Application.dataPath + AssetDatabase.GetAssetPath(prefab).Replace("Assets", "");
    }

    private static void ExecuteSelectionSvnCmd(Command cmd, int closeonend = -1)
    {
        string path = GetPrefabPath(Selection.activeGameObject);
        if (!string.IsNullOrEmpty(path))
            ExecuteSvnCommand(cmd, path, closeonend);
    }

    [MenuItem("GameObject/SVN Command/Log", false, 12)]
    [MenuItem("Assets/SVN Command/Log")]
    public static void SvnLogCommand()
    {
        ExecuteSelectionSvnCmd(Command.Log);
    }

    [MenuItem("GameObject/SVN Command/Revert", false, 12)]
    [MenuItem("Assets/SVN Command/Revert")]
    public static void SvnRevertCommand()
    {
        ExecuteSelectionSvnCmd(Command.Revert, 3);
    }

    [MenuItem("GameObject/SVN Command/Update", false, 12)]
    [MenuItem("Assets/SVN Command/Update")]
    public static void SvnUpdateCommand()
    {
        ExecuteSelectionSvnCmd(Command.Update);
    }

    [MenuItem("GameObject/SVN Command/Commit", false, 12)]
    [MenuItem("Assets/SVN Command/Commit")]
    public static void SvnCommitCommand()
    {
        ExecuteSelectionSvnCmd(Command.Commit);
    }
    #endregion
}