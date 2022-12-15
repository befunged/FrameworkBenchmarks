\
\ Forth server pages - page generation
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

windows? [IF]
require ../httpd/windows.fs
[ELSE]
require ../httpd/posix.fs
[THEN]

3510 CONSTANT fsp-not-found

CREATE temp-path 1024 ALLOT

\ Append c-addr1/u1 to the string in temp-path with length u2
: concat-temp ( c-addr1 u1 u2 -- u3 )
   2DUP + >R
   temp-path + SWAP CMOVE
   R>
;

: temp-dir ( -- c-addr u )
   \ Get value of environment variable TEMP
   \ (usually only set on Windows)
   [DEFINED] ReadEnv [IF]
   \ VFX Forth
   S" TEMP" ReadEnv
   [ELSE] [DEFINED] FIND-ENV [IF]
   \ SwiftForth
   S" TEMP" FIND-ENV 0= IF
      DROP 0
   THEN
   [ELSE]
   \ GForth
   S" TEMP" getenv
   [THEN] [THEN]

   \ If TEMP wasn't set, use /tmp
   DUP 0= IF
     2DROP S" /tmp"
   THEN
;

: create-temp-file ( c-addr1 u1 -- c-addr2 u2 file-id )
   temp-dir
   TUCK temp-path SWAP CMOVE
   temp-path OVER + windows? IF [CHAR] \ ELSE [CHAR] / THEN SWAP C! 1+
   concat-temp
   S\" XXXXXX\0" ROT concat-temp DROP
   temp-path
   [ windows? ] [IF]
   DUP strlen 1+ _mktemp_s DROP
   [ELSE]
   mkstemp close DROP
   [THEN]
   temp-path DUP strlen 2DUP R/W CREATE-FILE THROW
;

512 CONSTANT fsp-buf-size
CREATE fsp-buf fsp-buf-size ALLOT

: append-file ( c-addr u file-id -- )
   -ROT R/O OPEN-FILE IF fsp-not-found THROW THEN
   BEGIN
      DUP fsp-buf fsp-buf-size ROT READ-FILE THROW
      ( out-fid in-fid u )
      DUP 0= IF
         DROP CLOSE-FILE THROW DROP EXIT
      THEN
      >R OVER fsp-buf R> ROT WRITE-FILE THROW
   AGAIN
;

: compile-fsp ( c-addr u -- )
   \ Convert FSP file to a colon definition and
   \ write it to a temporary file
   2DUP create-temp-file
   -ROT 2>R
   DUP S" : " ROT WRITE-FILE THROW
   DUP 2OVER ROT WRITE-FILE THROW
   DUP S"  %> " ROT WRITE-FILE THROW
   DUP >R append-file R>
   DUP S"  <% ;" ROT WRITE-FILE THROW
   DUP 0. ROT REPOSITION-FILE THROW

   \ Include temporary file to compile the definition
   INCLUDE-FILE

   2R> DELETE-FILE DROP
;

: %> ( -- )
   BEGIN
      SOURCE >IN @ /STRING 2DUP S" <%" SEARCH IF
         NIP - DUP 2 + >IN +! POSTPONE SLITERAL POSTPONE TYPE EXIT
      THEN
      2DROP POSTPONE SLITERAL POSTPONE TYPE
      POSTPONE CR
   REFILL 0= UNTIL
; IMMEDIATE
