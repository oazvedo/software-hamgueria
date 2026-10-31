# Tela do Hamgueria servida pelo Caddy, que também repassa /api para a API (ADR 0023). Contexto: frontend/.
FROM node:24-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY . .
# Production always talks to the real API on the same origin (spec 004, RN-06): no MSW, no base URL.
ENV VITE_ENABLE_MOCKS=false
RUN npm run build && rm -f dist/mockServiceWorker.js

FROM caddy:2-alpine
COPY Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist /srv
EXPOSE 80 443
