#!/bin/sh

# This script will build and push an image of the web UI.
#
# You need to configure DOCKER_REGISTRY to point to a hostname of a docker registry.

set -e

pushd .

cd ../JsonMasher.Web

docker login $DOCKER_REGISTRY
docker build .. -f Dockerfile -t $DOCKER_REGISTRY/jsonmasher.web:latest
docker push $DOCKER_REGISTRY/jsonmasher.web:latest

popd