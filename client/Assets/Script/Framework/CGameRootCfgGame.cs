//#define UNITY_ONLY //移除unity之外的组件，用来测内存占用

using System;
using UnityEngine;


partial class CGameRootCfg
{
    public static CGameRootCfg mGame = new CGameRootCfg(

    #region 各系统初始化顺序定义
delegate (Transform rootObj)
{
    return new CGameSystem[]
            {
                CreateGameSys<CLogSys>(rootObj),
                CreateGameSys<CResourceSys>(rootObj),
                CreateGameSys<CTimeSys>(rootObj),
                CreateGameSys<GamePreLoadingSys>(rootObj),
                CreateGameSys<SceneLoadSys>(rootObj),
                CreateGameSys<TableResMgr>(rootObj),
                CreateGameSys<UINodesManager>(rootObj),
                CreateGameSys<PreLoadSys>(rootObj),
                CreateGameSys<ObjectPoolSys>(rootObj),
                CreateGameSys<ActorManager>(rootObj),
                CreateGameSys<GamingSys>(rootObj),
                CreateGameSys<ChaseCamera>(rootObj),
                CreateGameSys<GameUpdateSys>(rootObj),
                CreateGameSys<InputSys>(rootObj),
                CreateGameSys<FightUISys>(rootObj),
            };
},
    #endregion

    #region 状态定义 & 状态下各系统的定义
 new CGameState(EStateType.Root,
                new Type[]
                {
                    typeof(CLogSys),
                    typeof(ObjectPoolSys),
                    typeof(CResourceSys),
                    typeof(CTimeSys),
                    typeof(TableResMgr),
                    typeof(UINodesManager),
                },
                new CGameState[]
                {
                    new CGameState(EStateType.GameUpdate, new Type[]
                        {
                            typeof(GameUpdateSys),
                        }, null),

                    new CGameState(EStateType.GamePreLoading, new Type[]
                        {
                            typeof(GamePreLoadingSys),
                        }, null),

                    new CGameState(EStateType.GameLoading, new Type[]
                        {
                            typeof(SceneLoadSys),
                            typeof(PreLoadSys),
                        }, null),

                    new CGameState(EStateType.Gaming, new Type[]
                        {
                            typeof(ActorManager),
                            typeof(GamingSys),
                            typeof(ChaseCamera),
                            typeof(FightUISys),
                            typeof(InputSys),
                        }, null),
               })
    #endregion
        );
}

