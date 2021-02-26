#!/bin/sh

DOT_NET=$(which dotnet)
CODE_ANALYZER="$(pwd -P)/CSharpCodeAnalyer5.dll"
FILE_PATTERN=".*(\.cs)$"
RESULT_FILE_CACHE="$(pwd -P)/result.txt"
RESULT=0
for file in $(git diff-index --cached --nameonly HEAD | grep -E "$FILE_PATTERN"); do
    if [ ! -e $CODE_ANALYZER ]; then
	    echo "CodeAnalyzer not exist!"
	    exit 0
    fi
    RESULT=$DOTNET $CODE_ANALYZER $file >> $RESULT_FILE_CACHE
done

if [ $RESULT -ne 0 ]; then
	echo "Something was wrong. output result"
	cat $RESULT_FILE_CACHE
fi

echo "Finish"
exit $RESULT
