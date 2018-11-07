# -*- coding: utf-8 -*-
import codecs
from os import path as _op
import subprocess as sp
from subprocess import Popen
import re
import json

from c import *
from file_util import FileUtil


class ProtoCodeModifier(object):
    """协议代码修改器"""

    def __init__(self, sheet, code_file):
        self._sheet = sheet
        self._code_file = code_file
        self._contents = None

    def modify(self):
        self._read_content()
        self._decorate_indexed_fields()
        self._update_code_file()

    def _read_content(self):
        with open(self._code_file, 'r') as f:
            self._contents = f.readlines()

    def _update_code_file(self):
        with open(self._code_file, 'w') as f:
            for line in self._contents:
                f.write(line)

    def _decorate_indexed_fields(self):
        indexed_fields = self._sheet.indexed_fields
        if len(indexed_fields) == 0:
            return

        brace_depth = 0
        r = re.compile(r'(\s*)public\s+partial\s+class\s+(\w+)')
        for idx in xrange(len(self._contents)):
            line = self._contents[idx]
            for ch in line:
                brace_depth += (1 if ch == '{' else (-1 if ch == '}' else 0))

            if brace_depth != 1:
                continue

            m = r.match(line)
            if m is None:
                continue

            cls_name = m.group(2)
            if cls_name != self._sheet.name:
                continue

            attr_stmt = '{}[CfgIndices('.format(m.group(1))
            for field_index, field in enumerate(indexed_fields):
                if field_index != 0:
                    attr_stmt += ', '
                attr_stmt += '"{}"'.format(field.name)
            attr_stmt += ')]\n'
            self._contents.insert(idx, attr_stmt)
            break


class ProtoExporter(object):
    """协议导出器"""

    def __init__(self, reco):
        self._reco = reco

    def export(self, proto_path, client_path, server_path):
        print('source file:{}'.format(self._reco._xls_short_name.encode('utf-8')))
        for sheet in self._reco.sheets:
            print "---------",proto_path, '<--->', sheet.proto_name, '<-->', sheet.name
            with open(op.join(proto_path, sheet.proto_name), 'w') as f:
                f.write(sheet.make_proto().encode('utf-8'))

        old_cwd = os.getcwd()
        os.chdir(proto_path)
        for sheet in self._reco.sheets:
            self._gen_svr_proto(sheet, server_path)
            self._gen_cli_proto(sheet, client_path)

            self._gen_svr_cfg_cls(sheet, server_path)

            print '[ProtoExporter] Exported: {}'.format(
                sheet.name.encode(SHOWABLE_CODING))

        os.chdir(old_cwd)

        self._gen_svr_init_file(server_path)
        # self._copy_code_templates()

    def _gen_svr_proto(self, sheet, server_path):
        proto = sheet.proto_name.encode(FS_CODING)
        self._exec(sheet.proto_name, 'protoc',
                   '--python_out={}'.format(server_path.encode(FS_CODING)), proto)

    def _gen_cli_proto(self, sheet, client_path):
        proto = sheet.proto_name.encode(FS_CODING)
        cs_file = op.join(client_path.encode(FS_CODING), op.splitext(proto)[0] + '.cs')
        if 'AttrTypes.cs' in cs_file:
            return
        self._exec(proto, 'protogen', '-ns:{}'.format(NS), '-i:{}'.format(proto),
                   '-o:{}'.format(cs_file))
        ProtoCodeModifier(sheet, cs_file).modify()

    @staticmethod
    def _exec(proto, *args):
        child = Popen(args, shell=True, stdout=sp.PIPE)
        if child.wait() != 0:
            raise Exception(
                'generate proto<{}> failed, err: {}'.format(proto, child.stdout.read()))

    def _gen_svr_cfg_cls(self, sheet, server_path):
        proto = sheet.converted_name
        cls_name = proto + 'Cfg'
        with open(op.join(server_path, '{}'.format(cls_name + '.py')), 'w') as f:
            w = lambda cnt: f.write('{}\n'.format(cnt))

            # file head.
            w('# -*- coding: utf-8 -*-')
            w('# Auto generate by Script Tool')
            w('from os import path as op')
            w('from llbc import Singleton')
            w('from {}_pb2 import {}_ARRAY'.format(proto, proto))
            w('')
            w('')

            # config item class.
            def _MakeFieldMethods(field):
                w('    @property')
                w('    def {}(self):'.format(field.name))
                w('        """')
                w('        {}'.format(field.desc.encode('utf-8')))
                w('        """')
                w('        return self._rawItem.{}'.format(field.name))
                w('')

            item_cls_name = '{}Item'.format(cls_name)
            w('class {}(object):'.format(item_cls_name))
            w('    __slots__ = ("_rawItem")\n')
            w('    """')
            w('    File Name: {}'.format(self._reco._xls_short_name.encode('utf-8')))
            w('    """')
            w('    def __init__(self, rawItem):')
            w('        self._rawItem = rawItem')
            w('')
            for field in sheet.fields.itervalues():
                _MakeFieldMethods(field)
            w('')
            # config singleton class.
            w('class {}(Singleton):'.format(cls_name))
            w('    """')
            w('    File Name: {}'.format(self._reco._xls_short_name.encode('utf-8')))
            w('    """')
            # __init__
            w('    def __init__(self):')
            w('        super({}, self).__init__()'.format(cls_name))
            w('        cur_dir = op.dirname(op.abspath(__file__))')
            w("        datafile = open(op.join(cur_dir, 'data', '{}.data'), 'rb')".format(
                proto))
            w('        rawItems = {}_ARRAY()'.format(proto))
            w('        rawItems.ParseFromString(datafile.read())')
            w('        self._items = []')
            w('        for rawItem in rawItems.items:')
            w('            self._items.append({}(rawItem))'.format(item_cls_name))
            w('')

            for field in sheet.indexed_fields:
                name, titled_name = field.name, field.upper_case_name
                w('        self._indexed{} = {{}}'.format(titled_name))
                w('        for item in self._items:')
                w('            if item.{} not in self._indexed{}:'.format(name,
                                                                          titled_name))
                w('                self._indexed{}[item.{}] = []'.format(titled_name,
                                                                         name))
                w('            self._indexed{}[item.{}].append(item)'.format(titled_name,
                                                                             name))
            w('')
            # hotLoad
            w('    def Reload(self):')
            # w('        super({}, self).__init__()'.format(cls_name))
            w('        cur_dir = op.dirname(op.abspath(__file__))')
            w("        datafile = open(op.join(cur_dir, 'data', '{}.data'), 'rb')".format(
                proto))
            w('        from time import time')
            w('        print"{} reload cfg data:{}".format(int(time()), datafile)')
            w('        rawItems = {}_ARRAY()'.format(proto))
            w('        rawItems.ParseFromString(datafile.read())')
            w('        self._items = []')
            w('        for rawItem in rawItems.items:')
            w('            self._items.append({}(rawItem))'.format(item_cls_name))
            w('')

            for field in sheet.indexed_fields:
                name, titled_name = field.name, field.upper_case_name
                w('        self._indexed{} = {{}}'.format(titled_name))
                w('        for item in self._items:')
                w('            if item.{} not in self._indexed{}:'.format(name,
                                                                          titled_name))
                w('                self._indexed{}[item.{}] = []'.format(titled_name,
                                                                         name))
                w('            self._indexed{}[item.{}].append(item)'.format(titled_name,
                                                                             name))
            w('')

            print '******', cls_name

            # @property: items
            w('    @property')
            w('    def items(self):')
            w('        return self._items')
            w('')

            # GetByXXX/GetOneByXXX
            for field in sheet.indexed_fields:
                name, arg_name = field.name, '{}Val'.format(field.name)
                titled_name = self._to_title_name(field.name)
                w('    def GetBy{}(self, {}):'.format(titled_name, arg_name))
                w('        items = self._indexed{}.get({})'.format(titled_name, arg_name))
                w('        return items if items is not None else []')
                w('')
                w('    def GetOneBy{}(self, {}):'.format(titled_name, arg_name))
                w('        items = self.GetBy{}({})'.format(titled_name, arg_name))
                w('        if items:')
                w('            item = items[0]')
                w('            assert isinstance(item, {}Item)'.format(cls_name))
                w('            return item')
                w('        else:')
                w('            return None')
                w('')
                w('    @property')
                if name[-1] in 'aeiou':
                    w('    def {}es(self):'.format(name))
                else:
                    w('    def {}s(self):'.format(name))
                w('        return self._indexed{}.keys()'.format(titled_name))
                w('')

            # __str__/__len__
            w('    def __str__(self):')
            w('        return str(self._items)\n')
            w('    def __len__(self):')
            w('        return len(self._items)\n')

    @staticmethod
    def _gen_svr_init_file(server_path):
        stmts = []
        for root, dirs, names in os.walk(server_path):
            for name in names:
                filename = op.join(root, name)
                if '.svn' in filename:
                    continue
                elif op.splitext(filename)[1] != '.py':
                    continue

                basename = op.splitext(op.basename(filename))[0]
                if len(basename) <= 4:
                    continue

                if basename[-3:] == 'Cfg':
                    stmts.append(
                        'from {} import {}, {}Item'.format(basename, basename, basename))
                elif basename[-4:] == '_pb2':
                    cfgClsName = basename[0:-4]
                    stmts.append('from {} import {}, {}'.format(basename, cfgClsName,
                                                                cfgClsName + '_ARRAY'))

        with open(op.join(server_path, '__init__.py'), 'w') as f:
            stmts.sort()
            for stmt in stmts:
                f.write(stmt + '\n')
            if _op.exists(_op.join(server_path, 'attr_type.py')):
                f.write('from attr_type import AttrType\n')
            if _op.exists(_op.join(server_path, 'attr_base.py')):
                f.write('from attr_base import AttrBase\n')
            if _op.exists(_op.join(server_path, 'elem_attrs.py')):
                f.write('from elem_attrs import ElemAttrs\n')
            if _op.exists(_op.join(server_path, 'buff_ctrl_attrs.py')):
                f.write('from buff_ctrl_attrs import BuffCtrlAttrs\n')

    @staticmethod
    def _copy_code_templates():
        # Copy all .cs code templates to CLIENT_PATH.
        for f in FileUtil.get_files(CODE_TEMPL_PATH, exts=('.cs',)):
            FileUtil.copy(f, CLIENT_PATH)

        # Copy all .py code templates to SERVER_PATH.
        for f in FileUtil.get_files(CODE_TEMPL_PATH, exts=('.py',)):
            FileUtil.copy(f, SERVER_PATH)

    @staticmethod
    def _to_title_name(name):
        fch = ord(name[0])
        if ord('a') <= fch <= ord('z'):
            return chr(fch - 32) + name[1:]
        else:
            return name


class DataExporter(object):
    """数据导出器"""

    def __init__(self, reco):
        self._reco = reco

    def export(self, to_path, code_path):
        # print to_path,'<-- + -->', code_path
        dataSheets = self._reco.dataSheets
        uniqueChecks = {}  #field id:集合
        uniqueChecks[-1] = []
        for sheetName, dataSheet in dataSheets.items():
            makeData = None
            makeDebugData = None
            makeJsonData = list()
            fragment = 0
            for sheet in dataSheet:
                targetSheet = sheet
                if fragment == 0:
                    protoName = op.splitext(sheet.proto_name)[0]
                    pyName = op.splitext(sheet.py_name)[0]
                    exec 'from {} import {}, {}_ARRAY'.format(op.basename(code_path) + '.' + pyName, protoName, protoName)
                    exec 'data = {}_ARRAY()'.format(protoName)
                    exec 'debugData = {}_ARRAY()'.format(protoName)
                    makeData = data
                    makeDebugData = debugData
                sheet.make_debug_data(code_path, makeDebugData, uniqueChecks)
                sheet.make_json_data(makeJsonData)
                sheet.make_data(code_path, makeData)
                fragment += 1

            #输出*.data
            #with open(op.join(to_path, targetSheet.data_name), 'wb+') as f:
            #    f.write(makeData.SerializeToString())

            #输出*.bytes
            with open(op.join(to_path, targetSheet.data_name_client), 'wb+') as f:
                f.write(makeData.SerializeToString())

            #输出*.json
            #with codecs.open(op.join(to_path, targetSheet.json_name), 'wb+', 'utf-8') as f:
            #    f.write(json.dumps(makeJsonData, indent=4, sort_keys=True, ensure_ascii=False))

            #输出*.debug
            #with codecs.open(op.join(to_path, targetSheet.debug_data_name), 'wb+', 'utf-8') as f:
            #    f.write(unicode(makeDebugData))

            print '[DataExporter]  Exported: {}'.format(sheetName.encode(SHOWABLE_CODING))
        # for sheet in self._reco.sheets:
        #     print sheet.data_name, sheet.json_name, sheet.debug_data_name
        #     with open(op.join(to_path, sheet.data_name), 'wb+') as f:
        #         f.write(sheet.make_data(code_path))
        #
        #     with codecs.open(op.join(to_path, sheet.json_name), 'wb+', 'utf-8') as f:
        #         f.write(sheet.make_json_data())
        #
        #     with codecs.open(op.join(to_path, sheet.debug_data_name), 'wb+', 'utf-8') \
        #             as f:
        #         f.write(sheet.make_debug_data(code_path))
        #     print '[DataExporter]  Exported: {}'.format(
        #         sheet.name.encode(SHOWABLE_CODING))