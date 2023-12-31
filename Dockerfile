FROM mcr.microsoft.com/dotnet/sdk:8.0 AS TikholeBuild
COPY . /TikholeBuild
WORKDIR /TikholeBuild
RUN dotnet restore
RUN dotnet publish -c release -o /Tikhole.Website --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /Tikhole.Website
COPY --from=Tikhole.Website /Tikhole.Website .
ENTRYPOINT ./Tikhole.Website