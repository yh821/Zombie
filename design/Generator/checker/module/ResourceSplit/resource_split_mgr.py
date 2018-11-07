# -*- coding: utf-8 -*-
import os


class ResourceSplitMgr(object):
    def __init__(self, currentPath):
        path = currentPath + "/notcommit"
        os.path.join(path)
        sourceConfigDir = currentPath + "/server"
        self._sourceDataDir = currentPath + "/data"
        targetConfigDir = path + "/server"
        targetDataDir = targetConfigDir + "/data"

        # print path, sourceConfigDir, sourceDataDir, targetConfigDir, targetDataDir
        if not os.path.exists(path):
            os.mkdir(path)
            file = open(path + "/__init__.py", 'w')
            file.write("# -*- coding: utf-8 -*-")
            file.close()
            os.mkdir(targetConfigDir)
            os.mkdir(targetDataDir)
        self._copyFile(sourceConfigDir, targetConfigDir, ".py")
        self._copyFile(self._sourceDataDir, targetDataDir, ".data")

        self._serverConfigDir = currentPath + "/../../server/config/game/data"
        ResourceSplitMgr._copyFile(self._serverConfigDir, targetDataDir, ".animdata")
        
    def GetServerDataDir(self):
        return self._serverConfigDir

    def GetSourceDataDir(self):
        return self._sourceDataDir

    # region 内部方法
    @staticmethod
    def _copyFile(sourceDir, targetDir, suffix):
        files = os.listdir(sourceDir)
        for name in files:
            if name.endswith(suffix):
                sourceFile = os.path.join(sourceDir,  name)
                targetFile = os.path.join(targetDir,  name)
                open(targetFile, "wb").write(open(sourceFile, "rb").read())

    def _LoopMonsterGetFxAndAudio(self, monsterDict, checkModelDict, checkFxDict, checkAudioDict, errList):
        # 遍历怪物信息获取model,fx,audio
        from notcommit.server import *
        from checker.module.ResourceSplit.anim_cfg_check_mgr import ModelAnimCfgCheckMgr
        effectSummonMonster = dict()
        checkEffectDict = dict()   # 效应，包号
        for monsterId, listLv in monsterDict.items():
            monsterCfg = MonsterCfg().GetOneById(monsterId)
            if not monsterCfg:
                errList.append("[ERROR!]monster config lost, id:{}".format(monsterId))
                continue

            # 模型，包号
            self._UpdateListLv(checkModelDict, monsterCfg.model, listLv)
            # checkModelDict[monsterCfg.model] = listLv

            # 从技能中获取效应
            for skillId in monsterCfg.skill:
                skillCfg = SkillCfg().GetOneById(skillId)
                if not skillCfg:
                    errList.append("[ERROR!]skill config lost, id:{}, from:{}".format(skillId, monsterId))
                    continue

                # 技能里的效应
                for effectId in skillCfg.effect:
                    # checkEffectDict[effectId] = listLv
                    self._UpdateListLv(checkModelDict, effectId, listLv)

                # 根据模型id和技能动作获取效应
                actEffectList = ModelAnimCfgCheckMgr().GetModelActionEffects(monsterCfg.model, skillCfg.attAction)
                for effect in actEffectList:
                    # checkEffectDict[effect] = listLv
                    self._UpdateListLv(checkEffectDict, effect, listLv)

                fxList = ModelAnimCfgCheckMgr().GetModelActionFx(monsterCfg.model, skillCfg.attAction)
                for fx in fxList:
                    # checkFxDict[fx] = listLv
                    self._UpdateListLv(checkFxDict, fx, listLv)

                audioList = ModelAnimCfgCheckMgr().GetModelActionAudio(monsterCfg.model, skillCfg.attAction)
                for audio in audioList:
                    # checkAudioDict[audio] = listLv
                    self._UpdateListLv(checkAudioDict, audio, listLv)

        # 从效应中获取fx、audio、monster
        for effectId, listLv in checkEffectDict.items():
            effectCfg = EffectCfg().GetOneById(effectId)
            if not effectCfg:
                errList.append("[ERROR!]effect config lost, id:{}, from:{}".format(effectId, monsterId))
                continue
            if effectCfg.fx > 0:
                # checkFxDict[effectCfg.fx] = listLv
                self._UpdateListLv(checkFxDict, effectCfg.fx, listLv)

            if effectCfg.audio > 0:
                # checkAudioDict[effectCfg.audio] = listLv
                self._UpdateListLv(checkAudioDict, effectCfg.audio, listLv)

            if effectCfg.addBuff is not None:
                for buf in effectCfg.addBuff:
                    bufCfg = BuffCfg().GetOneById(buf)
                    if not buf:
                        assert False
                        continue
                    for fx in bufCfg.fx:
                        # checkFxDict[fx] = listLv
                        self._UpdateListLv(checkFxDict, fx, listLv)

            if effectCfg.monsterId and not monsterDict.get(effectCfg.monsterId):
                effectSummonMonster[effectCfg.monsterId] = listLv

        return effectSummonMonster

    @staticmethod
    def _UpdateListLv(dict, key, listLv):
        oldListLv = dict.get(key)
        if not oldListLv or oldListLv[0] > oldListLv[0]:
            newListLv = list(listLv)
            newListLv.append(key)
            dict[key] = newListLv

    # endregion

    def VerifyResourceSplit(self):
        from notcommit.server import *

        # 初始化动作模型检查配置
        from checker.module.ResourceSplit.anim_cfg_check_mgr import ModelAnimCfgCheckMgr
        ModelAnimCfgCheckMgr().Load(self._serverConfigDir)

        # 获取资源分包配置信息（人物等级对应的包号）
        lv2packetNumberDict = dict()
        for rule in ResourceSplitCfg().items:
            lv2packetNumberDict[rule.LvLimit] = rule.Packlist
        keys = lv2packetNumberDict.keys()
        keys.sort()

        # 获取副本配置对应的包号
        copyId2packetNumber = dict()
        for copy in ZhuXianPeiZhiCfg().items:
            for lv in keys:
                if lv >= copy.minLv:
                    listLv = list()
                    listLv.append(lv2packetNumberDict[lv])
                    listLv.append(copy.id)
                    copyId2packetNumber[copy.id] = listLv
                    break

        # 从副本基础配置表出发，检查每个副本中涉及到的所有资源包号是否合法
        checkMonsterDict = dict()  # 怪物id，包号
        checkSceneDict = dict()   # 场景，包号
        checkModelDict = dict()  # 模型，包号
        checkFxDict = dict()  # 特效，包号
        checkAudioDict = dict()  # 音效，包号

        # 场景
        for copyId, listLv in copyId2packetNumber.items():
            copyCfg = FuBenJiChuPeiZhiCfg().GetOneById(copyId)
            if not copyCfg:
                # assert False
                continue

            # 场景，包号
            # 多个副本依赖同一个场景时，取资源包号小的那个做标准，其他资源同理
            self._UpdateListLv(checkSceneDict, copyCfg.scene, listLv)

            # 怪物id获取
            monsterRefreshCfgs = MonsterRefreshCfg().GetById(copyId)
            for monsterRefreshCfg in monsterRefreshCfgs:
                if monsterRefreshCfg.refreshType == 1:
                    # checkMonsterDict[monsterRefreshCfg.monster] = listLv
                    self._UpdateListLv(checkMonsterDict, monsterRefreshCfg.monster, listLv)
                elif rawCfg.refreshType == 2:  # 怪物库
                    for gwkCfg in GuaiWuKuCfg().items:
                        if gwkCfg.monsterType == rawCfg.monster:
                            # checkMonsterDict[gwkCfg.monster] = listLv
                            self._UpdateListLv(checkMonsterDict, gwkCfg.monster, listLv)
                elif monsterRefreshCfg.refreshType == 3:  # 乱入库
                    for lrkCfg in LuanRuKuCfg().items:
                        if lrkCfg.monsterLibrary == monsterRefreshCfg.monster:
                            # checkMonsterDict[lrkCfg.monster] = listLv
                            self._UpdateListLv(checkMonsterDict, lrkCfg.monsterk, listLv)
                elif rawCfg.refreshType == 4:  # 随机怪物库
                    for sjkCfg in SuiJiGuaiWuKuCfg.items:
                        if sjkCfg.librariesId == rawCfg.monster:
                            # checkMonsterDict[sjkCfg.monsterId] = listLv
                            self._UpdateListLv(checkMonsterDict, sjkCfg.monsterId, listLv)

            # 剧情事件id获取
            eventCfg = DongHuaShiJianBiaoCfg().GetOneByInstanceId(copyId)
            if not eventCfg:
                continue
            eventSet = set()
            for event in eventCfg.beginEventId:
                eventSet.add(event)
            for event in eventCfg.endEventId:
                eventSet.add(event)
            for event in eventSet:
                eventParamCfg = ShiJianCanShuBiaoCfg().GetOneByParameterId(event)
                if not eventParamCfg:
                    continue
                if eventParamCfg.modelId > 1:
                    # checkModelDict[eventParamCfg.modelId] = listLv
                    self._UpdateListLv(checkModelDict, eventParamCfg.modelId, listLv)
                if eventParamCfg.fx:
                    # checkFxDict[eventParamCfg.fx] = listLv
                    self._UpdateListLv(checkFxDict, eventParamCfg.fx, listLv)
                if eventParamCfg.voice:
                    # checkAudioDict[eventParamCfg.voice] = listLv
                    self._UpdateListLv(checkAudioDict, eventParamCfg.voice, listLv)
                if eventParamCfg.anime:
                    # checkFxDict[eventParamCfg.anime] = listLv
                    self._UpdateListLv(checkFxDict, eventParamCfg.anime, listLv)
                if eventParamCfg.anime2:
                    import re
                    evList = re.split(',|;', eventParamCfg.anime2)
                    for evStr in evList:
                        # checkFxDict[int(evStr)] = listLv
                        self._UpdateListLv(checkFxDict, int(evStr), listLv)

        # 错误列表，打印用
        errorList = []

        # 循环调用，直到效应里没有出现召唤新怪物
        exMonsterDict = checkMonsterDict
        while True:
            exMonsterDict = self._LoopMonsterGetFxAndAudio(monsterDict=exMonsterDict,
                                                           checkModelDict=checkMonsterDict,
                                                           checkFxDict=checkFxDict,
                                                           checkAudioDict=checkAudioDict,
                                                           errList=errorList)
            if len(exMonsterDict) == 0:
                break

        # 1、场景资源的的引用和检索
        for scene, listLv in checkSceneDict.items():
            sceneCfg = SceneCfg().GetOneById(scene)
            if not sceneCfg:
                errorList.append("[ERROR!]scene config error,scene:{} config lost, legalLv:{}".format(scene, listLv))
                continue
            if sceneCfg.listlv > listLv[0]:
                errorList.append("[ERROR!]packet number error,scene:{}, error num:{}, correct info:{}"
                                 .format(scene, sceneCfg.listlv, listLv))
            # 场景特效
            if sceneCfg.sceneFX:
                self._UpdateListLv(checkFxDict, sceneCfg.sceneFX, listLv)

            # 场景音效
            if sceneCfg.music:
                self._UpdateListLv(checkAudioDict, sceneCfg.music, listLv)

        # 2、角色模型资源的引用和检索
        for model, listLv in checkModelDict.items():
            modelCfg = AvatarModelCfg().GetOneById(model)
            if not modelCfg:
                errorList.append("[ERROR!]model config error,model:{} config lost, legalLv:{}".format(model, listLv))
                continue
            if modelCfg.listlv > listLv[0]:
                errorList.append("[ERROR!]packet number error,model:{}, error num:{}, correct info:{}"
                                 .format(model, modelCfg.listlv, listLv))

        # 3、特效资源的的引用和检索
        for fx, listLv in checkFxDict.items():
            fxCfg = FxCfg().GetOneById(fx)
            if not fxCfg:
                errorList.append("[ERROR!]fx config error,fx:{} config lost, legalLv:{}".format(fx, listLv))
                continue
            if fxCfg.listlv > listLv[0]:
                errorList.append("[ERROR!]packet number error,fx:{}, error num:{}, correct info:{}"
                                 .format(fx, fxCfg.listlv, listLv))

        # 4、音乐音效资源的的引用和检索
        for audio, listLv in checkAudioDict.items():
            audioCfg = AudioCfg().GetOneById(audio)
            if not audioCfg:
                errorList.append("[ERROR!]audio config error,audio:{} config lost, legalLv:{}".format(audio, listLv))
                continue
            if audioCfg.listlv > listLv[0]:
                errorList.append("[ERROR!]packet number error,audio:{}, err num:{}, correct info:{}"
                                 .format(audio, audioCfg.listlv, listLv))

        # print
        for errStr in errorList:
            print errStr
