\
\ Rendering a template to the response
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../cs/render.fs

require ../httpd/putresp.fs

: send-content ( conn-addr -- )
   \ Replace response buffer by rendering buffer
   DUP >response-buf @ FREE DROP
   DUP DUP >response-handler-area @ SWAP >response-buf !
   DUP DUP >response-handler-area CELL+ @ SWAP >response-buf-size !
   DUP DUP >response-handler-area 2 CELLS + @ SWAP >response-buf-capacity !
   >'cleanup ['] DROP SWAP !
;

\ Called when the connection is closed prematurely, frees the page data
: cleanup-free-page ( conn-addr -- )
   >response-handler-area @ FREE DROP
;

\ Render the template given by c-addr/u using the data given by hdf
\ and write it to the connection given by conn-addr as an HTTP response
: render-response ( conn-addr hdf c-addr u -- )
   ['] render CATCH ?DUP IF
      ." error " . ." rendering template" cr
      2DROP DROP
      status-internal-server-error SWAP put-error
      EXIT
   THEN

   status-ok OVER put-status
   DUP put-date
   DUP put-server
   DUP S" text/html" ROT put-content-type
   DUP membuf-size @ S>D ROT put-content-length
   DUP S\" \r\x0a" ROT put-response
   
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
