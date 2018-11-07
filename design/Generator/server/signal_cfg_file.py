# -*- coding: utf-8 -*-
import config


class SignalCfgFile(object):
    def Reload(self):
        if hasattr(config.ResActorCfg(), 'Reload'):
            config.ResActorCfg().Reload()
        if hasattr(config.ResExpCfg(), 'Reload'):
            config.ResExpCfg().Reload()
        if hasattr(config.ResSceneCfg(), 'Reload'):
            config.ResSceneCfg().Reload()
        if hasattr(config.ResSkillCfg(), 'Reload'):
            config.ResSkillCfg().Reload()

