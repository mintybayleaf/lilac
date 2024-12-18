SHELL := bash
.SHELLFLAGS := -eu -o pipefail -c
.DELETE_ON_ERROR:

BUILD ?= build
CLASSES := $(BUILD)/classes

COMPILE_FLAGS ?= -g
RUN_FLAGS ?= -Xmx512M
JAVAC ?= javac
JAVA ?= java

ENTRY_POINT := me.baileykocin.lilac.Lilac

CLASSPATH := "$(CLASSES):lib/ini4j.jar"
FILES := src/me/baileykocin/lilac/Lilac.java \
         src/me/baileykocin/lilac/config/Config.java

.PHONY: all
all: compile

.PHONY: run
run: compile
	$(JAVA) $(RUN_FLAGS) -classpath $(CLASSPATH) $(ENTRY_POINT)

.PHONY: compile
compile:
	mkdir -p $(CLASSES)
	$(JAVAC) $(COMPILE_FLAGS) -d $(CLASSES) -classpath $(CLASSPATH) $(FILES)

.PHONY: clean
clean:
	rm -rf $(BUILD)
