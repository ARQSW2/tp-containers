#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UNAHUR.MessageFun.Worker/UNAHUR.MessageFun.Worker.csproj", "UNAHUR.MessageFun.Worker/"]
COPY ["UNAHUR.MessageFun.Business/UNAHUR.MessageFun.Business.csproj", "UNAHUR.MessageFun.Business/"]
RUN dotnet restore "UNAHUR.MessageFun.Worker/UNAHUR.MessageFun.Worker.csproj"
COPY . .
WORKDIR "/src/UNAHUR.MessageFun.Worker"
RUN dotnet build "UNAHUR.MessageFun.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UNAHUR.MessageFun.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UNAHUR.MessageFun.Worker.dll"]