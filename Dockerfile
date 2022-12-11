FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

ARG NODE_VERSION=18.12.1
RUN wget -qO- https://raw.githubusercontent.com/nvm-sh/nvm/v0.39.2/install.sh | bash
ARG NVM_DIR=/root/.nvm
RUN . "$NVM_DIR/nvm.sh" && nvm install $NODE_VERSION
RUN . "$NVM_DIR/nvm.sh" && nvm use $NODE_VERSION
RUN . "$NVM_DIR/nvm.sh" && nvm alias default node
ENV PATH="/root/.nvm/versions/node/v$NODE_VERSION/bin/:${PATH}"
RUN node --version
RUN npm i -g npm@latest
RUN npm --version

COPY *.csproj .
RUN dotnet restore

COPY . .
WORKDIR /source
RUN dotnet publish -c release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "RecommendationApp.dll"]