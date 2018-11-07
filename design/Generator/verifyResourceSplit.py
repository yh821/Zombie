# -*- coding: utf-8 -*-
import traceback
from c import *
from checker.module.ResourceSplit.resource_split_mgr import ResourceSplitMgr


def main():
    os.putenv('PYTHONPATH', CUR_PATH)
    resourceSplit = ResourceSplitMgr(CUR_PATH)
    os.system('svn up "{}"'.format(resourceSplit.GetServerDataDir()))
    os.system('svn up "{}"'.format(resourceSplit.GetSourceDataDir()))
    resourceSplit.VerifyResourceSplit()
    print "verify finish!"

if __name__ == '__main__':
    try:
        main()
    except Exception, e:
        print '######################## Error Occurred ########################'
        print traceback.print_exc()
        print e
        assert False
