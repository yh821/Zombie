# -*- coding: utf-8 -*-
from os import path as _op
from llbc import Singleton
from c import CUR_PATH


class CharInfo(object):
    def __init__(self):
        self.ch = u''
        self.spelling = u''
        self.tone = 0
        self.is_ascii = True

    @property
    def upper_case_spelling(self):
        if not self.spelling:
            return self.spelling

        if self.is_ascii:
            return self.spelling

        first_ch = self.spelling[0]
        if u'a' <= first_ch <= u'z':
            first_ch = chr(ord(first_ch) - 32).decode('utf-8')
            return first_ch + self.spelling[1:]
        else:
            return self.spelling


class SpellingConverter(Singleton):
    def __init__(self):
        super(SpellingConverter, self).__init__()
        self._chars = {}

        self._fill_ascii_data()
        self._parse_spelling_file()

    def get_char_info(self, ch):
        if not isinstance(ch, unicode):
            ch = ch.decode('utf-8')

        return self._chars.get(ch)

    def parse_str(self, string, to_upper_case=False):
        if not isinstance(string, unicode):
            string = string.decode('utf-8')

        ret_str = u''
        for ch in string:
            if to_upper_case:
                ret_str += self.get_char_info(ch).upper_case_spelling
            else:
                ret_str += self.get_char_info(ch).spelling

        return ret_str

    def _fill_ascii_data(self):
        for ascii_code in xrange(0, 128):
            ascii_info = CharInfo()
            ascii_info.ch = chr(ascii_code).decode('utf-8')
            ascii_info.spelling = ascii_info.ch

            self._chars[ascii_info.ch] = ascii_info

    def _parse_spelling_file(self):
        sp_path = _op.join(CUR_PATH, u'spelling_convert.spellingdata')
        with open(sp_path, 'r') as f:
            for line in f.readlines():
                parts = line.decode('utf-8').split(' ')
                chn_info = CharInfo()
                chn_info.ch = parts[0]
                chn_info.spelling = parts[1]
                if len(parts) > 2:
                    chn_info.tone = int(parts[2])

                chn_info.is_ascii = False

                self._chars[chn_info.ch] = chn_info



