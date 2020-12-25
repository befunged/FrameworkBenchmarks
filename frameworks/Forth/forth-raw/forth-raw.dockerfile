FROM ubuntu:xenial
RUN apt-get update && apt-get install -y --no-install-recommends gforth apache2 wget
EXPOSE 80 443

RUN mkdir /var/www/cgi-bin/
COPY src/json.fs /var/www/cgi-bin/json

RUN chmod +x /var/www/cgi-bin/json
COPY /src/plaintext.fs /var/www/cgi-bin/plaintext
RUN chmod +x /var/www/cgi-bin/plaintext

COPY ffl /usr/share/gforth/0.7.2/ffl

COPY config/serve-cgi-bin.conf /etc/apache2/conf-available/
COPY config/mpm_event.conf /etc/apache2/mods-available/
RUN ln -s /etc/apache2/mods-available/cgi.load /etc/apache2/mods-enabled/cgi.load

CMD apachectl -DFOREGROUND





