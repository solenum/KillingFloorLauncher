#!/bin/sh
# Polls steam for the killing floor server list with one api key and writes it where a webserver can
# serve it, so nobody running the launcher needs a key of their own.  The file is steams own response
# byte for byte, which is what the launcher already knows how to read.
set -eu

: "${STEAM_API_KEY:?set STEAM_API_KEY, see https://steamcommunity.com/dev/apikey}"
OUT="${OUT:-/var/www/html/kf-servers.json}"
APPID="${APPID:-1250}"

tmp="$(mktemp "${OUT}.XXXXXX")"
trap 'rm -f "$tmp"' EXIT

curl -fsS --max-time 20 -o "$tmp" \
  "https://api.steampowered.com/IGameServersService/GetServerList/v1/?key=${STEAM_API_KEY}&limit=5000&filter=%5Cappid%5C${APPID}"

# an error page or a truncated download must never replace a good list
grep -q '"servers"' "$tmp"

chmod 644 "$tmp"
mv "$tmp" "$OUT"
