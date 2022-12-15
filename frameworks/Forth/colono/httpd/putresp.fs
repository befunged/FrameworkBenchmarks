\
\ Writing to the response buffer
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/conn.fs
require ../httpd/time.fs
require ../httpd/status.fs

: put-response ( c-addr u conn-addr -- )
   2DUP >response-buf-size @ + response-buf-capacity > IF
      2DROP DROP buffer-full THROW
   THEN

   DUP >response-buf-capacity @ 0=  IF
      \ Allocate response buffer
      response-buf-capacity ALLOCATE THROW
      OVER >response-buf !
      response-buf-capacity OVER >response-buf-capacity !
   THEN
   move>response
;

: put-response-body ( c-addr u conn-addr -- )
   2DUP >object-size +!
   put-response
;

: put-time&date ( u1 u2 u3 u4 u5 u6 conn-addr -- )
   >R
   ROT S>D <# # # #> R@ put-response
   SWAP month R@ put-response
   S>D <# # # # # #> R@ put-response
   S"  " R@ put-response
   S>D <# # # #> R@ put-response
   S" :" R@ put-response
   S>D <# # # #> R@ put-response
   S" :" R@ put-response
   S>D <# # # #> R> put-response
;

: put-date ( conn-addr -- )
   DUP S" Date: " ROT put-response
   >R 0 time gmtime&date R@ put-time&date
   S\"  GMT\r\x0a" R> put-response
;

: put-server ( conn-addr -- )
   S\" Server: Colono/pre0.8\r\x0a" ROT put-response
;

: put-connection-close ( conn-addr -- )
   S\" Connection: Close\r\x0a" ROT put-response
;

: put-status-code ( status conn-addr -- )
   SWAP S>D <# # # # #> ROT put-response
;

: put-status-code-body ( status conn-addr -- )
   SWAP S>D <# # # # #> ROT put-response-body
;

\ Write status line to output buffer
: put-status ( status conn-addr -- )
   2DUP >status-code !
   DUP S" HTTP/1.1 " ROT put-response
   2DUP put-status-code
   DUP S"  " ROT put-response
   DUP ROT status-phrase ROT put-response
   S\" \r\x0a" ROT put-response
;

: put-content-length ( ud conn-addr -- )
   DUP S" Content-Length: " ROT put-response
   >R <# #S #> R@ put-response
   S\" \r\x0a" R> put-response
;

: put-content-type ( c-addr u conn-addr -- )
   DUP S" Content-Type: " ROT put-response
   DUP >R put-response
   S\" \r\x0a" R> put-response
;

51 CONSTANT err-content-length-base

\ Write a complete error response to the output buffer
: put-error ( status conn-addr -- )
   2DUP put-status
   DUP put-date
   DUP put-server
   2DUP SWAP status-phrase NIP 2* err-content-length-base + 0 ROT put-content-length
   DUP S\" Content-Type: text/html\r\x0a\r\x0a" ROT put-response

   DUP request-method S" HEAD" COMPARE 0= IF
      2DROP EXIT
   THEN
   
   DUP S" <!doctype html><html><title>" ROT put-response-body
   2DUP SWAP status-phrase ROT put-response-body
   DUP S" </title><p>" ROT put-response-body
   2DUP put-status-code-body
   DUP S"  " ROT put-response-body
   DUP ROT status-phrase ROT put-response-body
   S\" </html>\x0a" ROT put-response-body
;
