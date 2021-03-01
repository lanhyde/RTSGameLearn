#!/bin/sh

if command -v dotnet &> /dev/null; then
    exit 1
fi

exit 0