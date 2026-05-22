# SolidarityConnection.Donations

API de Donations do projeto Solidarity Connection. Este servico consome eventos de doacao, persiste o estado no SQL Server e publica o evento de doacao processada no Azure Service Bus.

## Visao geral

Nesta versao, a aplicacao funciona como um host com background service e health check. Ela nao expoe controllers de negocio no projeto atual; a validacao local principal e feita por `health`, logs e integracao com o Core.

Dependencias principais:

- SQL Server para persistencia e migracoes EF Core
- Azure Service Bus para consumo e publicacao de eventos
- OpenTelemetry e New Relic para observabilidade

## Estrutura do repositorio

- `src/SolidarityConnection.Donations.Api`: host da aplicacao, background service e health check
- `src/SolidarityConnection.Donations.Application`: regras de negocio e casos de uso
- `src/SolidarityConnection.Donations.Domain`: entidades e eventos de dominio
- `src/SolidarityConnection.Donations.Infrastructure`: EF Core, repositorios e Service Bus
- `src/SolidarityConnection.Donations.Shared`: contratos e utilitarios compartilhados
- `k8s/`: manifests Kubernetes

## Requisitos

- .NET SDK 8.x
- Docker Desktop
- SQL Server local ou em Docker para a validacao local
- Azure Service Bus acessivel para a validacao local
- `kubectl` e Azure CLI somente se for publicar no AKS

## Como subir localmente

### Opcao recomendada 

Ambos os servicos fazem bootstrap automatico do banco ao iniciar: se o banco ainda nao existir, ele e criado e as migracoes EF Core sao aplicadas antes da API ficar disponivel.

Se o objetivo for validar o fluxo fim a fim com o Core, suba o ambiente compartilhado a partir do repositório `fiap-solidarity-connection`, que já contém o compose de validação para os dois servicos.

Passo a passo:

1. Mantenha os dois repositórios lado a lado na mesma pasta:
   - `c:\Repos\HACKATON\fiap-solidarity-connection`
   - `c:\Repos\HACKATON\fiap-solidarity-connection-donations`
2. No repositório do Core, siga o guia `../fiap-solidarity-connection/others/docs/validacao-local-docker-compose.md`.
3. Crie o arquivo `.env` local com base no exemplo do README do Core.
4. Suba a stack na raiz do Core com Docker Compose passando o arquivo como parametro:

```bash
docker compose --env-file .env -f docker-compose.validation.yml up -d --build
```

5. Verifique os health checks:

```bash
curl http://localhost:8080/health
curl http://localhost:8081/health
```

6. Valide o fluxo de doacao pelo Core. O Donations sera iniciado automaticamente pelo compose.

### Execucao local somente da Donations API

Use esta opção se quiser subir apenas este servico para debug.

1. Restore das dependencias:

```bash
dotnet restore SolidarityConnection.Donations.sln
```

2. Suba um SQL Server local na porta 1433, caso ainda nao tenha um disponivel. Se voce ja tiver um banco local configurado, use ele no passo seguinte:

```bash
docker run -d --name solidarity-donations-sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=<SUA_SENHA_FORTE> -p 1433:1433 mcr.microsoft.com/mssql/server:2025-latest
```

3. Configure a conexao com o banco e com o Service Bus. O projeto carrega `appsettings.Development.json` no perfil de desenvolvimento e tambem aceita sobrescrita por variaveis de ambiente ou core-secret.

Exemplo com core-secret:

```bash
dotnet core-secret set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=donations-db;User Id=SA;Password=<SUA_SENHA_FORTE>;TrustServerCertificate=True;Encrypt=False;" --project src/SolidarityConnection.Donations.Api/SolidarityConnection.Donations.Api.csproj
dotnet core-secret set "ServiceBus:ConnectionString" "Endpoint=sb://<NAMESPACE>.servicebus.windows.net/;SharedAccessKeyName=<NOME_DA_CHAVE>;SharedAccessKey=<SUA_CHAVE>;" --project src/SolidarityConnection.Donations.Api/SolidarityConnection.Donations.Api.csproj
```

4. Execute a aplicacao:

```bash
dotnet run --project src/SolidarityConnection.Donations.Api/SolidarityConnection.Donations.Api.csproj
```

5. Confirme a subida no health check:

```bash
curl http://localhost:5060/health
```

Observacao: no perfil de desenvolvimento, a API expõe `http://localhost:5060` e `https://localhost:5050`.

## Validacao integrada com o Core

O fluxo esperado entre os dois repositorios e:

1. O Core publica `DonationRequestedEvent`.
2. A Donations consome o evento e processa a doacao.
3. A Donations publica `DonationProcessedEvent`.
4. O Core atualiza `total_raised_amount` da campanha.

Se voce estiver fazendo a correção manualmente, o caminho mais rapido e usar o compose do repositório Core, pois ele já sobe o SQL Server, o Core e esta API em conjunto.


## Docker

Build da imagem:

```bash
docker build -f src/SolidarityConnection.Donations.Api/Dockerfile -t solidarity-connection-donations:local .
```

Executar localmente:

```bash
docker run -p 8080:80 solidarity-connection-donations:local
```

## Kubernetes (AKS)

Os manifests estao em `k8s/`.

Aplicar:

```bash
kubectl apply -f k8s/donations-configmap.yaml
kubectl apply -f k8s/donations-service.yaml
kubectl apply -f k8s/donations-deployment.yaml
kubectl apply -f k8s/donations-hpa.yaml
```

Verificar rollout:

```bash
kubectl rollout status deployment/solidarity-connection-donations
```

## Configuracoes importantes

As variaveis mais relevantes sao:

- `ConnectionStrings__DefaultConnection`
- `ServiceBus__ConnectionString`
- `DONATION_TOPIC`
- `DONATION_SUBSCRIPTION`
- `DONATION_PROCESSED_TOPIC`

No ambiente local, a conexao com o banco costuma apontar para `localhost,1433` e o banco `donations-db`, mas voce pode usar qualquer SQL Server acessivel desde que ajuste a connection string.

## Pipelines

O pipeline principal esta em `.github/workflows/ci-cd.yml` e executa:

- Build da solucao .NET
- Build da imagem Docker no CI
- Publicacao da imagem no GitHub Container Registry

O gatilho principal e a branch `main`, entao cada push nela dispara o pipeline.

Se voce quiser manter o Azure DevOps como alternativa, o arquivo legado continua em `pipeline/azure-pipelines.yml`.


