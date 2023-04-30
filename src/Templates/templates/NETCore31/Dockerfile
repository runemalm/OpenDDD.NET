FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
EXPOSE 80

COPY src/Weather.sln ./
COPY src/Application/Application.csproj ./Application/
COPY src/Domain/Domain.csproj ./Domain/
COPY src/Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY src/Main/Main.csproj ./Main/
COPY src/Interchange/Interchange.csproj ./Interchange/
COPY src/Tests/Tests.csproj ./Tests/

RUN dotnet restore

COPY src .

RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Main.dll"]
