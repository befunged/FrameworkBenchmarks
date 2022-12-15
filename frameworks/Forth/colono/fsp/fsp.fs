\
\ Forth server pages - request handling
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/putresp.fs
require ../httpd/file.fs

require ../membuf/membuf.fs

3500 CONSTANT fsp-internal

VARIABLE fsp-connection

require ../fsp/param.fs

WORDLIST CONSTANT forth-server-pages-wid

: forth-server-pages GET-ORDER NIP forth-server-pages-wid SWAP SET-ORDER ;

GET-CURRENT

forth-server-pages-wid SET-CURRENT

\ Redefine TYPE etc. so that the output goes to the buffer

: TYPE ( c-addr u -- )
   membuf-append THROW
;

: ." ( "ccc<quote>" -- )
   [CHAR] " PARSE
   POSTPONE SLITERAL POSTPONE membuf-append POSTPONE THROW
; IMMEDIATE

: CR ( -- )
   S\" \r\x0a" membuf-append THROW
;

: EMIT ( b -- )
   membuf-append-char THROW
;

: SPACE ( -- )
   BL membuf-append-char THROW
;

: . ( n -- )
   DUP ABS 0 <# #S ROT SIGN #> membuf-append THROW
   BL membuf-append-char THROW
;

: U. ( u -- )
   0 <# #S #> membuf-append THROW
   BL membuf-append-char THROW
;

: D. ( d -- )
   TUCK DABS <# #S ROT SIGN #> membuf-append THROW
   BL membuf-append-char THROW
;

SET-CURRENT

ALSO forth-server-pages
INCLUDE ../fsp/generate.fs
PREVIOUS

: path-renderer ( c-addr u -- xt )
   2DUP forth-server-pages-wid SEARCH-WORDLIST IF
      -ROT 2DROP EXIT
   THEN

   \ Compile FSP
   GET-CURRENT
   ALSO forth-server-pages
   DEFINITIONS
   -ROT 2DUP compile-fsp
   ROT SET-CURRENT
   PREVIOUS

   forth-server-pages-wid SEARCH-WORDLIST IF EXIT THEN
   fsp-internal THROW
;

: send-content ( conn-addr -- )
   \ Replace response buffer by rendering buffer
   DUP >response-buf @ FREE DROP
   DUP DUP >response-handler-area @ SWAP >response-buf !
   DUP DUP >response-handler-area CELL+ @ SWAP >response-buf-size !
   DUP DUP >response-handler-area 2 CELLS + @ SWAP >response-buf-capacity !
   >'cleanup ['] DROP SWAP !
;

: cleanup-free-page ( conn-addr -- )
   >response-handler-area @ FREE DROP
;

: handle-fsp ( conn-addr -- )
   DUP request-method S" GET" COMPARE IF
      DUP request-method S" POST" COMPARE IF
         DUP request-method S" HEAD" COMPARE IF
            status-method-not-allowed SWAP put-error EXIT
         THEN
      THEN
   THEN

   DUP fsp-connection !
   init-membuf THROW
   DUP request-uri uri-path rel-path DUP 255 > IF
      2DROP status-uri-too-long SWAP put-error EXIT
   THEN

   ['] path-renderer CATCH ?DUP IF
      POSTPONE [
      -ROT 2DROP
      DUP fsp-not-found = IF
         DROP status-not-found SWAP put-error EXIT
      THEN
      ." error " . ." compiling FSP" CR
      status-internal-server-error SWAP put-error EXIT
   THEN
   CATCH ?DUP IF
      ." error " . ." rendering FSP" CR
      clear-parameters
      status-internal-server-error SWAP put-error EXIT
   THEN
   clear-parameters

   status-ok OVER put-status
   DUP put-date
   DUP put-server
   DUP S" text/html" ROT put-content-type
   DUP membuf-size @ S>D ROT put-content-length
   DUP S\" \r\x0a" ROT put-response

   membuf-size @ OVER >object-size !

   DUP request-method S" HEAD" COMPARE 0= IF
      membuf @ FREE 2DROP EXIT
   THEN
   
   \ Store buffer with the generated page in handler area
   \ This will become the response buffer after the headers have been sent
   membuf @ OVER >response-handler-area !
   membuf-size @ OVER >response-handler-area CELL+ !
   membuf-capacity @ OVER >response-handler-area 2 CELLS + !
   
   ['] send-content OVER >'provide-response !
   >'cleanup ['] cleanup-free-page SWAP !
;
