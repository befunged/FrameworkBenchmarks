\
\ Words for making ClearSilver available from Forth
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

\ ClearSilver doesn't come with shared libraries by default.
\ The static libraries libneo_utl.a and libneo_cs.a can be converted
\ to a single shared library using:
\ gcc -shared -o libneo_utl_cs.so -Wl,--whole-archive /usr/local/lib/libneo_utl.a \
\ /usr/local/lib/libneo_cs.a -/usr/local/lib/l/Wl,--no-whole-archive

3500 CONSTANT parse-file-failed
3501 CONSTANT render-failed

[DEFINED] c-library [IF]

c-library lib-cs

\ When using GForth, C_INCLUDE_PATH has to be set to e.g. /usr/local/include/ClearSilver
\ LD_LIBRARY_PATH may also have to be set to the directory constaining libneo_utl_cs.so.

s" neo_utl_cs" add-lib

\c #include <ClearSilver.h>

c-function hdf_init hdf_init a -- a
c-function hdf_destroy hdf_destroy a -- void
c-function hdf_set_value hdf_set_value a a a -- a
c-function hdf_get_value hdf_get_value a a a -- a
c-function hdf_set_buf hdf_set_buf a a a -- a
c-function hdf_dump hdf_dump a a -- a

c-function cs_init cs_init a a -- a
c-function cs_destroy cs_destroy a -- void
c-function cs_parse_string cs_parse_string a a n -- a
c-function cs_parse_file cs_parse_file a a -- a
c-function cs_render cs_render a a func -- a

c-function nerr_log_error nerr_log_error a -- void

c-callback cs-renderer n a -- a

end-c-library

[ELSE]

[DEFINED] LIBRARY [IF]
LIBRARY /usr/local/lib/libneo_utl_cs.so
[ELSE]
LIBRARY: /usr/local/lib/libneo_utl_cs.so
[THEN]

FUNCTION: hdf_init ( a -- a )
FUNCTION: hdf_destroy ( a -- )
FUNCTION: hdf_set_value ( a a a -- a )
FUNCTION: hdf_get_value ( a a a -- a )
FUNCTION: hdf_set_buf ( a a a -- a )
FUNCTION: hdf_dump ( a a -- a )

FUNCTION: cs_init ( a a -- a )
FUNCTION: cs_destroy ( a -- )
FUNCTION: cs_parse_string ( a a n -- a )
FUNCTION: cs_parse_file ( a a -- a )
FUNCTION: cs_render ( a a func -- a )

[THEN]
