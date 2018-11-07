# -*- coding: utf-8 -*-


import os
from shutil import copy
import json
import re
#from notcommit.server import *


class FilterRule:
    # 配置过滤规则
    def __init__(self, filterRule):
        # self._code = code
        self._filterDict = {}
        for k, v in filterRule.items():
            if not isinstance(v, list):
                assert False
            fList = []
            self._filterDict[k] = fList
            for fStr in v:
                params = fStr.split(' ')
                fList.append(params)

    def IsFilter(self, rawCfg, attrName, attrVal):
        fList = self._filterDict.get(attrName)
        if not fList:
            return False
        for fDetail in fList:
            ruleStr = None
            if len(fDetail) == 1:
                ruleStr = str(attrVal) + fDetail[0]
            else:
                val = getattr(rawCfg, fDetail[0])
                ruleStr = str(val) + fDetail[1]
            if eval(ruleStr):
                # print "[IsFilter] attrName:", attrName, ", attrVal", attrVal, "rule:", ruleStr
                return True

        return False


class VerifyData:
    def __init__(self, currentPath):
        path = currentPath + "/notcommit"
        os.path.join(path)
        sourceConfigDir = currentPath + "/server"
        sourceDataDir = currentPath + "/data"
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
        self._copyFile(sourceDataDir, targetDataDir, ".data")
        self._parseVerifyConfigRule(currentPath)

        self._allData = {}

        # 收集错误,统一打印,一条记录就是一个错误提示的字符串
        self._collectError = list()

    def _init(self):
        self._allSource.append()

    # region 加载配置检测规则文件
    def _loadConfigSource(self):
        from notcommit.server import *
        for id, codes in self._source.iteritems():
            for code in codes:
                sourceName = self._configCode.get(code)
                sourceObjectName = sourceName + "()"
                for rawCfg in eval(sourceObjectName).items:
                    attrName = getattr(rawCfg, id)
                    self._fillSourceIds(sourceName, attrName)

    def _loadConfigTarget(self):
        from notcommit.server import *
        for code, target in self._target.iteritems():
            sourceName = self._configCode.get(code)
            filterRule = self._filter.get(code)
            sourceObjectName = sourceName + "()"
            for rawCfg in eval(sourceObjectName).items:
                for k, v in target.items():
                    targetName = self._configCode.get(k)
                    for i in range(0, len(v)):
                        sourceAttr = getattr(rawCfg, v[i])
                        listAttr = []
                        if isinstance(sourceAttr, int):
                            listAttr.append(sourceAttr)
                        else:
                            listAttr = sourceAttr
                        for attr in listAttr:
                            if hasattr(attr, "value"):
                                attr = attr.value
                            if filterRule and filterRule.IsFilter(rawCfg, v[i], attr):
                                continue
                            self._fillTargetIds(targetName, sourceName, attr)

    def _loadSpecialConfigTarget(self):
        from notcommit.server import *
        for rawCfg in PetCfg().items:
            for item in rawCfg.skill:
                skills = [int(strSkillId) for strSkillId in item.value.split(',')]
                for skillId in skills:
                    self._fillTargetIds("SkillCfg", "Pet", skillId)

        cfg = JiChuSheZhiCfg()
        self._fillTargetIds("ItemsCfg", "JiChuSheZhi", cfg.GetByKey("probabilityItem")[0].num)

        # TODO 某些特殊加载规则在这里处理

    def loadData(self):
        from notcommit.server import *
        print "loadData begin"

        self._loadConfigSource()
        print "loadConfigSource end"

        self._loadConfigTarget()
        print "loadConfigTarget end"

        self._loadSpecialConfigTarget()
        print "loadSpecialConfigTarget end"

        print "loadData end"

    def _copyFile(self, sourceDir, targetDir, suffix):
        files = os.listdir(sourceDir)
        for name in files:
            if name.endswith(suffix):
                # print name
                sourceFile = os.path.join(sourceDir,  name)
                targetFile = os.path.join(targetDir,  name)
                open(targetFile, "wb").write(open(sourceFile, "rb").read())

    def _parseVerifyConfigRule(self, path):
        jsonPath = path + "/checker/vertifyConfigRule.json"
        f = file(jsonPath)
        if not f:
            f.close()
            return
        jsonConfig = json.load(f)
        self._configCode = jsonConfig.get("ConfigCode")
        self._source = jsonConfig.get("Source")
        self._target = jsonConfig.get("Target")
        self._filter = {}
        filterJson = jsonConfig.get("Filter")
        for k, v in filterJson.items():
            self._filter[k] = FilterRule(v)
        if not self._configCode \
                or not self._source or not self._target\
                or not self._filter:
            assert False
        self._mapping = jsonConfig.get("Mapping")
        self._sole = jsonConfig.get("Sole")
        self._repeated = jsonConfig.get("Repeated")
        self._Array = jsonConfig.get("Array")
        f.close()

    # endregion

    # region 打印信息
    def _printAllData(self):
        for key, value in self._allData.items():
            print key
            if key == "BuffCfg":
                for i, j in value[1].items():
                    for k in j:
                        print "MainKey =", key, "ViceKey =", i, "Value =", k

    def _print(self, map):
        for key, value in map.items():
            for id in value:
                print "tableName =", key, " ------>",id

    # endregion

    # region 检测
    def verify(self):
        print "vertify begin:"
        flag = False
        zeroSet = set()
        zeroSet.add(0)
        vertifyMessage = {}
        for key, value in self._allData.items():
            temp = []
            vertifyMessage[key] = temp
            for viceKey, viceValue in value[1].items():
                result = {}
                ids = viceValue - value[0]
                # if ids:
                #     flag = True
                result[viceKey] = ids
                temp.append(result)
        for key, value in vertifyMessage.items():
            for viceValue in value:
                for childKey, childValue in viceValue.items():
                    if childValue:
                        if len(childValue) == 1 and zeroSet.issubset(childValue):
                            continue
                        flag = True
                        print "SourceChartName =", key, "\nReferChartName =", childKey, "\nLoseID =", childValue , "\n"

        f2 = self._Verify()  # 特殊规则检测
        flag = f2 if not flag else flag
        if not flag:
            print "success to pass vertify!!!"
        return flag

    # 公会题库校验
    def SensitiveWords(self):
        import sys
        from notcommit.server import *
        from os import path as op
        reload(sys)
        sys.setdefaultencoding('utf-8')
        # file = open('../z中文字符/Word.txt'.decode('utf-8'), 'r')
        FS_CODING = sys.getfilesystemencoding()
        CUR_PATH = op.dirname(op.abspath(__file__)).decode(FS_CODING)
        CUR_PATH = CUR_PATH.encode('utf-8')
        # file = open('../z中文字符/Word.txt'.decode('utf-8'), 'r')
        pathStr = CUR_PATH.split('\\')
        path = ''
        for i in range(0, len(pathStr)-2 ):
            if i != 0:
                path += '/'
            path += pathStr[i]
        path +='/z中文字符/Word1.txt'
        file = open(path.decode('utf-8'), 'r')
        strc = file.read()
        Words = strc.split('|')
        Len = len(Words)
        for i in range(0, Len):
            StrB = Words[i]
            if StrB == '\n' or StrB == ' ' or len(StrB) <= 0:
                continue
            if StrB[0] == '\n' or StrB[0] == ' ' or StrB[0] == '"':
                StrB = StrB[1:]
            if StrB[len(StrB) - 1] == '\n' or StrB[len(StrB) - 1] == ' ' or StrB[len(StrB) - 1] == '"':
                StrB = StrB[:-1]
            Words[i] = StrB
        file.close()
        Wrong = list()
        answerWrong = list()
        # print '公会题库校验开始(无输出则无错误):'.decode("utf-8")
        for rawCfg in GongHuiTiKuCfg().items:
            for i in range(0, Len):
                StrAList = rawCfg.answer
                StrB = Words[i]
                if StrB == '\n' or StrB == ' ' or len(StrB) <= 0:
                    continue
                for StrA in StrAList:
                    StrA = StrA.encode("utf-8")
                    if rawCfg.id not in answerWrong and not self.vertyfyAnswer(StrA):
                        answerWrong.append(rawCfg.id)
                    if rawCfg.id not in Wrong and self.vertifyWord(StrA.decode('utf-8'), StrB.decode('utf-8')):
                        print StrA.decode('utf-8')
                        print '#'+StrB.decode('utf-8')+'#'
                        Wrong.append(rawCfg.id)
        if len(Wrong) > 0:
            print '公会题库出现敏感词:'.decode("utf-8")
            print Wrong
        if len(answerWrong) > 0:
            print '题库答案配置非法(存在空格)'.decode("utf-8")
            print answerWrong

    # 公会题目答案验证
    def vertyfyAnswer(self, strAnswer):
        if len(strAnswer) < 1:
            return False
        if strAnswer[0] == ' ':
            return False
        if strAnswer[len(strAnswer) - 1] == ' ':
            return False
        return True

    # 公会题目是否有敏感词
    def vertifyWord(self, StrA, StrB):
        # if StrA.find(StrB) >= 0:
        #     return True
        # else:
        #     return False
        lenA = len(StrA)
        lenB = len(StrB)
        idxStart = 0
        idx = 0
        isMatch = True
        while idx < lenB:
            if idxStart >= lenA:
                isMatch = False
                break
            Str = StrA[idxStart:lenA]
            t = Str.find(StrB[idx])
            if t < 0:
                isMatch = False
                break
            idxStart = t + 1
            idx += 1
        return isMatch

    def _Verify(self):
        # TODO 某些特殊检测规则在这里添加

        # ===== 验证挑战副本数据
        self._VerifyFuBenJiChuPeiZhiCfg()

        # ===== 验证怪物属性表数据
        self._VerifyMonsterAttrCfg()

        # ===== 验证任务数据
        self._VerifyTask()

        # ===== 验证mapping配置
        self._VerifyMapping()

        # ===== 验证sole配置
        self._VerifySole()

        # ===== 验证repeated配置
        self._VerifyRepeated()

        # ===== 验证跑环任务数据
        self._VerifyPaoHuanRenWuXiangGuanCfg()

        # ===== 验证成就事件
        self._VerifyAchievement()

        # ===== 验证挑战波次
        self._VerifyTiaoZhanBoCi()

        # ==== 验证宝石相关
        # self._VerifyBaoShi()

        # ==== 验证公会答题题库
        self.SensitiveWords()

        # ===== 验证怪物库
        self._VerifyMonsterRandomLib()

        # 卓越属性 libraryId 必须能找到
        self._VerifyEquipment()

        #
        self.VerifyArray()

        # buff检测
        self._VerifyBuffCfg()

        flag = False
        for errorStr in self._collectError:
            flag = True
            print(errorStr.decode('utf-8'))

        return flag

    # 卓越属性 libraryId 必须能找到
    # z装备系统/A装备洗炼配置表.xlsx	ZhuoYueShuXingCfgItem	libraryId
    # z装备系统/A装备配置表.xls	zZhuangBeiPeiZhiBiaoCfgItem	attrLibrary
    def _VerifyEquipment(self):
        # from config import zZhuangBeiPeiZhiBiaoCfg, zZhuangBeiPeiZhiBiaoCfgItem, ZhuoYueShuXingCfg
        from notcommit.server import *
        for item in zZhuangBeiPeiZhiBiaoCfg().items:
            assert isinstance(item, zZhuangBeiPeiZhiBiaoCfgItem)
            if not item.attrLibrary:
                continue
            attrLibrary = ZhuoYueShuXingCfg().GetByLibraryId(item.attrLibrary)
            if not attrLibrary:
                errorStr = '[zZhuangBeiPeiZhiBiaoCfg]can not find attr library, equip id:{}, attr library:{}'.format(item.id, item.attrLibrary)
                self._collectError.append(errorStr)

    def _VerifyTask(self):
        from notcommit.server import *
        for rawCfg in RenWuCfg().items:
            if rawCfg.id == 0:
                print '任务配置表存在不合法的配置,配置ID为0或者存在空白行配置!!!!!!!!!!!!!'.decode('utf-8')

        rawCfgs = RenWuPeiZhiCfg().items
        for rawCfg in rawCfgs:
            if rawCfg.type == 4:
                if len(rawCfg.target) != 3:
                    print 'RenWuPeiZhiCfg taskId:{} type:4 target must 3 elements'.format(rawCfg.id)
            elif rawCfg.type == 3:
                if len(rawCfg.target) != 2:
                    print 'RenWuPeiZhiCfg taskId:{} type:3 target must 2 elements'.format(rawCfg.id)
            elif rawCfg.type == 2:
                if len(rawCfg.target) < 1:
                    print 'RenWuPeiZhiCfg taskId:{} type:2 target must 1 elements'.format(rawCfg.id)
            # elif rawCfg.type == 1:
            #     if len(rawCfg.target.split(';')) != 3:
            #         print 'RenWuPeiZhiCfg taskId:{} type:1 target must 3 elements'.format(rawCfg.id)

    def _VerifyFuBenJiChuPeiZhiCfg(self):
        from notcommit.server import *
        for rawCfg in FuBenJiChuPeiZhiCfg().items:
            if rawCfg.type == 106 and rawCfg.sceneType == 2:  # 106是副本类型  2是表示场景是随机的
                monsterRefreshCfgs = MonsterRefreshCfg().GetById(rawCfg.id)
                if not monsterRefreshCfgs:
                    errorStr = '[挑战副本] 从 MonsterRefresh [A副本刷怪机制表.xls] 中找不到 id:{}'.format(rawCfg.id)
                    self._collectError.append(errorStr)
                    continue

                bBoss = False
                bMonster = False
                allMonsterTypeInMonsterRefreshByDungeonId = set()
                for monsterRefreshRaw in monsterRefreshCfgs:
                    allMonsterTypeInMonsterRefreshByDungeonId.add(monsterRefreshRaw.monster)
                    if monsterRefreshRaw.target == 1:
                        bMonster = True
                    elif monsterRefreshRaw.target == 2:
                        bBoss = True

                if bBoss is False:
                    errorStr = '[挑战副本] 从 id 为{}的 MonsterRefresh [A副本刷怪机制表.xls] 中找不到 target 字段为 2 的记录'\
                        .format(rawCfg.id)
                    self._collectError.append(errorStr)
                    continue

                if bMonster is False:
                    errorStr = '[挑战副本] 从 id 为{}的 MonsterRefresh [A副本刷怪机制表.xls] 中找不到 target 字段为 1 的记录'\
                        .format(rawCfg.id)
                    self._collectError.append(errorStr)
                    continue

                sceneIds = set()
                for sceneLibRaw in ChangJingSuiJiKuCfg().items:
                    if rawCfg.scene == sceneLibRaw.sceneLibrary:
                        sceneIds.add(sceneLibRaw.scene)

                if not sceneIds:
                    errorStr = '[挑战副本] 从 场景随机库 [A修改版副本配置表.xls] 中找不到 sceneLibrary:{}'.format(rawCfg.scene)
                    self._collectError.append(errorStr)

                for sceneId in sceneIds:
                    allMonsterTypeInGuaiWuKuBySceneId = set()
                    for guaiWuKuRaw in GuaiWuKuCfg().items:
                        if guaiWuKuRaw.monsterLibrary == sceneId:
                            allMonsterTypeInGuaiWuKuBySceneId.add(guaiWuKuRaw.monsterType)

                    if not allMonsterTypeInGuaiWuKuBySceneId:
                        errorStr = '[挑战副本] 怪物库 [A副本刷怪机制表.xls] 中没有 monsterLibrary:{}'.format(sceneId)
                        self._collectError.append(errorStr)
                        continue

                    notFoundTypes = set()
                    for monsterType in allMonsterTypeInMonsterRefreshByDungeonId:
                        if monsterType not in allMonsterTypeInGuaiWuKuBySceneId:
                            notFoundTypes.add(monsterType)

                    if notFoundTypes:
                        errorStr = '[挑战副本] 从 MonsterRefresh [A副本刷怪机制表.xls] 中存在的怪物类型:{} 需要同时存在于' \
                                   ' 怪物库[A副本刷怪机制表.xls] '.format(notFoundTypes)
                        self._collectError.append(errorStr)

    def _VerifyMonsterAttrCfg(self):
        # 检查BossRefresh表
        from notcommit.server import *
        specMonsterSet = set()
        for rawCfg in MonsterRefreshCfg().items:
            if rawCfg.refreshType == 1:  # 怪物id
                continue
                monsterCfg = MonsterCfg().GetOneById(rawCfg.monster)
                if not monsterCfg:
                    errorStr = '[怪物属性配置]在MonsterRefresh表{}中找不到id[{}]的怪物'\
                        .format(rawCfg.id, rawCfg.monster)
                    self._collectError.append(errorStr)
                    return
                monsterAttrCfgs = MonsterAttrCfg().GetById(monsterCfg.AttributeTemplateID)
                if len(monsterAttrCfgs) == 0:
                    errorStr = '[怪物属性配置]在MonsterAttrCfg表中找不到id[{}]的属性信息'\
                        .format(monsterCfg.AttributeTemplateID)
                    self._collectError.append(errorStr)
                    return
                findLv = False
                for attrCfg in monsterAttrCfgs:
                    if attrCfg.lv == rawCfg.monsterLv:
                        findLv = True
                        break
                if findLv == False:
                    errorStr = '[怪物属性配置]MonsterAttrCfg表id[{}]找不到等级为[{}]的属性，' \
                               '该怪在MonsterRefresh表id[{}]会用到，怪物id为[{}]'\
                        .format(monsterCfg.AttributeTemplateID, rawCfg.monsterLv, rawCfg.id, monsterCfg.id)
                    self._collectError.append(errorStr)
                    return
            elif rawCfg.refreshType == 2:  # 随机库
                monsterRandomCfg = MonsterRandomCfg().GetOneById(rawCfg.monster)
                if not monsterRandomCfg:
                    errorStr = '[怪物属性配置]在MonsterRandomCfg表中找不到id[{}]怪物随机库配置'\
                        .format(rawCfg.monster)
                    self._collectError.append(errorStr)
                    return
                # monsterLibList = monsterRandomCfg.librariesId
                # for libId in monsterLibList:
                #     monsterLibCfg = MonsterLibraryCfg().GetOneById(libId)
                #     if not monsterLibCfg:
                #         errorStr = '[怪物属性配置]在MonsterLibraryCfg表中找不到id[{}]的配置'\
                #             .format(monsterRandomCfg.librariesId)
                #         self._collectError.append(errorStr)
                #         return
                #     specMonsterSet.add(monsterLibCfg.monsterID)
            elif rawCfg.refreshType == 3:  # 乱入库（废弃）
                for lrkCfg in LuanRuKuCfg().items:
                    if lrkCfg.monsterLibrary == rawCfg.monster:
                        specMonsterSet.add(lrkCfg.monster)
            elif rawCfg.refreshType == 4:  # 随机怪物库（废弃）
                for sjkCfg in SuiJiGuaiWuKuCfg.items:
                    if sjkCfg.librariesId == rawCfg.monster:
                        specMonsterSet.add(sjkCfg.monsterId)

        # 检查召唤类的怪物
        for effectCfg in EffectCfg().items:
            if effectCfg.monsterId <= 0:
                continue
            specMonsterSet.add(effectCfg.monsterId)

        for monsterId in specMonsterSet:
            rawCfg = MonsterCfg().GetOneById(monsterId)
            if not rawCfg:
                errorStr = '[怪物属性配置]MonsterCfg中不存在id为{}的怪'\
                    .format(monsterId)
                self._collectError.append(errorStr)
                return

            attrLvSet = set()
            monsterAttrCfgs = MonsterAttrCfg().GetById(rawCfg.AttributeTemplateID)
            for attrCfg in monsterAttrCfgs:
                if attrCfg.lv in attrLvSet:
                    errorStr = '[怪物属性配置] id为{}的MonsterAttrCfg表id表存在重复的lv字段：{}'\
                        .format(attrCfg.id, attrCfg.lv)
                    self._collectError.append(errorStr)
                    return
                attrLvSet.add(attrCfg.lv)
            if len(attrLvSet) < 100:
                errorStr = '[怪物属性配置] id为{}的MonsterAttrCfg表lv字段不足100,怪物id:{}'\
                    .format(rawCfg.id, monsterId)
                self._collectError.append(errorStr)
                return

    # 跑环任务检测
    def _VerifyPaoHuanRenWuXiangGuanCfg(self):
        from notcommit.server import *
        NPC = {}    #关联NPC
        for rawCfg in NPCCfg().items:
            relevancyNpc = getattr(rawCfg,"relevancyNpc")
            id = getattr(rawCfg, "id")
            NPC[id] = relevancyNpc

        task = dict()
        for rawCfg in RenWuPeiZhiCfg().items:
            id = getattr(rawCfg,"id")
            lv = getattr(rawCfg,"lv")
            type = getattr(rawCfg,"type")
            taskType = getattr(rawCfg,"taskType")
            linkNpc1 = getattr(rawCfg,"linkNpc1")
            linkNpc2 = getattr(rawCfg,"linkNpc2")
            if taskType == 3 :
                if lv not in task:
                    task[lv] = []
                task[lv].append([id,type,linkNpc1,NPC[linkNpc1]]) #将所有的下一个任务的数据以等级为key存起来

        for rawCfg in RenWuPeiZhiCfg().items:
            taskType = getattr(rawCfg, "taskType")
            if taskType != 3:
                continue
            lv = getattr(rawCfg, "lv")
            linkNpc2 = getattr(rawCfg, "linkNpc2")
            type = getattr(rawCfg, "type")
            id = getattr(rawCfg, "id")
            flag = False
            for i in range(1,lv+1):
                if i not in task:
                    continue
                for code in task[i]:
                    # if code[1] != type:     # 下一个任务的类型要与当前任务类型一致
                    #     continue
                    if code[0] == id:       # 不能接取本次任务
                        continue
                    if code[3] is not None and NPC[linkNpc2] is not None and code[3] == NPC[linkNpc2]: #提交NPC和接取NPC的关联Npc不能相同
                        continue
                    if code[2] != linkNpc2: # 找到一个不相同的npc就算配对成功
                        flag = True
                        break
            if flag == False:
                self._collectError.append(
                    "PaoHuanRenWuPeiZhi verify failed: tasekId:{} lv:{} can not find the next task !!!"
                    .format(id, lv))

    def _VerifyAchievement(self):
        # 成就系统, 代码中有关注如下事件
        from notcommit.server import *
        linkSystem = set()
        for item in AchievementCfg().items:
            linkSystem.add(item.linkSystem)

        # print "------------linkSystem:{}".format(linkSystem)
        for sysId in xrange(1, 6):
            if sysId not in linkSystem:
                # self._collectError.append("AchievementCfg() can not find id:{}".format(sysId))
                pass

        # for sysId in linkSystem:
        #    if sysId not in xrange(0, 22):
        #        self._collectError.append("AchievementCfg() linkSystem({}) unknown".format(sysId))

    def _VerifyTiaoZhanBoCi(self):
        from notcommit.server import *
        for rawCfg in TiaoZhanBoCiCfg().items:
            if len(rawCfg.stageNum.split("/")) != 2:
                print ("TiaoZhanBoCiCfg id:{} stageNum:{}".format(rawCfg.id, rawCfg.stageNum, ))

    def _VerifyBaoShi(self):
        from notcommit.server import *
        items = BaoShiKongPeiZhiCfg().items
        print("abc")
        for item in items:
            pos = item.position
            slotOpen = item.slotOpen
            if pos < 1 or pos > 10:
                print ("A装备配置表.xls/宝石孔配置 position 取值范围应该为 1~10 ,but:{}".format(pos))
            if slotOpen < 1 or slotOpen > 4:
                print ("A装备配置表.xls/宝石孔配置 position 取值范围应该为 1~10 ,but:{}".format(slotOpen))

    def _VerifyMonsterRandomLib(self):
        # 怪物库校验
        from notcommit.server import *
        items = MonsterLibraryCfg().items
        dictMinBossUnLockLv = {}
        for rawCfg in items:
            if not dictMinBossUnLockLv.get(rawCfg.librariesId):
                dictMinBossUnLockLv[rawCfg.librariesId] = 100000
            minUnLockLv = dictMinBossUnLockLv[rawCfg.librariesId]
            if minUnLockLv > rawCfg.unlockLv:
                dictMinBossUnLockLv[rawCfg.librariesId] = rawCfg.unlockLv

        for libId, checkMinLv in dictMinBossUnLockLv.iteritems():
            if checkMinLv != 1:
                errorStr = "<_VerifyMonsterRandomLib> MonsterLibraryCfg, libId:{} min unlockLv != 1".format(libId)
                self._collectError.append(errorStr)

    # 字段数据唯一检测
    def _VerifySole(self):
        from notcommit.server import *

        for id,codes in self._sole.iteritems():
            sourceName = self._configCode.get(id)
            sourceObjectName = sourceName + "()"
            for soleId in codes:
                dic = {}
                List = set()
                eList = {}
                for rawCfg in eval(sourceObjectName).items:
                    source = getattr(rawCfg, soleId)
                    if source not in dic:
                        dic[source] = 'ok'
                    else:
                        List.add(source)
                if len(List) > 0:
                    eList[soleId] = List
            for k in eList:
                 self._collectError.append("Sole verify failed: type:{}  source:{} is not the only in table {}"
                                      .format(k,eList[k],sourceName))

    # 数组长度一致校验
    def VerifyArray(self):
        from notcommit.server import *
        arrayA = {}
        arrayB = {}
        for Key, Value in self._Array.iteritems():
            nameA = self._configCode[Key] + "()"
            itemA = eval(nameA).items
            for arrayType, arrayInfo in Value.iteritems():
                nameB = self._configCode[arrayInfo[0]] + "()"
                cfgMgrB = eval(nameB)
                itemB = cfgMgrB.items
                for i in range(0, len(itemA)):
                    rawCfgA = itemA[i]
                    typeA = getattr(rawCfgA, arrayType)
                    lenA = len(typeA)

                    rawCfgB = itemB[i]
                    typeB = getattr(rawCfgB, arrayInfo[1])
                    lenB = len(typeB)
                    if lenA != lenB:
                        self._collectError.append("[{}] id:{} type:'{}' not match [{}] id:{} type:'{}'"
                                                  .format(nameA, rawCfgA.id, arrayType, nameB, rawCfgB.id, arrayInfo[1]))

    def _VerifyRepeated(self):
        from notcommit.server import *
        for code, repeatedInfo in self._repeated.iteritems():
            tableName = self._configCode.get(code)
            tableObjName = tableName + "()"
            for rawCfg in eval(tableObjName).items:
                for attrName, checkInfo in repeatedInfo.iteritems():
                    atrVal = getattr(rawCfg, attrName)
                    if not isinstance(atrVal._values, list):
                        self._collectError.append("Repeated verify failed: table {}.{} type is not repeated, val:{}"
                                                .format(tableName, attrName, atrVal._values))
                        return
                    else:
                        if len(atrVal._values) != checkInfo[0]:
                            self._collectError.append("Repeated verify failed: table {}.{} arr len error, errVal:{},"
                                                      " rightVal:{}"
                                                      .format(tableName, attrName, atrVal._values, checkInfo[0]))

    def _VerifyMapping(self):
        from notcommit.server import *
        preTargetDict = {}
        preSrcDict = {}
        checkInfoDict = {}
        for srcCode, mappingInfo in self._mapping.iteritems():
            preSrcDict[srcCode] = dict()
            for sourceAttrName, targetInfo in mappingInfo.iteritems():
                targetCode = targetInfo[0]
                targetAttrName = targetInfo[1]
                if not preTargetDict.get(targetCode):
                    preTargetDict[targetCode] = set()
                preTargetDict[targetCode].add(targetAttrName)
                key = targetCode + targetAttrName
                preSrcDict[srcCode][sourceAttrName] = key
                checkInfoDict[key] = [srcCode, sourceAttrName, targetCode, targetAttrName]

        targetDict = {}
        for targetCode, attrNameList in preTargetDict.iteritems():
            targetName = self._configCode.get(targetCode)
            targetObjName = targetName + "()"
            for rawCfg in eval(targetObjName).items:
                for attrName in attrNameList:
                    key = targetCode + attrName
                    if not targetDict.get(key):
                        targetDict[key] = set()
                    listVal = []
                    val = getattr(rawCfg, attrName)
                    if isinstance(val, int):
                        listVal.append(val)
                    else:
                        listVal = val
                    for realVal in listVal:
                        if hasattr(realVal, "value"):
                            realVal = attr.value
                        targetDict[key].add(realVal)

        srcDict = {}
        for srcCode, srcInfo in preSrcDict.iteritems():
            srcName = self._configCode.get(srcCode)
            srcObjName = srcName + "()"
            for rawCfg in eval(srcObjName).items:
                for srcAttr, targetKey in srcInfo.iteritems():
                    srcVal = getattr(rawCfg, srcAttr)
                    if not srcDict.get(targetKey):
                        srcDict[targetKey] = set()
                    srcDict[targetKey].add(srcVal)

        for key, srcData in srcDict.iteritems():
            targetData = targetDict.get(key)
            if not targetData:
                assert False
            res = srcData - targetData
            if len(res) > 0:
                infoList = checkInfoDict[key]
                srcTableName = self._configCode.get(infoList[0])
                srcAttrName = infoList[1]
                targetTableName = self._configCode.get(infoList[2])
                targetAttrName = infoList[3]
                self._collectError.append("Mapping verify failed: table {}.{} {} can't find in {}.{}"
                                        .format(srcTableName, srcAttrName, list(res), targetTableName, targetAttrName))

    def _VerifyBuffCfg(self):
        from notcommit.server import *
        items = BuffCfg().items
        for buffCfg in items:
            assert isinstance(buffCfg, BuffCfgItem)
            if buffCfg.type == 7:
                if buffCfg.propId != 0 or len(buffCfg.rolePropId) != 0:
                    errorStr = "BuffCfg, buff witch type is 7 can't has propId, buff;{}, propId:{}, rolePropId:{}"\
                        .format(buffCfg.id, buffCfg.propId, buffCfg.rolePropId)
                    self._collectError.append(errorStr)

    # endregion

    # region 添加检测数据
    def _fillSourceIds(self, key, value):
        if key not in self._allData.keys():
            sourceId = set()
            targetId = {}
            ids = []
            ids.append(sourceId)
            ids.append(targetId)
            self._allData[key] = ids
        if value in self._allData[key][0]:
            # print "---------_fillSourceIds, repeat key:", key, " value:", value
            return
        self._allData[key][0].add(value)
        # print "[_fillSourceIds] key:", str(key), " value:", str(value)

    def _fillTargetIds(self, key, viceKey, value):
        if key not in self._allData.keys():
            sourceId = set()
            targetId = {}
            ids = []
            ids.append(sourceId)
            ids.append(targetId)
            self._allData[key] = ids
        ids = self._allData[key]
        if viceKey in ids[1].keys():
            if value in ids[1][viceKey]:
                # print "----------_fillTargetIds, repeat viceKey:", viceKey, " value:", value
                return
            ids[1][viceKey].add(value)
        else:
            temp = set()
            temp.add(value)
            ids[1][viceKey] = temp
        # print "[_fillTargetIds] key:", str(key), " viceKey:", str(viceKey), " value:", str(value)

    # endregion

    Dummy = None


