#!/bin/bash
set -e

exec supervisord -c /etc/supervisor/conf.d/supervisord.conf
