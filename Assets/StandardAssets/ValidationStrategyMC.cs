
public class ValidationStrategyMC: ValidationStrategy 
{
#if UNITY_WEBPLAYER
#else
	public const int ITERATIONS = 1000000;
	
	//protected
	public
		override int RequiredForAgreement( int choices, int trials, double confidence )
	{
		return MonteCarlo.RequiredForAgreement( choices, trials, confidence, ITERATIONS );
	}
	
	//protected
	public
		override double ConfidenceOfOutcome( int choices, int trials, int biggestAnswer )
	{
		return MonteCarlo.ConfidenceOfOutcome( choices, trials, biggestAnswer, ITERATIONS );
	}
#endif	
}
