using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

//note: there currently is no policy for only keeping the N most recent messages in memory (ie deleting old ones)
public class NetworkMailbox<TEnum> : IObservable<NetworkMailbox<TEnum>.Envelope> where TEnum : struct, IConvertible, IComparable, IFormattable
{//http://msdn.microsoft.com/en-us/library/dd990377.aspx

    private int inMessageCnt = 0;
    private static TEnum tInst = new TEnum();
    private Dictionary<TEnum /*NetworkClient.MessType_ToClient*/, List<Envelope>> dMessages = new Dictionary<TEnum, List<Envelope>>();
    private List<Envelope> listAllMessages = new List<Envelope>();

    private Dictionary<TEnum /*NetworkClient.MessType_ToClient*/, List<IObserver<Envelope>>> dObserversOfType = new Dictionary<TEnum /*NetworkClient.MessType_ToClient*/, List<IObserver<Envelope>>>();
    private List<IObserver<Envelope>> observersOfAll = new List<IObserver<Envelope>>();

    public class Envelope: IComparable<Envelope> 
    {
        public int messID{ get; private set; }
        public TEnum messType{ get; private set; }
        public object data{ get; private set; }

        //sorts by messID (lower id is less than higher id)
        //null is considered to be less than any other object.
        //-1: This object is < than other
        // 0: This object is == to other
        //+1: This object is > than other
        public virtual int CompareTo( Envelope other )
        {
            if (other == null) return 1;
            if (other == this) return 0;
            if (this.messID < other.messID) return -1;
            if (this.messID == other.messID) return 0;
            return 1;
        }

        public Envelope( int messageID, TEnum messageType, object messData )
        {
            //DebugConsole.Log( "Envelope constructor: messageID == " + messageID );
            //DebugConsole.Log( "Envelope constructor: messageType == " + messageType );
            //DebugConsole.Log( "Envelope constructor: typeof( TEnum ) == " + typeof( TEnum ) );
            //DebugConsole.Log( "Envelope constructor: messageType.ToInt32 == " + ( messageType.ToInt32( new System.Globalization.CultureInfo( "en-US" ) ) ) );
            //DebugConsole.Log( "Envelope constructor: messageID == " + messageID );
            this.messID = messageID;
            this.messType = messageType;
            this.data = messData;
        }

        /// <summary>
        /// TODO: check if data is an array, and if so, do pretty things
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "messID:" ).Append( messID ).Append( '\n' );
            sb.Append( "messType:" ).Append( ( (TEnum)messType ).ToString() ).Append( '\n' );
            sb.Append( "data{" ).Append( data.ToString() ).Append( "}" );
            return sb.ToString();
        }
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<Envelope>> _observers;
        private IObserver<Envelope> _observer;

        public Unsubscriber( List<IObserver<Envelope>> observers, IObserver<Envelope> observer )
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains( _observer ))
                _observers.Remove( _observer );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"></param>
    /// <param name="messType"></param>
    /// <returns></returns>
    //NetworkMailbox<NetworkClient.MessType_ToClient>.Envelope
    public virtual IDisposable Subscribe( IObserver<NetworkMailbox<TEnum>.Envelope> observer, TEnum messType /*mess type*/)
    {
        if (!dObserversOfType.ContainsKey( messType ))
        {
            dObserversOfType.Add( messType, new List<IObserver<Envelope>>() );
        }
        if (!dObserversOfType[ messType ].Contains( observer ))
        {
            dObserversOfType[ messType ].Add( observer );
        }
        return new Unsubscriber( dObserversOfType[ messType ], observer );
    }

    public IDisposable Subscribe( IObserver<Envelope> observer )
    {
        if (!observersOfAll.Contains( observer ))
        {
            observersOfAll.Add( observer );
        }
        return new Unsubscriber( observersOfAll, observer );
    }

    public void AddMessage( TEnum /*NetworkClient.MessType_ToClient*/ messType, string[] args )
    {
        //DebugConsole.Log( "NetworkMailbox.AddMessage: " + ( messType.ToInt32( new System.Globalization.CultureInfo( "en-US" ) ) ) + " converts to " + ( (TEnum)messType ).ToString() );
        inMessageCnt++;
        Envelope env = new Envelope( inMessageCnt, messType, args );
        DebugConsole.Log( "NetworkMailbox.AddMessage created envelope: " + env.ToString() );
        listAllMessages.Add( env );
        if (!dMessages.ContainsKey( messType ))
        {
            dMessages.Add( messType, new List<Envelope>() );
        }
        dMessages[ messType ].Add( env );

        foreach (var observer in observersOfAll)
            observer.OnNext( env );
        foreach (var observer in dObserversOfType[ messType ])
            observer.OnNext( env );
        DebugConsole.Log( "NetworkMailbox.AddMessage done notifying observers. ");
    }

    //returns null if we have no messages of specified type
    public List<Envelope> getAllMessagesOfType( TEnum /*NetworkClient.MessType_ToClient*/ messType )
    {
        if (!dMessages.ContainsKey( messType ))
            return null;
        return dMessages[ messType ];
    }

    //returns the message if found, otherwise returns null
    public Envelope getMessageWithID( int messID )
    {
        int index = listAllMessages.BinarySearch( new Envelope( messID, tInst, null ) );
        if (index >= 0)
            return listAllMessages[ index ];
        return null;
    }

    public void deleteMessage( Envelope e )
    {
        int index = listAllMessages.BinarySearch( e );
        if (index >= 0)
            listAllMessages.RemoveAt( index );
        if (dMessages.ContainsKey( e.messType ))
        {
            index = dMessages[ e.messType ].BinarySearch( e );
            dMessages[ e.messType ].RemoveAt( index );
        }
    }
}
