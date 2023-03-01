FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt update
RUN apt install -y npm nodejs
WORKDIR /app
COPY . .
WORKDIR /app/JsonMasher.Web
RUN dotnet restore
RUN dotnet build
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/JsonMasher.Web/out ./
ENTRYPOINT ["dotnet", "JsonMasher.Web.dll"]
