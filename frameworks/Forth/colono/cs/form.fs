\
\ HTML form data processing
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../cs/cs.fs

require ../httpd/urldecode.fs
require ../httpd/posix.fs
require ../httpd/string.fs
require ../httpd/error.fs

\ Decode the x-www-form-urlencoded string c-addr1/u and store it at c-addr2,
\ followed by a 0-byte.
: urldecodez ( c-addr1 u1 c-addr2 -- )
   DUP >R
   urldecode
   R> + 0 SWAP C!
;

: value>hdf ( c-addr1 u1 c-addr2 u2 hdf -- )
   >R
   DUP 1+ malloc DUP 0= IF malloc-failed THROW THEN
   DUP >R
   urldecodez R>
   ( c-addr1 u1 z-addr )
   -ROT $copyz DUP ROT
   R> -ROT hdf_set_buf
   DROP
   FREE THROW
;

: name=value>hdf ( c-addr u hdf -- )
   >R
   2DUP S" =" SEARCH 0= IF
      2DROP 2DROP R> DROP EXIT
   THEN
   DUP >R 2SWAP R> -
   2SWAP 1 /STRING
   R> value>hdf
;

\ Copy the form data given by c-add/u to the dataset given by hdf.
\ Multiple field names are not yet supported.
: form>hdf ( c-addr u hdf -- )
   >R
   BEGIN
   2DUP S" &" SEARCH WHIlE
      DUP >R 2SWAP R> - R@ name=value>hdf
      1 /STRING
   REPEAT
   R> name=value>hdf
   2DROP
;
