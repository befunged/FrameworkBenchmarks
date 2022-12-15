\
\ Dynamic memory buffer
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

50000 VALUE membuf-init-capacity 
VARIABLE membuf
VARIABLE membuf-capacity
VARIABLE membuf-size

\ If u chars don't fit in the buffer, extend buffer
: ?extend-membuf ( u -- ior )
   membuf-size @ + membuf-capacity @ 2DUP > IF
      \ extend buffer by at least membuf-init-capacity bytes
      membuf-init-capacity + MAX DUP membuf @ SWAP RESIZE
      ?DUP IF -ROT 2DROP EXIT THEN
      membuf ! membuf-capacity !
   ELSE
      2DROP
   THEN
   0
;

: membuf-append ( c-caddr u -- ior )
   TUCK
   DUP ?extend-membuf ?DUP IF >R 2DROP DROP R> EXIT THEN
   membuf @ membuf-size @ + SWAP CMOVE
   membuf-size +!
   0
;

: membuf-append-char ( b -- ior )
   1 ?extend-membuf ?DUP IF NIP EXIT THEN
   membuf @ membuf-size @ + C!
   1 membuf-size +!
   0
;

: init-membuf ( -- ior )
   membuf-init-capacity ALLOCATE ?DUP IF NIP EXIT THEN
   membuf !
   membuf-init-capacity membuf-capacity !
   0 membuf-size !
   0
;
