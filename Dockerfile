# Adjust DOTNET_OS_VERSION as desired
ARG DOTNET_OS_VERSION="-alpine"
ARG DOTNET_SDK_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}${DOTNET_OS_VERSION} AS build
WORKDIR /src

# copy everything
COPY ./VitalTrack.sln ./
COPY ./src ./src
COPY ./tests ./tests
COPY ./templates ./templates

# Keep ourselves honest here, run tests as a sanity check before deploying
RUN dotnet test

# restore as distinct layers
RUN dotnet restore

# build and publish a release
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_SDK_VERSION}

ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production

EXPOSE 8080

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT [ "dotnet", "VitalTrack.Web.dll" ]
