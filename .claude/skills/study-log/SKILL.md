---
name: study-log
description: Registra no diário de estudos (docs/learning/) o que foi aprendido em uma task ou spec. Use ao concluir uma task ou spec, ou quando o usuário disser "registre o que aprendi", "study log", "anota isso pra estudar", ou pedir para explicar e guardar um conceito.
---

# Skill: study-log

## Passos
1. Identifique **de 1 a 3 conceitos** que apareceram no trabalho recente (o diff, a task ou a conversa).
   Prefira os conceitos não óbvios: um trade-off, um padrão, um erro que apareceu, uma API do framework.
2. Para cada conceito, crie ou atualize uma entrada em `docs/learning/AAAA-MM-DD-conceito-em-kebab-case.md`:
   ```markdown
   # <Conceito>
   - **Data:** AAAA-MM-DD · **Spec/Task:** NNN / Tn · **Tags:** dotnet, ef-core, rabbitmq, react, tanstack-query...

   ## O que é
   2 a 4 frases, em linguagem simples.

   ## Por que importa aqui
   Ligue o conceito a um ADR ou a uma decisão do projeto.

   ## Onde aparece no código
   - `caminho/arquivo.cs:linha`: o que olhar ali.

   ## Exemplo mínimo
   Trecho curto (até 15 linhas), real ou simplificado a partir do código.

   ## Dúvidas em aberto / para aprofundar
   - pergunta ou link oficial (Microsoft Learn, react.dev, tanstack.com, rabbitmq.com).
   ```
3. Adicione uma linha no índice de `docs/learning/README.md`: data, conceito e tags.
4. Termine com **uma pergunta de revisão** para o usuário responder sozinho
   (ex.: "Por que o `OrderService` publica o evento só depois de gravar o pedido?").

## Regras
- Escreva para o usuário daqui a 3 meses: direto e sem floreio.
- Não copie a documentação oficial. Explique com base no código deste repositório.
- Se o conceito já tem entrada, acrescente uma seção datada em vez de duplicar o arquivo.
