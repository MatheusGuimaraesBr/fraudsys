# FraudSys

Sistema de API REST para gestão de limites de transações PIX, desenvolvida em .NET 8 com AWS DynamoDB.

---

## Tecnologias

- .NET 8
- C#
- AWS DynamoDB
- Swagger/OpenAPI
- xUnit, Moq, FluentAssertions, Coverlet, ReportGenerator

---

## Arquitetura

O projeto segue os princípios de DDD, Clean Code e SOLID, organizado em camadas:

```
FraudSys/
├── FraudSys.API/            → Controllers e configuração da aplicação
├── FraudSys.Application/    → Serviços e regras de negócio
├── FraudSys.Domain/         → Entidades e interfaces
├── FraudSys.Infrastructure/ → Repositórios e acesso ao DynamoDB
└── FraudSys.Tests/          → Testes unitários, de integração e de API
```

---

## Pré-requisitos

- .NET 8 SDK
- Conta AWS com acesso ao DynamoDB
- Git

---

## Configuração

### 1. Clone o repositório

```bash
git clone https://github.com/MatheusGuimaraesBr/fraudsys.git
cd FraudSys
```

### 2. Configure as credenciais AWS

Edite `FraudSys.API/appsettings.json`:

```json
{
  "AWS": {
    "Region": "sua-regiao",
    "AccessKey": "sua-access-key",
    "SecretKey": "sua-secret-key"
  }
}
```

Repita o mesmo em `FraudSys.Tests/appsettings.json`.

### 3. Crie a tabela no DynamoDB

| Campo          | Valor               |
|----------------|---------------------|
| Nome da tabela | `gestor-de-limites` |
| Partition Key  | `cpf` (String)      |
| Sort Key       | `conta` (String)    |

### 4. Execute o projeto

```bash
cd FraudSys.API
dotnet run
```

Documentação disponível em: `http://localhost:5089/swagger`

---

## Endpoints

### Gestão de Limites

| Método   | Rota                              | Descrição                    |
|----------|-----------------------------------|------------------------------|
| `POST`   | `/api/LimiteConta`                | Cadastrar limite de uma conta |
| `GET`    | `/api/LimiteConta/{cpf}/{conta}`  | Buscar limite de uma conta   |
| `PUT`    | `/api/LimiteConta/{cpf}/{conta}`  | Atualizar limite de uma conta |
| `DELETE` | `/api/LimiteConta/{cpf}/{conta}`  | Remover registro de limite   |

### Transações PIX

| Método | Rota                | Descrição                    |
|--------|---------------------|------------------------------|
| `POST` | `/api/TransacaoPix` | Processar uma transação PIX  |

---

## Exemplos de Requisição

### Cadastrar limite

```json
POST /api/LimiteConta
{
  "cpf": "12345678900",
  "agencia": "0001",
  "conta": "123456",
  "limitePix": 1000.00
}
```

### Processar transação PIX

```json
POST /api/TransacaoPix
{
  "cpf": "12345678900",
  "conta": "123456",
  "valor": 300.00
}
```

### Resposta — transação aprovada

```json
{
  "aprovada": true,
  "mensagem": "Transação aprovada.",
  "limiteAtual": 700.00
}
```

### Resposta — transação negada

```json
{
  "aprovada": false,
  "mensagem": "Limite insuficiente para realizar a transação.",
  "limiteAtual": 1000.00
}
```

---

## Testes

34 testes organizados em 3 categorias:

| Categoria   | Quantidade | Descrição                                          |
|-------------|------------|----------------------------------------------------|
| Unitários   | 17         | Lógica de negócio isolada com mocks                |
| Integração  | 7          | Repositório conectando ao DynamoDB real            |
| API         | 10         | Endpoints HTTP testados de ponta a ponta           |

### Executar testes e gerar relatório

```bash
powershell -ExecutionPolicy Bypass -File rodar-testes.ps1
```

O relatório HTML será gerado em `./RelatorioCobertura/index.html` e aberto automaticamente no navegador.

### Cobertura

| Camada                                | Cobertura |
|---------------------------------------|-----------|
| FraudSys.Domain                       | 100%      |
| FraudSys.API.TransacaoPixController   | 100%      |
| FraudSys.Infrastructure               | 97.4%     |
| FraudSys.API                          | 90.4%     |
| **Total**                             | **94%**   |
