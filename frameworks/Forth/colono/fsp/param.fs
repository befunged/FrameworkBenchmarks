\
\ Forth server pages - reading request parameters
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/hashtable.fs
require ../httpd/urldecode.fs
require ../httpd/conn.fs

256 fhashtable fsp-parameters

: request-parameter ( c-addr1 u1 -- c-addr2 u2 )
   2DUP fsp-parameters fht-get DUP IF
      2SWAP 2DROP EXIT
   THEN
   2DROP
   2DUP fsp-connection @ request-parameter-raw 0= IF
      2DROP 0 0 EXIT
   THEN
   DUP ALLOCATE THROW
   DUP >R urldecode R> SWAP

   2DUP 2>R
   fsp-parameters fht-put2
   2R>
;

: clear-parameters ( -- )
   fsp-parameters fht-clear
;
