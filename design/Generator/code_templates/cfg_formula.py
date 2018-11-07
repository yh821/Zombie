# -*- coding: utf8 -*-


class CfgFormula(object):
    __slots__ = ('_ty')

    """配置公式封装"""

    # 类型枚举
    Accum = 0
    NegAmend = 1
    Multiply = 2

    def __init__(self, ty, initArg):
        self._ty = ty

    @property
    def ty(self):
        return self._ty

    @property
    def val(self):
        raise NotImplemented()

    def AddArg(self, arg):
        raise NotImplemented()

    def RemoveArg(self, arg):
        raise NotImplemented()

    def SetArg(self, arg):
        raise NotImplementedError()


class CfgFormulaAccum(CfgFormula):
    __slots__ = ('_val')

    """累加公式封装"""

    def __init__(self, ty, initArg):
        super(CfgFormulaAccum, self).__init__(ty, initArg)
        self._val = ty(initArg)

    @property
    def val(self):
        return self._val

    def AddArg(self, arg):
        self._val += self._ty(arg)

    def RemoveArg(self, arg):
        self._val -= self._ty(arg)

    def SetArg(self, arg):
        self._val = self._ty(arg)


class CfgFormulaNegAmend(CfgFormula):
    __slots__ = ('_args', '_val')

    """负修正公式"""

    def __init__(self, ty, initArg):
        super(CfgFormulaNegAmend, self).__init__(ty, initArg)
        # self._ty = ty
        self._args = []
        self._val = ty(0.0)

        self.AddArg(initArg)

    @property
    def val(self):
        return self._val

    def AddArg(self, arg):
        self._args.append(self._ty(arg))
        self._Update()

    def RemoveArg(self, arg):
        arg = self._ty(arg)
        for idx in xrange(len(self._args)):
            if arg == self._args[idx]:
                del self._args[idx]
                self._Update()
                break

    def SetArg(self, arg):
        del self._args[:]
        self.AddArg(arg)

    def _Update(self):
        inlVal = 0.0
        for arg in self._args:
            if arg >= 1.0:
                self._val = self._ty(1.0)
                break
            elif arg == 0.0:
                continue
            inlVal += (1.0 / (1.0 - arg) - 1.0)

        self._val = self._ty(1.0 - 1.0 / (1.0 + inlVal))


class CfgFormulaMultiply(CfgFormula):
    __slots__ = ('_val', '_initVal',  '_argList')

    """累乘公式封装"""

    def __init__(self, ty, initArg):
        super(CfgFormulaMultiply, self).__init__(ty, initArg)
        self._initVal = ty(initArg)
        self._val = ty(initArg)
        self._argList = []

    @property
    def val(self):
        return self._val

    def AddArg(self, arg):
        self._argList.append(arg)
        self._argList.sort()
        self._CalVal()

    def RemoveArg(self, arg):
        if arg in self._argList:
            self._argList.remove(arg)
        self._CalVal()

    def _CalVal(self):
        self._val = self._initVal
        for val in self._argList:
            self._val = self._ty((self._val + 1000) * (val * 0.001 + 1) - 1000)

    def SetArg(self, arg):
        self._initVal = self._ty(arg)
        self._val = self._initVal
        self._argList = []


class CfgFormulaBuilder(object):
    """公式创建者"""
    @staticmethod
    def Build(formulaType, ty, initVal):
        if formulaType == CfgFormula.Accum:
            return CfgFormulaAccum(ty, initVal)
        elif formulaType == CfgFormula.NegAmend:
            return CfgFormulaNegAmend(ty, initVal)
        elif formulaType == CfgFormula.Multiply:
            return CfgFormulaMultiply(ty, initVal)
        return None
