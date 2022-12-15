\
\ Decoding URL encoded data
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

: hhdecode ( c-addr -- c )
   0. ROT 2 HEX >NUMBER DECIMAL 2DROP D>S
;

: +>bl ( c -- c )
   DUP [CHAR] + = IF
      DROP BL
   THEN
;

\ Decode the x-www-form-urlencoded string c-addr1/u and store it at c-addr2.
\ Return the length of the result
: urldecode ( c-addr1 u1 c-addr2 -- u2 )
   DUP >R
   BEGIN OVER 0> WHILE
      2 PICK C@
      DUP [CHAR] % = IF
         DROP OVER 3 < IF
            \ There are less than 2 chars after '%', stop
            NIP 0 SWAP
         ELSE
            2 PICK 1+ hhdecode OVER C!
            1+ -ROT 3 /STRING ROT
         THEN
      ELSE
         +>bl OVER C!
         1+ -ROT 1 /STRING ROT
      THEN
   REPEAT
   -ROT 2DROP R> -
;
