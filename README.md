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

## 🏗️ Sobre o Projeto

A solução foi construída seguindo princípios de **DDD**, **CQRS** e **Clean Architecture**, com foco na proteção das regras de negócio e na integridade dos dados.

### Principais Implementações

* **Domínio e Entidades:** Mantendo o padrão já estabelecido, a entidade `Account` foi estendida para suportar novos comportamentos (como `Deposit` e `Withdraw`). A nova entidade `Transaction` segue exatamente a mesma estrutura de *Builder* e validações já utilizadas nas contas.

* **Consistência e Movimentação Financeira:** As transações efetivam a movimentação de saldo entre as contas. O débito (Withdraw) na conta de origem e o crédito (Deposit) na conta de destino são processados dentro de uma única transação de banco de dados, garantindo atomicidade: ou a movimentação ocorre integralmente ou nada é persistido.

* **Validação de Saldo:** O sistema valida preventivamente a disponibilidade de saldo na conta de origem durante a criação da transação. Caso o saldo seja insuficiente, a operação é rejeitada e um erro de domínio é retornado, impedindo a geração de transações inválidas.

* **Estorno seguro:** O cancelamento de uma transação realiza a reversão financeira efetiva (devolvendo os valores). Caso a conta de destino não possua saldo suficiente para a devolução, o estorno é bloqueado, preservando a integridade financeira do sistema.

* **Suporte a Depósito:** Adicionado um endpoint (`PATCH`) para realizar depósitos, seguindo o padrão de implementação de *Commands* e *Handlers* já presente no projeto. Isso foi necessário para viabilizar o fluxo de saldo inicial nas transferências.


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
