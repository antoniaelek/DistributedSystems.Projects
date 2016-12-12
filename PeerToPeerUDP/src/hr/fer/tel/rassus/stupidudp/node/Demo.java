package hr.fer.tel.rassus.stupidudp.node;

/**
 * Created by aelek on 11/12/2016.
 */
public class Demo {
    public static void main(String[] args) {
        try {
            Node p = new Node();
            p.startServer();
            Thread.sleep(1000);
            p.startClient();
            Thread.sleep(4000);

            for(int i = 0; i < 4; i++){
                p.print();
               Thread.sleep(5000);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
