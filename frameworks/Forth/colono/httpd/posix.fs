\
\ POSIX definitions
\
\ Copyright (c) 2016, 2017 René Hartmann.
\ See the file LICENSE for redistribution information.
\

[DEFINED] c-library [IF]

\ GForth

c-library lib-posix

\c #include <unistd.h>
\c #include <string.h>
\c #include <sys/socket.h>
\c #include <netdb.h>
\c #include <arpa/inet.h>
\c #include <poll.h>
\c #include <stdio.h>
\c #include <time.h>
\c #include <sys/stat.h>
\c #include <sys/fcntl.h>
\c #include <stdlib.h>

c-function strlen strlen a -- n
c-function malloc malloc n -- a

c-function read read n a n -- n
c-function write write n a n -- n
c-function close close n -- n

c-function getaddrinfo getaddrinfo a a a a -- n
c-function socket socket n n n -- n
c-function bind bind n a n -- n
c-function listen listen n n -- n
c-function setsockopt setsockopt n n n a n -- n
c-function sockaccept accept n a a -- n
c-function inet_ntop inet_ntop n a a n -- a

c-function poll poll a n n -- n

c-function time time a -- n
c-function gmtime gmtime a -- a
c-function localtime localtime a -- a
c-function strftime strftime a n a a -- n

c-function stat stat a a -- n

c-function fcntl fcntl n n n -- n

c-function perror perror a -- void

c-function mkstemp mkstemp a -- n

end-c-library

[ELSE]

\ SwiftForth/VFX Forth

windows? [IF]
[DEFINED] LIBRARY [IF]
LIBRARY Ws2_32.dll
LIBRARY MSVCRT.DLL
[ELSE]
LIBRARY: Ws2_32.dll
LIBRARY: MSVCRT.DLL
[THEN]

FUNCTION: recv ( n a n n -- n )
FUNCTION: send ( n a n n -- n )
FUNCTION: closesocket ( n -- n )
: close closesocket ;
: read ( n a n -- n ) 0 recv ;
: write ( n a n -- n ) 0 send ;

[THEN]

FUNCTION: getaddrinfo ( a a a a -- n )
FUNCTION: socket ( n n n -- n )
FUNCTION: bind ( n a n -- n )
FUNCTION: listen ( n n -- n )
FUNCTION: setsockopt ( n n n a n -- n )

[DEFINED] AliasedExtern: [IF]
AliasedExtern: sockaccept int accept( int sockfd, void *, unsigned int *);
[ELSE]
AS sockaccept FUNCTION: accept ( n a a -- n )
[THEN]

FUNCTION: inet_ntop ( n a a u -- a )

windows? [IF]
[DEFINED] EXTERN: [IF]
EXTERN: size_t "C" strlen(char *);
EXTERN: void* "C" malloc(size_t);
EXTERN: int "C" _time32(int *);  
EXTERN: void* "C" _localtime32(int *);  
EXTERN: void* "C" _gmtime32(int *);
EXTERN: size_t "C" strftime(char *, size_t, char*, void*);
EXTERN: void "C" perror(char *);
[ELSE]
FUNCTION: strlen ( a -- u )
FUNCTION: _time32 ( a -- n )
FUNCTION: _gmtime32 ( a -- a )
FUNCTION: _localtime32 ( a -- a )
FUNCTION: strftime ( a u a a -- u )
FUNCTION: perror ( a -- )
[THEN]
: time _time32 ;
: gmtime _gmtime32 ;
: localtime _localtime32 ;
[ELSE]
FUNCTION: strlen ( a -- u )
FUNCTION: malloc ( u -- a )
FUNCTION: gmtime ( a -- a )
FUNCTION: time ( a -- n )
FUNCTION: localtime ( a -- a )
FUNCTION: strftime ( a u a a -- u )
FUNCTION: perror ( a -- )
FUNCTION: mkstemp ( a -- n )
[THEN]

windows? [IF]
FUNCTION: ioctlsocket ( n n a -- n )
[ELSE]
FUNCTION: fcntl ( n n u -- n )
[THEN]

windows? 0= [IF]
FUNCTION: sigemptyset ( a -- n )
[THEN]

windows? [IF]
FUNCTION: WSAPoll ( a u n -- n )
: poll WSAPoll ;
[ELSE]
FUNCTION: poll ( a u n -- n )
[THEN]

[UNDEFINED] stat [IF]

windows? [IF]

[DEFINED] EXTERN: [IF]
EXTERN: int "C" _stat(char *, void *);
[ELSE]
FUNCTION: _stat ( a a -- n )
[THEN]
: stat _stat ;

[ELSE]

FUNCTION: stat ( a a -- n )

\ VFX Forth
[DEFINED] func-loaded? [IF]

' stat func-loaded? 0= [IF]

FUNCTION: __xstat ( n a a -- n )

3 CONSTANT _STAT_VER_LINUX

: stat _STAT_VER_LINUX -ROT __xstat ;

[THEN]
[THEN]
[THEN]
[THEN]
[THEN]

: >tm_sec ( addr -- addr )
;

: >tm_min ( addr -- addr )
   CELL+
;

: >tm_hour ( addr -- addr )
   2 CELLS +
;

: >tm_mday ( addr -- addr )
   3 CELLS +
;

: >tm_mon ( addr -- addr )
   4 CELLS +
;

: >tm_year ( addr -- addr )
   5 CELLS +
;
