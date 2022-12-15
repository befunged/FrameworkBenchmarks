\
\ List of request handlers
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/match.fs

\ Each handler has an associated pattern.
\ For the list a linked list is used.
\ A list element is allocated from the heap consists of:
\ - A pointer to the next item
\ - The handler (an xt)
\ - The pattern (a counted string)

VARIABLE http-handlers
0 http-handlers !

: >handler-next ( addr -- addr )
;

: >handler-xt ( addr -- addr )
CELL+ ;

: >handler-pattern ( addr -- addr )
CELL+ CELL+ ;

: new-handler-entry ( xt c-addr u -- addr )
   DUP 1+ CELL+ CELL+ ALLOCATE THROW

   \ Store characters
   >R TUCK R@
   >handler-pattern 1+ SWAP CMOVE R>
   
   \ Store count byte
   ( xt u addr )
   TUCK >handler-pattern C!
   
   \ Store xt
   TUCK >handler-xt !
;

: add-handler ( xt c-addr u -- )
   new-handler-entry
   http-handlers @ ?DUP IF
      OVER >handler-next !
   ELSE
      0 OVER !
   THEN
   http-handlers !
;

: get-handler ( c-addr u -- xt TRUE | FALSE )
   http-handlers @
   BEGIN ?DUP WHILE
      >R 2DUP R@ >handler-pattern COUNT simple-match IF
         2DROP R> >handler-xt @ TRUE EXIT
      THEN
      R> >handler-next @
   REPEAT
   2DROP FALSE
;
