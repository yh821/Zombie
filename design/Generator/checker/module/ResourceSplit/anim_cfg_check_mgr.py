# -*- coding: utf-8 -*-
import json
from llbc import Singleton
import os
from os import path as _op


class AnimCheckCfg(object):
    def __init__(self, json):
        self._modelId = json['model_id']
        self._actName2effectIds = dict()  # dict{actName, list(effectId)}
        self._actName2Fx = dict()
        self._actName2Audio = dict()
        animList = json['anims']
        for anim in animList:
            actName = anim['name']
            tags = anim['tags']
            for tag in tags:
                function = tag['function']
                if function == 'Effect':
                    if not self._actName2effectIds.get(actName):
                        self._actName2effectIds[actName] = set()
                    self._actName2effectIds[actName].add(int(tag['int_val']))
                elif function == 'fx':
                    if not self._actName2Fx.get(actName):
                        self._actName2Fx[actName] = set()
                    self._actName2Fx[actName].add(int(tag['int_val']))
                elif function == 'audio':
                    if not self._actName2Audio.get(actName):
                        self._actName2Audio[actName] = set()
                    self._actName2Audio[actName].add(int(tag['int_val']))

    def GetModelId(self):
        return self._modelId

    def GetEffectsByActName(self, name):
        list = self._actName2effectIds.get(name)
        if list:
            return list
        return []

    def GetFxByActName(self, name):
        list = self._actName2Fx.get(name)
        if list:
            return list
        return []

    def GetAudioByActName(self, name):
        list = self._actName2Audio.get(name)
        if list:
            return list
        return []


class ModelAnimCfgCheckMgr(Singleton):
    """模型动作配置数据检测管理器"""
    def __init__(self):
        super(ModelAnimCfgCheckMgr, self).__init__()
        self._modelAnims = {}

    def Load(self, path):
        self._MyInit(path)
        self._AfterInit()

    def _MyInit(self, path):
        self._modelAnims.clear()
        for root, dirs, files in os.walk(_op.join(path)):
            for name in files:
                if _op.splitext(name)[1] != '.animdata':
                    continue

                animFile = _op.join(root, name)
                if '.svn' in animFile:
                    continue

                with open(animFile) as f:
                    jsonData = json.loads(f.read())
                    cfg = AnimCheckCfg(jsonData)
                    self._modelAnims[cfg.GetModelId()] = cfg

    def _AfterInit(self):
        # 从AvatorModelCfg中加载配有相同动作的modelId
        import re
        from notcommit.server import AvatarModelCfg
        avatarModelCfg = AvatarModelCfg()
        for rawItem in avatarModelCfg.items:
            if self._modelAnims.get(rawItem.id):
                continue
            actionName = rawItem.action
            if not actionName.strip():
                continue
            rule = re.compile(r'\d+')
            modelList = rule.findall(actionName)
            assert len(modelList) == 1
            extendModelId = int(modelList[0])
            modelAim = self._modelAnims.get(extendModelId)
            if not modelAim:
                continue
            self._modelAnims[rawItem.id] = modelAim

    def GetModelActionEffects(self, modelId, actionName):
        """取得模型动作附加的效应"""
        modelAnim = self._modelAnims.get(modelId)
        if modelAnim is not None:
            assert isinstance(modelAnim, AnimCheckCfg)
            return modelAnim.GetEffectsByActName(actionName)
        else:
            return []

    def GetModelActionFx(self, modelId, actionName):
        """取得模型动作附加的特效"""
        modelAnim = self._modelAnims.get(modelId)
        if modelAnim is not None:
            assert isinstance(modelAnim, AnimCheckCfg)
            return modelAnim.GetFxByActName(actionName)
        else:
            return []

    def GetModelActionAudio(self, modelId, actionName):
        """取得模型动作附加的音效"""
        modelAnim = self._modelAnims.get(modelId)
        if modelAnim is not None:
            assert isinstance(modelAnim, AnimCheckCfg)
            return modelAnim.GetAudioByActName(actionName)
        else:
            return []
