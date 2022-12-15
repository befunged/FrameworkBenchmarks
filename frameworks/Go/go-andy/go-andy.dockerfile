FROM golang:1.15

ADD ./ /go
WORKDIR /go/src

RUN go install com.example/andy/tfb
CMD tfb

