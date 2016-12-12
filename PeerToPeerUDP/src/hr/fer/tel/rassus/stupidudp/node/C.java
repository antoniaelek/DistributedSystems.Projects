package hr.fer.tel.rassus.stupidudp.node;

/**
 * Created by aelek on 11/12/2016.
 */
public class C {
    public static void main(String[] args) {
        try {
            Node p = new Node();
            p.startServer();
            p.startClient();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
