package hr.fer.tel.rassus.stupidudp.node;

import hr.fer.tel.rassus.stupidudp.config.Config;
import hr.fer.tel.rassus.stupidudp.network.EmulatedSystemClock;
import hr.fer.tel.rassus.stupidudp.network.SimpleSimulatedDatagramSocket;

import java.io.*;
import java.net.ServerSocket;
import java.util.HashSet;
import java.util.LinkedList;


/**
 * Created by aelek on 07/12/2016.
 */
public class Node {
    private LinkedList<String> measurements;

    private HashSet<Integer> peers;

    private LinkedList<String> configuration;

    private long startTime;

    private int port;

    public int getPort() {
        return port;
    }

    public Node() throws Exception {
        EmulatedSystemClock clock = new EmulatedSystemClock();
        startTime = System.currentTimeMillis();

        // setup config
        port = loadConfig();

        // fetch measurements from file
        measurements = new LinkedList<>();
        if (!mockMeasurements()){
            throw new UnsupportedOperationException("Unable to read measurements file.");
        }
    }

    private int loadConfig() throws Exception {
        this.peers = new HashSet<>();
        configuration = readLinesFromFile("./config.txt");
        for (String line : configuration) {
            try {
                peers.add(Integer.parseInt(line));
            } catch (NumberFormatException e) {
                // ignore this line
            }
        }

        for (int p : peers) {
            if (Config.isLocalPortFree(p)) {
                peers.remove(p);
                return p;
            }
        }
        throw new Exception("Invalid configuration, no free ports left.");
    }

    private boolean mockMeasurements(){
        measurements = new LinkedList<>();
        try {
            LinkedList<String> lines = readLinesFromFile("./measurements.csv");
            for (String line:lines) {
                String[] entries = line.split(",");
                measurements.add(entries[3]);
            }
        } catch (IOException e) {
            System.err.println(e.getMessage());
            return false;
        }
        return true;
    }

    private LinkedList<String> readLinesFromFile(String file) throws IOException {
        LinkedList<String> lines = new LinkedList<>();

        // open the file
        FileInputStream fileStream = new FileInputStream(file);

        // read the file line by line
        BufferedReader br = new BufferedReader(new InputStreamReader(fileStream));
        String line;
        while ((line = br.readLine()) != null) {
            lines.add(line);
        }
        // Close the input stream
        br.close();

        return lines;
    }

    public static boolean isLocalPortFree(int port) {
        try {
            new SimpleSimulatedDatagramSocket(port, 0.2, 1000).close();
            return true;
        } catch (IOException e) {
            return false;
        }
    }

    public void Do() throws Exception {
        // create a UDP server socket and bind it to the specified port on the localhost
        try(SimpleSimulatedDatagramSocket server = new SimpleSimulatedDatagramSocket(port, 0.2, 1000)) {
            System.out.println("UDP server at port " + port);

            // new thread that sends measurements
            Runnable clientWork = () -> {
                try {
                    Thread.sleep(3000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                for (int peer : peers) {
                    System.out.println("UDP Client sending messages to peer " + peer
                            + " on thread " + Thread.currentThread().getName());
                }
            };

            // new thread that accepts peers' measurements
            Runnable serverWork = () -> {
                for (int i = 5; i > 0; i--) {
                    System.out.println("UDP Server waiting for peers' messages on thread "
                            + Thread.currentThread().getName());
                    try {
                        Thread.sleep(3000);
                        System.out.println("UDP Server received peers' messages on thread "
                                + Thread.currentThread().getName());
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            };

            Thread c = new Thread(clientWork);
            c.start();

            try {
                while (true){
                    for(int i = 0; i < 5; i++){
                        Thread s = new Thread(serverWork);
                        s.start();
                        Thread.sleep(1000);
                    }
                }
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            System.out.println("Working on thread " + Thread.currentThread().getName());
            // sort()
            // print()
        }
    }

    private String getMeasure(){
        int lineNum;
        do {
            long estimatedTime = System.currentTimeMillis() - startTime;
            long active = (estimatedTime) / 1000;
            lineNum = Math.toIntExact((active % 100) + 2);
        } while (lineNum < 2 || lineNum - 2 > measurements.size() - 1);
        return measurements.get(lineNum - 1);
    }
}
