# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from ResActor_pb2 import ResActor_ARRAY


class ResActorCfgItem(object):
    __slots__ = ("_rawItem")

    """
    File Name: Excel/A角色配置.xlsx
    """
    def __init__(self, rawItem):
        self._rawItem = rawItem

    @property
    def id(self):
        """
        ID
        """
        return self._rawItem.id

    @property
    def type(self):
        """
        角色类型
        """
        return self._rawItem.type

    @property
    def level(self):
        """
        等级
        """
        return self._rawItem.level

    @property
    def prefab(self):
        """
        预制名
        """
        return self._rawItem.prefab

    @property
    def radius(self):
        """
        身体半径
        """
        return self._rawItem.radius

    @property
    def skills(self):
        """
        初始技能
        """
        return self._rawItem.skills

    @property
    def items(self):
        """
        自带道具
        """
        return self._rawItem.items

    @property
    def speed(self):
        """
        自身速度
        """
        return self._rawItem.speed

    @property
    def exp(self):
        """
        击杀经验
        """
        return self._rawItem.exp


class ResActorCfg(Singleton):
    """
    File Name: Excel/A角色配置.xlsx
    """
    def __init__(self):
        super(ResActorCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResActor.data'), 'rb')
        rawItems = ResActor_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResActorCfgItem(rawItem))

        self._indexedId = {}
        for item in self._items:
            if item.id not in self._indexedId:
                self._indexedId[item.id] = []
            self._indexedId[item.id].append(item)

    def Reload(self):
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResActor.data'), 'rb')
        from time import time
        print"{} reload cfg data:{}".format(int(time()), datafile)
        rawItems = ResActor_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResActorCfgItem(rawItem))

        self._indexedId = {}
        for item in self._items:
            if item.id not in self._indexedId:
                self._indexedId[item.id] = []
            self._indexedId[item.id].append(item)

    @property
    def items(self):
        return self._items

    def GetById(self, idVal):
        items = self._indexedId.get(idVal)
        return items if items is not None else []

    def GetOneById(self, idVal):
        items = self.GetById(idVal)
        if items:
            item = items[0]
            assert isinstance(item, ResActorCfgItem)
            return item
        else:
            return None

    @property
    def ids(self):
        return self._indexedId.keys()

    def __str__(self):
        return str(self._items)

    def __len__(self):
        return len(self._items)

