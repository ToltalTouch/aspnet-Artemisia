[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/O-004prE)
[![Open in Codespaces](https://classroom.github.com/assets/launch-codespace-2972f46106e565e64193e422d61a12cf1da4916b45550586e14ef0a7c637dd04.svg)](https://classroom.github.com/open-in-codespaces?assignment_repo_id=20957648)


# Artemisia — Resumo rápido

Projeto ASP.NET Core MVC (backend + Razor Views) para gestão simples de produtos e categorias.

Senha de admin para inserir novos produtos, editar e excluir: 
**bigorna123**

Principais pontos
- Autenticação admin simples baseada em cookie (login por chave administrativa).
- Página de listagem de produtos com botão "Novo Produto" (vai para /Produto/Create).
- Página para editar ou excluir produtos com o admin autenticado.
- Scripts front-end em wwwroot/js (ex.: site.js controla dropdowns de categoria).
- Banco: SQL Server LocalDB (ConnectionStrings em appsettings.json).

Requisitos
- .NET 9 SDK (ou compatível com o target do projeto).

Fluxo de uso
- Login admin: http://localhost:5105/Account/Login — informe a AdminKey para receber cookie com role Admin.
- Após login, vá para http://localhost:5105/Produto e use o botão "Novo Produto" para abrir /Produto/Create.
- Logout via botão no header (aparece quando autenticado).

Arquivos relevantes
- Program.cs — configuração de autenticação/cookie.
- Controllers/AccountController.cs — login/logout/access denied.
- Controllers/ProdutoController.cs — CRUD de produtos e método IsAdmin().
- Models/Produto.cs — modelo de produto.
- Views/Account/* — views de login / access denied.
- Views/Produto/Index.cshtml — listagem com botão "Novo Produto".
- Views/Shared/_Layout.cshtml — links Login/Logout no header.
- wwwroot/js/site.js — script para dropdowns (comportamento hover/click).
