package hr.fer.tel.rassus.stupidudp;

import hr.fer.tel.rassus.stupidudp.config.Config;
import hr.fer.tel.rassus.stupidudp.network.EmulatedSystemClock;
import hr.fer.tel.rassus.stupidudp.network.SimpleSimulatedDatagramSocket;

import java.io.*;
import java.util.HashSet;
import java.util.LinkedList;


/**
 * Created by aelek on 07/12/2016.
 */
public class Node {
    private LinkedList<String> measurements;
    private HashSet<Integer> peers;
    private long startTime;

    private int port;

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

    public int getPort() {
        return port;
    }

    private int loadConfig() throws Exception {
        this.peers = new HashSet<>();
        LinkedList<String> configuration = readLinesFromFile("./config.txt");
        for (String line : configuration) {
            try {
                peers.add(Integer.parseInt(line));
            } catch (NumberFormatException e) {
                // ignore
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

    private String measure(){
        int lineNum;
        do {
            long estimatedTime = System.currentTimeMillis() - startTime;
            long active = (estimatedTime) / 1000;
            lineNum = Math.toIntExact((active % 100) + 2);
        } while (lineNum < 2 || lineNum - 2 > measurements.size() - 1);
        return measurements.get(lineNum - 1);
    }

    public static void main(String[] args) {
        try {
            Node p = new Node();
            System.out.println(p.getPort());
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void Do() throws Exception {
        // create a UDP server socket and bind it to the specified port on the localhost
        try(SimpleSimulatedDatagramSocket server = new SimpleSimulatedDatagramSocket(port, 0.2, 1000)) {
            System.out.println("Opened port " + port);

            // new thread that sends measurements
            Runnable client = () -> {
                System.out.println("Client sending messages to peers on thread " + Thread.currentThread().getName());
                try {
                    Thread.sleep(3000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            };

            // new thread that accepts peers' measurements
            Runnable serverThread = () -> {
                for (int i = 5; i > 0; i--) {
                    System.out.println("Server waiting for peers' messages on thread " + Thread.currentThread().getName());
                    try {
                        Thread.sleep(3000);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            };

            Thread s = new Thread(serverThread);
            s.start();
            Thread c = new Thread(client);
            c.start();
            try {
                Thread.sleep(3000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            System.out.println("Working on thread " + Thread.currentThread().getName());
            // sort()
            // print()
        }
    }
}
