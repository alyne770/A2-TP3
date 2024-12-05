# API - Livro Digital
Este sistema foi desenvolvido em ASP.NET CORE WEB API utilizando o .NET CORE 8.0, o objetivo da aplicação é gerenciar uma biblioteca com recursos para cadastro de livros, editoras, categorias e autores, além de permitir o controle de empréstimos de livros aos usuários. 

## Funcionalidades

- *Cadastro e edição de livros*: Permite adicionar, editar, excluir e visualizar livros.
- *Gerenciamento de editoras, categorias e autores*: É possível cadastrar, editar e listar editoras, categorias e autores associados aos livros.
- *Controle de empréstimos*: O sistema permite registrar o empréstimo e a devolução de livros.


## Estrutura do Projeto

A aplicação possui ao menos seis entidades principais, com relações do tipo 1:N e N:N, garantindo um modelo relacional robusto. As entidades principais incluem:

1. *Livro*: representa os livros disponíveis na biblioteca.
2. *Editora*: relaciona-se com o livro (1:N) para definir a editora de cada livro.
3. *Categoria*: relação N:N com o livro, permitindo que cada livro pertença a múltiplas categorias.
4. *Autor*: permite atribuir autores aos livros.
5. *Usuário*: representa os usuários cadastrados na biblioteca para efetuar empréstimos.
6. *Empréstimo*: permite registrar o histórico de empréstimos e devoluções de livros pelos usuários.

## Tecnologias Utilizadas

- *ASP.NET WEB API* com .NET CORE 8.0
- *Entity Framework (Code First)* para mapeamento objeto-relacional e geração do banco de dados
- *SQL Server* como banco de dados

## Configuração e Execução

### Pré-requisitos

- Visual Studio 2022 (Community ou superior)
- SQL Server ou SQL Server Express
- SDK .NET CORE 8.0

### Passos para Configuração

1. *Clone o repositório*:
   ```bash
   git clone https://github.com/alyne770/A2-API.git

2. **Abra o Projeto**

   Abra o projeto no Visual Studio.

3. **Configure o Banco de Dados**

   - No arquivo `appsettings.json`  edite a string de conexão conforme o seu banco de dados.

   ```json
   {
     "ConnectionStrings": {
       "AppDbContext": "Server=SEU_SERVIDOR;Initial Catalog=LivrosDb;Integrated Security=True;TrustServerCertificate=True"
     }
   }

4. **Crie o Banco de Dados**

   Execute a migration para criar o banco de dados, Então abra o Gerenciador de Pacotes da aplicação e execute o seguinte comando:

    ```bash
     Update-Database
    
