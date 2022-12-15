\
\ Server start script under Windows
\
\ Copyright (c) 2017 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

[UNDEFINED] required [IF] S" compat/required.fs" INCLUDED [THEN]

\ OS is MS Windows
TRUE CONSTANT windows?

require server.fs
require file.fs

\ Handler for files - remove this line if the server must not serve files
' handle-GET-file S" *" add-handler

\ Additional handlers go here
\ These must be after the handler for * because otherwise that handler would get all requests

\ Listen host (optional) and port. The format is [host:]port. Default is 4004.
\ ListenAddress&Port 80

\ The document root direcory. A trailing / is required.
\ Default is the current directory.
\ DocumentRoot /var/www/

\ Set the request read timeout, in seconds (the time to receive the request).
\ Default is 60.
\ 60 TO request-read-timeout

\ Set the keep-alive timeout, in seconds (the time the server will wait for a subsequent request).
\ Setting the value to 0 disables persistent connections.
\ Default is 15.
\ 15 TO keep-alive-timeout

\ Uncomment this to enable access logging
\ AccessLog colono.log

init-winsock

httpserve
