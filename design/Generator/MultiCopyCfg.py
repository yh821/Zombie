# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from MultiCopy_pb2 import MultiCopy_ARRAY


class MultiCopyCfgItem(object):
    """
    File Name: d多人副本/A多人副本配置表.xls
    """
    def __init__(self, rawItem):
        self._rawItem = rawItem

    @property
    def key(self):
        """
        open_condition_i
        """
        return self._rawItem.key

    @property
    def value(self):
        """
        6.0
        """
        return self._rawItem.value


class MultiCopyCfg(Singleton):
    """
    File Name: d多人副本/A多人副本配置表.xls
    """
    def __init__(self):
        super(MultiCopyCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'MultiCopy.data'), 'rb')
        rawItems = MultiCopy_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(MultiCopyCfgItem(rawItem))

        self._indexedKey = {}
        for item in self._items:
            if item.key not in self._indexedKey:
                self._indexedKey[item.key] = []
            self._indexedKey[item.key].append(item)

    @property
    def items(self):
        return self._items

    def GetByKey(self, keyVal):
        items = self._indexedKey.get(keyVal)
        return items if items is not None else []

    def GetOneByKey(self, keyVal):
        items = self.GetByKey(keyVal)
        return items[0] if items else None

    @property
    def keys(self):
        return self._indexedKey.keys()

    def __str__(self):
        return str(self._items)

    def __len__(self):
        return len(self._items)

