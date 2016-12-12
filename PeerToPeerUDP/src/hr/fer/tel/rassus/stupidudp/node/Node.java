package hr.fer.tel.rassus.stupidudp.node;

import hr.fer.tel.rassus.stupidudp.config.Config;
import hr.fer.tel.rassus.stupidudp.network.EmulatedSystemClock;
import hr.fer.tel.rassus.stupidudp.network.SimpleSimulatedDatagramSocket;

import java.io.*;
import java.net.*;
import java.util.Dictionary;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.LinkedList;
import java.util.concurrent.ConcurrentHashMap;


/**
 * Created by aelek on 07/12/2016.
 */
public class Node {
    private LinkedList<String> measurements;

    private HashSet<Integer> peers;

    private LinkedList<String> configuration;

    private ConcurrentHashMap<String,String> sharedDictMsg;

    private ConcurrentHashMap<String,Boolean> sharedDictAck;

    private long startTime;

    private int port;

    private EmulatedSystemClock clock;

    public int getPort() {
        return port;
    }

    public Node() throws Exception {
        clock = new EmulatedSystemClock();
        startTime = System.currentTimeMillis();

        // setup config
        port = loadConfig();

        // fetch measurements from file
        measurements = new LinkedList<>();
        if (!mockMeasurements()){
            throw new UnsupportedOperationException("Unable to read measurements file.");
        }

        sharedDictMsg = new ConcurrentHashMap<>();
        sharedDictAck = new ConcurrentHashMap<>();

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

    public void startClient(){
        // new thread that sends measurements
        Thread c = new Thread(new Client());
        c.start();
    }

    public void startServer() throws Exception {
        // new thread that accepts peers' measurements
        Thread s = new Thread(new Server());
        s.start();
    }

    public class Server implements Runnable{

        @Override
        public void run() {
            byte[] receiveBuffer = new byte[256];
            byte[] sendBuffer = new byte[256];
            //String msg;

            try(SimpleSimulatedDatagramSocket socket = new SimpleSimulatedDatagramSocket(port, 0.2, 1000)) {
                System.out.println("UDP server at port " + port);
                System.out.println("UDP Server waiting for peers' messages on thread " + Thread.currentThread().getName());

                while (true)
                {
                    socket.setSoTimeout(0);
                    //int clientPort = socket.getLocalPort();
                    DatagramPacket receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);

                    // primi poruku od peer-a
                    socket.receive(receivePacket);
                    final String msg = new String(receivePacket.getData(), receivePacket.getOffset(), receivePacket.getLength());

                    //System.out.println("UDP Server received " + msg);
                    String msgBody = getBody_FromMsg(msg);
                    int servFrom = getServerFrom_FromMsg(msg);
                    int clientFrom = getClientFrom_FromMsg(msg);
                    long scalarTimeStamp = getScalarTimeStampFromMsg(msg);
                    long nowScalarTimeStamp = clock.currentTimeMillis();

                    if (msgBody.equals("ACK")){
                        System.out.println("UDP Server received ACK from " + servFrom);

                        // sto je u ack-u servFrom je u obicnoj poruci servTo, to je port ovog servera
                        // pronadji poruku u shared_dict_ACK i postavi value na true

                        sharedDictAck.forEach((k, v) -> {
                            if(getServerFrom_FromMsg(k) == getServerTo_FromMsg(msg) && getClientFrom_FromMsg(k) == getClientTo_FromMsg(msg) && !v) {
                                sharedDictAck.put(k,true);
                                System.out.println("UDP Server updated ACK for message from " + getServerFrom_FromMsg(k));
                            }
                        });
                    }
                    else {
                        // body-ftom-ts
                        System.out.println("UDP Server received message " + msg + " from " + servFrom);

                        // spremi je u shared_dict_MSG to-do zasto je to uopce dict...
                        sharedDictMsg.put(msg,msgBody);

                        // salji ACK peer-u
                        int peerClientPort = receivePacket.getPort();
                        try{
                            // sto je od poruke servFrom i clientFrom sad postaje servToClientTo
                            String message = generateMsg("ACK", port, 0,servFrom,clientFrom,nowScalarTimeStamp);
                            System.out.println("UDP Server sent ACK to " + servFrom);
                            sendBuffer = message.getBytes();
                            InetAddress address = InetAddress.getLocalHost();
                            DatagramPacket packet = new DatagramPacket(sendBuffer, sendBuffer.length, address, servFrom);
                            socket.send(packet);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                    }
                    Thread.sleep(1000);
                }
            } catch (IOException | InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    private String getBody_FromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return arr[0];
    }

    private int getServerFrom_FromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[1]);
    }

    private int getClientFrom_FromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[2]);
    }

    private int getServerTo_FromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[3]);
    }

    private int getClientTo_FromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[4]);
    }

    private long getScalarTimeStampFromMsg(String msg) { // to-do
        String[] arr = msg.split("-");
        return Long.parseLong(arr[5]);
    }

    private String generateMsg(String msgBody, int serverFrom, int clientFrom, int serverTo, int clientTo, long scalarTimeStamp) { // to-do
        return msgBody + "-" + serverFrom + "-" + clientFrom + "-" + serverTo + "-" + clientTo + "-" + scalarTimeStamp;
    }


    public class Client implements Runnable {
        byte[] receiveBuffer = new byte[256];
        byte[] sendBuffer = new byte[256];

        @Override
        public void run() {
            SimpleSimulatedDatagramSocket socket = null;
            try{
                // Open socket
                socket = new SimpleSimulatedDatagramSocket(0.2, 1000);
                int clientPort = socket.getLocalPort();

                // Generate message
                String msgBody = Node.this.getMeasure();
                long scalarTimeStamp = clock.currentTimeMillis();

                for (int peer : Node.this.peers) {
                    String message = generateMsg(msgBody, port, clientPort, peer, 0,  scalarTimeStamp);
                    sendBuffer = message.getBytes();
                    String ackMsg = generateMsg("ACK",port,clientPort,peer,0,scalarTimeStamp);
                    // Save it to ACK shared dictionary on this node
                    sharedDictAck.put(ackMsg, false);

                    // Send to peer
                    InetAddress address = InetAddress.getLocalHost();
                    DatagramPacket packet = new DatagramPacket(sendBuffer, sendBuffer.length, address, peer);
                    socket.send(packet);

                    System.out.println("UDP Client at " + socket.getLocalPort() + " sent message " + message + " to peer " + peer);
                }

                // Wait for acks
                Thread.sleep(4000);

                while (true){
                    // dok ima elemenata u shared_dict_ACK sa poruka_id i value false
                    Hashtable<String,Boolean> tmp = new Hashtable<>();
                    SimpleSimulatedDatagramSocket finalSocket = socket;
                    sharedDictAck.forEach((k, v) -> {
                        if(getServerFrom_FromMsg(k) == port && getClientFrom_FromMsg(k) == clientPort && !v)
                            tmp.put(generateMsg(msgBody, port, clientPort, getServerTo_FromMsg(k), getClientTo_FromMsg(k),scalarTimeStamp),v);
                    });

                    if (tmp.isEmpty()) break;

                    // ponovo salji
                    for(String msg : tmp.keySet()) {
                        InetAddress address = InetAddress.getLocalHost();
                        sendBuffer = msg.getBytes();
                        DatagramPacket packet = new DatagramPacket(sendBuffer, sendBuffer.length, address, getServerTo_FromMsg(msg));
                        socket.send(packet);
                    }

                    // Wait for acks
                    Thread.sleep(4000);
                }
                System.out.println("UDP Client at " + socket.getLocalPort() + " received all ACKS.");
            } catch (InterruptedException | IOException e) {
                e.printStackTrace();
            } finally {
                if (socket != null) socket.close();
            }
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
