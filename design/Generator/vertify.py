# -*- coding: utf-8 -*-
import subprocess
import traceback

from c import *
from checker.verifydata import VerifyData

def main():
    os.putenv('PYTHONPATH', CUR_PATH)
    vertifydata = VerifyData(CUR_PATH)
    vertifydata.loadData()
    if vertifydata.verify():
    # raw_input('It is finish!')
        assert False
    else:
        print "vertify success!!!"

if __name__ == '__main__':
    try:
        main()
    except Exception, e:
        print '######################## Error Occurred ########################'
        print traceback.print_exc()
        print e
        # raw_input('Check this error please, then press any key to exit...')
        assert False
