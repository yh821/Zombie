using UnityEngine;
using System.Collections;

[System.Serializable]

public class GameConfig {
    public int MajorVersion;
    public int MinorVersion;

    private static string cConfigPath = "Config/GameConfig";
	public static GameConfig LoadGameConfig()
    {
        GameConfig gameCfg = (GameConfig)CUility.DeSerializerObjectFromAssert(cConfigPath, typeof(GameConfig));

        string gameCfgPath = Application.persistentDataPath + "/" + cConfigPath;
        if (System.IO.File.Exists(gameCfgPath))
        {
            gameCfg = (GameConfig)CUility.DeSerializerObject(gameCfgPath, typeof(GameConfig));
        }
        return gameCfg;
    }
}
 