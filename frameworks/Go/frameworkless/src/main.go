package main

import (
	"database/sql"
	"encoding/json"
	_ "github.com/go-sql-driver/mysql"
	"log"
	"math/rand"
	"net/http"
	"strconv"
	"time"
)

var (
	db                  *sql.DB
	dbPreparedStatement *sql.Stmt
)

func main() {

	initDB()
	defer closeDB()

	http.HandleFunc("/plaintext", plaintextHandler)
	http.HandleFunc("/json", jsonHandler)
	http.HandleFunc("/db", dbHandler)
	http.HandleFunc("/query", queryHandler)

	log.Fatal(http.ListenAndServe(":8080", nil))
}

func initDB() {
	db, err := sql.Open("mysql", "benchmarkdbuser:benchmarkdbpass@tcp(tfb-database:3306)/hello_world")
	if err != nil {
		panic(err)
	}

	// See "Important settings" section.
	db.SetConnMaxLifetime(time.Minute * 3)
	db.SetMaxOpenConns(10)
	db.SetMaxIdleConns(10)

	dbPreparedStatement, err = db.Prepare("select id, randomnumber from world where id = ?")
	if err != nil {
		panic(err)
	}
}

func closeDB() {
	err := db.Close()
	if err != nil {
		panic(err)
	}
}

var plaintextBody = []byte("Hello, World!")
var serverKey = "Server"
var serverValue = "Go net/http"
var contentTypeKey = "Content-Type"
var contentTypeValueText = "text/plain"

func plaintextHandler(w http.ResponseWriter, r *http.Request) {

	w.Header().Set(serverKey, serverValue)
	w.Header().Set(contentTypeKey, contentTypeValueText)
	w.Write(plaintextBody)
}

type JsonMessageStruct struct {
	Message string `json:"message"`
}

var jsonMessageText = "Hello, World!"
var contentTypeValueJson = "application/json"

func jsonHandler(w http.ResponseWriter, r *http.Request) {

	w.Header().Set(serverKey, serverValue)
	w.Header().Set(contentTypeKey, contentTypeValueJson)
	jsonMessageStruct := JsonMessageStruct{jsonMessageText}
	jsonMessage, _ := json.Marshal(jsonMessageStruct)
	w.Write(jsonMessage)
}

type WorldStruct struct {
	Id           int `json:"id"`
	RandomNumber int `json:"randomnumber"`
}

func queryRandomWorldRow() WorldStruct {
	worldStruct := WorldStruct{}
	randomId := rand.Intn(10000) + 1
	err := dbPreparedStatement.QueryRow(randomId).Scan(&worldStruct.Id, &worldStruct.RandomNumber)
	if err != nil {
		panic(err)
	}
	return worldStruct
}

func dbHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set(serverKey, serverValue)
	w.Header().Set(contentTypeKey, contentTypeValueJson)

	worldStruct := queryRandomWorldRow()
	jsonMessage, _ := json.Marshal(worldStruct)
	w.Write(jsonMessage)
}

func queryHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set(serverKey, serverValue)
	w.Header().Set(contentTypeKey, contentTypeValueJson)

	numQueries := readQueriesQueryParam(r)

	messages := make([]WorldStruct, numQueries)

	for i := 0; i < numQueries; i++ {
		messages[i] = queryRandomWorldRow()
	}

	jsonMessage, _ := json.Marshal(messages)
	w.Write(jsonMessage)
}

func readQueriesQueryParam(r *http.Request) int {

	queriesStr := r.URL.Query().Get("queries")

	queries, err := strconv.Atoi(queriesStr)
	if err != nil {
		return 1
	}

	if queries < 1 {
		return 1
	}

	if queries > 500 {
		return 500
	}

	return queries
}
