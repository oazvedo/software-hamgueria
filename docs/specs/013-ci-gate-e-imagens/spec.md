# 013. O CI cobra o gate de interface e as imagens de produção: Spec

- **Status:** Concluída
- **Data:** 2026-10-31

## Problema
O gate de interface (spec 006) e os Dockerfiles (spec 004) só rodam quando alguém lembra. Um tique de copy, uma cor
crua ou um Dockerfile quebrado só aparecem depois da integração, ou pior, no deploy.

## Critérios de aceite
- **CA-01:** Dado um PR, quando o CI roda, então o job `frontend` roda o gate de interface (`scripts/ui/gate.sh`).
- **CA-02 (falha):** Dado um PR com travessão, emoji, seta em texto, cor crua ou `transition-all` em `frontend/src`,
  então o job `frontend` falha.
- **CA-03:** Dado um PR, quando o CI roda, então um job `images` constrói as imagens da API e da tela.
- **CA-04 (falha):** Dado um Dockerfile quebrado, então o job `images` falha.

## Fora de escopo
- Publicar imagens num registro (ECR) e deploy automático.
