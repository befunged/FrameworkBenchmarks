package com.techempower.frameworks.java.nginx;

import nginx.clojure.java.ArrayMap;
import nginx.clojure.java.NginxJavaRingHandler;

import java.util.Map;

import static nginx.clojure.MiniConstants.CONTENT_TYPE;
import static nginx.clojure.MiniConstants.NGX_HTTP_OK;

public  class HelloWorld implements NginxJavaRingHandler {

    public Object[] invoke(Map<String, Object> request) {
        return new Object[] {
                NGX_HTTP_OK, //http status 200
                ArrayMap.create(CONTENT_TYPE, "text/plain"), //headers map
                "Hello, Java & NGINX!"  //response body can be string, File or Array/Collection of them
        };
    }
}