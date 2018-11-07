# -*- coding: utf-8 -*-

import os
from os import path as op
import shutil


class FileUtil(object):
    """文件操作工具"""

    @classmethod
    def get_files(cls, topPath, exts=None, filterOuts=('.svn', )):
        """取得指定目录下的所有文件,允许排除及后缀过滤"""
        def legalCheck(f):
            for filterOut in filterOuts:
                if filterOut in f:
                    return False
            return True

        def extCheck(f):
            if not exts:
                return True

            fileExt = op.splitext(f)[1]
            return fileExt in exts

        files = []
        for root, dirs, names in os.walk(topPath):
            for name in names:
                fullname = op.join(root, name)
                if not legalCheck(fullname) or \
                        not extCheck(fullname):
                    continue
                files.append(fullname)
        return files

    @classmethod
    def remove(cls, f, removeSelf=True):
        """移除 目录/文件"""
        if not op.exists(f):
            return

        if op.isdir(f):
            for root, dirs, names in os.walk(f, topdown=False):
                for name in names:
                    os.remove(op.join(root, name))
                for d in dirs:
                    shutil.rmtree(op.join(root, d))
            if removeSelf:
                os.removedirs(f)
        else:
            os.remove(f)

    @classmethod
    def copy(cls, src, dest, suffixes=None, filterouts=('.svn', ), force=False):
        """目录/文件 拷贝"""
        if not op.exists(src):
            raise Exception('source file/directory not exists: {}'.format(src))

        def _IsInFilterOuts(filepath):
            for fo in filterouts:
                if fo in filepath:
                    return True
            return False

        def _IsSuffixLegal(filepath):
            return True if (suffixes is None or op.splitext(filepath)[1] in suffixes) else False

        update_files = []
        add_files = []
        if op.isdir(src):
            if not op.isdir(dest):
                cls.remove(dest)
            if not op.exists(dest):
                os.makedirs(dest)

            for root, dirs, names in os.walk(src):
                for d in dirs:
                    srcSubdir = op.join(root, d)
                    if _IsInFilterOuts(srcSubdir):
                        continue
                    destSubdir = op.join(dest, srcSubdir[len(src) + 1:])
                    cls.copy(srcSubdir, destSubdir, suffixes, filterouts)

                for name in names:
                    srcFile = op.join(root, name)
                    if _IsInFilterOuts(srcFile):
                        continue
                    elif not _IsSuffixLegal(srcFile):
                        continue

                    destFile = op.join(dest, srcFile[len(src) + 1:])
                    if op.exists(destFile):
                        if cls.is_newer(srcFile, destFile) or force:
                            shutil.copyfile(srcFile, destFile)
                            update_files.append(srcFile)
                    else:
                        shutil.copyfile(srcFile, destFile)
                        add_files.append(srcFile)

        else:
            if op.isdir(dest):
                dest = op.join(dest, op.basename(src))
            shutil.copyfile(src, dest)
        return update_files, add_files

    @classmethod
    def move(cls, src, dest):
        """移动 目录/文件"""
        cls.copy(src, dest)
        cls.remove(src)

    @classmethod
    def is_newer(cls, file1, file2):
        """确认file1是否比file2更新(基于mtime)"""
        if not op.exists(file2):
            return True

        return os.stat(file1).st_mtime > os.stat(file2).st_mtime

    @classmethod
    def is_older(cls, file1, file2):
        """确认file1是否比file2更旧(基于mtime)"""
        if not op.exists(file1):
            return True

        return os.stat(file1).st_mtime < os.stat(file2).st_mtime
