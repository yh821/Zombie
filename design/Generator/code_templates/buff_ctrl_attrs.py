# -*- coding: utf-8 -*-

class BuffCtrlAttrs(object):
    """
    Buff控制相关属性集合
    """

    def __init__(self, elemType):
        self.elemType = elemType  # 元素类型

        self.addProbAddAmend = 0.0  # 添加机率加法修正
        self.addProbMulPosAmend = 0.0  # 添加机率乘法正修正
        self.addProbMulNegAmend = 0.0  # 添加机率乘法负修正
        self.durTimeAddAmend = 0.0  # 持续时间加法修正(毫秒)
        self.durTimeMulPosAmend = 0.0  # 持续时间乘法正修正
        self.durTimeMulNegAmend = 0.0  # 持续时间乘法负修正

