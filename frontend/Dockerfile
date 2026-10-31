# Tela do Hamgueria servida pelo Caddy, que também repassa /api para a API (ADR 0023). Contexto: frontend/.
FROM node:26-alpine@sha256:c6ad9e9c1d80f6eb607c26b4b0a132ff7a2344fad1c78ea3baa2a7d4c8de6d94 AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
# Production always talks to the real API on the same origin (spec 004, RN-06): no MSW, no base URL.
ENV VITE_ENABLE_MOCKS=false
RUN npm run build && rm -f dist/mockServiceWorker.js

FROM caddy:2-alpine@sha256:e70b164da81ef698b3ad19af799ea6086ce6d9e48ed4079eb9fe26f6fbf14ee5
COPY Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist /srv
EXPOSE 80 443
