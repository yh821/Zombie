# -*- coding: utf-8 -*-


class ElemAttrs(object):
    """
    元素属性集合
    """

    def __init__(self, elemType):
        self.elemType = elemType  # 元素类型

        self.elemLv = 1  # 元素等级
        self.addiDmgScale = 0.  # 附加伤害比例
        self.undAtkMulPosAmend = 0.0  # 受击乘法正修正
        self.undAtkMulNegAmend = 0.0  # 受击乘法负修正
        self.dmgMulPosAmend = 0.0  # 伤害乘法正修正
        self.addAmend = 0.0  # 加法修正
