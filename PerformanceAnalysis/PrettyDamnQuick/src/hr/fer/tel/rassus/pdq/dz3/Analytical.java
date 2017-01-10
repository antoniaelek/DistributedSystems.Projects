/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package hr.fer.tel.rassus.pdq.dz3;

import java.util.ArrayList;

/**
 *
 * @author aelek
 */
public class Analytical {
    public static void main(String[] args) {
        final double lambda = 0.1;
        final double step = 0.5;
        final int numIterations = 11;
        
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
        final double v2 = (1-h*g)/(1-f+d-h*e);
        final double v3 = ((1-h*g)/(1-f+d-h*e))*a;
        final double v4 = ((1-h*g)/(1-f+d-h*e))*b;
        final double v5 = ((1-h*g)/(1-f+d-h*e))*c;
        final double v6 = (d-h*e)/(1-f+d-h*e);
        final double v7 = (e+g*d)/(1-f+d-h*e);
        
        final int m = 7;
        
        ArrayList<Double> demands = new ArrayList<>(m);
        demands.add(S1);
        demands.add(S2);
        demands.add(S3);
        demands.add(S4);
        demands.add(S5);
        demands.add(S6);
        demands.add(S7);

        ArrayList<Double> vs = new ArrayList<>(m);
        vs.add(v1);
        vs.add(v2);
        vs.add(v3);
        vs.add(v4);
        vs.add(v5);
        vs.add(v6);
        vs.add(v7);
        
        for(double l = lambda, upperBound = lambda + numIterations * step; l < upperBound; l += step){
            double T = 0;
            for(int i=0; i<m; i++){
                double D = vs.get(i)*demands.get(i);
                T += D/(1-l*D);
            }
            System.out.printf("T(%.06f) = %.06f\n",l,T);
        }
    }
}
