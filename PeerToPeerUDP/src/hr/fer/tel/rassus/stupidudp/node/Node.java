package hr.fer.tel.rassus.stupidudp.node;

import hr.fer.tel.rassus.stupidudp.config.Config;
import hr.fer.tel.rassus.stupidudp.network.EmulatedSystemClock;
import hr.fer.tel.rassus.stupidudp.network.SimpleSimulatedDatagramSocket;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;


/**
 * Created by aelek on 07/12/2016.
 */
public class Node {
    private final int LOSS_RATE = 0;

    private final boolean PRINT_VERBOSE_VECTOR = false;

    private final boolean PRINT_SERVER = true;

    private final boolean PRINT_CLIENT = false;

    private final int DURATION = 20;

    private LinkedList<String> measurements;

    private HashSet<Integer> peers;

    private LinkedList<String> configuration;

    private ConcurrentHashMap<String,String> sharedDictMsg;

    private ConcurrentHashMap<String,Boolean> sharedDictAck;

    private ConcurrentHashMap<String,Integer> vectorTimeStamp;

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

        vectorTimeStamp = new ConcurrentHashMap<>(configuration.size());
        for (String node :
                configuration) {
            vectorTimeStamp.put(node, 0);
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

    public boolean isLocalPortFree(int port) {
        try {
            new SimpleSimulatedDatagramSocket(port, LOSS_RATE, 1000).close();
            return true;
        } catch (IOException e) {
            return false;
        }
    }

    public void startClient(){
        // new thread that sends measurements
        for (int j = 0; j < DURATION; j++) {
            Thread c = new Thread(new Client());
            c.start();
//            try {
//                Thread.sleep(1000);
//            } catch (InterruptedException e) {
//                e.printStackTrace();
//            }
        }
    }

    public void startServer() throws Exception {
        // new thread that accepts peers' measurements
        Thread s = new Thread(new Server());
        s.start();
    }

    public void printSorted(){

        Map<String,Long> sortedByVector;
        Map<String,Long> sortedByScalar;

        Map<String,Long> scalar = new Hashtable<>();
        Map<String,Long> vector = new Hashtable<>();

        int sum = 0;

        for (String msg : sharedDictMsg.keySet()) {
            scalar.put(msg,getScalarTimeStampFromMsg(msg));
            sum += Integer.parseInt(getBody_FromMsg(msg));
            //System.out.println(getVectorTimeStampFromMsgAsVector(msg).get(Integer.toString(port)));
            int vectKey = getVectorTimeStampFromMsgAsVector(msg).get(Integer.toString(port));
            vector.put(msg, (long) vectKey);
        }

        double mean = sharedDictMsg.size() > 0 ? (double)sum / sharedDictMsg.size() : 0;

        sortedByScalar = sortByValue(scalar);
        sortedByVector = sortByValue(vector);

        System.out.println("------------------------------------------------------------");
        System.out.println("B Y   S C A L A R");
        for (String msg : sortedByScalar.keySet()) {
            System.out.print("SCAL " + getScalarTimeStampFromMsg(msg));
            System.out.print(" VECT " + getVectorTimeStampFromMsg(msg));
            System.out.print(" MSG " + getBody_FromMsg(msg) + "\n");
        }
        System.out.println("------------------------------------------------------------");

        System.out.println("B Y   V E C T O R");
        for (String msg : sortedByVector.keySet()) {
            System.out.print("SCAL " + getScalarTimeStampFromMsg(msg));
            System.out.print(" VECT " + getVectorTimeStampFromMsg(msg));
            System.out.print(" MSG " + getBody_FromMsg(msg) + "\n");
        }
        System.out.println("------------------------------------------------------------");


        System.out.println("Mean = " + mean);

        System.out.println("\n");
        sharedDictMsg.clear();
    }

    public Map<String, Long> sortByValue( Map<String, Long>  map )
    {
        List<Map.Entry<String, Long> > list = new LinkedList<>(map.entrySet());
        Collections.sort( list, Comparator.comparing(o -> (o.getValue())));

        Map<String, Long>  result = new LinkedHashMap<>();
        for (Map.Entry<String, Long>  entry : list)
        {
            result.put( entry.getKey(), entry.getValue() );
        }
        return result;
    }

    public class Server implements Runnable{

        @Override
        public void run() {
            byte[] receiveBuffer = new byte[256];
            byte[] sendBuffer = new byte[256];
            //String msg;

            try(SimpleSimulatedDatagramSocket socket = new SimpleSimulatedDatagramSocket(port, 0.2, 1000)) {
                //System.out.println("UDP server at port " + port);
                if (PRINT_SERVER) System.out.println("UDP Server waiting for peers' messages at port " + port);

                while (true)
                {
                    socket.setSoTimeout(0);
                    //int clientPort = socket.getLocalPort();
                    DatagramPacket receivePacket = new DatagramPacket(receiveBuffer, receiveBuffer.length);

                    // primi poruku od peer-a
                    socket.receive(receivePacket);
                    final String msg = new String(receivePacket.getData(), receivePacket.getOffset(), receivePacket.getLength());


                    String msgBody = getBody_FromMsg(msg);
                    int servFrom = getServerFrom_FromMsg(msg);
                    int clientFrom = getClientFrom_FromMsg(msg);
                    long scalarTimeStamp = getScalarTimeStampFromMsg(msg);
                    long nowScalarTimeStamp = clock.currentTimeMillis();

                    updateVectorAfterReceive(getVectorTimeStampFromMsgAsVector(msg));

                    if (msgBody.equals("ACK")){
                        if (PRINT_SERVER) System.out.println("UDP Server received ACK from " + servFrom);

                        // sto je u ack-u servFrom je u obicnoj poruci servTo, to je port ovog servera
                        // pronadji poruku u shared_dict_ACK i postavi value na true

                        sharedDictAck.forEach((k, v) -> {
                            if(getServerFrom_FromMsg(k) == getServerTo_FromMsg(msg) && getClientFrom_FromMsg(k) == getClientTo_FromMsg(msg) && !v) {
                                sharedDictAck.put(k,true);
                                //if (PRINT_SERVER) System.out.println("UDP Server updated ACK for message from " + getServerFrom_FromMsg(k));
                            }
                        });
                    }
                    else {
                        // body-ftom-ts
                        if (PRINT_SERVER) System.out.println("UDP Server received message " + print_Msg(msg) + " from " + servFrom);

                        // spremi je u shared_dict_MSG
                        sharedDictMsg.put(msg,msgBody);

                        // salji ACK peer-u
                        int peerClientPort = receivePacket.getPort();
                        try{
                            // sto je od poruke servFrom i clientFrom sad postaje servToClientTo

                            // update vector timestamp
                            updateVectorBeforeSend();

                            String message = generateMsg("ACK", port, 0,servFrom,clientFrom,nowScalarTimeStamp, vectorTimeStamp);
                            if (PRINT_SERVER) System.out.println("UDP Server sent ACK to " + servFrom);
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

    private ConcurrentHashMap<String, Integer> getVectorTimeStampFromMsgAsVector(String msg) {
        ConcurrentHashMap<String, Integer> ret = new ConcurrentHashMap<>();
        String strRepr = getVectorTimeStampFromMsg(msg);
        //System.out.println(strRepr);
        // oblika [123:=0,124=0]
        String[] elements = strRepr.substring(1,strRepr.length()-1).split(",");

        for (String element: elements) {
            String[] kvp = element.split("=");
            ret.put(kvp[0],Integer.parseInt(kvp[1]));
        }
        return ret;
    }

    private void updateVectorBeforeSend(){

        int curr = vectorTimeStamp.get(Integer.toString(port));
        if (PRINT_VERBOSE_VECTOR) System.out.println("Updating vector " + vectorTimeStamp.toString() + " before send.");
        vectorTimeStamp.put(Integer.toString(port),curr+1);
        if (PRINT_VERBOSE_VECTOR) System.out.println("Updated vector " + vectorTimeStamp.toString());
    }

    private void updateVectorAfterReceive(ConcurrentHashMap<String,Integer> receivedVector){
        if (PRINT_VERBOSE_VECTOR) System.out.println("Updating vector " + vectorTimeStamp.toString() +" after receive");
        for (String key :
                vectorTimeStamp.keySet()) {
            int value = vectorTimeStamp.get(key);
            int receivedValue = receivedVector.get(key);
            if (key.equals(Integer.toString(port))){
                vectorTimeStamp.put(key,value+1);
                continue;
            }
            if (value < receivedValue) vectorTimeStamp.put(key,receivedValue);
        }
        int curr = vectorTimeStamp.get(Integer.toString(port));
        vectorTimeStamp.put(Integer.toString(port),curr+1);
        if (PRINT_VERBOSE_VECTOR) System.out.println("Updated vector " + vectorTimeStamp.toString());
    }

    private String print_Msg(String msg) {
        String[] arr = msg.split("-");
        return arr[0];
    }

    private String getBody_FromMsg(String msg) {
        String[] arr = msg.split("-");
        return arr[0];
    }

    private int getServerFrom_FromMsg(String msg) {
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[1]);
    }

    private int getClientFrom_FromMsg(String msg) {
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[2]);
    }

    private int getServerTo_FromMsg(String msg) {
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[3]);
    }

    private int getClientTo_FromMsg(String msg) {
        String[] arr = msg.split("-");
        return Integer.parseInt(arr[4]);
    }

    private long getScalarTimeStampFromMsg(String msg) {
        String[] arr = msg.split("-");
        return Long.parseLong(arr[5]);
    }

    private String getVectorTimeStampFromMsg(String msg) {
        String[] arr = msg.split("-");
        return arr[6];
    }


    private String generateMsg(String msgBody, int serverFrom, int clientFrom, int serverTo, int clientTo, long scalarTimeStamp, ConcurrentHashMap<String, Integer> vectorTimeStamp) {
        String vector = "[";
        if (vectorTimeStamp.size() > 0){
            for (String k :
                    vectorTimeStamp.keySet()) {
                vector += k;
                vector += "=";
                vector += vectorTimeStamp.get(k);
                vector += ",";
            }
        }
        vector = vector.substring(0,vector.lastIndexOf(","));
        vector += "]";
        return msgBody + "-" + serverFrom + "-" + clientFrom + "-" + serverTo + "-" + clientTo + "-" + scalarTimeStamp + "-" + vector;
    }


    public class Client implements Runnable {
        byte[] receiveBuffer = new byte[256];
        byte[] sendBuffer = new byte[256];

        @Override
        public void run() {
            SimpleSimulatedDatagramSocket socket = null;
            try{
                // Open socket
                socket = new SimpleSimulatedDatagramSocket(LOSS_RATE, 1000);
                int clientPort = socket.getLocalPort();

                // Generate message
                String msgBody = Node.this.getMeasure();
                long scalarTimeStamp = clock.currentTimeMillis();

                // put it in shared dict because we need to print it with all others messages after 5 secs
                sharedDictMsg.put(generateMsg(msgBody, port, clientPort, 0, 0,  scalarTimeStamp, vectorTimeStamp),msgBody);

                for (int peer : Node.this.peers) {
                    // update vector timestamp
                    updateVectorBeforeSend();

                    // Send to peer
                    String message = generateMsg(msgBody, port, clientPort, peer, 0,  scalarTimeStamp, vectorTimeStamp);
                    sendBuffer = message.getBytes();
                    String ackMsg = generateMsg("ACK",port,clientPort,peer,0,scalarTimeStamp, vectorTimeStamp);
                    // Save it to ACK shared dictionary on this node
                    sharedDictAck.put(ackMsg, false);


                    InetAddress address = InetAddress.getLocalHost();
                    DatagramPacket packet = new DatagramPacket(sendBuffer, sendBuffer.length, address, peer);
                    socket.send(packet);

                    if (PRINT_CLIENT) System.out.println("UDP Client at " + socket.getLocalPort() + " sent message " + print_Msg(message) + " to peer " + peer);
                }

                // Wait for acks
                Thread.sleep(1000);

                while (true){
                    // dok ima elemenata u shared_dict_ACK sa poruka_id i value false
                    Hashtable<String,Boolean> tmp = new Hashtable<>();
                    SimpleSimulatedDatagramSocket finalSocket = socket;
                    sharedDictAck.forEach((k, v) -> {
                        if(getServerFrom_FromMsg(k) == port && getClientFrom_FromMsg(k) == clientPort && !v)
                            tmp.put(generateMsg(msgBody, port, clientPort, getServerTo_FromMsg(k), getClientTo_FromMsg(k),scalarTimeStamp, vectorTimeStamp),v);
                    });

                    if (tmp.isEmpty()) break;

                     // ponovo salji
                    for(String msg : tmp.keySet()) {
                        InetAddress address = InetAddress.getLocalHost();
                        sendBuffer = msg.getBytes();
                        DatagramPacket packet = new DatagramPacket(sendBuffer, sendBuffer.length, address, getServerTo_FromMsg(msg));
                        if (PRINT_CLIENT)  System.out.println("UDP Client at " + socket.getLocalPort() +  " resending message " + print_Msg(msg) + " to peer " + getServerTo_FromMsg(msg));
                        socket.send(packet);
                    }

                    // Wait for acks
                    Thread.sleep(4000);
                }
                if (PRINT_CLIENT)  System.out.println("UDP Client at " + socket.getLocalPort() + " received all ACKS.");
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
