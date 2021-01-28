#!/usr/bin/sh

# usage: ./publish.sh <api key>

VERSION=0.0.1

set -e

cd JsonMasher
dotnet build -c Release
dotnet pack -c Release

cd ..
dotnet test

dotnet nuget push JsonMasher/bin/Release/JsonMasher.$VERSION.nupkg --api-key $1 --source https://api.nuget.org/v3/index.json --skip-duplicate
