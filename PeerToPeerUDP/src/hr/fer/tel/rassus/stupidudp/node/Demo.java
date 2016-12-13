package hr.fer.tel.rassus.stupidudp.node;

/**
 * Created by aelek on 11/12/2016.
 */
public class Demo {
    public static void main(String[] args) {
        try {
            Node p = new Node();
            System.out.println("Node on port " + p.getPort());
            p.startServer();
            Thread.sleep(1000);
            p.startClient();

            while(true){
            //for(int i = 0; i < 2; i++){
                Thread.sleep(2000);
                p.printSorted();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
