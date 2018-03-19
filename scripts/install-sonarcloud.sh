#!/usr/bin/env bash
SCANNER_HOME="$HOME/.sonarscanner"
if [ -z "$SCANNER_VERSION" ]; then
	SCANNER_VERSION="4.1.0.1148"
fi
SCANNER_REPO="https://github.com/SonarSource/sonar-scanner-msbuild/releases/download"
echo "Installing SonarCloud Scanner..."
# check which version to download
SCANNER_ZIP="sonar-scanner-msbuild-$SCANNER_VERSION"
if [[ -n "$SONAR_USE_NETCOREAPP20" ]]; then
	echo " - using netcoreapp2.0 version"
	SCANNER_ZIP="$SCANNER_ZIP-netcoreapp2.0"
elif [[ -n "$SONAR_USE_NET46" ]]; then
	echo " - using net46 version"
	SCANNER_ZIP="$SCANNER_ZIP-net46"
else
	echo " - using standard version"
fi
SCANNER_ZIP+=".zip"
# cleaning directory & downloading the scanner
rm -rf "$SCANNER_HOME"
mkdir -p "$SCANNER_HOME"
echo "Downloading zip file from $SCANNER_REPO/$SCANNER_VERSION/$SCANNER_ZIP"
curl -sSLo "$SCANNER_HOME/sonar-scanner.zip" "$SCANNER_REPO/$SCANNER_VERSION/$SCANNER_ZIP"
unzip "$SCANNER_HOME/sonar-scanner.zip" -d "$SCANNER_HOME" > "$SCANNER_HOME/install.log"
# set scanner to be executable
SCANNER_EXECUTABLE_DIR="$(ls -1 $SCANNER_HOME | grep sonar-scanner-)"
SCANNER_EXECUTABLE_DIR="$SCANNER_HOME/$SCANNER_EXECUTABLE_DIR/bin"
chmod +x "$SCANNER_EXECUTABLE_DIR/sonar-scanner"
export PATH="$SCANNER_HOME:$SCANNER_EXECUTABLE_DIR:$PATH"