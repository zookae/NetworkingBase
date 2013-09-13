using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValidationStrategyMemoized: ValidationStrategy 
{
#if UNITY_WEBPLAYER
#else
	private static ValidationStrategyMC vsMC = new ValidationStrategyMC();
	private DBManipulation dbManip = null;
	
	//internal Dictionary<int/*choices*/, Dictionary<int/*trials*/,double/*confidence*/> > diChoicesTOdTrialConf = new Dictionary<int,Dictionary>();
	//internal Dictionary<int/*trials*/, double/*confidence*/ > diTrialsTOConfidence = new Dictionary<int,double>();
	
	//expects the db to read-from/save-to to be open and initialized propperly
	public ValidationStrategyMemoized( DBManipulation dbIn ) : base()
	{
		this.dbManip = dbIn;
	}
	
	//protected
	public
		override int RequiredForAgreement( int choices, int trials, double confidence )
	{
        int reqForAgreement = dbManip.LookupMonteCarloResults_RequiredForAgreement(choices, trials, confidence);
        if ( -1 == reqForAgreement ) 
        {//db did not have an answer stored
            reqForAgreement = vsMC.RequiredForAgreement( choices, trials, confidence );
			dbManip.SaveMonteCarloResults_RequiredForAgreement( choices, trials, confidence, reqForAgreement );
        }

		return reqForAgreement;
	}
	
	//protected
	//select * from confidenceLookupMC where choices=8 and trials = 6 and reqForAgreement IS NOT NULL and reqForAgreement <= 3 ORDER BY intConfidence DESC
	public
		override double ConfidenceOfOutcome( int choices, int trials, int biggestAnswer )
	{
		
		double doubConfidence= dbManip.LookupMonteCarloResults_ConfidenceOfOutcome( choices, trials, biggestAnswer );
		if( -1 == doubConfidence ){//db had no answer
			doubConfidence = vsMC.ConfidenceOfOutcome( choices, trials, biggestAnswer );
            dbManip.SaveMonteCarloResults_ConfidenceOfOutcome(choices, trials, biggestAnswer, doubConfidence);
		}

		return doubConfidence;
	}
#endif	
}
