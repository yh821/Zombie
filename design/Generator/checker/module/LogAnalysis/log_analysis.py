# -*- coding: utf-8 -*-
import traceback
from c import *


class LogAnalysis(object):
    def __init__(self):
        pass

    def Analysis(self, logPath):
        f = file(logPath)
        if not f:
            f.close()
            return

        lines = f.readlines()
        dictKeyLogCount = {}
        minCutLen = 15
        lowerLen = 10
        for lineLog in lines:
            splitStr = lineLog.split('[:0] - ')
            if len(splitStr) == 2:
                effectStr = splitStr[1]
                splitStr = effectStr.split(' ')
                logKey = splitStr[0] if len(splitStr[0]) > lowerLen else effectStr[0:minCutLen]
                if not dictKeyLogCount.get(logKey):
                    dictKeyLogCount[logKey] = 1
                else:
                    dictKeyLogCount[logKey] += 1

        sortList = sorted(dictKeyLogCount.iteritems(), key=lambda asd: asd[1], reverse=True)
        maxLen = 50
        for v in sortList:
            print v
            maxLen -= 1
            if maxLen <= 0:
                break


if __name__ == '__main__':
    try:
        os.putenv('PYTHONPATH', CUR_PATH)
        logPath = CUR_PATH + '/scene_server.log'
        logAnalysis = LogAnalysis()
        logAnalysis.Analysis(logPath)
    except Exception, e:
        print '######################## Error Occurred ########################'
        print traceback.print_exc()
        print e
        raw_input('Check this error please, then press any key to exit...')
        assert False