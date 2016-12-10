package hr.fer.tel.rassus.stupidudp.config;

import java.util.Scanner;

/**
 * Created by aelek on 10/12/2016.
 */
public class Demo {
    public static void main(String[] args) {
        System.out.print("Number of peers: ");
        Scanner in = new Scanner(System.in);
        try {
            Config config = new Config(in.nextInt());
            config.print();
        } catch (Exception e) {
            System.err.println(e.getMessage());
        }
    }
}
