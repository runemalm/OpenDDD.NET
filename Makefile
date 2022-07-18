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
NETWORK := dddfordotnet
BUILD_VERSION := 0.9.0-alpha6

SRC_DIR := $(PWD)/src
DDD_DIR := $(SRC_DIR)/DDD
BUILD_DIR := $(DDD_DIR)/DDD/bin
TESTS_DIR := $(SRC_DIR)/DDD.Tests
TOOLS_DIR := $(SRC_DIR)/Tools
FEED_DIR := $(HOME)/Projects/LocalFeed
USER_NUGET_CONFIG_DIR=$(HOME)/.config/NuGet/NuGet.Config

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
	ENV_FILE=env.test dotnet test $(TESTS_DIR)

##########################################################################
# BUILD
##########################################################################
.PHONY: clean
clean: ##@Build	 clean the solution
	find . $(SRC_DIR) -iname "bin" | xargs rm -rf
	find . $(SRC_DIR) -iname "obj" | xargs rm -rf

.PHONY: build
build: ##@Build	 build the solution
	cd $(SRC_DIR) && \
	dotnet build

.PHONY: pack
pack: ##@Build	 Create the nuget in local feed
	make build
	cd $(SRC_DIR) && \
	dotnet pack -c Release -o $(FEED_DIR) -p:PackageVersion=$(BUILD_VERSION)

##########################################################################
# TOOLS
##########################################################################

.PHONY: build-tools-image
build-tools-image: ##@Tools	Build the tools image
	cd $(TOOLS_DIR) && docker build --progress plain -t runemalm/dddfordotnet-tools .

.PHONY: push-tools-image
push-tools-image: ##@Tools	Push the tools image
	echo "TODO..."
