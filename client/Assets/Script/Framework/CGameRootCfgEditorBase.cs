using UnityEngine;
using System.Collections;
using System;


partial class CGameRootCfg
{
    public static CGameRootCfg mEditorBase = new CGameRootCfg(
            delegate (Transform rootObj)
            {
                return new CGameSystem[]
                {
                    CreateGameSys<CResourceSys>(rootObj),
                    CreateGameSys<GamePreLoadingSys>(rootObj),
                    CreateGameSys<SceneLoadSys>(rootObj),
                    CreateGameSys<TableResMgr>(rootObj),
                };
            },
            new CGameState(EStateType.Root,
                new Type[]
                {
                    typeof(CResourceSys),
                    typeof(TableResMgr),
                },
                new CGameState[] 
                {
                    
                }
            )
        );
}