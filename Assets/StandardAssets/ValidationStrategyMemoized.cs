using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ValidationStrategyMemoized: ValidationStrategy 
{
#if UNITY_WEBPLAYER
#else
	private static ValidationStrategyMC vsMC = new ValidationStrategyMC();
	private DBAccess db = null;
	
	//internal Dictionary<int/*choices*/, Dictionary<int/*trials*/,double/*confidence*/> > diChoicesTOdTrialConf = new Dictionary<int,Dictionary>();
	//internal Dictionary<int/*trials*/, double/*confidence*/ > diTrialsTOConfidence = new Dictionary<int,double>();
	
	//expects the db to read-from/save-to to be open and initialized propperly
	public ValidationStrategyMemoized( DBAccess dbIn ) : base()
	{
		this.db = dbIn;
	}
	
	//protected
	public
		override int RequiredForAgreement( int choices, int trials, double confidence )
	{
		//if( diChoicesTOdTrialConf.ContainsKey( choices ) )
		//{
		//	if( diChoicesTOdTrialConf[ choices ].ContainsKey( trials ) )
		//	{
		//	}
		//}
		
		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
		sbSQL.Append( "SELECT reqForAgreement FROM confidenceLookupMC WHERE" );
		sbSQL.Append( " choices=" ).Append( choices.ToString() );
		sbSQL.Append( " AND trials=" ).Append( trials.ToString() );
		sbSQL.Append( " AND intConfidence=" ).Append( ((int)confidence*100).ToString() );
		//sbSQL.Append( " AND reqForAgreement IS NOT NULL" );
		sbSQL.Append( " LIMIT 1" );
		
		//sbSQL.Append();
		//sbSQL.Append();
		
		System.Data.IDataReader res = this.db.BasicQuery( sbSQL.ToString() );
		
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		int reqForAgreement=-1;
		if( res.Read() ){ //TODO dispose of the readers http://stackoverflow.com/questions/744051/sqldatareader-dispose
			if( sb.Length > 0 ) sb.Append( "," );
			if( res.IsDBNull( 0 ) ) //means there is no minimum that will satisfy the given (choices,trials,confidence)
				reqForAgreement = -1; 
			else 
				reqForAgreement = res.GetInt32( 0 ); 
			
			sb.Append( reqForAgreement.ToString() );
		}else{ //db had no answer
			reqForAgreement = vsMC.RequiredForAgreement( choices, trials, confidence );
			
			sbSQL = new System.Text.StringBuilder();
			sbSQL.Append( "INSERT OR IGNORE INTO confidenceLookupMC(choices,trials,intConfidence,reqForAgreement) VALUES(" );
			sbSQL.Append( choices.ToString() ).Append( "," );
			sbSQL.Append( trials.ToString() ).Append( "," );
			sbSQL.Append( ((int)confidence*100).ToString() ).Append( "," );
			sbSQL.Append( reqForAgreement == -1 ? "NULL" : reqForAgreement.ToString() );
			sbSQL.Append( ")" );
			
			System.Data.IDataReader res2 = this.db.BasicQuery( sbSQL.ToString() );
			
		}

		return reqForAgreement;
	}
	
	//protected
	//select * from confidenceLookupMC where choices=8 and trials = 6 and reqForAgreement IS NOT NULL and reqForAgreement <= 3 ORDER BY intConfidence DESC
	public
		override double ConfidenceOfOutcome( int choices, int trials, int biggestAnswer )
	{
		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
		sbSQL.Append( "SELECT intConfidence FROM confidenceLookupMC WHERE" );
		sbSQL.Append( " choices=" ).Append( choices.ToString() );
		sbSQL.Append( " AND trials=" ).Append( trials.ToString() );
		sbSQL.Append( " AND reqForAgreement=" ).Append( biggestAnswer );
		sbSQL.Append( " ORDER BY intConfidence DESC LIMIT 1" );
		
		System.Data.IDataReader res = this.db.BasicQuery( sbSQL.ToString() );
		
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		double doubConfidence=-1;
		if( res.Read() ){ //TODO dispose of the readers http://stackoverflow.com/questions/744051/sqldatareader-dispose
			if( sb.Length > 0 ) sb.Append( "," );
			if( res.IsDBNull( 0 ) ) //means there is no minimum that will satisfy the given (choices,trials,confidence)
				doubConfidence = -1; 
			else 
				doubConfidence = res.GetInt32( 0 ) / 100.0; 
			
			sb.Append( doubConfidence.ToString() );
		}else{ //db had no answer
			doubConfidence = vsMC.ConfidenceOfOutcome( choices, trials, biggestAnswer );
			
			sbSQL = new System.Text.StringBuilder();
			sbSQL.Append( "INSERT OR IGNORE INTO confidenceLookupMC(choices,trials,intConfidence,reqForAgreement) VALUES(" );
			sbSQL.Append( choices.ToString() ).Append( "," );
			sbSQL.Append( trials.ToString() ).Append( "," );
			sbSQL.Append( ((int)doubConfidence*100).ToString() ).Append( "," );
			sbSQL.Append( biggestAnswer.ToString() );
			sbSQL.Append( ")" );
			
			System.Data.IDataReader res2 = this.db.BasicQuery( sbSQL.ToString() );
			
		}

		return doubConfidence;
	}
#endif	
}
