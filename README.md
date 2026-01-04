# Sistema de Ocorrências Bancárias

Sistema desenvolvido com foco na centralização, padronização e consulta de ocorrências bancárias. O projeto foi construído utilizando **Angular** e **TypeScript** no frontend e **ASP.NET Core** com **C#** no backend, integrados a um banco de dados **PostgreSQL** containerizado com **Docker**. 

A aplicação permite o cadastro, edição e validação de ocorrências e seus respectivos motivos, organizados por instituição bancária, simulando um cenário real de uso em ambientes financeiros e operacionais.

## 🖼️ Preview do Sistema

![Portal de Ocorrências Bancárias]([./docs/images/portal-screenshot.png](https://github.com/gildevson/BancoOcorrenciasAngular/blob/main/src/assets/logo/IMAGEMBANCOOCORRENCIAS.png?raw=true))

## 🚀 Tecnologias Utilizadas

### Frontend
- **Angular** ^20.3.0
- **TypeScript**
- **RxJS** ~7.8.0
- **Chart.js** ^4.5.1 (Visualização de dados)
- **ng2-charts** ^8.0.0
- **Angular Router** (Navegação)
- **Angular Forms** (Validação de formulários)

### Backend
- **ASP.NET Core**
- **C#**
- **Micro-ORM** (Acesso performático ao banco de dados)
- **JWT** (Autenticação segura)

### Banco de Dados
- **PostgreSQL**
- **Docker** (Containerização)

### Portal de Notícias
- **Node.js**
- Estrutura de tabelas dinâmicas para notícias
- Sistema de colunas configuráveis
- Conteúdo dinâmico (não estático)

## ✨ Funcionalidades

### Gestão de Ocorrências
- ✅ Cadastro de ocorrências bancárias
- ✅ Edição e atualização de registros
- ✅ Validação de dados
- ✅ Organização por instituição bancária
- ✅ Gerenciamento de motivos de ocorrência

### Autenticação e Segurança
- 🔐 Validação de usuários
- 🔐 Autenticação via JWT
- 🔐 Controle de acesso

### Portal de Notícias
- 📰 Sistema de notícias dinâmico
- 📰 Estrutura de tabelas no banco de dados
- 📰 Colunas configuráveis
- 📰 Conteúdo gerenciável (não texto estático)

### Visualização de Dados
- 📊 Gráficos e dashboards com Chart.js
- 📊 Análise de ocorrências por instituição

## 🏗️ Arquitetura

O projeto foi desenvolvido seguindo boas práticas de arquitetura e padrões de desenvolvimento:

- **Separação de responsabilidades** (Frontend/Backend)
- **Arquitetura em camadas** no backend
- **Componentes reutilizáveis** no Angular
- **Containerização** com Docker
- **Micro-ORM** para performance otimizada
- **API RESTful** para comunicação

## 🔗 Links do Projeto

- **Frontend (Angular)**: [https://github.com/seu-usuario/ocorrencias-bancarias-frontend](https://github.com/seu-usuario/ocorrencias-bancarias-frontend)
- **Backend (.NET)**: [https://github.com/seu-usuario/ocorrencias-bancarias-backend](https://github.com/seu-usuario/ocorrencias-bancarias-backend)
- **Portal de Notícias (Node.js)**: [https://github.com/seu-usuario/portal-noticias](https://github.com/seu-usuario/portal-noticias)

## 📦 Dependências do Frontend
```json
{
  "dependencies": {
    "@angular/common": "^20.3.0",
    "@angular/compiler": "^20.3.0",
    "@angular/core": "^20.3.0",
    "@angular/forms": "^20.3.0",
    "@angular/platform-browser": "^20.3.0",
    "@angular/router": "^20.3.0",
    "chart.js": "^4.5.1",
    "ng2-charts": "^8.0.0",
    "rxjs": "~7.8.0",
    "tslib": "^2.3.0",
    "zone.js": "~0.15.0"
  }
}
```

## 🐳 Docker

O projeto utiliza Docker para containerização do banco de dados PostgreSQL, facilitando o setup e garantindo consistência entre ambientes de desenvolvimento e produção.
```bash
# Exemplo de comando para subir o container PostgreSQL
docker-compose up -d
```

## 🎯 Características Técnicas

- **Escalável**: Arquitetura preparada para crescimento
- **Performático**: Uso de micro-ORM para otimização de consultas
- **Seguro**: Autenticação JWT e validações robustas
- **Organizado**: Código estruturado e documentado
- **Moderno**: Tecnologias atualizadas e melhores práticas

## 📝 Modelo de Dados

O sistema possui estrutura de tabelas para:
- Ocorrências bancárias
- Instituições financeiras
- Motivos de ocorrência
- Usuários e autenticação
- Notícias (portal Node.js)
- Colunas dinâmicas de conteúdo

## 🚦 Como Executar

### Pré-requisitos
- Node.js
- .NET Core SDK
- Docker
- Angular CLI

### Executando o projeto
```bash
# Backend (.NET)
cd backend
dotnet restore
dotnet run

# Frontend (Angular)
cd frontend
npm install
ng serve

# Portal de Notícias (Node.js)
cd portal-noticias
npm install
npm start
```

## 👨‍💻 Autor

Desenvolvido com dedicação aplicando boas práticas de desenvolvimento, resultando em uma solução escalável, organizada e preparada para evolução contínua.

---

⭐ Se este projeto foi útil para você, considere dar uma estrela!
