\ App demo:
include 1991/1991.fs

jos-create json-message

sourcedir s" public" s+ set-public-path
sourcedir s" views/" s+ set-view-path

: handle-plaintext 
    s" text/plain" set-content-type
    s" Hello, World!" 
    dup set-content-length
;

: handle-json
    s" application/json" set-content-type
    json-message jos-init
    json-message jos-write-start-object 
    s" message" json-message jos-write-name
    s" Hello, World!" json-message jos-write-string
    json-message jos-write-end-object 
    json-message str-get
    dup set-content-length
;

: handle-hi 
    s" hi!" 
;

\ Basic routing:
/1991 /plaintext handle-plaintext 

/1991 /json handle-json

/1991 /hi handle-hi

\ Views:
: page-title s" hmmmm" ;

: handle-/index
    s" index.html" render-view ;

: handle-import
    s" import-version.html" render-view ;

/1991 /index handle-/index

/1991 /import handle-import

: handle-uid
    s" uid:" get-query-string s+ ;

: handle-uid-new
    s" uid:new:" get-query-string s+ ;

: handle-uid-delete
    s" uid:delete:" get-query-string s+ ;

: handle-pid
    s" pid:" get-query-string s+ ;

: handle-pid-new
    s" pid:new:" get-query-string s+ ;

: handle-pid-delete
    s" pid:delete:" get-query-string s+ ;

/1991 /api/v1/users/<uid> handle-uid

/1991 /api/v1/users/<uid>/new handle-uid-new

/1991 /api/v1/users/<uid>/delete handle-uid-delete

/1991 /api/v1/users/<uid>/post/<pid> handle-pid

/1991 /api/v1/users/<uid>/post/<pid>/new handle-pid-new

/1991 /api/v1/users/<uid>/post/<pid>/delete handle-pid-delete

8080 1991:
