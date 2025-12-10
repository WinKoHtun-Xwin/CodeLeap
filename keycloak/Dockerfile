# Keycloak Dockerfile with Realm Import
# Based on official Keycloak image with automatic realm configuration import

FROM quay.io/keycloak/keycloak:26.4.7

# Set working directory
WORKDIR /opt/keycloak

# Copy realm configuration for automatic import
COPY realm-export.json /opt/keycloak/data/import/realm-export.json

# The start-dev command with --import-realm will be specified in docker-compose
# This allows for flexibility in choosing between dev and production modes
