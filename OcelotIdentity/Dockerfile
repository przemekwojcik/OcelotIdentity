FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY OcelotIdentity/OcelotIdentity.csproj OcelotIdentity/
RUN dotnet restore OcelotIdentity/OcelotIdentity.csproj
COPY . .
WORKDIR /src/OcelotIdentity
RUN dotnet build OcelotIdentity.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish OcelotIdentity.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OcelotIdentity.dll"]
