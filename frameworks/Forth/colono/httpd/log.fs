\
\ Logging
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

[UNDEFINED] required [IF] S" compat/required.fs" INCLUDED [THEN]

require ../httpd/posix.fs
require ../httpd/os.fs
require ../httpd/putresp.fs
require ../httpd/time.fs

FALSE VALUE access-logging

0 VALUE access-log-fid

\ Try to open file write-only, if it does not exist create it
: open-or-create-file ( c-addr u -- fileid ior )
    2DUP
    W/O OPEN-FILE 0= IF
       -ROT 2DROP
       DUP FILE-SIZE DUP 0<> IF >R 2DROP R> EXIT THEN
       DROP 2 PICK REPOSITION-FILE EXIT
    THEN
    DROP W/O CREATE-FILE
;

: AccessLog ( "logfile" -- )
   BL WORD COUNT open-or-create-file 0= IF
      TO access-log-fid
      TRUE TO access-logging
   THEN
;

64 CONSTANT timebuf-size
CREATE timebuf timebuf-size ALLOT

: common-time&date ( n -- c-addr u )
   tmp-time !
   tmp-time localtime
   >R timebuf timebuf-size S\" %d/%b/%Y:%H:%M:%S %z\0" DROP R> strftime
   DUP 0<> IF 1- THEN
   timebuf SWAP
;

INET_ADDRSTRLEN INET6_ADDRSTRLEN MAX CONSTANT addrstrlen

CREATE addrstrbuf addrstrlen ALLOT

: sockaddr_ntop ( sockaddr -- c-addr )
   DUP W@ AF_INET = IF
      AF_INET SWAP >sin_addr addrstrbuf addrstrlen inet_ntop
   ELSE
      AF_INET6 SWAP >sin6_addr addrstrbuf addrstrlen inet_ntop
   THEN
;

: log-access ( conn-addr -- )
   access-logging IF
      DUP >sockaddr sockaddr_ntop DUP 
      0= IF
         S" -" access-log-fid WRITE-FILE THROW DROP
      ELSE
         DUP strlen access-log-fid WRITE-FILE THROW
      THEN
      S"  - - [" access-log-fid WRITE-FILE THROW
      0 time common-time&date access-log-fid WRITE-FILE THROW
      S\" ] \"" access-log-fid WRITE-FILE THROW
      DUP request-line access-log-fid WRITE-FILE THROW
      S\" \" " access-log-fid WRITE-FILE THROW
      DUP >status-code @ 0 <# # # # #> access-log-fid WRITE-FILE THROW
      S"  " access-log-fid WRITE-FILE THROW
      >object-size @ 0 <# #S #> access-log-fid WRITE-LINE THROW
      access-log-fid FLUSH-FILE THROW
   ELSE
      DROP
   THEN
;
