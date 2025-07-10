# BancoDigitalAna
Código exemplo de APIs em C# no contexto financeiro, com dois objetivos principais:

1. **Demonstrar minhas habilidades de programação e organização de projetos**
2. **Simular um pequeno banco digital**, oferecendo APIs para operações bancárias básicas

## Sobre o Projeto

O BancoDigitalAna é uma API desenvolvida em C# com .NET 8.0, utilizando SQLite como banco de dados. O sistema permite a criação e gerenciamento de contas bancárias, suportando operações essenciais como:

- Criação de novas contas
- Consulta de saldo
- Depósito
- Crédito
- Transferências entre contas da mesma instituição
- Desativação de contas
- Sistema de login para garantir a segurança das informações e transações

## Tecnologias Utilizadas

- **C# .NET 8.0**
- **SQLite** (banco de dados leve e fácil de configurar)

## Como Rodar Localmente

1. Clone este repositório:
    ```bash
    git clone https://github.com/ThaisStefanini/BancoDigitalAna.git
    ```
2. Acesse a pasta do projeto:
    ```bash
    cd BancoDigitalAna
    ```
3. Execute o projeto como HTTPS (já está configurado para rodar assim).
4. Caso deseje executar de outra forma, atualize o valor de `UrlBase` em `appsettings.json`:
    ```json
    "ApiConfig": {
        "UrlBase": "https://localhost:7014"
    }
    ```
   Certifique-se que o valor corresponda ao definido em `launchSettings.json`.

## Status do Projeto

O projeto está pronto para uso, mas está aberto a melhorias. O próximo passo previsto é a inclusão de testes unitários automatizados.

## Melhorias Futuras

- [ ] Adicionar testes unitários automatizados
- [ ] Otimização de código, facilitando a navegação e manutenção do mesmo do escalar o projeto
- [ ] Adicionar tratamento de mensageria e métodos com execução assíncrona para novas APIs

## Autor

Thais Stefanini

---

Sinta-se à vontade para sugerir melhorias ou relatar problemas via Issues!
