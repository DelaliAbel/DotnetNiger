#!/bin/bash

echo "Lancement de la surveillance Tailwind CSS..."

npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --watch

exit 0