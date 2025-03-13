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
NETWORK := openddd-net
BUILD_VERSION := 3.0.0-beta.2

NUGET_NAME := OpenDDD.NET
ROOT_NAMESPACE := OpenDDD

SRC_DIR := $(PWD)/src
TESTS_DIR := $(SRC_DIR)/OpenDDD.Tests
DOCS_DIR := $(PWD)/docs
SAMPLES_DIR := $(PWD)/samples
NAMESPACE_DIR := $(SRC_DIR)/$(ROOT_NAMESPACE)
BUILD_DIR := $(NAMESPACE_DIR)/$(ROOT_NAMESPACE)/bin
FEED_DIR := $(HOME)/Projects/LocalFeed
USER_NUGET_CONFIG_DIR=$(HOME)/.config/NuGet/NuGet.Config
SPHINXDOC_IMG := openddd.net/sphinxdoc

NETWORK := opendddnet

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
test: ##@Test	 Run all tests (unit & integration)
	cd $(TESTS_DIR) && dotnet test --configuration Release

.PHONY: test-unit
test-unit: ##@Test	 Run only unit tests
	cd $(TESTS_DIR) && dotnet test --configuration Release --filter "Category=Unit"

.PHONY: test-integration
test-integration: ##@Test	 Run only integration tests
	cd $(TESTS_DIR) && dotnet test --configuration Release --filter "Category=Integration"

.PHONY: test-ef-migrations-create-postgres
test-ef-migrations-create-postgres: ##@Test Create PostgreSQL migrations for PostgresTestDbContext
	cd $(TESTS_DIR) && \
	dotnet ef migrations add Postgres_InitialCreate \
	    --context PostgresTestDbContext \
	    --output-dir Integration/Infrastructure/Persistence/EfCore/Migrations/Postgres \
	    --project $(TESTS_DIR) \
	    -- --database-provider postgres

.PHONY: test-ef-migrations-create-sqlite
test-ef-migrations-create-sqlite: ##@Test Create SQLite migrations for SqliteTestDbContext
	cd $(TESTS_DIR) && \
	dotnet ef migrations add Sqlite_InitialCreate \
	    --context SqliteTestDbContext \
	    --output-dir Integration/Infrastructure/Persistence/EfCore/Migrations/Sqlite \
	    --project $(TESTS_DIR) \
	    -- --database-provider sqlite

##########################################################################
# BUILD
##########################################################################

.PHONY: clean
clean: ##@Build	 clean the solution
	find . $(SRC_DIR) -iname "bin" | xargs rm -rf
	find . $(SRC_DIR) -iname "obj" | xargs rm -rf

.PHONY: clear-nuget-caches
clear-nuget-caches: ##@Build	 clean all nuget caches
	nuget locals all -clear

.PHONY: restore
restore: ##@Build	 restore the solution
	cd src && dotnet restore

.PHONY: build
build: ##@Build	 build the solution
	cd $(SRC_DIR) && \
	dotnet build

.PHONY: deep-rebuild
deep-rebuild: ##@Build	 clean, clear nuget caches, restore and build the project
	make clean
	make clear-nuget-caches
	make restore
	make build

.PHONY: pack
pack: ##@Build	 Create the nuget in local feed
	make build
	cd $(SRC_DIR) && \
	dotnet pack -c Release -o $(FEED_DIR) -p:PackageVersion=$(BUILD_VERSION)

.PHONY: push
push: ##@Build	 Push the nuget to the global feed
	cd $(FEED_DIR) && \
	dotnet nuget push $(NUGET_NAME).$(BUILD_VERSION).nupkg --api-key $(NUGET_API_KEY) --source https://api.nuget.org/v3/index.json

##########################################################################
# DOCS
##########################################################################

DOCSAUTOBUILD_HOST_NAME := docsautobuild-openddd.net
DOCSAUTOBUILD_CONTAINER_NAME := docsautobuild-openddd.net
DOCSAUTOBUILD_PORT := 10001

.PHONY: sphinx-buildimage
sphinx-buildimage: ##@Docs	 Build the custom sphinxdoc image
	docker build -t $(SPHINXDOC_IMG) $(DOCS_DIR)

.PHONY: sphinx-html
sphinx-html: ##@Docs	 Build the sphinx html
	docker run -it --rm -v $(DOCS_DIR):/docs $(SPHINXDOC_IMG) make html

.PHONY: sphinx-clean
sphinx-clean: ##@Docs	 Clean the sphinx docs
	rm -rf $(DOCS_DIR)/_build

.PHONY: sphinx-rebuild
sphinx-rebuild: ##@Docs	 Re-build the sphinx docs
	make sphinx-clean && \
	make sphinx-html

.PHONY: sphinx-autobuild
sphinx-autobuild: ##@Docs	 Activate autobuild of docs
	docker run \
		-it \
		--rm \
		--name $(DOCSAUTOBUILD_CONTAINER_NAME) \
		--hostname $(DOCSAUTOBUILD_HOST_NAME) \
		-p "$(DOCSAUTOBUILD_PORT):8000" \
		-v $(DOCS_DIR):/docs \
		$(SPHINXDOC_IMG) \
		sphinx-autobuild /docs /docs/_build/html

.PHONY: sphinx-opendocs
sphinx-opendocs: ##@Docs	 Open the docs in browser
	open $(DOCS_DIR)/_build/html/index.html

##########################################################################
# TEMPLATES
##########################################################################

TEMPLATES_DIR := $(PWD)/templates
TEMPLATES_CSPROJ := $(TEMPLATES_DIR)/templatepack.csproj
TEMPLATES_OUT := $(TEMPLATES_DIR)/bin/templates
TEMPLATES_NAME := OpenDDD.NET-Templates
TEMPLATES_VERSION := 3.0.0-beta.2
TEMPLATES_NUPKG := $(TEMPLATES_OUT)/$(TEMPLATES_NAME).$(TEMPLATES_VERSION).nupkg

.PHONY: templates-install
templates-install: ##@Template	 Install the OpenDDD.NET project template locally
	dotnet new install $(TEMPLATES_NUPKG)

.PHONY: templates-uninstall
templates-uninstall: ##@Template	 Uninstall the OpenDDD.NET project template
	dotnet new uninstall $(TEMPLATES_NAME)

.PHONY: templates-pack
templates-pack: ##@Template	 Pack the OpenDDD.NET project template into a NuGet package
	dotnet pack $(TEMPLATES_CSPROJ) -o $(TEMPLATES_OUT)

.PHONY: templates-publish
templates-publish: ##@Template	 Publish the template to NuGet
	dotnet nuget push $(TEMPLATES_NUPKG) --api-key $(NUGET_API_KEY) --source https://api.nuget.org/v3/index.json

.PHONY: templates-rebuild
templates-rebuild: templates-uninstall templates-pack templates-install ##@Template	 Rebuild and reinstall the template

##########################################################################
# ACT
##########################################################################

ACT_IMAGE := ghcr.io/catthehacker/ubuntu:act-latest

.PHONY: act-install
act-install: ##@Act	 Install act CLI
	brew install act

.PHONY: act-clean
act-clean: ##@Act Stop and remove all act containers
	@docker stop $$(docker ps -q --filter ancestor=$(ACT_IMAGE)) 2>/dev/null || true
	@docker rm $$(docker ps -aq --filter ancestor=$(ACT_IMAGE)) 2>/dev/null || true
	@echo "✅ All act containers stopped and removed."

.PHONY: act-list
act-list: ##@Act	 List available workflows
	act -l

.PHONY: act-test
act-test: ##@Act	 Run all tests locally using act
	act -P ubuntu-latest=$(ACT_IMAGE) --reuse

.PHONY: act-test-dotnet
act-test-dotnet: ##@Act	 Run tests for a specific .NET version (usage: make act-test-dotnet DOTNET_VERSION=8.0.x)
	@if [ -z "$(DOTNET_VERSION)" ]; then \
		echo "Error: Specify .NET version using DOTNET_VERSION=<version>"; \
		exit 1; \
	fi
	act -P ubuntu-latest=$(ACT_IMAGE) -s matrix.dotnet-version=$(DOTNET_VERSION) --reuse

.PHONY: act-unit-tests
act-unit-tests: ##@Act	 Run only unit tests
	act -P ubuntu-latest=$(ACT_IMAGE) -j unit-tests --reuse

.PHONY: act-integration-tests
act-integration-tests: ##@Act	 Run only integration tests
	act -P ubuntu-latest=$(ACT_IMAGE) -j integration-tests --reuse -s AZURE_SERVICE_BUS_CONNECTION_STRING=$(AZURE_SERVICE_BUS_CONNECTION_STRING)

.PHONY: act-debug
act-debug: ##@Act	 Run act with verbose logging
	act -P ubuntu-latest=$(ACT_IMAGE) --verbose --reuse

##########################################################################
# AZURE
##########################################################################

.PHONY: azure-create-resource-group
azure-create-resource-group: ##@Azure	 Create the Azure Resource Group
	az group create --name $(AZURE_RESOURCE_GROUP) --location $(AZURE_REGION)

.PHONY: azure-create-service-principal
azure-create-service-principal: ##@Azure	 Create an Azure Service Principal for GitHub Actions
	@echo "Creating Azure Service Principal..."
	az ad sp create-for-rbac \
		--name "github-actions-opendddnet" \
		--role "Contributor" \
		--scopes /subscriptions/$(AZURE_SUBSCRIPTION_ID)/resourceGroups/$(AZURE_RESOURCE_GROUP) \
		--sdk-auth
	@echo "✅ Copy the output above and add it as 'AZURE_CREDENTIALS' in GitHub Secrets."

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

.PHONY: azure-delete-servicebus-namespace
azure-delete-servicebus-namespace: ##@Azure	 Delete the Azure Service Bus namespace
	az servicebus namespace delete --resource-group $(AZURE_RESOURCE_GROUP) --name $(AZURE_SERVICEBUS_NAMESPACE)

.PHONY: azure-list-servicebus-namespaces
azure-list-servicebus-namespaces: ##@Azure	 List all Azure Service Bus namespaces in the resource group
	az servicebus namespace list --resource-group $(AZURE_RESOURCE_GROUP) --output table

.PHONY: azure-list-servicebus-topics
azure-list-servicebus-topics: ##@Azure	 List all topics in the Azure Service Bus namespace
	az servicebus topic list --resource-group $(AZURE_RESOURCE_GROUP) --namespace-name $(AZURE_SERVICEBUS_NAMESPACE) --output table

.PHONY: azure-list-servicebus-subscriptions
azure-list-servicebus-subscriptions: ##@Azure	 List all subscriptions for a given topic (usage: make azure-list-servicebus-subscriptions TOPIC_NAME=<topic>)
	@if [ -z "$(TOPIC_NAME)" ]; then \
		echo "Error: Specify the topic name using TOPIC_NAME=<topic>"; \
		exit 1; \
	fi
	az servicebus topic subscription list --resource-group $(AZURE_RESOURCE_GROUP) --namespace-name $(AZURE_SERVICEBUS_NAMESPACE) --topic-name $(TOPIC_NAME) --output table

.PHONY: azure-list-servicebus-queues
azure-list-servicebus-queues: ##@Azure	 List all queues in the Azure Service Bus namespace
	az servicebus queue list --resource-group $(AZURE_RESOURCE_GROUP) --namespace-name $(AZURE_SERVICEBUS_NAMESPACE) --output table

.PHONY: azure-list-servicebus-authorization-rules
azure-list-servicebus-authorization-rules: ##@Azure	 List all authorization rules for the Azure Service Bus namespace
	az servicebus namespace authorization-rule list --resource-group $(AZURE_RESOURCE_GROUP) --namespace-name $(AZURE_SERVICEBUS_NAMESPACE) --output table

##########################################################################
# RABBITMQ
##########################################################################

RABBITMQ_PORT := 5672

.PHONY: rabbitmq-start
rabbitmq-start: ##@@RabbitMQ	 Start a RabbitMQ container
	docker run --rm -d --name rabbitmq --hostname rabbitmq \
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

.PHONY: rabbitmq-logs
rabbitmq-logs: ##@RabbitMQ	 Show RabbitMQ logs
	docker logs -f rabbitmq

##########################################################################
# KAFKA
##########################################################################

ZOOKEEPER_CONTAINER := opendddnet-zookeeper

KAFKA_NETWORK := $(NETWORK)
KAFKA_CONTAINER := opendddnet-kafka
KAFKA_BROKER := localhost:9092
KAFKA_ZOOKEEPER := localhost:2181

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
	@docker exec -it $(KAFKA_CONTAINER) /opt/kafka/bin/kafka-configs.sh --bootstrap-server localhost:9092 --describe --entity-type brokers

.PHONY: kafka-list-topics
kafka-list-topics: ##@Kafka	 List all Kafka topics
	@docker exec -it $(KAFKA_CONTAINER) kafka-topics.sh --list --bootstrap-server $(KAFKA_BROKER)

.PHONY: kafka-broker-status
kafka-broker-status: ##@Kafka	 Show Kafka broker status
	@docker exec -it $(KAFKA_CONTAINER) /opt/kafka/bin/kafka-broker-api-versions.sh --bootstrap-server localhost:9092

.PHONY: kafka-list-consumer-groups
kafka-list-consumer-groups: ##@Kafka	 List active Kafka consumer groups
	@docker exec -it $(KAFKA_CONTAINER) /opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server localhost:9092 --list

.PHONY: kafka-describe-consumer-groups
kafka-describe-consumer-groups: ##@Kafka	 List detailed info for all consumer groups
	@docker exec -it $(KAFKA_CONTAINER) kafka-consumer-groups.sh --bootstrap-server $(KAFKA_BROKER) --all-groups --describe

.PHONY: kafka-describe-consumer-group
kafka-describe-consumer-group: ##@Kafka	 Describe Kafka consumer group (requires GROUP=<group>)
ifndef GROUP
	$(error Consumer group not specified. Usage: make kafka-describe-consumer-group GROUP=<GroupName>)
endif
	@docker exec -it $(KAFKA_CONTAINER) kafka-consumer-groups.sh --bootstrap-server $(KAFKA_BROKER) --group $(GROUP) --describe

.PHONY: kafka-check-lag
kafka-check-lag: ##@Kafka	 Check Kafka consumer lag for a group (requires GROUP=<group>)
ifndef GROUP
	$(error Consumer group not specified. Usage: make kafka-check-lag GROUP=<GroupName>)
endif
	@docker exec -it $(KAFKA_CONTAINER) kafka-consumer-groups.sh --bootstrap-server $(KAFKA_BROKER) --group $(GROUP) --describe | grep -E 'TOPIC|LAG'

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
# POSTGRES
##########################################################################

POSTGRES_CONTAINER := opendddnet-testspostgres
POSTGRES_PORT := 5432
POSTGRES_DB := testdb

.PHONY: postgres-start
postgres-start: ##@Postgres	 Start a PostgreSQL container
	@docker run --rm -d --name $(POSTGRES_CONTAINER) --network $(NETWORK) \
	    -e POSTGRES_DB=$(POSTGRES_DB) \
	    -e POSTGRES_USER=$(POSTGRES_USER) \
	    -e POSTGRES_PASSWORD=$(POSTGRES_PASSWORD) \
	    -p $(POSTGRES_PORT):5432 postgres:latest
	@echo "PostgreSQL started on port $(POSTGRES_PORT)."

.PHONY: postgres-stop
postgres-stop: ##@Postgres	 Stop the PostgreSQL container
	@docker stop $(POSTGRES_CONTAINER) || true
	@echo "PostgreSQL stopped."

.PHONY: postgres-clean
postgres-clean: ##@Postgres	 Remove PostgreSQL container and its volumes
	@docker rm -f $(POSTGRES_CONTAINER) || true
	@echo "PostgreSQL container removed."

.PHONY: postgres-logs
postgres-logs: ##@Postgres	 Show PostgreSQL logs
	@docker logs -f $(POSTGRES_CONTAINER)

.PHONY: postgres-shell
postgres-shell: ##@Postgres	 Open a shell inside the PostgreSQL container
	docker exec -it $(POSTGRES_CONTAINER) psql -U $(POSTGRES_USER) -d $(POSTGRES_DB)

.PHONY: postgres-connection-strings
postgres-connection-strings: ##@Postgres    Display the connection strings for PostgreSQL
	@echo "PostgreSQL Connection String (Key-Value/DSN): Host=localhost;Port=$(POSTGRES_PORT);Database=$(POSTGRES_DB);Username=$(POSTGRES_USER);Password=$(POSTGRES_PASSWORD)"
	@echo "PostgreSQL Connection String (URI): postgresql://$(POSTGRES_USER):$(POSTGRES_PASSWORD)@localhost:$(POSTGRES_PORT)/$(POSTGRES_DB)"
	@echo "PostgreSQL Connection String (JDBC): jdbc:postgresql://localhost:$(POSTGRES_PORT)/$(POSTGRES_DB)?user=$(POSTGRES_USER)&password=$(POSTGRES_PASSWORD)"
