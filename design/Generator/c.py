# -*- coding: utf-8 -*-
import sys
from functools import partial

from os import path as op
import os

# Show coding,win32 DOS window use Local coding setting to show text,
# PyCharm use UTF-8 to show text.
FS_CODING = sys.getfilesystemencoding()
CUR_PATH = op.dirname(op.abspath(__file__)).decode(FS_CODING)
CFG_PATH = op.dirname(CUR_PATH)
TRIGGER_PATH = op.join(CUR_PATH, 'trigger')

# Tools internal configs(do not modify it!)
_IN_PYCHARM = False
_JOIN_IT = partial(op.join, CUR_PATH)
_CREATE_IT = lambda d: (os.makedirs(_JOIN_IT(d)) if not op.exists(_JOIN_IT(d)) else None) or _JOIN_IT(d)

# Config prefixes.
CFG_PREFIXES = ('A')
# Config suffixes.
CFG_SUFFIXES = ('.xls', '.xlsx', '.xlsm')

# Showable string coding(in win32, use MBCS(In Chinese OS version is CP936), in linux & PyCharm dev platform, is utf-8).
SHOWABLE_CODING = 'utf-8' if _IN_PYCHARM else FS_CODING

# Output files paths setting(by the way, makesurer all paths already created).
PROTO_PATH = _CREATE_IT('proto')
DATA_PATH = _CREATE_IT('data')
SERVER_PATH = _CREATE_IT('server')
CLIENT_PATH = _CREATE_IT('client')
CODE_TEMPL_PATH = _CREATE_IT('code_templates')

# Generated code namespace(only available in Client).
NS = 'GameData'

# Separators/EscapeCharacter define
ESCAPE = '\\'  # Escape character
KW_SEP = [':', '：']  # Keyword separator
PAIR_SEP = [';', '；']  # Pair separator(use to separate key->word
ARRAY_SEP = [';', ',', '，', '；']  # Array separator(use to separate REPEATED field datas

# Total used as type define, constraint, value name, describe rows count.
# Row 1: Field Constraints
# Row 2: Field Type
# Row 3: Field Name
# Row 4: Field Brief Description
TOTAL_DEF_ROWS = 4

# Determine delete proto directory or not, default is delete when all codes and datas generated.
DONT_DEL_PROTO = True

# Pre generate trigger files config.
PRE_TRIGGER_FILES = [
]

# After generate trigger files config.
AFTER_TRIGGER_FILES = [
    (u'A属性模版.xlsm', u'gen_attr_code.py'),
    (u'A系统开启表.xls', u'gen_attr_code.py')
]

