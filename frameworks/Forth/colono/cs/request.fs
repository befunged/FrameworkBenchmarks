\
\ Getting request data
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../cs/form.fs
require ../httpd/conn.fs

: request>hdf ( conn-addr hdf -- )
   OVER request-method 2DUP S" POST" COMPARE 0= IF
      2DROP
      SWAP request-data crlfcrlf SEARCH IF
         4 /STRING ROT form>hdf
      ELSE
         2DROP DROP
      THEN
   ELSE
      S" GET" COMPARE 0= IF
         SWAP request-query ROT form>hdf
      ELSE
         2DROP
      THEN
   THEN
;
