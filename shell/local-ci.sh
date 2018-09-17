#!/bin/sh
set -e
TAG=$(date +"%Y-%m-%d--%H-%M-%S")
PROJ=schemes-screenshots
REGISTRY=localhost:5000
IMAGE=$REGISTRY/$PROJ:$TAG
eval $(docker-machine env default --shell bash)
docker build -t $IMAGE \
    --build-arg DOTNET_CONFIG=Build \
    --build-arg INSTALL_CLRDBG="apt-get update \
        && apt-get install -y --no-install-recommends unzip curl \
        && rm -rf /var/lib/apt/lists/* \
        && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg" \
    ../
kubectl config use-context minikube
docker push $IMAGE

# getting value from local env var
cloudinaryUrl=$(echo -n "$CLOUDINARY_URL" | base64 -w 0);

    # --set env.SCREENSHOT_SIZES=1280x800x200~640x400x200 \
    # --set env.SCREENSHOT_OUT_DIR=./wwwroot \
    # --set env.SCREENSHOT_URLS=https://www.google.com/search?hl\=en\&q\={colorSchemeName} \

helm upgrade --install --set image=$IMAGE \
    --set env.ASPNETCORE_ENVIRONMENT=Development \
    --set secrets.cloudinaryUrl=$cloudinaryUrl \
    --set livenessProbe.initialDelaySeconds=30 \
    --set livenessProbe.periodSeconds=90 \
    --set livenessProbe.timeoutSeconds=60 \
    --set readinessProbe.initialDelaySeconds=20 \
    --set readinessProbe.periodSeconds=30 \
    --set readinessProbe.timeoutSeconds=30 \
    --set screenshotConfig.cdnIdTemplate=tmp/{id}/{title}/{size} \
    --set screenshotConfig.cdnPrefixTemplate=tmp/{id}/ \
    $PROJ ../kube/$PROJ
