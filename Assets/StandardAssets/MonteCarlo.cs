//using UnityEngine;
using System.Collections;
using System;

//ported & modified from AutoMan
//edu.umass.cs.automan.core.strategy.MonteCarlo

public static class MonteCarlo {
	
	private class Occurrences 
	{
        public int[] occurrences;
        public Occurrences(int size) {
            occurrences = new int[size];
        }
    }
	
	//returns -1 if not enough trials exist to reach desired confidence
	//otherwise, returns minimum number of trials such that agreement is with specified confidence
	public static int RequiredForAgreement(int choices, int trials, double confidence, int iterations) {
        // array to track max result of each test
        Occurrences o = new Occurrences(trials + 1);

        // Spin up a bunch of runs.
        for (int i = 0; i < iterations; i++) {
            Iteration(choices, trials, o);
        }

        // Calculate and return minimum number of trials
        return Calculate_min_agreement(trials, confidence, iterations, o);
    }
	
	public static double ConfidenceOfOutcome(int choices, int trials, int max_agree, int iterations) {
        // array to track max result of each test
        Occurrences o = new Occurrences(trials + 1);

        // Spin up a bunch of runs.
        for (int i = 0; i < iterations; i++) {
            Iteration(choices, trials, o);
        }

        // Calculate and return odds
        return Calculate_odds(trials, max_agree, iterations, o);
    }
	
	private static void Iteration(int choices, int trials, Occurrences o) {
        System.Random r = new System.Random();
        int[] choice = new int[trials];
        // {[0..choices-1], [0..choices-1], ...} // # = trials

        // make a choice for every trial
        for (int j = 0; j < trials; j++) {
            choice[j] = Math.Abs(r.Next(int.MaxValue)) % choices;
        }

        // Make a histogram, adding up occurrences of each choice.
        int[] counter = new int[choices];
        for (int k = 0; k < choices; k++) {
            counter[k] = 0;
        }
        for (int z = 0; z < trials; z++) {
            counter[choice[z]] += 1;
        }

        // Find the biggest choice
        int max = 0;
        for (int k = 0; k < choices; k++) {
            if (counter[k] > max) {
                max = counter[k];
            }
        }

        // Return the number of votes that the biggest choice got
		if( max > trials )
			UnityEngine.Debug.LogError( "Max votes is > #trials, which is impossible." );		
        o.occurrences[max]++;
    }
	
	private static int Calculate_min_agreement(int trials, double confidence, int iterations, Occurrences o) {
        // Determine the number of trials in order for the odds
        // to drop below alpha (i.e., 1 - confidence).
        // This is done by subtracting the area under the histogram for each trial
        // from 1.0 until the answer is less than alpha.
        int i = 1;
        double odds = 1.0;
        double alpha = 1.0 - confidence;
        while ((i <= trials) && (odds > alpha)) {
            double odds_i = o.occurrences[i] / (double) iterations;
            odds -= odds_i;
            i++;
        }

        // If we found an answer, then return # of trials
        if ((i <= trials) && (odds <= alpha)) {
            UnityEngine.Debug.Log("DEBUG: MONTECARLO: " + i + " identical answers required for " + trials + " HITs.");
            return i;
        // Otherwise
        } else {
            // Error condition: not enough trials to achieve the desired confidence.
            return -1;
        }
    }

    private static double Calculate_odds(int trials, int max_agree, int runs, Occurrences o) {
        int i = trials;
        double odds = 0.0;
        while((i >= max_agree)) {
            double odds_i = o.occurrences[i] / (double) runs;
            odds += odds_i;
            i--;
        }
        return 1 - odds;
    }
}
