\
\ Example POST handler which returns the reqest data to the client
\
\ Copyright (c) 2016 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

\ On Windows, replace FALSE by TRUE
FALSE CONSTANT windows?

[UNDEFINED] required [IF]
S" ../httpd/compat/required.fs" INCLUDED
[THEN]

include ../httpd/server.fs
include ../httpd/file.fs

: handle-POST-test ( conn-addr -- )
   DUP request-method S" POST" COMPARE IF
      status-method-not-allowed SWAP put-error EXIT
   THEN

   status-ok OVER put-status
   DUP put-date
   DUP put-server
   DUP DUP request-data NIP 92 + 0 ROT put-content-length
   DUP S\" Content-Type: text/html; charset=UTF-8\r\n" ROT put-response
   DUP S\" \r\n" ROT put-response
   DUP S\" <html><title>Thank you</title><p>Thanks for the POST. This is the request data:<pre>\n" ROT put-response-body
   DUP DUP request-data ROT put-response-body
   S\" \n</pre>" ROT put-response-body
;

S" /etc/mime.types" read-mime-types

' handle-GET-file S" *" add-handler

' handle-POST-test S" /post" add-handler

httpserve
