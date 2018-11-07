# -*- coding: utf-8 -*-

from c import *
from recognizer import WBRecognizer
from spelling_converter import SpellingConverter


class CfgFinder(object):
    """配置文件查找器"""

    def __init__(self):
        self._find_path = ''  # find path

        self._idx2name = {}  # key: index, value: workbook file name
        self._finded = {}  # key: workbook file name, value: sheet name list

    @property
    def files(self):
        return self._idx2name.itervalues()

    def get_by_id(self, idx):
        return self._idx2name.get(idx)

    def find(self, in_path, only_changed=True, verbose=True):
        self._find_path = in_path

        if verbose:
            print u'正在搜索配置文件, 请稍后...',

        files = self._find_in_path()
        if only_changed:
            self._svn_filter(files)

        for filename in files:
            cfg_sheets = WBRecognizer.like_cfg_sheets(filename)
            if cfg_sheets:
                self._finded[filename] = cfg_sheets

        files = self._finded.keys()
        self._sort_files(files)
        self._idx2name.update([(i + 1, name) for i, name in enumerate(files)])

        if verbose:
            print u'完成!'

    def list_finded(self):
        for idx, name in self._idx2name.iteritems():
            print '{} ----{}'.format(idx, op.basename(name).encode(SHOWABLE_CODING)),
            sheets = ': '
            for sheet_idx, sheet in enumerate(self._finded[name]):
                if sheet_idx != 0:
                    sheets += ', '
                sheets += '{}'.format(sheet.encode(SHOWABLE_CODING))
            print sheets

    def _find_in_path(self):
        files = []
        for root, dirs, names in os.walk(self._find_path):
            for name in names:
                filename = op.abspath(op.join(root, name))
                if '.svn' in filename:
                    continue
                elif not WBRecognizer.is_like_cfg(filename):
                    continue
                files.append(filename)
        return files

    @staticmethod
    def _svn_filter(files):
        assert isinstance(files, list)
        for i in xrange(len(files) - 1, -1, -1):
            if not os.popen('svn st {}'.format(files[i].encode(FS_CODING))).readline():
                del files[i]

    @staticmethod
    def _sort_files(files):
        sp_converter = SpellingConverter()

        def _Comp(left, right):
            cvt_left = u''.join([sp_converter.get_char_info(ch).spelling for ch in left])
            cvt_right = u''.join([sp_converter.get_char_info(ch).spelling for ch in right])
            return cmp(cvt_left, cvt_right)

        files.sort(cmp=_Comp, key=lambda fn: op.basename(fn))
