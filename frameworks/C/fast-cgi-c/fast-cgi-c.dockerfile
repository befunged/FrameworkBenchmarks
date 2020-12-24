FROM ubuntu:20.04

COPY ./ ./

RUN groupadd wwwgroup && useradd -G wwwgroup wwwuser --badnames --no-create-home --no-user-group

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update && apt-get install -yqq libtool autoconf git make wget lighttpd


# get and build fast cgi lib
RUN git clone https://github.com/FastCGI-Archives/fcgi2 && \
	cd fcgi2/ && \
	./autogen.sh && \
	./configure && \
	make && \
	make install

# compile plaintext server
RUN cp src/plaintext.c . && \
    gcc plaintext.c -o plaintext.fcgi -Wl,-rpath /usr/local/lib -lfcgi -O3 -Wall -Wextra -pedantic -std=c11
	
# RUN cp config/lighttpd.conf .

RUN chown wwwuser plaintext.fcgi

EXPOSE 8080

CMD ["lighttpd", "-D",  "-f", "config/lighttpd.conf"]


