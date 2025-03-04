FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet restore

RUN mv /app/WithingsToGarminSync/appsettings.stub.json /app/WithingsToGarminSync/appsettings.json

RUN dotnet publish WithingsToGarminSync/WithingsToGarminSync.csproj \
    -c Release -r linux-x64 --self-contained true \
    /p:PublishSingleFile=true \
    /p:EnableCompressionInSingleFile=true \
    -o /publish

FROM debian:stable-slim
WORKDIR /app

RUN apt-get update && apt-get upgrade -y && \
    apt-get install -y cron libicu72 && \
    rm -rf /var/lib/apt/lists/*
    
RUN mkdir -p /app/data/log
COPY --from=build /publish/WithingsToGarminSync /app/
COPY --from=build /publish/appsettings.json /app/appsettings.json

# cronjob to run
COPY <<EOF /app/cronjob.sh
#!/bin/sh
cd /app
./WithingsToGarminSync >> /var/log/cron.log 2>&1
EOF

# cron schedule setup
COPY <<EOF /app/entrypoint.sh
#!/bin/sh
set -e

# environment variables for cron
env | grep -v "no_proxy" > /etc/environment

if [ -z "\$CRON_SCHEDULE" ]; then
  echo "CRON_SCHEDULE not set, using default value '0 * * * *'"
  CRON_SCHEDULE="0 * * * *"
fi

echo "\$CRON_SCHEDULE . /etc/environment; /app/cronjob.sh" > /etc/cron.d/app-cron

chmod 0644 /etc/cron.d/app-cron
crontab /etc/cron.d/app-cron
touch /var/log/cron.log

echo "Cron is running: \$CRON_SCHEDULE"
cron && tail -f /var/log/cron.log
EOF

RUN chmod +x /app/WithingsToGarminSync
RUN chmod +x /app/cronjob.sh
RUN chmod +x /app/entrypoint.sh

CMD ["/app/entrypoint.sh"]
