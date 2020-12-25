#! /usr/bin/gforth-fast
warnings off
include ffl/jos.fs

: message-text
	s" Hello, World!"
;

: content-type
	." Content-type: application/json" cr cr
;	

content-type
jos-create jos1
jos1 jos-write-start-object 
s" message" jos1 jos-write-name   
message-text jos1 jos-write-string
jos1 jos-write-end-object  
jos1 str-get type cr 

bye
