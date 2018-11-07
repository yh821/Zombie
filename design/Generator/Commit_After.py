# -*- coding: utf-8 -*-
import pysvn
import os
from os import path as op
from file_util import FileUtil


CUR_DIR = op.dirname(op.abspath(__file__))
ROOT_DIR = op.dirname(op.dirname(CUR_DIR))
SERV_VERSION_DIR = op.join(ROOT_DIR, 'server', 'version')
GENED_VERSION_DIR = op.join(CUR_DIR, 'version')

dataPath = op.join(ROOT_DIR, 'design', 'run', 'data')
msgPath = op.join(ROOT_DIR, 'msg', 'proto')

# curPath = os.getcwd()
# lastName = curPath.split("\\")[-1]
# if lastName == "design":
#     dataPath = os.path.join(curPath, "run", "data")
#     msgPath = op.join(op.dirname(curPath), "msg", "proto")
# elif lastName == "msg":
#     dataPath = op.join(op.dirname(curPath), "design", "run", "data")
#     msgPath = op.join(curPath, "proto")
# else:
#     dataPath = ""
#     msgPath = ""
allPath = [dataPath, msgPath]


class SvnCmd(object):
    def __init__(self, path, cmd, logmsg=''):
        self._path = path
        self._cmd = cmd
        self._logmsg = logmsg

    def Run(self):
        os.system('TortoiseProc /command:{} /path:{} '
                  '/logmsg:"{}" /closeonend:0'.format(self._cmd, self._path, self._logmsg))


def main():
    if not dataPath or (not msgPath):
        return

    svn = pysvn.Client()

    versionStr = ""
    lenList = len(allPath)
    for index, tPath in enumerate(allPath):
        LogList = svn.log(tPath, limit=1)
        info = LogList[0]
        versionNum = info.revision.number
        if index == (lenList - 1):
            versionStr += str(versionNum)
        else:
            versionStr += str(versionNum)
            versionStr += "."

    exportPath1 = op.join(op.dirname(dataPath), "version")
    exportPath = op.join(exportPath1, 'version.txt')

    f = open(exportPath, 'w')
    f.write(versionStr)
    f.close()

    SvnCmd(path=exportPath, cmd='commit', logmsg='Commit version text.{}||{}'.format(exportPath1, SERV_VERSION_DIR)).Run()

    # copy version 到 trunk
    SvnCmd(path=SERV_VERSION_DIR, cmd='update', logmsg='update version files.').Run()

    update_files_version, add_files_version = FileUtil.copy(exportPath1, SERV_VERSION_DIR, suffixes=('.txt', ), force=True)

    # print("{} .txt files {}".format("#" * 16, "#" * 16))
    # print("updated:")
    # # prettyOutput(update_files_version)
    # print('')

    if add_files_version:
        print("added:")
        prettyOutput(add_files_version)
        for new_file in add_files_version:
            dst_file = op.join(SERV_VERSION_DIR, op.basename(new_file))
            SvnCmd(path=dst_file, cmd='add', logmsg='add new config data files').Run()

    SvnCmd(path=SERV_VERSION_DIR, cmd='commit', logmsg='commit version files').Run()

if __name__ == '__main__':
    main()
