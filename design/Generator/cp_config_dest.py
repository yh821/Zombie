# -*- coding: utf8 -*-
from os import path as op
import os
import shutil
from file_util import FileUtil
from svn_cmd import SvnCmd

CUR_DIR = op.dirname(op.abspath(__file__))
GENED_CODE_DIR = op.split(CUR_DIR)[0]

TEMP_DIR = op.dirname(op.dirname(CUR_DIR))
TEMP_DIR = op.dirname(op.dirname(op.dirname(TEMP_DIR)))
TOP_DIR = op.dirname(TEMP_DIR)

CODE_DIR = op.join(GENED_CODE_DIR, 'config', )

GUIDE_TEST_DIR = op.join(TOP_DIR, 'stable_project', 'guide_test', 'server', 'config', 'game')


def _copy_dir(src_dir, dest_dir, legal_suffixes=()):
    for root, dirs, names in os.walk(src_dir):
        for name in names:
            filepath = op.join(root, name)
            if '.svn' in filepath or op.splitext(filepath)[1] not in legal_suffixes:
                continue
            bname = op.basename(filepath)
            shutil.copy(filepath, op.join(dest_dir, bname))


def main():
    code_dir = op.join(GENED_CODE_DIR, 'run', 'server')
    data_dir = op.join(GENED_CODE_DIR, 'run', 'data')

    dest_dir = raw_input("please write dest_dir:\n")
    dest_dir = op.join(dest_dir, 'server', 'config', 'game')
    if not op.exists(dest_dir):
        print '>>> ' + dest_dir + ' not exist. '
        os.system('pause')
        return

    SvnCmd(path=dest_dir, cmd='update', logmsg='update config files').Run()
    SvnCmd(path=op.join(dest_dir, 'data'), cmd='update', logmsg='update config files').Run()

    update_files_cfg, add_files_cfg = FileUtil.copy(code_dir, dest_dir, suffixes=('.py', ), force=True)
    update_files_data, add_files_data = FileUtil.copy(
        data_dir, op.join(dest_dir, 'data'), suffixes=('.data', ), force=True)
    update_files_json, add_files_json = FileUtil.copy(
        data_dir, op.join(dest_dir, 'data'), suffixes=('.json', ), force=True)
    print("{} .data files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_data)
    print('')

    if add_files_data:
        print("added:")
        prettyOutput(add_files_data)
        for new_file in add_files_data:
            dst_file = op.join(op.join(dest_dir, 'data'), op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config data files').Run()

    print("{} .cfg files {}   ".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_cfg)
    print('')
    if add_files_cfg:
        print("added:")
        prettyOutput(add_files_cfg)
        for new_file in add_files_cfg:
            dst_file = op.join(dest_dir, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config files').Run()

    print("{} .json files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_json)
    print('')

    if add_files_json:
        print("added:")
        prettyOutput(add_files_json)
        for new_file in add_files_json:
            dst_file = op.join(op.join(dest_dir, 'data'), op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config json files').Run()

    SvnCmd(path=dest_dir, cmd='commit', logmsg='update config files').Run()

    # 复制配置数据
    # _copy_dir(code_dir, dest_dir, ('.py', ))
    # _copy_dir(data_dir, op.join(dest_dir, 'data'), ('.data', ))
    # _copy_dir(data_dir, op.join(dest_dir, 'data'), ('.json', ))
    # print '>>> All config files and config-data files copied to server?'


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
    main()
    raw_input("Done!Press return key to exit.")


