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
NETWORK := opendddnet-sln-netcore31-myboundedcontext

IMAGE_NAME := myboundedcontext.azurecr.io/myboundedcontext-backend
IMAGE_TAG := 1.0.0-alpha.1

PROJECT_DIR := $(PWD)
SCRIPTS_DIR := $(PROJECT_DIR)/scripts
SRC_DIR := $(PROJECT_DIR)/src
VERTICAL_DIR := $(SRC_DIR)/MyBoundedContext/Vertical
TESTS_DIR := $(SRC_DIR)/MyBoundedContext/Tests

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
	ENV_FILE=env.test dotnet test $(SRC_DIR)

##########################################################################
# BUILD
##########################################################################
.PHONY: clean
clean: ##@Build	 Clean the solution
	find . $(SRC_DIR) -iname "bin" | xargs rm -rf
	find . $(SRC_DIR) -iname "obj" | xargs rm -rf

.PHONY: clean-and-restore
clean-and-restore: ##@Build	 Clean and restore the project
	make clean
	make restore

.PHONY: build
build: ##@Build	 Build the docker image
	docker build -t $(IMAGE_NAME):$(IMAGE_TAG) .

.PHONY: deep-rebuild
deep-rebuild: ##@Build	 clean, clear nuget caches, restore and build the project
	make clean
	make clear-nuget-caches
	make restore
	make build

.PHONY: restore
restore: ##@Build	 restore the solution
	cd $(SRC_DIR) && dotnet restore

.PHONY: clear-nuget-caches
clear-nuget-caches: ##@Build	 clean all nuget caches
	nuget locals all -clear

##########################################################################
# RELEASE
##########################################################################
.PHONY: push
push: ##@Build	 Push the docker image
	docker push $(IMAGE_NAME):$(IMAGE_TAG)

##########################################################################
# LOCAL NUGET REFERENCES
##########################################################################
.PHONY: use-projects
use-projects: ##@Build	 Switch to using local projects
	cd $(SRC_DIR) && \
	dnt switch-to-projects

.PHONY: use-nugets
use-nugets: ##@Build	 Switch to using nugets
	cd $(SRC_DIR) && \
	dnt switch-to-packages
