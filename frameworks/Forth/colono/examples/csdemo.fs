\
\ Example for processing a request generating a response page using Clearsilver
\ Currently this works under SwiftForth and VFX Forth only.
\
\ Copyright (c) 2017 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

\ On Windows, replace FALSE by TRUE
FALSE CONSTANT windows?

[UNDEFINED] required [IF]
S" ../httpd/compat/required.fs" INCLUDED
[THEN]

require ../httpd/server.fs
require ../httpd/file.fs
require ../cs/request.fs
require ../cs/render.fs
require ../cs/response.fs

VARIABLE hdf

: handle-request ( conn-addr -- )
   hdf hdf_init DROP
   DUP hdf @ request>hdf

   \ Render the page using the dataset hdf and the template file template.cs
   hdf @ S" template.cs" render-response

   hdf hdf_destroy
;

S" /etc/mime.types" read-mime-types

' handle-GET-file S" *" add-handler

' handle-request S" /post" add-handler

' handle-request S" /template*" add-handler

httpserve
