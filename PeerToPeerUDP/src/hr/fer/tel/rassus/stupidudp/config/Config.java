package hr.fer.tel.rassus.stupidudp.config;

import hr.fer.tel.rassus.stupidudp.network.SimpleSimulatedDatagramSocket;

import java.io.IOException;
import java.net.ServerSocket;

/**
 * Created by aelek on 10/12/2016.
 */
public class Config {
    private static final int MIN_PORT = 49152;

    private static final int MAX_PORT = 65535;

    private int[] peers;

    private int numberOfPeers;

    public int[] getPeers() {
        return peers;
    }

    public int getNumberOfPeers() {
        return numberOfPeers;
    }

    public Config(int numberOfPeers) throws Exception {
        this.numberOfPeers = numberOfPeers;
        peers = new int[numberOfPeers];
        int min = MIN_PORT;
        for (int i = 0; i < numberOfPeers; i++){
            peers[i] = nextFreePort(min, MAX_PORT);
            min = peers[i] + 1;
        }
    }

    public void print(){
        int[] peers = getPeers();
        for(int i = 0; i < getNumberOfPeers(); i++){
            System.out.println(peers[i]);
        }
    }

    public static int nextFreePort(int min, int max) throws Exception {
        int curr = min;
        while (true) {
            if (curr > max)
                throw new Exception("Unable to find free port.");
            if (isLocalPortFree(curr))
                return curr;
            curr += 1;
        }
    }

    public static boolean isLocalPortFree(int port) {
        try {
            new SimpleSimulatedDatagramSocket(port, 0.2, 1000).close();
            return true;
        } catch (IOException e) {
            return false;
        }
    }
}
