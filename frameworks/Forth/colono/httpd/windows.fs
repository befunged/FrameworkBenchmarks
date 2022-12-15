\
\ Windows-specific constants and offsets
\
\ Copyright (c) 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

require ../httpd/error.fs

1 CONSTANT AI_PASSIVE
0 CONSTANT AF_UNSPEC
1 CONSTANT SOCK_STREAM
65535 CONSTANT SOL_SOCKET
4 CONSTANT SO_REUSEADDR

256 CONSTANT POLLRDNORM
16 CONSTANT POLLWRNORM
1 CONSTANT POLLERR
2 CONSTANT POLLHUP

2147772030 CONSTANT FIONBIO

10035 CONSTANT EAGAIN

: >ai_flags ;
: >ai_family 1 CELLS + ;
: >ai_socktype 2 CELLS + ;
: >ai_protocol 3 CELLS + ;
: >ai_addrlen 4 CELLS + ;
: >ai_addr 6 CELLS + ;
: >ai_next 7 CELLS + ;

: >st_mtime 32 + ;

VARIABLE colono_errno

[DEFINED] LIBRARY [IF]
LIBRARY Ws2_32.dll
[ELSE]
LIBRARY: Ws2_32.dll
[THEN]

FUNCTION: WSAGetLastError ( -- n )

: __errno_location ( -- a )
   WSAGetLastError colono_errno !
   colono_errno
;

28 CONSTANT sockaddr-size

22 CONSTANT INET_ADDRSTRLEN

65 CONSTANT INET6_ADDRSTRLEN

2 CONSTANT AF_INET
23 CONSTANT AF_INET6

: >sin_addr ( a-addr -- a-addr ) 4 + ;

: >sin6_addr ( a-addr -- a-addr ) 8 + ;

CREATE wsadata 400 ALLOT

FUNCTION: WSAStartup ( n a -- n )

: init-winsock ( -- )
   1 wsadata WSAStartup
   0<> IF sockets-unavailable THROW THEN
;

-2147483648 CONSTANT HKEY_CLASSES_ROOT

131097 CONSTANT KEY_READ

0 CONSTANT ERROR_SUCCESS

FUNCTION: RegOpenKeyEx ( a a u u a -- n )
FUNCTION: RegQueryValueEx ( n a n n n n -- n )
FUNCTION: RegCloseKey ( n -- n )

[DEFINED] EXTERN: [IF]
EXTERN: int "C" _mktemp_s( char *, size_t);  
[ELSE]
FUNCTION: _mktemp_s ( a n -- n )
[THEN]
