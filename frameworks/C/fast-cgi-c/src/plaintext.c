#include <fcgi_stdio.h>
#include <stdlib.h>

int main (void) {
    while (FCGI_Accept() >= 0) {
        printf("Status: 200 OK\r\n");
        printf("Content-type: text/html\r\n\r\n");
        printf("Hello, World!");
    }
    return EXIT_SUCCESS;
}
