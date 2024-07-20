FROM ortussolutions/commandbox:3.6.4

ENV cfconfig_adminPassword=password
ENV box_server_runwar_args=--cache-servlet-paths=true

COPY ./src/server-adobe.json /app/server.json
COPY ./src/.cfconfig.json /app/.cfconfig.json
COPY ./src/box.json /app/box.json

RUN box install --verbose --force

RUN ${BUILD_DIR}/util/warmup-server.sh

RUN cd /app && box cfpm install postgresql,feed

RUN export FINALIZE_STARTUP=true;$BUILD_DIR/run.sh;unset FINALIZE_STARTUP

HEALTHCHECK NONE

EXPOSE 8080

COPY ./src /app/