FROM mcr.microsoft.com/dotnet/sdk:8.0 AS csharp-sdk
RUN mkdir /package
COPY /Conductor /package/Conductor
COPY /README.md /package/Conductor/README.md
COPY /README.md /package/README.md
COPY /Conductor.AI /package/Conductor.AI
COPY /Conductor.AI.OpenAI /package/Conductor.AI.OpenAI
COPY /Conductor.AI.GoogleADK /package/Conductor.AI.GoogleADK
COPY /Conductor.AI.SemanticKernel /package/Conductor.AI.SemanticKernel
WORKDIR /package/Conductor

FROM csharp-sdk AS linter
RUN dotnet format --verify-no-changes *.csproj
RUN dotnet format --verify-no-changes ../Conductor.AI/Conductor.AI.csproj
RUN dotnet format --verify-no-changes ../Conductor.AI.OpenAI/Conductor.AI.OpenAI.csproj
RUN dotnet format --verify-no-changes ../Conductor.AI.GoogleADK/Conductor.AI.GoogleADK.csproj
RUN dotnet format --verify-no-changes ../Conductor.AI.SemanticKernel/Conductor.AI.SemanticKernel.csproj

FROM csharp-sdk AS build
RUN dotnet build *.csproj

FROM build AS harness-build
COPY /Harness /package/Harness
WORKDIR /package/Harness
RUN dotnet publish Harness.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS harness
COPY --from=harness-build /app /app
WORKDIR /app
EXPOSE 9991
ENTRYPOINT ["dotnet", "Harness.dll"]

FROM build AS pack_release
ARG SDK_VERSION
RUN dotnet pack conductor-csharp.csproj \
    -o /build \
    --include-symbols \
    --include-source \
    -c Release \
    "/p:Version=${SDK_VERSION}"
RUN dotnet pack ../Conductor.AI/Conductor.AI.csproj \
    -o /build \
    --include-symbols \
    --include-source \
    -c Release \
    "/p:Version=${SDK_VERSION}"
RUN dotnet pack ../Conductor.AI.OpenAI/Conductor.AI.OpenAI.csproj \
    -o /build \
    --include-symbols \
    --include-source \
    -c Release \
    "/p:Version=${SDK_VERSION}"
RUN dotnet pack ../Conductor.AI.GoogleADK/Conductor.AI.GoogleADK.csproj \
    -o /build \
    --include-symbols \
    --include-source \
    -c Release \
    "/p:Version=${SDK_VERSION}"
RUN dotnet pack ../Conductor.AI.SemanticKernel/Conductor.AI.SemanticKernel.csproj \
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
