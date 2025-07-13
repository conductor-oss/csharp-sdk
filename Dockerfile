FROM mcr.microsoft.com/dotnet/sdk:8.0 AS csharp-sdk
RUN mkdir /package
COPY /Conductor /package/Conductor
COPY /README.md /package/Conductor/README.md
WORKDIR /package/Conductor

FROM csharp-sdk AS linter
RUN dotnet format --verify-no-changes *.csproj

FROM csharp-sdk AS build
RUN dotnet build *.csproj

FROM build AS test
ARG KEY
ARG SECRET
ARG CONDUCTOR_SERVER_URL
ENV CONDUCTOR_AUTH_KEY=${KEY}
ENV CONDUCTOR_AUTH_SECRET=${SECRET}
ENV CONDUCTOR_SERVER_URL=${CONDUCTOR_SERVER_URL}

COPY /csharp-examples /package/csharp-examples
COPY /Tests           /package/Tests
WORKDIR /package/Tests
# collect coverage while testing
RUN dotnet test -p:DefineConstants=EXCLUDE_EXAMPLE_WORKERS \
                --collect:"XPlat Code Coverage" \
                -l "console;verbosity=normal" \
    || true

##########   coverage_export   ##########
FROM test AS coverage_export
RUN mkdir /out \
 && cp $(find /package/Tests/TestResults -name 'coverage.cobertura.xml' | head -n 1) \
       /out/coverage.cobertura.xml

FROM build AS pack_release
ARG SDK_VERSION
RUN dotnet pack conductor-csharp.csproj \
    -o /build \
    --include-symbols \
    --include-source \
    -c Release \
    "/p:Version=${SDK_VERSION}"

FROM pack_release AS publish_release
ARG NUGET_SRC
ARG NUGET_API_KEY
RUN dotnet nuget push "/build/*.symbols.nupkg" \
    --source "${NUGET_SRC}" \
    --api-key "${NUGET_API_KEY}"
