\
\ File download using GET
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/putresp.fs
require ../httpd/posix.fs
require ../httpd/os.fs
require ../httpd/string.fs
require ../httpd/uri.fs

windows? [IF]
require ../httpd/regmime.fs
[ELSE]
require ../httpd/mime.fs
[THEN]

2VARIABLE document-root
0. document-root 2!

\ Set document root
: DocumentRoot ( "path"  -- )
   BL WORD COUNT document-root clone-string!
;

: read-file-response ( fid conn-addr -- )
   TUCK response-buf-space ROT READ-FILE THROW
   SWAP 2DUP >response-buf-size +!
   >object-size +!
;

DEFER read-more

: close-or-set-handler ( fid conn-addr -- )
   DUP response-buf-full? IF
      ['] read-more OVER >'provide-response !
      >response-handler-area !
   ELSE
      >'cleanup ['] DROP SWAP !
      CLOSE-FILE THROW
   THEN
;

:NONAME ( conn-addr -- )
   DUP >response-handler-area @ SWAP 2DUP read-file-response
   close-or-set-handler
; IS read-more

\ Return the file extension including '.'
\ or '.' if there is no extension
: path-ext ( c-addr1 u1 -- c-addr1 u1 )
   S" ." SEARCH 0= IF
      2DROP S" ."
   THEN
;

\ Get the response MIME type from request-uri
: response-type ( c-addr1 u1 -- c-addr1 u1 )
   path-ext
   [DEFINED] reg-mime-type [IF]
      reg-mime-type
   [ELSE]
      1 /STRING mime-type
   [THEN]      
;

\ Convert absolute to relative path by removing leading slashes
: rel-path ( c-addr1 u1 -- c-addr2 u2 )
   BEGIN
      DUP 0= IF EXIT THEN
      OVER C@ [CHAR] / = WHILE
      1 /STRING
   REPEAT
;

: path-valid? ( c-addr u -- flag )
   S" .." SEARCH -ROT 2DROP 0=
;

1024 CONSTANT max-pathlen

3400 CONSTANT path-too-long

CREATE path-buf max-pathlen ALLOT

CREATE file-stat 88 ALLOT

: put-last-modified ( conn-addr -- )
   path-buf file-stat stat
   -1 <> IF
      DUP S" Last-Modified: " ROT put-response
      >R file-stat >st_mtime @ gmtime&date R@ put-time&date
      S\"  GMT\r\x0a" R> put-response
   ELSE
      DROP ." Error calling stat(): " 0 perror
   THEN
;

\ Concatenate docroot and filename, store resulting path (plus terminating zero)
\ in path-buf and open file
: open-server-file ( c-addr u -- fileid ior )
   \ Check length
   DUP document-root 2@ NIP + max-pathlen 1- > IF
      2DROP path-too-long EXIT
   THEN

   \ Concatenate and open file
   document-root 2@ path-buf SWAP CMOVE
   TUCK document-root 2@ NIP path-buf + SWAP CMOVE
   document-root 2@ NIP + path-buf SWAP
   2DUP + 0 SWAP C!
   R/O OPEN-FILE
;

\ Called when the connection is closed prematurely, closes the file
: cleanup-close-file ( conn-addr -- )
   >response-handler-area @ CLOSE-FILE DROP
;

: handle-GET-file ( conn-addr -- )
   DUP request-method S" GET" COMPARE IF
      DUP request-method S" HEAD" COMPARE IF
         status-method-not-allowed SWAP put-error EXIT
      THEN
   THEN
   DUP request-uri uri-path rel-path 2DUP path-valid? 0= IF
      2DROP status-bad-request SWAP put-error EXIT
   THEN
   
   open-server-file 0<> IF
      DROP status-not-found SWAP put-error EXIT
   THEN

   OVER status-ok SWAP put-status
   SWAP >R \ conn-addr is now on the return stack

   R@ put-date
   R@ put-server
   R@ put-last-modified
   R@ request-uri response-type R@ put-content-type
   DUP FILE-SIZE THROW R@ put-content-length
   S\" \r\x0a" R@ put-response

   ( fid )
   R@ request-method S" HEAD" COMPARE 0= IF
      R> DROP CLOSE-FILE THROW EXIT
   THEN

   R@ >'cleanup ['] cleanup-close-file SWAP !
   
   \ Read from file, trying to fill remaining buffer
   DUP R@ read-file-response

   R> close-or-set-handler
;
