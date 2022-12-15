FROM ubuntu:precise

ARG DEBIAN_FRONTEND=noninteractive
ARG TERM=linux

RUN apt-get update && apt-get install -y gforth curl libtool 
EXPOSE 8080

ADD ./ ./
RUN cd 1991/examples

CMD gforth app.fs





