echo 'clean .svn\wc.db'
sqlite3.exe .svn\wc.db "delete from work_queue;"
ping 1.1.0.0 -n 1 -w 2000