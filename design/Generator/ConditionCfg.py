# -*- coding: utf-8 -*-
# Auto generate by Script Tool
from os import path as op
from llbc import Singleton
from Condition_pb2 import Condition_ARRAY


class ConditionCfgItem(object):
    """
    File Name: z战斗系统/A条件配置表.xls
    """
    def __init__(self, rawItem):
        self._rawItem = rawItem

    @property
    def conditionId(self):
        """
        条件id
        """
        return self._rawItem.conditionId

    @property
    def target(self):
        """
        目标类型
1.敌人
2.队友
3.自己
4.我方
5.属主
6.全部
7.宠物
8.伤害来源
        """
        return self._rawItem.target

    @property
    def occasion(self):
        """
        1.受到攻击
2.属性变更
3.施放技能
4.进场开始
5.死亡开始
        """
        return self._rawItem.occasion

    @property
    def delay(self):
        """
        延时
        """
        return self._rawItem.delay

    @property
    def damageType(self):
        """
        0.任意
1.近战
2.远程
3.物理
4.魔法
        """
        return self._rawItem.damageType

    @property
    def attr(self):
        """
        属性变更
1.血量
2.行动点
        """
        return self._rawItem.attr

    @property
    def percentage(self):
        """
        百分数
        """
        return self._rawItem.percentage

    @property
    def base(self):
        """
        绝对值
        """
        return self._rawItem.base


class ConditionCfg(Singleton):
    """
    File Name: z战斗系统/A条件配置表.xls
    Last Changed Author(When generating): zhouxinming
    """
    def __init__(self):
        super(ConditionCfg, self).__init__()
        cur_dir = op.dirname(op.abspath(__file__))
        datafile = open(op.join(cur_dir, 'data', 'Condition.data'), 'rb')
        rawItems = Condition_ARRAY()
        rawItems.ParseFromString(datafile.read())
        self._items = []
        for rawItem in rawItems.items:
            self._items.append(ConditionCfgItem(rawItem))

        self._indexedConditionId = {}
        for item in self._items:
            if item.conditionId not in self._indexedConditionId:
                self._indexedConditionId[item.conditionId] = []
            self._indexedConditionId[item.conditionId].append(item)

    @property
    def items(self):
        return self._items

    def GetByConditionId(self, conditionIdVal):
        items = self._indexedConditionId.get(conditionIdVal)
        return items if items is not None else []

    def GetOneByConditionId(self, conditionIdVal):
        items = self.GetByConditionId(conditionIdVal)
        return items[0] if items else None

    @property
    def conditionIds(self):
        return self._indexedConditionId.keys()

    def __str__(self):
        return str(self._items)

    def __len__(self):
        return len(self._items)

