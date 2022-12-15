\
\ Time and date
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/posix.fs

VARIABLE tmp-time

: gmtime&date ( time_t -- u1 u2 u3 u4 u5 u6 )
   tmp-time !
   tmp-time gmtime
   DUP >tm_sec @
   SWAP DUP >tm_min @
   SWAP DUP >tm_hour @
   SWAP DUP >tm_mday @
   SWAP DUP >tm_mon @ 1+
   SWAP >tm_year @ 1900 +
;

CREATE months ,"  Jan  Feb  Mar  Apr  May  Jun  Jul  Aug  Sep  Oct  Nov  Dec "

: month ( u1 -- caddr u2 )
   months 1+ SWAP 1- 5 * + 5
;
