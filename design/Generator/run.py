# -*- coding: utf-8 -*-
import subprocess
import traceback

from c import *
from file_util import FileUtil
from finder import CfgFinder
from recognizer import WBRecognizer
from exporter import ProtoExporter, DataExporter
from svn_cmd import SvnCmd
#from checker.verifydata import VerifyData
import pysvn


def _parse_input(ids):
    parsed_ids = set()
    for cfg_id in [item.strip() for item in ids.split(',') if item.strip()]:
        if '-' in cfg_id:
            m, n = [int(val) for val in cfg_id.split('-')[0:2]]
            if m > n:
                m, n = n, m
            parsed_ids.update(range(m, n + 1))
        else:
            parsed_ids.add(int(cfg_id))
    sorted = list(parsed_ids)
    sorted.sort()
    return sorted


def _get_files_from_input():
    #list_all = raw_input('list all(y/n)?').strip().lower()
    #list_all = True if (list_all and (list_all == 'y' or int(list_all))) else False
    list_all = True

    finder = CfgFinder()
    finder.find(op.dirname(CUR_PATH) + '\\Excel', only_changed=not list_all)
    finder.list_finded()

    files = []
    ipt = raw_input(u'输入导出的配置序号(例如: 1,3,4-8):'.encode(
        SHOWABLE_CODING))
    # ipt = '1'
    for cfg_id in _parse_input(ipt):
        cfg_file = finder.get_by_id(cfg_id)
        if cfg_file is None:
            print u'找不到对应序号文件, index: {}'.format(cfg_id)
        else:
            files.append(cfg_file)

    return files


def _get_files_from_argv():
    argv = [arg.decode(FS_CODING) for arg in sys.argv[1:]]
    return [filename for filename in argv if WBRecognizer.is_like_cfg(filename)]


def _get_files():
    if len(sys.argv) == 1:
        return _get_files_from_input()
    else:
        return _get_files_from_argv()


def _trigger(cfg_file, is_after):
    base_name = op.basename(cfg_file)
    trigger_cfgs = AFTER_TRIGGER_FILES if is_after else PRE_TRIGGER_FILES
    for trigger_cfg in trigger_cfgs:
        if trigger_cfg[0] == base_name:
            script = op.join(TRIGGER_PATH, trigger_cfg[1])
            os.system(u'python {}'.format(script))
            print '[Trigger]       {}Trigger, Cfg:{}, Script:{}' \
                .format('After' if is_after else 'Pre',
                        base_name.encode(SHOWABLE_CODING),
                        trigger_cfg[1].encode(SHOWABLE_CODING))


def _checkGenPy():
    # 只需要检测__init__.py
    checkFiles = '__init__.py'
    filename = os.path.join(SERVER_PATH, checkFiles)
    subprocess.check_call(["python", filename])


def _checkDataFiles():
    for root, dirs, names in os.walk(DATA_PATH):
        for name in names:
            srcFile = os.path.join(root, name)
            if os.stat(srcFile).st_size <= 0:
                raise Exception(
                    "data file can NOT be 0 byte size, file:{}".format(srcFile))


def _get_svnadd_files(allReco):
    os.system("svn st {} > svnlog.txt".format(CUR_PATH.encode(FS_CODING) + '\\'))
    svnStats = []
    cwdir = os.getcwd()
    svnlogPath = cwdir + "\\svnlog.txt"
    f = open(svnlogPath, "r")
    if f:
        for line in f:
            if line[0] == '?':
                svnStats.append(line)
    else:
        print "can't open svnlog.txt. maybe not exist"
        os.system("pause")
    f.close()
    os.remove(svnlogPath)

    addFiles = []  # 所需检查是否要添加的xml表名
    newAddFiles = []  # 所需检查是否要添加的完整文件名
    addFilePath = []  # 所需添加完整路径文件名
    for reco in allReco:
        for k in reco._name2sheet.keys():
            addFiles.append(reco._name2sheet[k]._converted_name)

    if addFiles:
        for addFile in addFiles:
            newAddFilepy = addFile + "_pb2.py"
            newAddFilecfgpy = addFile + "Cfg.py"
            newAddFile1data = addFile + ".data"
            newAddFile2data = "cc_" + addFile + ".bytes"
            newAddFilecs = addFile + ".cs"
            newAddFilejson = addFile + ".json"
            newAddFileDebug = addFile + ".debug"
            newAddFiles.append(newAddFilepy)
            newAddFiles.append(newAddFilecfgpy)
            newAddFiles.append(newAddFile1data)
            newAddFiles.append(newAddFile2data)
            newAddFiles.append(newAddFilecs)
            newAddFiles.append(newAddFilejson)
            newAddFiles.append(newAddFileDebug)

    if newAddFiles:
        for parent, dirnames, filenames in os.walk(CUR_PATH.encode(FS_CODING)):
            for name in filenames:
                for newAddFile in newAddFiles:
                    if newAddFile.encode('utf-8') == name:
                        # addFilePath.append(os.path.join(parent, name))
                        filePath = os.path.join(parent, name)
                        for svnStat in svnStats:
                            if filePath in svnStat:
                                addFilePath.append(filePath)

    return addFilePath


def _generateSignalCfgFile(path):
    targetPath = path +'/server'
    # print '*************************', targetPath, '*************************'
    cfgNames = _getFileList(targetPath)
    # for value in fileNames:
    #     print value
    print 'The cfg file num is: ', len(cfgNames)
    from os import path as _op
    fileName = _op.join(targetPath, u'signal_cfg_file.py')
    f = _F(fileName)
    w = f.w
    # Generate enum code file
    w('# -*- coding: utf-8 -*-')
    w('import config')
    w('')
    w('')
    w('class SignalCfgFile(object):')
    w('    def Reload(self):')
    for cfgName in cfgNames:
        w('        if hasattr(config.{}(), \'Reload\'):'.format(cfgName))
        w('            config.{}().Reload()'.format(cfgName))
    w('')
    print 'Success to generate the signalCfgFile!!!'


def _getFileList(findPath):
    fileList=[]
    FileNames=os.listdir(findPath)
    for fileName in FileNames:
        if fileName.endswith('Cfg.py'):
            fileList.append(fileName[0:len(fileName) - 3])
    return fileList


def main():
    # set PYTHONPATH
    os.putenv('PYTHONPATH', CUR_PATH)

    # Get all need generate config files.
    files = _get_files()
    if not files:
        return

    allReco = []
    # Execute generate(based on user input or argv)
    for cfg_file in files:
        # Before trigger
        _trigger(cfg_file, False)

        # Recognize config file
        reco = WBRecognizer()
        reco.recognize(cfg_file)
        allReco.append(reco)
        # Export proto
        pto_exporter = ProtoExporter(reco)
        pto_exporter.export(PROTO_PATH, CLIENT_PATH, SERVER_PATH)

        # Check generated py files legality
        _checkGenPy()

        # Export data
        data_exporter = DataExporter(reco)
        data_exporter.export(DATA_PATH, SERVER_PATH)

        # 貌似没啥必要检测这个，只要_checkGenPy正常，下面的检测肯定通过
        # 如果仍有例外情况，再放开这个检测
        # _checkDataFiles()

        # After trigger
        _trigger(cfg_file, True)

    # Remove proto directory, if need.
    if not DONT_DEL_PROTO:
        FileUtil.remove(PROTO_PATH)

    # Commit changed and add files.
    print 'cur_path:', CUR_PATH.encode(FS_CODING),"\n\n"

    addFilePath = _get_svnadd_files(allReco)
    if addFilePath:
        for i in addFilePath:
            os.system("svn add {}".format(i))

    # SvnCmd(path=op.dirname(CUR_PATH.encode(FS_CODING) + '/'), cmd='add', logmsg='add
    # files').Run()

    # 2016年1月26日16:42:52 wizard加生成记录所有cfg文件的逻辑
    _generateSignalCfgFile(CUR_PATH)

    flag = False
    #vertifydata = VerifyData(CUR_PATH)
    #vertifydata.loadData()
    #flag = vertifydata.verify()
    if flag:
        choice = raw_input('\n\nThe cfgData exist error! Do you really want to ignore it to commit(Y(y)/N(n))?').strip().lower()
        choice = True if (choice == "y") else False
        userDecision(choice)
        # if choice:
        #     print "You chose to commit!"
        #     SvnCmd(path=op.dirname(CUR_PATH.encode(FS_CODING) + '/'), cmd='commit',
        #            logmsg='update excel files and config files').Run()
        #     raw_input('It is finish!')
        # else:
        #     print "You chose to terminate commit!"
        #     raw_input('It is finish!')
            # 2015年9月25日10:50:34 注释下面语句,测试用
            # SvnCmd(path=op.dirname(CUR_PATH.encode(FS_CODING) + '/'), cmd='commit',
            #        logmsg='update excel files and config files').Run()
            # raw_input('It is finish!')
    else:
        choice = raw_input('\n\nDo you want to commit(y/n)?').strip().lower()
        choice = True if (choice == "y") else False
        userDecision(choice)


def userDecision(choice):
    if choice:
        print u"你选择了提交!"
        SvnCmd(path=op.dirname(CUR_PATH.encode(FS_CODING) + '/'), cmd='commit',
               logmsg='update excel files and config files').Run()

        # 检查data_version.txt 进行修改提交
        #runDir = op.dirname(op.abspath(__file__))
        #ROOT_DIR = op.dirname(op.dirname(runDir))
        #dataPath = op.join(runDir, 'data')
        #dataVersionFile = op.join(runDir, 'version', 'data_version.txt')
        #re = op.exists(dataVersionFile)
        #svn = pysvn.Client()
        #SvnCmd(path=dataVersionFile, cmd='update', logmsg='update data version files').Run()

        #if not re:
        #    fWrite = open(dataVersionFile, 'w')
        #    LogList = svn.log(dataPath, limit=1)
        #    info = LogList[0]
        #    versionNum = info.revision.number

        #    fWrite.write(str(versionNum))
        #    fWrite.close()

        #    SvnCmd(path=dataVersionFile, cmd='add', logmsg='add new data version txt files').Run()
        #    SvnCmd(path=dataVersionFile, cmd='commit', logmsg='Commit data version text.').Run()
        #else:
        #    fRead = open(dataVersionFile, 'r')
        #    data = fRead.read()
        #    fRead.close()

        #    logList = svn.log(dataPath, limit=1)
        #    info = logList[0]
        #    newVersion = info.revision.number
        #    if newVersion != int(data):
        #        fWrite = open(dataVersionFile, 'w')
        #        LogList = svn.log(dataPath, limit=1)
        #        info = LogList[0]
        #        versionNum = info.revision.number

        #        fWrite.write(str(versionNum))
        #        fWrite.close()

        #        SvnCmd(path=dataVersionFile, cmd='commit', logmsg='Commit data version text.').Run()

        raw_input('It is finish!')
    else:
        print u"你放弃了提交!"
        raw_input('It is finish!')


class _F(object):
    def __init__(self, file_name, mode='w'):
        self._f = open(file_name, mode)

    def w(self, line):
        self._f.write(line.encode('utf-8') if isinstance(line, unicode) else str(line) + '\n')

    def done(self):
        self._f.close()

if __name__ == '__main__':
    try:
        main()
    except Exception, e:
        print '######################## Error Occurred ########################'
        print traceback.print_exc()
        print e

        raw_input('Check this error please, then press any key to exit...')

