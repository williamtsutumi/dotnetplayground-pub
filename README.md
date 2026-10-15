# .NET Playground
 
Projeto pessoal para experimentar e estudar **ASP.NET Core e infraestrutura de aplicações .NET em Linux/AWS**.

O projeto roda em uma **AWS EC2** e atualmente explora:

* ASP.NET Core / Kestrel
* Nginx
* systemd
* Docker
* AWS EC2
 
## Arquitetura

Atualmente existem **três instâncias da aplicação** rodando na EC2:

```text
                         Internet
                            │
                            ▼
                        Nginx :80
                            │
             ┌──────────────┼──────────────┐
             ▼              ▼              ▼
          App A           App B          App C
          :5000           :5001           :5010
             │              │               │
          systemd        systemd          Docker
```

As duas primeiras instâncias são executadas diretamente no Linux e gerenciadas pelo `systemd`.

A terceira instância é executada dentro de um container Docker.
O Docker faz parte do playground para experimentar a **containerização de uma aplicação ASP.NET Core** e comparar essa abordagem com a execução via `systemd`.

O Nginx funciona como **reverse proxy** e permite experimentar **load balancing entre múltiplas instâncias da aplicação**.

## Portas

| Porta  | Componente   | Execução |
| ------ | ------------ | -------- |
| `80`   | Nginx        | Linux    |
| `5000` | ASP.NET Core | systemd  |
| `5001` | ASP.NET Core | systemd  |
| `5010` | ASP.NET Core | Docker   |

As aplicações nas portas `5000`, `5001` e `5010` são instâncias independentes do mesmo projeto.

## Tecnologias

* **C# / ASP.NET Core** — aplicação web
* **Nginx** — reverse proxy e load balancing
* **systemd** — gerenciamento das instâncias nas portas 5000 e 5001
* **Docker** — execução da instância na porta 5010
* **AWS EC2** — ambiente de hospedagem 

