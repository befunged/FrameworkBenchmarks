\
\ URI path extraction
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/urldecode.fs
require ../httpd/error.fs

4096 CONSTANT uri-path-buffer-size

CREATE uri-path-buffer uri-path-buffer-size ALLOT

: uri-path ( c-addr1 u1 -- c-addr2 u2 )
   S" ://" SEARCH IF
      3 /STRING
      S" /" SEARCH DROP
   THEN
   2DUP
   S" ?" SEARCH IF
      NIP -
   ELSE
      2DROP
   THEN
   DUP uri-path-buffer-size > IF uri-path-too-long THROW THEN
   uri-path-buffer urldecode
   uri-path-buffer SWAP
;
