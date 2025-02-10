##########################################################################
# This is the project's makefile.
#
# Simply run 'make' on the command line to list all available tasks.
##########################################################################

##########################################################################
# CONFIGURATION
##########################################################################

# Load env file
include env.make
export $(shell sed 's/=.*//' env.make)

##########################################################################
# VARIABLES
##########################################################################

HOME := $(shell echo ~)
PWD := $(shell pwd)
SRC := $(PWD)/src
TESTS := $(SRC)/src/Bookstore/Tests
MIGRATIONS := $(SRC)/Bookstore/Infrastructure/Persistence/EfCore/Migrations
NETWORK := openddd-sample-bookstore

RABBITMQ_PORT := 5672

ZOOKEEPER_CONTAINER := bookstore-zookeeper

KAFKA_NETWORK := $(NETWORK)
KAFKA_CONTAINER := bookstore-kafka
KAFKA_BROKER := localhost:9092
KAFKA_ZOOKEEPER := localhost:2181

POSTGRES_CONTAINER := bookstore-postgres
POSTGRES_PORT := 5432
POSTGRES_DB := bookstore

##########################################################################
# CUSTOM MENU
##########################################################################

BLUE      := $(shell tput -Txterm setaf 4)
GREEN     := $(shell tput -Txterm setaf 2)
TURQUOISE := $(shell tput -Txterm setaf 6)
WHITE     := $(shell tput -Txterm setaf 7)
YELLOW    := $(shell tput -Txterm setaf 3)
GREY      := $(shell tput -Txterm setaf 1)
RESET     := $(shell tput -Txterm sgr0)
SMUL      := $(shell tput smul)
RMUL      := $(shell tput rmul)

# Add the following 'help' target to your Makefile
# And add help text after each target name starting with '\#\#'
# A category can be added with @category
HELP_FUN = \
	%help; \
	use Data::Dumper; \
	while(<>) { \
		if (/^([a-zA-Z\-_0-9]+)\s*:.*\#\#(?:@([a-zA-Z\-0-9\.\s]+))?\t(.*)$$/) { \
			$$c = $$2; $$t = $$1; $$d = $$3; \
			push @{$$help{$$c}}, [$$t, $$d, $$ARGV] unless grep { grep { grep /^$$t$$/, $$_->[0] } @{$$help{$$_}} } keys %help; \
		} \
	}; \
	for (sort keys %help) { \
		printf("${WHITE}%24s:${RESET}\n\n", $$_); \
		for (@{$$help{$$_}}) { \
			printf("%s%25s${RESET}%s  %s${RESET}\n", \
				( $$_->[2] eq "Makefile" || $$_->[0] eq "help" ? "${YELLOW}" : "${GREY}"), \
				$$_->[0], \
				( $$_->[2] eq "Makefile" || $$_->[0] eq "help" ? "${GREEN}" : "${GREY}"), \
				$$_->[1] \
			); \
		} \
		print "\n"; \
	}

# make
.DEFAULT_GOAL := help

# Variables
PWD = $(shell pwd)

# Variable wrapper
define defw
	custom_vars += $(1)
	$(1) ?= $(2)
	export $(1)
	shell_env += $(1)="$$($(1))"
endef

.PHONY: help
help:: ##@Other Show this help.
	@echo ""
	@printf "%30s " "${YELLOW}TARGETS"
	@echo "${RESET}"
	@echo ""
	@perl -e '$(HELP_FUN)' $(MAKEFILE_LIST)

##########################################################################
# TEST
##########################################################################

.PHONY: test
test: ##@Test	 run all unit tests
	dotnet test $(TESTS)

##########################################################################
# BUILD
##########################################################################

.PHONY: clean
clean: ##@Build	 clean the solution
	find . $(SRC) -iname "bin" | xargs rm -rf
	find . $(SRC) -iname "obj" | xargs rm -rf

.PHONY: build
build: ##@Build	 build the solution
	cd $(SRC) && dotnet build

.PHONY: deep-rebuild
deep-rebuild: ##@Build	 clean, clear nuget caches, restore and build the project
	make clean
	make clear-nuget-caches
	make restore
	make build

.PHONY: restore
restore: ##@Build	 restore the solution
	cd $(SRC) && dotnet restore

.PHONY: clear-nuget-caches
clear-nuget-caches: ##@Build	 clean all nuget caches
	nuget locals all -clear

##########################################################################
# MIGRATIONS
##########################################################################

.PHONY: create-migration
create-migration: ##@Migrations	 Create a new migration (uses NAME)
ifndef NAME
	$(error Migration name not specified. Usage: make create-migration NAME=<MigrationName>)
endif
	dotnet ef migrations add $(NAME) \
		--output-dir $(MIGRATIONS) \
		--project $(SRC)/Bookstore

.PHONY: apply-migrations
apply-migrations: ##@Migrations	 Apply all pending migrations to the database
	dotnet ef database update \
		--project $(SRC)/Bookstore

##########################################################################
# AZURE
##########################################################################

.PHONY: azure-create-servicebus-namespace
azure-create-servicebus-namespace: ##@Azure	 Create the Azure Service Bus namespace
	az servicebus namespace create --name $(AZURE_SERVICEBUS_NAMESPACE) --resource-group $(AZURE_RESOURCE_GROUP) --location $(AZURE_REGION) --sku Standard

.PHONY: azure-get-servicebus-connection
azure-get-servicebus-connection: ##@Azure	 Get the Service Bus connection string
	az servicebus namespace authorization-rule keys list \
	    --resource-group $(AZURE_RESOURCE_GROUP) \
	    --namespace-name $(AZURE_SERVICEBUS_NAMESPACE) \
	    --name RootManageSharedAccessKey \
	    --query primaryConnectionString \
	    --output tsv

##########################################################################
# RABBITMQ
##########################################################################

.PHONY: rabbitmq-start
rabbitmq-start: ##@@RabbitMQ	 Start a RabbitMQ container
	docker run -d --name rabbitmq --hostname rabbitmq \
		-e RABBITMQ_DEFAULT_USER=$(RABBITMQ_DEFAULT_USER) \
		-e RABBITMQ_DEFAULT_PASS=$(RABBITMQ_DEFAULT_PASS) \
		-p 5672:$(RABBITMQ_PORT) -p 15672:15672 rabbitmq:management
	@echo "RabbitMQ started. Management UI available at http://localhost:15672"

.PHONY: rabbitmq-stop
rabbitmq-stop: ##@RabbitMQ	 Stop the RabbitMQ container
	docker stop rabbitmq
	@echo "RabbitMQ stopped."

.PHONY: rabbitmq-status
rabbitmq-status: ##@RabbitMQ	 Check RabbitMQ container status
	docker ps | grep rabbitmq || echo "RabbitMQ is not running."

.PHONY: rabbitmq-get-connection
rabbitmq-get-connection: ##@RabbitMQ	 Get the RabbitMQ connection string
	@echo "amqp://$(RABBITMQ_DEFAULT_USER):$(RABBITMQ_DEFAULT_PASS)@localhost:$(RABBITMQ_PORT)/"

.PHONY: rabbitmq-clean
rabbitmq-clean: ##@RabbitMQ	 Remove RabbitMQ container and its volumes
	docker rm -f rabbitmq || true
	@echo "RabbitMQ container removed."

.PHONY: rabbitmq-logs
rabbitmq-logs: ##@RabbitMQ	 Show RabbitMQ logs
	docker logs -f rabbitmq

##########################################################################
# KAFKA
##########################################################################

.PHONY: kafka-start
kafka-start: ##@Kafka	 Start Kafka and Zookeeper using Docker
	@docker network inspect $(KAFKA_NETWORK) >/dev/null 2>&1 || docker network create $(KAFKA_NETWORK)
	@docker run -d --rm --name $(ZOOKEEPER_CONTAINER) --network $(KAFKA_NETWORK) -p 2181:2181 \
	    wurstmeister/zookeeper:latest
	@docker run -d --rm --name $(KAFKA_CONTAINER) --network $(KAFKA_NETWORK) -p 9092:9092 \
	    -e KAFKA_BROKER_ID=1 \
	    -e KAFKA_ZOOKEEPER_CONNECT=$(ZOOKEEPER_CONTAINER):2181 \
	    -e KAFKA_LISTENERS=PLAINTEXT://0.0.0.0:9092 \
	    -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
	    -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
	    wurstmeister/kafka:latest

.PHONY: kafka-stop
kafka-stop: ##@Kafka	 Stop Kafka and Zookeeper
	@docker stop $(KAFKA_CONTAINER) || true
	@docker stop $(ZOOKEEPER_CONTAINER) || true

.PHONY: kafka-logs
kafka-logs: ##@Kafka	 Show Kafka logs
	@docker logs -f $(KAFKA_CONTAINER)

.PHONY: kafka-shell
kafka-shell: ##@@Kafka	 Open a shell inside the Kafka container
	docker exec -it $(KAFKA_CONTAINER) /bin/sh

.PHONY: kafka-create-topic
kafka-create-topic: ##@Kafka	 Create a Kafka topic (uses NAME)
ifndef NAME
	$(error Topic name not specified. Usage: make kafka-create-topic NAME=<TopicName>)
endif
	@docker exec -it $(KAFKA_CONTAINER) kafka-topics.sh --create --topic $(NAME) --bootstrap-server $(KAFKA_BROKER) --replication-factor 1 --partitions 1

.PHONY: kafka-list-brokers
kafka-list-brokers: ##@Kafka	 List Kafka broker configurations
	docker exec -it $(KAFKA_CONTAINER) /opt/kafka/bin/kafka-configs.sh --bootstrap-server localhost:9092 --describe --entity-type brokers

.PHONY: kafka-list-topics
kafka-list-topics: ##@Kafka	 List all Kafka topics
	@docker exec -it $(KAFKA_CONTAINER) kafka-topics.sh --list --bootstrap-server $(KAFKA_BROKER)

.PHONY: kafka-broker-status
kafka-broker-status: ##@Kafka	 Show Kafka broker status
	docker exec -it bookstore-kafka /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server localhost:9092

.PHONY: kafka-list-consumers
kafka-list-consumers: ##@Kafka	 List active Kafka consumer groups
	docker exec -it bookstore-kafka /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server localhost:9092 --list

.PHONY: kafka-consume
kafka-consume: ##@Kafka	 Consume messages from a Kafka topic (uses NAME)
ifndef NAME
	$(error Topic name not specified. Usage: make kafka-consume NAME=<TopicName>)
endif
	@docker exec -it $(KAFKA_CONTAINER) kafka-console-consumer.sh --bootstrap-server $(KAFKA_BROKER) --topic $(NAME) --from-beginning

.PHONY: kafka-produce
kafka-produce: ##@Kafka	 Produce messages to a Kafka topic (uses NAME)
ifndef NAME
	$(error Topic name not specified. Usage: make kafka-produce NAME=<TopicName>)
endif
	@docker exec -it $(KAFKA_CONTAINER) kafka-console-producer.sh --broker-list $(KAFKA_BROKER) --topic $(NAME)

##########################################################################
# POSTGRESQL
##########################################################################

.PHONY: postgres-start
postgres-start: ##@PostgreSQL	 Start a PostgreSQL container
	@docker run -d --name $(POSTGRES_CONTAINER) --network $(NETWORK) \
	    -e POSTGRES_DB=$(POSTGRES_DB) \
	    -e POSTGRES_USER=$(POSTGRES_USER) \
	    -e POSTGRES_PASSWORD=$(POSTGRES_PASSWORD) \
	    -p $(POSTGRES_PORT):5432 postgres:latest
	@echo "PostgreSQL started on port $(POSTGRES_PORT)."

.PHONY: postgres-stop
postgres-stop: ##@PostgreSQL	 Stop the PostgreSQL container
	@docker stop $(POSTGRES_CONTAINER) || true
	@echo "PostgreSQL stopped."

.PHONY: postgres-clean
postgres-clean: ##@PostgreSQL	 Remove PostgreSQL container and its volumes
	@docker rm -f $(POSTGRES_CONTAINER) || true
	@echo "PostgreSQL container removed."

.PHONY: postgres-logs
postgres-logs: ##@PostgreSQL	 Show PostgreSQL logs
	@docker logs -f $(POSTGRES_CONTAINER)

.PHONY: postgres-shell
postgres-shell: ##@PostgreSQL	 Open a shell inside the PostgreSQL container
	docker exec -it $(POSTGRES_CONTAINER) psql -U $(POSTGRES_USER) -d $(POSTGRES_DB)

.PHONY: postgres-connection-strings
postgres-connection-strings: ##@PostgreSQL    Display the connection strings for PostgreSQL
	@echo "PostgreSQL Connection String (Key-Value/DSN): host=localhost port=$(POSTGRES_PORT) dbname=$(POSTGRES_DB) user=$(POSTGRES_USER) password=$(POSTGRES_PASSWORD)"
	@echo "PostgreSQL Connection String (URI): postgresql://$(POSTGRES_USER):$(POSTGRES_PASSWORD)@localhost:$(POSTGRES_PORT)/$(POSTGRES_DB)"
	@echo "PostgreSQL Connection String (JDBC): jdbc:postgresql://localhost:$(POSTGRES_PORT)/$(POSTGRES_DB)?user=$(POSTGRES_USER)&password=$(POSTGRES_PASSWORD)"
