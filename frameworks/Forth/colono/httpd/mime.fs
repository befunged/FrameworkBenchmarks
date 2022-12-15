\ Words for reading MIME types from a file and looking them up by file extension.
\ MIME types and file extensions are kept in a hashtable where the extension
\ is the key and the type is the value.
\
\ Copyright (c) 2016, 2017 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/error.fs
require ../httpd/hashtable.fs

2048 CONSTANT mime-table-size

mime-table-size fhashtable mime-table

180 CONSTANT mime-line-buf-size

\ Add extension and type
: add-ext-mime ( c-addr u c-addr u -- )
   mime-table fht-put
;

CREATE mime-line-buf mime-line-buf-size ALLOT

\ Search first non-printable character
: search-ws ( c-addr u -- c-addr u flag )
   DUP 0 ?DO
      OVER I + C@ BL <= IF I /STRING TRUE UNLOOP EXIT THEN
   LOOP
   FALSE
;

: search-nonws ( c-addr u -- c-addr u flag )
   DUP 0 ?DO
      OVER I + C@ BL > IF I /STRING TRUE UNLOOP EXIT THEN
   LOOP
   FALSE
;

\ Return the next token. Tokens are separated by whitespace characters.
\ The next token is returned as c-addr2/u2, the remaining buffer as c-addr1/u1.
\ If no token is found, c-addr1/u1 is returned unchanged.
: next-token ( c-addr u -- c-addr1 u1 c-addr2 u2 TRUE | c-addr1 u1 FALSE )
   search-nonws 0= IF
      FALSE EXIT
   THEN
   2DUP search-ws 0= IF
      + 0 2SWAP TRUE EXIT
   THEN
   DUP >R
   2SWAP
   R> -   
   TRUE
;

\ Try to get mime type and extension(s) from line buffer add insert them to the hash table
: ?add-mime-line ( u -- )
   DUP 0= IF DROP EXIT THEN
   
   \ Ignore line starting with #
   mime-line-buf C@ [CHAR] # = IF
      DROP EXIT
   THEN

   \ Insert all mime-type/extension pairs into the table
   mime-line-buf SWAP next-token 0= IF
      2DROP EXIT
   THEN
   2SWAP
   BEGIN
      next-token WHILE
      2ROT 2DUP 2>R add-ext-mime 2R> 2SWAP
   REPEAT
   2DROP 2DROP
;

\ Add mime types from file
: read-mime-types ( c-addr u -- )
   R/O OPEN-FILE THROW
   BEGIN
      DUP mime-line-buf mime-line-buf-size 2 - ROT READ-LINE THROW WHILE
      ?add-mime-line
   REPEAT
   DROP CLOSE-FILE DROP
;

\ Get mime type by extension, return "application/octet-stream" if no mime type is found
: mime-type ( c-addr u -- c-addr u )
   mime-table fht-get
   DUP 0= IF
      2DROP S" application/octet-stream"
   THEN
;
