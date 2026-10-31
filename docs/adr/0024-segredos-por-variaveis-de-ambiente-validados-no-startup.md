# 0024. Segredos por variáveis de ambiente, validados na inicialização

- **Status:** Aceito
- **Data:** 2026-10-31

## Contexto
O `appsettings.json` versiona a connection string com senha do Postgres, a senha do RabbitMQ, o `Auth:JwtSecret` e a
senha do admin (`admin123`). O `AuthService` ainda tem `?? "admin123"` como fallback: sem configuração, a cozinha abre
com a senha de desenvolvimento. Para publicar (ADR 0023), segredos não podem estar no repositório, e um esquecimento
na configuração não pode resultar numa cozinha aberta. Complementa o ADR 0009 sem substituí-lo: a autenticação continua
a mesma, muda só de onde vêm as credenciais.

## Decisão
- **`appsettings.json` não tem segredo.** Fica só o que não é sensível (`Auth:Issuer`, `Auth:Audience`,
  `Auth:AdminUsername`, `RabbitMq:Host`/`Port`, logging).
- **Desenvolvimento:** os valores locais (Postgres na 5433, RabbitMQ na 5673, `admin`/`admin123`, chave de dev) ficam
  no `appsettings.Development.json`. Não são segredos: valem só na máquina do dev e nos testes.
- **Produção:** os segredos chegam por variável de ambiente no formato do ASP.NET (`Auth__AdminPassword`,
  `ConnectionStrings__Postgres`...), montadas pelo `docker-compose.prod.yml` a partir do `.env` da raiz. O `.env` é
  ignorado pelo git. O `.env.example` versionado lista cada variável, sem valor real, com o comando para gerar.
- **Sem fallback de segredo no código.** Nada de `?? "admin123"`. Ler um segredo ausente é erro, não padrão.
- **Validação na inicialização:** fora do ambiente `Development`, a API chama `RequiredSettings.Check(configuration)`
  (Application) **antes** de `builder.Build()`. Se houver problema, lança `InvalidOperationException` com a lista dos
  **nomes** das chaves e das regras violadas, nunca dos valores, e a API não sobe.

```csharp
// Program.cs
if (!builder.Environment.IsDevelopment())
{
    var problems = RequiredSettings.Check(builder.Configuration);
    if (problems.Count > 0)
        throw new InvalidOperationException("Configuração de produção inválida: " + string.Join("; ", problems));
}
```

Regras do `RequiredSettings` (spec 004, RN-02 e RN-03): obrigatórios `ConnectionStrings:Postgres`,
`Auth:JwtSecret` (≥ 32 caracteres), `Auth:AdminUsername`, `Auth:AdminPassword` (≥ 12 caracteres e diferente de
`admin123`), `RabbitMq:Username` e `RabbitMq:Password` (diferentes de `guest`).

## Alternativas consideradas
- **AWS Secrets Manager / SSM Parameter Store:** melhor para rotação e auditoria, mas adiciona IAM, SDK e custo para
  uma instância só. Pode substituir o `.env` numa spec futura sem mudar o código, já que o ASP.NET lê o que estiver no
  ambiente.
- **`dotnet user-secrets` no desenvolvimento:** tira até os valores de dev do repositório, mas cada dev teria um passo
  manual a mais, e os testes e o CI dependem de `admin123` hoje.
- **Validar com `IOptions` + `ValidateOnStart`:** idiomático, mas valida só depois do `Build()`, quando o
  `Migrate()` do startup já tentou conectar. A checagem explícita antes do `Build()` falha primeiro e com mensagem clara.

## Consequências
- ✅ Esquecer um segredo ou usar a senha de dev em produção derruba a inicialização em vez de abrir a cozinha.
- ✅ O repositório pode ser público sem expor nada do ambiente publicado.
- ⚠️ Os valores de dev continuam versionados no `appsettings.Development.json`. Rodar a API com
  `ASPNETCORE_ENVIRONMENT=Development` num servidor público reabre a cozinha com `admin123`. O compose de produção fixa
  `Production`.
- ⚠️ O `.env` no servidor é texto puro: proteja com `chmod 600` e não copie para fora da instância.
