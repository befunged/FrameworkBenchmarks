FROM golang:1.15

ADD ./ /go
WORKDIR /go/src

# install mysql driver
RUN go get -u github.com/go-sql-driver/mysql

# build and run http server
RUN go install frameworkless
CMD frameworkless

