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
BUILD_VERSION := 3.0.0-beta.1

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
TEMPLATES_VERSION := 3.0.0-alpha.1
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

# ACT_IMAGE := ghcr.io/catthehacker/ubuntu:full-latest
# ACT_IMAGE := ghcr.io/catthehacker/ubuntu:runner-latest
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
azure-delete-servicebus: ##@Azure	 Delete the Azure Service Bus namespace
	az servicebus namespace delete --resource-group $(AZURE_RESOURCE_GROUP) --name $(AZURE_SERVICE_BUS_NAMESPACE)
