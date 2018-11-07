# -*- coding: utf8 -*-
"""
author: pyb pengyongbin@37.com
date: 2015-01-21
拷贝生成的配置文件到服务端相应目录,并提交svn

date: 2015-01-28
只拷贝更新过的文件,增加提交svn功能

date: 2015-01-29
只拷贝.data/.py文件,不拷贝.pyc文件

date: 2015-09-06
增加拷贝更新.json文件
"""

from os import path as op
from file_util import FileUtil
from svn_cmd import SvnCmd
import pysvn


CUR_DIR = op.dirname(op.abspath(__file__))

GENED_DATA_DIR = op.join(CUR_DIR, 'data')
GENED_CFG_DIR = op.join(CUR_DIR, 'server')
GEND_VERSION_DIR = op.join(CUR_DIR, 'version')

ROOT_DIR = op.dirname(op.dirname(CUR_DIR))
SERV_CFG_DIR = op.join(ROOT_DIR, 'server', 'config', 'game')
SERV_DATA_DIR = op.join(SERV_CFG_DIR, 'data')
SERV_VERSION_DIR = op.join(ROOT_DIR, 'server', 'version')


def copy2Serv():
    if not op.exists(SERV_CFG_DIR) or not op.exists(SERV_DATA_DIR):
        print("ONLY used for server part.")
        print("path:{} or {} not existed!".format(SERV_DATA_DIR, SERV_CFG_DIR))
        return False

    SvnCmd(path=SERV_DATA_DIR, cmd='update', logmsg='update config files').Run()
    SvnCmd(path=SERV_CFG_DIR, cmd='update', logmsg='update config files').Run()
    SvnCmd(path=SERV_VERSION_DIR, cmd='update', logmsg='update version files').Run()

    update_files_data, add_files_data = FileUtil.copy(GENED_DATA_DIR, SERV_DATA_DIR, suffixes=('.data',), force=True)
    update_files_cfg, add_files_cfg = FileUtil.copy(GENED_CFG_DIR, SERV_CFG_DIR, suffixes=('.py',), force=True)
    update_files_json, add_files_json = FileUtil.copy(GENED_DATA_DIR, SERV_DATA_DIR, suffixes=('.json', ), force=True)
    update_files_version, add_files_version = FileUtil.copy(GEND_VERSION_DIR, SERV_VERSION_DIR, suffixes=('.txt', ), force=True)

    print("{} .data files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_data)
    print('')

    if add_files_data:
        print("added:")
        prettyOutput(add_files_data)
        for new_file in add_files_data:
            dst_file = op.join(SERV_DATA_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config data files').Run()

    print("{} .cfg files {}   ".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_cfg)
    print('')
    if add_files_cfg:
        print("added:")
        prettyOutput(add_files_cfg)
        for new_file in add_files_cfg:
            dst_file = op.join(SERV_CFG_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config files').Run()

    print("{} .json files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_json)
    print('')

    if add_files_json:
        print("added:")
        prettyOutput(add_files_json)
        for new_file in add_files_json:
            dst_file = op.join(SERV_DATA_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config json files').Run()

    SvnCmd(path=SERV_CFG_DIR, cmd='commit', logmsg='update config files').Run()

    print("{} .txt files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_version)
    print('')

    if add_files_version:
        print("added:")
        prettyOutput(add_files_version)
        for new_file in add_files_version:
            dst_file = op.join(SERV_VERSION_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new data version txt files').Run()

    SvnCmd(path=SERV_VERSION_DIR, cmd='commit', logmsg='commit data version txt files').Run()


def prettyOutput(names):
    count = 0
    for name in names:
        print "{0: <25}".format(op.basename(name)),
        count += 1
        if count % 3 == 0:
            print ''
    if count % 3 != 0:
        print ''


if __name__ == '__main__':
    copy2Serv()
    raw_input("Done!Press return key to exit.")
