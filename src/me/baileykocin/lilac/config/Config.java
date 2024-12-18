package me.baileykocin.lilac.config;

import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.util.Objects;
import java.util.Optional;

import org.ini4j.Ini;

public class Config {
    private final static String DEFAULT_CONFIG = "config.ini";

    private File configFile;
    private Ini config;

    public Config() {
        this.configFile = Path.of(DEFAULT_CONFIG).toFile();
    }

    public Config(String configName) {
        Objects.requireNonNull(configName);
        this.configFile = Path.of(configName).toFile();
    }

    public Config(String configName, String path) {
        Objects.requireNonNull(configName);
        Objects.requireNonNull(path);
        this.configFile = Path.of(configName, path).toFile();
    }

    private Ini parse() {
        if (this.config == null) {
            try {
                this.config = new Ini(this.configFile);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        return this.config;
    }

    public <T> Optional<T> value(String section, String subsection, String entry, Class<T> type) {
        Objects.requireNonNull(section);
        Objects.requireNonNull(entry);
        Objects.requireNonNull(type);

        this.parse();
        String actualSection = subsection.isBlank() ? section : section.concat(".").concat(subsection);
        T value = this.config.get(actualSection, entry, type);
        return value != null ? Optional.of(value) : Optional.empty();
    }

    public <T> Optional<T> value(String section, String entry, Class<T> type) {
        return this.value(section, "", entry, type);
    }

}
