# -*- coding: utf-8 -*-
import json
from llbc import BitSet
from types import NoneType

import xlrd

from c import *
from spelling_converter import SpellingConverter


class FieldType(object):
    """字段类型(仅描述RAW type)"""
    Begin = 1
    Bool = Begin
    Int32 = 2
    UInt32 = 3
    Int64 = 4
    UInt64 = 5
    Float = 6
    Double = 7
    String = 8
    End = 9

    @classmethod
    def str_2_type(cls, s, raise_except=True):
        s = s.lower()
        if s == 'bool':
            return cls.Bool
        elif s == 'int32':
            return cls.Int32
        elif s == 'uint32':
            return cls.UInt32
        elif s == 'int64':
            return cls.Int64
        elif s == 'uint64':
            return cls.UInt64
        elif s == 'float':
            return cls.Float
        elif s == 'double':
            return cls.Double
        elif s == 'string' or s == 'bytes':
            return cls.String
        else:
            if raise_except:
                raise Exception('Unsupport field type <{}>'.format(s))
            else:
                return 0

    @classmethod
    def dft_val_str(cls, ty, raise_except=True):
        if ty == cls.Bool:
            return 'true'
        elif ty in (cls.Int32, cls.UInt32, cls.Int64, cls.UInt64):
            return '0'
        elif ty == cls.Float or ty == cls.Double:
            return '0.0'
        elif ty == cls.String:
            return '""'
        else:
            if raise_except:
                raise Exception('Unsupport field type <{}>'.format(ty))
            else:
                return ''

    @classmethod
    def is_valid_type(cls, ty):
        return True if cls.Begin <= ty < cls.End else False

    @classmethod
    def is_valid_type_str(cls, type_str):
        return True if cls.str_2_type(type_str, raise_except=False) != 0 else False


class FieldCodec(object):
    """字段编解码器"""

    @classmethod
    def decode(cls, constraints, def_type, raw_data):
        if isinstance(raw_data, unicode):
            raw_data = raw_data.encode('utf-8')

        if constraints.has_bits(FieldConstraint.Mapped):
            key_type, value_type = def_type.split(':')
            return cls.decode_map(raw_data, key_type, value_type)
        elif constraints.has_bits(FieldConstraint.Repeated):
            return cls.decode_arr(def_type, raw_data)
        else:
            ty_enum = FieldType.str_2_type(def_type)
            if ty_enum == FieldType.Bool:
                return cls.decode_bool(raw_data)
            elif ty_enum == FieldType.Int32 or ty_enum == FieldType.UInt32:
                return cls.decode_int(raw_data)
            elif ty_enum == FieldType.Int64 or ty_enum == FieldType.UInt64:
                return cls.decode_long(raw_data)
            elif ty_enum == FieldType.Float or ty_enum == FieldType.Double:
                return cls.decode_float(raw_data)
            elif ty_enum == FieldType.String:
                return cls.decode_str(raw_data)
            else:
                raise Exception('unknown field type: {}'.format(def_type))

    @classmethod
    def decode_bool(cls, raw_data):
        if isinstance(raw_data, NoneType):
            return False
        elif isinstance(raw_data, str):
            raw_data = raw_data.strip().lower()
            if raw_data == 'true':
                return True
            elif raw_data == 'false':
                return False

            # Try to convert to float, and continue to convert to bool
            raw_data = bool(float(raw_data)) if raw_data else False
        return True if raw_data else False

    @classmethod
    def decode_int(cls, raw_data):
        if isinstance(raw_data, NoneType):
            return 0
        if isinstance(raw_data, str):
            if raw_data and raw_data[-1] == '%':
                raw_data = raw_data[:len(raw_data) - 1]
            raw_data = raw_data.strip()
            return int(float(raw_data)) if raw_data else 0
        return int(raw_data)

    @classmethod
    def decode_long(cls, raw_data):
        if isinstance(raw_data, NoneType):
            return 0
        elif isinstance(raw_data, str):
            raw_data = raw_data.strip()
        return long(float(raw_data)) if raw_data else 0L

    @classmethod
    def decode_float(cls, raw_data):
        if isinstance(raw_data, NoneType):
            return 0.0

        if isinstance(raw_data, str):
            raw_data = raw_data.strip()
            if raw_data and raw_data[-1] == '%':
                raw_data = raw_data[:len(raw_data) - 1]
        return float(raw_data) if raw_data else 0.0

    @classmethod
    def decode_str(cls, raw_data):
        if isinstance(raw_data, NoneType):
            return ''
        elif isinstance(raw_data, str):
            return raw_data.decode('utf-8')
        return str(raw_data)

    @classmethod
    def decode_arr(cls, def_type, raw_data):
        ret = []
        raw_data = str(raw_data)
        if not raw_data:
            return ret

        const = BitSet(FieldConstraint.Required)
        for elem in cls._split(str(raw_data), ARRAY_SEP, ESCAPE):
            ret.append(cls.decode(const, def_type, elem))
        return ret

    @classmethod
    def decode_map(cls, raw_data, key_type, value_type):
        raw_data = str(raw_data)

        ret = {}
        pairs = cls._split(str(raw_data), PAIR_SEP, ESCAPE)
        for pair in pairs:
            if not pair:
                continue
            k, v = cls._split(pair, KW_SEP, ESCAPE)
            key_const = BitSet(FieldConstraint.Required)
            value_const = BitSet(FieldConstraint.Optional)
            ret.update([(cls.decode(key_const, key_type, k),
                         cls.decode(value_const, value_type, v))])

        return ret

    @classmethod
    def _split(cls, s, sep, escape='\\'):
        ret = []
        if not s:
            ret.append(s)
            return ret

        s = s.decode('utf-8')
        escape = escape.decode('utf-8')
        sep = [sep_elem.decode('utf-8') for sep_elem in sep]

        cur_item, i = u'', 0
        while i < len(s):
            ch = s[i]
            if ch == escape:
                i += 1
                if i < len(s):
                    cur_item += s[i]
            elif ch in sep:
                ret.append(cur_item)
                cur_item = u''
            else:
                cur_item += ch
            i += 1

        if cur_item or s[-1] in sep:
            ret.append(cur_item)

        return [ret_elem for ret_elem in ret]


class FieldConstraint(object):
    """字段约束"""
    Optional = 0x00000001
    Required = 0x00000002
    Repeated = 0x00000004
    Indexed = 0x00000008
    Mapped = 0x00000010
    Unique = 0x00000020

    @classmethod
    def str_to_constraints(cls, s):
        constraints = 0
        splited = [part.lower().strip() for part in s.split('|')]
        for part in splited:
            if part == 'required':
                constraints |= cls.Required
            elif part == 'optional':
                constraints |= cls.Optional
            elif part == 'repeated':
                constraints |= cls.Repeated
            elif part == 'indexed':
                constraints |= cls.Indexed
            elif part == 'mapped':
                constraints |= cls.Mapped
            elif part == 'unique':
                constraints |= cls.Unique
        return constraints

    @classmethod
    def constratint_to_str(cls, constraint):
        if constraint & cls.Optional:
            return 'optional'
        elif constraint & cls.Required:
            return 'required'
        elif constraint & cls.Repeated:
            return 'repeated'
        elif constraint & cls.Indexed:
            return 'indexed'
        elif constraint & cls.Mapped:
            return 'mapped'
        elif constraint & cls.Unique:
            return 'unique'
        else:
            raise Exception('invalid constraint: {:x}'.format(constraint))

    @classmethod
    def is_valid_constraints(cls, s, constraints=0):
        return cls._check(s, constraints, False)

    @classmethod
    def check_and_raise(cls, s, constraints=0):
        cls._check(s, constraints, True)

    @classmethod
    def _check(cls, s, constraints, need_raise):
        if not constraints:
            constraints = cls.str_to_constraints(s)

        if constraints == 0:
            if need_raise:
                raise Exception('Invalid constraint(s): {}'.format(s))
            else:
                return False

        if constraints & cls.Indexed:
            if constraints & (cls.Optional | cls.Required | cls.Repeated) == 0:
                if need_raise:
                    raise Exception('<indexed> or <mapped> cannot not be used alone')
                else:
                    return False
        elif (constraints & cls.Indexed) and (constraints & cls.Mapped):
            if need_raise:
                raise Exception('Conflict to use <indexed> and <mapped> constraint')
            else:
                return False
        return True


class CfgField(object):
    """配置字段"""

    def __init__(self):
        self._wb = None
        self._sheet = None
        self._col_index = 0

        self._constraints = BitSet()
        self._type = ''
        self._name = ''
        self._desc = ''
        self._detailed_desc = ''
        self._dft_val = None

        self._values = []

    def construct(self, wb, sheet, col_index):
        if not self.is_like_cfg(sheet, col_index):
            return False

        self._wb = wb
        self._sheet = sheet
        self._col_index = col_index

        # Get field definitions information.
        lbda = lambda val: val if val is None else (
        item.strip() if isinstance(val, (str, unicode)) else str(item))
        FC = FieldConstraint
        col_values = sheet.col_values(col_index)
        self._constraints.set_bits(FC.str_to_constraints(col_values[0]))
        self._type, self._name, self._desc = [lbda(item) for item in
                                              col_values[1:TOTAL_DEF_ROWS]]

        # Try get default value.
        pair = self._name.split('=', 1)
        self._name = pair[0].strip()
        if len(pair) == 2:
            self._dft_val = pair[1]

        # Get field values.
        self._values = col_values[TOTAL_DEF_ROWS:]

        # Check it.
        return self._check()

    @property
    def constraints(self):
        return self._constraints.bits

    @property
    def col_index(self):
        return self._col_index

    @property
    def type(self):
        return self._type

    @property
    def name(self):
        return self._name

    @property
    def upper_case_name(self):
        fch = ord(self._name[0])
        if ord('a') <= fch <= ord('z'):
            return chr(fch - 32) + self._name[1:]
        else:
            return self._name

    @property
    def desc(self):
        return self._desc

    @property
    def detailed_desc(self):
        return self._detailed_desc

    @property
    def indexed(self):
        return self.has_constraint(FieldConstraint.Indexed)

    @property
    def mapped(self):
        return self.has_constraint(FieldConstraint.Mapped)

    @staticmethod
    def is_like_cfg(sheet, col_index):
        # Constaints check.
        def _get_val(row):
            val = sheet.cell(row, col_index).value
            if isinstance(val, unicode):
                val = val.encode('utf-8')
            elif not isinstance(val, str):
                val = str(val)
            return val

        const = _get_val(0)
        FC = FieldConstraint
        if not FC.is_valid_constraints(const):
            return False

        # Type check.
        field_type = _get_val(1)
        if ':' in field_type:
            field_pair = field_type.split(':', 1)
        else:
            field_pair = [field_type]
        for field_type in field_pair:
            if not FieldType.is_valid_type_str(field_type):
                return False

        # Name check.
        if not _get_val(2).strip():
            return False

        return True

    def has_constraint(self, constraint):
        return self._constraints.has_bits(constraint)

    def make_proto(self):
        FC = FieldConstraint

        bits = self.constraints
        field_number = self._col_index + 1

        stmt = '  /*\n' + self._desc.encode('utf-8') + '\n  */\n' if self._desc else ''
        if self._constraints.has_bits(FC.Mapped):
            stmt += '  message {}Pair {{\n'.format(self._name.title())
            stmt += '    required {} key = 1;\n'.format(self._type.split(':')[0])
            stmt += '    optional {} value = 2;\n'.format(self._type.split(':')[1])
            stmt += '  }\n'
            stmt += '  repeated {}Pair {} = {};'.format(self._name.title(), self._name,
                                                        field_number)
        else:
            stmt += '  {} {} {} = {}'.format(FC.constratint_to_str(bits), self._type,
                                             self._name, field_number)
            if self._constraints.has_bits(FC.Optional):
                ty_enum = FieldType.str_2_type(self._type)
                if self._dft_val is None:
                    stmt += '[default = {}]'.format(FieldType.dft_val_str(ty_enum))
                else:
                    if ty_enum == FieldType.String:
                        stmt += '[default = "{}"]'.format(self._dft_val)
                    else:
                        stmt += '[default = {}]'.format(self._dft_val)
            stmt += ';'

        return stmt.decode('utf-8')

    def parse_data(self, row):
        row -= TOTAL_DEF_ROWS
        if row < 0 or row >= len(self._values):
            return None

        return FieldCodec.decode(self._constraints, self._type, self._values[row])

    def _check(self):
        FC = FieldConstraint
        if self._constraints.has_bits(FC.Mapped):
            if len(self._type.split(':')) != 2:
                self._raise(
                    '<mapped> constraint field type must has two type identifiers, '
                    'eg: <int32:string>')
        # 重复的值检查 由于以前的表的问题，暂时屏蔽
        # if self._constraints.has_bits(FC.Indexed):
        #    data = set()
        #    for value in self._values:
        #        if value in data:
        #            self._raise('Indexed field type can NOT have same value:{}'.format(
        #                value))
        #        data.add(value)
        return True

    def _raise(self, msg):
        raise Exception(
            '[{}:{}:{}]: {}'.format(
                op.basename(self._wb.filepath.encode(SHOWABLE_CODING)),
                self._sheet.name.encode(SHOWABLE_CODING), self._col_index, msg))


class CfgSheet(object):
    def __init__(self):
        self._wb = None
        self._sheet = None
        self._converted_name = u''
        self._dataName = u''
        self._fields = {}

    @property
    def name(self):
        return self._sheet.name

    @property
    def dataName(self):
        return self._dataName

    @property
    def converted_name(self):
        return self._converted_name

    @property
    def proto_name(self):
        # return self._converted_name + '.proto'
        return self._dataName + '.proto'

    @property
    def cs_name(self):
        # return self._converted_name + '.cs'
        return self._dataName + '.cs'

    @property
    def py_name(self):
        # return self._converted_name + '_pb2.py'
        return self._dataName + '_pb2.py'

    @property
    def data_name(self):
        # return self._converted_name + '.data'
        return self._dataName + '.data'

    @property
    def data_name_client(self):
        return 'cc_' + self._dataName + '.bytes'

    @property
    def json_name(self):
        # return self._converted_name + '.json'
        return self._dataName + '.json'

    @property
    def debug_data_name(self):
        # return self._converted_name + '.debug'
        return self._dataName + '.debug'

    @property
    def fields(self):
        return self._fields

    @property
    def indexed_fields(self):
        return [field for field in self._fields.itervalues() if field.indexed]

    @staticmethod
    def is_like_cfg(sheet):
        FC = FieldConstraint
        if sheet.nrows == 0:
            return False

        # for by in buffer(sheet.name.encode('utf-8')):
        #     if int(by.encode('hex'), 16) >= 128:
        #         return False

        for field_const in sheet.row_values(0):
            if isinstance(field_const, unicode):
                field_const = field_const.encode('utf-8')
            elif not isinstance(field_const, str):
                field_const = str(field_const)
            if FC.is_valid_constraints(field_const) != 0:
                return True

        return False

    def construct(self, wb, sheet):
        if not self.is_like_cfg(sheet):
            # print '[{}:{}] looks not like config sheet, ignore'.format(wb.filepath,
            # sheet.name)
            return False

        self._wb = wb
        self._sheet = sheet
        fillName = sheet._cell_values[0][0]
        if not fillName:
            self._raise(sheet._cell_values[0][0], 'fill name must not empty, please check A1 grid!')
        self._converted_name = SpellingConverter().parse_str(fillName, to_upper_case=True)
        self._dataName = self._converted_name

        # print '&*&*&*&*construct', self._sheet.name, sheet.name,  '<---->', self._converted_name

        for col_index in xrange(sheet.ncols):
            field = CfgField()
            if field.construct(wb, sheet, col_index):
                self._fields[col_index] = field

        wb_name = op.basename(self._wb.filepath).encode(SHOWABLE_CODING)
        print '[Recognizer]    Recognized: {}:{}'.format(wb_name, self.name.encode(SHOWABLE_CODING))

        return True

    def make_proto(self):
        msg_name = self._converted_name
        stmt = u'message {} {{\n'.format(msg_name)
        for field in self._fields.itervalues():
            stmt += field.make_proto() + u'\n'
        stmt += u'}\n'

        stmt += u'message {}_ARRAY {{\n'.format(msg_name)
        stmt += u'  repeated {} items = 1; // All config rows data.\n'.format(msg_name)
        return stmt + u'}'

    def make_data(self, code_path, attr):
        # return self._make_proto_data_common(code_path).SerializeToString()
        self._make_proto_data_common(code_path, attr, None)

    def make_debug_data(self, code_path, attr, uniqueChecks):
        # return unicode(self._make_proto_data_common(code_path))
        return self._make_proto_data_common(code_path, attr, uniqueChecks)

    def _make_proto_data_common(self, code_path, arr, uniqueChecks):
        # name, py_name = op.splitext(self.proto_name)[0], op.splitext(self.py_name)[0]
        # exec 'from {} import {}, {}_ARRAY'.format(op.basename(code_path) + '.' + py_name, name, name)
        # exec 'arr = {}_ARRAY()'.format(name)
        for row in xrange(TOTAL_DEF_ROWS, self._sheet.nrows):
            try:
                item = arr.items.add()
                for field in self._fields.itervalues():
                    field_data = field.parse_data(row)
                    if isinstance(field_data, dict):
                        for k, v in field_data.iteritems():
                            pair = getattr(item, field.name).add()
                            setattr(pair, 'key', k)
                            setattr(pair, 'value', v)
                    elif isinstance(field_data, (tuple, list)):
                        getattr(item, field.name).extend(field_data)
                    else:
                        if field_data is not None:
                            setattr(item, field.name, field_data)

                            if uniqueChecks is not None and field.has_constraint(FieldConstraint.Unique):
                                if field.name not in uniqueChecks:
                                    uniqueChecks[field.name] = []
                                    chechList = uniqueChecks[field.name]
                                else:
                                    chechList = uniqueChecks[field.name]

                                if field_data in chechList:
                                        self._raise(field, 'Field constraint is unique, but cell({}, {}) is repeated, '
                                                           'field name:{}, value:{}'.
                                                    format(row+1, field.col_index+1, field.name, field_data))
                                else:
                                    chechList.append(field_data)
                        elif not field.has_constraint(FieldConstraint.Optional):
                            self._raise(field, 'Field constraint is not OPTIONAL, '
                                               'but cell({}, {}) is None'.format(row,
                                                                                 field.col_index))
            except Exception, e:
                raise e.__class__(
                    'Parse data failed, Sheet: {}, Row: {}, Col: {}, fieldName: {},\n raw error:\n\t{}\n'.format(
                        self._converted_name, row + 1, field.col_index + 1, field.name,  e))
            # return arr
        return uniqueChecks

    def make_json_data(self, array):
        # array = []
        for row in xrange(TOTAL_DEF_ROWS, self._sheet.nrows):
            rowDict = {}
            for field in self._fields.itervalues():
                rowDict[field.name] = field.parse_data(row)
            array.append(rowDict)
        # return json.dumps(array, indent=4, sort_keys=True, ensure_ascii=False)

    def _raise(self, field, msg):
        fieldName = field.name if hasattr(field, 'name') else field
        raise Exception(
            '[{}:{}:{}]: {}'.format(
                op.basename(self._wb.filepath.encode(SHOWABLE_CODING)),
                self.name.encode(SHOWABLE_CODING), fieldName, msg))


class WBRecognizer(object):
    """Excel Workbook 识别器"""

    def __init__(self):
        self._xls_file = ''
        self._name2sheet = {}
        self._datasheet = {}
        self._index2sheet = {}
        self._xls_short_name = ''
        self._file_author = 'unknown'
        self._last_changed_date = 'unknown'

    @property
    def xls_file(self):
        return self._xls_file

    @property
    def sheets(self):
        return self._name2sheet.itervalues()

    @property
    def dataSheets(self):
        return self._datasheet

    @property
    def file_author(self):
        return self._file_author

    @property
    def last_changed_date(self):
        return self._last_changed_date

    @classmethod
    def is_like_cfg(cls, xls_file):
        if not cls._is_name_like_cfg(xls_file):
            return False

        wb = xlrd.open_workbook(xls_file)
        return any([CfgSheet.is_like_cfg(sheet) for sheet in wb.sheets()])

    @classmethod
    def like_cfg_sheets(cls, xls_file):
        if not cls._is_name_like_cfg(xls_file):
            return False

        wb = xlrd.open_workbook(xls_file)
        return [sheet.name for sheet in wb.sheets() if CfgSheet.is_like_cfg(sheet)]

    def _set_file_info(self, raw_file):
        lines = os.popen('svn info {}'.format(raw_file.encode('gb2312'))).readlines()
        # print("file:{} lines:{}".format(filename, lines))
        for line in lines:
            pos = line.find('Last Changed Author: ')
            if pos != -1:
                self._file_author = line[(pos + len('Last Changed Author: ')):].strip()
                # self._file_author = line[pos:].strip()
                continue
            pos = line.find('Last Changed Date: ')
            if pos != -1:
                self._last_changed_date = line[
                                          (pos + len('Last Changed Date: ')):].decode(
                    'gb2312').strip()

                # self._last_changed_date = line[pos:].decode('gb2312').strip()

    def recognize(self, xls_file):
        pos = xls_file.find('design') + len('design') + 1
        if pos < len(xls_file):
            self._xls_short_name = xls_file[pos:]
            # print '$%$%$%$%$%$%Source', self._xls_short_name
            self._xls_short_name = self._xls_short_name.replace('\\', '/')
            # print '$%$%$%$%$%$%Target', self._xls_short_name

        self._xls_file = unicode(xls_file)
        # print '$%$%$%$%$%$%     ', self._xls_file

        self._set_file_info(xls_file)

        wb = xlrd.open_workbook(self._xls_file)
        wb.filepath = xls_file

        # 检查是否存在分表标识
        names = []  # 表名若出现多次，则说明需要分表
        submeter = None
        for st in wb._sheet_list:
            sheetName = self._getValue(st)
            if not sheetName:
                continue

            if sheetName not in names:
                names.append(sheetName)
            else:
                submeter = sheetName
                break

        for sheet_index in xrange(wb.nsheets):
            sheet = CfgSheet()
            if sheet.construct(wb, wb.sheet_by_index(sheet_index)):
                fillName = self._getValue(sheet._sheet)
                if submeter == fillName:
                    self._name2sheet[fillName] = sheet

                    if submeter in self._datasheet.keys():
                        self._datasheet[fillName].append(sheet)
                    else:
                        temp = list()
                        temp.append(sheet)
                        self._datasheet[fillName] = temp
                else:
                    self._name2sheet[fillName] = sheet
                    if fillName in self._datasheet.keys():
                        self._datasheet[fillName].append(sheet)
                    else:
                        temp = list()
                        temp.append(sheet)
                        self._datasheet[fillName] = temp
        # for key, values in self._datasheet.items():
        #     print 'key=', key
        #     for value in values:
        #         print 'valueName', value.name
        return True if len(self._name2sheet) else False

    @classmethod
    def _is_name_like_cfg(cls, xls_file):
        if not cls._prefix_check(xls_file):
            return False
        elif not cls._suffix_check(xls_file):
            return False
        elif not op.exists(xls_file):
            return False

        return True

    @staticmethod
    def _prefix_check(xls_file):
        base_name = op.basename(xls_file)
        base_name_len = len(base_name)
        for prefix in CFG_PREFIXES:
            uni_prefix = prefix.decode('utf-8')
            prefix_len = len(uni_prefix)
            if base_name_len >= prefix_len and \
                            base_name[:prefix_len] == uni_prefix:
                return True
        return False

    @staticmethod
    def _suffix_check(xls_file):
        splited = op.splitext(xls_file)
        if len(splited) < 2:
            return False
        elif splited[1] not in CFG_SUFFIXES:
            return False

        return True

    @staticmethod
    def _getValue(sheet, rowIndex=0, lineIndex=0):
        if rowIndex >= sheet.nrows or lineIndex >= sheet.ncols:
            return None

        return sheet._cell_values[rowIndex][lineIndex]
