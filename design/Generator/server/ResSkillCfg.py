# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from ResSkill_pb2 import ResSkill_ARRAY


class ResSkillCfgItem(object):
    __slots__ = ("_rawItem")

    """
    File Name: Excel/A技能配置.xlsx
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
        技能类型
        """
        return self._rawItem.type

    @property
    def level(self):
        """
        等级
        """
        return self._rawItem.level

    @property
    def hp(self):
        """
        治愈值
        """
        return self._rawItem.hp

    @property
    def attack_duration(self):
        """
        攻击间隔(毫秒)
        """
        return self._rawItem.attack_duration

    @property
    def bullet(self):
        """
        子弹
        """
        return self._rawItem.bullet

    @property
    def food(self):
        """
        食物
        """
        return self._rawItem.food

    @property
    def hp_lmt(self):
        """
        增加上限
        """
        return self._rawItem.hp_lmt

    @property
    def sub_speed(self):
        """
        附加速度
        """
        return self._rawItem.sub_speed

    @property
    def attack(self):
        """
        攻击力
        """
        return self._rawItem.attack

    @property
    def defence(self):
        """
        防御力
        """
        return self._rawItem.defence

    @property
    def attack_dist(self):
        """
        攻击距离
        """
        return self._rawItem.attack_dist

    @property
    def find_dist(self):
        """
        发现距离
        """
        return self._rawItem.find_dist

    @property
    def lose_dist(self):
        """
        丢失距离
        """
        return self._rawItem.lose_dist

    @property
    def max_count(self):
        """
        堆叠数
        """
        return self._rawItem.max_count

    @property
    def kind(self):
        """
        武器种类
        """
        return self._rawItem.kind

    @property
    def hit_ratio(self):
        """
        命中率
        """
        return self._rawItem.hit_ratio

    @property
    def dodge_ratio(self):
        """
        闪避率
        """
        return self._rawItem.dodge_ratio

    @property
    def crit_ratio(self):
        """
        暴击率
        """
        return self._rawItem.crit_ratio

    @property
    def decrit_ratio(self):
        """
        抗暴率
        """
        return self._rawItem.decrit_ratio


class ResSkillCfg(Singleton):
    """
    File Name: Excel/A技能配置.xlsx
    """
    def __init__(self):
        super(ResSkillCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResSkill.data'), 'rb')
        rawItems = ResSkill_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResSkillCfgItem(rawItem))

        self._indexedId = {}
        for item in self._items:
            if item.id not in self._indexedId:
                self._indexedId[item.id] = []
            self._indexedId[item.id].append(item)

    def Reload(self):
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'ResSkill.data'), 'rb')
        from time import time
        print"{} reload cfg data:{}".format(int(time()), datafile)
        rawItems = ResSkill_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ResSkillCfgItem(rawItem))

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
            assert isinstance(item, ResSkillCfgItem)
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

