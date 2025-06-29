# Contributing to OpenDDD.NET

Thank you for your interest in contributing to **OpenDDD.NET**!

This is an opinionated framework for building Domain-Driven Design (DDD) applications on ASP.NET Core. It exists to help developers focus on their domain model — while the framework handles persistence, messaging, and transactional consistency.

At the moment, this is a solo-developed project — but I would love to see more like-minded developers join and help shape its future.

---

## How to Contribute

- Open an issue to suggest ideas, report bugs, or start a discussion.
- Improve the documentation.
- Submit bug fixes or improvements.
- Propose new features aligned with the philosophy of the framework.

---


## Development Setup

This project uses a `Makefile` to simplify common development tasks.

### Install Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/) or later
- [Docker](https://www.docker.com/) (for integration tests and messaging infrastructure)
- [Make](https://www.gnu.org/software/make/) (pre-installed on macOS/Linux, available on Windows via WSL or GNU Make for Windows)

---

### Common Development Tasks

| Task                      | Command                      | Description                                    |
| ------------------------- | ---------------------------- | ---------------------------------------------- |
| Build the solution        | `make build`                 | Build all projects                             |
| Run all tests             | `make test`                  | Run unit and integration tests                 |
| Run only unit tests       | `make test-unit`             |                                                 |
| Run only integration tests| `make test-integration`      | Requires Docker containers for dependencies     |
| Clean build artifacts     | `make clean`                 | Remove `/bin` and `/obj` folders               |
| Restore dependencies      | `make restore`               |                                                 |
| Deep clean + rebuild      | `make deep-rebuild`          | Clean, clear NuGet caches, restore, and build  |

---

### Infrastructure with Docker

Some components require Docker for development, particularly integration testing against:

- PostgreSQL
- RabbitMQ
- Kafka (with Zookeeper)

| Task                   | Command                   | Description                           |
| ---------------------- | ------------------------- | ------------------------------------- |
| Start PostgreSQL       | `make postgres-start`     | Start PostgreSQL container            |
| Stop PostgreSQL        | `make postgres-stop`      |                                       |
| Start RabbitMQ         | `make rabbitmq-start`     | Start RabbitMQ with management UI     |
| Stop RabbitMQ          | `make rabbitmq-stop`      |                                       |
| Start Kafka + Zookeeper| `make kafka-start`        |                                       |
| Stop Kafka + Zookeeper | `make kafka-stop`         |                                       |

---

### 📄 Documentation

| Task                      | Command             | Description                          |
| ------------------------- | ------------------- | ------------------------------------ |
| Build docs (HTML)         | `make sphinx-html`  | Generate static HTML docs            |
| Auto-reload docs          | `make sphinx-autobuild` | Local server with live reload      |

---

### Templates

| Task                        | Command                | Description                      |
| --------------------------- | ---------------------- | -------------------------------- |
| Build project templates      | `make templates-pack`  |                                  |
| Install templates locally    | `make templates-install`|                                  |
| Uninstall templates          | `make templates-uninstall`|                                 |
| Rebuild and reinstall       | `make templates-rebuild`|                                  |

---

### Tip

Run `make` or `make help` to see all available commands.

## Let's Talk

If you have questions, ideas, or are unsure if something fits, feel free to open an issue. I’m happy to discuss.

## License

By contributing, you agree that your contributions will be licensed under the same license as OpenDDD.NET, which is GPLv3.

Thank you for helping improve OpenDDD.NET!
