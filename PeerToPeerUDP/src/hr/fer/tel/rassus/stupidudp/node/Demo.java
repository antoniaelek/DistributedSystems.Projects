package hr.fer.tel.rassus.stupidudp.node;

/**
 * Created by aelek on 11/12/2016.
 */
public class Demo {
    public static void main(String[] args) {
        try {
            Node p = new Node();
            System.out.println(p.getPort());
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
