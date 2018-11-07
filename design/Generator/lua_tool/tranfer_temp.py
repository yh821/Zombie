# -*- coding: utf-8 -*-
import os
import chardet


TempPath = os.path.abspath('..')
curPath = os.path.join(TempPath, 'proto')

for root, dirs, names in os.walk(curPath):
    for filename in names:
        if filename.endswith('proto'):
            filename = os.path.join(curPath, filename)
            f1 = open(filename, 'rb')
            temp = f1.read()
            source_encoding = chardet.detect(temp)['encoding']
            f1.close()
            if source_encoding != "utf-8":
                print filename, source_encoding
                f2 = open(filename, 'wb')
                f2.write(temp.decode(source_encoding).encode('utf-8'))
                f2.close()
print "Finish"

