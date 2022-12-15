package main

import (
	"log"
	"net/http"
)

func main() {

	http.HandleFunc("/plaintext", plaintextHandler)

	log.Fatal(http.ListenAndServe(":8080", nil))
}

var plaintextBody = []byte("Hello, World!")

func plaintextHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Server", "Go")
	w.Header().Set("Content-Type", "text/plain")
	w.Write(plaintextBody)
}
