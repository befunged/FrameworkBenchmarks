\
\ Linux-specific constants and offsets
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

1 CONSTANT AI_PASSIVE
0 CONSTANT AF_UNSPEC
1 CONSTANT SOCK_STREAM
1 CONSTANT SOL_SOCKET
2 CONSTANT SO_REUSEADDR

64 CONSTANT POLLRDNORM
256 CONSTANT POLLWRNORM
8 CONSTANT POLLERR
16 CONSTANT POLLHUP

3 CONSTANT F_GETFL
4 CONSTANT F_SETFL

2048 CONSTANT O_NONBLOCK

11 CONSTANT EAGAIN

: >ai_flags ;
: >ai_family 1 CELLS + ;
: >ai_socktype 2 CELLS + ;
: >ai_protocol 3 CELLS + ;
: >ai_addrlen 4 CELLS + ;
: >ai_addr 5 CELLS + ;
: >ai_next 7 CELLS + ;

[DEFINED] c-library [IF]

: >st_mtime 80 + ;

c-library lib-errno

\c #include <errno.h>

c-function __errno_location __errno_location -- a

end-c-library

[ELSE]

: >st_mtime 64 + ;

FUNCTION: __errno_location ( -- a )

[THEN]

13 CONSTANT SIGPIPE

140 CONSTANT sigaction-size

1 CONSTANT SIG_IGN

: >sa_handler ( a-addr -- a-addr ) ;

: >sa_mask ( a-addr -- a-addr ) CELL + ;

: >sa_flags ( a-addr -- addr ) 132 + ;

28 CONSTANT sockaddr-size

16 CONSTANT INET_ADDRSTRLEN

46 CONSTANT INET6_ADDRSTRLEN

2 CONSTANT AF_INET
10 CONSTANT AF_INET6

: >sin_addr ( a-addr -- a-addr ) 4 + ;

: >sin6_addr ( a-addr -- a-addr ) 8 + ;
