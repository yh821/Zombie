# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from ResScene_pb2 import ResScene_ARRAY


class ResSceneCfgItem(object):
    __slots__ = ("_rawItem")

    """
    File Name: Excel/A场景配置.xlsx
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
    def actor_id(self):
        """
        角色ID
        """
        return self._rawItem.actor_id

    @property
    def pos_x(self):
        """
        出生坐标x
        """
        return self._rawItem.pos_x

    @property
    def pos_z(self):
        """
        坐标z
        """
        return self._rawItem.pos_z

    @property
    def range(self):
        """
        显示半径
        """
        return self._rawItem.range

    @property
    def reset_time(self):
        """
        刷新时间
        """
        return self._rawItem.reset_time


class ResSceneCfg(Singleton):
    """
    File Name: Excel/A场景配置.xlsx
    """
    def __init__(self):
        super(ResSceneCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResScene.data'), 'rb')
        rawItems = ResScene_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResSceneCfgItem(rawItem))

        self._indexedId = {}
        for item in self._items:
            if item.id not in self._indexedId:
                self._indexedId[item.id] = []
            self._indexedId[item.id].append(item)

    def Reload(self):
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResScene.data'), 'rb')
        from time import time
        print"{} reload cfg data:{}".format(int(time()), datafile)
        rawItems = ResScene_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResSceneCfgItem(rawItem))

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
            assert isinstance(item, ResSceneCfgItem)
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

