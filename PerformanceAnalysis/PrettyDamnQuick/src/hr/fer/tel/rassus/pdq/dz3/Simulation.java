/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package hr.fer.tel.rassus.pdq.dz3;

import com.perfdynamics.pdq.Job;
import com.perfdynamics.pdq.Methods;
import com.perfdynamics.pdq.Node;
import com.perfdynamics.pdq.PDQ;
import com.perfdynamics.pdq.QDiscipline;
import java.util.ArrayList;

/**
 *
 * @author aelek
 */
public class Simulation {
    public static void main(String[] args) {
        final double lambda = 0.01;
        final double step = 0.25;
        final int numIterations = 27;
        
        final double a = 0.2;
        final double b = 0.3;
        final double c = 0.5;
        final double d = 0.3;
        final double e = 0.7;
        final double f = 0.5;
        final double g = 0.1;
        final double h = 0.3;
        
        final double S1 = 0.003;
        final double S2 = 0.001;
        final double S3 = 0.01;
        final double S4 = 0.04;
        final double S5 = 0.1;
        final double S6 = 0.13;
        final double S7 = 0.15;
        
        final double v1 = 1;
        final double v2 = (1-g*h)/(1-g*h-(d+e*h)*f);
        final double v3 = a * v2;
        final double v4 = b * v2;        
        final double v5 = c * v2;
        final double v6 = ((d+e*h)/(1-g*h))*v2;
        final double v7 = (e+g*(d+e*h)/(1-g*h))*v2;
        
        final int m = 7;   
        
        // demands
        ArrayList<Double> demands = new ArrayList<>(m);
        demands.add(S1);
        demands.add(S2);
        demands.add(S3);
        demands.add(S4);
        demands.add(S5);
        demands.add(S6);
        demands.add(S7);
        
        // V
        ArrayList<Double> visits = new ArrayList<>(m);
        visits.add(v1);
        visits.add(v2);
        visits.add(v3);
        visits.add(v4);
        visits.add(v5);
        visits.add(v6);
        visits.add(v7);
 
        for(double l = lambda, upperBound = lambda + numIterations * step; l < upperBound; l += step){
            // Postavljanje pocetnih postavki PDQ sustava
            PDQ pdq = new PDQ();
            pdq.Init("Sustav");
            pdq.CreateOpen("Zahtjevi", l);

            for(int i=0; i<m ;i++){
                String sName = "S" + (i + 1);

                pdq.CreateNode(sName, Node.CEN, QDiscipline.FCFS);
                pdq.SetVisits(sName, "Zahtjevi", visits.get(i), demands.get(i));
            }

            // Pokretanje izracuna
            pdq.Solve(Methods.CANON);

            // Prikaz rezultata  
            // pdq.Report();
            double T = pdq.GetResponse(Job.TRANS, "Zahtjevi");
            System.out.printf("T(%.06f) = %.06f\n",l,T);
        }
    }
}
