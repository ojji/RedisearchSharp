#!/usr/bin/env bash
dotnet clean -c Release -o ../dist
dotnet build ../RediSearchSharp.sln -c Release -o ../dist