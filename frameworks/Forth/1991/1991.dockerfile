# FROM ubuntu:precise
FROM debian:sid

ARG DEBIAN_FRONTEND=noninteractive
ARG TERM=linux

# RUN cd /etc/apt && mkdir test && cp sources.lst test && cd test && sed -i -- 's/us.archive/old-releases/g' * && sed -i -- 's/security/old-releases/g' * && cp sources.lst ../
RUN apt-get update && apt-get install -y gforth curl libtool 
EXPOSE 8080

COPY ffl /usr/share/gforth/0.7.0/ffl

ADD ./ ./

CMD gforth app.fs


