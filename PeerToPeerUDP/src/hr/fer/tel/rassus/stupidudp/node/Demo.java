package hr.fer.tel.rassus.stupidudp.node;

/**
 * Created by aelek on 11/12/2016.
 */
public class Demo {
    public final static int INTERVAL = 5000;
    public static void main(String[] args) {
        try {
            Node p = new Node();
            System.out.println("Node on port " + p.getPort());
            p.startServer();
            p.startClient();

            while(true){
                Thread.sleep(INTERVAL);
                p.printSorted();
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
