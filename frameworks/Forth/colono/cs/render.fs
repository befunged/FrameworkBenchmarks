\
\ Rendering a template to a memory buffer
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../cs/cs.fs

require ../httpd/posix.fs
require ../httpd/string.fs

require ../membuf/membuf.fs

VARIABLE csparse

: render>buf ( a z-caddr -- err )
   NIP DUP strlen membuf-append
;

[DEFINED] CB: [IF]

\ SwiftForth

: cb-render>buf ( -- err )
   0 _PARAM_1 render>buf
;

' cb-render>buf 2 CB: c_render_to_buf

[ELSE]

[DEFINED] CALLBACK: [IF]

\ VFX Forth

2 1 CALLBACK: c_render_to_buf

' render>buf to-callback c_render_to_buf

[ELSE]

\ GForth

' render>buf cs-renderer CONSTANT c_render_to_buf

[THEN]
[THEN]

\ Render the data given by hdf using the template given by c-addr/u
\ to membuf
: render ( hdf c-addr u -- )
   csparse 3 ROLL cs_init DROP
   $copyz
   csparse @ OVER cs_parse_file IF parse-file-failed THROW THEN
   FREE THROW
   init-membuf THROW
   csparse @ 0 c_render_to_buf cs_render
   IF render-failed THROW THEN
   csparse cs_destroy
;
