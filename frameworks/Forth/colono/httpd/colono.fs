#! /usr/bin/gforth
\
\ Server start script
\
\ Copyright (c) 2016 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

FALSE CONSTANT windows?

[UNDEFINED] required [IF] S" compat/required.fs" INCLUDED [THEN]

require ../httpd/server.fs
require ../httpd/file.fs
require ../httpd/mime.fs

\ Modify this line if mime.types is at a different location
S" /etc/mime.types" read-mime-types

\ Handler for files - remove this line if files must not be sent
' handle-GET-file S" *" add-handler

\ Additional handlers go here
\ These must be after the handler for * because otherwise that handler would get all requests

\ Listen host (optional) and port. The format is [host:]port. Default is 4004.
ListenAddress&Port 8080

\ The document root direcory. A trailing / is required.
\ Default is the current directory.
\ DocumentRoot /var/www/

\ Set the request read timeout, in seconds (the time to receive the request).
\ Default is 60.
60 TO request-read-timeout

\ Set the keep-alive timeout, in seconds (the time the server will wait for a subsequent request).
\ Setting the value to 0 disables persistent connections.
\ Default is 15.
15 TO keep-alive-timeout

\ Uncomment this to enable access logging
\ AccessLog /var/log/colono.log

httpserve
