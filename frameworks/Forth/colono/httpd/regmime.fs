\
\ Reading the MIME type from the registry.
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/windows.fs
require ../httpd/string.fs

VARIABLE hkey
VARIABLE extz

256 CONSTANT reg-buf-def-size
VARIABLE reg-buf-size
CREATE reg-buf reg-buf-def-size ALLOT

: reg-mime-type ( c-addr u -- c-addr u )
   $copyz extz !
   HKEY_CLASSES_ROOT extz @ 0 KEY_READ hkey RegOpenKeyEx ERROR_SUCCESS <> IF
      extz @ FREE THROW
      S\" application/octet-stream" EXIT
   THEN
   reg-buf reg-buf-def-size ERASE
   reg-buf-def-size reg-buf-size !
   hkey @ S\" Content Type\0" DROP 0 0 reg-buf reg-buf-size RegQueryValueEx
   hkey @ RegCloseKey DROP
   ERROR_SUCCESS <> IF
      extz @ FREE THROW
      S\" application/octet-stream" EXIT
   THEN	  
   reg-buf reg-buf-size @
   BEGIN
      DUP 0= IF EXIT THEN
      2DUP + 1- C@ 0= WHILE
	  1-
   REPEAT
;
