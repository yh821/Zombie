# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from ResExp_pb2 import ResExp_ARRAY


class ResExpCfgItem(object):
    __slots__ = ("_rawItem")

    """
    File Name: Excel/A角色配置.xlsx
    """
    def __init__(self, rawItem):
        self._rawItem = rawItem

    @property
    def level(self):
        """
        等级
        """
        return self._rawItem.level

    @property
    def min(self):
        """
        下限
        """
        return self._rawItem.min

    @property
    def max(self):
        """
        上限
        """
        return self._rawItem.max

    @property
    def hp_lmt(self):
        """
        生命
        """
        return self._rawItem.hp_lmt

    @property
    def attack(self):
        """
        攻击
        """
        return self._rawItem.attack

    @property
    def defence(self):
        """
        防御
        """
        return self._rawItem.defence


class ResExpCfg(Singleton):
    """
    File Name: Excel/A角色配置.xlsx
    """
    def __init__(self):
        super(ResExpCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResExp.data'), 'rb')
        rawItems = ResExp_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResExpCfgItem(rawItem))

        self._indexedLevel = {}
        for item in self._items:
            if item.level not in self._indexedLevel:
                self._indexedLevel[item.level] = []
            self._indexedLevel[item.level].append(item)

    def Reload(self):
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResExp.data'), 'rb')
        from time import time
        print"{} reload cfg data:{}".format(int(time()), datafile)
        rawItems = ResExp_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResExpCfgItem(rawItem))

        self._indexedLevel = {}
        for item in self._items:
            if item.level not in self._indexedLevel:
                self._indexedLevel[item.level] = []
            self._indexedLevel[item.level].append(item)

    @property
    def items(self):
        return self._items

    def GetByLevel(self, levelVal):
        items = self._indexedLevel.get(levelVal)
        return items if items is not None else []

    def GetOneByLevel(self, levelVal):
        items = self.GetByLevel(levelVal)
        if items:
            item = items[0]
            assert isinstance(item, ResExpCfgItem)
            return item
        else:
            return None

    @property
    def levels(self):
        return self._indexedLevel.keys()

    def __str__(self):
        return str(self._items)

    def __len__(self):
        return len(self._items)

