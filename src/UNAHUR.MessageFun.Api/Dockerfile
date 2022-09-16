#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
# DEFAULT HTTPPORT
ENV Kestrel__Endpoints__Http__Url  http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UNAHUR.MessageFun.Api/UNAHUR.MessageFun.Api.csproj", "UNAHUR.MessageFun.Api/"]
COPY ["UNAHUR.MessageFun.Business/UNAHUR.MessageFun.Business.csproj", "UNAHUR.MessageFun.Business/"]
RUN dotnet restore "UNAHUR.MessageFun.Api/UNAHUR.MessageFun.Api.csproj"
COPY . .
WORKDIR "/src/UNAHUR.MessageFun.Api"
RUN dotnet build "UNAHUR.MessageFun.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UNAHUR.MessageFun.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UNAHUR.MessageFun.Api.dll"]