#===========================================#
#               DOTNET  BUILD               #
#===========================================#
FROM microsoft/dotnet:2.1-sdk as dotnet-build
ARG DOTNET_CONFIG=Release
COPY /app/*.csproj /build/
WORKDIR /build
RUN dotnet restore
COPY /app/ .
RUN dotnet publish -c ${DOTNET_CONFIG} -o ./results

#===========================================#
#               DOTNET  TEST                #
#===========================================#
FROM microsoft/dotnet:2.1-sdk as dotnet-test
WORKDIR /test
COPY --from=dotnet-build /build .
RUN dotnet test -c Test

#===========================================#
#               IMAGE BUILD                 #
#===========================================#
FROM microsoft/dotnet:2.1-aspnetcore-runtime as app
RUN apt-get update && apt-get install -y xvfb
RUN apt-get update && apt-get install -y gnupg gnupg2 gnupg1 wget --no-install-recommends \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    && apt-get update \
    && apt-get install -y google-chrome-stable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst ttf-freefont \
      --no-install-recommends \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get purge --auto-remove -y curl \
    && rm -rf /src/*.deb
ARG INSTALL_CLRDBG
RUN bash -c "${INSTALL_CLRDBG}"
WORKDIR /app
EXPOSE 5000
COPY --from="dotnet-build" /build/results .
COPY /shell/entrypoint.sh .
# Add user so we don't need --no-sandbox.
RUN groupadd -r pptruser && useradd -r -g pptruser -G audio,video pptruser \
    && mkdir -p /home/pptruser/Downloads \
    && mkdir -p /tmp/.X11-unix \
    && chown -R pptruser:pptruser /home/pptruser \
    && chown -R pptruser:pptruser /app \
    && find /app -type d -exec chmod 2775 {} \; \
    && find /app -type f -exec chmod ug+rw {} \; \
    && chmod +x /app/entrypoint.sh
# Run everything after as non-privileged user.
USER pptruser
CMD ./entrypoint.sh
