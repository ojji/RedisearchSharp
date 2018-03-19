#!/usr/bin/env bash
dotnet test -c Release --logger:trx --no-build --no-restore ./RediSearchSharp.Tests/RediSearchSharp.Tests.csproj -o ./dist -r "$TEST_RESULTS_PATH"