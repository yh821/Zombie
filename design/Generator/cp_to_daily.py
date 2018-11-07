from os import path as op
from file_util import FileUtil
from svn_cmd import SvnCmd


CUR_DIR = op.dirname(op.abspath(__file__))

GENED_DATA_DIR = op.join(CUR_DIR, 'data')
GENED_CFG_DIR = op.join(CUR_DIR, 'server')

ROOT_DIR = op.dirname(op.dirname(op.dirname(CUR_DIR)))
DAILY_CFG_DIR = op.join(ROOT_DIR, 'branches', 'daily', 'server', 'config', 'game')
DAILY_DATA_DIR = op.join(DAILY_CFG_DIR, 'data')


def copy2Daily():
    if not op.exists(DAILY_CFG_DIR) or not op.exists(DAILY_DATA_DIR):
        print("ONLY used for daily part.")
        print("path:{} or {} not existed!".format(DAILY_CFG_DIR, DAILY_DATA_DIR))
        return False

    SvnCmd(path=DAILY_DATA_DIR, cmd='update', logmsg='update config files').Run()
    SvnCmd(path=DAILY_CFG_DIR, cmd='update', logmsg='update config files').Run()

    update_files_data, add_files_data = FileUtil.copy(GENED_DATA_DIR, DAILY_DATA_DIR, suffixes=('.data',), force=True)
    update_files_cfg, add_files_cfg = FileUtil.copy(GENED_CFG_DIR, DAILY_CFG_DIR, suffixes=('.py',), force=True)
    update_files_json, add_files_json = FileUtil.copy(GENED_DATA_DIR, DAILY_DATA_DIR, suffixes=('.json'), force=True)

    print("{} .data files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_data)
    print('')

    if add_files_data:
        print("added:")
        prettyOutput(add_files_data)
        for new_file in add_files_data:
            dst_file = op.join(DAILY_DATA_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config data files').Run()

    print("{} .cfg files {}   ".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_cfg)
    print('')
    if add_files_cfg:
        print("added:")
        prettyOutput(add_files_cfg)
        for new_file in add_files_cfg:
            dst_file = op.join(DAILY_CFG_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config files').Run()

    print("{} .json files {}".format("#" * 16, "#" * 16))
    print("updated:")
    prettyOutput(update_files_json)
    print('')

    if add_files_json:
        print("added:")
        prettyOutput(add_files_json)
        for new_file in add_files_json:
            dst_file = op.join(DAILY_DATA_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config json files').Run()

    SvnCmd(path=DAILY_CFG_DIR, cmd='commit', logmsg='update config files').Run()


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
    copy2Daily()
    raw_input("Done!Press return key to exit.")
