# Sistema de Ocorrências Bancárias

Sistema desenvolvido com foco na centralização, padronização e consulta de ocorrências bancárias. O projeto foi construído utilizando **Angular** e **TypeScript** no frontend e **ASP.NET Core** com **C#** no backend, integrados a um banco de dados **PostgreSQL** containerizado com **Docker**. 

A aplicação permite o cadastro, edição e validação de ocorrências e seus respectivos motivos, organizados por instituição bancária, simulando um cenário real de uso em ambientes financeiros e operacionais.

## 🖼️ Preview do Sistema

![Portal de Ocorrências Bancárias](https://github.com/gildevson/BancoOcorrenciasAngular/blob/main/src/assets/logo/IMAGEMBANCOOCORRENCIAS.png?raw=true)

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
- 🔐 Reset de senha
- 🔐 Gerenciamento de permissões

### Portal de Notícias
- 📰 Sistema de notícias dinâmico
- 📰 Estrutura de tabelas no banco de dados
- 📰 Colunas configuráveis
- 📰 Conteúdo gerenciável (não texto estático)

### Visualização de Dados
- 📊 Gráficos e dashboards com Chart.js
- 📊 Análise de ocorrências por instituição
- 📊 Previsão do tempo integrada

## 🏗️ Arquitetura do Backend

O projeto segue uma arquitetura em camadas bem definida, promovendo separação de responsabilidades e manutenibilidade:
```
backend/
│
├── 📁 Controllers/                    # Camada de Apresentação (API)
│   ├── AuthController.cs             # Autenticação e autorização
│   ├── BancosController.cs           # Gestão de bancos
│   ├── CurrencyController.cs         # Conversão de moedas
│   ├── HealthController.cs           # Health check da aplicação
│   ├── NoticiasController.cs         # CRUD de notícias
│   ├── OcorrenciasMotivoController.cs # Motivos de ocorrências
│   ├── UsuariosController.cs         # Gestão de usuários
│   └── WeatherForecastController.cs  # Previsão do tempo
│
├── 📁 Data/                           # Camada de Acesso a Dados
│   ├── DbConnectionFactory.cs        # Factory para conexões com BD
│   └── 📁 DataTables/                # Scripts SQL das tabelas
│       ├── bancos.sql
│       ├── INSERTINTOusuarios.sql
│       ├── noticias.sql
│       ├── ocorrencias_motivos.sql
│       └── USUARIOS.sql
│
├── 📁 DTO/                            # Data Transfer Objects
│   ├── CreateOcorrenciaMotivRequest.cs
│   ├── CreateUsuarioRequest.cs
│   ├── ForgotPasswordRequest.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── LoginUserDto.cs
│   ├── RegisterRequest.cs
│   ├── ResetPasswordRequest.cs
│   └── UpdateOcorrenciaMotivRequest.cs
│
├── 📁 Models/                         # Modelos de Domínio
│   ├── Bancos.cs                     # Entidade Bancos
│   ├── Noticia.cs                    # Entidade Notícias
│   ├── OcorrenciaMotivo.cs           # Entidade Ocorrências
│   ├── Permissao.cs                  # Entidade Permissões
│   └── Usuario.cs                    # Entidade Usuários
│
├── 📁 Repositories/                   # Camada de Repositórios
│   ├── BancosRepository.cs           # Repositório de Bancos
│   ├── NoticiaRepository.cs          # Repositório de Notícias
│   ├── OcorrenciasMotivosRepository.cs
│   ├── PermissaoRepository.cs        # Repositório de Permissões
│   ├── ResetSenhaRepository.cs       # Repositório Reset de Senha
│   └── UsuarioRepository.cs          # Repositório de Usuários
│
└── 📁 Services/                       # Camada de Serviços (Lógica de Negócio)
    ├── AuthService.cs                # Serviço de Autenticação
    ├── EmailService.cs               # Serviço de E-mail
    ├── PasswordResetService.cs       # Serviço de Reset de Senha
    └── UsuarioService.cs             # Serviço de Usuários
```

### 📋 Descrição das Camadas

#### **Controllers (Camada de Apresentação)**
Responsável por receber as requisições HTTP, validar dados de entrada e retornar respostas adequadas. Cada controller gerencia um domínio específico da aplicação.

#### **Services (Camada de Lógica de Negócio)**
Contém as regras de negócio da aplicação, orquestrando operações entre repositórios e aplicando validações complexas.

#### **Repositories (Camada de Acesso a Dados)**
Implementa o padrão Repository, abstraindo o acesso ao banco de dados e fornecendo métodos para operações CRUD.

#### **Models (Camada de Domínio)**
Define as entidades do sistema que representam as tabelas do banco de dados.

#### **DTO (Data Transfer Objects)**
Objetos utilizados para transferência de dados entre camadas, garantindo que apenas informações necessárias sejam expostas.

#### **Data (Infraestrutura)**
Gerencia conexões com o banco de dados através do padrão Factory e contém scripts SQL para criação das tabelas.

## 🗄️ Estrutura do Banco de Dados
```sql
-- Tabelas principais
├── bancos                    # Instituições bancárias
├── usuarios                  # Usuários do sistema
├── noticias                  # Portal de notícias
├── ocorrencias_motivos       # Ocorrências e motivos
└── permissoes                # Controle de acesso
```

## 🔗 Links do Projeto

- **Frontend (Angular)**: [https://github.com/gildevson/BancoOcorrenciasAngular](https://github.com/gildevson/BancoOcorrenciasAngular)
- **Portal de Notícias**: [https://bancosocorrencia.com](https://bancosocorrencia.com)

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
