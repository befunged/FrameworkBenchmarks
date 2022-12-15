\
\ Words for managing connections, buffers etc.
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/posix.fs
require ../httpd/match.fs
require ../httpd/error.fs
require ../httpd/os.fs

512 CONSTANT max-connections

22 CELLS sockaddr-size + CONSTANT conn-entry-size

512 CONSTANT initial-buf-size

1024 1024 * CONSTANT max-request-buf-size

\ The request read timeout, in seconds
60 VALUE request-read-timeout

\ The keepalive timeout, in seconds
15 VALUE keep-alive-timeout

: >conn-fd ( addr -- addr )
;

: >request-buf ( conn-addr -- a-addr )
CELL + ;

: >request-buf-size ( conn-addr -- a-addr )
2 CELLS + ;

: >request-buf-pos ( conn-addr -- a-addr )
3 CELLS + ;

: >response-buf ( conn-addr -- a-addr )
4 CELLS + ;

: >response-buf-size ( conn-addr -- a-addr )
5 CELLS + ;

: >response-buf-pos ( conn-addr -- a-addr )
6 CELLS + ;

: >response-buf-capacity ( conn-addr -- a-addr )
7 CELLS + ;

\ xt called to provide new response data with a connection address on the stack.
\ after the response buffer was written
: >'provide-response ( conn-addr -- a-addr )
8 CELLS + ;

: >timeout ( conn-addr -- a-addr )
9 CELLS + ;

\ xt called on connection close with a connection address on the stack.
\ Default is drop. When it was called, the default is restored.
: >'cleanup ( conn-addr -- a-addr )
10 CELLS + ;

: >process-next-request ( conn-addr -- a-addr )
11 CELLS + ;

\ Space of 8 cells freely usable by the handler
: >response-handler-area ( conn-addr -- a-addr )
12 CELLS + ;

: >status-code ( conn-addr -- a-addr )
20 CELLS + ;

: >object-size ( conn-addr -- a-addr )
21 CELLS + ;

\ Client address
: >sockaddr ( conn-addr -- a-addr )
22 CELLS + ;

\ The response buffer does not grow
1024 16 * VALUE response-buf-capacity

CREATE sockaddr sockaddr-size ALLOT

: crlfcrlf ( -- c-addr u )
   S\" \r\x0a\r\x0a"
;

: connection[] ( -- ) CREATE
max-connections conn-entry-size * ALLOT
DOES> ( n -- conn-addr ) SWAP conn-entry-size * + ;

connection[] connection

: init-connections ( -- )
   max-connections 0 DO
      -1 I connection >conn-fd !
   LOOP
;

: >pollfd-events ( addr -- addr )
CELL + ;

: >pollfd-revents ( addr -- addr )
CELL + 2 + ;

\ 1 Entry for each connection entry plus 1 for the listen socket
: pollfd[] ( -- ) CREATE
max-connections 1+ 2 CELLS * ALLOT
DOES> ( n -- addr ) SWAP 2 CELLS * + ;

pollfd[] pollfd

: find-free-conn ( -- n flag )
   max-connections 0 DO
      I connection >conn-fd @ -1 = IF
         I TRUE UNLOOP EXIT
      THEN
   LOOP
   -1 FALSE
;

: add-connection ( fd -- )
   find-free-conn IF
      initial-buf-size ALLOCATE
      IF
         ." No memory available for new connection." cr
         2DROP close DROP
      ELSE
         SWAP connection
         DUP conn-entry-size ERASE
         TUCK
         >request-buf !
         TUCK
         >conn-fd !
         DUP >request-buf-size initial-buf-size SWAP !
         0 time request-read-timeout + OVER >timeout !
         DUP ['] DROP SWAP >'cleanup !
         sockaddr SWAP >sockaddr sockaddr-size CMOVE
      THEN
   ELSE
      ." Maximum # of connections reached." cr
      DROP close DROP
   THEN
;

S" MAX-N" ENVIRONMENT? 0= [IF] ABORT" Unable to retrieve MAX-N" [THEN]
CONSTANT time-max

\ Get the next connection timeout
: next-timeout ( -- n )
   time-max
   max-connections 0 DO
      I connection >conn-fd @ -1 > IF
         I connection >timeout @ MIN
      THEN 
   LOOP
;

: close-conn ( conn-addr -- )
   \ Call cleanup xt and reset it
   DUP DUP >'cleanup @ EXECUTE
   ['] DROP OVER >'cleanup !

   \ Close fd and set it to -1, marking the entry as unused
   DUP >conn-fd DUP @ close DROP -1 SWAP !

   \ Free request buffer
   DUP >request-buf-size @ 0> IF
      >request-buf @ FREE DROP
   ELSE
      DROP
   THEN
;

: request-buf-full? ( conn-addr -- )
   DUP >request-buf-size @
   SWAP >request-buf-pos @
   =
;

: extend-buffer ( conn-addr -- flag )
   DUP >request-buf-size max-request-buf-size >= IF
      ." request buffer exceeded"
      DROP FALSE EXIT
   THEN
   DUP DUP >request-buf @ SWAP >request-buf-size @ 2* max-request-buf-size MIN TUCK RESIZE
   IF 2DROP FALSE EXIT THEN
   >R OVER >request-buf R> SWAP !
   SWAP >request-buf-size !
   TRUE
;

: request-buf ( conn-addr -- addr u )
   DUP >request-buf @ SWAP
   >request-buf-pos @
;

: request-method ( conn-addr -- c-addr u )
   DUP request-buf S"  " SEARCH DROP
   NIP SWAP request-buf ROT -
;

: request-uri ( conn-addr -- c-addr u )
   request-buf S"  " SEARCH 0= IF 
      DROP 0 EXIT
   THEN
   1 /STRING
   2DUP S"  " SEARCH DROP
   NIP -
;

: request-line ( conn-addr -- c-addr u )
   request-buf 2DUP S\" \r\x0a" SEARCH DROP
   NIP -
;

: request-query ( conn-addr -- c-addr u )
   request-line S" ?" SEARCH IF
      1 /STRING
      2DUP S"  " SEARCH IF
         NIP -
      ELSE
         2DROP
      THEN
   ELSE
      DROP 0
   THEN
;

: request-http-version ( conn-addr -- c-addr u )
   request-buf S"  " SEARCH 0= IF 
      DROP 0 EXIT
   THEN
   1 /STRING
   S"  " SEARCH 0= IF
      DROP 0 EXIT
   THEN
   1 /STRING
   2DUP S\" \r\x0a" SEARCH DROP
   NIP -
;

: request-free-buf ( conn-addr -- addr u )
   DUP DUP >request-buf @ SWAP >request-buf-pos @ + SWAP
   DUP >request-buf-size @ SWAP >request-buf-pos @ -
;

: request-body-expected? ( conn-addr -- flag )
   request-method 2DUP S" POST" COMPARE 0= IF
      2DROP TRUE EXIT
   THEN
   2DUP S" PUT" COMPARE 0= IF
      2DROP TRUE EXIT
   THEN
   S" TRACE" COMPARE 0=
;

: request-header-complete? ( conn-addr -- flag )
   request-buf crlfcrlf SEARCH -ROT 2DROP
;

\ Return the request headers, including the Request-Line
\ and also the trailing cr-lf-cr-lf sequence if present.
: request-headers ( conn-addr -- c-addr u )
   request-buf 2DUP crlfcrlf SEARCH IF
      NIP - 4 +
   ELSE
      2DROP
   THEN
;

CREATE name-pattern 13 C, 10 C, 126 ALLOT

: header-pattern ( c-addr u -- u )
   \ Copy header name and append ':'
   TUCK name-pattern 2 + SWAP CMOVE
   2 + [CHAR] : OVER name-pattern + C! 1+
;

\ Remove leading blanks and control characters
: -leading+ ( c-addr1 u1 -- c-addr2 u2 )
   BEGIN
      DUP 0= IF EXIT THEN
      OVER C@ BL <= WHILE
      1 /STRING
   REPEAT
;

\ Get header value by name
: request-header ( conn-addr c-addr1 u1 -- c-addr2 u2 TRUE | FALSE )
   header-pattern

   \ Search for header
   TUCK SWAP request-headers ROT name-pattern SWAP search(nc)
   0= IF
      2DROP DROP FALSE EXIT
   THEN
   
   \ Search for terminating CR/LF
   ROT /STRING 2DUP S\" \r\x0a" SEARCH 0= IF
      2DROP 2DROP FALSE
   THEN
   NIP -
   -leading+ -TRAILING TRUE
;

: request-length ( conn-addr -- u )
   DUP request-body-expected? IF
      DUP S" Content-Length" request-header IF
         0. 2SWAP >NUMBER 2DROP D>S
         SWAP request-headers NIP +
      ELSE
         request-headers NIP
      THEN
   ELSE
      request-headers NIP
   THEN
;

: request-data ( conn-addr -- addr u )
   DUP request-length SWAP request-buf ROT MIN
;

: request-complete? ( conn-addr -- flag )
   DUP request-body-expected? IF
      DUP request-header-complete? IF
         DUP request-length
         SWAP >request-buf-pos @ <=
      ELSE
         DROP FALSE
      THEN
   ELSE
      request-header-complete?
   THEN
;

: move>response ( c-addr u conn-addr -- )
   2>R 2R@
   DUP >response-buf @ SWAP >response-buf-size @ +
   SWAP CMOVE
   2R> >response-buf-size +!
;

: response-buf-space ( conn-addr -- addr u )
\ Return the space between response-buf-size and response-buf-capacity
   DUP >response-buf @ SWAP DUP >response-buf-capacity @ SWAP >response-buf-size @ /STRING
;

: response-buf-full? ( conn-addr -- flag )
   DUP >response-buf-size @ SWAP >response-buf-capacity @ =
;

: request-uri-valid? ( c-addr u -- flag )
   DUP 0= IF
      2DROP FALSE
   ELSE
      \ Currently only an absolute path is permitted
      DROP C@ [CHAR] / =
   THEN
;

: response-data-ready? ( conn-addr -- flag )
   DUP >response-buf-pos @ SWAP >response-buf-size @ <
;

: response-unwritten ( conn-addr -- addr u )
   DUP DUP >response-buf @ SWAP >response-buf-pos @ + SWAP
   DUP >response-buf-size @ SWAP >response-buf-pos @ -
;

: search-parameter ( c-addr1 u1 c-addr2 u2 -- FALSE | c-addr3 u3 TRUE )
   2>R
   BEGIN
      2R@ SEARCH 0= IF 2DROP 2R> 2DROP FALSE EXIT THEN
      DUP 0= IF 2DROP 2R> 2DROP FALSE EXIT THEN
      2R@ NIP /STRING OVER C@ [CHAR] = = -ROT
      1 /STRING
   ROT UNTIL
   2R> 2DROP TRUE
;

: buf-get-parameter ( c-addr1 u1 c-addr2 u2 -- FALSE | c-addr3 u3 TRUE )
   search-parameter
   0= IF FALSE EXIT THEN
   2DUP S" &" SEARCH IF
      NIP -
   ELSE
      2DROP
   THEN
   TRUE
;

: request-parameter-raw ( c-addr1 u1 conn-addr -- FALSE | c-addr2 u2 TRUE )
   DUP request-method 2DUP S" POST" COMPARE 0= IF
      2DROP
      request-data 2SWAP buf-get-parameter
   ELSE
      2DUP S" GET" COMPARE IF
         2DUP S" HEAD" COMPARE IF
            2DROP DROP 2DROP FALSE EXIT
         THEN
      THEN
      2DROP
      request-query 2SWAP buf-get-parameter
   THEN
;
