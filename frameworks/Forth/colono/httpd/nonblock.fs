\
\ Non-blocking I/O
\
\ Copyright (c) 2016,2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/posix.fs
require ../httpd/os.fs

windows? [IF]
VARIABLE ioctlsocket-arg
1 ioctlsocket-arg !

: +nonblocking ( socket -- )
   FIONBIO ioctlsocket-arg ioctlsocket
   -1 = IF fcntl-failed THROW THEN
;
[ELSE]
: +nonblocking ( socket -- )
   DUP F_GETFL 0 fcntl
   DUP -1 = IF fcntl-failed THROW THEN
   O_NONBLOCK OR F_SETFL SWAP fcntl
   -1 = IF fcntl-failed THROW THEN
;
[THEN]
