# -*- coding: utf-8 -*-
import os


class SvnCmd(object):
    def __init__(self, path, cmd, logmsg=''):
        self._path = path
        self._cmd = cmd
        self._logmsg = logmsg

    def Run(self):
        os.system('TortoiseProc /command:{} /path:{} '
                  '/logmsg:"{}" /closeonend:0'.format(self._cmd, self._path, self._logmsg))
