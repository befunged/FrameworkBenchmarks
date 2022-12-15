\
\ String copying
\
\ Copyright (c) 2016, 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

\ Create a copy of a string on the heap
: clone-string ( c-addr1 u -- c-addr2 )
   DUP ALLOCATE THROW
   DUP >R
   SWAP CMOVE
   R>
;

\ Store c-addr/u at a-addr, free previous string stored at a-addr
: string! ( c-addr u a-addr -- )
   DUP 2@ 0<> IF FREE THROW
   ELSE DROP
   THEN
   2!
;

\ Create a copy of a string on the heap and store address and length
: clone-string! ( caddr u a-addr -- )
   -ROT
   TUCK clone-string
   -ROT SWAP string!
;

\ Move and add trainling zero
: cmovez ( c-addr u c-addr2 -- )
   2DUP + 0 SWAP C!
   SWAP CMOVE
;

\ Copy string into zero-terminated string allocated from the heap
: $copyz ( caddr u -- z-caddr )
   DUP 1+ ALLOCATE THROW
   DUP >R cmovez R>
;
