FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["BudgetBot/BudgetBot.csproj", "BudgetBot/"]
RUN dotnet restore "BudgetBot/BudgetBot.csproj"
COPY . .
WORKDIR "/src/BudgetBot"
RUN dotnet build "BudgetBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BudgetBot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BudgetBot.dll"]