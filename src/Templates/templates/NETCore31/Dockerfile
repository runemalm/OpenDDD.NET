FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
EXPOSE 80

COPY src/MyBoundedContext.sln ./
COPY src/MyBoundedContext/MyBoundedContext.csproj ./MyBoundedContext/

RUN dotnet restore

COPY src .

RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Main.dll"]
