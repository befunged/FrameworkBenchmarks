\ A fixed-size hash table.
\
\ Copyright (c) 2017 Rene Hartmann.
\ See the file LICENSE for redistribution information.
\

\ This hash table implementation has some restrictions:
\ - The size of the has table is fixed. If the hash table is full, adding additional key/value pairs will fail.
\ - Deleting individual keys is not supported. The hash table can only be cleared as a whole.

require ../httpd/string.fs

3450 CONSTANT fht-table-full

4 CELLS CONSTANT fht-entry-size

: fhashtable ( size -- ) CREATE
   DUP , 0 , fht-entry-size * HERE
   OVER ALLOT SWAP ERASE
;

\ String hashing function
: fht-hash ( c-addr u -- n )
   5381 -ROT
   0 ?DO
      I OVER + C@ ROT 33 * + SWAP
   LOOP
   DROP
;

: entry-addr ( u a-addr -- a-addr )
   2 CELLS +
   SWAP 4 CELLS *
   +
;

: fht-find-entry ( c-addr u a-addr -- u )
   >R 2DUP fht-hash ABS R@ @ MOD
   BEGIN
      ( c-addr u idx )
      DUP R@ entry-addr DUP @ 0= IF
         DROP -ROT 2DROP R> DROP EXIT
      THEN
      ( c-addr u idx c-addr )
      2OVER ROT 2@ COMPARE WHILE
      ( c-addr u idx )
      1+ DUP R@ @ >= IF
         \ End of table reached, wrap around
         DROP 0
      THEN
   REPEAT
   R> DROP -ROT 2DROP
;

: fht-free-entries ( a-addr -- u )
   DUP @ SWAP CELL+ @ -
;

: fht-put-entry ( c-addr1 u1 x1 x2 a-addr -- )
   DUP >R
   2 CELLS + 2!
   R> clone-string!
;

\ Store key c-addr/u and value x1/x2 in hashtable a-addr,
\ Copy only the key
: fht-put2 ( c-addr u x1 x2 a-addr -- )
   \ At least one entry must remain empty
   DUP fht-free-entries 1 <= IF
      DROP 2DROP 2DROP fht-table-full THROW
   THEN
   >R 2OVER R@ fht-find-entry
   R@ entry-addr
   ( c-addr1 u1 c-addr2 u2 a-addr )
   fht-put-entry
   R> CELL+ 1 SWAP +!
;

\ Store key c-addr1/u1 and value c-addr2/u2 in hashtable a-addr
\ Copy both key and value
: fht-put ( c-addr1 u1 c-addr2 u2 a-addr -- )
   -ROT TUCK clone-string SWAP ROT  \ clone value
   fht-put2
;

: fht-get ( c-addr1 u1 a-addr -- c-addr2 u2 )
   DUP >R fht-find-entry R> entry-addr 2 CELLS +
   2@
;

\ Clear the hashtable and delete all keys and values
: fht-clear ( c-addr -- )
   DUP @ 0 DO
      I OVER entry-addr
      DUP @ 0> IF
         DUP CELL+ @ FREE THROW
         3 CELLS + @ FREE THROW
      ELSE
         DROP
      THEN
   LOOP
   DUP CELL+ 0 SWAP !
   DUP 2 CELLS + SWAP @ 4 CELLS * ERASE
;
