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
        && apt-get install -y --no-install-recommends unzip \
        && rm -rf /var/lib/apt/lists/* \
        && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg" \
    ../
kubectl config use-context minikube
docker push $IMAGE

# getting value from local env var
cloudinaryUrl=$($CLOUDINARY_URL | base64 -w 0);

    # --set env.SCREENSHOT_SIZES=1280x800x200~640x400x200 \
    # --set env.SCREENSHOT_OUT_DIR=./wwwroot \

helm upgrade --install --set image=$IMAGE \
    --set env.ASPNETCORE_ENVIRONMENT=Development \
    --set env.SCREENSHOT_URLS=https://www.google.com/search?hl\=en\&q\={colorSchemeName} \
    --set secrets.cloudinaryUrl=$cloudinaryUrl \
    $PROJ ../kube/$PROJ
