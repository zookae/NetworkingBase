using System.Collections;
using System;

//Automan inspired -- assumes multinomial
//see edu.umass.cs.automan.core.strategy.DefaultStrategy
//http://people.cs.umass.edu/~emery/pubs/AutoMan-UMass-CS-TR-2012-013.pdf


public abstract class ValidationStrategy 
{
#if UNITY_WEBPLAYER
#else

	virtual public double CurrentConfidence( int choices, int trials, int biggestAnswer )
	{
		if( choices <= 1 )
			return 1;
		if( trials <= 0 )
			return 0;
		if( trials == biggestAnswer ) //everyone chose same option
		{//e1(n,n) = 1 - 1/choices^(trials-1)
			return 1.0 - 1.0 / Math.Pow( choices, trials-1 );
		}
			
		return ConfidenceOfOutcome( choices, trials, biggestAnswer );
	}
	
	virtual public bool IsConfident( int choices, int trials, int biggestAnswer, double confidence )
	{
		if( choices <= 1 )
			return true;
		if( trials <= 0 )
			return false;
		if( trials == biggestAnswer && //everyone chose same option
			confidence <= ( 1.0 - 1.0 / Math.Pow( choices, trials-1 ) ) 
		   )
			return true;
		
		int min_agree = this.RequiredForAgreement( choices, trials, confidence );
		if( biggestAnswer >= min_agree )
		{//"Reached or exceeded alpha = " + (1 - _confidence).toString
			//UnityEngine.Debug.Log();	
			return true;
		}
		else
		{//"Need " + min_agree + " for alpha = " + (1 - _confidence) + "; have " + biggest_answer
			return false;
		}
	}
	
	//protected 
	public
		abstract int RequiredForAgreement( int choices, int trials, double confidence );
	//protected 
	public
		abstract double ConfidenceOfOutcome( int choices, int trials, int biggestAnswer );
#endif
}
