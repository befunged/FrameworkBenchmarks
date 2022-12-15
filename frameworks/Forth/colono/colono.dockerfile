FROM debian:jessie

ARG DEBIAN_FRONTEND=noninteractive
ARG TERM=linux

# RUN apt-get update && apt-get install -y gforth curl libtool
RUN apt-get update && apt-get install -y gforth curl libtool libtool-bin
EXPOSE 8080

COPY ./ ./

CMD gforth httpd/colono.fs





