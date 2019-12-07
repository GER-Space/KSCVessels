#/usr/bin/bash
#
#
PROJECTNAME=KSCVessels

cd /c/Git/${PROJECTNAME}

VERSION=`grep -i AssemblyFileVersion src/Properties/AssemblyInfo.cs  | cut -d "\"" -f 2`


MAJOR=`echo ${VERSION} | cut -d "." -f 1 `
MINOR=`echo ${VERSION} | cut -d "." -f 2 `
PATCH=`echo ${VERSION} | cut -d "." -f 3 `
BUILD=`echo ${VERSION} | cut -d "." -f 4 `


if (test $1 != "") ; then

VERSION=${VERSION}_$1
fi

FILENAME="${PROJECTNAME}-${VERSION}.zip"



if test -e $FILENAME ; then
	rm -f  ${PROJECTNAME}-${VERSION}.zip
fi


sed -i "s/MAJOR\":./MAJOR\":$MAJOR/" GameData/${PROJECTNAME}/${PROJECTNAME}.version
sed -i "s/MINOR\":./MINOR\":$MINOR/" GameData/${PROJECTNAME}/${PROJECTNAME}.version
sed -i "s/PATCH\":./PATCH\":$PATCH/" GameData/${PROJECTNAME}/${PROJECTNAME}.version
sed -i "s/BUILD\":.*/BUILD\":$BUILD/" GameData/${PROJECTNAME}/${PROJECTNAME}.version


/c/Program\ Files/7-Zip/7z.exe a -r  $FILENAME GameData

sleep 2