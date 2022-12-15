\
\ Pattern matching string search
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

[UNDEFINED] 2NIP [IF]
: 2NIP 2SWAP 2DROP ;
[THEN]

: starts-with ( caddr1 u1 caddr2 u2 -- flag )
   ROT OVER U< IF 2DROP DROP FALSE EXIT THEN

   TUCK COMPARE 0=
;

: ends-with ( caddr1 u1 caddr2 u2 -- flag )
   2 PICK OVER U< IF 2DROP 2DROP FALSE EXIT THEN

   2SWAP +
   OVER - OVER COMPARE 0=
;

: simple-match ( caddr1 u1 caddr2 u2 -- flag )
\ Check if the string given by caddr and u1 matches the pattern given by caddr2 and u2.
\ If the pattern does not contain a '*' character, the string must match exactly.
\ Otherwise, the string must begin with the substring before the first '*' character
\ and end with the substring after the first '*' character.
   2SWAP 2>R
   2DUP S" *" SEARCH IF
      ROT OVER - -ROT
      1 /STRING 2R@ 2SWAP ends-with 0= IF 2DROP 2R> 2DROP FALSE EXIT THEN
      2R> 2SWAP starts-with
   ELSE
      2DROP 2R> COMPARE 0=
   THEN
;

[UNDEFINED] search(nc) [IF]
[UNDEFINED] capscompare [IF]
: capscompare icompare ;
[THEN]
\ Case-insensitive SEARCH, uses capscompare
: search(nc) ( caddr1 u1 caddr2 u2 -- caddr3 u3 flag )
   2 PICK OVER < IF
      2DROP FALSE EXIT
   THEN
   2>R 2DUP 2R>
   2 PICK OVER - 1+ 0 DO
      2OVER 2OVER
      ROT DROP DUP -ROT capscompare 0= IF
         2DROP 2NIP TRUE UNLOOP EXIT
      THEN
      2SWAP 1 /STRING 2SWAP
   LOOP
   2DROP 2DROP FALSE
;
[THEN]

\ Search for the last occurence of char in the string given by caddr1/u1
: csearchlast ( caddr1 u1 char -- caddr2 u2 flag )
   >R TUCK R>
   SWAP DUP 0= IF 2DROP SWAP FALSE EXIT THEN
   1- 0 SWAP DO
      OVER I + C@ OVER = IF
         DROP SWAP I /STRING TRUE UNLOOP EXIT
      THEN
   -1 +LOOP
   DROP SWAP FALSE
;
