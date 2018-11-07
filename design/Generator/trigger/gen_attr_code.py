# -*- coding: utf-8 -*-

from os import path as _op

import xlrd

from c import *
from file_util import FileUtil
from spelling_converter import SpellingConverter


class AttrSummary(object):
    """属性摘要"""

    def __init__(self):
        self.id = 0
        self.id_type = u'float'
        self.comments = u''
        self.dft_val = u''
        self.formula = 0
        self.enum = u''
        self.lang_id = 0
        self.sub_attrs = {}  # key: careerType, value: [(attrType(int), attrValue(float))]
        self.display = 0

    @property
    def utf8_id_type(self):
        return self.id_type.encode('utf-8')

    @property
    def utf8_comments(self):
        return self.comments.encode('utf-8')

    @property
    def utf8_enum(self):
        return self.enum.encode('utf-8')

    @property
    def lower_case_enum(self):
        en = self.utf8_enum
        if not en:
            return en

        if 65 <= ord(en[0]) <= 90:
            return chr(ord(en[0]) + 32) + en[1:]
        return en

    @property
    def converted_dft_val(self):
        val_str = str(self.dft_val)
        if not val_str:
            val_str = '0'
        id_type = self.utf8_id_type

        if id_type == 'float':
            return float(val_str)
        elif id_type == 'int':
            return int(float(val_str))
        elif id_type == 'bool':
            return bool(float(val_str))
        elif id_type == 'long':
            return long(float(val_str))
        else:
            return val_str


class AttrSummaryPicker(object):
    """属性摘要拾取器"""

    # 列Id枚举
    _ID_IDX = 1
    _COMMENTS_IDX = _ID_IDX + 1
    _TYPE_IDX = _ID_IDX + 2
    _DFT_VAL_IDX = _ID_IDX + 3
    _FORMULA = _ID_IDX + 4
    _ENUM_IDX = _ID_IDX + 5
    _LANG_ID = _ID_IDX + 6
    _SUB_ATTR_TYPES = _ID_IDX + 7
    _SUB_ATTR_VALUES = _ID_IDX + 8
    _DISPLAY_IDX = _ID_IDX + 9

    def __init__(self, attr_file):
        self._summaries = []
        self._attr_file = attr_file

    @property
    def attr_file(self):
        return self._attr_file

    @property
    def summaries(self):
        return self._summaries

    def pick(self):
        if not _op.exists(self._attr_file):
            return

        wb = xlrd.open_workbook(self._attr_file)
        sheet = wb.sheet_by_name(u'AttrTypes')
        self._summaries.extend([self._make_summary(sheet, idx)
                                for idx in xrange(4, sheet.nrows)])

    def pickSystemOpen(self, des):
        if not _op.exists(self._attr_file):
            return

        wb = xlrd.open_workbook(self._attr_file)
        sheet = wb.sheet_by_name(u'SystemOpen')

        systemS = set()
        valueS = set()
        for idx in xrange(4, sheet.nrows):
            system = sheet.cell(idx, 0).value.encode('utf-8')
            value = int(float(sheet.cell(idx, 1).value))
            pinyin = SpellingConverter().parse_str(system).encode('utf-8')

            if pinyin in systemS:
                print('{} - SystemOpen id:{} repeated'.format(des, pinyin))
                assert False
            if value in valueS:
                print('{} - SystemOpen id:{} repeated'.format(des, value))
                assert False

            self._summaries.append((pinyin, value, system))

    @classmethod
    def _make_summary(cls, sheet, row_idx):
        summ = AttrSummary()

        id_cell = sheet.cell(row_idx, cls._ID_IDX)
        summ.id = int(id_cell.value)

        comm_cell = sheet.cell(row_idx, cls._COMMENTS_IDX)
        summ.comments = comm_cell.value

        type_cell = sheet.cell(row_idx, cls._TYPE_IDX)
        summ.id_type = type_cell.value
        if summ.id_type == 'int64':
            summ.id_type = 'long'

        dft_val_cell = sheet.cell(row_idx, cls._DFT_VAL_IDX)
        if dft_val_cell.value:
            summ.dft_val = dft_val_cell.value
        else:
            summ.dft_val = '0'

        formula_cell = sheet.cell(row_idx, cls._FORMULA)
        if formula_cell.value:
            summ.formula = int(formula_cell.value)
        else:
            summ.formula = 0

        enum_cell = sheet.cell(row_idx, cls._ENUM_IDX)
        summ.enum = enum_cell.value

        lang_id_cell = sheet.cell(row_idx, cls._LANG_ID)
        summ.lang_id = int(lang_id_cell.value)

        def _exec_split(row_val, value_type):
            result = {}
            pairs = row_val.split(';')
            for pair in pairs:
                sub_pairs = pair.split(':')
                if len(sub_pairs) != 2:
                    continue
                career_type = int(sub_pairs[0])
                values = sub_pairs[1].split(',')

                result[career_type] = []
                for value in values:
                    if not value:
                        result[career_type].append(value_type(0))
                    else:
                        result[career_type].append(value_type(float(value)))
            return result

        sub_attr_types_cell = sheet.cell(row_idx, cls._SUB_ATTR_TYPES)
        sub_attr_types = _exec_split(str(sub_attr_types_cell.value), int)

        sub_attr_values_cell = sheet.cell(row_idx, cls._SUB_ATTR_VALUES)
        sub_attr_values = _exec_split(str(sub_attr_values_cell.value), float)

        if len(sub_attr_types) != len(sub_attr_values):
            print 'Sub attrs career type count[{}] != Sub attrs career value count[{}], execl row: {}' \
                .format(len(sub_attr_types), len(sub_attr_values), row_idx)
            return summ

        for career_type, attr_types in sub_attr_types.iteritems():
            attr_values = sub_attr_values.get(career_type)
            if attr_values is None:
                print 'Sub attr values not found, row: {}, career: {}'.format(row_idx, career_type)
                return summ
            elif len(attr_types) != len(attr_values):
                print 'Sub attr types count[{}] !f values count[{}], row: {}, career: {}' \
                    .format(len(attr_types), len(attr_values), row_idx, career_type)
                return summ
            summ.sub_attrs[career_type] = [(attr_type, attr_values[idx]) for idx, attr_type in enumerate(attr_types)]

        display_cell = sheet.cell(row_idx, cls._DISPLAY_IDX)
        if display_cell.value:
            summ.display = int(display_cell.value)
        else:
            summ.display = 0

        return summ


class _F(object):
    def __init__(self, file_name, mode='w'):
        self._f = open(file_name, mode)

    def w(self, line):
        self._f.write(line.encode('utf-8') if isinstance(line, unicode) else str(line) + '\n')

    def done(self):
        self._f.close()


class PyAttrBaseInfoExporter(object):
    def __init__(self, export_path):
        self._export_path = export_path
        self._export_file = _op.join(export_path, u'attr_base_info.py')

    def export(self, summaries):
        f = _F(self._export_file)
        w = f.w

        # Generate enum code file
        w('# -*- coding: utf-8 -*-')
        w('')

        w('class AttrBaseInfo(object):')
        w('    def __init__(self):')
        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            name = summ.lower_case_enum

            w('        self.{} = {}({})'.format(name, summ.id_type, summ.dft_val))
        w('')


class PyEnumExporter(object):
    """Python枚举导出器"""

    def __init__(self, export_path):
        self._export_path = export_path
        self._export_file = _op.join(export_path, u'attr_type.py')

    def export(self, summaries):
        priorityId = dict()
        f = _F(self._export_file)
        w = f.w

        # Generate enum code file
        w('# -*- coding: utf-8 -*-')
        w('')

        w('class AttrType(object):')
        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            w('    {} = {}  # {}'.format(summ.utf8_enum, summ.id, summ.utf8_comments))
            if summ.utf8_comments.endswith('*'):
                priorityId[summ.id] = summ
        return priorityId


class PyMgrEnumExporter(object):
    """Python枚举导出器"""

    def __init__(self, export_path):
        self._export_path = export_path
        self._export_file = _op.join(export_path, u'module_type.py')

    def export(self, summaries):
        f = _F(self._export_file)
        w = f.w

        # Generate enum code file
        w('# -*- coding: utf-8 -*-')
        w('# Auto generate by Script Tool')
        w('')

        w('class SystemOpen(object):')
        for summ in summaries:
            key, value, comment = summ
            # w('    {} = {}  # {}'.format(key, value, comment))
            w('    Id{} = {}  # {}'.format(value, value, comment))

        pass


class PyAttrBaseExporter(object):
    """Python枚举导出器"""

    def __init__(self, export_path):
        self._export_path = export_path
        self._export_file = _op.join(export_path, u'attr_base.py')

    def export(self, summaries, priorityId):
        f = _F(self._export_file)
        w = f.w

        # Copy code-template files to export path.
        FileUtil.copy(_op.join(CODE_TEMPL_PATH, 'elem_attrs.py'), self._export_path)
        # FileUtil.copy(_op.join(CODE_TEMPL_PATH, 'cfg_formula.py'), self._export_path)
        FileUtil.copy(_op.join(CODE_TEMPL_PATH, 'buff_ctrl_attrs.py'), self._export_path)

        # Generate attr-base code file
        w('# -*- coding: utf-8 -*-')
        w('# Auto generate by Script Tool\n')
        w('from llbc import Log\n')
        w('from attr_type import AttrType as _AttrType')
        w('from AttrsCfg import AttrsCfg')
        w('from cfg_formula import *')
        w('from elem_attrs import *')
        w('import weakref')
        w('from buff_ctrl_attrs import *\n\n')

        w('class WeakMethod(object):')
        w('    __slots__ = ("f", "c")')
        w('')
        w('    def __init__(self, f):')
        w('        self.f = f.im_func')
        w('        self.c = weakref.ref(f.im_self)')
        w('')
        w('    def __call__(self, *arg):')
        w('        if self.c() is None:')
        w('            print "No more object"')
        w('            assert 0')
        w('            return')
        w('        return apply(self.f, (self.c(), ) + arg)')
        w('\n')

        updated_meth_name = '_OnAttrChanged'

        sub_attrs = []
        gets = []
        sets = []
        adds = []
        removes = []
        swaps = []
        getsets = []
        copy_stmts = []
        resets = []
        attrNames = {}
        cls_name = 'AttrBase'
        w('class AttrBase(object):')
        w('    _subAttrs = {')
        for summ in summaries:
            if not summ.sub_attrs:
                continue

            # key: attr_type, value {career: [(sub_type, sub_value)]}
            w('        {}: {{'.format(summ.id))
            for career_type, type_value_pairs in summ.sub_attrs.iteritems():
                if not type_value_pairs:
                    continue

                w('            {}: ['.format(career_type))
                for pair in type_value_pairs:
                    w('                ({}, {}),'.format(pair[0], pair[1]))
                w('            ],')
            w('        }')
        w('    }')
        w('')

        w('    @classmethod')
        w('    def GetSubAttrs(cls, attrType, career):')
        w('        careerAttrs = cls._subAttrs.get(attrType)')
        w('        if not careerAttrs:')
        w('            return []')
        w('        attrPairs = careerAttrs.get(career)')
        w('        if attrPairs is None:')
        w('            return []')
        w('        else:')
        w('            return attrPairs')
        w('')

        w('    def __init__(self):')
        w('        self._autoExpandSubAttrs = True')
        w('        self._attrCareerType = 0')
        w('')

        for summ in priorityId.itervalues():
            assert isinstance(summ, AttrSummary)
            name = summ.lower_case_enum
            w('        self._{} = CfgFormulaBuilder.Build({}, {}, {})  # {}, Attribute Id: {}'
              .format(name, summ.formula, summ.utf8_id_type, summ.converted_dft_val, summ.utf8_comments, summ.id))
            if not gets:
                gets.append('        if attrType == _AttrType.{}:'.format(summ.utf8_enum))
            else:
                gets.append('        elif attrType == _AttrType.{}:'.format(summ.utf8_enum))

            sets.append(gets[-1])
            # adds.append(gets[-1])
            # removes.append(gets[-1])
            swaps.append(gets[-1])
            gets.append('            return self.{}'.format(name))
            sets.append('            self._Set{}(attrVal, nty)'.format(summ.utf8_enum))
            # adds.append('            self._Add{}(attrVal, nty)'.format(summ.utf8_enum))
            # removes.append('            self._Remove{}(attrVal, nty)'.format(summ.utf8_enum))
            swaps.append('            self.Swap{}(oldAttrVal, newAttrVal)'.format(summ.utf8_enum))
            getsets.append('    @property')
            getsets.append('    def {}(self):'.format(name))
            getsets.append('        """取得{}"""'.format(summ.utf8_comments))
            getsets.append('        return self._{}.val'.format(name))
            getsets.append('')
            getsets.append('    @{}.setter'.format(name))
            getsets.append('    def {}(self, value):'.format(name))
            getsets.append('        """设置{}"""'.format(summ.utf8_comments))
            getsets.append('        self.Set{}(value)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Add{}(self, addVal, nty):'.format(summ.utf8_enum))
            getsets.append('        addVal = {}(addVal)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.AddArg(addVal)'.format(summ.lower_case_enum))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._AddValue(subAttrPair[0], subAttrPair[1] * addVal, nty)')
            getsets.append('')
            getsets.append('    def Add{}(self, addVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}增加值"""'.format(summ.utf8_comments))
            getsets.append('        self._Add{}(addVal, True)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Get{}(self):'.format(summ.utf8_enum))
            getsets.append('        """私有接口获取{}值"""'.format(summ.utf8_comments))
            getsets.append('        return self.{}'.format(name))
            getsets.append('')
            getsets.append('    def _Remove{}(self, removeVal, nty):'.format(summ.utf8_enum))
            getsets.append('        removeVal = {}(removeVal)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.RemoveArg(removeVal)'.format(summ.lower_case_enum))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._RemoveValue(subAttrPair[0], subAttrPair[1] * removeVal, nty)')
            getsets.append('')
            getsets.append('    def Remove{}(self, removeVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}移除一个值"""'.format(summ.utf8_comments))
            getsets.append('        self._Remove{}(removeVal, True)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def Swap{}(self, oldVal, newVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}更新一个值"""'.format(summ.utf8_comments))
            getsets.append('        if oldVal == newVal:')
            getsets.append('            return')
            getsets.append('')
            # getsets.append('        self._Remove{}(oldVal, False)'.format(summ.utf8_enum))
            # getsets.append('        self._Add{}(newVal, True)'.format(summ.utf8_enum))
            getsets.append('        self.Add{}(newVal - oldVal)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Set{}(self, value, nty):'.format(summ.utf8_enum))
            getsets.append('        value = {}(value)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.SetArg(value)'.format(name))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._SetValue(subAttrPair[0], subAttrPair[1] * value, nty)')
            getsets.append('')
            getsets.append('    def Set{}(self, value):'.format(summ.utf8_enum))
            getsets.append('        """设置{}"""'.format(summ.utf8_comments))
            getsets.append('        self._Set{}(value, True)'.format(summ.utf8_enum))
            getsets.append('')

            copy_stmts.append('        cp._{}._val = self._{}._val'.format(summ.lower_case_enum, summ.lower_case_enum))
            resets.append('        self._{}.SetArg({})'.format(summ.lower_case_enum, summ.converted_dft_val))

        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            if summ.id in priorityId.keys():
                continue
            name = summ.lower_case_enum
            w('        self._{} = CfgFormulaBuilder.Build({}, {}, {})  # {}, Attribute Id: {}'
              .format(name, summ.formula, summ.utf8_id_type, summ.converted_dft_val, summ.utf8_comments, summ.id))
            if not gets:
                gets.append('        if attrType == _AttrType.{}:'.format(summ.utf8_enum))
            else:
                gets.append('        elif attrType == _AttrType.{}:'.format(summ.utf8_enum))
            attrNames[summ.id] = summ.enum

            sets.append(gets[-1])
            # adds.append(gets[-1])
            # removes.append(gets[-1])
            swaps.append(gets[-1])
            gets.append('            return self.{}'.format(name))
            sets.append('            self._Set{}(attrVal, nty)'.format(summ.utf8_enum))
            # adds.append('            self._Add{}(attrVal, nty)'.format(summ.utf8_enum))
            # removes.append('            self._Remove{}(attrVal, nty)'.format(summ.utf8_enum))
            swaps.append('            self.Swap{}(oldAttrVal, newAttrVal)'.format(summ.utf8_enum))

            getsets.append('    @property')
            getsets.append('    def {}(self):'.format(name))
            getsets.append('        """取得{}"""'.format(summ.utf8_comments))
            getsets.append('        return self._{}.val'.format(name))
            getsets.append('')
            getsets.append('    @{}.setter'.format(name))
            getsets.append('    def {}(self, value):'.format(name))
            getsets.append('        """设置{}"""'.format(summ.utf8_comments))
            getsets.append('        self.Set{}(value)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Add{}(self, addVal, nty):'.format(summ.utf8_enum))
            getsets.append('        addVal = {}(addVal)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.AddArg(addVal)'.format(summ.lower_case_enum))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._AddValue(subAttrPair[0], subAttrPair[1] * addVal, nty)')
            getsets.append('')
            getsets.append('    def Add{}(self, addVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}增加值"""'.format(summ.utf8_comments))
            getsets.append('        self._Add{}(addVal, True)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Get{}(self):'.format(summ.utf8_enum))
            getsets.append('        """私有接口获取{}值"""'.format(summ.utf8_comments))
            getsets.append('        return self.{}'.format(name))
            getsets.append('')
            getsets.append('    def _Remove{}(self, removeVal, nty):'.format(summ.utf8_enum))
            getsets.append('        removeVal = {}(removeVal)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.RemoveArg(removeVal)'.format(summ.lower_case_enum))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._RemoveValue(subAttrPair[0], subAttrPair[1] * removeVal, nty)')
            getsets.append('')
            getsets.append('    def Remove{}(self, removeVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}移除一个值"""'.format(summ.utf8_comments))
            getsets.append('        self._Remove{}(removeVal, True)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def Swap{}(self, oldVal, newVal):'.format(summ.utf8_enum))
            getsets.append('        """为{}更新一个值"""'.format(summ.utf8_comments))
            getsets.append('        if oldVal == newVal:')
            getsets.append('            return')
            getsets.append('')
            # getsets.append('        self._Remove{}(oldVal, False)'.format(summ.utf8_enum))
            # getsets.append('        self._Add{}(newVal, True)'.format(summ.utf8_enum))
            getsets.append('        self.Add{}(newVal - oldVal)'.format(summ.utf8_enum))
            getsets.append('')
            getsets.append('    def _Set{}(self, value, nty):'.format(summ.utf8_enum))
            getsets.append('        value = {}(value)  # Force convert to legal type'.format(summ.utf8_id_type))
            getsets.append('        oldVal = self._{}.val'.format(name))
            getsets.append('        self._{}.SetArg(value)'.format(name))
            getsets.append('        if self._{}.val != oldVal:'.format(name))
            getsets.append('            if nty:')
            getsets.append(
                '                self._OnAttrChanged(_AttrType.{}, oldVal, self._{}.val)'.format(summ.utf8_enum, name))
            getsets.append('        if self._autoExpandSubAttrs:')
            getsets.append('            subAttrPairs = self.GetSubAttrs({}, self._attrCareerType)'.format(summ.id))
            getsets.append('            for subAttrPair in subAttrPairs:')
            getsets.append('                self._SetValue(subAttrPair[0], subAttrPair[1] * value, nty)')
            getsets.append('')
            getsets.append('    def Set{}(self, value):'.format(summ.utf8_enum))
            getsets.append('        """设置{}"""'.format(summ.utf8_comments))
            getsets.append('        self._Set{}(value, True)'.format(summ.utf8_enum))
            getsets.append('')

            copy_stmts.append('        cp._{}._val = self._{}._val'.format(summ.lower_case_enum, summ.lower_case_enum))
            resets.append('        self._{}.SetArg({})'.format(summ.lower_case_enum, summ.converted_dft_val))
        strAttrNames = ''
        for attrId, name in attrNames.iteritems():
            if not strAttrNames:
                strAttrNames += (str(attrId) + ':"' + name + '"')
            else:
                strAttrNames += (' ,' + str(attrId) + ':"' + name + '"')
        strAttrNamesElement = '    _attrNames = {' + strAttrNames + '}'
        w('')
        w(strAttrNamesElement)
        w('')
        w('')
        w('    @staticmethod')
        w('    def GetAttrNameById(attrId):')
        w('        return AttrBase._attrNames[attrId] if attrId in AttrBase._attrNames else None')
        w('')
        # GetAttrCareerType()/SetAttrCareerType()
        w('    def GetAttrCareerType(self):')
        w('        """取得属性职业类型"""')
        w('        return self._attrCareerType')
        w('')
        w('    def SetAttrCareerType(self, careerType):')
        w('        """设置属性职业类型"""')
        w('        self._attrCareerType = careerType')
        w('')

        # IsAutoExpandSubAttrs()/SetAutoExpandSubAttrs()
        w('    def IsAutoExpandSubAttrs(self):')
        w('        """确认是否已经开启属性自动展"""')
        w('        return self._autoExpandSubAttrs')
        w('')
        w('    def SetAutoExpandSubAttrs(self, flag):')
        w('        """设置属性自动扩展"""')
        w('        self._autoExpandSubAttrs = flag')
        w('')

        for getset in getsets:
            w(getset)

        # GetValue()
        w('    def GetValue(self, attrType):')
        w('        """根据类型取得值"""')
        w('        return eval("self._Get{}()".format(self.GetAttrNameById(attrType)))')
        w('')

        # SetValue()
        w('    def _SetValue(self, attrType, attrVal, nty):')
        w('        return eval("self._Set{}(attrVal, nty)".format(self.GetAttrNameById(attrType)))')
        w('')
        w('    def SetValue(self, attrType, attrVal):')
        w('        """根据类型设置值"""')
        w('        self._SetValue(attrType, attrVal, True)')
        w('')

        # AddValue()
        w('    def _AddValue(self, attrType, attrVal, nty, syncHp=False):')
        w('        return eval("self._Add{}(attrVal, nty)".format(self.GetAttrNameById(attrType)))')
        w('')
        # w('        bakHpLmt = 0')
        # w('        if syncHp and self._isHpLmtType(attrType):')
        # w('            bakHpLmt = self.hpLmt')
        # ================== 2016年6月20日11:54:27 Wizard注释 ==================
        # for add_stmt in adds:
        #     w(add_stmt)
        # ================== 2016年6月20日11:54:27 Wizard注释 ==================
        # w('        else:')
        # w("            Log.e('[AttrBase] Could not add type[{}] attribute value'.format(attrType))")
        # w('        if syncHp and self._isHpLmtType(attrType):')
        # w('            addVal = self.hpLmt - bakHpLmt')
        # w('            if addVal > 0:')
        # w('                self._AddHp(addVal, nty=nty)')
        # w('            elif addVal < 0:')
        # w('                if self.hp > self.hpLmt:')
        # w('                    self._SetHp(self.hpLmt, nty=nty)')
        # w('')

        w('    def AddValue(self, attrType, attrVal, syncHp=False, nty=True, srcType=0, subType=0, templateId=0):')
        w('        """根据类型增加值"""')
        w('        self._AddValue(attrType, attrVal, nty=nty, syncHp=syncHp)')
        w('')

        w('    @staticmethod')
        w('    def _isHpLmtType(attrType):')
        w('        return attrType == _AttrType.HpLmt or attrType == _AttrType.HpMulAmend')
        w('')

        # RemoveValue()
        w('    def _RemoveValue(self, attrType, attrVal, nty):')
        # ================== 2016年6月20日11:54:27 Wizard注释 ==================
        # for remove_stmt in removes:
        #     w(remove_stmt)
        # ================== 2016年6月20日11:54:27 Wizard注释 ==================
        w('        return eval("self._Remove{}(attrVal, nty)".format(self.GetAttrNameById(attrType)))')
        w('')
        w('    def RemoveValue(self, attrType, attrVal, nty=True, srcType=0, subType=0):')
        w('        """根据类型减少值"""')
        w('        self._RemoveValue(attrType, attrVal, nty)')
        w('')

        # SwapValue()
        w('    def SwapValue(self, attrType, oldAttrVal, newAttrVal, srcType=0, subType=0):')
        w('        """根据类型交换值"""')
        for swap_stmt in swaps:
            w(swap_stmt)
        w('        else:')
        w("            Log.e('[AttrBase] Could not swap type[{}] attribute values'.format(attrType))")
        w('')

        # AddValuesByTemplateId()
        w('    def AddValuesByTemplateId(self, templId, syncHp=False, nty=True, srcType=0, subType=0):')
        w('        """根据配置模板ID增加属性值"""')
        w('        attrs = AttrsCfg().GetOneById(templId)')
        w('        if attrs is None:')
        w('            return\n')
        w('        for idx, attrId in enumerate(attrs.attrs):')
        w('            self.AddValue(attrId, attrs.values[idx], syncHp=syncHp, nty=nty, srcType=srcType, subType=subType, templateId=templId)')
        w('')

        # RemoveValuesByTemplateId()
        w('    def RemoveValuesByTemplateId(self, templId, nty=True, srcType=0, subType=0):')
        w('        """根据配置模板ID减少属性值"""')
        w('        attrs = AttrsCfg().GetOneById(templId)')
        w('        if attrs is None:')
        w('            return\n')
        w('        for idx, attrId in enumerate(attrs.attrs):')
        w('            self.RemoveValue(attrId, attrs.values[idx], nty, srcType, subType)')
        w('')

        # SwapValuesByTemplateId()
        w('    def SwapValuesByTemplateId(self, oldTemplId, newTemplId, srcType=0, subType=0):')
        w('        """执行两个配置模板Id的属性交换"""')
        w('        oldAttrs = AttrsCfg().GetOneById(oldTemplId)')
        w('        oldAttrIds = set()')
        w('        oldAttrId2Values = {}')
        w('        if oldAttrs is not None:')
        w('            oldAttrIds.update(oldAttrs.attrs)')
        w('            oldAttrId2Values.update([(attrId, oldAttrs.values[attrIdx]) '
          'for attrIdx, attrId in enumerate(oldAttrs.attrs)])')
        w('')
        w('        newAttrs = AttrsCfg().GetOneById(newTemplId)')
        w('        newAttrIds = set()')
        w('        newAttrId2Values = {}')
        w('        if newAttrs is not None:')
        w('            newAttrIds.update(newAttrs.attrs)')
        w('            newAttrId2Values.update([(attrId, newAttrs.values[attrIdx]) '
          'for attrIdx, attrId in enumerate(newAttrs.attrs)])')
        w('')
        w('        for attrId in oldAttrIds.intersection(newAttrIds):')
        w('            self.SwapValue(attrId, oldAttrId2Values[attrId], newAttrId2Values[attrId], srcType=0, subType=0)')
        w('        for attrId in oldAttrIds.difference(newAttrIds):')
        w('            self.RemoveValue(attrId, oldAttrId2Values[attrId], nty=True, srcType=srcType, subType=subType)')
        w('        for attrId in newAttrIds.difference(oldAttrIds):')
        w('            self.AddValue(attrId, newAttrId2Values[attrId], nty=True, srcType=srcType, subType=subType, templateId=newTemplId)')
        w('')

        w('    def ResetAllAttrs(self):')
        for resetAttrItem in resets:
            w(resetAttrItem)
        w('')

        # GetElemAttrs()
        w('    def GetElemAttrs(self, elemType):')
        w('        """取得元素属性集合"""')
        w('        from com import ElemType')
        w('        attrs = ElemAttrs(elemType)')
        w('        if elemType == ElemType.Earth:')
        w('            attrs.elemLv = self.earthLv')
        w('            attrs.addiDmgScale = self.addiEarthDmgScale')
        w('            attrs.undAtkMulPosAmend = self.earthUndAtkMulPosAmend')
        w('            attrs.undAtkMulNegAmend = self.earthUndAtkMulNegAmend')
        w('            attrs.dmgMulPosAmend = self.earthDmgMulPosAmend')
        w('            attrs.addAmend = self.earthAddAmend')
        w('        elif elemType == ElemType.Fire:')
        w('            attrs.elemLv = self.fireLv')
        w('            attrs.addiDmgScale = self.addiFireDmgScale')
        w('            attrs.undAtkMulPosAmend = self.fireUndAtkMulPosAmend')
        w('            attrs.undAtkMulNegAmend = self.fireUndAtkMulNegAmend')
        w('            attrs.dmgMulPosAmend = self.fireDmgMulPosAmend')
        w('            attrs.addAmend = self.fireAddAmend')
        w('        elif elemType == ElemType.Water:')
        w('            attrs.elemLv = self.waterLv')
        w('            attrs.addiDmgScale = self.addiWaterDmgScale')
        w('            attrs.undAtkMulPosAmend = self.waterUndAtkMulPosAmend')
        w('            attrs.undAtkMulNegAmend = self.waterUndAtkMulNegAmend')
        w('            attrs.dmgMulPosAmend = self.waterDmgMulPosAmend')
        w('            attrs.addAmend = self.waterAddAmend')
        w('        elif elemType == ElemType.Thunder:')
        w('            attrs.elemLv = self.thunderLv')
        w('            attrs.addiDmgScale = self.addiThunderDmgScale')
        w('            attrs.undAtkMulPosAmend = self.thunderUndAtkMulPosAmend')
        w('            attrs.undAtkMulNegAmend = self.thunderUndAtkMulNegAmend')
        w('            attrs.dmgMulPosAmend = self.thunderDmgMulPosAmend')
        w('            attrs.addAmend = self.thunderAddAmend')
        w('        return attrs')
        w('')

        # SetElemAttrs()
        w('    def SetElemAttrs(self, elemAttrs):')
        w('        from com import ElemType')
        w('        elemType = elemAttrs.elemType')
        w('        if elemType == ElemType.Earth:')
        w('            self.earthLv = elemAttrs.elemLv')
        w('            self.addiEarthDmgScale = elemAttrs.addiDmgScale')
        w('            self.earthUndAtkMulPosAmend = elemAttrs.undAtkMulPosAmend')
        w('            self.earthUndAtkMulNegAmend = elemAttrs.undAtkMulNegAmend')
        w('            self.earthDmgMulPosAmend = elemAttrs.dmgMulPosAmend')
        w('            self.earthAddAmend = elemAttrs.addAmend')
        w('        elif elemType == ElemType.Fire:')
        w('            self.fireLv = elemAttrs.elemLv')
        w('            self.addiFireDmgScale = elemAttrs.addiDmgScale')
        w('            self.fireUndAtkMulPosAmend = elemAttrs.undAtkMulPosAmend')
        w('            self.fireUndAtkMulNegAmend = elemAttrs.undAtkMulNegAmend')
        w('            self.fireDmgMulPosAmend = elemAttrs.dmgMulPosAmend')
        w('        elif elemType == ElemType.Water:')
        w('            self.waterLv = elemAttrs.elemLv')
        w('            self.addiWaterDmgScale = elemAttrs.addiDmgScale')
        w('            self.waterUndAtkMulPosAmend = elemAttrs.undAtkMulPosAmend')
        w('            self.waterUndAtkMulNegAmend = elemAttrs.undAtkMulNegAmend')
        w('            self.waterDmgMulPosAmend = elemAttrs.dmgMulPosAmend')
        w('        elif elemType == ElemType.Thunder:')
        w('            self.thunderLv = elemAttrs.elemLv')
        w('            self.addiThunderDmgScale = elemAttrs.addiDmgScale')
        w('            self.thunderUndAtkMulPosAmend = elemAttrs.undAtkMulPosAmend')
        w('            self.thunderUndAtkMulNegAmend = elemAttrs.undAtkMulNegAmend')
        w('            self.thunderDmgMulPosAmend = elemAttrs.dmgMulPosAmend')
        w('')

        # GetBuffCtrlAttrs()
        w('    def GetBuffCtrlAttrs(self, elemType):')
        w('        """根据元素类型,取得Buff控制相关属性集合"""')
        w('        from com import ElemType')
        w('        attrs = BuffCtrlAttrs(elemType)')
        w('        if elemType == ElemType.Earth:')
        w('            attrs.addProbAddAmend = self.earthBuffAddProbAddAmend')
        w('            attrs.addProbMulPosAmend = self.earthBuffAddProbMulPosAmend')
        w('            attrs.addProbMulNegAmend = self.earthBuffAddProbMulNegAmend')
        w('            attrs.durTimeAddAmend = self.earthBuffDurTimeAddAmend')
        w('            attrs.durTimeMulPosAmend = self.earthBuffDurTimeMulPosAmend')
        w('            attrs.durTimeMulNegAmend = self.earthBuffDurTimeMulNegAmend')
        w('        elif elemType == ElemType.Fire:')
        w('            attrs.addProbAddAmend = self.fireBuffAddProbAddAmend')
        w('            attrs.addProbMulPosAmend = self.fireBuffAddProbMulPosAmend')
        w('            attrs.addProbMulNegAmend = self.fireBuffAddProbMulNegAmend')
        w('            attrs.durTimeAddAmend = self.fireBuffDurTimeAddAmend')
        w('            attrs.durTimeMulPosAmend = self.fireBuffDurTimeMulPosAmend')
        w('            attrs.durTimeMulNegAmend = self.earthBuffDurTimeMulNegAmend')
        w('        elif elemType == ElemType.Water:')
        w('            attrs.addProbAddAmend = self.waterBuffAddProbAddAmend')
        w('            attrs.addProbMulPosAmend = self.waterBuffAddProbMulPosAmend')
        w('            attrs.addProbMulNegAmend = self.waterBuffAddProbMulNegAmend')
        w('            attrs.durTimeAddAmend = self.waterBuffDurTimeAddAmend')
        w('            attrs.durTimeMulPosAmend = self.waterBuffDurTimeMulPosAmend')
        w('            attrs.durTimeMulNegAmend = self.earthBuffDurTimeMulNegAmend')
        w('        elif elemType == ElemType.Thunder:')
        w('            attrs.addProbAddAmend = self.thunderBuffAddProbAddAmend')
        w('            attrs.addProbMulPosAmend = self.thunderBuffAddProbMulPosAmend')
        w('            attrs.addProbMulNegAmend = self.thunderBuffAddProbMulNegAmend')
        w('            attrs.durTimeAddAmend = self.thunderBuffDurTimeAddAmend')
        w('            attrs.durTimeMulPosAmend = self.thunderBuffDurTimeMulPosAmend')
        w('            attrs.durTimeMulNegAmend = self.earthBuffDurTimeMulNegAmend')
        w('        return attrs')
        w('')

        # SetBuffCtrlAttrs()
        w('    def SetBuffCtrlAttrs(self, attrs):')
        w('        """将指定元素类型的Buff控制属性集合设置到BaseAttr中"""')
        w('        from com import ElemType')
        w('        elemType = attrs.elemType')
        w('        if elemType == ElemType.Earth:')
        w('            self.earthBuffAddProbAddAmend = attrs.addProbAddAmend')
        w('            self.earthBuffAddProbMulPosAmend = attrs.addProbMulPosAmend')
        w('            self.earthBuffAddProbMulNegAmend = attrs.addProbMulNegAmend')
        w('            self.earthBuffDurTimeAddAmend = attrs.durTimeAddAmend')
        w('            self.earthBuffDurTimeMulPosAmend = attrs.durTimeMulPosAmend')
        w('            self.earthBuffDurTimeMulNegAmend = attrs.durTimeMulNegAmend')
        w('        elif elemType == ElemType.Fire:')
        w('            self.fireBuffAddProbAddAmend = attrs.addProbAddAmend')
        w('            self.fireBuffAddProbMulPosAmend = attrs.addProbMulPosAmend')
        w('            self.fireBuffAddProbMulNegAmend = attrs.addProbMulNegAmend')
        w('            self.fireBuffDurTimeAddAmend = attrs.durTimeAddAmend')
        w('            self.fireBuffDurTimeMulPosAmend = attrs.durTimeMulPosAmend')
        w('            self.fireBuffDurTimeMulNegAmend = attrs.durTimeMulNegAmend')
        w('        elif elemType == ElemType.Water:')
        w('            self.waterBuffAddProbAddAmend = attrs.addProbAddAmend')
        w('            self.waterBuffAddProbMulPosAmend = attrs.addProbMulPosAmend')
        w('            self.waterBuffAddProbMulNegAmend = attrs.addProbMulNegAmend')
        w('            self.waterBuffDurTimeAddAmend = attrs.durTimeAddAmend')
        w('            self.waterBuffDurTimeMulPosAmend = attrs.durTimeMulPosAmend')
        w('            self.waterBuffDurTimeMulNegAmend = attrs.durTimeMulNegAmend')
        w('        elif elemType == ElemType.Thunder:')
        w('            self.thunderBuffAddProbAddAmend = attrs.addProbAddAmend')
        w('            self.thunderBuffAddProbMulPosAmend = attrs.addProbMulPosAmend')
        w('            self.thunderBuffAddProbMulNegAmend = attrs.addProbMulNegAmend')
        w('            self.thunderBuffDurTimeAddAmend = attrs.durTimeAddAmend')
        w('            self.thunderBuffDurTimeMulPosAmend = attrs.durTimeMulPosAmend')
        w('            self.thunderBuffDurTimeMulNegAmend = attrs.durTimeMulNegAmend')
        w('')

        # Copy method
        w('    def CopyAllAttrs(self):')
        w('        cp = {}()'.format(cls_name))
        for copy_stmt in copy_stmts:
            w(copy_stmt)
        w('        return cp')
        w('')

        # Update method
        w('    def {}(self, attrType, oldVal, newVal):'.format(updated_meth_name))
        w('        """实现属性更新后的操作"""')
        w('        pass')
        w('')

        # Reset method
        w('    def ResetAll(self):')
        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            if summ.id in priorityId.keys():
                continue
            name = summ.lower_case_enum
            w('        self._{} = CfgFormulaBuilder.Build({}, {}, {})  # {}, Attribute Id: {}'
              .format(name, summ.formula, summ.utf8_id_type, summ.converted_dft_val, summ.utf8_comments, summ.id))
        w('')


class CSharpEnumExporter(object):
    """CSharp枚举导出器"""

    def __init__(self, export_path):
        self._export_file = _op.join(export_path, u'AttrTypes.cs')

    def export(self, summaries):
        f = _F(self._export_file)

        w = f.w
        w('/**')
        w(' * AutoGen by tools')
        w(' */')
        w('')

        w('namespace {}'.format(NS))
        w('{')

        w('/// <summary>')
        w('/// 游戏属性枚举')
        w('/// </summary>')
        w('public enum AttrType')
        w('{')

        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            w('    /// <summary>')
            w('    /// {}'.format(summ.utf8_comments))
            w('    /// </summary>')
            w('    {} = {},'.format(summ.utf8_enum, summ.id))
            w('')

        w('}')
        w('}')


class CSharpAttrBaseExporter(object):
    """C#属性类型代码导出器"""

    def __init__(self, export_path):
        self._export_path = export_path
        # self._export_file = _op.join(export_path, 'BaseAttrs.cs')

    def export(self, summaries):
        # Copy code-template file to export path
        # templ_file = 'CfgFormula.cs'
        # FileUtil.copy(_op.join(CODE_TEMPL_PATH, templ_file), _op.join(self._export_path, templ_file))
        templ_file = 'BuffCtrlAttrs.cs'
        FileUtil.copy(_op.join(CODE_TEMPL_PATH, templ_file), _op.join(self._export_path, templ_file))

        # Generate BaseAttrs code file.
        f = _F(self._export_file)
        w = f.w

        w('/**')
        w(' * AutoGen by tools')
        w(' */')

        w('using System;')
        w('using System.Collections.Generic;')
        w('using UnityEngine;')
        w('using {};'.format(NS))
        w('')

        gen_summ = lambda the_summ: w('    /// <summary>\n    /// {}\n    /// </summary>'.format(the_summ))

        # 生成类头
        cls_name = 'BaseAttrs'
        w('public partial class {}'.format(cls_name))
        w('{')

        # 子数据相关静态数据,方法生成
        pair_cls = 'KeyValuePair<{}.AttrType, float>'.format(NS)
        pairs_cls = 'List<{}>'.format(pair_cls)
        career_pairs_cls = 'Dictionary<int, {}>'.format(pairs_cls)
        sub_attrs_cls = 'Dictionary<{}.AttrType, {}>'.format(NS, career_pairs_cls)
        w('    private static readonly {} subAttrs = new {}();'.format(sub_attrs_cls, sub_attrs_cls))
        w('    private static readonly {} emptyAttrPairs = new {}();'.format(pairs_cls, pairs_cls))
        w('    private static bool constructedSubAttrs = false;')
        w('')
        w('    private static void ConstructSubAttrs()')
        w('    {')
        w('        if (constructedSubAttrs)')
        w('            return;')
        w('')

        for summ in summaries:
            if not summ.sub_attrs:
                continue

            w('        // 构造 {} 子属性'.format(summ.utf8_comments))
            w('        {} {}CareerPairs = new {}();'.format(career_pairs_cls, summ.lower_case_enum, career_pairs_cls))
            for career_type, pairs in summ.sub_attrs.iteritems():
                w('        {} {}Pairs_{} = new {}();'.format(pairs_cls, summ.lower_case_enum, career_type, pairs_cls))
                for pair in pairs:
                    w('        {}Pairs_{}.Add(new {}(({}.AttrType)({}), {}f));'
                      .format(summ.lower_case_enum, career_type, pair_cls, NS, pair[0], pair[1]))
                w('        {}CareerPairs.Add({}, {}Pairs_{});'
                  .format(summ.lower_case_enum, career_type, summ.lower_case_enum, career_type))
            w('        subAttrs.Add(({}.AttrType)({}), {}CareerPairs);'.format(NS, summ.id, summ.lower_case_enum))
        w('')
        w('        constructedSubAttrs = true;')
        w('    }')
        w('')

        # 生成取得子属性方法
        gen_summ('根据属性类型,职业取得子属性列表')
        w('    public static {} GetSubAttrs({}.AttrType attrType, int career)'.format(pairs_cls, NS))
        w('    {')
        w('        {} careerPairs = null;'.format(career_pairs_cls))
        w('        if (!subAttrs.TryGetValue(attrType, out careerPairs))')
        w('            return emptyAttrPairs;')
        w('')
        w('        {} pairs = null;'.format(pairs_cls))
        w('        if (!careerPairs.TryGetValue(career, out pairs))')
        w('            return emptyAttrPairs;')
        w('        else')
        w('            return pairs;')
        w('    }')
        w('')

        # 生成默认构造函数
        gen_summ('默认构造,初始化子属性字典')
        w('    public {}()'.format(cls_name))
        w('    {')
        w('        ConstructSubAttrs();')
        w('    }')
        w('')

        # 自动扩充子属性逻辑支持
        w('    private bool autoExpandSubAttrs = false;')
        w('')
        gen_summ('是否开启子属性自动扩充')
        w('    public bool AutoExpandSubAttrs '
          '{ get { return autoExpandSubAttrs; } set { autoExpandSubAttrs = value; } }')
        w('')

        # 设置职业
        w('    private int attrCareer = 0;')
        w('')
        gen_summ('设置属性职业')
        w('    public int AttrCareer { get { return attrCareer; } set { attrCareer = value; } }')
        w('')

        assign_stmts = []
        get_case_stmts = []
        set_case_stmts = []
        add_case_stmts = []
        remove_case_stmts = []
        copy_stmts = []
        lang_stmts = []
        display_stmts = []
        resets = []
        for summ in summaries:
            assert isinstance(summ, AttrSummary)
            gen_summ('{}, Attribute Id:{}'.format(summ.utf8_comments, summ.id))
            w('    public virtual {} {}'.format(summ.utf8_id_type, summ.utf8_enum))
            w('    {')
            w('        get {{ return {}.Val; }}'.format(summ.lower_case_enum))
            w('        set')
            w('        {')
            w('            {} oldVal = {}.Val;'.format(summ.utf8_id_type, summ.lower_case_enum))
            w('            {}.Val = value;'.format(summ.lower_case_enum))
            w('            if ({}.Val != oldVal)'.format(summ.lower_case_enum))
            w('                this.OnAttrChanged({}.AttrType.{}, (float)oldVal, (float){}.Val);'
              .format(NS, summ.utf8_enum, summ.lower_case_enum))
            w('            if (autoExpandSubAttrs)')
            w('            {')
            w('                {} attrPairs = GetSubAttrs(({}.AttrType)({}), attrCareer);'
              .format(pairs_cls, NS, summ.id))
            # w('                foreach (var pair in attrPairs)')
            # w('                    this.SetValue(({}.AttrType)(pair.Key), pair.Value * value);'.format(NS))
            w('                for (int i = 0; i < attrPairs.Count; i ++)')
            w('                    this.SetValue(({}.AttrType)(attrPairs[i].Key), attrPairs[i].Value * value);'.format(NS))
            w('            }')
            w('        }')
            w('    }')
            w('')
            gen_summ("为 '{}' 增加一个值".format(summ.utf8_comments))
            w('    public virtual void Add{}({} val)'.format(summ.utf8_enum, summ.utf8_id_type))
            w('    {')
            w('        if (val != 0)')
            w('        {')
            w('            {} oldVal = {}.Val;'.format(summ.utf8_id_type, summ.lower_case_enum))
            w('            {}.AddArg(val);'.format(summ.lower_case_enum))
            w('            if ({}.Val != oldVal)'.format(summ.lower_case_enum))
            w('                this.OnAttrChanged({}.AttrType.{}, (float)oldVal, (float){}.Val);'
              .format(NS, summ.utf8_enum, summ.lower_case_enum))
            w('            if (autoExpandSubAttrs)')
            w('            {')
            w('                {} attrPairs = GetSubAttrs(({}.AttrType)({}), attrCareer);'
              .format(pairs_cls, NS, summ.id))
            # w('                foreach (var pair in attrPairs)')
            # w('                    this.AddValue(({}.AttrType)(pair.Key), pair.Value * val);'.format(NS))
            w('                for (int i = 0; i < attrPairs.Count; i ++)')
            w('                    this.AddValue(({}.AttrType)(attrPairs[i].Key), attrPairs[i].Value * val);'.format(NS))
            w('            }')
            w('        }')
            w('    }')
            w('')
            gen_summ("为 '{}' 减少一个值".format(summ.utf8_comments))
            w('    public virtual void Remove{}({} val)'.format(summ.utf8_enum, summ.utf8_id_type))
            w('    {')
            w('        if (val != 0)')
            w('        {')
            w('            {} oldVal = {}.Val;'.format(summ.utf8_id_type, summ.lower_case_enum))
            w('            {}.RemoveArg(val);'.format(summ.lower_case_enum))
            w('            if ({}.Val != oldVal)'.format(summ.lower_case_enum))
            w('               this.OnAttrChanged({}.AttrType.{}, (float)oldVal, (float){}.Val);'
              .format(NS, summ.utf8_enum, summ.lower_case_enum))
            w('            if (autoExpandSubAttrs)')
            w('            {')
            w('                {} attrPairs = GetSubAttrs(({}.AttrType)({}), attrCareer);'
              .format(pairs_cls, NS, summ.id))
            # w('                foreach (var pair in attrPairs)')
            # w('                    this.RemoveValue(({}.AttrType)(pair.Key), pair.Value * val);'.format(NS))
            w('                for (int i = 0; i < attrPairs.Count; i ++)')
            w('                    this.RemoveValue(({}.AttrType)(attrPairs[i].Key), attrPairs[i].Value * val);'.format(NS))
            w('            }')
            w('        }')
            w('    }')
            w('')
            dft_val = '({}){}'.format(summ.utf8_id_type, summ.converted_dft_val)
            base_cls_name = 'CfgFormula<{}>'.format(summ.utf8_id_type)
            bdr_cls_name = 'CfgFormulaBuilder<{}>'.format(summ.utf8_id_type)
            formula = int(summ.formula)
            if formula == 0:
                formula_type = 'CfgFormulaType.Accum'
            elif formula == 1:
                formula_type = 'CfgFormulaType.NegAmend'
            else:
                formula_type = 'CfgFormulaType.Multiply'
            w('    private {} {} = {}.Build({}, {});'.format(
                base_cls_name, summ.lower_case_enum, bdr_cls_name, formula_type, dft_val))
            w('')

            assign_stmts.append('        {} = another.{};'.format(summ.utf8_enum, summ.utf8_enum))

            get_case_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            get_case_stmts.append('            return this.{}.Val;'.format(summ.lower_case_enum))

            set_case_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            add_case_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            remove_case_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            if summ.id_type != u'float':
                set_case_stmts.append('            this.{} = ({})value;'
                                      .format(summ.utf8_enum, summ.utf8_id_type))
                add_case_stmts.append('            this.Add{}(({})addVal);'
                                      .format(summ.utf8_enum, summ.utf8_id_type))
                remove_case_stmts.append('            this.Remove{}(({})removeVal);'
                                         .format(summ.utf8_enum, summ.utf8_id_type))
            else:
                set_case_stmts.append('            this.{} = value;'.format(summ.utf8_enum))
                add_case_stmts.append('            this.Add{}(addVal);'.format(summ.utf8_enum))
                remove_case_stmts.append('            this.Remove{}(removeVal);'.format(summ.utf8_enum))
            set_case_stmts.append('            break;')
            add_case_stmts.append('            break;')
            remove_case_stmts.append('            break;')

            lang_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            lang_stmts.append('            langId = {};'.format(summ.lang_id))
            lang_stmts.append('            break;')

            display_stmts.append('        case {}.AttrType.{}:'.format(NS, summ.utf8_enum))
            display_stmts.append('            return {};'.format(summ.display))

            copy_stmts.append('        copied.{} = this.{}; // {}'
                              .format(summ.utf8_enum, summ.utf8_enum, summ.utf8_comments))
            resets.append('        {}.Val = {};'.format(summ.lower_case_enum, dft_val))

        # AssignAllAttrs()
        gen_summ("赋值")
        w('    public virtual void AssignAllAttrs({} another)'.format(cls_name))
        w('    {')
        for assign_stmt in assign_stmts:
            w(assign_stmt)
        w('    }')
        w('')

        # GetValue()
        w('    /// <summary>')
        w('    /// 根据属性类型返回对应属性值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    public virtual float GetValue({}.AttrType attrType)'.format(NS))
        w('    {')
        w('        switch (attrType)')
        w('        {')
        for stmt in get_case_stmts:
            w(stmt)
        w('        default:')
        w('            Debug.LogError("未找到属性值:" + attrType);')
        w('            return 0.0f;')
        w('        }')
        w('    }')

        # SetValue
        w('    /// <summary>')
        w('    /// 根据属性类型设置对应属性值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    /// <param name="value">属性值</param>')
        w('    public virtual void SetValue({}.AttrType attrType, float value)'.format(NS))
        w('    {')
        w('        switch (attrType)')
        w('        {')
        for stmt in set_case_stmts:
            w(stmt)
        w('        default:')
        w('            Debug.LogError("Could not found attrType:" + attrType + ", set value failed");')
        w('            break;')
        w('        }')
        w('    }')

        # AddValue
        w('    /// <summary>')
        w('    /// 根据属性类型为对应属性添加值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    /// <param name="addVal">要增加的属性值</param>')
        w('    public virtual void AddValue({}.AttrType attrType, float addVal)'.format(NS))
        w('    {')
        w('        switch (attrType)')
        w('        {')
        for stmt in add_case_stmts:
            w(stmt)
        w('        default:')
        w('            Debug.LogError("Could not fould attrType:" + attrType + ", add value failed");')
        w('            break;')
        w('        }')
        w('    }')
        w('')

        # RemoveValue
        w('    /// <summary>')
        w('    /// 根据属性类型为对应属性移除值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    /// <param name="removeVal">要移除的属性值</param>')
        w('    public virtual void RemoveValue({}.AttrType attrType, float removeVal)'.format(NS))
        w('    {')
        w('        switch (attrType)')
        w('        {')
        for stmt in remove_case_stmts:
            w(stmt)
        w('        default:')
        w('            Debug.LogError("Could not found attrType:" + attrType + ", remove value failed");')
        w('            break;')
        w('        }')
        w('    }')
        w('')

        # SwapValue
        w('    /// <summary>')
        w('    /// 交换属性值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    /// <param name="oldVal">老值</param>')
        w('    /// <param name="newVal">新值</param>')
        w('    public virtual void SwapValue({}.AttrType attrType, float oldVal, float newVal)'.format(NS))
        w('    {')
        w('        this.RemoveValue(attrType, oldVal);')
        w('        this.AddValue(attrType, newVal);')
        w('    }')
        w('')

        # AddValuesByTemplateId()
        w('    /// <summary>')
        w('    /// 根据属性模板Id增加属性值')
        w('    /// </summary>')
        w('    /// <param name="templId">属性模板Id</param>')
        w('    public virtual void AddValuesByTemplateId(int templId)')
        w('    {')
        w('        {}.Attrs attrCfg = DataReader<{}.Attrs>.Get(templId);'.format(NS, NS))
        w('        if (attrCfg == null)')
        w('        {')
        w('            Debug.LogError(@"Could not found attr template config, templId: " + templId);')
        w('            return;')
        w('        }')
        w('')
        w('        for (int i = 0; i < attrCfg.attrs.Count; i++)')
        w('            this.AddValue(({}.AttrType)(attrCfg.attrs[i]), (float)(attrCfg.values[i]));'.format(NS))
        w('    }')
        w('')

        # RemoveValuesByTemplateId()
        w('    /// <summary>')
        w('    /// 根据属性模板Id移除属性值')
        w('    /// </summary>')
        w('    /// <param name="templId">属性模板Id</param>')
        w('    public virtual void RemoveValuesByTemplateId(int templId)')
        w('    {')
        w('        {}.Attrs attrCfg = DataReader<{}.Attrs>.Get(templId);'.format(NS, NS))
        w('        if (attrCfg == null)')
        w('        {')
        w('            Debug.LogError(@"Could not found attr template config, templId: " + templId);')
        w('            return;')
        w('        }')
        w('')
        w('        for (int i = 0; i < attrCfg.attrs.Count; i++)')
        w('            this.RemoveValue(({}.AttrType)(attrCfg.attrs[i]), (float)(attrCfg.values[i]));'.format(NS))
        w('    }')
        w('')

        # ResetAllAttrs()
        w('    /// <summary>')
        w('    /// 重置所有属性')
        w('    /// </summary>')
        w('    public virtual void ResetAllAttrs()')
        w('    {')
        for resetAttrItem in resets:
            w(resetAttrItem)
        w('    }')
        w('')

        # CopyAllAttrs()
        w('    /// <summary>')
        w('    /// 拷贝所有属性')
        w('    /// </summary>')
        w('    public virtual {} CopyAllAttrs()'.format(cls_name))
        w('    {')
        w('        {} copied = new {}();'.format(cls_name, cls_name))
        for copy_stmt in copy_stmts:
            w(copy_stmt)
        w('')
        w('        return copied;')
        w('    }')

        # GetBuffCtrlAttrs()
        gen_summ('取得指定元素类型的buff控制属性集合')
        w('    public BuffCtrlAttrs GetBuffCtrlAttrs(int elemType)')
        w('    {')
        w('        BuffCtrlAttrs attrs = new BuffCtrlAttrs(elemType);')
        w('        if (elemType == 1)')
        w('        {')
        w('            attrs.AddProbAddAmend = EarthBuffAddProbAddAmend;')
        w('            attrs.AddProbMulPosAmend = EarthBuffAddProbMulPosAmend;')
        w('            attrs.AddProbMulNegAmend = EarthBuffAddProbMulNegAmend;')
        w('            attrs.DurTimeAddAmend = EarthBuffDurTimeAddAmend;')
        w('            attrs.DurTimeMulPosAmend = EarthBuffDurTimeMulPosAmend;')
        w('            attrs.DurTimeMulNegAmend = EarthBuffDurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 2)')
        w('        {')
        w('            attrs.AddProbAddAmend = FireBuffAddProbAddAmend;')
        w('            attrs.AddProbMulPosAmend = FireBuffAddProbMulPosAmend;')
        w('            attrs.AddProbMulNegAmend = FireBuffAddProbMulNegAmend;')
        w('            attrs.DurTimeAddAmend = FireBuffDurTimeAddAmend;')
        w('            attrs.DurTimeMulPosAmend = FireBuffDurTimeMulPosAmend;')
        w('            attrs.DurTimeMulNegAmend = FireBuffDurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 3)')
        w('        {')
        w('            attrs.AddProbAddAmend = WaterBuffAddProbAddAmend;')
        w('            attrs.AddProbMulPosAmend = WaterBuffAddProbMulPosAmend;')
        w('            attrs.AddProbMulNegAmend = WaterBuffAddProbMulNegAmend;')
        w('            attrs.DurTimeAddAmend = WaterBuffDurTimeAddAmend;')
        w('            attrs.DurTimeMulPosAmend = WaterBuffDurTimeMulPosAmend;')
        w('            attrs.DurTimeMulNegAmend = WaterBuffDurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 4)')
        w('        {')
        w('            attrs.AddProbAddAmend = ThunderBuffAddProbAddAmend;')
        w('            attrs.AddProbMulPosAmend = ThunderBuffAddProbMulPosAmend;')
        w('            attrs.AddProbMulNegAmend = ThunderBuffAddProbMulNegAmend;')
        w('            attrs.DurTimeAddAmend = ThunderBuffDurTimeAddAmend;')
        w('            attrs.DurTimeMulPosAmend = ThunderBuffDurTimeMulPosAmend;')
        w('            attrs.DurTimeMulNegAmend = ThunderBuffDurTimeMulNegAmend;')
        w('        }')
        w('        return attrs;')
        w('    }')
        w('')

        # SetBuffCtrlAttrs()
        gen_summ('设置指定元素类型的buff控制属性集合')
        w('    public void SetBuffCtrlAttrs(BuffCtrlAttrs attrs)')
        w('    {')
        w('        int elemType = attrs.ElemType;')
        w('        if (elemType == 1)')
        w('        {')
        w('            EarthBuffAddProbAddAmend = attrs.AddProbAddAmend;')
        w('            EarthBuffAddProbMulPosAmend = attrs.AddProbMulPosAmend;')
        w('            EarthBuffAddProbMulNegAmend = attrs.AddProbMulNegAmend;')
        w('            EarthBuffDurTimeAddAmend = attrs.DurTimeAddAmend;')
        w('            EarthBuffDurTimeMulPosAmend = attrs.DurTimeMulPosAmend;')
        w('            EarthBuffDurTimeMulNegAmend = attrs.DurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 2)')
        w('        {')
        w('            FireBuffAddProbAddAmend = attrs.AddProbAddAmend;')
        w('            FireBuffAddProbMulPosAmend = attrs.AddProbMulPosAmend;')
        w('            FireBuffAddProbMulNegAmend = attrs.AddProbMulNegAmend;')
        w('            FireBuffDurTimeAddAmend = attrs.DurTimeAddAmend;')
        w('            FireBuffDurTimeMulPosAmend = attrs.DurTimeMulPosAmend;')
        w('            FireBuffDurTimeMulNegAmend = attrs.DurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 3)')
        w('        {')
        w('            WaterBuffAddProbAddAmend = attrs.AddProbAddAmend;')
        w('            WaterBuffAddProbMulPosAmend = attrs.AddProbMulPosAmend;')
        w('            WaterBuffAddProbMulNegAmend = attrs.AddProbMulNegAmend;')
        w('            WaterBuffDurTimeAddAmend = attrs.DurTimeAddAmend;')
        w('            WaterBuffDurTimeMulPosAmend = attrs.DurTimeMulPosAmend;')
        w('            WaterBuffDurTimeMulNegAmend = attrs.DurTimeMulNegAmend;')
        w('        }')
        w('        else if (elemType == 4)')
        w('        {')
        w('            ThunderBuffAddProbAddAmend = attrs.AddProbAddAmend;')
        w('            ThunderBuffAddProbMulPosAmend = attrs.AddProbMulPosAmend;')
        w('            ThunderBuffAddProbMulNegAmend = attrs.AddProbMulNegAmend;')
        w('            ThunderBuffDurTimeAddAmend = attrs.DurTimeAddAmend;')
        w('            ThunderBuffDurTimeMulPosAmend = attrs.DurTimeMulPosAmend;')
        w('            ThunderBuffDurTimeMulNegAmend = attrs.DurTimeMulNegAmend;')
        w('        }')
        w('    }')
        w('')

        # GetDesc()
        w('    /// <summary>')
        w('    /// 根据属性类型描述中文描述')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    public static string GetDesc({}.AttrType attrType)'.format(NS))
        w('    {')
        w('        int langId = 0;')
        w('        switch (attrType)')
        w('        {')
        for lang_stmt in lang_stmts:
            w(lang_stmt)
        w('        default:')
        w('            break;')
        w('        }')
        w('        {}.ChineseData langCfg = DataReader<{}.ChineseData>.Get(langId);'.format(NS, NS))
        w('        if (langCfg == null)')
        w('        {')
        w('            Debug.LogError(@"Could not found lang config, langId: " + langId + @", AttrType: " + attrType);')
        w('            return @"" + langId;')
        w('        }')
        w('')
        w('        return langCfg.content;')
        w('    }')

        # OnAttrChanged()
        w('    /// <summary>')
        w('    /// 属性变更事件函数,需要重写')
        w('    /// </summary>')
        w('    protected virtual void OnAttrChanged({}.AttrType attrType, float oldVal, float newVal)'.format(NS))
        w('    {')
        w('        // 重写以对属性变更进行后续处理')
        w('    }')

        w('')

        # GetDisplay
        w('    /// <summary>')
        w('    /// 根据属性类型描述获取Display值')
        w('    /// </summary>')
        w('    /// <param name="attrType">属性类型</param>')
        w('    public static int GetDisplay({}.AttrType attrType)'.format(NS))
        w('    {')
        w('        switch (attrType)')
        w('        {')
        for display_stmt in display_stmts:
            w(display_stmt)
        w('        default:')
        w('            return 0;')
        w('        }')
        w('    }')
        w('}')

if __name__ == '__main__':
    attr_file = _op.join(CFG_PATH, u's数值规划', u'A属性模版.xlsm')
    picker = AttrSummaryPicker(attr_file)
    picker.pick()

    summaries = picker.summaries
    # cs_enum_exporter = CSharpEnumExporter(CLIENT_PATH)
    # cs_enum_exporter.export(summaries)
    #
    # cs_ab_exporter = CSharpAttrBaseExporter(CLIENT_PATH)
    # cs_ab_exporter.export(summaries)

    pyenum_exporter = PyEnumExporter(SERVER_PATH)
    priorityId = pyenum_exporter.export(summaries)

    py_ab_exporter = PyAttrBaseExporter(SERVER_PATH)
    py_ab_exporter.export(summaries, priorityId)

    py_abi_exporter = PyAttrBaseInfoExporter(SERVER_PATH)
    py_abi_exporter.export(summaries)

    # 自动依据 系统开启表 生成 module_open.py'
    table = u'A系统开启表.xls'
    attr_file2 = _op.join(CFG_PATH, u'X新手引导', table)
    picker2 = AttrSummaryPicker(attr_file2)
    picker2.pickSystemOpen(table)

    py_mgr_enum_export = PyMgrEnumExporter(SERVER_PATH)
    py_mgr_enum_export.export(picker2.summaries)
