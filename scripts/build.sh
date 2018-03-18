#!/usr/bin/env bash
dotnet clean -c Release -o ./dist
dotnet SonarScanner.MSBuild.dll begin /k:"ojji.redisearchsharp" /d:sonar.organization="ojji-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login=$SONAR_TOKEN
dotnet build ./RediSearchSharp.sln -c Release -o ./dist
dotnet SonarScanner.MSBuild.dll end /d:sonar.login=$SONAR_TOKEN