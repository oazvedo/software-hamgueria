# Tela do Hamgueria servida pelo Caddy, que também repassa /api para a API (ADR 0023). Contexto: frontend/.
FROM node:24-alpine@sha256:116b800c3754b440ee0aa3ed2f20f5bf3d99a554bfd9adeea09ea8017f6e85f5 AS build
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
