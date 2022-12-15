\
\ Listening for and accepting TCP connections
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/string.fs
require ../httpd/match.fs
require ../httpd/posix.fs
require ../httpd/os.fs
require ../httpd/error.fs
require ../httpd/nonblock.fs

50 VALUE backlog

2VARIABLE listen-address&port
0. listen-address&port 2!

\ Set listen address and port
: ListenAddress&Port ( "[host:]port"  -- )
   BL WORD COUNT listen-address&port clone-string!
;

VARIABLE addrinfo

CREATE hints 8 CELLS ALLOT

: /hints ( n n n -- )
   hints 8 CELLS ERASE
   hints >ai_socktype !
   hints >ai_family !
   hints >ai_flags !
;

VARIABLE optval


: bind-addr ( addr -- socket flag )
   \ Call socket() and bind() with data from addr
   DUP >ai_family @ SWAP
   DUP >ai_socktype @ SWAP
   DUP >ai_protocol @ SWAP
   >R socket R> SWAP DUP 0< IF 2DROP -1 FALSE EXIT THEN

   optval ON
   DUP SOL_SOCKET SO_REUSEADDR optval CELL setsockopt 
   0< IF DROP setsockopt-failed THROW THEN

   DUP ROT DUP >ai_addr @
   SWAP >ai_addrlen @
   bind 0< IF
      close DROP -1 FALSE EXIT
   THEN
   TRUE
;

\ Convert a string in the format [host:]service to zero-terminated host and service.
\ If no host is given, z-addr1 will be zero.
: node&servicez ( caddr u -- z-addr1 z-addr2 )
   2DUP [CHAR] : csearchlast IF
      2SWAP 2 PICK -
      2SWAP 1 /STRING 2SWAP
      $copyz
   ELSE
      2DROP 0
   THEN
   -ROT $copyz
;

: server-listen ( -- listenfd )
   AI_PASSIVE AF_UNSPEC SOCK_STREAM /hints

   listen-address&port 2@ DUP 0= IF
      2DROP S" 4004"
   THEN
   node&servicez 2DUP hints addrinfo getaddrinfo
   -ROT FREE THROW ?DUP IF FREE THROW THEN
   0<> IF getaddrinfo-failed THROW THEN

   addrinfo @
   \ Go through list until bind-addr succeeds
   BEGIN
      DUP bind-addr
      0= WHILE
      DROP >ai_next @ DUP 0= IF
         ." Error calling bind() or socket(): " 0 perror bind-failed THROW
      THEN
   REPEAT
   NIP

   DUP backlog listen
   0<> IF close listen-failed THROW THEN
   DUP +nonblocking
;
