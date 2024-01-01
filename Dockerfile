FROM mcr.microsoft.com/dotnet/sdk:8.0 AS tikholebuild
COPY . /tikholebuild
WORKDIR /tikholebuild
RUN dotnet restore
RUN dotnet publish Tikhole.Website -c Release -o /Tikhole.Website --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /Tikhole.Website
COPY --from=tikholebuild /Tikhole.Website .
ENTRYPOINT ./Tikhole.Website