#!/bin/sh

ORIGIN=$1
HEAD=$2
git_diff_with_abs_path() {
    local path
	echo $ORIGIN
	echo $HEAD
    path=`$(git rev-parse --show-toplevel) &&
   git diff $ORIGIN...$HEAD --name-only | grep -E ".*(\.cs)$" | sed "s,^,$path/,"`
}

DOT_NET=$(which dotnet)
CODE_ANALYZER="$(pwd -P)/CSharpCodeAnalyzer5.dll"
RESULT_FILE_CACHE="$(pwd -P)/result.txt"
RESULT=0
DIFF=$(git_diff_with_abs_path)

for file in $DIFF; do
	echo $file
	if [ ! -e "$CODE_ANALYZER" ]; then
	    echo "CodeAnalyzer not exist!"
	    exit 0
    fi
    ("$DOT_NET" "$CODE_ANALYZER" "$file") >> $RESULT_FILE_CACHE
	RESULT=$((RESULT+$?))
done

if [ $RESULT -ne 0 ]; then
	echo "Something was wrong. output result"
	cat $RESULT_FILE_CACHE
fi

rm $RESULT_FILE_CACHE
exit $RESULT
