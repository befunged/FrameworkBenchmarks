\
\ Server main loop and request processing
\
\ Copyright (c) 2016, 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/listen.fs
require ../httpd/hlist.fs
require ../httpd/os.fs
require ../httpd/posix.fs
require ../httpd/conn.fs
require ../httpd/putresp.fs
require ../httpd/error.fs
require ../httpd/status.fs
require ../httpd/nonblock.fs
require ../httpd/log.fs
require ../httpd/uri.fs

VARIABLE listen-fd

-2049 CONSTANT err-sigpipe

: keep-alive? ( conn-addr -- flag )
   keep-alive-timeout 0= IF
      DROP FALSE EXIT
   THEN
   DUP request-http-version S" HTTP/1.0" COMPARE 0= IF
      S" Connection" request-header IF
         S" keep-alive" COMPARE 0=
      ELSE
         FALSE
      THEN
   ELSE
      S" Connection" request-header IF
         S" close" COMPARE 0<>
      ELSE
         TRUE
      THEN
   THEN
;

: reset-object-size ( conn-addr -- )
    >object-size 0 SWAP !
;

: end-request ( conn-addr -- )
   DUP log-access
   DUP keep-alive? IF
      0 time keep-alive-timeout + OVER >timeout !
      DUP reset-object-size
      DUP request-length OVER >request-buf-pos @ < IF
         DUP request-length >R DUP request-buf OVER R@ + -ROT R@ - CMOVE
         DUP >request-buf-pos R> NEGATE SWAP +!
         DUP request-complete? IF
            \ Signal that a new request is ready for processing
            >process-next-request TRUE SWAP !
         ELSE
            DROP
         THEN
      ELSE
         >request-buf-pos 0 SWAP !
      THEN
   ELSE
      close-conn
   THEN
;

: process-request ( conn-addr -- )
   \ Set default action after response buffer has been written
   ['] end-request OVER >'provide-response !

   DUP request-method
   2DUP S" GET" COMPARE IF
      2DUP S" HEAD" COMPARE IF
         2DUP S" PUT" COMPARE IF
            2DUP S" POST" COMPARE IF
               2DUP S" DELETE" COMPARE IF
                  2DUP S" OPTIONS" COMPARE 0= IF
                     2DROP status-method-not-allowed SWAP put-error EXIT
                  THEN
                  2DUP S" TRACE" COMPARE 0= IF
                     2DROP status-method-not-allowed SWAP put-error EXIT
                  THEN
                  S" CONNECT" COMPARE 0= IF
                     status-method-not-allowed SWAP put-error EXIT
                  THEN
                  status-bad-request SWAP put-error EXIT
               THEN
            THEN
         THEN
      THEN
   THEN
   2DROP

   \ Get request URI
   DUP request-uri 2DUP request-uri-valid? 0= IF
      2DROP status-bad-request SWAP put-error EXIT
   THEN

   \ Get handler for URI and call it
   ['] uri-path CATCH IF
      2DROP
      status-uri-too-long SWAP put-error
      EXIT
   THEN
   get-handler IF
      EXECUTE
   ELSE
      status-not-found SWAP put-error
   THEN
;

: process-request-data ( conn-addr -- )
   DUP request-buf-full? IF
      DUP extend-buffer 0= IF DUP status-payload-too-large SWAP put-error EXIT THEN
   THEN
   DUP DUP >conn-fd @ SWAP request-free-buf
   read DUP 0< IF
      DROP __errno_location @ EAGAIN = IF
         DROP
      ELSE
         0 perror close-conn
      THEN
   ELSE
      DUP 0= IF
         \ EOF received - close connection
         DROP close-conn
      ELSE
         OVER >request-buf-pos +!
         DUP request-complete? IF process-request ELSE DROP THEN
      THEN
   THEN
;

: write-response-data ( conn-addr -- )
   DUP DUP >conn-fd @ SWAP
   response-unwritten write
   DUP 0< IF
      DROP __errno_location @ EAGAIN = IF
         DROP
      ELSE
         0 perror close-conn
      THEN
   ELSE
      DUP 0= IF
         DROP close-conn
      ELSE
         OVER >response-buf-pos +!
         DUP response-data-ready? IF
            DROP
         ELSE
            \ All response data has been written,
            \ reset buffer and ask for more data
            DUP >response-buf-size 0 SWAP !
            DUP >response-buf-pos 0 SWAP !
            DUP DUP >'provide-response DUP @ SWAP ['] end-request SWAP !
            EXECUTE
            DUP >process-next-request @ IF
               DUP >process-next-request FALSE SWAP !
               process-request
            ELSE
               DROP
            THEN
         THEN
      THEN
   THEN
;

: make-pollfds ( -- )
   0 pollfd max-connections 1+ 2 CELLS * ERASE

   listen-fd @ 0 pollfd !
   POLLRDNORM 0 pollfd >pollfd-events W!

   max-connections 0 DO
      I connection >conn-fd @ DUP I 1+ pollfd !
      -1 <> IF
         I connection response-data-ready? IF
            POLLWRNORM
         ELSE
            POLLRDNORM
         THEN
         I 1+ pollfd >pollfd-events W!
      THEN
   LOOP
;

[DEFINED] sigaction [IF]
CREATE pipe-sigaction sigaction-size ALLOT

: ignore-SIGPIPE ( -- )
   pipe-sigaction sigaction-size ERASE
   SIG_IGN pipe-sigaction >sa_handler !
   0 pipe-sigaction >sa_flags !
   pipe-sigaction >sa_mask sigemptyset DROP
   SIGPIPE pipe-sigaction 0 sigaction
   -1 = IF ." sigaction() failed: " 0 perror THEN
;
[THEN]

VARIABLE sockaddr-size-var

: handle-new-client ( -- )
   sockaddr-size sockaddr-size-var !
   listen-fd @ sockaddr sockaddr-size-var sockaccept
   DUP 0< IF DROP
   ELSE DUP +nonblocking add-connection
   THEN
;

: close-on-timeout ( n -- )
   DUP >R 1+ pollfd >pollfd-events W@ POLLRDNORM AND IF
      R@ connection >timeout @
      0 time <= IF
         R@ connection close-conn
      THEN
   THEN
   R> DROP
;

: httpserve ( -- )
[DEFINED] sigaction [IF]
   ignore-SIGPIPE
[THEN]
   server-listen
   DUP 0< IF DROP EXIT THEN
   listen-fd !
   init-connections
   BEGIN
      make-pollfds
      0 pollfd max-connections 1+
      
      \ Determine which timeout will pass next
      \ and pass this timeout to poll()
      next-timeout DUP time-max = IF
         DROP -1  \ No timeout
      ELSE
         0 time - DUP 0> IF
            1000 *
         ELSE
            DROP 1   \ Timeout(s) already passed, wait for 1ms
         THEN
      THEN
      poll
      0< IF 0 perror cr poll-failed THROW THEN

      \ Incoming connection?
      0 pollfd >pollfd-revents W@ 0<> IF
         handle-new-client
      THEN

      \ Incoming data?
      max-connections 0 DO
         I 1+ pollfd >pollfd-revents W@ DUP POLLRDNORM POLLERR OR AND IF
            I connection process-request-data
         THEN
         I connection >conn-fd @ -1 > IF  \ process-request-data might have closed the connection
            POLLWRNORM AND IF
               I connection ['] write-response-data CATCH ?DUP 0<> IF
                  \ Close connection on SIGPIPE, otherwise rethrow exception
                  DUP err-sigpipe = IF
                     DROP I connection close-conn
                  ELSE
                     THROW
                  THEN
               THEN
            ELSE
               \ check for POLLHUP
               I 1+ pollfd >pollfd-revents W@ POLLHUP AND IF
                  I connection close-conn
               ELSE
                  \ check for timeout
                  I close-on-timeout
               THEN
            THEN
         ELSE
            DROP
         THEN
      LOOP
   AGAIN
;
