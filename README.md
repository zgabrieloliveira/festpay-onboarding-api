# Festpay Onboarding API - Teste Técnico (Transações)

Este repositório contém a implementação do teste técnico para a vaga de Engenheiro de Software na Festpay, englobando a criação do domínio e CRUD da entidade `Transaction`.

## 🛠 Tecnologias Utilizadas

A aplicação foi construída seguindo a arquitetura Vertical Slice e os princípios de Clean Architecture e DDD, utilizando as seguintes tecnologias:

* **.NET 9 / C# 13**
* **Minimal APIs** com **Carter** para roteamento
* **MediatR** para o padrão CQRS (Commands e Queries)
* **FluentValidation** para validação de entrada via Pipeline Behaviors
* **Entity Framework Core** com provedor **SQLite** para persistência
* **xUnit** para testes de unidade (Domínio e Aplicação)
* **EF Core In-Memory Database** para isolamento de testes de Application

## 🚀 Instruções para Rodar o Projeto

### Pré-requisitos
* .NET SDK 9.0 ou superior instalado.

### 1. Executando a API
A aplicação está configurada para gerar o banco de dados SQLite (`festpay.db`) e aplicar as configurações automaticamente durante a inicialização (via `context.Database.Migrate()`). 

No terminal, acesse a raiz do projeto e execute:
```bash
dotnet restore
dotnet build
cd Festpay.Onboarding.Api
dotnet run
```

A API estará disponível e o Swagger poderá ser acessado via navegador (a URL/porta será exibida no console).

### 2. Executando os Testes
A suíte de testes cobre as regras de domínio (exceções e validações do `Builder`) e a orquestração da camada de aplicação (Handlers).

Na raiz do projeto, execute:
```bash
dotnet test
```
