#!/usr/bin/env bash
dotnet test -c Release --logger:trx --no-build --no-restore ./RediSearchSharp.Test/RediSearchSharp.Test.csproj -o ./dist -r "$TEST_RESULTS_PATH"