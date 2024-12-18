package me.baileykocin.lilac;

import me.baileykocin.lilac.config.Config;

public class Lilac {
    public static void main(String[] args) {
        System.out.println("RUNNING!");
        Config config = new Config("config.ini");
        System.out.println(config.value("info", "number", int.class));
        System.out.println(config.value("info", "string", String.class));
        System.out.println(config.value("info", "extra", "string", String.class));
    }
}