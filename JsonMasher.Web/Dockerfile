FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
RUN apk add --update nodejs npm
WORKDIR /app
COPY . .
WORKDIR /app/JsonMasher.Web
RUN dotnet restore
RUN dotnet build
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/JsonMasher.Web/out ./
ENTRYPOINT ["dotnet", "JsonMasher.Web.dll"]
