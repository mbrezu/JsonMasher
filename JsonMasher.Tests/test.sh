#!/bin/sh

set -e

rm -rf TestResults
dotnet test --collect:"XPlat Code Coverage"
REPORTFILE=`find . | grep coverage.cobertura.xml`
reportgenerator \
    -reports:$REPORTFILE \
    -targetdir:TestResults/html

