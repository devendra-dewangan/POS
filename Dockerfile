# ---------- Build Stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj first (better layer caching)
COPY POS/POS.csproj POS/
RUN dotnet restore POS/POS.csproj

# Copy remaining source
COPY POS/ POS/

# Publish (build included automatically)
RUN dotnet publish POS/POS.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Runtime Stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "POS.dll"]