# LogiFleet - Sistema de Gestão de Frota

LogiFleet é um sistema de gestão de frota e logística, projetado para otimizar o processo de entregas, desde a criação de pedidos até a rota de entrega final.

## Tecnologias Utilizadas

*   **Back-end:** C# com ASP.NET Core 8
*   **Front-end:** HTML5, CSS3, JavaScript
*   **Banco de Dados:** SQL Server
*   **Geocodificação:** API Nominatim (OpenStreetMap)
*   **Informação sobre o tempo:** OpenWeather

## Como Executar o Projeto

### Pré-requisitos

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior
*   SQL Server (Express ou outra edição)
*   Node.js (para o front-end, opcionalmente, para usar um live server)

### 1. Configurando o Back-end

1.  **Abra o projeto `A2/A2.sln`** no Visual Studio 2022 ou superior.
2.  **Configure a string de conexão:**
    *   No arquivo `A2/appsettings.Development.json`, localize a seção `ConnectionStrings`.
    *   Altere o valor de `DefaultConnection` para apontar para a sua instância do SQL Server. Por exemplo:
        ```json
        "ConnectionStrings": {
          "DefaultConnection": "Server=SEU_SERVIDOR;Database=LogiFleetDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=false"
        }
        ```
3.  **Aplique as Migrations:**
    *   Abra o "Console do Gerenciador de Pacotes" (Package Manager Console).
    *   Execute o comando `Update-Database`. Isso criará o banco de dados e todas as tabelas necessárias, além de popular o banco com dados iniciais (seed data).
4.  **Execute o projeto:**
    *   Pressione F5 ou clique no botão de play para iniciar o servidor do back-end.
    *   A API estará disponível em `https://localhost:PORTA` (a porta pode variar).

### 2. Configurando o Front-end

1.  **Abra o arquivo `logiFleet-front/index.html`** em um navegador web.
2.  Para uma melhor experiência de desenvolvimento, é recomendado usar um servidor web local. Se você tiver o Node.js instalado, pode usar o `live-server`:
    ```bash
    # Instale o live-server globalmente (se ainda não tiver)
    npm install -g live-server

    # Navegue até a pasta do front-end
    cd logiFleet-front

    # Inicie o servidor
    live-server
    ```
    Isso abrirá o front-end em seu navegador e o recarregará automaticamente sempre que você salvar um arquivo.

## Logins de Acesso

O banco de dados é populado com usuários de teste com diferentes perfis. A senha para todos os usuários é `123456`.

*   **Administrador:**
    *   **Email:** `admin@logifleet.com`
    *   **Visão:** Acesso completo a todas as funcionalidades do sistema.

*   **Cliente:**
    *   **Email:** `roberto.cli@techstore.com`
    *   **Visão:** Acesso a uma página simplificada que mostra o status do seu pedido mais recente.
