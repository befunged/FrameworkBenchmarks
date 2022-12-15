\
\ HTTP status codes
\
\ Copyright (c) 2016 René Hartmann.
\ See the file LICENSE for redistribution information.
\

100 CONSTANT status-continue
101 CONSTANT status-switching-protocols
200 CONSTANT status-ok
201 CONSTANT status-created
202 CONSTANT status-accepted
203 CONSTANT status-non-authoritative-information
204 CONSTANT status-no-content
205 CONSTANT status-reset-content
206 CONSTANT status-partial-content
300 CONSTANT status-multiple-choices
301 CONSTANT status-moved-permanently
302 CONSTANT status-found
303 CONSTANT status-see-other
304 CONSTANT status-not-modified
305 CONSTANT status-use-proxy
307 CONSTANT status-temporary-redirect
400 CONSTANT status-bad-request
401 CONSTANT status-unauthorized
402 CONSTANT status-payment-required
403 CONSTANT status-forbidden
404 CONSTANT status-not-found
405 CONSTANT status-method-not-allowed
406 CONSTANT status-not-acceptable
407 CONSTANT status-proxy-authentication-required
408 CONSTANT status-request-timeout
409 CONSTANT status-conflict
410 CONSTANT status-gone
411 CONSTANT status-length-required
412 CONSTANT status-precondition-failed
413 CONSTANT status-payload-too-large
414 CONSTANT status-uri-too-long
415 CONSTANT status-unsupported-media-type
416 CONSTANT status-range-not-satisfiable
417 CONSTANT status-expectation-failed
426 CONSTANT status-upgrade-required
451 CONSTANT status-unavailable
500 CONSTANT status-internal-server-error
501 CONSTANT status-not-implemented
502 CONSTANT status-bad-gateway
503 CONSTANT status-service-unavailable
504 CONSTANT status-gateway-timeout
505 CONSTANT status-http-version-not-supported

: status-phrase ( u1 -- c-addr u2 )
   CASE
      status-continue OF S" Continue" ENDOF
      status-switching-protocols OF S" Switching Protocols" ENDOF
      status-ok OF S" OK" ENDOF
      status-created OF S" Created" ENDOF
      status-accepted OF S" Accepted" ENDOF
      status-non-authoritative-information OF S" Non-Authoritative Information" ENDOF
      status-no-content OF S" No Content" ENDOF
      status-reset-content OF S" Reset Content" ENDOF
      status-partial-content OF S" Partial Content" ENDOF
      status-multiple-choices OF S" Multiple Choices" ENDOF
      status-moved-permanently OF S" Moved Permanently" ENDOF
      status-found OF S" Found" ENDOF
      status-see-other OF S" See Other" ENDOF
      status-not-modified OF S" Not Modified" ENDOF
      status-use-proxy OF S" Use Proxy" ENDOF
      status-temporary-redirect OF S" Temporary Redirect" ENDOF
      status-bad-request OF S" Bad Request" ENDOF
      status-unauthorized OF S" Unauthorized" ENDOF
      status-payment-required OF S" Payment Required" ENDOF
      status-forbidden OF S" Forbidden" ENDOF
      status-not-found OF S" Not Found" ENDOF
      status-method-not-allowed OF S" Method Not Allowed" ENDOF
      status-not-acceptable OF S" Not Acceptable" ENDOF
      status-proxy-authentication-required OF S" Proxy Authentication Required" ENDOF
      status-request-timeout OF S" Request Timeout" ENDOF
      status-conflict OF S" Conflict" ENDOF
      status-gone OF S" Gone" ENDOF
      status-length-required OF S" Length Required" ENDOF
      status-precondition-failed OF S" Precondition Failed" ENDOF
      status-payload-too-large OF S" Payload Too Large" ENDOF
      status-uri-too-long OF S" URI Too Long" ENDOF
      status-unsupported-media-type OF S" Unsupported Media Type" ENDOF
      status-range-not-satisfiable OF S" Range Not Satisfiable" ENDOF
      status-expectation-failed OF S" Expectation Failed" ENDOF
      status-upgrade-required OF S" Upgrade Required" ENDOF
      status-unavailable OF S" Unavailable For Legal Reasons" ENDOF
      status-internal-server-error OF S" Internal Server Error" ENDOF
      status-not-implemented OF S" Not Implemented" ENDOF
      status-bad-gateway OF S" Bad Gateway" ENDOF
      status-service-unavailable OF S" Service Unavailable" ENDOF
      status-gateway-timeout OF S" Gateway Timeout" ENDOF
      status-http-version-not-supported OF S" HTTP Version Not Supported" ENDOF
      S" Unknown" ROT
   ENDCASE
;
