######################## 1단계: build ########################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN echo "=== DEBUG find . -name \"*.csproj\" ===" && find . -name "*.csproj"
RUN dotnet publish ./OrderApi.csproj -c Release -o /out

######################## 2단계: runtime #######################
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-5100}
EXPOSE 5100

ENTRYPOINT ["dotnet","OrderApi.dll"]
