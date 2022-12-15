\
\ Example for processing a request generating a response page using Forth Server Pages
\
\ Copyright (c) 2017 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

TRUE CONSTANT windows?

[UNDEFINED] required [IF]
S" ../httpd/compat/required.fs" INCLUDED
[THEN]

require ../httpd/server.fs
require ../httpd/file.fs
require ../fsp/fsp.fs

' handle-GET-file S" *" add-handler

' handle-fsp S" *.fsp" add-handler

init-winsock

httpserve
