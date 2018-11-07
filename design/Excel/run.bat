@echo 'run ...'
python .\\../Generator//run.py
if not %ERRORLEVEL% == 0 goto Error
@echo 'done'
goto End

:Error
echo ">>> [ERROR]run error,please check"
goto End

:End
pause